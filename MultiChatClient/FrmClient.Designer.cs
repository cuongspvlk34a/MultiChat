namespace MultiChatClient;

partial class FrmClient
{
    private System.ComponentModel.IContainer components = null;

    // Header
    private Panel  pnlHeader;
    private Label  lblAppTitle, lblStatusDot, lblConnectionStatus;

    // Connection bar
    private Panel   pnlConnection;
    private Label   lblServerIPLabel, lblPortLabel, lblUserNameLabel;
    private TextBox txtServerIP, txtPort, txtUserName;
    private Button  btnConnect, btnDisconnect;

    // Settings toolbar
    private Panel      pnlSettings;
    private ComboBox   cmbTheme;
    private Label      lblThemeLabel, lblFontLabel;
    private TrackBar   trkFontSize;
    private CheckBox   chkSound;
    private Button     btnSearch, btnExport, btnClearAll;

    // Split
    private SplitContainer splitMain;

    // Chat panel (Panel1)
    private Panel        pnlChatHeader;
    private Label        lblChatTitle;
    private Button       btnCall, btnVideoCall, btnClearChat;
    private RichTextBox  rtbChat;
    private Label        lblTyping;
    private ContextMenuStrip chatMenu;

    // Users panel (Panel2)
    private Label   lblUsersTitle;
    private Label   lblUsersHint;
    private ListBox lstUsers;

    // Toolbar
    private Panel  pnlToolbar;
    private Button btnEmoji, btnSendImage, btnSendFile, btnCapture, btnVoiceMsg, btnRecall;

    // Input row
    private Panel   pnlInput;
    private TextBox txtMessage;
    private Label   lblCharCount;
    private Button  btnSend;

    // Tray
    private NotifyIcon       notifyIcon;
    private ContextMenuStrip trayMenu;

    protected override void Dispose(bool disposing)
    {
        if (disposing) { components?.Dispose(); notifyIcon?.Dispose(); }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        var T      = ChatTheme.Dark;
        var fUi    = new Font("Segoe UI", 9.5f);
        var fBold  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
        var fSmall = new Font("Segoe UI", 8.5f);
        var fEmoji = new Font("Segoe UI Emoji", 13f);

        Button IBtn(string ico, string tip, Color bg, EventHandler? handler = null)
        {
            var b = new Button { Text = ico, Font = fEmoji, BackColor = bg, ForeColor = T.Text, FlatStyle = FlatStyle.Flat, Size = new Size(44, 36), Cursor = Cursors.Hand, TabStop = false };
            b.FlatAppearance.BorderSize = 0;
            new ToolTip().SetToolTip(b, tip);
            if (handler != null) b.Click += handler;
            return b;
        }

        // ── HEADER ────────────────────────────────────────────────────────────
        pnlHeader           = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = T.Header };
        lblAppTitle         = new Label { Text = "💬  MultiChat", Font = new Font("Segoe UI", 15f, FontStyle.Bold), ForeColor = Color.White, AutoSize = true, Location = new Point(14, 12) };
        lblStatusDot        = new Label { Text = "●", Font = new Font("Segoe UI", 11f), ForeColor = T.Danger, AutoSize = true, Location = new Point(208, 16) };
        lblConnectionStatus = new Label { Text = "未連線", Font = fSmall, ForeColor = T.Muted, AutoSize = true, Location = new Point(226, 19) };
        pnlHeader.Controls.AddRange(new Control[] { lblAppTitle, lblStatusDot, lblConnectionStatus });

        // ── CONNECTION BAR ────────────────────────────────────────────────────
        pnlConnection = new Panel { Dock = DockStyle.Top, Height = 46, BackColor = T.Panel };
        void Lbl(Label l, string t2, Point p) { l.Text = t2; l.Font = fSmall; l.ForeColor = T.Muted; l.AutoSize = true; l.Location = p; }
        void Tbx(TextBox t2, string d, Point p, int w) { t2.Text = d; t2.Font = fUi; t2.BackColor = T.Input; t2.ForeColor = T.Text; t2.BorderStyle = BorderStyle.FixedSingle; t2.Location = p; t2.Size = new Size(w, 26); }

