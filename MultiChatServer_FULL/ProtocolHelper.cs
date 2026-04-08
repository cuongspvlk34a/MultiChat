using System.Linq;
namespace MultiChatServer;

public static class ProtocolHelper
{
    public static string BuildJoin(string userName) => $"JOIN|{SanitizeField(userName)}";

    /// <summary>
    /// Builds a MSG broadcast line. Called by SERVER only — server assigns messageId.
    /// FIX BUG2: message uses SanitizeMessage (preserves commas); only field separators are stripped.
    /// </summary>
    public static string BuildMessage(string userName, string messageId, string message)
        => $"MSG|{SanitizeField(userName)}|{SanitizeField(messageId)}|{SanitizeMessage(message)}";

    public static string BuildEmoji(string userName, string emoji)
        => $"EMOJI|{SanitizeField(userName)}|{SanitizeMessage(emoji)}";

    public static string BuildImage(string userName, string messageId, string fileName, string base64)
        => $"IMG|{SanitizeField(userName)}|{SanitizeField(messageId)}|{SanitizeField(fileName)}|{base64}";

    public static string BuildSystem(string message)
        => $"SYS|{SanitizeMessage(message)}";

    public static string[] Parse(string raw)
        => raw.Split('|');

    public static string BuildRecall(string userName, string messageId)
        => $"RECALL|{SanitizeField(userName)}|{SanitizeField(messageId)}";

    public static string BuildPrivateRecall(string fromUser, string toUser, string messageId)
        => $"PMRECALL|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(messageId)}";

    public static string BuildRead(string readerName, string senderName)
        => $"READ|{SanitizeField(readerName)}|{SanitizeField(senderName)}";

    /// <summary>
    /// FIX PMRECALL: thêm messageId để client hai bên có thể match khi recall.
    /// Format mới: PM|fromUser|toUser|messageId|message
    /// </summary>
    public static string BuildPrivateMessage(string fromUser, string toUser, string messageId, string message)
        => $"PM|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(messageId)}|{SanitizeMessage(message)}";

    public static string BuildFile(string userName, string fileName, string base64)
        => $"FILE|{SanitizeField(userName)}|{SanitizeField(fileName)}|{base64}";

    public static string BuildPrivateImage(string fromUser, string toUser, string messageId, string fileName, string base64)
        => $"PMIMG|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(messageId)}|{SanitizeField(fileName)}|{base64}";

    /// <summary>
    /// FIX PMRECALL: thêm messageId để client hai bên có thể match khi recall.
    /// Format mới: PMFILE|fromUser|toUser|messageId|fileName|base64
    /// </summary>
    public static string BuildPrivateFile(string fromUser, string toUser, string messageId, string fileName, string base64)
        => $"PMFILE|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(messageId)}|{SanitizeField(fileName)}|{base64}";

    // FIX BUG2: USERS list uses comma as delimiter → must sanitize commas in usernames only
    public static string BuildUserList(IEnumerable<string> users)
        => $"USERS|{string.Join(",", users.Select(SanitizeField))}";

    public static string BuildTyping(string userName)
        => $"TYPING|{SanitizeField(userName)}";

    /// <summary>
    /// Builds a REPLY broadcast line. Called by SERVER only — server assigns newMessageId.
    /// </summary>
    public static string BuildReply(string userName, string newMessageId, string quotedMsgId, string message)
        => $"REPLY|{SanitizeField(userName)}|{SanitizeField(newMessageId)}|{SanitizeField(quotedMsgId)}|{SanitizeMessage(message)}";

    /// <summary>
    /// Builds a REACT broadcast line. Server relays as-is; emoji is a short Unicode symbol.
    /// </summary>
    public static string BuildReact(string userName, string messageId, string emoji)
        => $"REACT|{SanitizeField(userName)}|{SanitizeField(messageId)}|{SanitizeField(emoji)}";

    /// <summary>
    /// [USER STATUS] Builds a STATUS broadcast line.
    /// status values: "online" | "busy" | "away"
    /// </summary>
    public static string BuildStatus(string userName, string status)
        => $"STATUS|{SanitizeField(userName)}|{SanitizeField(status)}";

    /// <summary>
    /// FIX BUG2: Used for structured fields (userName, messageId, fileName).
    /// Strips pipe, comma, and newlines — these are protocol delimiters.
    /// </summary>
    private static string SanitizeField(string value)
        => value.Replace("|", "/").Replace(",", " ").Replace("\r", " ").Replace("\n", " ");

    /// <summary>
    /// FIX BUG2: Used for message content. Strips only pipe and newlines.
    /// Commas are preserved so users can type normal sentences.
    /// </summary>
    private static string SanitizeMessage(string value)
        => value.Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
}
