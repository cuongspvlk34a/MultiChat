using System.Net;
using System.Net.Sockets;

namespace MultiChatServer;

public partial class FrmServer : Form
{
    private readonly IChatServer _server = new ChatServer();

    public FrmServer()
    {
        InitializeComponent();
        _server.LogGenerated += AppendLog;
        _server.ClientListChanged += RefreshClientList;
    }

    private void FrmServer_Load(object? sender, EventArgs e)
    {
        txtIP.Text = GetLocalIPv4() ?? "127.0.0.1";
        txtPort.Text = "5000";
        btnStop.Enabled = false;
    }

    private void btnStart_Click(object? sender, EventArgs e)
    {
        try
        {
            _server.Start(txtIP.Text.Trim(), int.Parse(txtPort.Text.Trim()));
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            txtPort.Enabled = false;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"啟動失敗：{ex.Message}");
        }
    }

    private void btnStop_Click(object? sender, EventArgs e)
    {
        _server.Stop();
        btnStart.Enabled = true;
        btnStop.Enabled = false;
        txtPort.Enabled = true;
    }

    private void btnBroadcast_Click(object? sender, EventArgs e)
    {
        string msg = txtServerMsg.Text.Trim();
        if (string.IsNullOrWhiteSpace(msg)) return;

        _server.Broadcast(ProtocolHelper.BuildSystem($"[公告] {msg}"));
        AppendLog($"Server 公告：{msg}");
        txtServerMsg.Clear();
        txtServerMsg.Focus();
    }

    private void AppendLog(string msg)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(AppendLog), msg);
            return;
        }

        lstLog.Items.Add($"{DateTime.Now:HH:mm:ss} {msg}");
        lstLog.TopIndex = lstLog.Items.Count - 1;
    }

    private void RefreshClientList(List<string> clients)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<List<string>>(RefreshClientList), clients);
            return;
        }

        lstClients.Items.Clear();
        lstClients.Items.AddRange(clients.ToArray());
    }

    private static string? GetLocalIPv4()
    {
        string hostName = Dns.GetHostName();
        IPAddress[] addresses = Dns.GetHostAddresses(hostName);

        foreach (IPAddress addr in addresses)
        {
            if (addr.AddressFamily == AddressFamily.InterNetwork)
            {
                return addr.ToString();
            }
        }

        return null;
    }

    private void FrmServer_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _server.Stop();
    }

    private void txtIP_TextChanged(object sender, EventArgs e)
    {

    }
}