        lblServerIPLabel = new Label(); lblPortLabel = new Label(); lblUserNameLabel = new Label();
        txtServerIP = new TextBox(); txtPort = new TextBox(); txtUserName = new TextBox();
        Lbl(lblServerIPLabel, "Server IP", new Point(12, 13)); Tbx(txtServerIP, "127.0.0.1", new Point(72,  9), 118);
        Lbl(lblPortLabel,     "Port",      new Point(200, 13)); Tbx(txtPort,     "5000",      new Point(232, 9), 66);
        Lbl(lblUserNameLabel, "暱稱",       new Point(308, 13)); Tbx(txtUserName, "",          new Point(338, 9), 118);

        btnConnect    = new Button { Text = "連線", Font = fBold, BackColor = T.Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(468, 8), Size = new Size(72, 28), Cursor = Cursors.Hand };
        btnDisconnect = new Button { Text = "離線", Font = fUi,   BackColor = T.Danger, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(548, 8), Size = new Size(72, 28), Cursor = Cursors.Hand, Enabled = false };
        btnConnect.FlatAppearance.BorderSize = 0; btnDisconnect.FlatAppearance.BorderSize = 0;
        btnConnect.Click += btnConnect_Click; btnDisconnect.Click += btnDisconnect_Click;
        pnlConnection.Controls.AddRange(new Control[] { lblServerIPLabel, txtServerIP, lblPortLabel, txtPort, lblUserNameLabel, txtUserName, btnConnect, btnDisconnect });

