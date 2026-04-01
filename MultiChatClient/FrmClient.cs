using HA = System.Windows.Forms.HorizontalAlignment;
using System.Text;

namespace MultiChatClient;

public partial class FrmClient : Form
{
    private readonly ChatClient _client = new();
    private bool    _isConnected;
    private string  _myUserName   = "";
    private int     _unreadCount;
    private ChatTheme _theme      = ChatTheme.Dark;
    private float   _fontSize     = 10.5f;

    private System.Windows.Forms.Timer? _typingTimer;
    private string  _tempFolder = Path.Combine(Path.GetTempPath(), "MultiChat");

    // ── Avatar colour palette ─────────────────────────────────────────────────
    private static readonly Color[] AvatarColors =
    {
        Color.FromArgb(124,106,247), Color.FromArgb(0,150,255),
        Color.FromArgb(0,180,120),   Color.FromArgb(255,100,80),
        Color.FromArgb(255,160,0),   Color.FromArgb(200,60,150),
        Color.FromArgb(80,200,200),  Color.FromArgb(160,80,240)
    };
    private static Color AvatarColor(string name)
        => AvatarColors[Math.Abs(name.GetHashCode()) % AvatarColors.Length];

    // ── Recall tracking ───────────────────────────────────────────────────────
    private int    _recallBubbleStart = -1;
    private int    _recallBubbleEnd   = -1;
    private string _lastMsgId         = "";

    // ── Read-receipt tracking ─────────────────────────────────────────────────
    private int  _receiptStart = -1;
    private int  _receiptEnd   = -1;
    private bool _othersOnline = false;  // true once another user is seen

    // ── Colour shortcuts ──────────────────────────────────────────────────────
    private Color CBg        => _theme.Background;
    private Color CAccent    => _theme.Accent;
    private Color COwnBubble => _theme.OwnBubble;
    private Color COtherBub  => _theme.OtherBubble;
    private Color CSys       => _theme.SysText;
    private Color CText      => _theme.Text;
    private Color CTime      => _theme.TimeText;
    private Color CGreen     => _theme.Success;
    private Color CRed       => _theme.Danger;
    private Color CName      => _theme.NameText;

