namespace MultiChatClient;

/// <summary>Lightweight search bar that highlights matches in the chat RichTextBox.</summary>
public class SearchForm : Form
{
    private readonly RichTextBox _rtb;
    private TextBox  _txtSearch = null!;
    private Button   _btnPrev   = null!;
    private Button   _btnNext   = null!;
    private Label    _lblResult = null!;
    private int      _lastIdx   = -1;
    private List<int> _matches  = new();

    public SearchForm(RichTextBox rtb, ChatTheme theme)
    {
        _rtb = rtb;
        Build(theme);
    }

    private void Build(ChatTheme t)
    {
        Text            = "🔍 Tìm kiếm tin nhắn";
        Size            = new Size(440, 110);
        FormBorderStyle = FormBorderStyle.FixedToolWindow;
        BackColor       = t.Panel;
        StartPosition   = FormStartPosition.Manual;

        _txtSearch = new TextBox
        {
            Font = new Font("Segoe UI", 10f), BackColor = t.Input,
            ForeColor = t.Text, BorderStyle = BorderStyle.FixedSingle,
            Location  = new Point(8, 12), Size = new Size(220, 28)
        };
        _txtSearch.TextChanged += (_, _) => DoSearch();
        _txtSearch.KeyDown     += (_, e) => { if (e.KeyCode == Keys.Enter) GoNext(); };

        var btnSearch = new Button
        {
            Text = "Tìm", BackColor = t.Accent, ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat, Location = new Point(234, 11), Size = new Size(58, 28)
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += (_, _) => DoSearch();

        _btnPrev = new Button
        {
            Text = "◀", BackColor = t.Toolbar, ForeColor = t.Text,
            FlatStyle = FlatStyle.Flat, Location = new Point(298, 11), Size = new Size(38, 28), Enabled = false
        };
        _btnPrev.FlatAppearance.BorderSize = 0;
        _btnPrev.Click += (_, _) => GoPrev();

        _btnNext = new Button
        {
            Text = "▶", BackColor = t.Toolbar, ForeColor = t.Text,
            FlatStyle = FlatStyle.Flat, Location = new Point(340, 11), Size = new Size(38, 28), Enabled = false
        };
        _btnNext.FlatAppearance.BorderSize = 0;
        _btnNext.Click += (_, _) => GoNext();

        _lblResult = new Label
        {
            Text = "Nhập từ khoá để tìm...", Font = new Font("Segoe UI", 8.5f),
            ForeColor = t.Muted, Location = new Point(8, 48), Size = new Size(400, 20)
        };

        Controls.AddRange(new Control[] { _txtSearch, btnSearch, _btnPrev, _btnNext, _lblResult });
        _txtSearch.Focus();
    }

    private void DoSearch()
    {
        _matches.Clear();
        _lastIdx = -1;
        string keyword = _txtSearch.Text.Trim();

        if (string.IsNullOrEmpty(keyword))
        {
            _lblResult.Text = "Nhập từ khoá để tìm...";
            _btnPrev.Enabled = _btnNext.Enabled = false;
            return;
        }

        string full = _rtb.Text;
        int    idx  = 0;
        while ((idx = full.IndexOf(keyword, idx, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            _matches.Add(idx);
            idx += keyword.Length;
        }

        _lblResult.Text = _matches.Count > 0
            ? $"Tìm thấy {_matches.Count} kết quả."
            : "Không tìm thấy.";
        _btnPrev.Enabled = _btnNext.Enabled = _matches.Count > 0;

        if (_matches.Count > 0) Highlight(0);
    }

    private void GoNext()
    {
        if (_matches.Count == 0) return;
        _lastIdx = (_lastIdx + 1) % _matches.Count;
        Highlight(_lastIdx);
        _lblResult.Text = $"Kết quả {_lastIdx + 1} / {_matches.Count}";
    }

    private void GoPrev()
    {
        if (_matches.Count == 0) return;
        _lastIdx = (_lastIdx - 1 + _matches.Count) % _matches.Count;
        Highlight(_lastIdx);
        _lblResult.Text = $"Kết quả {_lastIdx + 1} / {_matches.Count}";
    }

    private void Highlight(int idx)
    {
        string kw = _txtSearch.Text.Trim();
        _rtb.SelectionStart  = _matches[idx];
        _rtb.SelectionLength = kw.Length;
        _rtb.ScrollToCaret();
    }
}
