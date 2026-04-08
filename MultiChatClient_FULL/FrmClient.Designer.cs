namespace MultiChatClient;

partial class FrmClient
{
    private System.ComponentModel.IContainer components = null;

    // ── Existing controls ──────────────────────────────────────────
    private Label      lblServerIP;
    private TextBox    txtServerIP;
    private Label      lblPort;
    private TextBox    txtPort;
    private Label      lblUserName;
    private TextBox    txtUserName;
    private Button     btnConnect;
    private Button     btnDisconnect;
    private Label      lblStatusTitle;
    private Label      lblStatus;
    private Label      lblChat;
    private ListBox    lstChat;
    private TextBox    txtMessage;
    private Button     btnSend;
    private Button     btnEmoji;
    private Button     btnSendImage;
    private Label      lblTheme;
    private ComboBox   cmbTheme;

    // ── Toolbar controls ───────────────────────────────────────────
    private Label      lblFontSize;
    private TrackBar   trkFontSize;
    private Label      lblFontSizeVal;
    private CheckBox   chkSound;
    private Label      lblSearch;
    private TextBox    txtSearch;
    private Button     btnSearch;

    // ── Feature controls ───────────────────────────────────────────
    private Button     btnExportChat;
    private Button     btnVoice;
    private Panel      pnlEmojiPopup;