    // ─────────────────────────────────────────────────────────────────────────
    public FrmClient()
    {
        InitializeComponent();
        Directory.CreateDirectory(_tempFolder);

        _client.MessageReceived += OnMessage;
        _client.StatusChanged   += OnStatus;
        _client.ImageReceived   += OnImage;
        _client.FileReceived    += OnFile;
        _client.TypingReceived  += OnTyping;
        _client.PmReceived      += OnPm;
        _client.RecallReceived  += OnRecall;

        _typingTimer = new System.Windows.Forms.Timer { Interval = 3000 };
        _typingTimer.Tick += (_, _) => { lblTyping.Text = ""; _typingTimer.Stop(); };
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  LOAD / RESIZE
    // ═════════════════════════════════════════════════════════════════════════

    private void FrmClient_Load(object? sender, EventArgs e)
    {
        txtUserName.Text = Environment.UserName;
        AppendSystem("歡迎使用 💬 MultiChat！");
        AppendSystem("功能：文字·圖片·表情·Avatar·私訊·撤回·已讀·搜尋·主題·匯出·通知");
    }

    private void FrmClient_Resize(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized) { notifyIcon.Visible = true; Hide(); }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  THEME
    // ═════════════════════════════════════════════════════════════════════════

    private void cmbTheme_Changed(object? sender, EventArgs e)
    {
        _theme = ChatTheme.All[cmbTheme.SelectedIndex];
        ApplyTheme();
        AppendSystem($"🎨 已切換主題：{_theme.Name}");
    }

    private void ApplyTheme()
    {
        var T = _theme;
        BackColor = T.Background;
        pnlHeader.BackColor  = T.Header;
        lblStatusDot.ForeColor = _isConnected ? T.Success : T.Danger;
        lblConnectionStatus.ForeColor = _isConnected ? T.Success : T.Muted;
        pnlConnection.BackColor = T.Panel;
        lblServerIPLabel.ForeColor = lblPortLabel.ForeColor = lblUserNameLabel.ForeColor = T.Muted;
        foreach (var tb in new[] { txtServerIP, txtPort, txtUserName }) { tb.BackColor = T.Input; tb.ForeColor = T.Text; }
        btnConnect.BackColor = T.Accent; btnDisconnect.BackColor = T.Danger;
        pnlSettings.BackColor = T.Toolbar;
        lblThemeLabel.ForeColor = lblFontLabel.ForeColor = T.Muted;
        cmbTheme.BackColor = T.Input; cmbTheme.ForeColor = T.Text;
        trkFontSize.BackColor = T.Toolbar;
        chkSound.ForeColor = T.Text; chkSound.BackColor = T.Toolbar;
        btnSearch.BackColor = btnExport.BackColor = T.Input;
        btnSearch.ForeColor = btnExport.ForeColor = T.Text;
        btnClearAll.BackColor = T.Danger;
        pnlToolbar.BackColor = T.Toolbar;
        foreach (var b in new[] { btnEmoji, btnSendImage, btnSendFile, btnCapture, btnVoiceMsg, btnRecall })
        { b.BackColor = T.Toolbar; b.ForeColor = T.Text; }
        pnlInput.BackColor = T.Panel; txtMessage.BackColor = T.Input; txtMessage.ForeColor = T.Text;
        lblCharCount.BackColor = T.Panel; lblCharCount.ForeColor = T.Muted; btnSend.BackColor = T.Accent;
        pnlChatHeader.BackColor = lblChatTitle.BackColor = T.Header; lblChatTitle.ForeColor = T.Text;
        splitMain.BackColor = splitMain.Panel1.BackColor = T.Background;
        splitMain.Panel2.BackColor = T.Panel;
        rtbChat.BackColor = T.Background; rtbChat.ForeColor = T.Text;
        lblTyping.BackColor = T.Background; lblTyping.ForeColor = T.Muted;
        lblUsersTitle.BackColor = T.Header; lblUsersTitle.ForeColor = T.Text;
        lblUsersHint.BackColor  = T.Panel;  lblUsersHint.ForeColor  = T.Muted;
        lstUsers.BackColor = T.Panel; lstUsers.ForeColor = T.Text;
        chatMenu.BackColor = T.Panel; chatMenu.ForeColor = T.Text;
        foreach (ToolStripItem item in chatMenu.Items)
        { item.BackColor = T.Panel; item.ForeColor = T.Text; }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  FONT SIZE
    // ═════════════════════════════════════════════════════════════════════════

    private void trkFontSize_Changed(object? sender, EventArgs e)
    {
        _fontSize = trkFontSize.Value;
        _chatFont = new Font("Segoe UI", _fontSize);
        AppendSystem($"🔠 Cỡ chữ: {_fontSize}pt");
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CONNECTION
    // ═════════════════════════════════════════════════════════════════════════

    private void btnConnect_Click(object? sender, EventArgs e)
    {
        _myUserName = txtUserName.Text.Trim();
        if (string.IsNullOrWhiteSpace(_myUserName))               { Tip("請輸入暱稱！"); return; }
        if (!int.TryParse(txtPort.Text.Trim(), out int port))     { Tip("Port 格式錯誤"); return; }
        try { _client.Connect(txtServerIP.Text.Trim(), port, _myUserName); }
        catch (Exception ex) { MessageBox.Show($"連線失敗：{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void btnDisconnect_Click(object? sender, EventArgs e) => _client.Disconnect();

    // ═════════════════════════════════════════════════════════════════════════
    //  TEXT
    // ═════════════════════════════════════════════════════════════════════════

    private void btnSend_Click(object? sender, EventArgs e) => SendText();

    private void txtMessage_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter && !e.Shift) { e.SuppressKeyPress = true; SendText(); return; }
        if (_isConnected) _client.SendTyping();
    }

    private void txtMessage_TextChanged(object? sender, EventArgs e)
    {
        int len = txtMessage.Text.Length;
        if (len > 500) { txtMessage.Text = txtMessage.Text[..500]; return; }
        lblCharCount.Text      = $"{len}/500";
        lblCharCount.ForeColor = len > 450 ? Color.OrangeRed : CTime;
        btnSend.Enabled        = len > 0 && _isConnected;
    }

    private void SendText()
    {
        string msg = txtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(msg) || !_isConnected) return;
        string id = GenerateMsgId();
        _lastMsgId = id;
        _client.SendChat(msg);
        AppendOwn(msg);
        txtMessage.Clear();
        txtMessage.Focus();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  ↩️ RECALL  (撤回最後一則訊息)
    // ═════════════════════════════════════════════════════════════════════════

    private void btnRecall_Click(object? sender, EventArgs e) => RecallLastMessage();

    private void RecallLastMessage()
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        if (_recallBubbleStart < 0 || _recallBubbleStart >= rtbChat.TextLength)
        { Tip("沒有可撤回的訊息！"); return; }

        try
        {
            int len = _recallBubbleEnd - _recallBubbleStart;
            if (len <= 0) return;

            // Replace own bubble text with recalled placeholder
            rtbChat.Select(_recallBubbleStart, len);
            rtbChat.SelectionColor     = CTime;
            rtbChat.SelectionBackColor = CBg;
            rtbChat.SelectionFont      = new Font("Segoe UI", _fontSize, FontStyle.Italic);
            rtbChat.SelectedText       = "  [此訊息已撤回] ❌  \n";

            _client.SendRecall(_lastMsgId);
            _recallBubbleStart = -1;

            // Also clear read receipt
            _receiptStart = -1;
            AppendSystem("你撤回了一則訊息");
        }
        catch { /* RTB position drift — ignore */ }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  💬 PRIVATE MESSAGE  (私訊)
    // ═════════════════════════════════════════════════════════════════════════

    private void lstUsers_DoubleClick(object? sender, EventArgs e)
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        if (lstUsers.SelectedItem is not string item) return;

        // Strip prefix and "(你)" suffix
        string target = item.Replace("  🟢  ", "").Replace("  🟡  ", "").Replace("（你）", "").Trim();
        if (string.IsNullOrEmpty(target) || target == _myUserName)
        { Tip("無法發送私訊給自己！"); return; }

        ShowPmDialog(target);
    }

    private void ShowPmDialog(string recipient)
    {
        var T   = _theme;
        var dlg = new Form
        {
            Text            = $"💬 私訊給 {recipient}",
            Size            = new Size(420, 160),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            BackColor       = T.Panel,
            StartPosition   = FormStartPosition.CenterParent,
            MaximizeBox     = false,
            MinimizeBox     = false
        };

        var lbl = new Label { Text = $"發送私訊給 【{recipient}】：", Font = new Font("Segoe UI", 9.5f), ForeColor = T.Text, Location = new Point(12, 12), Size = new Size(380, 22) };

        var txt = new TextBox { Font = new Font("Segoe UI", 11f), BackColor = T.Input, ForeColor = T.Text, BorderStyle = BorderStyle.FixedSingle, Location = new Point(12, 40), Size = new Size(380, 30) };

        var btnSendPm = new Button
        {
            Text = "送出私訊 💬", BackColor = CAccent, ForeColor = Color.White, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            FlatStyle = FlatStyle.Flat, Location = new Point(240, 84), Size = new Size(150, 32), Cursor = Cursors.Hand
        };
        btnSendPm.FlatAppearance.BorderSize = 0;

        var btnCancel = new Button
        {
            Text = "取消", BackColor = T.Toolbar, ForeColor = T.Text, Font = new Font("Segoe UI", 9.5f),
            FlatStyle = FlatStyle.Flat, Location = new Point(12, 84), Size = new Size(80, 32)
        };
        btnCancel.FlatAppearance.BorderSize = 0;
        btnCancel.Click += (_, _) => dlg.Close();

        btnSendPm.Click += (_, _) =>
        {
            string msg = txt.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            _client.SendPm(recipient, msg);
            AppendPmOwn(recipient, msg);
            dlg.Close();
        };

        txt.KeyDown += (_, ke) => { if (ke.KeyCode == Keys.Enter) { ke.SuppressKeyPress = true; btnSendPm.PerformClick(); } };

        dlg.Controls.AddRange(new Control[] { lbl, txt, btnSendPm, btnCancel });
        dlg.ActiveControl = txt;
        dlg.ShowDialog(this);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  MEDIA BUTTONS
    // ═════════════════════════════════════════════════════════════════════════

    private void btnSendImage_Click(object? sender, EventArgs e)
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        using var dlg = new OpenFileDialog { Title = "選擇圖片", Filter = "圖片|*.jpg;*.jpeg;*.png;*.gif;*.bmp;*.webp|所有|*.*" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            using var img   = Image.FromFile(dlg.FileName);
            using var thumb = ResizeImg(img, 300, 300);
            using var ms    = new MemoryStream();
            thumb.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] bytes    = ms.ToArray();
            _client.SendImage(bytes);
            AppendOwnImage(bytes, Path.GetFileName(dlg.FileName));
        }
        catch (Exception ex) { MessageBox.Show($"圖片失敗：{ex.Message}"); }
    }

    private void btnSendFile_Click(object? sender, EventArgs e)
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        using var dlg = new OpenFileDialog { Title = "選擇檔案", Filter = "所有檔案|*.*" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        var fi = new FileInfo(dlg.FileName);
        if (fi.Length > 2 * 1024 * 1024) { Tip("檔案太大！最大 2 MB"); return; }
        try
        {
            byte[] bytes = File.ReadAllBytes(dlg.FileName);
            _client.SendFile(fi.Name, bytes);
            AppendOwnFile(fi.Name, bytes);
        }
        catch (Exception ex) { MessageBox.Show($"檔案失敗：{ex.Message}"); }
    }

    private void btnCapture_Click(object? sender, EventArgs e)
    {
        var r = MessageBox.Show("選擇截取方式：\n[是] 截圖螢幕\n[否] 從相簿選取", "📷", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (r == DialogResult.Yes) CaptureScreen();
        else if (r == DialogResult.No) btnSendImage_Click(sender, e);
    }

    private void CaptureScreen()
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        try
        {
            Hide(); Thread.Sleep(280);
            var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
            using var bmp = new Bitmap(bounds.Width, bounds.Height);
            using var g   = Graphics.FromImage(bmp);
            g.CopyFromScreen(bounds.Location, Point.Empty, bounds.Size);
            Show(); WindowState = FormWindowState.Normal;
            using var ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] bytes = ms.ToArray();
            _client.SendImage(bytes);
            AppendOwnImage(bytes, "截圖.jpg");
        }
        catch (Exception ex) { Show(); MessageBox.Show($"截圖失敗：{ex.Message}"); }
    }

    private void btnVoiceMsg_Click(object? sender, EventArgs e)
        => MessageBox.Show("🎤 語音訊息\n需加入 NAudio 套件以啟用錄音。", "語音訊息", MessageBoxButtons.OK, MessageBoxIcon.Information);

    private void btnCall_Click(object? sender, EventArgs e)
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        AppendSystem($"📞 {_myUserName} 發起語音通話邀請...");
        _client.SendChat("[📞 發起語音通話邀請]");
    }

