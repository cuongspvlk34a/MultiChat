using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiChatClient;

public class ChatClient : IChatClient
{
    private Socket? _clientSocket;
    private volatile bool _isConnected;
    private string _userName = string.Empty;
    private readonly StringBuilder _receiveBuffer = new();
    private int _receiveSearchPos = 0;

    // FIX BUG3: lock to prevent data interleaving when UI thread and Task.Run
    // both call SendRawLine at the same time (e.g. SendImage + SendTyping concurrently).
    private readonly object _sendLock = new();

    // CancellationTokenSource để dừng ReceiveLoop một cách có kiểm soát
    private CancellationTokenSource? _cts;

    public bool IsConnected => _isConnected;
    public string UserName => _userName;

    public event Action<string>? MessageReceived;
    public event Action<string, string, string, byte[]>? ImageReceived;
    public event Action<string, string, byte[]>? FileReceived;
    public event Action<string, string, string, byte[]>? PrivateImageReceived;
    public event Action<string, string, string, byte[]>? PrivateFileReceived;
    public event Action<string>? StatusChanged;

    /// <summary>Fired for broadcast chat messages (MSG command). Args: userName, messageId, message.</summary>
    public event Action<string, string, string>? ChatMessageReceived;

    /// <summary>Fired when a RECALL command is received. Arg: messageId</summary>
    public event Action<string>? MessageRecalled;
    public event Action<string, string>? PrivateMessageRecalled;

    /// <summary>Fired when a READ receipt arrives. Args: readerName, senderName</summary>
    public event Action<string, string>? ReadReceiptReceived;

    /// <summary>Fired when a private message arrives. Args: fromUser, toUser, messageId, message</summary>
    public event Action<string, string, string, string>? PrivateMessageReceived;

    /// <summary>Fired when the server sends an updated online user list.</summary>
    public event Action<List<string>>? UserListUpdated;

    /// <summary>Fired when another user is typing. Arg: userName</summary>
    public event Action<string>? TypingReceived;

    /// <summary>Fired when a REPLY arrives. Args: userName, newMessageId, quotedMsgId, message</summary>
    public event Action<string, string, string, string>? ReplyMessageReceived;

    /// <summary>
    /// FIX BUG4: Fired when server sends KICK (e.g. duplicate name, reserved name).
    /// Arg: reason string. Client should treat this as a forced disconnect.
    /// </summary>
    public event Action<string>? KickedByServer;

    /// <summary>Fired when a REACT command arrives. Args: userName, messageId, emoji</summary>
    public event Action<string, string, string>? ReactionReceived;

    /// <summary>[USER STATUS] Fired when another client's status changes. Args: userName, status ("online"|"busy"|"away")</summary>
    public event Action<string, string>? StatusUpdateReceived;

    public async Task ConnectAsync(string ip, int port, string userName)
    {
        if (_isConnected) return;

        _userName     = userName;
        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            await _clientSocket.ConnectAsync(new IPEndPoint(IPAddress.Parse(ip), port));
        }
        catch
        {
            _clientSocket.Dispose();
            _clientSocket = null;
            throw;
        }

        _receiveBuffer.Clear();
        _receiveSearchPos = 0;
        _cts = new CancellationTokenSource();
        _isConnected = true;
        StatusChanged?.Invoke("已連線");
        SendRawLine(ProtocolHelper.BuildJoin(_userName));

