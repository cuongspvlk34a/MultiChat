using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Media;
using System.Speech.Recognition;
using System.Text;

namespace MultiChatClient;

public partial class FrmClient : Form
{
    private readonly IChatClient _client = new ChatClient();

    // ── ChatClientService: chứa _messageMap, typing timers, FormatChatMessage, ParseItemText ──
    private readonly ChatClientService _service;

    private readonly List<ChatListItem> _chatItems = new();
    private const int MaxImageBytes = 2 * 1024 * 1024;

    // ── Feature state ──────────────────────────────────────────────
    private bool _soundEnabled = true;
    private readonly List<int> _searchResultIndices = new();
    private int _searchCurrentIdx = -1;

    // ── Voice recognition ──────────────────────────────────────────
    private SpeechRecognitionEngine? _speechEngine;
    private bool _isListening = false;

    // ── Emoji panel ────────────────────────────────────────────────
    private bool _emojiPanelBuilt = false;

    // ── Avatar color map ──────────────────────────────────────────
    private readonly Dictionary<string, Color> _avatarColors = new();
    private static readonly Color[] AvatarPalette =
    [
        Color.FromArgb(229, 57,  53),
        Color.FromArgb(30,  136, 229),
        Color.FromArgb(67,  160, 71),
        Color.FromArgb(142, 36,  170),
        Color.FromArgb(251, 140, 0),
        Color.FromArgb(0,   137, 123),
        Color.FromArgb(216, 27,  96),
        Color.FromArgb(57,  73,  171),
        Color.FromArgb(121, 85,  72),
        Color.FromArgb(84,  110, 122),
    ];

    // ── Private message target ────────────────────────────────────
    private string? _pmTarget = null;

    // ── Online user list ──────────────────────────────────────────
    private readonly List<string> _onlineUsers = new();

    // ── [USER STATUS] Status map ──────────────────────────────────
    private readonly Dictionary<string, string> _userStatuses = new();
    // key=userName, value="online"|"busy"|"away"

    // ── Unread message count (Feature 1) ──────────────────────────
    private int _unreadCount = 0;
    private bool _windowFocused = true;

    // ── Reply/quote state (Feature 3) ────────────────────────────
    private string? _replyToMsgId = null;
    private string? _replyToPreview = null;
    private bool _userScrolledUp = false;

    // ── Reaction picker ───────────────────────────────────────────
    private static readonly string[] QuickReacts = ["❤️", "😂", "👍", "😮", "😢", "🙏"];

    // ──────────────────────────────────────────────
    //  THEME SYSTEM
    // ──────────────────────────────────────────────

    private sealed record ChatTheme(
        string Name,
        Color FormBack,
        Color LabelFore,
        Color InputBack,
        Color InputFore,
        Color ButtonBack,
        Color ButtonFore,
        Color ButtonBorder,
        Color ListBack,
        Color ListFore,
        Color BubbleRight,
        Color BubbleLeft,
        Color BubbleCenter,
        Color BubbleBorder,
        Color BubbleRightText,
        Color BubbleLeftText,
        Color BubbleCenterText
    );

    private static readonly ChatTheme[] Themes =
    [
        new ChatTheme(
            Name:             "🌙 Dark",
            FormBack:         Color.FromArgb(24, 24, 37),
            LabelFore:        Color.FromArgb(205, 205, 215),
            InputBack:        Color.FromArgb(42, 42, 58),
            InputFore:        Color.FromArgb(205, 205, 215),
            ButtonBack:       Color.FromArgb(55, 55, 78),
            ButtonFore:       Color.FromArgb(205, 205, 215),
            ButtonBorder:     Color.FromArgb(80, 80, 105),
            ListBack:         Color.FromArgb(30, 30, 45),
            ListFore:         Color.FromArgb(205, 205, 215),
            BubbleRight:      Color.FromArgb(40, 95, 55),
            BubbleLeft:       Color.FromArgb(48, 48, 68),
            BubbleCenter:     Color.FromArgb(40, 40, 58),
            BubbleBorder:     Color.FromArgb(65, 65, 88),
            BubbleRightText:  Color.FromArgb(200, 235, 200),
            BubbleLeftText:   Color.FromArgb(205, 205, 215),
            BubbleCenterText: Color.FromArgb(160, 160, 175)
        ),
        new ChatTheme(
            Name:             "☀️ Light",
            FormBack:         Color.FromArgb(245, 245, 248),
            LabelFore:        Color.FromArgb(50, 50, 60),
            InputBack:        Color.White,
            InputFore:        Color.FromArgb(50, 50, 60),
            ButtonBack:       Color.FromArgb(225, 225, 232),
            ButtonFore:       Color.FromArgb(40, 40, 50),
            ButtonBorder:     Color.FromArgb(190, 190, 200),
            ListBack:         Color.White,
            ListFore:         Color.FromArgb(50, 50, 60),
            BubbleRight:      Color.FromArgb(220, 248, 198),
            BubbleLeft:       Color.White,
            BubbleCenter:     Color.FromArgb(235, 235, 238),
            BubbleBorder:     Color.Gainsboro,
            BubbleRightText:  Color.FromArgb(40, 40, 50),
            BubbleLeftText:   Color.FromArgb(40, 40, 50),
            BubbleCenterText: Color.FromArgb(110, 110, 120)
        ),
        new ChatTheme(
            Name:             "💙 Zalo",
            FormBack:         Color.FromArgb(236, 242, 252),
            LabelFore:        Color.FromArgb(15, 45, 100),
            InputBack:        Color.White,
            InputFore:        Color.FromArgb(15, 45, 100),
            ButtonBack:       Color.FromArgb(0, 104, 255),
            ButtonFore:       Color.White,
            ButtonBorder:     Color.FromArgb(0, 80, 200),
            ListBack:         Color.FromArgb(236, 242, 252),
            ListFore:         Color.FromArgb(15, 45, 100),
            BubbleRight:      Color.FromArgb(210, 228, 255),
            BubbleLeft:       Color.White,
            BubbleCenter:     Color.FromArgb(220, 232, 248),
            BubbleBorder:     Color.FromArgb(180, 210, 250),
            BubbleRightText:  Color.FromArgb(15, 45, 100),
            BubbleLeftText:   Color.FromArgb(15, 45, 100),
            BubbleCenterText: Color.FromArgb(80, 110, 160)
        ),
        new ChatTheme(
            Name:             "💚 WhatsApp",
            FormBack:         Color.FromArgb(236, 229, 221),
            LabelFore:        Color.FromArgb(30, 30, 30),
            InputBack:        Color.White,
            InputFore:        Color.FromArgb(30, 30, 30),
            ButtonBack:       Color.FromArgb(7, 94, 84),
            ButtonFore:       Color.White,
            ButtonBorder:     Color.FromArgb(5, 70, 62),
            ListBack:         Color.FromArgb(230, 221, 212),
            ListFore:         Color.FromArgb(30, 30, 30),
            BubbleRight:      Color.FromArgb(220, 248, 198),
            BubbleLeft:       Color.White,
            BubbleCenter:     Color.FromArgb(215, 210, 205),
            BubbleBorder:     Color.FromArgb(200, 195, 188),
            BubbleRightText:  Color.FromArgb(30, 30, 30),
            BubbleLeftText:   Color.FromArgb(30, 30, 30),
            BubbleCenterText: Color.FromArgb(100, 95, 90)
        ),
        new ChatTheme(
            Name:             "🎮 Discord",
            FormBack:         Color.FromArgb(54, 57, 63),
            LabelFore:        Color.FromArgb(185, 187, 190),
            InputBack:        Color.FromArgb(64, 68, 75),
            InputFore:        Color.FromArgb(220, 221, 222),
            ButtonBack:       Color.FromArgb(88, 101, 242),
            ButtonFore:       Color.White,
            ButtonBorder:     Color.FromArgb(68, 79, 200),
            ListBack:         Color.FromArgb(54, 57, 63),
            ListFore:         Color.FromArgb(220, 221, 222),
            BubbleRight:      Color.FromArgb(88, 101, 242),
            BubbleLeft:       Color.FromArgb(47, 49, 54),
            BubbleCenter:     Color.FromArgb(58, 61, 68),
            BubbleBorder:     Color.FromArgb(72, 75, 84),
            BubbleRightText:  Color.White,
            BubbleLeftText:   Color.FromArgb(220, 221, 222),
            BubbleCenterText: Color.FromArgb(148, 155, 164)
        ),
        new ChatTheme(
            Name:             "🌅 Sunset",
            FormBack:         Color.FromArgb(255, 240, 228),
            LabelFore:        Color.FromArgb(90, 35, 5),
            InputBack:        Color.FromArgb(255, 252, 248),
            InputFore:        Color.FromArgb(90, 35, 5),
            ButtonBack:       Color.FromArgb(255, 107, 53),
            ButtonFore:       Color.White,
            ButtonBorder:     Color.FromArgb(220, 80, 30),
            ListBack:         Color.FromArgb(255, 245, 235),
            ListFore:         Color.FromArgb(90, 35, 5),
            BubbleRight:      Color.FromArgb(255, 210, 130),
            BubbleLeft:       Color.White,
            BubbleCenter:     Color.FromArgb(255, 228, 210),
            BubbleBorder:     Color.FromArgb(255, 180, 130),
            BubbleRightText:  Color.FromArgb(90, 35, 5),
            BubbleLeftText:   Color.FromArgb(90, 35, 5),
            BubbleCenterText: Color.FromArgb(160, 80, 30)
        )
    ];

    private ChatTheme _currentTheme = Themes[1];

    // ──────────────────────────────────────────────
    //  CONSTRUCTOR
    // ──────────────────────────────────────────────

    public FrmClient()
    {
        InitializeComponent();

        // Khởi tạo service — callback cập nhật lblTyping chạy trên UI thread
        _service = new ChatClientService(UpdateTypingLabel);

        _client.MessageReceived += AppendChat;
        _client.ChatMessageReceived += OnChatMessageReceived;
        _client.ImageReceived += AppendImageMessage;
        _client.FileReceived += AppendFileMessage;
        _client.PrivateImageReceived += AppendPrivateImageMessage;
        _client.PrivateFileReceived += AppendPrivateFileMessage;
        _client.StatusChanged += UpdateStatus;
        _client.MessageRecalled += OnMessageRecalled;
        _client.PrivateMessageRecalled += OnPrivateMessageRecalled;
        _client.ReadReceiptReceived += OnReadReceiptReceived;
        _client.PrivateMessageReceived += OnPrivateMessageReceived;
        _client.UserListUpdated += OnUserListUpdated;
        _client.TypingReceived += OnTypingReceived;
        _client.ReplyMessageReceived += OnReplyMessageReceived;
        _client.KickedByServer += OnKickedByServer;
        _client.ReactionReceived += OnReactionReceived;
        _client.StatusUpdateReceived += OnStatusUpdateReceived; // [USER STATUS]

        BuildEmojiPanel();
        BuildContextMenu();
        this.DragEnter += FrmClient_DragEnter;
        this.DragDrop += FrmClient_DragDrop;
        lstChat.MouseWheel += lstChat_MouseWheel;
    }

