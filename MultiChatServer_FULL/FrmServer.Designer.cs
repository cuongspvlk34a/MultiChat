namespace MultiChatServer;

partial class FrmServer
{
    private System.ComponentModel.IContainer components = null;
    private Label lblIP;
    private TextBox txtIP;
    private Label lblPort;
    private TextBox txtPort;
    private Button btnStart;
    private Button btnStop;
    private Label lblClients;
    private ListBox lstClients;
    private Label lblLog;
    private ListBox lstLog;
    private TextBox txtServerMsg;
    private Button btnBroadcast;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        lblIP = new Label();
        txtIP = new TextBox();
        lblPort = new Label();
        txtPort = new TextBox();
        btnStart = new Button();
        btnStop = new Button();
        lblClients = new Label();
        lstClients = new ListBox();
        lblLog = new Label();
        lstLog = new ListBox();
        txtServerMsg = new TextBox();
        btnBroadcast = new Button();
        SuspendLayout();
        // 
        // lblIP
        // 
        lblIP.AutoSize = true;
        lblIP.Location = new Point(20, 22);
        lblIP.Name = "lblIP";
        lblIP.Size = new Size(41, 15);
        lblIP.TabIndex = 0;
        lblIP.Text = "本機IP";
        // 
        // txtIP
        // 
        txtIP.Location = new Point(74, 18);
        txtIP.Name = "txtIP";
        txtIP.ReadOnly = false;
        txtIP.Size = new Size(160, 23);
        txtIP.TabIndex = 1;
        txtIP.TextChanged += txtIP_TextChanged;
        // 
        // lblPort
        // 
        lblPort.AutoSize = true;
        lblPort.Location = new Point(250, 22);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(29, 15);
        lblPort.TabIndex = 2;
        lblPort.Text = "Port";
        // 
        // txtPort
        // 
        txtPort.Location = new Point(285, 18);
        txtPort.Name = "txtPort";
        txtPort.Size = new Size(82, 23);
        txtPort.TabIndex = 3;
        txtPort.Text = "5000";
        // 
        // btnStart
        // 
        btnStart.Location = new Point(390, 15);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(120, 30);
        btnStart.TabIndex = 4;
        btnStart.Text = "啟動伺服器";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        // 
        // btnStop
        // 
        btnStop.Location = new Point(525, 15);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(120, 30);
        btnStop.TabIndex = 5;
        btnStop.Text = "停止伺服器";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        // 
        // lblClients
        // 
        lblClients.AutoSize = true;
        lblClients.Location = new Point(20, 65);
        lblClients.Name = "lblClients";
        lblClients.Size = new Size(91, 15);
        lblClients.TabIndex = 6;
        lblClients.Text = "目前在線使用者";
        // 
        // lstClients
        // 
        lstClients.FormattingEnabled = true;
        lstClients.ItemHeight = 15;
        lstClients.Location = new Point(20, 90);
        lstClients.Name = "lstClients";
        lstClients.Size = new Size(250, 424);
        lstClients.TabIndex = 7;
        // 
        // lblLog
        // 
        lblLog.AutoSize = true;
        lblLog.Location = new Point(290, 65);
        lblLog.Name = "lblLog";
        lblLog.Size = new Size(67, 15);
        lblLog.TabIndex = 8;
        lblLog.Text = "伺服器紀錄";
        // 
        // lstLog
        // 
        lstLog.FormattingEnabled = true;
        lstLog.HorizontalScrollbar = true;
        lstLog.ItemHeight = 15;
        lstLog.Location = new Point(290, 90);
        lstLog.Name = "lstLog";
        lstLog.Size = new Size(580, 424);
        lstLog.TabIndex = 9;
        // 
        // txtServerMsg
        // 
        txtServerMsg.Location = new Point(20, 540);
        txtServerMsg.Name = "txtServerMsg";
        txtServerMsg.Size = new Size(700, 23);
        txtServerMsg.TabIndex = 10;
        // 
        // btnBroadcast
        // 
        btnBroadcast.Location = new Point(740, 536);
        btnBroadcast.Name = "btnBroadcast";
        btnBroadcast.Size = new Size(130, 30);
        btnBroadcast.TabIndex = 11;
        btnBroadcast.Text = "發送公告";
        btnBroadcast.UseVisualStyleBackColor = true;
        btnBroadcast.Click += btnBroadcast_Click;
        // 
        // FrmServer
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 590);
        Controls.Add(btnBroadcast);
        Controls.Add(txtServerMsg);
        Controls.Add(lstLog);
        Controls.Add(lblLog);
        Controls.Add(lstClients);
        Controls.Add(lblClients);
        Controls.Add(btnStop);
        Controls.Add(btnStart);
        Controls.Add(txtPort);
        Controls.Add(lblPort);
        Controls.Add(txtIP);
        Controls.Add(lblIP);
        Name = "FrmServer";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "多人聊天室 - Server";
        FormClosing += FrmServer_FormClosing;
        Load += FrmServer_Load;
        ResumeLayout(false);
        PerformLayout();
    }
}
