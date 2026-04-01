namespace MultiChatClient;

public static class ProtocolHelper
{
    public static string BuildJoin(string userName)    => $"JOIN|{userName}";
    public static string BuildTyping(string userName)  => $"TYPING|{userName}";
    public static string BuildSystem(string message)   => $"SYS|{Sanitize(message)}";

    public static string BuildMessage(string userName, string message)
        => $"MSG|{userName}|{Sanitize(message)}";

    public static string BuildImage(string userName, byte[] imageBytes)
        => $"IMG|{userName}|{Convert.ToBase64String(imageBytes)}";

    public static string BuildFile(string userName, string fileName, byte[] fileBytes)
        => $"FILE|{userName}|{Sanitize(fileName)}|{Convert.ToBase64String(fileBytes)}";

    /// <summary>Private message: PM|sender|recipient|message</summary>
    public static string BuildPm(string sender, string recipient, string message)
        => $"PM|{sender}|{recipient}|{Sanitize(message)}";

    /// <summary>Recall a message: RECALL|sender|msgId</summary>
    public static string BuildRecall(string sender, string msgId)
        => $"RECALL|{sender}|{msgId}";

    private static string Sanitize(string s) => s.Replace("\r", " ").Replace("\n", " ");
}