    private void btnVideoCall_Click(object? sender, EventArgs e)
    {
        if (!_isConnected) { Tip("請先連線！"); return; }
        AppendSystem($"🎥 {_myUserName} 發起視訊通話邀請...");
        _client.SendChat("[🎥 發起視訊通話邀請]");
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  EMOJI
    // ═════════════════════════════════════════════════════════════════════════

    private void btnEmoji_Click(object? sender, EventArgs e)
    {
        string[][] all = {
            new[]{"😀","😂","🥰","😎","🤔","😭","😡","🥺","😴","🤯","😏","🙃","😇","🤩","🥳"},
            new[]{"👍","👎","❤️","🔥","🎉","✨","💯","🙏","💪","🎊","👋","🤝","💔","💘","⭐"},
            new[]{"🍔","🍕","🍜","🍣","☕","🍺","🎮","📱","💻","🚀","🎵","📸","🌈","🌙","💎"},
            new[]{"😤","🤣","😍","🤪","😬","🙄","😰","😱","🤮","😈","👀","🫶","🤌","💀","🫠"}
        };
        var T = _theme;
        var frm = new Form { Size = new Size(420, 60), FormBorderStyle = FormBorderStyle.FixedToolWindow, BackColor = T.Panel, Text = "😊 Emoji", StartPosition = FormStartPosition.Manual };
        var pt  = PointToScreen(new Point(btnEmoji.Left, pnlToolbar.Top));
        frm.Location = new Point(Math.Max(0, pt.X), pt.Y - 240);

        int x = 6, y = 6;
        foreach (var grp in all)
        {
            foreach (var em in grp)
            {
                string e2 = em;
                var b = new Button { Text = e2, Font = new Font("Segoe UI Emoji", 14f), BackColor = T.Toolbar, ForeColor = T.Text, FlatStyle = FlatStyle.Flat, Size = new Size(40, 36), Location = new Point(x, y) };
                b.FlatAppearance.BorderSize = 0;
                b.Click += (_, _) =>
                {
                    if (!txtMessage.Enabled) return;
                    int pos = txtMessage.SelectionStart;
                    txtMessage.Text = txtMessage.Text.Insert(pos, e2);
                    txtMessage.SelectionStart = pos + e2.Length;
                    txtMessage.Focus(); frm.Close();
                };
                frm.Controls.Add(b);
                x += 42; if (x > 400) { x = 6; y += 38; }
            }
            x = 6; y += 42;
        }
        frm.Height = y + 30;
        frm.ShowDialog(this);
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  SEARCH / EXPORT / CLEAR
    // ═════════════════════════════════════════════════════════════════════════

    private void btnSearch_Click(object? sender, EventArgs e)
    {
        var sf = new SearchForm(rtbChat, _theme);
        sf.Location = new Point(Location.X + Width - sf.Width - 20, Location.Y + 200);
        sf.Show(this);
    }

    private void btnExport_Click(object? sender, EventArgs e)
    {
        using var dlg = new SaveFileDialog { Title = "儲存聊天記錄", Filter = "文字|*.txt|RTF|*.rtf", FileName = $"Chat_{DateTime.Now:yyyyMMdd_HHmm}" };
        if (dlg.ShowDialog() != DialogResult.OK) return;
        try
        {
            File.WriteAllText(dlg.FileName, dlg.FilterIndex == 2 ? rtbChat.Rtf ?? "" : rtbChat.Text, Encoding.UTF8);
            MessageBox.Show($"✅ 匯出成功！\n{dlg.FileName}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex) { MessageBox.Show($"匯出失敗：{ex.Message}"); }
    }

    private void btnClearAll_Click(object? sender, EventArgs e)  => ClearChat();
    private void btnClearChat_Click(object? sender, EventArgs e) => ClearChat();

    private void ClearChat()
    {
        if (MessageBox.Show("確定清除所有聊天記錄？", "清除", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
        {
            rtbChat.Clear(); _unreadCount = 0; UpdateTitle();
            _recallBubbleStart = -1; _receiptStart = -1;
            AppendSystem("聊天記錄已清除 🗑");
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CLIENT EVENTS
    // ═════════════════════════════════════════════════════════════════════════

    private void OnMessage(string raw)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnMessage), raw); return; }
        if (raw.StartsWith("[系統]"))
        {
            string sys = raw[4..].Trim();
            AppendSystem(sys);
            TryUpdateUsers(sys);
        }
        else
        {
            int c = raw.IndexOf('：');
            if (c > 0)
            {
                string sndr    = raw[..c];
                string content = raw[(c+1)..];
                if (sndr != _myUserName)
                {
                    AppendOther(sndr, content);
                    UpgradeReceipt();           // someone is reading → mark sent as read
                    NotifyNew(sndr, content);
                }
            }
            else AppendSystem(raw);
        }
    }

    private void OnImage(string sender, byte[] bytes)
    {
        if (InvokeRequired) { Invoke(new Action<string, byte[]>(OnImage), sender, bytes); return; }
        if (sender != _myUserName) { AppendOtherImage(sender, bytes); UpgradeReceipt(); NotifyNew(sender, "[圖片 🖼️]"); }
    }

    private void OnFile(string sender, string data)
    {
        if (InvokeRequired) { Invoke(new Action<string, string>(OnFile), sender, data); return; }
        int p = data.IndexOf('|');
        if (p < 0) return;
        string fn = data[..p], b64 = data[(p+1)..];
        if (sender != _myUserName) { AppendOtherFile(sender, fn, b64); UpgradeReceipt(); NotifyNew(sender, $"[檔案：{fn}]"); }
    }

    private void OnTyping(string sender)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnTyping), sender); return; }
        if (sender == _myUserName) return;
        lblTyping.Text = $"  ✍️  {sender} 正在輸入...";
        _typingTimer?.Stop(); _typingTimer?.Start();
    }