        // ── SETTINGS TOOLBAR ──────────────────────────────────────────────────
        pnlSettings = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = T.Toolbar };
        lblThemeLabel = new Label { Text = "🎨 Theme:", Font = fSmall, ForeColor = T.Muted, AutoSize = true, Location = new Point(8, 12) };
        cmbTheme = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = fSmall, BackColor = T.Input, ForeColor = T.Text, Location = new Point(76, 8), Size = new Size(118, 24) };
        foreach (var th in ChatTheme.All) cmbTheme.Items.Add(th.Name);
        cmbTheme.SelectedIndex = 0;
        cmbTheme.SelectedIndexChanged += cmbTheme_Changed;

        lblFontLabel = new Label { Text = "🔠 Size:", Font = fSmall, ForeColor = T.Muted, AutoSize = true, Location = new Point(204, 12) };
        trkFontSize  = new TrackBar { Minimum = 8, Maximum = 16, Value = 10, SmallChange = 1, TickFrequency = 2, Location = new Point(254, 8), Size = new Size(88, 26), BackColor = T.Toolbar };
        trkFontSize.ValueChanged += trkFontSize_Changed;

        chkSound = new CheckBox { Text = "🔔 音效", Font = fSmall, ForeColor = T.Text, Checked = true, Location = new Point(348, 12), AutoSize = true, BackColor = T.Toolbar };

        btnSearch  = new Button { Text = "🔍 搜尋", Font = fSmall, BackColor = T.Input, ForeColor = T.Text, FlatStyle = FlatStyle.Flat, Location = new Point(432, 8), Size = new Size(70, 26), Cursor = Cursors.Hand };
        btnExport  = new Button { Text = "💾 匯出", Font = fSmall, BackColor = T.Input, ForeColor = T.Text, FlatStyle = FlatStyle.Flat, Location = new Point(508, 8), Size = new Size(70, 26), Cursor = Cursors.Hand };
        btnClearAll= new Button { Text = "🗑 清除", Font = fSmall, BackColor = T.Danger, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(584, 8), Size = new Size(70, 26), Cursor = Cursors.Hand };
        btnSearch.FlatAppearance.BorderSize = btnExport.FlatAppearance.BorderSize = btnClearAll.FlatAppearance.BorderSize = 0;
        btnSearch.Click += btnSearch_Click; btnExport.Click += btnExport_Click; btnClearAll.Click += btnClearAll_Click;
        pnlSettings.Controls.AddRange(new Control[] { lblThemeLabel, cmbTheme, lblFontLabel, trkFontSize, chkSound, btnSearch, btnExport, btnClearAll });

        // ── TOOLBAR (media) ────────────────────────────────────────────────────
        pnlToolbar = new Panel { Dock = DockStyle.Bottom, Height = 42, BackColor = T.Toolbar };
        btnEmoji     = IBtn("😊", "Emoji",                    T.Toolbar, btnEmoji_Click);
        btnSendImage = IBtn("🖼️", "Gửi hình ảnh",             T.Toolbar, btnSendImage_Click);
        btnSendFile  = IBtn("📎", "Gửi tệp",                  T.Toolbar, btnSendFile_Click);
        btnCapture   = IBtn("📷", "Chụp màn hình",             T.Toolbar, btnCapture_Click);
        btnVoiceMsg  = IBtn("🎤", "Giọng nói",                T.Toolbar, btnVoiceMsg_Click);
        btnRecall    = IBtn("↩️", "↩️撤回最後一則訊息",        T.Toolbar, btnRecall_Click);
        btnEmoji.Location = new Point(4, 3); btnSendImage.Location = new Point(50, 3);
        btnSendFile.Location = new Point(96, 3); btnCapture.Location = new Point(142, 3);
        btnVoiceMsg.Location = new Point(188, 3); btnRecall.Location = new Point(234, 3);
        pnlToolbar.Controls.AddRange(new Control[] { btnEmoji, btnSendImage, btnSendFile, btnCapture, btnVoiceMsg, btnRecall });

        // ── INPUT ROW ─────────────────────────────────────────────────────────
        pnlInput = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = T.Panel };
        txtMessage = new TextBox { Font = new Font("Segoe UI", 11f), BackColor = T.Input, ForeColor = T.Text, BorderStyle = BorderStyle.FixedSingle, Enabled = false, Location = new Point(8, 11), Size = new Size(100, 28) };
        txtMessage.KeyDown += txtMessage_KeyDown; txtMessage.TextChanged += txtMessage_TextChanged;
        lblCharCount = new Label { Text = "0/500", Font = fSmall, ForeColor = T.Muted, TextAlign = ContentAlignment.MiddleCenter, BackColor = T.Panel, Size = new Size(52, 28) };
        btnSend = new Button { Text = "送出 ▶", Font = fBold, BackColor = T.Accent, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Size = new Size(88, 28), Cursor = Cursors.Hand, Enabled = false };
        btnSend.FlatAppearance.BorderSize = 0; btnSend.Click += btnSend_Click;
        pnlInput.Controls.AddRange(new Control[] { txtMessage, lblCharCount, btnSend });
        pnlInput.Resize += (_, _) =>
        {
            int r = pnlInput.Width - 8;
            btnSend.Location      = new Point(r - 88,  11);
            lblCharCount.Location = new Point(r - 144, 11);
            txtMessage.Width      = lblCharCount.Left - 16;
        };

        // ── SPLIT ─────────────────────────────────────────────────────────────
        splitMain = new SplitContainer { Dock = DockStyle.Fill, BackColor = T.Background, BorderStyle = BorderStyle.None, SplitterWidth = 2, SplitterDistance = 760 };
        splitMain.Panel1.BackColor = T.Background; splitMain.Panel2.BackColor = T.Panel;
        ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
        splitMain.Panel1.SuspendLayout(); splitMain.Panel2.SuspendLayout();

        // Chat area
        pnlChatHeader = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = T.Header };
        btnClearChat = IBtn("🗑",  "Xoá chat",    Color.Transparent, btnClearChat_Click); btnClearChat.Dock = DockStyle.Right;
        btnVideoCall = IBtn("🎥", "Gọi video",    Color.Transparent, btnVideoCall_Click); btnVideoCall.Dock = DockStyle.Right;
        btnCall      = IBtn("📞", "Gọi thoại",    Color.Transparent, btnCall_Click);      btnCall.Dock      = DockStyle.Right;
        lblChatTitle = new Label { Text = "  💬  Phòng chat", Font = fBold, ForeColor = T.Text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, BackColor = T.Header };
        pnlChatHeader.Controls.AddRange(new Control[] { btnClearChat, btnVideoCall, btnCall, lblChatTitle });

        lblTyping = new Label { Text = "", Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = T.Muted, BackColor = T.Background, Dock = DockStyle.Bottom, Height = 18, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(12, 0, 0, 0) };

        // Context menu for recall + copy
        chatMenu = new ContextMenuStrip { BackColor = T.Panel, ForeColor = T.Text, Font = fUi };
        var mnuRecallItem = new ToolStripMenuItem("↩️  撤回最後一則訊息") { BackColor = T.Panel, ForeColor = T.Text };
        mnuRecallItem.Click += (_, _) => RecallLastMessage();
        var mnuCopyItem = new ToolStripMenuItem("📋  複製選取文字") { BackColor = T.Panel, ForeColor = T.Text };
        mnuCopyItem.Click += (_, _) => { if (rtbChat.SelectionLength > 0) Clipboard.SetText(rtbChat.SelectedText); };
        chatMenu.Items.AddRange(new ToolStripItem[] { mnuRecallItem, new ToolStripSeparator(), mnuCopyItem });

        rtbChat = new RichTextBox { Dock = DockStyle.Fill, BackColor = T.Background, ForeColor = T.Text, Font = new Font("Segoe UI", 10.5f), BorderStyle = BorderStyle.None, ReadOnly = true, ScrollBars = RichTextBoxScrollBars.Vertical, Padding = new Padding(10, 4, 10, 4), DetectUrls = true, ContextMenuStrip = chatMenu };
        rtbChat.LinkClicked += (_, e) => { try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.LinkText!) { UseShellExecute = true }); } catch { } };

        splitMain.Panel1.Controls.Add(rtbChat);
        splitMain.Panel1.Controls.Add(lblTyping);
        splitMain.Panel1.Controls.Add(pnlChatHeader);

        // Users panel — double-click for PM
        lblUsersTitle = new Label { Text = "  👥  線上用戶", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = T.Text, BackColor = T.Header, Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleLeft };
        lblUsersHint  = new Label { Text = "  👆 雙擊發送私訊", Font = new Font("Segoe UI", 8f, FontStyle.Italic), ForeColor = T.Muted, BackColor = T.Panel, Dock = DockStyle.Bottom, Height = 22, TextAlign = ContentAlignment.MiddleLeft };
        lstUsers      = new ListBox { BackColor = T.Panel, ForeColor = T.Text, BorderStyle = BorderStyle.None, Font = fUi, ItemHeight = 34, Dock = DockStyle.Fill };
        lstUsers.DoubleClick += lstUsers_DoubleClick;

        splitMain.Panel2.Controls.Add(lstUsers);
        splitMain.Panel2.Controls.Add(lblUsersHint);
        splitMain.Panel2.Controls.Add(lblUsersTitle);
        ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
        splitMain.Panel1.ResumeLayout(); splitMain.Panel2.ResumeLayout();

        // ── TRAY ──────────────────────────────────────────────────────────────
        trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("開啟 MultiChat", null, (_, _) => { Show(); WindowState = FormWindowState.Normal; Activate(); });
        trayMenu.Items.Add("離開",            null, (_, _) => { notifyIcon.Visible = false; Application.Exit(); });
        notifyIcon = new NotifyIcon { Text = "MultiChat", Icon = SystemIcons.Application, ContextMenuStrip = trayMenu, Visible = false };
        notifyIcon.DoubleClick += (_, _) => { Show(); WindowState = FormWindowState.Normal; Activate(); };

        // ── FORM ──────────────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(1020, 720);
        MinimumSize         = new Size(860, 560);
        BackColor           = T.Background;
        Text                = "MultiChat";
        StartPosition       = FormStartPosition.CenterScreen;

        Controls.Add(splitMain);
        Controls.Add(pnlInput);
        Controls.Add(pnlToolbar);
        Controls.Add(pnlSettings);
        Controls.Add(pnlConnection);
        Controls.Add(pnlHeader);

        FormClosing += FrmClient_FormClosing;
        Resize      += FrmClient_Resize;
        Load        += FrmClient_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private Font _chatFont = new("Segoe UI", 10.5f);
}
