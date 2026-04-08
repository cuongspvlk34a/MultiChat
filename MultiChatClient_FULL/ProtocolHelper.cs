using System.Linq;

namespace MultiChatClient;

public static class ProtocolHelper
{
    public static string BuildJoin(string userName) => $"JOIN|{SanitizeField(userName)}";

    /// <summary>
    /// Builds a MSG line to send to the server.
    /// The server will assign a stable messageId and broadcast MSG|name|id|message to all clients.
    /// Do NOT add a messageId parameter here — that is the server's responsibility.
    /// FIX BUG2: message uses SanitizeMessage (preserves commas).
    /// </summary>
    public static string BuildMessage(string userName, string message)
        => $"MSG|{SanitizeField(userName)}|{SanitizeMessage(message)}";

    public static string BuildEmoji(string userName, string emoji)
        => $"EMOJI|{SanitizeField(userName)}|{SanitizeMessage(emoji)}";

    public static string BuildImage(string userName, string fileName, byte[] imageBytes)
        => $"IMG|{SanitizeField(userName)}|{SanitizeField(fileName)}|{Convert.ToBase64String(imageBytes)}";

    public static string BuildFile(string userName, string fileName, byte[] fileBytes)
        => $"FILE|{SanitizeField(userName)}|{SanitizeField(fileName)}|{Convert.ToBase64String(fileBytes)}";

    public static string BuildPrivateImage(string fromUser, string toUser, string fileName, byte[] imageBytes)
        => $"PMIMG|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(fileName)}|{Convert.ToBase64String(imageBytes)}";

    /// <summary>
    /// FIX PMRECALL: client chỉ gửi lên server, KHÔNG có messageId (server tạo).
    /// Format gửi: PMFILE|fromUser|toUser|fileName|base64
    /// </summary>
    public static string BuildPrivateFile(string fromUser, string toUser, string fileName, byte[] fileBytes)
        => $"PMFILE|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(fileName)}|{Convert.ToBase64String(fileBytes)}";

    public static string BuildRecall(string userName, string messageId)
        => $"RECALL|{SanitizeField(userName)}|{SanitizeField(messageId)}";

    public static string BuildPrivateRecall(string fromUser, string toUser, string messageId)
        => $"PMRECALL|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeField(messageId)}";

    public static string BuildRead(string readerName, string senderName)
        => $"READ|{SanitizeField(readerName)}|{SanitizeField(senderName)}";

    /// <summary>
    /// FIX PMRECALL: client chỉ gửi lên server, KHÔNG có messageId (server tạo).
    /// Format gửi: PM|fromUser|toUser|message
    /// </summary>
    public static string BuildPrivateMessage(string fromUser, string toUser, string message)
        => $"PM|{SanitizeField(fromUser)}|{SanitizeField(toUser)}|{SanitizeMessage(message)}";

    // FIX BUG2: USERS uses comma as delimiter → sanitize commas in names only
    public static string BuildUserList(IEnumerable<string> users)
        => $"USERS|{string.Join(",", users.Select(SanitizeField))}";

    public static string BuildTyping(string userName)
        => $"TYPING|{SanitizeField(userName)}";

    /// <summary>
    /// Builds a REPLY line to send to the server.
    /// The server will assign a stable newMessageId and broadcast REPLY|name|newId|quotedId|message.
    /// Do NOT add a newMessageId parameter here — that is the server's responsibility.
    /// FIX BUG2: message preserves commas.
    /// </summary>
    public static string BuildReply(string userName, string quotedMsgId, string message)
        => $"REPLY|{SanitizeField(userName)}|{SanitizeField(quotedMsgId)}|{SanitizeMessage(message)}";

    /// <summary>
    /// Builds a REACT line to send to the server.
    /// </summary>
    public static string BuildReact(string userName, string messageId, string emoji)
        => $"REACT|{SanitizeField(userName)}|{SanitizeField(messageId)}|{SanitizeField(emoji)}";

    /// <summary>
    /// [USER STATUS] Builds a STATUS line to send to the server.
    /// status values: "online" | "busy" | "away"
    /// </summary>
    public static string BuildStatus(string userName, string status)
        => $"STATUS|{SanitizeField(userName)}|{SanitizeField(status)}";

    public static string[] Parse(string raw)
        => raw.Split('|');

    /// <summary>FIX BUG2: For structured fields (userName, messageId). Strips pipe, comma, newlines.</summary>
    private static string SanitizeField(string value)
        => value.Replace("|", "/").Replace(",", " ").Replace("\r", " ").Replace("\n", " ");

    /// <summary>FIX BUG2: For message content. Strips only pipe and newlines. Commas preserved.</summary>
    private static string SanitizeMessage(string value)
        => value.Replace("|", "/").Replace("\r", " ").Replace("\n", " ");
}