    // ──────────────────────────────────────────────
    //  FORM EVENTS
    // ──────────────────────────────────────────────

    private void FrmClient_Load(object? sender, EventArgs e)
    {
        txtServerIP.Text = "127.0.0.1";
        txtPort.Text = "5000";
        txtUserName.Text = Environment.UserName;
        lblStatus.Text = "未連線";

        btnDisconnect.Enabled = false;
        btnEmoji.Enabled = false;
        btnSendImage.Enabled = false;
        btnSendFile.Enabled = false;
        btnScreenshot.Enabled = false;
        btnVoice.Enabled = true;

        foreach (ChatTheme t in Themes)
            cmbTheme.Items.Add(t.Name);

        cmbTheme.SelectedIndex = 1; // ☀️ Light

        lstChat.Font = new Font(lstChat.Font.FontFamily, trkFontSize.Value, lstChat.Font.Style);
        this.AllowDrop = true;
        lstChat.AllowDrop = true;

        Button btnJumpBottom = new Button
        {
            Name = "btnJumpBottom",
            Text = "↓",
            Size = new Size(44, 28),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(0, 104, 255),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10F,
                            FontStyle.Bold),
            Cursor = Cursors.Hand,
            Visible = false,
            TabStop = false
        };
        btnJumpBottom.FlatAppearance.BorderSize = 0;
        btnJumpBottom.Click += (_, _) =>
        {
            _userScrolledUp = false;
            if (lstChat.Items.Count > 0)
                lstChat.TopIndex = lstChat.Items.Count - 1;
            btnJumpBottom.Visible = false;
            lstChat.Invalidate();
        };

        // Đặt vị trí — cần gọi sau khi form đã layout
        lstChat.Parent!.Controls.Add(btnJumpBottom);
        PositionJumpButton();

        // ── [USER STATUS] Setup lstUsers owner-draw ──────────────
        lstUsers.DrawMode = DrawMode.OwnerDrawFixed;
        lstUsers.DrawItem += LstUsers_DrawItem;

