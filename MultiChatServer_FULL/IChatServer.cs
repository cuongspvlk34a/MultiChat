namespace MultiChatServer;

public interface IChatServer
{
    void Start(string ip, int port);
    void Stop();
    void Broadcast(string message);
    event Action<string>? LogGenerated;
    event Action<List<string>>? ClientListChanged;
}
