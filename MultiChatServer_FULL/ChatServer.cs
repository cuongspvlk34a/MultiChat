using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiChatServer;

public class ChatServer : IChatServer
{
    private Socket? _serverSocket;
    private readonly List<ClientSession> _clients = new();
    private volatile bool _isRunning;

    // FIX BUG1: Prevents recursive RemoveClient (A offline→broadcast→B offline→RemoveClient inside RemoveClient)
    private readonly HashSet<ClientSession> _removing = new();

    // CancellationTokenSource để dừng AcceptLoop và tất cả ReceiveLoop một cách có kiểm soát
    private CancellationTokenSource? _cts;
    private System.Threading.Timer? _heartbeatTimer;

    private const int MaxClients = 100;
    private const int MaxSameIpClients = 20;

    public event Action<string>? LogGenerated;
    public event Action<List<string>>? ClientListChanged;

    public void Start(string ip, int port)
    {
        if (_isRunning) return;

        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint endPoint = new(ipAddress, port);

        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(endPoint);
        _serverSocket.Listen(Math.Max(100, MaxClients));

        _cts = new CancellationTokenSource();
        _isRunning = true;
        LogGenerated?.Invoke($"伺服器已啟動：{ip}:{port}");

        _ = Task.Run(() => AcceptLoop(_cts.Token), _cts.Token);
        _heartbeatTimer = new System.Threading.Timer(_ => SendHeartbeat(), null,
            TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    public void Stop()
    {
        // FIX BUG_DISPOSE: Guard against Stop() being called multiple times
        // (e.g. user clicks Stop then closes the form → FormClosing calls Stop again)
        if (!_isRunning && _serverSocket == null) return;

        _heartbeatTimer?.Dispose(); _heartbeatTimer = null;
        _isRunning = false;
        _cts?.Cancel();

        lock (_clients)
        {
            foreach (ClientSession client in _clients)
            {
                try { client.WorkSocket?.Shutdown(SocketShutdown.Both); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
                try { client.WorkSocket?.Close(); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
            }
            _clients.Clear();
        }

        try { _serverSocket?.Close(); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
        _serverSocket = null;

        ClientListChanged?.Invoke(new List<string>());
        LogGenerated?.Invoke("伺服器已停止");
    }

    private void AcceptLoop(CancellationToken ct)
    {
        while (_isRunning && !ct.IsCancellationRequested)
        {
            try
            {
                if (_serverSocket is null) break;
                Socket clientSocket = _serverSocket.Accept();

                int currentCount;
                lock (_clients) { currentCount = _clients.Count; }
                if (currentCount >= MaxClients)
                {
                    try
                    {
                        byte[] refuseMsg = Encoding.UTF8.GetBytes(
                            ProtocolHelper.BuildSystem("[錯誤] 伺服器已達最大連線數，請稍後再試") + "\n");
                        clientSocket.Send(refuseMsg);
                    }
                    catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
                    try { clientSocket.Shutdown(SocketShutdown.Both); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
                    try { clientSocket.Close(); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
                    LogGenerated?.Invoke($"拒絕連線：已達最大連線數 {MaxClients}");
                    continue;
                }

                // [2] Per-IP connection limit
                string clientIp = (clientSocket.RemoteEndPoint
                    as System.Net.IPEndPoint)?.Address.ToString() ?? "";
                int sameIpCount;
                lock (_clients)
                {
                    sameIpCount = _clients.Count(c =>
                        (c.WorkSocket?.RemoteEndPoint as System.Net.IPEndPoint)
                        ?.Address.ToString() == clientIp);
                }
                if (sameIpCount >= MaxSameIpClients)
                {
                    try {
                        byte[] msg = Encoding.UTF8.GetBytes(
                            ProtocolHelper.BuildSystem(
                                "[錯誤] 同一 IP 最多允許 5 個連線") + "\n");
                        clientSocket.Send(msg);
                    } catch { }
                    try { clientSocket.Shutdown(SocketShutdown.Both); } catch { }
                    try { clientSocket.Close(); } catch { }
                    LogGenerated?.Invoke($"[WARN] Từ chối kết nối từ {clientIp}: vượt giới hạn per-IP");
                    continue;
                }

                clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                clientSocket.SendTimeout = 5_000;

                ClientSession session = new()
                {
                    WorkSocket = clientSocket,
                    UserName = "未命名"
                };

                lock (_clients)
                {
                    _clients.Add(session);
                }

                LogGenerated?.Invoke($"有新連線：{session.EndPointText}");
                UpdateClientList();

                _ = Task.Run(() => ReceiveLoop(session, ct), ct);
            }
            catch (ObjectDisposedException)
            {
                // FIX BUG_DISPOSE: Socket was disposed by Stop() — exit loop cleanly
                break;
            }
            catch (SocketException) when (!_isRunning || ct.IsCancellationRequested)
            {
                // FIX BUG_DISPOSE: SocketException caused by intentional Stop() — exit cleanly
                break;
            }
            catch (Exception ex)
            {
                if (_isRunning && !ct.IsCancellationRequested)
                {
                    LogGenerated?.Invoke($"Accept 錯誤：{ex.Message}");
                }
                // If server is stopping, break instead of looping back to a disposed socket
                if (!_isRunning || ct.IsCancellationRequested) break;
            }
        }
    }

    private void ReceiveLoop(ClientSession session, CancellationToken ct)
    {
        byte[] buffer = new byte[8192];
        Decoder decoder = Encoding.UTF8.GetDecoder();

        while (_isRunning && !ct.IsCancellationRequested)
        {
            try
            {
                if (session.WorkSocket is null) break;

                int len = session.WorkSocket.Receive(buffer);
                if (len == 0) break;

                int charCount = decoder.GetCharCount(buffer, 0, len);
                char[] chars  = new char[charCount];
                decoder.GetChars(buffer, 0, len, chars, 0);
                string chunk  = new string(chars);
                ProcessChunk(session, chunk);
            }
            catch (Exception ex)
            {
                LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}");
                break;
            }
        }

        RemoveClient(session);
    }

    private void ProcessChunk(ClientSession session, string chunk)
    {
        if (session.ReceiveBuffer.Length + chunk.Length > 10 * 1024 * 1024)
        {
            LogGenerated?.Invoke($"{session.UserName} gửi dữ liệu bất thường, ngắt kết nối");
            RemoveClient(session);
            return;
        }

        session.ReceiveBuffer.Append(chunk);

        while (true)
        {
            int newlineIndex = -1;
            for (int i = session.ReceiveSearchPos; i < session.ReceiveBuffer.Length; i++)
            {
                if (session.ReceiveBuffer[i] == '\n')
                {
                    newlineIndex = i;
                    break;
                }
            }

            if (newlineIndex < 0)
            {
                session.ReceiveSearchPos = session.ReceiveBuffer.Length;
                break;
            }

            string line = session.ReceiveBuffer.ToString(0, newlineIndex).TrimEnd('\r');
            session.ReceiveBuffer.Remove(0, newlineIndex + 1);
            session.ReceiveSearchPos = 0;

            if (!string.IsNullOrWhiteSpace(line))
            {
                HandleMessage(session, line);
            }
        }
    }

    private void HandleMessage(ClientSession session, string raw)
    {
        string[] parts = ProtocolHelper.Parse(raw);
        if (parts.Length == 0) return;

        string command = parts[0];

        if (command != "JOIN" && string.Equals(session.UserName, "未命名", StringComparison.Ordinal))
        {
            LogGenerated?.Invoke($"[警告] 未認證的連線嘗試發送 {command}，已拒絕");
            return;
        }

        if (command == "JOIN" && parts.Length >= 2)
        {
            string requestedName = parts[1];

            bool nameTaken;
            lock (_clients)
            {
                nameTaken = _clients.Any(c =>
                    c != session &&
                    string.Equals(c.UserName, requestedName, StringComparison.Ordinal));
            }

            if (nameTaken)
            {
                SendToRaw(session, ProtocolHelper.BuildSystem($"[錯誤] 暱稱「{requestedName}」已被使用，請重新連線並選擇其他暱稱"));
                // FIX BUG4: send explicit KICK so client can detect rejection and update UI
                SendToRaw(session, "KICK|NAMEDUP");
                LogGenerated?.Invoke($"拒絕連線：暱稱「{requestedName}」已被使用");
                Task.Delay(200).ContinueWith(_ => RemoveClient(session));
                return;
            }

            if (string.Equals(requestedName, "未命名", StringComparison.Ordinal))
            {
                SendToRaw(session, ProtocolHelper.BuildSystem("[錯誤] 暱稱「未命名」為系統保留字，請選擇其他暱稱"));
                SendToRaw(session, "KICK|RESERVED");
                LogGenerated?.Invoke("拒絕連線：嘗試使用保留暱稱「未命名」");
                Task.Delay(200).ContinueWith(_ => RemoveClient(session));
                return;
            }

            session.UserName = requestedName;
            LogGenerated?.Invoke($"{session.UserName} 已加入聊天室");
            Broadcast(ProtocolHelper.BuildSystem($"{session.UserName} 已加入聊天室"));
            UpdateClientList();
        }
        else if (command == "MSG" && parts.Length >= 3)
        {
            if (IsRateLimited(session)) return;

            string userName  = parts[1];
            string message   = string.Join("|", parts.Skip(2));
            string messageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{userName}：{message}");
            Broadcast(ProtocolHelper.BuildMessage(userName, messageId, message));
        }
        else if (command == "EMOJI" && parts.Length >= 3)
        {
            if (IsRateLimited(session)) return;
            string userName = parts[1];
            string emoji = string.Join("|", parts.Skip(2));
            LogGenerated?.Invoke($"{userName}：{emoji}");
            Broadcast(ProtocolHelper.BuildEmoji(userName, emoji));
        }
        else if (command == "IMG" && parts.Length >= 4)
        {
            string userName  = parts[1];
            string fileName  = parts[2];
            string base64    = string.Join("|", parts.Skip(3));
            string messageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{userName} 傳送圖片：{fileName}");
            if (base64.Length > 7_000_000)
            {
                LogGenerated?.Invoke($"[WARN] {userName} gửi ảnh quá lớn, bị drop");
                return;
            }
            Broadcast(ProtocolHelper.BuildImage(userName, messageId, fileName, base64));
        }
        else if (command == "RECALL" && parts.Length >= 3)
        {
            string userName  = parts[1];
            string messageId = parts[2];
            if (!string.Equals(userName, session.UserName, StringComparison.Ordinal))
            {
                LogGenerated?.Invoke($"[警告] {session.UserName} 試圖冒充 {userName} 收回訊息，已拒絕");
                return;
            }
            LogGenerated?.Invoke($"{userName} 收回訊息：{messageId}");
            Broadcast(ProtocolHelper.BuildRecall(userName, messageId));
        }
        else if (command == "PMRECALL" && parts.Length >= 4)
        {
            string fromUser  = parts[1];
            string toUser    = parts[2];
            string messageId = parts[3];
            if (!string.Equals(fromUser, session.UserName, StringComparison.Ordinal))
            {
                LogGenerated?.Invoke($"[警告] {session.UserName} 試圖冒充 {fromUser} 收回PM，已拒絕");
                return;
            }
            LogGenerated?.Invoke($"{fromUser} 收回私訊 → {toUser}：{messageId}");
            string built = ProtocolHelper.BuildPrivateRecall(fromUser, toUser, messageId);
            SendTo(toUser,   built);
            SendTo(fromUser, built);
        }
        else if (command == "READ" && parts.Length >= 3)
        {
            string readerName = parts[1];
            string senderName = parts[2];
            LogGenerated?.Invoke($"{readerName} 已讀 {senderName} 的訊息");
            SendTo(senderName, ProtocolHelper.BuildRead(readerName, senderName));
        }
        else if (command == "PM" && parts.Length >= 4)
        {
            string fromUser  = parts[1];
            string toUser    = parts[2];
            string message   = string.Join("|", parts.Skip(3));
            string messageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{fromUser} 私訊 {toUser}：{message}");
            string built = ProtocolHelper.BuildPrivateMessage(fromUser, toUser, messageId, message);
            SendTo(toUser,   built);
            SendTo(fromUser, built);
        }
        else if (command == "FILE" && parts.Length >= 4)
        {
            if (IsRateLimited(session)) return;
            string userName = parts[1];
            string fileName = parts[2];
            string base64   = string.Join("|", parts.Skip(3));
            if (base64.Length > 7_000_000)
            {
                LogGenerated?.Invoke($"[WARN] {userName} gửi file quá lớn, bị drop");
                return;
            }
            LogGenerated?.Invoke($"{userName} 傳送檔案：{fileName}");
            Broadcast(ProtocolHelper.BuildFile(userName, fileName, base64));
        }
        else if (command == "PMIMG" && parts.Length >= 5)
        {
            if (IsRateLimited(session)) return;
            string fromUser = parts[1];
            string toUser   = parts[2];
            string fileName = parts[3];
            string base64   = string.Join("|", parts.Skip(4));
            if (base64.Length > 7_000_000)
            {
                LogGenerated?.Invoke($"[WARN] {fromUser} gửi ảnh PM quá lớn, bị drop");
                return;
            }
            string messageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{fromUser} 私訊圖片 → {toUser}：{fileName}");
            // Gửi tới người nhận
            SendTo(toUser, ProtocolHelper.BuildPrivateImage(fromUser, toUser, messageId, fileName, base64));
            // Gửi lại cho người gửi để hiển thị trong chat của họ
            SendTo(fromUser, ProtocolHelper.BuildPrivateImage(fromUser, toUser, messageId, fileName, base64));
        }
        else if (command == "PMFILE" && parts.Length >= 5)
        {
            if (IsRateLimited(session)) return;
            string fromUser  = parts[1];
            string toUser    = parts[2];
            string fileName  = parts[3];
            string base64    = string.Join("|", parts.Skip(4));
            if (base64.Length > 7_000_000)
            {
                LogGenerated?.Invoke($"[WARN] {fromUser} gửi file PM quá lớn, bị drop");
                return;
            }
            string messageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{fromUser} 私訊檔案 → {toUser}：{fileName}");
            string built = ProtocolHelper.BuildPrivateFile(fromUser, toUser, messageId, fileName, base64);
            // Gửi tới người nhận
            SendTo(toUser,   built);
            // Gửi lại cho người gửi để hiển thị trong chat của họ
            SendTo(fromUser, built);
        }
        else if (command == "TYPING" && parts.Length >= 2)
        {
            string userName = parts[1];
            
            byte[] data     = Encoding.UTF8.GetBytes(ProtocolHelper.BuildTyping(userName) + "\n");
            List<ClientSession> snapshot;
            lock (_clients) { snapshot = _clients.ToList(); }
            List<ClientSession> deadClients = new();
            foreach (ClientSession client in snapshot)
            {
                if (client == session) continue;
                if (!TrySendRaw(client, data)) deadClients.Add(client);
            }
            foreach (ClientSession dead in deadClients)
                RemoveClient(dead);
        }
        else if (command == "REPLY" && parts.Length >= 4)
        {
            if (IsRateLimited(session)) return;
            string userName     = parts[1];
            string quotedMsgId  = parts[2];
            string message      = string.Join("|", parts.Skip(3));
            string newMessageId = Guid.NewGuid().ToString("N")[..8];
            LogGenerated?.Invoke($"{userName} 回覆：{message}");
            Broadcast(ProtocolHelper.BuildReply(userName, newMessageId, quotedMsgId, message));
        }
        else if (command == "REACT" && parts.Length >= 4)
        {
            if (IsRateLimited(session)) return;
            string userName  = parts[1];
            string messageId = parts[2];
            string emoji     = parts[3];
            LogGenerated?.Invoke($"{userName} reacted {emoji} to {messageId}");
            Broadcast(ProtocolHelper.BuildReact(userName, messageId, emoji));
        }
        // [USER STATUS] Relay STATUS broadcast; only allow 3 valid values.
        else if (command == "STATUS" && parts.Length >= 3)
        {
            if (IsRateLimited(session)) return;
            string userName = parts[1];
            string status   = parts[2];
            if (status is not ("online" or "busy" or "away"))
                return;
            LogGenerated?.Invoke($"{userName} status → {status}");
            Broadcast(ProtocolHelper.BuildStatus(userName, status));
        }
    }

    public void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");

        List<ClientSession> snapshot;
        lock (_clients) { snapshot = _clients.ToList(); }

        List<ClientSession> deadClients = new();
        foreach (ClientSession client in snapshot)
        {
            if (!TrySendRaw(client, data)) deadClients.Add(client);
        }

        foreach (ClientSession dead in deadClients)
            RemoveClient(dead);
    }

    /// <summary>
    /// FIX BUG3: All sends go through here. Per-client lock prevents data interleaving
    /// when multiple threads send large IMG/FILE payloads to the same client simultaneously.
    /// Returns false on failure — never throws, never calls RemoveClient.
    /// </summary>
    private static bool TrySendRaw(ClientSession client, byte[] data)
    {
        if (client.WorkSocket is null) return false;
        lock (client.SendLock)
        {
            try
            {
                client.WorkSocket.Send(data);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// FIX BUG1: _removing HashSet ensures RemoveClient is idempotent and non-recursive.
    /// If A's removal triggers detection of dead B, B's RemoveClient starts fresh
    /// without nesting inside A's call stack.
    /// </summary>
    private void RemoveClient(ClientSession session)
    {
        lock (_removing)
        {
            if (!_removing.Add(session)) return; // already in progress
        }

        bool removed;
        lock (_clients)
        {
            removed = _clients.Remove(session);
        }

        lock (_removing)
        {
            _removing.Remove(session);
        }

        if (!removed) return;

        try { session.WorkSocket?.Shutdown(SocketShutdown.Both); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }
        try { session.WorkSocket?.Close(); } catch (Exception ex) { LogGenerated?.Invoke($"[ERR] {ex.GetType().Name}: {ex.Message}"); }

        // FIX BUG_DISPOSE2: Capture EndPointText BEFORE closing socket and BEFORE setting WorkSocket=null.
        // After Close(), accessing RemoteEndPoint on a disposed socket throws ObjectDisposedException.
        // WorkSocket=null also prevents EndPointText property from being called on disposed socket
        // anywhere else (e.g. UpdateClientList → Select(c => c.EndPointText)).
        string name = (!string.IsNullOrWhiteSpace(session.UserName) && session.UserName != "未命名")
            ? session.UserName
            : session.EndPointText;
        session.WorkSocket = null; // nullify AFTER capturing name, AFTER Close()

        LogGenerated?.Invoke($"{name} 已離線");

        List<string> displayList;
        List<string> userNames;
        lock (_clients)
        {
            displayList = _clients.Select(c => $"{c.UserName} ({c.EndPointText})").ToList();
            userNames   = _clients.Select(c => c.UserName).ToList();
        }

        byte[] sysData   = Encoding.UTF8.GetBytes(ProtocolHelper.BuildSystem($"{name} 已離開聊天室") + "\n");
        byte[] usersData = Encoding.UTF8.GetBytes(ProtocolHelper.BuildUserList(userNames) + "\n");

        List<ClientSession> snapshot;
        lock (_clients) { snapshot = _clients.ToList(); }

        List<ClientSession> deadClients = new();
        foreach (ClientSession client in snapshot)
        {
            if (!TrySendRaw(client, sysData))   { deadClients.Add(client); continue; }
            if (!TrySendRaw(client, usersData)) { if (!deadClients.Contains(client)) deadClients.Add(client); }
        }

        foreach (ClientSession dead in deadClients)
            RemoveClient(dead);

        ClientListChanged?.Invoke(displayList);
    }

    private void UpdateClientList()
    {
        List<string> displayList;
        List<string> userNames;
        lock (_clients)
        {
            displayList = _clients.Select(c => $"{c.UserName} ({c.EndPointText})").ToList();
            userNames   = _clients.Select(c => c.UserName).ToList();
        }

        ClientListChanged?.Invoke(displayList);
        Broadcast(ProtocolHelper.BuildUserList(userNames));
    }

    private void SendTo(string targetUserName, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");

        ClientSession? target;
        lock (_clients)
        {
            target = _clients.FirstOrDefault(c =>
                string.Equals(c.UserName, targetUserName, StringComparison.Ordinal));
        }

        if (target is null) return;

        if (!TrySendRaw(target, data)) RemoveClient(target);
    }

    private void SendToRaw(ClientSession target, string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");
        TrySendRaw(target, data);
    }

    private void SendHeartbeat()
    {
        List<ClientSession> snapshot;
        lock (_clients) { snapshot = _clients.ToList(); }
        byte[] data = Encoding.UTF8.GetBytes("PING\n");
        List<ClientSession> deadClients = new();
        foreach (ClientSession client in snapshot)
        {
            if (!TrySendRaw(client, data)) deadClients.Add(client);
        }
        foreach (ClientSession dead in deadClients)
            RemoveClient(dead);
    }

    private bool IsRateLimited(ClientSession session)
    {
        const int MsgLimit  = 20;
        const int WindowSec = 10;
        DateTime now = DateTime.UtcNow;
        if ((now - session.MsgWindowStart).TotalSeconds > WindowSec)
        {
            session.MsgWindowStart = now;
            session.MsgCount = 0;
        }
        session.MsgCount++;
        if (session.MsgCount > MsgLimit)
        {
            LogGenerated?.Invoke($"[WARN] {session.UserName} rate-limited");
            return true;
        }
        return false;
    }
}
