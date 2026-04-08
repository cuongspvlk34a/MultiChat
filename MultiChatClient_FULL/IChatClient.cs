namespace MultiChatClient;

public interface IChatClient
{
    bool IsConnected { get; }
    string UserName { get; }

    event Action<string>? MessageReceived;
    event Action<string, string, string, byte[]>? ImageReceived;
    event Action<string, string, byte[]>? FileReceived;
    event Action<string, string, string, byte[]>? PrivateImageReceived;
    event Action<string, string, string, byte[]>? PrivateFileReceived;
    event Action<string>? StatusChanged;

    /// <summary>Fired for broadcast chat messages (MSG command). Args: userName, messageId, message.</summary>
    event Action<string, string, string>? ChatMessageReceived;

    /// <summary>Fired when a RECALL command is received. Arg: messageId</summary>
    event Action<string>? MessageRecalled;
    event Action<string, string>? PrivateMessageRecalled;

    /// <summary>Fired when a READ receipt arrives. Args: readerName, senderName</summary>
    event Action<string, string>? ReadReceiptReceived;

    /// <summary>Fired when a private message arrives. Args: fromUser, toUser, message</summary>
    event Action<string, string, string, string>? PrivateMessageReceived;

    /// <summary>Fired when the server sends an updated online user list.</summary>
    event Action<List<string>>? UserListUpdated;

    /// <summary>Fired when another user is typing. Arg: userName</summary>
    event Action<string>? TypingReceived;

    /// <summary>Fired when a REPLY arrives. Args: userName, newMessageId, quotedMsgId, message</summary>
    event Action<string, string, string, string>? ReplyMessageReceived;

    /// <summary>Fired when server sends KICK. Arg: reason string.</summary>
    event Action<string>? KickedByServer;

    /// <summary>Fired when a REACT command arrives. Args: userName, messageId, emoji</summary>
    event Action<string, string, string>? ReactionReceived;

    /// <summary>[USER STATUS] Fired when another client's status changes. Args: userName, status</summary>
    event Action<string, string>? StatusUpdateReceived;

    Task ConnectAsync(string ip, int port, string userName);
    void Disconnect();
    void SendChat(string message);
    void SendEmoji(string emoji);
    void SendImage(string fileName, byte[] imageBytes);
    void SendFile(string fileName, byte[] fileBytes);
    void SendRecall(string messageId);
    void SendPrivateRecall(string toUser, string messageId);
    void SendReadReceipt(string senderName);
    void SendPrivateMessage(string toUser, string message);
    void SendTyping();
    void SendReply(string quotedMsgId, string message);
    void SendRawReaction(string userName, string messageId, string emoji);

    /// <summary>[USER STATUS] Sends the local user's status. Values: "online" | "busy" | "away"</summary>
    void SendStatus(string status);
    void SendPrivateImage(string toUser, string fileName, byte[] imageBytes);
    void SendPrivateFile(string toUser, string fileName, byte[] fileBytes);
}
