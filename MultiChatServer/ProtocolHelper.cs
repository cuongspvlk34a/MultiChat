namespace MultiChatServer;

public static class ProtocolHelper
{
    public static string BuildJoin(string userName) => $"JOIN|{userName}";

    public static string BuildMessage(string userName, string message)
        => $"MSG|{userName}|{message.Replace("\r", " ").Replace("\n", " ")}";

    public static string BuildSystem(string message)
        => $"SYS|{message.Replace("\r", " ").Replace("\n", " ")}";

    public static string[] Parse(string raw)
        => raw.Split('|');
}
