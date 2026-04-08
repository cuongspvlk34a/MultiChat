namespace MultiChatClient;

/// <summary>
/// Chứa business logic tách ra từ FrmClient:
///   - _messageMap: lưu messageId → nội dung (phục vụ RECALL / REPLY preview)
///   - Typing debounce timer (gửi lên server)
///   - Per-user typing-hide timers (ẩn indicator sau 3 giây)
///   - FormatChatMessage: tạo display text chuẩn hoá
///   - ParseItemText: tách timestamp + nội dung khỏi item.Text
/// FrmClient chỉ giữ lại event-handler UI và khởi tạo service này.
/// </summary>
public sealed class ChatClientService : IDisposable
{
    // ── 1. Message map (messageId → nội dung ngắn để build QuotedPreview) ──
    private readonly Dictionary<string, string> _messageMap = new();
    private readonly Queue<string> _messageOrder = new();
    private const int MaxMessageMapSize = 500;

    // ── 2. Typing debounce: tránh spam TYPING lên server ──────────────────
    private readonly System.Windows.Forms.Timer _typingDebounceTimer;

    // ── 3. Per-user typing-hide timers ────────────────────────────────────
    //    Mỗi user nhận TYPING signal có 1 timer riêng.
    //    Khi A và B cùng gõ, timer của A không bị B reset.
    private readonly Dictionary<string, System.Windows.Forms.Timer> _typingTimers = new();

    // Callback để FrmClient cập nhật lblTyping mà không cần ref đến Label
    private readonly Action _onTypingStateChanged;

    // Bao lâu thì xem là ngừng gõ (ms)
    private const int TypingHideMs = 3000;
    // Cooldown gửi TYPING signal lên server (ms)
    private const int TypingDebounceMs = 800;

    public ChatClientService(Action onTypingStateChanged)
    {
        _onTypingStateChanged = onTypingStateChanged;

        _typingDebounceTimer = new System.Windows.Forms.Timer { Interval = TypingDebounceMs };
        _typingDebounceTimer.Tick += (_, _) => _typingDebounceTimer.Stop();
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Message map API
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Lưu messageId → nội dung. Gọi khi thêm item mới vào danh sách chat.</summary>
    public void TrackMessage(string messageId, string content)
    {
        if (string.IsNullOrEmpty(messageId)) return;
        if (_messageMap.ContainsKey(messageId))
        {
            _messageMap[messageId] = content;
            return;
        }
        if (_messageMap.Count >= MaxMessageMapSize)
        {
            string oldest = _messageOrder.Dequeue();
            _messageMap.Remove(oldest);
        }
        _messageMap[messageId] = content;
        _messageOrder.Enqueue(messageId);
    }

    /// <summary>Xoá messageId khỏi map (khi bị RECALL). Trả về true nếu tồn tại.</summary>
    public bool RemoveMessage(string messageId)
        => _messageMap.Remove(messageId);

    /// <summary>Lấy nội dung đã lưu. Trả về null nếu không tìm thấy.</summary>
    public string? GetMessage(string messageId)
        => _messageMap.TryGetValue(messageId, out string? v) ? v : null;

    // ──────────────────────────────────────────────────────────────────────
    //  Typing debounce
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi từ txtMessage_TextChanged. Trả về true nếu nên gửi TYPING lên server ngay lập tức,
    /// false nếu đang trong cooldown (bỏ qua, tránh spam).
    /// </summary>
    public bool ShouldSendTyping()
    {
        if (_typingDebounceTimer.Enabled)
            return false;          // đang trong cooldown → bỏ qua

        _typingDebounceTimer.Start(); // bắt đầu cooldown 2s
        return true;               // gửi TYPING ngay
    }

    // ──────────────────────────────────────────────────────────────────────
    //  Per-user typing timers
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gọi khi nhận TYPING từ server. Đảm bảo mỗi user có timer riêng.
    /// </summary>
    public void HandleTypingReceived(string userName)
    {
        if (!_typingTimers.TryGetValue(userName, out System.Windows.Forms.Timer? timer))
        {
            timer = new System.Windows.Forms.Timer { Interval = TypingHideMs };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                _typingTimers.Remove(userName);
                _onTypingStateChanged();
            };
            _typingTimers[userName] = timer;
        }

        timer.Stop();
        timer.Start();
        _onTypingStateChanged();
    }

    /// <summary>Danh sách user đang gõ hiện tại.</summary>
    public IReadOnlyCollection<string> TypingUsers => _typingTimers.Keys;

    // ──────────────────────────────────────────────────────────────────────
    //  3. FormatChatMessage
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tạo chuỗi hiển thị chuẩn hoá cho một tin nhắn chat thông thường.
    /// Format: "HH:mm:ss userName：message"
    /// </summary>
    public static string FormatChatMessage(string userName, string messageId, string message)
        => $"{DateTime.Now:HH:mm:ss} {userName}：{message}";

    // ──────────────────────────────────────────────────────────────────────
    //  ParseItemText (static helper, dùng chung cho FrmClient và DrawItem)
    // ──────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tách item.Text (format "HH:mm:ss content") thành timestamp và displayText.
    /// content cho MSG:  "Name：message"
    /// content cho SYS:  "[系統] ..."
    /// content cho IMG:  "Name：圖片 - file"
    /// content cho FILE: "Name：📁 檔案 - file"
    /// content cho PM:   "🔒 [私訊←From] From：msg"
    /// DisplayText = content sau khi bỏ prefix "Name：".
    /// </summary>
    public static (string timeStamp, string displayText) ParseItemText(
        string fullText, string senderName, bool isCenter, bool isPrivate)
    {
        string ts   = string.Empty;
        string rest = fullText;

        if (fullText.Length >= 8 && fullText[2] == ':' && fullText[5] == ':')
        {
            ts   = fullText[..8];
            rest = fullText.Length > 9 ? fullText[9..] : string.Empty;
        }

        if (isCenter)
            return (ts, rest);

        if (!string.IsNullOrEmpty(senderName))
        {
            string prefix = senderName + "：";
            if (rest.StartsWith(prefix))
                rest = rest[prefix.Length..];
        }

        if (isPrivate && !string.IsNullOrEmpty(senderName))
        {
            string prefix2 = senderName + "：";
            int idx = rest.IndexOf(prefix2, StringComparison.Ordinal);
            if (idx >= 0)
                rest = rest[(idx + prefix2.Length)..];
        }

        return (ts, rest);
    }

    // ──────────────────────────────────────────────────────────────────────
    //  IDisposable
    // ──────────────────────────────────────────────────────────────────────

    public void Dispose()
    {
        _typingDebounceTimer.Dispose();
        foreach (System.Windows.Forms.Timer t in _typingTimers.Values)
            t.Dispose();
        _typingTimers.Clear();
    }
}