        // ── [USER STATUS] Add status ComboBox next to btnDisconnect ──
        ComboBox cmbMyStatus = new ComboBox
        {
            Name = "cmbMyStatus",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 110,
            Font = new Font("Segoe UI", 9F)
        };
        cmbMyStatus.Items.AddRange(new object[]
            { "🟢 Online", "🔴 Busy", "🌙 Away" });
        cmbMyStatus.SelectedIndex = 0;
        cmbMyStatus.SelectedIndexChanged += (_, _) =>
        {
            if (!_client.IsConnected) return;
            string[] map = { "online", "busy", "away" };
            int idx = cmbMyStatus.SelectedIndex;
            if (idx >= 0 && idx < map.Length)
                _client.SendStatus(map[idx]);
        };
        btnDisconnect.Parent!.Controls.Add(cmbMyStatus);
        cmbMyStatus.Location = new Point(
            btnDisconnect.Right + 6,
            btnDisconnect.Top + (btnDisconnect.Height - cmbMyStatus.Height) / 2);
    }

    private void FrmClient_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _client.Disconnect();
        foreach (ChatListItem item in _chatItems)
            item.Thumbnail?.Dispose();
        StopListening();
        _speechEngine?.Dispose();
        _service.Dispose(); // disposes _typingDebounceTimer + all per-user timers
    }

    private void PositionJumpButton()
    {
        Control? btn = lstChat.Parent?
            .Controls["btnJumpBottom"];
        if (btn is null) return;

        Rectangle r = lstChat.Bounds;
        btn.Location = new Point(
            r.Right - btn.Width - 12,
            r.Bottom - btn.Height - 12);
        btn.BringToFront();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        PositionJumpButton();
    }

    private void FrmClient_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data!.GetDataPresent(DataFormats.FileDrop))
            e.Effect = DragDropEffects.Copy;
        else
            e.Effect = DragDropEffects.None;
    }

    private void FrmClient_DragDrop(object? sender, DragEventArgs e)
    {
        if (!_client.IsConnected)
        {
            MessageBox.Show("請先連線後再拖曳檔案", "提示");
            return;
        }
        string[]? files = e.Data!.GetData(DataFormats.FileDrop) as string[];
        if (files is null || files.Length == 0) return;
        foreach (string filePath in files)
        {
            FileInfo fi = new(filePath);
            if (!fi.Exists) continue;
            string ext = fi.Extension.ToLowerInvariant();
            bool isImage = ext is ".png" or ".jpg" or ".jpeg"
                                or ".bmp" or ".gif";
            if (isImage)
            {
                if (fi.Length > MaxImageBytes)
                {
                    MessageBox.Show(
                        $"圖片「{fi.Name}」超過 2MB 限制，已跳過。",
                        "圖片過大");
                    continue;
                }
                byte[] bytes = File.ReadAllBytes(filePath);
                _client.SendImage(fi.Name, bytes);
            }
            else
            {
                if (fi.Length > MaxFileBytes)
                {
                    MessageBox.Show(
                        $"檔案「{fi.Name}」超過 5MB 限制，已跳過。",
                        "檔案過大");
                    continue;
                }
                byte[] bytes = File.ReadAllBytes(filePath);
                _client.SendFile(fi.Name, bytes);
            }
        }
    }

    // ──────────────────────────────────────────────
    //  FEATURE 1: UNREAD COUNT IN TITLE BAR
    // ──────────────────────────────────────────────

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _windowFocused = true;
        _unreadCount = 0;
        Text = "MultiChat";
    }

    protected override void OnDeactivate(EventArgs e)
    {
        base.OnDeactivate(e);
        _windowFocused = false;
    }

    private void IncrementUnread()
    {
        if (_windowFocused) return;
        _unreadCount++;
        Text = $"({_unreadCount}) MultiChat";
    }

    // ──────────────────────────────────────────────
    //  THEME
    // ──────────────────────────────────────────────

    private void cmbTheme_SelectedIndexChanged(object? sender, EventArgs e)
    {
        int idx = cmbTheme.SelectedIndex;
        if (idx >= 0 && idx < Themes.Length)
            ApplyTheme(Themes[idx]);
    }

    private void ApplyTheme(ChatTheme theme)
    {
        _currentTheme = theme;
        BackColor = theme.FormBack;
        ApplyThemeToControls(Controls, theme);

        trkFontSize.BackColor = theme.FormBack;
        chkSound.ForeColor = theme.LabelFore;
        chkSound.BackColor = Color.Transparent;

        pnlUsers.BackColor = theme.ListBack;
        lblUsersTitle.BackColor = theme.ButtonBack;
        lblUsersTitle.ForeColor = theme.ButtonFore;
        lstUsers.BackColor = theme.ListBack;
        lstUsers.ForeColor = theme.ListFore;
        lblPMTarget.ForeColor = theme.LabelFore;
        lblPMTarget.BackColor = Color.Transparent;

        if (pnlEmojiPopup.Controls.Count > 0 && pnlEmojiPopup.Controls[0] is Label hdr)
        {
            hdr.BackColor = theme.ButtonBack;
            hdr.ForeColor = theme.ButtonFore;
        }
        pnlEmojiPopup.BackColor = theme.FormBack;
        
        if (!_isListening)
        {
            btnVoice.BackColor = theme.ButtonBack;
            btnVoice.ForeColor = theme.ButtonFore;
            btnVoice.FlatAppearance.BorderColor = theme.ButtonBorder;
        }

        lstChat.Invalidate();
    }

    private static void ApplyThemeToControls(Control.ControlCollection controls, ChatTheme theme)
    {
        foreach (Control c in controls)
        {
            switch (c)
            {
                case ListBox lb:
                    lb.BackColor = theme.ListBack;
                    lb.ForeColor = theme.ListFore;
                    break;
                case TextBox tb:
                    tb.BackColor = theme.InputBack;
                    tb.ForeColor = theme.InputFore;
                    break;
                case ComboBox cmb:
                    cmb.BackColor = theme.InputBack;
                    cmb.ForeColor = theme.InputFore;
                    break;
                case Button btn:
                    btn.UseVisualStyleBackColor = false;
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.BackColor = theme.ButtonBack;
                    btn.ForeColor = theme.ButtonFore;
                    btn.FlatAppearance.BorderColor = theme.ButtonBorder;
                    btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(theme.ButtonBack, 0.15f);
                    btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(theme.ButtonBack, 0.1f);
                    break;
                case Label lbl:
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = theme.LabelFore;
                    break;
            }
            if (c.HasChildren)
                ApplyThemeToControls(c.Controls, theme);
        }
    }

    // ──────────────────────────────────────────────
    //  FONT SIZE SLIDER
    // ──────────────────────────────────────────────

    private void trkFontSize_Scroll(object? sender, EventArgs e)
    {
        int pt = trkFontSize.Value;
        lblFontSizeVal.Text = $"{pt}pt";
        lstChat.Font = new Font(lstChat.Font.FontFamily, pt, lstChat.Font.Style);
        RefreshChatList();
    }

    private void RefreshChatList()
    {
        int topIdx = lstChat.TopIndex;
        lstChat.BeginUpdate();
        lstChat.Items.Clear();
        foreach (ChatListItem item in _chatItems)
            lstChat.Items.Add(item);
        lstChat.EndUpdate();

        if (lstChat.Items.Count > 0)
            lstChat.TopIndex = Math.Min(topIdx, lstChat.Items.Count - 1);

        lstChat.Invalidate();
    }

    // ──────────────────────────────────────────────
    //  SOUND TOGGLE
    // ──────────────────────────────────────────────

    private void chkSound_CheckedChanged(object? sender, EventArgs e)
    {
        _soundEnabled = chkSound.Checked;
    }

    // ──────────────────────────────────────────────
    //  MESSAGE SEARCH
    // ──────────────────────────────────────────────

    private void btnSearch_Click(object? sender, EventArgs e) => PerformSearch();

    private void txtSearch_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            PerformSearch();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            ClearSearch();
        }
    }

    private void PerformSearch()
    {
        string query = txtSearch.Text.Trim();
        if (string.IsNullOrEmpty(query)) { ClearSearch(); return; }

        _searchResultIndices.Clear();
        for (int i = 0; i < _chatItems.Count; i++)
        {
            if (_chatItems[i].Text.Contains(query, StringComparison.OrdinalIgnoreCase))
                _searchResultIndices.Add(i);
        }

        if (_searchResultIndices.Count == 0)
        {
            _searchCurrentIdx = -1;
            lstChat.Invalidate();
            MessageBox.Show($"找不到「{query}」", "搜尋結果", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _searchCurrentIdx = (_searchCurrentIdx + 1) % _searchResultIndices.Count;
        lstChat.TopIndex = _searchResultIndices[_searchCurrentIdx];
        lstChat.Invalidate();
        btnSearch.Text = $"搜尋 {_searchCurrentIdx + 1}/{_searchResultIndices.Count}";
    }

    private void ClearSearch()
    {
        _searchResultIndices.Clear();
        _searchCurrentIdx = -1;
        txtSearch.Clear();
        btnSearch.Text = "搜尋 ↑↓";
        lstChat.Invalidate();
    }

    // ──────────────────────────────────────────────
    //  CONNECTION CONTROLS
    // ──────────────────────────────────────────────

    private async void btnConnect_Click(object? sender, EventArgs e)
    {
        try
        {
            string userName = txtUserName.Text.Trim();
            if (string.IsNullOrWhiteSpace(userName)) { MessageBox.Show("請輸入暱稱"); return; }

            btnConnect.Enabled = false;
            await _client.ConnectAsync(txtServerIP.Text.Trim(), int.Parse(txtPort.Text.Trim()), userName);

            btnDisconnect.Enabled = true;
            AppendChat($"[本機] 已嘗試連線到 {txtServerIP.Text.Trim()}:{txtPort.Text.Trim()}");
            _client.SendStatus("online"); // [USER STATUS] announce default status
        }
        catch (Exception ex)
        {
            btnConnect.Enabled = true;
            MessageBox.Show($"連線失敗：{ex.Message}");
        }
    }

    private void btnDisconnect_Click(object? sender, EventArgs e)
    {
        _client.Disconnect();
        btnConnect.Enabled = true;
        btnDisconnect.Enabled = false;
    }

    // ──────────────────────────────────────────────
    //  MESSAGING — UI event handlers
    // ──────────────────────────────────────────────

    private void btnSend_Click(object? sender, EventArgs e) => SendCurrentMessage();

    private void btnSendImage_Click(object? sender, EventArgs e)
    {
        if (!_client.IsConnected) { MessageBox.Show("請先連線後再發送圖片"); return; }

        using OpenFileDialog dialog = new()
        {
            Title = "選擇要傳送的圖片",
            Filter = "圖片檔案|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        FileInfo fileInfo = new(dialog.FileName);
        if (!fileInfo.Exists) return;

        if (fileInfo.Length > MaxImageBytes)
        {
            MessageBox.Show("圖片請控制在 2MB 以內，避免聊天室傳送過慢。", "圖片過大");
            return;
        }

        byte[] imageBytes = File.ReadAllBytes(dialog.FileName);

        // Nếu đang PM → gửi riêng tư, không broadcast
        if (_pmTarget != null)
            _client.SendPrivateImage(_pmTarget, fileInfo.Name, imageBytes);
        else
            _client.SendImage(fileInfo.Name, imageBytes);
    }

    private void txtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            SendCurrentMessage();
        }
    }

    // FEATURE 2: gửi TYPING — delegate cooldown logic sang _service
    private void txtMessage_TextChanged(object? sender, EventArgs e)
    {
        if (!_client.IsConnected) return;

        if (_service.ShouldSendTyping())
            _client.SendTyping();
    }

    private void SendCurrentMessage()
    {
        string msg = txtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(msg)) return;

        string myName = txtUserName.Text.Trim();

        if (_replyToMsgId != null)
        {
            _client.SendReply(_replyToMsgId, msg);
            _replyToMsgId = null;
            _replyToPreview = null;
            pnlReplyPreview.Visible = false;
        }
        else if (_pmTarget != null)
        {
            _client.SendPrivateMessage(_pmTarget, msg);
            // (không AddChatItem ở đây nữa — chờ server echo về)
        }
        else
        {
            _client.SendChat(msg);
        }

        txtMessage.Clear();
        txtMessage.Focus();
    }

    // ──────────────────────────────────────────────
    //  CHAT APPEND (event handlers từ ChatClient)
    // ──────────────────────────────────────────────

    private void AppendChat(string msg)
    {
        if (InvokeRequired) { Invoke(new Action<string>(AppendChat), msg); return; }

        string myName = txtUserName.Text.Trim();

        ChatItemAlignment alignment;
        if (msg.StartsWith("[系統]") || msg.StartsWith("[本機]"))
            alignment = ChatItemAlignment.Center;
        else if (!string.IsNullOrWhiteSpace(myName) && msg.StartsWith(myName + "："))
            alignment = ChatItemAlignment.Right;
        else
            alignment = ChatItemAlignment.Left;

        string senderName = ExtractSenderName(msg);

        var item = new ChatListItem
        {
            MessageId = Guid.NewGuid().ToString("N")[..8],
            Text = $"{DateTime.Now:HH:mm:ss} {msg}",
            SenderName = senderName,
            Alignment = alignment,
            ReadStatus = ReadStatus.None
        };
        AddChatItem(item);

        if (_soundEnabled && alignment == ChatItemAlignment.Left)
            SystemSounds.Asterisk.Play();
    }

    private void OnChatMessageReceived(string userName, string messageId, string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string>(OnChatMessageReceived), userName, messageId, message);
            return;
        }

        string myName = txtUserName.Text.Trim();
        ChatItemAlignment alignment = string.Equals(userName, myName, StringComparison.Ordinal)
            ? ChatItemAlignment.Right
            : ChatItemAlignment.Left;

        // Dùng service để format text chuẩn hoá
        string displayText = ChatClientService.FormatChatMessage(userName, messageId, message);

        var item = new ChatListItem
        {
            MessageId = messageId,
            Text = displayText,
            SenderName = userName,
            Alignment = alignment,
            ReadStatus = alignment == ChatItemAlignment.Right ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);
        _service.TrackMessage(messageId, message); // lưu vào map để RECALL/REPLY preview

        if (alignment == ChatItemAlignment.Left)
        {
            _client.SendReadReceipt(userName);
            IncrementUnread();
        }

        if (_soundEnabled && alignment == ChatItemAlignment.Left)
            SystemSounds.Asterisk.Play();
    }

    private void AppendImageMessage(string userName, string messageId, string fileName, byte[] imageBytes)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string, byte[]>(AppendImageMessage), userName, messageId, fileName, imageBytes);
            return;
        }

        string savePath = SaveIncomingImage(fileName, imageBytes);
        using MemoryStream ms = new(imageBytes);
        using Image original = Image.FromStream(ms);
        Image thumb = CreateThumbnail(original, 220, 160);

        string myName = txtUserName.Text.Trim();
        ChatItemAlignment alignment = string.Equals(userName, myName, StringComparison.Ordinal)
            ? ChatItemAlignment.Right
            : ChatItemAlignment.Left;

        var item = new ChatListItem
        {
            MessageId = messageId,
            Text = $"{DateTime.Now:HH:mm:ss} {userName}：圖片 - {fileName}",
            Thumbnail = thumb,
            FilePath = savePath,
            SenderName = userName,
            Alignment = alignment,
            ReadStatus = alignment == ChatItemAlignment.Right ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);

        if (_soundEnabled && alignment == ChatItemAlignment.Left)
            SystemSounds.Asterisk.Play();
    }

    private void AddChatItem(ChatListItem item)
    {
        _chatItems.Add(item);
        lstChat.Items.Add(item);

        // Chỉ auto-scroll nếu user chưa cuộn lên
        if (!_userScrolledUp)
        {
            lstChat.TopIndex = lstChat.Items.Count - 1;
        }
        else
        {
            // Cập nhật badge số tin mới
            UpdateJumpButton();
        }

        lstChat.Invalidate();
    }

    private void UpdateJumpButton()
    {
        Control? btn = lstChat.Parent?.Controls["btnJumpBottom"];
        if (btn is null) return;

        int missed = lstChat.Items.Count - 1
                     - lstChat.TopIndex;
        if (missed > 0 && _userScrolledUp)
        {
            btn.Text =
                missed > 99 ? "↓99+" : $"↓{missed}";
            btn.Visible = true;
            btn.BringToFront();
        }
        else
        {
            btn.Visible = false;
        }
    }

    private void lstChat_MouseWheel(object? sender, MouseEventArgs e)
    {
        int lastIdx = lstChat.Items.Count - 1;
        if (lastIdx < 0) return;

        // Kiểm tra có đang ở cuối không
        // (TopIndex + visible items >= total)
        int visibleCount = lstChat.ClientSize.Height
                           / Math.Max(1, lstChat.ItemHeight);
        bool atBottom = lstChat.TopIndex
                        >= lastIdx - visibleCount + 1;

        _userScrolledUp = !atBottom;
        UpdateJumpButton();
    }

    private void UpdateStatus(string status)
    {
        if (InvokeRequired) { Invoke(new Action<string>(UpdateStatus), status); return; }

        lblStatus.Text = status;
        lblStatus.ForeColor = _currentTheme.LabelFore;

        bool connected = status == "已連線";
        btnConnect.Enabled = !connected;
        btnDisconnect.Enabled = connected;
        btnEmoji.Enabled = connected;
        btnSendImage.Enabled = connected;
        btnSendFile.Enabled = connected;
        btnScreenshot.Enabled = connected;
    }

    // ──────────────────────────────────────────────
    //  FIX BUG4: KICKED BY SERVER
    // ──────────────────────────────────────────────

    private void OnKickedByServer(string reason)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnKickedByServer), reason); return; }

        string msg = reason switch
        {
            "NAMEDUP" => "暱稱已被其他用戶使用，請更換暱稱後重新連線。",
            "RESERVED" => "暱稱「未命名」為系統保留字，請選擇其他暱稱。",
            _ => $"伺服器拒絕連線（原因：{reason}）。"
        };

        MessageBox.Show(msg, "連線被拒絕", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        btnConnect.Enabled = true;
        btnDisconnect.Enabled = false;
        btnEmoji.Enabled = false;
        btnSendImage.Enabled = false;
        btnSendFile.Enabled = false;
        btnScreenshot.Enabled = false;
    }

    private void OnMessageRecalled(string messageId)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnMessageRecalled), messageId); return; }

        var item = _chatItems.FirstOrDefault(i => i.MessageId == messageId);
        if (item is null || item.IsRecalled) return;

        item.Text = $"{DateTime.Now:HH:mm:ss} 🚫 [此訊息已被收回]";
        item.IsRecalled = true;
        item.Thumbnail?.Dispose();
        item.Thumbnail = null;
        if (!string.IsNullOrEmpty(item.FilePath))
        {
            try { File.Delete(item.FilePath); } catch { }
            item.FilePath = null;
        }

        _service.RemoveMessage(messageId); // xoá khỏi map
        lstChat.Invalidate();
    }

    private void OnPrivateMessageRecalled(string fromUser, string messageId)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string>(OnPrivateMessageRecalled), fromUser, messageId);
            return;
        }

        var item = _chatItems.FirstOrDefault(i => i.MessageId == messageId && i.IsPrivate);
        if (item is null || item.IsRecalled) return;

        item.Text = $"{DateTime.Now:HH:mm:ss} 🚫 [此訊息已被收回]";
        item.IsRecalled = true;
        item.Thumbnail?.Dispose();
        item.Thumbnail = null;
        if (!string.IsNullOrEmpty(item.FilePath))
        {
            try { File.Delete(item.FilePath); } catch { }
            item.FilePath = null;
        }

        _service.RemoveMessage(messageId);
        lstChat.Invalidate();
    }

    // ──────────────────────────────────────────────
    //  READ RECEIPT
    // ──────────────────────────────────────────────

    private void OnReadReceiptReceived(string readerName, string senderName)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string>(OnReadReceiptReceived), readerName, senderName);
            return;
        }

        string myName = txtUserName.Text.Trim();
        if (!string.Equals(senderName, myName, StringComparison.Ordinal)) return;

        bool changed = false;
        for (int i = _chatItems.Count - 1; i >= 0; i--)
        {
            var item = _chatItems[i];
            if (item.Alignment == ChatItemAlignment.Right &&
                item.ReadStatus == ReadStatus.Sent &&
                !item.IsRecalled)
            {
                item.ReadStatus = ReadStatus.Read;
                changed = true;
                break;
            }
        }

        if (changed) lstChat.Invalidate();
    }

    // ──────────────────────────────────────────────
    //  PRIVATE MESSAGE RECEIVED
    // ──────────────────────────────────────────────

    private void OnPrivateMessageReceived(string fromUser, string toUser, string messageId, string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string, string>(OnPrivateMessageReceived), fromUser, toUser, messageId, message);
            return;
        }

        string myName = txtUserName.Text.Trim();
        bool isMine = string.Equals(fromUser, myName, StringComparison.Ordinal);

        var item = new ChatListItem
        {
            MessageId = messageId,
            Text = isMine
                         ? $"{DateTime.Now:HH:mm:ss} 🔒 [私訊→{toUser}] {fromUser}：{message}"
                         : $"{DateTime.Now:HH:mm:ss} 🔒 [私訊←{fromUser}] {fromUser}：{message}",
            SenderName = fromUser,
            Alignment = isMine ? ChatItemAlignment.Right : ChatItemAlignment.Left,
            IsPrivate = true,
            ReadStatus = isMine ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);
        _service.TrackMessage(messageId, message);

        if (!isMine)  // chỉ gửi read receipt và unread khi là người nhận
        {
            _client.SendReadReceipt(fromUser);
            IncrementUnread();
            if (_soundEnabled) SystemSounds.Exclamation.Play();
        }
    }

    // ──────────────────────────────────────────────
    //  ONLINE USER LIST
    // ──────────────────────────────────────────────

    private void OnUserListUpdated(List<string> users)
    {
        if (InvokeRequired) { Invoke(new Action<List<string>>(OnUserListUpdated), users); return; }

        _onlineUsers.Clear();
        _onlineUsers.AddRange(users);

        string myName = txtUserName.Text.Trim();
        lstUsers.BeginUpdate();
        lstUsers.Items.Clear();
        foreach (string u in users)
        {
            if (!string.Equals(u, myName, StringComparison.Ordinal))
                lstUsers.Items.Add(u);
        }
        lstUsers.EndUpdate();
    }

    private void lstUsers_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lstUsers.SelectedItem is string selectedUser)
        {
            _pmTarget = selectedUser;
            lblPMTargetName.Text = $"🔒 {selectedUser}";
            lblPMTargetName.ForeColor = Color.FromArgb(0, 104, 255);
            txtMessage.PlaceholderText = $"私訊給 {selectedUser}...";
        }
    }

    // ──────────────────────────────────────────────
    //  [USER STATUS] STATUS UPDATE RECEIVED
    // ──────────────────────────────────────────────

    private void OnStatusUpdateReceived(string userName, string status)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string>(OnStatusUpdateReceived), userName, status);
            return;
        }
        _userStatuses[userName] = status;
        lstUsers.Invalidate();
    }

    // ──────────────────────────────────────────────
    //  [USER STATUS] OWNER-DRAW lstUsers
    // ──────────────────────────────────────────────

    private void LstUsers_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0) return;
        string userName = lstUsers.Items[e.Index]?.ToString() ?? "";

        e.DrawBackground();

        // Determine dot color from status
        string status = _userStatuses.TryGetValue(userName, out string? s) ? s : "online";
        Color dotColor = status switch
        {
            "busy" => Color.FromArgb(231, 76, 60),
            "away" => Color.FromArgb(149, 165, 166),
            _ => Color.FromArgb(46, 204, 113)
        };

        int dotR = 5;
        int dotX = e.Bounds.X + 6;
        int dotY = e.Bounds.Y + (e.Bounds.Height - dotR * 2) / 2;

        using SolidBrush dotBrush = new(dotColor);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        e.Graphics.FillEllipse(dotBrush, dotX, dotY, dotR * 2, dotR * 2);

        // Draw user name to the right of the dot
        Rectangle textRect = new Rectangle(
            dotX + dotR * 2 + 6,
            e.Bounds.Y,
            e.Bounds.Width - dotX - dotR * 2 - 8,
            e.Bounds.Height);

        TextRenderer.DrawText(
            e.Graphics,
            userName,
            lstUsers.Font,
            textRect,
            (e.State & DrawItemState.Selected) != 0
                ? SystemColors.HighlightText
                : _currentTheme.ListFore,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

        e.DrawFocusRectangle();
    }

    private void btnClearPM_Click(object? sender, EventArgs e)
    {
        _pmTarget = null;
        lstUsers.ClearSelected();
        lblPMTargetName.Text = "(點擊用戶選擇)";
        lblPMTargetName.ForeColor = Color.Gray;
        txtMessage.PlaceholderText = "";
    }

    // ──────────────────────────────────────────────
    //  FEATURE 2: TYPING INDICATOR — nhận từ server
    // ──────────────────────────────────────────────

    private void OnTypingReceived(string userName)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnTypingReceived), userName); return; }

        // Delegate hoàn toàn sang service; callback UpdateTypingLabel đã được truyền vào constructor
        _service.HandleTypingReceived(userName);
    }

    private void UpdateTypingLabel()
    {
        if (!lblTyping.IsHandleCreated) return;
        if (InvokeRequired) { Invoke(UpdateTypingLabel); return; }

        var users = _service.TypingUsers.ToList();

        if (users.Count == 0)
        {
            lblTyping.Visible = false;
        }
        else
        {
            string names = string.Join(", ", users);
            lblTyping.Text = $"✏️ {names} đang gõ...";
            lblTyping.Visible = true;
        }
    }

    // ──────────────────────────────────────────────
    //  FEATURE 3: REPLY — bắt đầu, huỷ, nhận
    // ──────────────────────────────────────────────

    private void StartReply()
    {
        if (_contextMenuItemIndex < 0 || _contextMenuItemIndex >= _chatItems.Count) return;
        ChatListItem source = _chatItems[_contextMenuItemIndex];
        if (source.IsRecalled) return;

        _replyToMsgId = source.MessageId;

        var (_, disp) = ChatClientService.ParseItemText(source.Text, source.SenderName,
                                      source.Alignment == ChatItemAlignment.Center, source.IsPrivate);
        string preview = $"{source.SenderName}: {disp}";
        _replyToPreview = preview.Length > 50 ? preview[..50] + "…" : preview;

        lblReplyPreview.Text = $"↩️ 引用：{_replyToPreview}";
        pnlReplyPreview.Visible = true;

        _pmTarget = null;
        lstUsers.ClearSelected();
        lblPMTargetName.Text = "(點擊用戶選擇)";
        lblPMTargetName.ForeColor = Color.Gray;
        txtMessage.PlaceholderText = "";

        txtMessage.Focus();
    }

    private void btnCancelReply_Click(object? sender, EventArgs e)
    {
        _replyToMsgId = null;
        _replyToPreview = null;
        pnlReplyPreview.Visible = false;
    }

    private void OnReplyMessageReceived(string userName, string newMsgId, string quotedMsgId, string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string, string>(OnReplyMessageReceived),
                   userName, newMsgId, quotedMsgId, message);
            return;
        }

        string myName = txtUserName.Text.Trim();
        ChatItemAlignment alignment = string.Equals(userName, myName, StringComparison.Ordinal)
            ? ChatItemAlignment.Right
            : ChatItemAlignment.Left;

        var quoted = _chatItems.FirstOrDefault(i => i.MessageId == quotedMsgId);
        string quotedPreview;
        if (quoted is null)
        {
            // Fallback: thử lấy từ service message map
            string? cached = _service.GetMessage(quotedMsgId);
            quotedPreview = cached is not null
                ? (cached.Length > 50 ? cached[..50] + "…" : cached)
                : "(tin gốc không tìm thấy)";
        }
        else
        {
            var (_, qDisp) = ChatClientService.ParseItemText(quoted.Text, quoted.SenderName,
                                           quoted.Alignment == ChatItemAlignment.Center, quoted.IsPrivate);
            string raw = $"{quoted.SenderName}: {qDisp}";
            quotedPreview = raw.Length > 50 ? raw[..50] + "…" : raw;
        }

        var item = new ChatListItem
        {
            MessageId = newMsgId,
            Text = $"{DateTime.Now:HH:mm:ss} {userName}：{message}",
            SenderName = userName,
            Alignment = alignment,
            ReadStatus = alignment == ChatItemAlignment.Right ? ReadStatus.Sent : ReadStatus.None,
            QuotedMsgId = quotedMsgId,
            QuotedPreview = quotedPreview
        };
        AddChatItem(item);
        _service.TrackMessage(newMsgId, message);

        if (alignment == ChatItemAlignment.Left)
        {
            _client.SendReadReceipt(userName);
            IncrementUnread();
            if (_soundEnabled) SystemSounds.Asterisk.Play();
        }
    }

    // ──────────────────────────────────────────────
    //  REACTION FEATURE
    // ──────────────────────────────────────────────

    private void OnReactionReceived(string userName, string messageId, string emoji)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string>(OnReactionReceived), userName, messageId, emoji);
            return;
        }

        var item = _chatItems.FirstOrDefault(i => i.MessageId == messageId);
        if (item is null) return;

        if (!item.Reactions.ContainsKey(emoji))
            item.Reactions[emoji] = new List<string>();

        if (!item.Reactions[emoji].Contains(userName))
            item.Reactions[emoji].Add(userName);

        lstChat.Invalidate();
    }

    private void ShowReactionPicker(int itemIndex, Point loc)
    {
        ContextMenuStrip picker = new();
        foreach (string emoji in QuickReacts)
        {
            ToolStripMenuItem mi = new(emoji);
            mi.Font = new Font("Segoe UI Emoji", 16F);
            mi.Click += (_, _) =>
            {
                string mid = _chatItems[itemIndex].MessageId;
                _client.SendRawReaction(txtUserName.Text.Trim(), mid, emoji);
            };
            picker.Items.Add(mi);
        }
        picker.Show(lstChat, loc);
    }

    // ──────────────────────────────────────────────
    //  AVATAR HELPERS
    // ──────────────────────────────────────────────

    private Color GetAvatarColor(string name)
    {
        if (!_avatarColors.TryGetValue(name, out Color color))
        {
            int idx = Math.Abs(name.GetHashCode()) % AvatarPalette.Length;
            color = AvatarPalette[idx];
            _avatarColors[name] = color;
        }
        return color;
    }

    private static string GetAvatarInitial(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "?";
        return name.Substring(0, 1).ToUpperInvariant();
    }

    private void DrawAvatar(Graphics g, string name, int centerX, int centerY, int radius)
    {
        Color bg = GetAvatarColor(name);
        using SolidBrush brush = new(bg);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);

        using Pen borderPen = new(Color.FromArgb(180, 255, 255, 255), 1.5f);
        g.DrawEllipse(borderPen, centerX - radius, centerY - radius, radius * 2, radius * 2);

        string initial = GetAvatarInitial(name);
        using Font font = new("Segoe UI", Math.Max(7, radius - 3), FontStyle.Bold);
        SizeF textSize = g.MeasureString(initial, font);
        using SolidBrush textBrush = new(Color.White);
        g.DrawString(initial, font, textBrush,
            centerX - textSize.Width / 2,
            centerY - textSize.Height / 2);
    }

    // ──────────────────────────────────────────────
    //  EMOJI PANEL
    // ──────────────────────────────────────────────

    private static readonly string[] AllEmojis =
    [
        "😀","😃","😄","😁","😆","😅","😂","🤣","😊","😇",
        "🙂","😉","😍","🥰","😘","😗","😙","😚","🤩","😎",
        "🥳","😏","😒","😞","😔","😟","😕","🙁","☹️","😣",
        "😖","😫","😩","😢","😭","😤","😠","😡","🤬","😳",
        "👍","👎","👏","🙌","🤝","👋","🤞","✌️","🤟","🤙",
        "❤️","🧡","💛","💚","💙","💜","🖤","🤍","💔","💯",
        "🌟","⭐","🔥","💥","🎉","🎊","🎁","🎈","🏆","💡"
    ];

    private void BuildEmojiPanel()
    {
        if (_emojiPanelBuilt) return;
        _emojiPanelBuilt = true;

        Label lblHeader = new()
        {
            Text = "表情符號",
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            AutoSize = false,
            Size = new Size(pnlEmojiPopup.Width - 2, 26),
            Location = new Point(0, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = Color.FromArgb(50, 50, 60),
            ForeColor = Color.White
        };
        pnlEmojiPopup.Controls.Add(lblHeader);

        const int cols = 10;
        const int btnSize = 36;
        const int padding = 4;
        const int startY = 30;

        for (int i = 0; i < AllEmojis.Length; i++)
        {
            string emoji = AllEmojis[i];
            int col = i % cols;
            int row = i / cols;

            Button btn = new()
            {
                Text = emoji,
                Font = new Font("Segoe UI Emoji", 16F, FontStyle.Regular, GraphicsUnit.Point),
                Size = new Size(btnSize, btnSize),
                Location = new Point(padding + col * (btnSize + 2), startY + row * (btnSize + 2)),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.Black,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 230, 255);
            btn.Click += (_, _) =>
            {
                pnlEmojiPopup.Visible = false;
                if (_client.IsConnected)
                    _client.SendEmoji(emoji);
                else
                    txtMessage.Text += emoji;
            };
            pnlEmojiPopup.Controls.Add(btn);
        }

        Button btnClose = new()
        {
            Text = "✕",
            Size = new Size(20, 20),
            Location = new Point(pnlEmojiPopup.Width - 26, 3),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            TabStop = false,
            Font = new Font("Segoe UI", 8F, FontStyle.Bold)
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.Click += (_, _) => pnlEmojiPopup.Visible = false;
        pnlEmojiPopup.Controls.Add(btnClose);

        pnlEmojiPopup.BackColor = Color.FromArgb(248, 248, 255);
        pnlEmojiPopup.Height = startY + ((AllEmojis.Length + cols - 1) / cols) * (btnSize + 2) + padding;
        pnlEmojiPopup.BringToFront();
    }

    private void btnEmoji_Click(object? sender, EventArgs e)
    {
        if (pnlEmojiPopup.Visible) { pnlEmojiPopup.Visible = false; return; }

        Point btnPos = btnEmoji.Location;
        int panelX = Math.Max(0, btnPos.X - pnlEmojiPopup.Width / 2);
        int panelY = btnPos.Y - pnlEmojiPopup.Height - 4;
        if (panelY < 0) panelY = btnPos.Y + btnEmoji.Height + 4;

        pnlEmojiPopup.Location = new Point(panelX, panelY);
        pnlEmojiPopup.BringToFront();
        pnlEmojiPopup.Visible = true;
    }

    // ──────────────────────────────────────────────
    //  CONTEXT MENU (right-click: recall + reply)
    // ──────────────────────────────────────────────

    private ContextMenuStrip? _chatContextMenu;
    private int _contextMenuItemIndex = -1;
    private ToolStripMenuItem? _menuReply;
    private ToolStripMenuItem? _menuRecall;
    private ToolStripMenuItem? _menuCopy;

    private void BuildContextMenu()
    {
        _chatContextMenu = new ContextMenuStrip();

        ToolStripMenuItem menuCopy = new ToolStripMenuItem("📋 複製訊息");
        menuCopy.Click += (_, _) => CopySelectedMessage();
        _chatContextMenu.Items.Insert(0, menuCopy);
        _chatContextMenu.Items.Insert(1, new ToolStripSeparator());
        // Lưu reference để dùng trong lstChat_MouseDown
        _menuCopy = menuCopy;

        _menuReply = new ToolStripMenuItem("↩️ 引用回覆");
        _menuReply.Click += (_, _) => StartReply();
        _chatContextMenu.Items.Add(_menuReply);

        _chatContextMenu.Items.Add(new ToolStripSeparator());

        _menuRecall = new ToolStripMenuItem("🚫 收回訊息");
        _menuRecall.Click += (_, _) => RecallSelectedMessage();
        _chatContextMenu.Items.Add(_menuRecall);
    }

    private void lstChat_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right) return;

        int index = lstChat.IndexFromPoint(e.Location);
        if (index < 0 || index >= _chatItems.Count) return;

        ChatListItem item = _chatItems[index];
        if (item.Alignment == ChatItemAlignment.Center) return;

        _contextMenuItemIndex = index;
        lstChat.SelectedIndex = index;

        _menuReply!.Visible = !item.IsRecalled && !item.IsPrivate;
        _menuRecall!.Visible = item.Alignment == ChatItemAlignment.Right && !item.IsRecalled;
        _menuCopy!.Visible = !item.IsRecalled;

        _chatContextMenu?.Show(lstChat, e.Location);
    }

    private void RecallSelectedMessage()
    {
        if (_contextMenuItemIndex < 0 || _contextMenuItemIndex >= _chatItems.Count) return;

        ChatListItem item = _chatItems[_contextMenuItemIndex];
        if (item.IsRecalled || item.Alignment != ChatItemAlignment.Right) return;

        if (item.IsPrivate)
        {
            // FIX: gửi PMRECALL tới server → server relay tới người nhận
            if (_pmTarget != null)
                _client.SendPrivateRecall(_pmTarget, item.MessageId);

            item.Text = $"{DateTime.Now:HH:mm:ss} 🚫 [此訊息已被收回]";
            item.IsRecalled = true;
            item.Thumbnail?.Dispose();
            item.Thumbnail = null;
            if (!string.IsNullOrEmpty(item.FilePath))
            {
                try { File.Delete(item.FilePath); } catch { }
                item.FilePath = null;
            }
            _service.RemoveMessage(item.MessageId);
            lstChat.Invalidate();
            _contextMenuItemIndex = -1;
            return;
        }

        _client.SendRecall(item.MessageId);

        item.Text = $"{DateTime.Now:HH:mm:ss} 🚫 [此訊息已被收回]";
        item.IsRecalled = true;
        item.Thumbnail?.Dispose();
        item.Thumbnail = null;
        if (!string.IsNullOrEmpty(item.FilePath))
        {
            try { File.Delete(item.FilePath); } catch { }
            item.FilePath = null;
        }
        _service.RemoveMessage(item.MessageId);
        lstChat.Invalidate();
        _contextMenuItemIndex = -1;
    }

    private void CopySelectedMessage()
    {
        if (_contextMenuItemIndex < 0 ||
            _contextMenuItemIndex >= _chatItems.Count)
            return;
        ChatListItem item =
            _chatItems[_contextMenuItemIndex];
        if (item.IsRecalled) return;
        var (_, dispText) = ChatClientService.ParseItemText(
            item.Text, item.SenderName,
            item.Alignment == ChatItemAlignment.Center,
            item.IsPrivate);
        try
        {
            Clipboard.SetText(dispText);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"無法複製：{ex.Message}", "錯誤",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
    // ──────────────────────────────────────────────

    private void btnExportChat_Click(object? sender, EventArgs e)
    {
        if (_chatItems.Count == 0)
        {
            MessageBox.Show("聊天紀錄是空的，無法匯出。", "匯出", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using SaveFileDialog dlg = new()
        {
            Title = "匯出聊天紀錄",
            Filter = "純文字檔 (*.txt)|*.txt|RTF 格式 (*.rtf)|*.rtf",
            FileName = $"ChatHistory_{DateTime.Now:yyyyMMdd_HHmmss}",
            DefaultExt = "txt"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        bool isRtf = dlg.FilterIndex == 2 || dlg.FileName.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase);

        try
        {
            if (isRtf) ExportAsRtf(dlg.FileName);
            else ExportAsTxt(dlg.FileName);

            MessageBox.Show($"✅ 已成功匯出到：\n{dlg.FileName}", "匯出完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"匯出失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportAsTxt(string filePath)
    {
        StringBuilder sb = new();
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine($"  多人聊天室 - 聊天紀錄");
        sb.AppendLine($"  匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"  使用者：{txtUserName.Text.Trim()}");
        sb.AppendLine("═══════════════════════════════════════════════");
        sb.AppendLine();

        foreach (ChatListItem item in _chatItems)
        {
            string prefix = item.Alignment switch
            {
                ChatItemAlignment.Right => "  [我] ",
                ChatItemAlignment.Center => "[系統] ",
                _ => "       "
            };
            sb.AppendLine(prefix + item.Text);
        }

        sb.AppendLine();
        sb.AppendLine($"── 共 {_chatItems.Count} 則訊息 ──");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    private void ExportAsRtf(string filePath)
    {
        using RichTextBox rtb = new();
        rtb.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        rtb.SelectionColor = Color.FromArgb(0, 80, 180);
        rtb.AppendText("多人聊天室 - 聊天紀錄\n");
        rtb.SelectionFont = new Font("Segoe UI", 9F);
        rtb.SelectionColor = Color.Gray;
        rtb.AppendText($"匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}　使用者：{txtUserName.Text.Trim()}\n");
        rtb.AppendText("─────────────────────────────────────────\n\n");

        Font fontNormal = new("Segoe UI", 10F);
        Font fontSystem = new("Segoe UI", 9F, FontStyle.Italic);

        foreach (ChatListItem item in _chatItems)
        {
            switch (item.Alignment)
            {
                case ChatItemAlignment.Right:
                    rtb.SelectionFont = fontNormal;
                    rtb.SelectionColor = Color.FromArgb(20, 100, 40);
                    rtb.SelectionAlignment = HorizontalAlignment.Right;
                    rtb.AppendText(item.Text + "\n");
                    break;
                case ChatItemAlignment.Center:
                    rtb.SelectionFont = fontSystem;
                    rtb.SelectionColor = Color.Gray;
                    rtb.SelectionAlignment = HorizontalAlignment.Center;
                    rtb.AppendText(item.Text + "\n");
                    break;
                default:
                    rtb.SelectionFont = fontNormal;
                    rtb.SelectionColor = Color.FromArgb(30, 30, 80);
                    rtb.SelectionAlignment = HorizontalAlignment.Left;
                    rtb.AppendText(item.Text + "\n");
                    break;
            }
        }

        rtb.SelectionAlignment = HorizontalAlignment.Center;
        rtb.SelectionFont = fontSystem;
        rtb.SelectionColor = Color.Gray;
        rtb.AppendText($"\n── 共 {_chatItems.Count} 則訊息 ──");
        File.WriteAllText(filePath, rtb.Rtf, Encoding.UTF8);
    }

    // ──────────────────────────────────────────────
    //  VOICE INPUT
    // ──────────────────────────────────────────────

    private void btnVoice_Click(object? sender, EventArgs e)
    {
        if (_isListening) { StopListening(); return; }

        try
        {
            InitSpeechEngine();
            _speechEngine!.RecognizeAsync(RecognizeMode.Single);
            _isListening = true;
            btnVoice.Text = "🔴";
            btnVoice.BackColor = Color.FromArgb(255, 80, 80);
            btnVoice.ForeColor = Color.White;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"無法啟動語音輸入：{ex.Message}\n\n請確認系統已安裝語音識別引擎。", "語音輸入",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void InitSpeechEngine()
    {
        if (_speechEngine is not null) return;

        _speechEngine = new SpeechRecognitionEngine();
        _speechEngine.LoadGrammar(new DictationGrammar());
        _speechEngine.SetInputToDefaultAudioDevice();

        _speechEngine.SpeechRecognized += (_, args) =>
        {
            if (InvokeRequired) Invoke(() => OnSpeechRecognized(args.Result.Text));
            else OnSpeechRecognized(args.Result.Text);
        };

        _speechEngine.RecognizeCompleted += (_, _) =>
        {
            if (InvokeRequired) Invoke(StopListening); else StopListening();
        };
    }

    private void OnSpeechRecognized(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            txtMessage.Text += text;
            txtMessage.SelectionStart = txtMessage.Text.Length;
        }
        StopListening();
    }

    private void StopListening()
    {
        if (!_isListening) return;
        _isListening = false;
        try { _speechEngine?.RecognizeAsyncStop(); } catch { }
        btnVoice.Text = "🎤";
        btnVoice.BackColor = _currentTheme.ButtonBack;
        btnVoice.ForeColor = _currentTheme.ButtonFore;
    }

    // ──────────────────────────────────────────────
    //  CUSTOM DRAW – CHAT BUBBLES
    // ──────────────────────────────────────────────

    private const int AvatarRadius = 16;
    private const int AvatarDiameter = AvatarRadius * 2;
    private const int AvatarMargin = 6;
    private const int BubbleLeftOffset = AvatarDiameter + AvatarMargin * 2;
    private const int BubbleRightOffset = AvatarDiameter + AvatarMargin * 2;
    private const int NameLabelHeight = 16;

    private void lstChat_MeasureItem(object? sender, MeasureItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _chatItems.Count) return;

        ChatListItem item = _chatItems[e.Index];
        bool isCenter = item.Alignment == ChatItemAlignment.Center;

        var (_, dispText) = ChatClientService.ParseItemText(item.Text, item.SenderName, isCenter, item.IsPrivate);
        if (item.IsRecalled) dispText = "🚫 此訊息已被收回";

        int maxBubbleWidth = Math.Max(180, lstChat.ClientSize.Width - (isCenter ? 60 : BubbleLeftOffset + BubbleRightOffset + 30));
        int textHeight = TextRenderer.MeasureText(
            dispText,
            lstChat.Font,
            new Size(Math.Max(100, maxBubbleWidth - 20), 0),
            TextFormatFlags.WordBreak).Height;

        int extraHeight = 14;
        if (!isCenter) extraHeight += NameLabelHeight;
        if (item.Thumbnail is not null) extraHeight += item.Thumbnail.Height + 8;

        if (item.Alignment == ChatItemAlignment.Right && item.ReadStatus != ReadStatus.None)
            extraHeight += 14;

        if (!string.IsNullOrEmpty(item.QuotedPreview) && !item.IsRecalled)
            extraHeight += 42;

        // REACTION BAR: add height if there are reactions
        if (item.Reactions.Count > 0)
            extraHeight += 28;

        e.ItemHeight = Math.Max(40, textHeight + extraHeight + 20);
    }

    private void lstChat_DrawItem(object? sender, DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= _chatItems.Count) return;

        ChatListItem item = _chatItems[e.Index];
        ChatTheme theme = _currentTheme;

        bool isSearchMatch = _searchResultIndices.Contains(e.Index);
        bool isCurrentResult = isSearchMatch
            && _searchCurrentIdx >= 0
            && _searchCurrentIdx < _searchResultIndices.Count
            && _searchResultIndices[_searchCurrentIdx] == e.Index;

        Color rowBack = isCurrentResult
            ? BlendColor(theme.ListBack, Color.FromArgb(255, 213, 0), 0.25f)
            : theme.ListBack;

        using (SolidBrush bgBrush = new(rowBack))
            e.Graphics.FillRectangle(bgBrush, e.Bounds);

        Rectangle bounds = e.Bounds;
        bool isCenter = item.Alignment == ChatItemAlignment.Center;
        bool isRight = item.Alignment == ChatItemAlignment.Right;
        bool isLeft = item.Alignment == ChatItemAlignment.Left;

        var (_, _dispText2) = ChatClientService.ParseItemText(item.Text, item.SenderName, isCenter, item.IsPrivate);
        if (item.IsRecalled) _dispText2 = "🚫 此訊息已被收回";

        int maxBubbleWidth = Math.Max(180, bounds.Width - (isCenter ? 60 : BubbleLeftOffset + BubbleRightOffset + 30));
        Size textSize = TextRenderer.MeasureText(
            _dispText2,
            lstChat.Font,
            new Size(Math.Max(100, maxBubbleWidth - 20), 0),
            TextFormatFlags.WordBreak);

        int contentWidth = item.Thumbnail is null
            ? textSize.Width
            : Math.Max(textSize.Width, item.Thumbnail.Width);

        int bubbleWidth = Math.Min(maxBubbleWidth, Math.Max(120, contentWidth + 24));
        int bubbleHeight = textSize.Height + 14 + 12 + (item.Thumbnail is null ? 0 : item.Thumbnail.Height + 8);

        int nameY = bounds.Y + 4;
        int bubbleTopY = isCenter ? bounds.Y + 4 : bounds.Y + 4 + NameLabelHeight;

        int avatarCenterX, avatarCenterY;
        int bubbleX;

        if (isCenter)
        {
            bubbleX = bounds.X + Math.Max(12, (bounds.Width - bubbleWidth) / 2);
            avatarCenterX = 0;
            avatarCenterY = 0;
        }
        else if (isLeft)
        {
            avatarCenterX = bounds.X + AvatarMargin + AvatarRadius;
            avatarCenterY = bubbleTopY + Math.Max(AvatarRadius, bubbleHeight / 2);
            bubbleX = bounds.X + BubbleLeftOffset;
        }
        else
        {
            avatarCenterX = bounds.Right - AvatarMargin - AvatarRadius;
            avatarCenterY = bubbleTopY + Math.Max(AvatarRadius, bubbleHeight / 2);
            bubbleX = bounds.Right - BubbleRightOffset - bubbleWidth;
        }

        Rectangle bubbleRect = new(bubbleX, bubbleTopY, bubbleWidth, bubbleHeight);

        Color bubbleBack = item.Alignment switch
        {
            ChatItemAlignment.Right => item.IsPrivate ? Color.FromArgb(200, 160, 255) : theme.BubbleRight,
            ChatItemAlignment.Center => theme.BubbleCenter,
            _ => item.IsPrivate ? Color.FromArgb(220, 190, 255) : theme.BubbleLeft
        };
        Color textColor = item.Alignment switch
        {
            ChatItemAlignment.Right => item.IsRecalled ? theme.BubbleCenterText : theme.BubbleRightText,
            ChatItemAlignment.Center => theme.BubbleCenterText,
            _ => item.IsRecalled ? theme.BubbleCenterText : theme.BubbleLeftText
        };

        if (item.IsRecalled) bubbleBack = BlendColor(bubbleBack, Color.Gray, 0.5f);
        if (isSearchMatch) bubbleBack = BlendColor(bubbleBack, Color.FromArgb(255, 213, 0), isCurrentResult ? 0.35f : 0.18f);

        if (!isCenter && !string.IsNullOrEmpty(item.SenderName))
        {
            Color nameColor = GetAvatarColor(item.SenderName);
            using Font nameFont = new("Segoe UI", 7.5F, FontStyle.Bold);
            Rectangle nameLabelRect = new Rectangle(bubbleX, nameY, bubbleWidth, NameLabelHeight);
            TextRenderer.DrawText(
                e.Graphics, item.SenderName, nameFont, nameLabelRect, nameColor,
                isLeft
                    ? TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                    : TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        if (!isCenter && !string.IsNullOrEmpty(item.SenderName))
            DrawAvatar(e.Graphics, item.SenderName, avatarCenterX, avatarCenterY, AvatarRadius);

        using (GraphicsPath path = CreateRoundedRectanglePath(bubbleRect, 12))
        using (SolidBrush bubbleBrush = new(bubbleBack))
        {
            Color borderColor = isCurrentResult ? Color.FromArgb(220, 170, 0) : theme.BubbleBorder;
            using Pen borderPen = new(borderColor, isCurrentResult ? 2f : 1f);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.FillPath(bubbleBrush, path);
            e.Graphics.DrawPath(borderPen, path);
        }

        var (tsText, dispText) = ChatClientService.ParseItemText(item.Text, item.SenderName, isCenter, item.IsPrivate);
        if (item.IsRecalled)
        {
            dispText = "🚫 此訊息已被收回";
            tsText = item.Text.Length >= 8 ? item.Text[..8] : tsText;
        }

        int quoteBoxHeight = 0;
        if (!string.IsNullOrEmpty(item.QuotedPreview) && !item.IsRecalled)
        {
            quoteBoxHeight = 40;
            Rectangle quoteRect = new(bubbleRect.X + 8, bubbleRect.Y + 6, bubbleRect.Width - 16, quoteBoxHeight);

            Color quoteBack = Color.FromArgb(35, 0, 0, 0);
            using (GraphicsPath qPath = CreateRoundedRectanglePath(quoteRect, 5))
            using (SolidBrush qBrush = new(quoteBack))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(qBrush, qPath);
            }

            Color accentColor = !string.IsNullOrEmpty(item.SenderName)
                ? GetAvatarColor(item.SenderName) : Color.Gray;
            using Pen accentPen = new(accentColor, 3f);
            e.Graphics.DrawLine(accentPen, quoteRect.X + 5, quoteRect.Y + 5, quoteRect.X + 5, quoteRect.Bottom - 5);

            Color qTextColor = Color.FromArgb(155, textColor.R, textColor.G, textColor.B);
            using Font qFont = new("Segoe UI", 7.5F, FontStyle.Italic);
            Rectangle qTextRect = new(quoteRect.X + 12, quoteRect.Y + 4, quoteRect.Width - 16, quoteRect.Height - 8);
            TextRenderer.DrawText(e.Graphics, item.QuotedPreview, qFont, qTextRect,
                qTextColor, TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.EndEllipsis);
        }

        Rectangle textRect = new(
            bubbleRect.X + 10,
            bubbleRect.Y + 6 + quoteBoxHeight + (quoteBoxHeight > 0 ? 4 : 0),
            bubbleRect.Width - 20,
            textSize.Height + 2);

        TextRenderer.DrawText(e.Graphics, dispText, lstChat.Font, textRect, textColor,
            TextFormatFlags.WordBreak | TextFormatFlags.Left);

        if (item.Thumbnail is not null)
        {
            int imageY = textRect.Bottom + 6;
            int imageX = bubbleRect.X + 10;
            e.Graphics.DrawImage(item.Thumbnail, imageX, imageY, item.Thumbnail.Width, item.Thumbnail.Height);
            e.Graphics.DrawRectangle(Pens.Gray, imageX, imageY, item.Thumbnail.Width, item.Thumbnail.Height);
        }

        if (!string.IsNullOrEmpty(tsText))
        {
            using Font tsFont = new("Segoe UI", 7F, FontStyle.Regular);
            Color tsColor = Color.FromArgb(130, textColor.R, textColor.G, textColor.B);
            Rectangle tsRect = new(bubbleRect.X + 6, bubbleRect.Bottom - 14, bubbleRect.Width - 12, 13);
            TextRenderer.DrawText(e.Graphics, tsText, tsFont, tsRect, tsColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        if (isRight && item.ReadStatus != ReadStatus.None)
        {
            string statusText = item.ReadStatus == ReadStatus.Read ? "✓✓ 已讀" : "✓ 已送出";
            Color statusColor = item.ReadStatus == ReadStatus.Read
                ? Color.FromArgb(34, 153, 34)
                : Color.FromArgb(120, 120, 130);

            using Font statusFont = new("Segoe UI", 7.5F, FontStyle.Regular);
            Rectangle statusRect = new(bubbleX, bubbleRect.Bottom + 2, bubbleWidth, 14);
            TextRenderer.DrawText(e.Graphics, statusText, statusFont, statusRect, statusColor,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter);
        }

        // REACTION BAR: draw reaction pills below bubble
        if (item.Reactions.Count > 0)
        {
            int rx = bubbleX;
            int ry = bubbleRect.Bottom + (isRight && item.ReadStatus != ReadStatus.None ? 18 : 4);
            using Font rf = new("Segoe UI Emoji", 9F);
            foreach (var (em, users) in item.Reactions)
            {
                string label = $"{em}{users.Count}";
                Size rs = TextRenderer.MeasureText(label, rf);
                Rectangle rRect = new(rx, ry, rs.Width + 10, rs.Height + 4);
                Color rb = theme.BubbleBorder;
                using SolidBrush rBrush = new(Color.FromArgb(40, rb.R, rb.G, rb.B));
                e.Graphics.FillRectangle(rBrush, rRect);
                e.Graphics.DrawRectangle(new Pen(rb, 0.5f), rRect);
                TextRenderer.DrawText(e.Graphics, label, rf, rRect,
                    theme.BubbleLeftText,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                rx += rRect.Width + 4;
            }
        }

        e.DrawFocusRectangle();
    }

    // ──────────────────────────────────────────────
    //  HELPERS
    // ──────────────────────────────────────────────

    private static string ExtractSenderName(string msg)
    {
        if (msg.StartsWith("[")) return string.Empty;
        int sep = msg.IndexOf('：');
        if (sep > 0) return msg[..sep].Trim();
        return string.Empty;
    }

    private static Color BlendColor(Color c1, Color c2, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(c1.A + (c2.A - c1.A) * t),
            (int)(c1.R + (c2.R - c1.R) * t),
            (int)(c1.G + (c2.G - c1.G) * t),
            (int)(c1.B + (c2.B - c1.B) * t));
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int radius)
    {
        GraphicsPath path = new();
        int diameter = radius * 2;
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static Image CreateThumbnail(Image original, int maxWidth, int maxHeight)
    {
        Size size = GetScaledSize(original.Size, maxWidth, maxHeight);
        Bitmap bmp = new(size.Width, size.Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.DrawImage(original, 0, 0, size.Width, size.Height);
        return bmp;
    }

    private static Size GetScaledSize(Size original, int maxWidth, int maxHeight)
    {
        if (original.Width <= maxWidth && original.Height <= maxHeight) return original;
        double ratioX = (double)maxWidth / original.Width;
        double ratioY = (double)maxHeight / original.Height;
        double ratio = Math.Min(ratioX, ratioY);
        return new Size(
            Math.Max(1, (int)(original.Width * ratio)),
            Math.Max(1, (int)(original.Height * ratio)));
    }

    private static string SaveIncomingImage(string fileName, byte[] imageBytes)
    {
        string baseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "MultiChatClientImages",
            DateTime.Now.ToString("yyyyMMdd"));
        Directory.CreateDirectory(baseFolder);
        string safeFileName = Path.GetFileName(fileName);
        // FIX: dùng Guid thay DateTime để tránh race condition khi 2 client nhận cùng lúc
        string uniquePrefix = Guid.NewGuid().ToString("N")[..8];
        string fullPath = Path.Combine(baseFolder, $"{uniquePrefix}_{safeFileName}");
        File.WriteAllBytes(fullPath, imageBytes);
        return fullPath;
    }

    // ──────────────────────────────────────────────
    //  MOUSE
    // ──────────────────────────────────────────────

    private void lstChat_MouseDoubleClick(object? sender, MouseEventArgs e)
    {
        int index = lstChat.IndexFromPoint(e.Location);
        if (index < 0 || index >= _chatItems.Count) return;

        ChatListItem item = _chatItems[index];

        // ✅ ƯU TIÊN 1: Nếu item có FilePath hợp lệ (ảnh hoặc file) → mở bằng Windows
        if (!string.IsNullOrWhiteSpace(item.FilePath) && File.Exists(item.FilePath))
        {
            try
            {
                Process.Start(new ProcessStartInfo(item.FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法開啟圖片：{ex.Message}");
            }
            return;
        }

        // ✅ ƯU TIÊN 2: Nếu KHÔNG phải ảnh/file → mở Reaction Picker (chỉ cho tin nhắn thường)
        if (_client.IsConnected && !item.IsRecalled && item.Alignment != ChatItemAlignment.Center)
        {
            ShowReactionPicker(index, e.Location);
        }
    }

    // ──────────────────────────────────────────────
    //  SEND FILE
    // ──────────────────────────────────────────────

    private const long MaxFileBytes = 5 * 1024 * 1024;

    private void btnSendFile_Click(object? sender, EventArgs e)
    {
        if (!_client.IsConnected) { MessageBox.Show("請先連線後再傳送檔案"); return; }

        using OpenFileDialog dialog = new()
        {
            Title = "選擇要傳送的檔案",
            Filter = "所有檔案|*.*|文件|*.pdf;*.docx;*.txt;*.xlsx|壓縮檔|*.zip;*.rar"
        };

        if (dialog.ShowDialog() != DialogResult.OK) return;

        FileInfo fileInfo = new(dialog.FileName);
        if (!fileInfo.Exists) return;

        if (fileInfo.Length > MaxFileBytes)
        {
            MessageBox.Show("檔案請控制在 5MB 以內。", "檔案過大");
            return;
        }

        byte[] fileBytes = File.ReadAllBytes(dialog.FileName);

        // Nếu đang PM → gửi riêng tư, không broadcast
        if (_pmTarget != null)
            _client.SendPrivateFile(_pmTarget, fileInfo.Name, fileBytes);
        else
            _client.SendFile(fileInfo.Name, fileBytes);
    }

    // ──────────────────────────────────────────────
    //  SCREENSHOT & SEND
    // ──────────────────────────────────────────────

    private void btnScreenshot_Click(object? sender, EventArgs e)
    {
        if (!_client.IsConnected) { MessageBox.Show("請先連線後再截圖"); return; }

        Hide();
        System.Threading.Thread.Sleep(300);

        try
        {
            Rectangle bounds = Screen.PrimaryScreen!.Bounds;
            using Bitmap bmp = new(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bmp))
                g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);

            using MemoryStream ms = new();
            bmp.Save(ms, ImageFormat.Png);
            byte[] imageBytes = ms.ToArray();

            if (imageBytes.Length > MaxImageBytes)
            {
                MessageBox.Show("截圖超過 2MB 限制，請縮小解析度後重試。", "截圖過大");
                return;
            }

            string fileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            _client.SendImage(fileName, imageBytes);
            AppendChat($"[本機] 截圖已送出：{fileName}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"截圖失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Show();
            Activate();
        }
    }

    // ──────────────────────────────────────────────
    //  RECEIVE FILE
    // ──────────────────────────────────────────────

    private void AppendFileMessage(string userName, string fileName, byte[] fileBytes)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, byte[]>(AppendFileMessage), userName, fileName, fileBytes);
            return;
        }

        string savePath = SaveIncomingFile(fileName, fileBytes);
        string myName = txtUserName.Text.Trim();

        ChatItemAlignment alignment = string.Equals(userName, myName, StringComparison.Ordinal)
            ? ChatItemAlignment.Right
            : ChatItemAlignment.Left;

        string displayText = $"{DateTime.Now:HH:mm:ss} {userName}：📁 檔案 - {fileName}  [點兩下開啟]";
        var item = new ChatListItem
        {
            MessageId = Guid.NewGuid().ToString("N")[..8],
            Text = displayText,
            FilePath = savePath,
            SenderName = userName,
            Alignment = alignment,
            ReadStatus = alignment == ChatItemAlignment.Right ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);

        if (_soundEnabled && alignment == ChatItemAlignment.Left)
            SystemSounds.Asterisk.Play();
    }

    private void AppendPrivateImageMessage(string fromUser, string messageId, string fileName, byte[] imageBytes)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string, byte[]>(AppendPrivateImageMessage), fromUser, messageId, fileName, imageBytes);
            return;
        }

        string myName = txtUserName.Text.Trim();
        string savePath = SaveIncomingImage(fileName, imageBytes);
        using MemoryStream ms = new(imageBytes);
        using Image original = Image.FromStream(ms);
        Image thumb = CreateThumbnail(original, 220, 160);

        bool isMine = string.Equals(fromUser, myName, StringComparison.Ordinal);
        ChatItemAlignment alignment = isMine ? ChatItemAlignment.Right : ChatItemAlignment.Left;
        string direction = isMine ? $"私訊→{_pmTarget}" : $"私訊←{fromUser}";

        var item = new ChatListItem
        {
            MessageId = messageId,
            Text = $"{DateTime.Now:HH:mm:ss} 🔒 [{direction}] {fromUser}：圖片 - {fileName}",
            Thumbnail = thumb,
            FilePath = savePath,
            SenderName = fromUser,
            Alignment = alignment,
            IsPrivate = true,
            ReadStatus = isMine ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);

        if (_soundEnabled && !isMine)
            SystemSounds.Exclamation.Play();
    }

    private void AppendPrivateFileMessage(string fromUser, string messageId, string fileName, byte[] fileBytes)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string, string, string, byte[]>(AppendPrivateFileMessage), fromUser, messageId, fileName, fileBytes);
            return;
        }

        string myName = txtUserName.Text.Trim();
        string savePath = SaveIncomingFile(fileName, fileBytes);

        bool isMine = string.Equals(fromUser, myName, StringComparison.Ordinal);
        ChatItemAlignment alignment = isMine ? ChatItemAlignment.Right : ChatItemAlignment.Left;
        string direction = isMine ? $"私訊→{_pmTarget}" : $"私訊←{fromUser}";

        string displayText = $"{DateTime.Now:HH:mm:ss} 🔒 [{direction}] {fromUser}：📁 檔案 - {fileName}  [點兩下開啟]";
        var item = new ChatListItem
        {
            MessageId = messageId,
            Text = displayText,
            FilePath = savePath,
            SenderName = fromUser,
            Alignment = alignment,
            IsPrivate = true,
            ReadStatus = isMine ? ReadStatus.Sent : ReadStatus.None
        };
        AddChatItem(item);

        if (_soundEnabled && !isMine)
            SystemSounds.Exclamation.Play();
    }

    private static string SaveIncomingFile(string fileName, byte[] fileBytes)
    {
        string baseFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "MultiChatClientFiles",
            DateTime.Now.ToString("yyyyMMdd"));
        Directory.CreateDirectory(baseFolder);
        string safeFileName = Path.GetFileName(fileName);
        // FIX: dùng Guid thay DateTime để tránh race condition khi 2 client nhận cùng lúc
        // hoặc khi file đang bị process khác giữ (đã mở bằng double-click)
        string uniquePrefix = Guid.NewGuid().ToString("N")[..8];
        string fullPath = Path.Combine(baseFolder, $"{uniquePrefix}_{safeFileName}");
        File.WriteAllBytes(fullPath, fileBytes);
        return fullPath;
    }

    // ──────────────────────────────────────────────
    //  CLEAR / LOAD CHAT HISTORY
    // ──────────────────────────────────────────────

    private void btnClearChat_Click(object? sender, EventArgs e)
    {
        if (_chatItems.Count == 0)
        {
            MessageBox.Show("聊天紀錄已經是空的。", "清除", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var result = MessageBox.Show(
            $"確定要清除全部 {_chatItems.Count} 則訊息嗎？\n此動作無法復原。",
            "確認清除", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes) return;

        foreach (ChatListItem item in _chatItems) item.Thumbnail?.Dispose();
        _chatItems.Clear();
        lstChat.Items.Clear();
        ClearSearch();
        _contextMenuItemIndex = -1;
        lstChat.Invalidate();
    }

    private void btnLoadChat_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog dlg = new()
        {
            Title = "載入聊天紀錄",
            Filter = "純文字檔 (*.txt)|*.txt",
            DefaultExt = "txt"
        };

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            string[] lines = File.ReadAllLines(dlg.FileName, System.Text.Encoding.UTF8);

            foreach (ChatListItem item in _chatItems) item.Thumbnail?.Dispose();
            _chatItems.Clear();
            lstChat.Items.Clear();

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith("═") || line.StartsWith("──") || line.StartsWith("  多人")) continue;

                ChatItemAlignment align;
                string text;

                if (line.StartsWith("  [我] "))
                {
                    align = ChatItemAlignment.Right;
                    text = line["  [我] ".Length..];
                }
                else if (line.StartsWith("[系統] "))
                {
                    align = ChatItemAlignment.Center;
                    text = line["[系統] ".Length..];
                }
                else if (line.StartsWith("       "))
                {
                    align = ChatItemAlignment.Left;
                    text = line.TrimStart();
                }
                else continue;

                string senderName = ExtractSenderName(text);
                string msgId = Guid.NewGuid().ToString("N")[..8];
                var item = new ChatListItem
                {
                    MessageId = msgId,
                    Text = text,
                    SenderName = senderName,
                    Alignment = align,
                    ReadStatus = ReadStatus.None
                };
                _chatItems.Add(item);
                lstChat.Items.Add(item);
            }

            if (lstChat.Items.Count > 0) lstChat.TopIndex = lstChat.Items.Count - 1;
            lstChat.Invalidate();
            MessageBox.Show($"✅ 已載入 {_chatItems.Count} 則紀錄", "載入完成",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"載入失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ──────────────────────────────────────────────
    //  NESTED TYPES (không thay đổi)
    // ──────────────────────────────────────────────

    private sealed class ChatListItem
    {
        public string MessageId { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public Image? Thumbnail { get; set; }
        public string? FilePath { get; set; }
        public ChatItemAlignment Alignment { get; set; } = ChatItemAlignment.Left;
        public ReadStatus ReadStatus { get; set; } = ReadStatus.None;
        public bool IsRecalled { get; set; } = false;
        public bool IsPrivate { get; set; } = false;
        public string? QuotedMsgId { get; set; }
        public string? QuotedPreview { get; set; }

        /// <summary>Emoji reactions. Key = emoji string, Value = list of userNames who reacted.</summary>
        public Dictionary<string, List<string>> Reactions { get; } = new();

        public override string ToString() => Text;
    }

    private enum ChatItemAlignment { Left, Right, Center }

    private enum ReadStatus
    {
        None,
        Sent,
        Read
    }

    private void txtServerIP_TextChanged(object sender, EventArgs e)
    {

    }
}