        _ = Task.Run(() => ReceiveLoop(_cts.Token), _cts.Token);
    }

    public void Disconnect()
    {
        _isConnected = false;
        _cts?.Cancel();

        try { _clientSocket?.Shutdown(SocketShutdown.Both); } catch (Exception ex) { StatusChanged?.Invoke($"[ERR] {ex.Message}"); }
        try { _clientSocket?.Close(); } catch (Exception ex) { StatusChanged?.Invoke($"[ERR] {ex.Message}"); }
        _clientSocket = null;

        StatusChanged?.Invoke("已離線");
    }

    public void SendChat(string message)
    {
        if (!_isConnected || string.IsNullOrWhiteSpace(message)) return;
        SendRawLine(ProtocolHelper.BuildMessage(_userName, message));
    }

    public void SendEmoji(string emoji)
    {
        if (!_isConnected || string.IsNullOrWhiteSpace(emoji)) return;
        SendRawLine(ProtocolHelper.BuildEmoji(_userName, emoji));
    }

    // Large payloads offloaded to thread-pool to avoid blocking UI.
    // FIX BUG3: SendRawLine is now protected by _sendLock so concurrent calls are safe.
    public void SendImage(string fileName, byte[] imageBytes)
    {
        if (!_isConnected || imageBytes.Length == 0) return;
        string line = ProtocolHelper.BuildImage(_userName, fileName, imageBytes);
        Task.Run(() => SendRawLine(line));
    }

    public void SendFile(string fileName, byte[] fileBytes)
    {
        if (!_isConnected || fileBytes.Length == 0) return;
        string line = ProtocolHelper.BuildFile(_userName, fileName, fileBytes);
        Task.Run(() => SendRawLine(line));
    }

    public void SendPrivateImage(string toUser, string fileName, byte[] imageBytes)
    {
        if (!_isConnected || imageBytes.Length == 0) return;
        string line = ProtocolHelper.BuildPrivateImage(_userName, toUser, fileName, imageBytes);
        Task.Run(() => SendRawLine(line));
    }

    public void SendPrivateFile(string toUser, string fileName, byte[] fileBytes)
    {
        if (!_isConnected || fileBytes.Length == 0) return;
        string line = ProtocolHelper.BuildPrivateFile(_userName, toUser, fileName, fileBytes);
        Task.Run(() => SendRawLine(line));
    }

    public void SendRecall(string messageId)
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildRecall(_userName, messageId));
    }

    public void SendPrivateRecall(string toUser, string messageId)
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildPrivateRecall(_userName, toUser, messageId));
    }

    public void SendReadReceipt(string senderName)
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildRead(_userName, senderName));
    }

    public void SendPrivateMessage(string toUser, string message)
    {
        if (!_isConnected || string.IsNullOrWhiteSpace(message)) return;
        SendRawLine(ProtocolHelper.BuildPrivateMessage(_userName, toUser, message));
    }

    public void SendTyping()
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildTyping(_userName));
    }

    public void SendReply(string quotedMsgId, string message)
    {
        if (!_isConnected || string.IsNullOrWhiteSpace(message)) return;
        SendRawLine(ProtocolHelper.BuildReply(_userName, quotedMsgId, message));
    }

    public void SendRawReaction(string userName, string messageId, string emoji)
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildReact(userName, messageId, emoji));
    }

    /// <summary>[USER STATUS] Sends the local user's current status to the server.</summary>
    public void SendStatus(string status)
    {
        if (!_isConnected) return;
        SendRawLine(ProtocolHelper.BuildStatus(_userName, status));
    }

    private void SendRawLine(string text)
    {
        if (_clientSocket is null || !_isConnected) return;

        byte[] data = Encoding.UTF8.GetBytes(text + "\n");

        // FIX BUG3: lock prevents two threads writing to the socket simultaneously,
        // which would interleave bytes and corrupt the framing protocol.
        lock (_sendLock)
        {
            if (_clientSocket is null || !_isConnected) return;
            try
            {
                _clientSocket.Send(data);
            }
            catch (Exception ex) when (ex is SocketException or ObjectDisposedException)
            {
                _isConnected = false;
                StatusChanged?.Invoke("連線中斷");
            }
        }
    }

    private void ReceiveLoop(CancellationToken ct)
    {
        byte[] buffer = new byte[8192];
        Decoder decoder = Encoding.UTF8.GetDecoder();

        while (_isConnected && !ct.IsCancellationRequested)
        {
            try
            {
                if (_clientSocket is null) break;

                int len = _clientSocket.Receive(buffer);
                if (len == 0) break;

                int charCount = decoder.GetCharCount(buffer, 0, len);
                char[] chars  = new char[charCount];
                decoder.GetChars(buffer, 0, len, chars, 0);
                string chunk  = new string(chars);
                ProcessChunk(chunk);
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke($"[ERR] {ex.Message}");
                break;
            }
        }

        if (_isConnected)
        {
            Disconnect();
        }
    }

    private void ProcessChunk(string chunk)
    {
        if (_receiveBuffer.Length + chunk.Length > 10 * 1024 * 1024) // 10 MB guard
        {
            Disconnect();
            return;
        }

        _receiveBuffer.Append(chunk);

        while (true)
        {
            int newlineIndex = -1;
            for (int i = _receiveSearchPos; i < _receiveBuffer.Length; i++)
            {
                if (_receiveBuffer[i] == '\n')
                {
                    newlineIndex = i;
                    break;
                }
            }

            if (newlineIndex < 0)
            {
                _receiveSearchPos = _receiveBuffer.Length;
                break;
            }

            string line = _receiveBuffer.ToString(0, newlineIndex).TrimEnd('\r');
            _receiveBuffer.Remove(0, newlineIndex + 1);
            _receiveSearchPos = 0;

            if (!string.IsNullOrWhiteSpace(line))
            {
                HandleMessage(line);
            }
        }
    }

    private void HandleMessage(string raw)
    {
        string[] parts = ProtocolHelper.Parse(raw);
        if (parts.Length == 0) return;

        string command = parts[0];

        if (command == "SYS" && parts.Length >= 2)
        {
            MessageReceived?.Invoke($"[系統] {string.Join("|", parts.Skip(1))}");
        }
        else if (command == "MSG" && parts.Length >= 4)
        {
            string userName  = parts[1];
            string messageId = parts[2];
            string message   = string.Join("|", parts.Skip(3));
            ChatMessageReceived?.Invoke(userName, messageId, message);
        }
        else if (command == "EMOJI" && parts.Length >= 3)
        {
            string userName = parts[1];
            string emoji = string.Join("|", parts.Skip(2));
            MessageReceived?.Invoke($"{userName}：{emoji}");
        }
        else if (command == "IMG" && parts.Length >= 5)
        {
            string userName  = parts[1];
            string messageId = parts[2];
            string fileName  = parts[3];
            string base64    = string.Join("|", parts.Skip(4));
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64);
                ImageReceived?.Invoke(userName, messageId, fileName, imageBytes);
            }
            catch (FormatException)
            {
                MessageReceived?.Invoke("[系統] 收到損壞的圖片資料，已略過。");
            }
        }
        else if (command == "FILE" && parts.Length >= 4)
        {
            string userName = parts[1];
            string fileName = parts[2];
            string base64   = string.Join("|", parts.Skip(3));
            try
            {
                byte[] fileBytes = Convert.FromBase64String(base64);
                FileReceived?.Invoke(userName, fileName, fileBytes);
            }
            catch (FormatException)
            {
                MessageReceived?.Invoke("[系統] 收到損壞的檔案資料，已略過。");
            }
        }
        else if (command == "PMIMG" && parts.Length >= 6)
        {
            string fromUser  = parts[1];
            // parts[2] = toUser (không cần dùng ở client)
            string messageId = parts[3];
            string fileName  = parts[4];
            string base64    = string.Join("|", parts.Skip(5));
            try
            {
                byte[] imageBytes = Convert.FromBase64String(base64);
                PrivateImageReceived?.Invoke(fromUser, messageId, fileName, imageBytes);
            }
            catch (FormatException)
            {
                MessageReceived?.Invoke("[系統] 收到損壞的私訊圖片資料，已略過。");
            }
        }
        else if (command == "PMFILE" && parts.Length >= 6)
        {
            string fromUser  = parts[1];
            // parts[2] = toUser (không cần dùng ở client)
            string messageId = parts[3];
            string fileName  = parts[4];
            string base64    = string.Join("|", parts.Skip(5));
            try
            {
                byte[] fileBytes = Convert.FromBase64String(base64);
                PrivateFileReceived?.Invoke(fromUser, messageId, fileName, fileBytes);
            }
            catch (FormatException)
            {
                MessageReceived?.Invoke("[系統] 收到損壞的私訊檔案資料，已略過。");
            }
        }
        else if (command == "RECALL" && parts.Length >= 3)
        {
            string messageId = parts[2];
            MessageRecalled?.Invoke(messageId);
        }
        else if (command == "PMRECALL" && parts.Length >= 4)
        {
            string fromUser  = parts[1];
            // parts[2] = toUser, không cần dùng ở client
            string messageId = parts[3];
            PrivateMessageRecalled?.Invoke(fromUser, messageId);
        }
        else if (command == "READ" && parts.Length >= 3)
        {
            string readerName = parts[1];
            string senderName = parts[2];
            ReadReceiptReceived?.Invoke(readerName, senderName);
        }
        else if (command == "PM" && parts.Length >= 5)
        {
            string fromUser  = parts[1];
            string toUser    = parts[2];
            string messageId = parts[3];
            string message   = string.Join("|", parts.Skip(4));
            PrivateMessageReceived?.Invoke(fromUser, toUser, messageId, message);
        }
        else if (command == "USERS" && parts.Length >= 2)
        {
            List<string> users = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            UserListUpdated?.Invoke(users);
        }
        else if (command == "TYPING" && parts.Length >= 2)
        {
            string userName = parts[1];
            TypingReceived?.Invoke(userName);
        }
        else if (command == "REPLY" && parts.Length >= 5)
        {
            string userName    = parts[1];
            string newMsgId    = parts[2];
            string quotedMsgId = parts[3];
            string message     = string.Join("|", parts.Skip(4));
            ReplyMessageReceived?.Invoke(userName, newMsgId, quotedMsgId, message);
        }
        // FIX BUG4: Handle KICK command — server rejected this client (duplicate name etc.)
        else if (command == "KICK" && parts.Length >= 2)
        {
            string reason = parts[1];
            // Mark disconnected before firing event so UI updates correctly
            _isConnected = false;
            _cts?.Cancel();                          // ← DÒNG ĐƯỢC THÊM VÀO
            KickedByServer?.Invoke(reason);
            StatusChanged?.Invoke("已離線");
            try { _clientSocket?.Shutdown(SocketShutdown.Both); } catch (Exception ex) { StatusChanged?.Invoke($"[ERR] {ex.Message}"); }
            try { _clientSocket?.Close(); } catch (Exception ex) { StatusChanged?.Invoke($"[ERR] {ex.Message}"); }
            _clientSocket = null;
        }
        else if (command == "REACT" && parts.Length >= 4)
        {
            string userName  = parts[1];
            string messageId = parts[2];
            string emoji     = parts[3];
            ReactionReceived?.Invoke(userName, messageId, emoji);
        }
        // [USER STATUS] Handle STATUS broadcast from server.
        else if (command == "STATUS" && parts.Length >= 3)
        {
            string userName = parts[1];
            string status   = parts[2];
            StatusUpdateReceived?.Invoke(userName, status);
        }
    }
}
