namespace MultiChatClient;

/// <summary>Defines a complete colour palette for the chat UI.</summary>
public class ChatTheme
{
    public string Name       { get; init; } = "";
    public Color  Background { get; init; }
    public Color  Panel      { get; init; }
    public Color  Header     { get; init; }
    public Color  Toolbar    { get; init; }
    public Color  Input      { get; init; }
    public Color  Accent     { get; init; }
    public Color  Danger     { get; init; }
    public Color  Success    { get; init; }
    public Color  Text       { get; init; }
    public Color  Muted      { get; init; }
    public Color  OwnBubble  { get; init; }
    public Color  OtherBubble{ get; init; }
    public Color  SysText    { get; init; }
    public Color  TimeText   { get; init; }
    public Color  NameText   { get; init; }

    // ── Preset themes ─────────────────────────────────────────────────────────

    public static readonly ChatTheme Dark = new()
    {
        Name        = "🌙 Dark",
        Background  = Color.FromArgb(18,  18,  32),
        Panel       = Color.FromArgb(28,  28,  44),
        Header      = Color.FromArgb(13,  13,  26),
        Toolbar     = Color.FromArgb(22,  22,  38),
        Input       = Color.FromArgb(40,  40,  62),
        Accent      = Color.FromArgb(124, 106, 247),
        Danger      = Color.FromArgb(220, 60,  70),
        Success     = Color.FromArgb(0,   200, 110),
        Text        = Color.FromArgb(220, 220, 232),
        Muted       = Color.FromArgb(100, 100, 140),
        OwnBubble   = Color.FromArgb(100, 84,  220),
        OtherBubble = Color.FromArgb(40,  40,  66),
        SysText     = Color.FromArgb(90,  90,  130),
        TimeText    = Color.FromArgb(100, 100, 150),
        NameText    = Color.FromArgb(180, 165, 255),
    };

    public static readonly ChatTheme Light = new()
    {
        Name        = "☀️ Light",
        Background  = Color.FromArgb(245, 245, 248),
        Panel       = Color.FromArgb(255, 255, 255),
        Header      = Color.FromArgb(255, 255, 255),
        Toolbar     = Color.FromArgb(248, 248, 252),
        Input       = Color.FromArgb(238, 238, 244),
        Accent      = Color.FromArgb(100, 80,  230),
        Danger      = Color.FromArgb(210, 48,  60),
        Success     = Color.FromArgb(0,   170, 90),
        Text        = Color.FromArgb(30,  30,  50),
        Muted       = Color.FromArgb(130, 130, 160),
        OwnBubble   = Color.FromArgb(110, 90,  240),
        OtherBubble = Color.FromArgb(220, 220, 235),
        SysText     = Color.FromArgb(150, 150, 180),
        TimeText    = Color.FromArgb(160, 160, 190),
        NameText    = Color.FromArgb(90,  70,  210),
    };

    public static readonly ChatTheme Zalo = new()
    {
        Name        = "💙 Zalo",
        Background  = Color.FromArgb(235, 242, 255),
        Panel       = Color.FromArgb(255, 255, 255),
        Header      = Color.FromArgb(0,   104, 255),
        Toolbar     = Color.FromArgb(240, 245, 255),
        Input       = Color.FromArgb(228, 236, 255),
        Accent      = Color.FromArgb(0,   104, 255),
        Danger      = Color.FromArgb(220, 48,  58),
        Success     = Color.FromArgb(0,   180, 100),
        Text        = Color.FromArgb(20,  20,  50),
        Muted       = Color.FromArgb(110, 120, 160),
        OwnBubble   = Color.FromArgb(0,   104, 255),
        OtherBubble = Color.FromArgb(255, 255, 255),
        SysText     = Color.FromArgb(140, 150, 190),
        TimeText    = Color.FromArgb(150, 160, 200),
        NameText    = Color.FromArgb(0,   80,  200),
    };

    public static readonly ChatTheme WhatsApp = new()
    {
        Name        = "💚 WhatsApp",
        Background  = Color.FromArgb(230, 242, 230),
        Panel       = Color.FromArgb(255, 255, 255),
        Header      = Color.FromArgb(18,  140, 126),
        Toolbar     = Color.FromArgb(240, 250, 240),
        Input       = Color.FromArgb(220, 242, 220),
        Accent      = Color.FromArgb(18,  140, 126),
        Danger      = Color.FromArgb(200, 50,  60),
        Success     = Color.FromArgb(18,  140, 126),
        Text        = Color.FromArgb(20,  40,  20),
        Muted       = Color.FromArgb(100, 140, 110),
        OwnBubble   = Color.FromArgb(220, 248, 198),
        OtherBubble = Color.FromArgb(255, 255, 255),
        SysText     = Color.FromArgb(120, 160, 130),
        TimeText    = Color.FromArgb(130, 170, 140),
        NameText    = Color.FromArgb(14,  110, 100),
    };

    public static readonly ChatTheme Discord = new()
    {
        Name        = "🎮 Discord",
        Background  = Color.FromArgb(54,  57,  63),
        Panel       = Color.FromArgb(47,  49,  54),
        Header      = Color.FromArgb(32,  34,  37),
        Toolbar     = Color.FromArgb(40,  43,  48),
        Input       = Color.FromArgb(64,  68,  75),
        Accent      = Color.FromArgb(88,  101, 242),
        Danger      = Color.FromArgb(237, 66,  69),
        Success     = Color.FromArgb(87,  242, 135),
        Text        = Color.FromArgb(220, 221, 222),
        Muted       = Color.FromArgb(148, 155, 164),
        OwnBubble   = Color.FromArgb(88,  101, 242),
        OtherBubble = Color.FromArgb(64,  68,  75),
        SysText     = Color.FromArgb(130, 135, 145),
        TimeText    = Color.FromArgb(148, 155, 164),
        NameText    = Color.FromArgb(160, 170, 255),
    };

    public static readonly ChatTheme Sunset = new()
    {
        Name        = "🌅 Sunset",
        Background  = Color.FromArgb(30,  14,  22),
        Panel       = Color.FromArgb(44,  22,  34),
        Header      = Color.FromArgb(20,  8,   16),
        Toolbar     = Color.FromArgb(36,  16,  28),
        Input       = Color.FromArgb(60,  30,  46),
        Accent      = Color.FromArgb(255, 100, 80),
        Danger      = Color.FromArgb(220, 50,  70),
        Success     = Color.FromArgb(255, 180, 40),
        Text        = Color.FromArgb(255, 220, 200),
        Muted       = Color.FromArgb(160, 100, 120),
        OwnBubble   = Color.FromArgb(200, 80,  60),
        OtherBubble = Color.FromArgb(60,  30,  46),
        SysText     = Color.FromArgb(140, 80,  100),
        TimeText    = Color.FromArgb(160, 100, 120),
        NameText    = Color.FromArgb(255, 160, 100),
    };

    public static ChatTheme[] All => new[] { Dark, Light, Zalo, WhatsApp, Discord, Sunset };
}
