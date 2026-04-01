using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiChatClient;

public class ChatClient
{
    private Socket?       _socket;
    private bool          _isConnected;
    private string        _userName = string.Empty;
    private readonly StringBuilder _buf = new();

    public event Action<string>?            MessageReceived;
    public event Action<string>?            StatusChanged;
    public event Action<string, byte[]>?    ImageReceived;    // sender, bytes
    public event Action<string, string>?    FileReceived;     // sender, "name|b64"
    public event Action<string>?            TypingReceived;   // sender
    public event Action<string, string, string>? PmReceived;  // sender, recipient, message
    public event Action<string, string>?    RecallReceived;   // sender, msgId

    // ── connect / disconnect ──────────────────────────────────────────────────

    public void Connect(string ip, int port, string userName)
    {
        if (_isConnected) return;
        _userName = userName;
        _socket   = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _socket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        _isConnected = true;
        StatusChanged?.Invoke("已連線");
        SendRaw(ProtocolHelper.BuildJoin(_userName));
        Task.Run(ReceiveLoop);
    }

    public void Disconnect()
    {
        _isConnected = false;
        try { _socket?.Shutdown(SocketShutdown.Both); } catch { }
        try { _socket?.Close(); }                         catch { }
        _socket = null;
        StatusChanged?.Invoke("已離線");
    }

    // ── send ─────────────────────────────────────────────────────────────────

    public void SendChat(string message)
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildMessage(_userName, message));
    }

    public void SendImage(byte[] bytes)
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildImage(_userName, bytes));
    }

    public void SendFile(string fileName, byte[] bytes)
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildFile(_userName, fileName, bytes));
    }

    public void SendTyping()
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildTyping(_userName));
    }

    public void SendPm(string recipient, string message)
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildPm(_userName, recipient, message));
    }

    public void SendRecall(string msgId)
    {
        if (!_isConnected) return;
        SendRaw(ProtocolHelper.BuildRecall(_userName, msgId));
    }

    // ── internal ──────────────────────────────────────────────────────────────

    private void SendRaw(string text)
    {
        if (_socket is null) return;
        byte[] data = Encoding.UTF8.GetBytes(text + "\x04");
        try { _socket.Send(data); } catch { }
    }

    private void ReceiveLoop()
    {
        byte[] buf = new byte[1024 * 512];
        while (_isConnected)
        {
            try
            {
                if (_socket is null) break;
                int len = _socket.Receive(buf);
                if (len == 0) break;

                _buf.Append(Encoding.UTF8.GetString(buf, 0, len));
                string full = _buf.ToString();
                int idx;
                while ((idx = full.IndexOf('\x04')) >= 0)
                {
                    string msg = full[..idx];
                    full = full[(idx + 1)..];
                    if (!string.IsNullOrWhiteSpace(msg)) HandleMessage(msg);
                }
                _buf.Clear();
                _buf.Append(full);
            }
            catch { break; }
        }
        if (_isConnected) Disconnect();
    }

    private void HandleMessage(string raw)
    {
        int p1 = raw.IndexOf('|');
        string cmd  = p1 >= 0 ? raw[..p1]       : raw;
        string rest = p1 >= 0 ? raw[(p1 + 1)..] : "";

        switch (cmd)
        {
            case "SYS":
                MessageReceived?.Invoke($"[系統] {rest}");
                break;

            case "MSG":
            {
                int p2 = rest.IndexOf('|');
                if (p2 < 0) break;
                MessageReceived?.Invoke($"{rest[..p2]}：{rest[(p2+1)..]}");
                break;
            }

            case "IMG":
            {
                int p2 = rest.IndexOf('|');
                if (p2 < 0) break;
                try { ImageReceived?.Invoke(rest[..p2], Convert.FromBase64String(rest[(p2+1)..])); }
                catch { MessageReceived?.Invoke($"{rest[..p2]}：[圖片失敗]"); }
                break;
            }

            case "FILE":
            {
                string[] parts = rest.Split('|', 3);
                if (parts.Length < 3) break;
                FileReceived?.Invoke(parts[0], $"{parts[1]}|{parts[2]}");
                break;
            }

            case "TYPING":
                TypingReceived?.Invoke(rest);
                break;

            case "PM":
            {
                // PM|sender|recipient|message
                string[] parts = rest.Split('|', 3);
                if (parts.Length < 3) break;
                PmReceived?.Invoke(parts[0], parts[1], parts[2]);
                break;
            }

            case "RECALL":
            {
                int p2 = rest.IndexOf('|');
                if (p2 < 0) break;
                RecallReceived?.Invoke(rest[..p2], rest[(p2+1)..]);
                break;
            }
        }
    }
}
