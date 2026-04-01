using System.Net.Sockets;

namespace MultiChatServer;

public class ClientSession
{
    public Socket? WorkSocket { get; set; }
    public string UserName { get; set; } = "未命名";

    public string EndPointText => WorkSocket?.RemoteEndPoint?.ToString() ?? string.Empty;
}