    // ── NEW: Online users panel (for Private Message) ──────────────
    private Panel      pnlUsers;
    private Label      lblUsersTitle;
    private ListBox    lstUsers;
    private Label      lblPMTarget;
    private Label      lblPMTargetName;
    private Button     btnClearPM;
    // ── NEW: File send + Load chat ────────────────────────────────
    private Button     btnSendFile;
    private Button     btnLoadChat;
    // ── NEW: Screenshot + Clear chat ─────────────────────────────
    private Button     btnScreenshot;
    private Button     btnClearChat;
    // ── NEW: Feature 1/2/3 controls ──────────────────────────────
    private Label      lblTyping;        // Feature 2: typing indicator
    private Panel      pnlReplyPreview; // Feature 3: reply preview strip
    private Label      lblReplyPreview; // Feature 3: preview text inside panel
    private Button     btnCancelReply;  // Feature 3: cancel reply button

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblServerIP = new Label();
        txtServerIP = new TextBox();
        lblPort = new Label();
        txtPort = new TextBox();
        lblUserName = new Label();
        txtUserName = new TextBox();
        btnConnect = new Button();
        btnDisconnect = new Button();
        lblStatusTitle = new Label();
        lblStatus = new Label();
        lblChat = new Label();
        lstChat = new ListBox();
        txtMessage = new TextBox();
        btnSend = new Button();
        btnEmoji = new Button();
        btnSendImage = new Button();
        lblTheme = new Label();
        cmbTheme = new ComboBox();
        lblFontSize = new Label();
        trkFontSize = new TrackBar();
        lblFontSizeVal = new Label();
        chkSound = new CheckBox();
        lblSearch = new Label();
        txtSearch = new TextBox();
        btnSearch = new Button();
        btnExportChat = new Button();
        btnVoice = new Button();
        pnlEmojiPopup = new Panel();
        pnlUsers = new Panel();
        lblUsersTitle = new Label();
        lstUsers = new ListBox();
        lblPMTarget = new Label();
        lblPMTargetName = new Label();
        btnClearPM = new Button();
        btnSendFile = new Button();
        btnLoadChat = new Button();
        btnScreenshot = new Button();
        btnClearChat = new Button();
        lblTyping = new Label();
        pnlReplyPreview = new Panel();
        lblReplyPreview = new Label();
        btnCancelReply = new Button();
        ((System.ComponentModel.ISupportInitialize)trkFontSize).BeginInit();
        pnlUsers.SuspendLayout();
        pnlReplyPreview.SuspendLayout();
        SuspendLayout();
        // 
        // lblServerIP
        // 
        lblServerIP.AutoSize = true;
        lblServerIP.Location = new Point(20, 22);
        lblServerIP.Name = "lblServerIP";
        lblServerIP.Size = new Size(52, 15);
        lblServerIP.TabIndex = 0;
        lblServerIP.Text = "Server IP";
        // 
        // txtServerIP
        // 
        txtServerIP.Location = new Point(82, 18);
        txtServerIP.Name = "txtServerIP";
        txtServerIP.Size = new Size(150, 23);
        txtServerIP.TabIndex = 1;
        txtServerIP.Text = "127.0.0.1";
        txtServerIP.TextChanged += txtServerIP_TextChanged;
        // 
        // lblPort
        // 
        lblPort.AutoSize = true;
        lblPort.Location = new Point(248, 22);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(29, 15);
        lblPort.TabIndex = 2;
        lblPort.Text = "Port";
        // 
        // txtPort
        // 
        txtPort.Location = new Point(283, 18);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(80, 23);
        txtPort.TabIndex = 3;
        txtPort.Text = "5000";
        // 
        // lblUserName
        // 
        lblUserName.AutoSize = true;
        lblUserName.Location = new Point(380, 22);
        lblUserName.Name = "lblUserName";
        lblUserName.Size = new Size(31, 15);
        lblUserName.TabIndex = 4;
        lblUserName.Text = "暱稱";
        // 
        // txtUserName
        // 
        txtUserName.Location = new Point(418, 18);
        txtUserName.Name = "txtUserName";
        txtUserName.Size = new Size(140, 23);
        txtUserName.TabIndex = 5;
        txtUserName.Text = "User1";
        // 
        // btnConnect
        // 
        btnConnect.Location = new Point(580, 15);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(90, 30);
        btnConnect.TabIndex = 6;
        btnConnect.Text = "連線";
        btnConnect.UseVisualStyleBackColor = true;
        btnConnect.Click += btnConnect_Click;
        // 
        // btnDisconnect
        // 
        btnDisconnect.Location = new Point(680, 15);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(90, 30);
        btnDisconnect.TabIndex = 7;
        btnDisconnect.Text = "離線";
        btnDisconnect.UseVisualStyleBackColor = true;
        btnDisconnect.Click += btnDisconnect_Click;
        // 
        // lblStatusTitle
        // 
        lblStatusTitle.AutoSize = true;
        lblStatusTitle.Location = new Point(20, 58);
        lblStatusTitle.Name = "lblStatusTitle";
        lblStatusTitle.Size = new Size(37, 15);
        lblStatusTitle.TabIndex = 8;
        lblStatusTitle.Text = "狀態：";
        // 
        // lblStatus
        // 
        lblStatus.AutoSize = true;
        lblStatus.Location = new Point(69, 58);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(43, 15);
        lblStatus.TabIndex = 9;
        lblStatus.Text = "未連線";
        // 
        // lblChat
        // 
        lblChat.AutoSize = true;
        lblChat.Location = new Point(20, 126);
        lblChat.Name = "lblChat";
        lblChat.Size = new Size(56, 15);
        lblChat.TabIndex = 21;
        lblChat.Text = "聊天內容";
        // 
        // lstChat
        // 
        lstChat.DrawMode = DrawMode.OwnerDrawVariable;
        lstChat.FormattingEnabled = true;
        lstChat.ItemHeight = 15;
        lstChat.Location = new Point(20, 144);
        lstChat.Name = "lstChat";
        lstChat.Size = new Size(920, 355);
        lstChat.TabIndex = 22;
        lstChat.DrawItem += lstChat_DrawItem;
        lstChat.MeasureItem += lstChat_MeasureItem;
        lstChat.MouseDoubleClick += lstChat_MouseDoubleClick;
        lstChat.MouseDown += lstChat_MouseDown;
        // 
        // txtMessage
        // 
        txtMessage.Location = new Point(20, 530);
        txtMessage.Name = "txtMessage";
        txtMessage.Size = new Size(410, 23);
        txtMessage.TabIndex = 23;
        txtMessage.TextChanged += txtMessage_TextChanged;
        txtMessage.KeyDown += txtMessage_KeyDown;
        // 
        // btnSend
        // 
        btnSend.Location = new Point(849, 526);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(80, 30);
        btnSend.TabIndex = 28;
        btnSend.Text = "送出";
        btnSend.UseVisualStyleBackColor = true;
        btnSend.Click += btnSend_Click;
        // 
        // btnEmoji
        // 
        btnEmoji.Location = new Point(484, 526);
        btnEmoji.Name = "btnEmoji";
        btnEmoji.Size = new Size(80, 30);
        btnEmoji.TabIndex = 25;
        btnEmoji.Text = "表情";
        btnEmoji.UseVisualStyleBackColor = true;
        btnEmoji.Click += btnEmoji_Click;
        // 
        // btnSendImage
        // 
        btnSendImage.Location = new Point(758, 526);
        btnSendImage.Name = "btnSendImage";
        btnSendImage.Size = new Size(85, 30);
        btnSendImage.TabIndex = 27;
        btnSendImage.Text = "傳圖片";
        btnSendImage.UseVisualStyleBackColor = true;
        btnSendImage.Click += btnSendImage_Click;
        // 
        // lblTheme
        // 
        lblTheme.AutoSize = true;
        lblTheme.Location = new Point(460, 58);
        lblTheme.Name = "lblTheme";
        lblTheme.Size = new Size(52, 15);
        lblTheme.TabIndex = 10;
        lblTheme.Text = "🎨 主題：";
        // 
        // cmbTheme
        // 
        cmbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTheme.FormattingEnabled = true;
        cmbTheme.Location = new Point(530, 54);
        cmbTheme.Name = "cmbTheme";
        cmbTheme.Size = new Size(240, 23);
        cmbTheme.TabIndex = 11;
        cmbTheme.SelectedIndexChanged += cmbTheme_SelectedIndexChanged;
        // 
        // lblFontSize
        // 
        lblFontSize.AutoSize = true;
        lblFontSize.Location = new Point(20, 93);
        lblFontSize.Name = "lblFontSize";
        lblFontSize.Size = new Size(49, 15);
        lblFontSize.TabIndex = 12;
        lblFontSize.Text = "🔤 字體:";
        // 
        // trkFontSize
        // 
        trkFontSize.Location = new Point(78, 84);
        trkFontSize.Maximum = 16;
        trkFontSize.Minimum = 8;
        trkFontSize.Name = "trkFontSize";
        trkFontSize.Size = new Size(110, 45);
        trkFontSize.TabIndex = 13;
        trkFontSize.TickFrequency = 2;
        trkFontSize.Value = 9;
        trkFontSize.Scroll += trkFontSize_Scroll;
        // 
        // lblFontSizeVal
        // 
        lblFontSizeVal.Location = new Point(193, 93);
        lblFontSizeVal.Name = "lblFontSizeVal";
        lblFontSizeVal.Size = new Size(34, 15);
        lblFontSizeVal.TabIndex = 14;
        lblFontSizeVal.Text = "9pt";
        // 
        // chkSound
        // 
        chkSound.AutoSize = true;
        chkSound.Checked = true;
        chkSound.CheckState = CheckState.Checked;
        chkSound.Location = new Point(235, 91);
        chkSound.Name = "chkSound";
        chkSound.Size = new Size(89, 19);
        chkSound.TabIndex = 15;
        chkSound.Text = "🔔 通知音效";
        chkSound.UseVisualStyleBackColor = true;
        chkSound.CheckedChanged += chkSound_CheckedChanged;
        // 
        // lblSearch
        // 
        lblSearch.AutoSize = true;
        lblSearch.Location = new Point(392, 93);
        lblSearch.Name = "lblSearch";
        lblSearch.Size = new Size(49, 15);
        lblSearch.TabIndex = 16;
        lblSearch.Text = "🔍 搜尋:";
        // 
        // txtSearch
        // 
        txtSearch.Location = new Point(450, 89);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new Size(196, 23);
        txtSearch.TabIndex = 17;
        txtSearch.KeyDown += txtSearch_KeyDown;
        // 
        // btnSearch
        // 
        btnSearch.Location = new Point(652, 87);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(78, 27);
        btnSearch.TabIndex = 18;
        btnSearch.Text = "搜尋 ↑↓";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        // 
        // btnExportChat
        // 
        btnExportChat.Location = new Point(748, 87);
        btnExportChat.Name = "btnExportChat";
        btnExportChat.Size = new Size(90, 27);
        btnExportChat.TabIndex = 19;
        btnExportChat.Text = "📥 匯出";
        btnExportChat.UseVisualStyleBackColor = true;
        btnExportChat.Click += btnExportChat_Click;
        // 
        // btnVoice
        // 
        btnVoice.Location = new Point(436, 526);
        btnVoice.Name = "btnVoice";
        btnVoice.Size = new Size(42, 30);
        btnVoice.TabIndex = 24;
        btnVoice.Text = "🎤";
        btnVoice.UseVisualStyleBackColor = true;
        btnVoice.Click += btnVoice_Click;
        // 
        // pnlEmojiPopup
        // 
        pnlEmojiPopup.AutoScroll = true;
        pnlEmojiPopup.BorderStyle = BorderStyle.FixedSingle;
        pnlEmojiPopup.Location = new Point(0, 0);
        pnlEmojiPopup.Name = "pnlEmojiPopup";
        pnlEmojiPopup.Size = new Size(390, 265);
        pnlEmojiPopup.TabIndex = 31;
        pnlEmojiPopup.Visible = false;
        // 
        // pnlUsers
        // 
        pnlUsers.BorderStyle = BorderStyle.FixedSingle;
        pnlUsers.Controls.Add(lblUsersTitle);
        pnlUsers.Controls.Add(lstUsers);
        pnlUsers.Controls.Add(lblPMTarget);
        pnlUsers.Controls.Add(lblPMTargetName);
        pnlUsers.Controls.Add(btnClearPM);
        pnlUsers.Location = new Point(950, 144);
        pnlUsers.Name = "pnlUsers";
        pnlUsers.Size = new Size(175, 410);
        pnlUsers.TabIndex = 32;
        // 
        // lblUsersTitle
        // 
        lblUsersTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblUsersTitle.Location = new Point(0, 0);
        lblUsersTitle.Name = "lblUsersTitle";
        lblUsersTitle.Size = new Size(173, 26);
        lblUsersTitle.TabIndex = 0;
        lblUsersTitle.Text = "👥 在線用戶";
        lblUsersTitle.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // lstUsers
        // 
        lstUsers.BorderStyle = BorderStyle.None;
        lstUsers.ItemHeight = 15;
        lstUsers.Location = new Point(2, 28);
        lstUsers.Name = "lstUsers";
        lstUsers.Size = new Size(169, 255);
        lstUsers.TabIndex = 1;
        lstUsers.SelectedIndexChanged += lstUsers_SelectedIndexChanged;
        // 
        // lblPMTarget
        // 
        lblPMTarget.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        lblPMTarget.Location = new Point(2, 295);
        lblPMTarget.Name = "lblPMTarget";
        lblPMTarget.Size = new Size(169, 18);
        lblPMTarget.TabIndex = 2;
        lblPMTarget.Text = "私訊對象：";
        // 
        // lblPMTargetName
        // 
        lblPMTargetName.Font = new Font("Segoe UI Emoji", 9F);
        lblPMTargetName.ForeColor = Color.Gray;
        lblPMTargetName.Location = new Point(2, 314);
        lblPMTargetName.Name = "lblPMTargetName";
        lblPMTargetName.Size = new Size(169, 24);
        lblPMTargetName.TabIndex = 3;
        lblPMTargetName.Text = "(點擊用戶選擇)";
        lblPMTargetName.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // btnClearPM
        // 
        btnClearPM.FlatStyle = FlatStyle.Flat;
        btnClearPM.Location = new Point(37, 342);
        btnClearPM.Name = "btnClearPM";
        btnClearPM.Size = new Size(100, 26);
        btnClearPM.TabIndex = 4;
        btnClearPM.Text = "❌ 取消私訊";
        btnClearPM.UseVisualStyleBackColor = true;
        btnClearPM.Click += btnClearPM_Click;
        // 
        // btnSendFile
        // 
        btnSendFile.Enabled = false;
        btnSendFile.FlatStyle = FlatStyle.Flat;
        btnSendFile.Location = new Point(664, 526);
        btnSendFile.Name = "btnSendFile";
        btnSendFile.Size = new Size(88, 30);
        btnSendFile.TabIndex = 26;
        btnSendFile.Text = "📁 傳檔案";
        btnSendFile.Click += btnSendFile_Click;
        // 
        // btnLoadChat
        // 
        btnLoadChat.FlatStyle = FlatStyle.Flat;
        btnLoadChat.Location = new Point(848, 87);
        btnLoadChat.Name = "btnLoadChat";
        btnLoadChat.Size = new Size(90, 27);
        btnLoadChat.TabIndex = 20;
        btnLoadChat.Text = "📂 載入";
        btnLoadChat.Click += btnLoadChat_Click;
        // 
        // btnScreenshot
        // 
        btnScreenshot.Enabled = false;
        btnScreenshot.FlatStyle = FlatStyle.Flat;
        btnScreenshot.Location = new Point(570, 526);
        btnScreenshot.Name = "btnScreenshot";
        btnScreenshot.Size = new Size(88, 30);
        btnScreenshot.TabIndex = 29;
        btnScreenshot.Text = "📸 截圖";
        btnScreenshot.Click += btnScreenshot_Click;
        // 
        // btnClearChat
        // 
        btnClearChat.FlatStyle = FlatStyle.Flat;
        btnClearChat.Location = new Point(948, 87);
        btnClearChat.Name = "btnClearChat";
        btnClearChat.Size = new Size(90, 27);
        btnClearChat.TabIndex = 30;
        btnClearChat.Text = "🗑 清除";
        btnClearChat.Click += btnClearChat_Click;
        // 
        // lblTyping
        // 
        lblTyping.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        lblTyping.BackColor = Color.Transparent;
        lblTyping.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
        lblTyping.ForeColor = Color.Gray;
        lblTyping.Location = new Point(20, 481);
        lblTyping.Name = "lblTyping";
        lblTyping.Size = new Size(500, 16);
        lblTyping.TabIndex = 33;
        lblTyping.Visible = false;
        // 
        // pnlReplyPreview
        // 
        pnlReplyPreview.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        pnlReplyPreview.BackColor = Color.FromArgb(220, 235, 255);
        pnlReplyPreview.BorderStyle = BorderStyle.FixedSingle;
        pnlReplyPreview.Controls.Add(lblReplyPreview);
        pnlReplyPreview.Controls.Add(btnCancelReply);
        pnlReplyPreview.Location = new Point(20, 496);
        pnlReplyPreview.Name = "pnlReplyPreview";
        pnlReplyPreview.Size = new Size(916, 28);
        pnlReplyPreview.TabIndex = 34;
        pnlReplyPreview.Visible = false;
        // 
        // lblReplyPreview
        // 
        lblReplyPreview.BackColor = Color.Transparent;
        lblReplyPreview.Dock = DockStyle.Fill;
        lblReplyPreview.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
        lblReplyPreview.ForeColor = Color.FromArgb(30, 80, 160);
        lblReplyPreview.Location = new Point(0, 0);
        lblReplyPreview.Name = "lblReplyPreview";
        lblReplyPreview.Padding = new Padding(6, 0, 0, 0);
        lblReplyPreview.Size = new Size(884, 26);
        lblReplyPreview.TabIndex = 0;
        lblReplyPreview.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnCancelReply
        // 
        btnCancelReply.BackColor = Color.Transparent;
        btnCancelReply.Cursor = Cursors.Hand;
        btnCancelReply.Dock = DockStyle.Right;
        btnCancelReply.FlatAppearance.BorderSize = 0;
        btnCancelReply.FlatStyle = FlatStyle.Flat;
        btnCancelReply.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnCancelReply.ForeColor = Color.FromArgb(80, 80, 80);
        btnCancelReply.Location = new Point(884, 0);
        btnCancelReply.Name = "btnCancelReply";
        btnCancelReply.Size = new Size(30, 26);
        btnCancelReply.TabIndex = 1;
        btnCancelReply.TabStop = false;
        btnCancelReply.Text = "✕";
        btnCancelReply.UseVisualStyleBackColor = false;
        btnCancelReply.Click += btnCancelReply_Click;
        // 
        // FrmClient
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1140, 580);
        Controls.Add(lblServerIP);
        Controls.Add(txtServerIP);
        Controls.Add(lblPort);
        Controls.Add(txtPort);
        Controls.Add(lblUserName);
        Controls.Add(txtUserName);
        Controls.Add(btnConnect);
        Controls.Add(btnDisconnect);
        Controls.Add(lblStatusTitle);
        Controls.Add(lblStatus);
        Controls.Add(lblTheme);
        Controls.Add(cmbTheme);
        Controls.Add(lblFontSize);
        Controls.Add(trkFontSize);
        Controls.Add(lblFontSizeVal);
        Controls.Add(chkSound);
        Controls.Add(lblSearch);
        Controls.Add(txtSearch);
        Controls.Add(btnSearch);
        Controls.Add(btnExportChat);
        Controls.Add(btnLoadChat);
        Controls.Add(lblChat);
        Controls.Add(lstChat);
        Controls.Add(txtMessage);
        Controls.Add(btnVoice);
        Controls.Add(btnEmoji);
        Controls.Add(btnSendFile);
        Controls.Add(btnSendImage);
        Controls.Add(btnSend);
        Controls.Add(btnScreenshot);
        Controls.Add(btnClearChat);
        Controls.Add(pnlEmojiPopup);
        Controls.Add(pnlUsers);
        Controls.Add(pnlReplyPreview);
        Controls.Add(lblTyping);
        Name = "FrmClient";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MultiChat";
        FormClosing += FrmClient_FormClosing;
        Load += FrmClient_Load;
        ((System.ComponentModel.ISupportInitialize)trkFontSize).EndInit();
        pnlUsers.ResumeLayout(false);
        pnlReplyPreview.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }
}
