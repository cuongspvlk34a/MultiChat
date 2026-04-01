using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiChatServer;

public class ChatServer
{
    private Socket? _serverSocket;
    private readonly List<ClientSession> _clients = new();
    private bool _isRunning;

    public event Action<string>? LogGenerated;
    public event Action<List<string>>? ClientListChanged;

    public void Start(string ip, int port)
    {
        if (_isRunning) return;

        IPAddress ipAddress = IPAddress.Parse(ip);
        IPEndPoint endPoint = new(ipAddress, port);

        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _serverSocket.Bind(endPoint);
        _serverSocket.Listen(20);

        _isRunning = true;
        LogGenerated?.Invoke($"伺服器已啟動：{ip}:{port}");

        Task.Run(AcceptLoop);
    }

    public void Stop()
    {
        _isRunning = false;

        lock (_clients)
        {
            foreach (ClientSession client in _clients)
            {
                try { client.WorkSocket?.Shutdown(SocketShutdown.Both); } catch { }
                try { client.WorkSocket?.Close(); } catch { }
            }
            _clients.Clear();
        }

        try { _serverSocket?.Close(); } catch { }
        _serverSocket = null;

        ClientListChanged?.Invoke(new List<string>());
        LogGenerated?.Invoke("伺服器已停止");
    }

    private void AcceptLoop()
    {
        while (_isRunning)
        {
            try
            {
                if (_serverSocket is null) break;
                Socket clientSocket = _serverSocket.Accept();

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

                Task.Run(() => ReceiveLoop(session));
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    LogGenerated?.Invoke($"Accept 錯誤：{ex.Message}");
                }
            }
        }
    }

    private void ReceiveLoop(ClientSession session)
    {
        byte[] buffer = new byte[2048];

        while (_isRunning)
        {
            try
            {
                if (session.WorkSocket is null) break;

                int len = session.WorkSocket.Receive(buffer);
                if (len == 0) break;

                string raw = Encoding.UTF8.GetString(buffer, 0, len);
                HandleMessage(session, raw);
            }
            catch
            {
                break;
            }
        }

        RemoveClient(session);
    }

    private void HandleMessage(ClientSession session, string raw)
    {
        string[] parts = ProtocolHelper.Parse(raw);
        if (parts.Length == 0) return;

        string command = parts[0];

        if (command == "JOIN" && parts.Length >= 2)
        {
            session.UserName = parts[1];
            LogGenerated?.Invoke($"{session.UserName} 已加入聊天室");
            Broadcast(ProtocolHelper.BuildSystem($"{session.UserName} 已加入聊天室"));
            UpdateClientList();
        }
        else if (command == "MSG" && parts.Length >= 3)
        {
            string userName = parts[1];
            string message = string.Join("|", parts.Skip(2));
            LogGenerated?.Invoke($"{userName}：{message}");
            Broadcast(ProtocolHelper.BuildMessage(userName, message));
        }
    }

    public void Broadcast(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        List<ClientSession> deadClients = new();

        lock (_clients)
        {
            foreach (ClientSession client in _clients)
            {
                try
                {
                    client.WorkSocket?.Send(data);
                }
                catch
                {
                    deadClients.Add(client);
                }
            }
        }

        foreach (ClientSession dead in deadClients)
        {
            RemoveClient(dead);
        }
    }

    private void RemoveClient(ClientSession session)
    {
        bool removed;
        lock (_clients)
        {
            removed = _clients.Remove(session);
        }

        if (!removed) return;

        try { session.WorkSocket?.Shutdown(SocketShutdown.Both); } catch { }
        try { session.WorkSocket?.Close(); } catch { }

        string name = string.IsNullOrWhiteSpace(session.UserName) ? session.EndPointText : session.UserName;
        LogGenerated?.Invoke($"{name} 已離線");
        Broadcast(ProtocolHelper.BuildSystem($"{name} 已離開聊天室"));
        UpdateClientList();
    }

    private void UpdateClientList()
    {
        List<string> list;
        lock (_clients)
        {
            list = _clients.Select(c => $"{c.UserName} ({c.EndPointText})").ToList();
        }

        ClientListChanged?.Invoke(list);
    }
}