    private void OnPm(string sender, string recipient, string message)
    {
        if (InvokeRequired) { Invoke(new Action<string, string, string>(OnPm), sender, recipient, message); return; }

        // Only show PM if we are sender or recipient
        bool isMine = sender == _myUserName;
        bool isForMe = recipient == _myUserName;

        if (!isMine && !isForMe) return;  // not our business

        if (!isMine)  // received PM
        {
            AppendPmOther(sender, message);
            NotifyNew(sender, $"[私訊] {message}");
        }
        // isMine case already handled by AppendPmOwn in ShowPmDialog
    }

    private void OnRecall(string sender, string msgId)
    {
        if (InvokeRequired) { Invoke(new Action<string, string>(OnRecall), sender, msgId); return; }
        if (sender != _myUserName)
            AppendSystem($"↩️  {sender} 撤回了一則訊息");
    }

    private void OnStatus(string status)
    {
        if (InvokeRequired) { Invoke(new Action<string>(OnStatus), status); return; }
        bool on = status == "已連線";
        _isConnected = on;
        SetConnectUI(on);
        if (on)
        {
            AppendSystem($"✅ 連線成功！你好，{_myUserName}！");
            lstUsers.Items.Clear();
            lstUsers.Items.Add($"  🟢  {_myUserName}（你）");
            _unreadCount = 0; UpdateTitle();
            _othersOnline = false;
        }
        else if (status == "已離線") { AppendSystem("🔴 已斷線。"); lstUsers.Items.Clear(); _othersOnline = false; }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  UI STATE
    // ═════════════════════════════════════════════════════════════════════════

    private void SetConnectUI(bool on)
    {
        btnConnect.Enabled = !on; btnDisconnect.Enabled = on;
        txtServerIP.Enabled = !on; txtPort.Enabled = !on; txtUserName.Enabled = !on;
        txtMessage.Enabled = on; btnSend.Enabled = false;
        btnSendImage.Enabled = on; btnSendFile.Enabled = on; btnCapture.Enabled = on;
        lblStatusDot.ForeColor        = on ? CGreen : CRed;
        lblConnectionStatus.Text      = on ? "已連線" : "未連線";
        lblConnectionStatus.ForeColor = on ? CGreen  : _theme.Muted;
    }

    private void TryUpdateUsers(string msg)
    {
        if (msg.Contains("已加入"))
        {
            string n    = msg.Replace("已加入聊天室", "").Trim();
            string item = $"  🟢  {n}";
            if (!string.IsNullOrEmpty(n) && !lstUsers.Items.Contains(item))
            {
                lstUsers.Items.Add(item);
                if (n != _myUserName) { _othersOnline = true; UpgradeReceipt(); }
            }
        }
        else if (msg.Contains("已離開"))
        {
            string n = msg.Replace("已離開聊天室", "").Trim();
            lstUsers.Items.Remove($"  🟢  {n}");
            _othersOnline = lstUsers.Items.Count > 1;
        }
    }

    private void UpdateTitle() => Text = _unreadCount > 0 ? $"({_unreadCount}) MultiChat" : "MultiChat";

    // ═════════════════════════════════════════════════════════════════════════
    //  NOTIFICATION
    // ═════════════════════════════════════════════════════════════════════════

    private void NotifyNew(string sender, string preview)
    {
        _unreadCount++; UpdateTitle();
        if (chkSound.Checked) System.Media.SystemSounds.Asterisk.Play();
        if (!Visible || WindowState == FormWindowState.Minimized)
        {
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(4000, $"💬 {sender}", preview.Length > 80 ? preview[..80] + "…" : preview, ToolTipIcon.Info);
        }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  🔵 READ RECEIPT — ✓ 已送出 → ✓✓ 已讀
    // ═════════════════════════════════════════════════════════════════════════

    private void AddReceipt()
    {
        _receiptStart = rtbChat.TextLength;
        string mark = _othersOnline ? "  ✓✓ 已讀\n" : "  ✓ 已送出\n";
        Ink(mark, _othersOnline ? CGreen : CTime, 7.5f, CBg, HA.Right, italic: true);
        _receiptEnd = rtbChat.TextLength;
    }

    private void UpgradeReceipt()
    {
        _othersOnline = true;
        if (_receiptStart < 0 || _receiptStart >= rtbChat.TextLength) return;
        try
        {
            int len = _receiptEnd - _receiptStart;
            if (len <= 0) return;
            int saved = rtbChat.SelectionStart;
            rtbChat.Select(_receiptStart, len);
            rtbChat.SelectionColor     = CGreen;
            rtbChat.SelectionFont      = new Font("Segoe UI", 7.5f, FontStyle.Italic);
            rtbChat.SelectedText       = "  ✓✓ 已讀\n";
            rtbChat.SelectionStart     = saved;
            _receiptStart = -1;
        }
        catch { }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  AVATAR HELPER
    // ═════════════════════════════════════════════════════════════════════════

    private static Bitmap MakeAvatar(string name)
    {
        var bmp = new Bitmap(30, 30);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        using var brush = new SolidBrush(AvatarColor(name));
        g.FillEllipse(brush, 1, 1, 28, 28);
        string letter = name.Length > 0 ? name[0].ToString().ToUpper() : "?";
        using var font = new Font("Segoe UI", 13f, FontStyle.Bold);
        var sz = g.MeasureString(letter, font);
        g.DrawString(letter, font, Brushes.White, (30 - sz.Width) / 2f, (30 - sz.Height) / 2f);
        return bmp;
    }

    private void InsertAvatar(string name, HA align)
    {
        using var bmp = MakeAvatar(name);
        rtbChat.SelectionAlignment = align;
        var prev = Clipboard.GetDataObject();
        Clipboard.SetImage(bmp);
        rtbChat.SelectionStart  = rtbChat.TextLength;
        rtbChat.SelectionLength = 0;
        rtbChat.Paste();
        if (prev != null) try { Clipboard.SetDataObject(prev); } catch { }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CHAT RENDER — TEXT
    // ═════════════════════════════════════════════════════════════════════════

    private void AppendOwn(string text)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);

        // Avatar + name row
        Ink("  ", CTime, 4f, CBg, HA.Right);
        InsertAvatar(_myUserName, HA.Right);
        Ink($"  你  ", CName, _fontSize - 1.5f, CBg, HA.Right, bold: true);
        Ink($"{Now}\n", CTime, 7.5f, CBg, HA.Right);

        // Bubble — track position for recall
        _recallBubbleStart = rtbChat.TextLength;
        Ink($"  {text}  \n", Color.White, _fontSize, COwnBubble, HA.Right);
        _recallBubbleEnd = rtbChat.TextLength;

        // Read receipt
        AddReceipt();

        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void AppendOther(string sender, string text)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);

        // Avatar + name row
        InsertAvatar(sender, HA.Left);
        Ink($"  {sender}  ", CAccent, _fontSize - 1f, CBg, HA.Left, bold: true);
        Ink($"{Now}\n", CTime, 7.5f, CBg, HA.Left);

        // Bubble
        Ink($"  {text}  \n", CText, _fontSize, COtherBub, HA.Left);

        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void AppendSystem(string text)
    {
        rtbChat.SuspendLayout();
        Ink($"\n  ── {text} ──\n", CSys, 8.5f, CBg, HA.Center, italic: true);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CHAT RENDER — PRIVATE MESSAGE
    // ═════════════════════════════════════════════════════════════════════════

    private static readonly Color CPmBg  = Color.FromArgb(60,  30, 80);
    private static readonly Color CPmText = Color.FromArgb(220,180,255);

    private void AppendPmOwn(string recipient, string text)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        Ink($"  💬 私訊 → {recipient}  ", CPmText, _fontSize - 1.5f, CPmBg, HA.Right, bold: true);
        Ink($"{Now}\n", CTime, 7.5f, CBg, HA.Right);
        Ink($"  {text}  \n", Color.White, _fontSize, CPmBg, HA.Right);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void AppendPmOther(string sender, string text)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        InsertAvatar(sender, HA.Left);
        Ink($"  💬 私訊 {sender}  ", CPmText, _fontSize - 1f, CPmBg, HA.Left, bold: true);
        Ink($"{Now}\n", CTime, 7.5f, CBg, HA.Left);
        Ink($"  {text}  \n", CPmText, _fontSize, CPmBg, HA.Left);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CHAT RENDER — IMAGES
    // ═════════════════════════════════════════════════════════════════════════

    private void AppendOwnImage(byte[] bytes, string label)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        InsertAvatar(_myUserName, HA.Right);
        Ink($"  你  {Now}\n",       CName,       _fontSize - 2, CBg,        HA.Right, bold: true);
        Ink($"  🖼️  {label}\n",     CName,       _fontSize,     COwnBubble, HA.Right);
        InsertImageToChat(bytes, HA.Right);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void AppendOtherImage(string sender, byte[] bytes)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        InsertAvatar(sender, HA.Left);
        Ink($"  {sender}  {Now}\n",       CAccent, _fontSize - 2, CBg,      HA.Left, bold: true);
        Ink($"  🖼️  傳送了一張圖片\n",    CText,   _fontSize,     COtherBub, HA.Left);
        InsertImageToChat(bytes, HA.Left);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void InsertImageToChat(byte[] bytes, HA align)
    {
        try
        {
            using var ms    = new MemoryStream(bytes);
            using var img   = Image.FromStream(ms);
            using var thumb = ResizeImg(img, 240, 180);
            rtbChat.SelectionAlignment = align;
            var prev = Clipboard.GetDataObject();
            Clipboard.SetImage(thumb);
            rtbChat.SelectionStart = rtbChat.TextLength;
            rtbChat.Paste();
            if (prev != null) try { Clipboard.SetDataObject(prev); } catch { }
            Ink("\n", CTime, 4f, CBg, HA.Left);
        }
        catch { Ink("  [圖片無法顯示]\n", CTime, 9f, CBg, align); }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CHAT RENDER — FILES
    // ═════════════════════════════════════════════════════════════════════════

    private void AppendOwnFile(string fileName, byte[] bytes)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        InsertAvatar(_myUserName, HA.Right);
        Ink($"  你  {Now}\n",           CName,  _fontSize - 2, CBg,        HA.Right, bold: true);
        Ink($"  📎  {fileName}\n",       CName,  _fontSize,     COwnBubble, HA.Right);
        Ink($"  {FormatSize(bytes.Length)}\n", CTime, 8f, COwnBubble, HA.Right);
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    private void AppendOtherFile(string sender, string fileName, string b64)
    {
        rtbChat.SuspendLayout();
        Ink("\n", CTime, 5f, CBg, HA.Left);
        InsertAvatar(sender, HA.Left);
        Ink($"  {sender}  {Now}\n",       CAccent, _fontSize - 2, CBg,       HA.Left, bold: true);
        Ink($"  📎  {fileName}\n",         CText,   _fontSize,     COtherBub, HA.Left);
        try
        {
            byte[] data = Convert.FromBase64String(b64);
            string path = Path.Combine(_tempFolder, fileName);
            File.WriteAllBytes(path, data);
            Ink($"  💾  {FormatSize(data.Length)}  已儲存\n  {path}\n", Color.FromArgb(100, 180, 255), 8.5f, COtherBub, HA.Left, italic: true);
        }
        catch { Ink("  [檔案失敗]\n", CTime, 9f, COtherBub, HA.Left); }
        rtbChat.ResumeLayout();
        ScrollEnd();
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  CORE INK
    // ═════════════════════════════════════════════════════════════════════════

    private void Ink(string text, Color fg, float size, Color bg,
        HA align, bool bold = false, bool italic = false)
    {
        rtbChat.SelectionStart     = rtbChat.TextLength;
        rtbChat.SelectionLength    = 0;
        rtbChat.SelectionAlignment = align;
        FontStyle fs = FontStyle.Regular;
        if (bold)   fs |= FontStyle.Bold;
        if (italic) fs |= FontStyle.Italic;
        rtbChat.SelectionFont      = new Font("Segoe UI", size, fs);
        rtbChat.SelectionColor     = fg;
        rtbChat.SelectionBackColor = bg;
        rtbChat.AppendText(text);
    }

    private void ScrollEnd()
    {
        rtbChat.SelectionStart = rtbChat.TextLength;
        rtbChat.ScrollToCaret();
        if (ContainsFocus) { _unreadCount = 0; UpdateTitle(); }
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  FORM CLOSE
    // ═════════════════════════════════════════════════════════════════════════

    private void FrmClient_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _client.Disconnect();
        notifyIcon.Visible = false;
    }

    // ═════════════════════════════════════════════════════════════════════════
    //  UTILITIES
    // ═════════════════════════════════════════════════════════════════════════

    private static Image ResizeImg(Image img, int maxW, int maxH)
    {
        float s = Math.Min((float)maxW / img.Width, (float)maxH / img.Height);
        int w = (int)(img.Width * s), h = (int)(img.Height * s);
        var bmp = new Bitmap(w, h);
        using var g = Graphics.FromImage(bmp);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.DrawImage(img, 0, 0, w, h);
        return bmp;
    }

    private static string FormatSize(long b)
        => b >= 1_048_576 ? $"{b/1_048_576.0:F1} MB"
         : b >= 1_024     ? $"{b/1_024.0:F0} KB"
                           : $"{b} B";

    private static string Now        => DateTime.Now.ToString("HH:mm");
    private static string GenerateMsgId() => DateTime.Now.ToString("HHmmssff");

    private static void Tip(string msg)
        => MessageBox.Show(msg, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
}
