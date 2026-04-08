using System.Net.Sockets;
using System.Text;

namespace MultiChatServer;

public class ClientSession
{
    public Socket? WorkSocket { get; set; }
    public string UserName { get; set; } = "未命名";
    public StringBuilder ReceiveBuffer { get; } = new();
    /// <summary>
    /// Tracks how far into ReceiveBuffer we have already scanned for '\n'.
    /// Avoids O(n²) allocations in ProcessChunk when receiving large files.
    /// </summary>
    public int ReceiveSearchPos { get; set; } = 0;

    /// <summary>
    /// FIX BUG3: Per-client send lock. Prevents interleaving when multiple threads
    /// (e.g. broadcast + TYPING) send to the same client at the same time.
    /// </summary>
    public readonly object SendLock = new();

    // [1] Rate-limit: message counter and sliding window start time
    public int MsgCount { get; set; } = 0;
    public DateTime MsgWindowStart { get; set; } = DateTime.UtcNow;

    // FIX BUG_DISPOSE2: RemoteEndPoint throws ObjectDisposedException when accessed
    // after WorkSocket.Close() is called (e.g. duplicate-name rejection race between
    // Task.Delay(200)+RemoveClient and ReceiveLoop's RemoveClient).
    // Wrap in try/catch so any late access returns empty string instead of crashing.
    public string EndPointText
    {
        get
        {
            try { return WorkSocket?.RemoteEndPoint?.ToString() ?? string.Empty; }
            catch (ObjectDisposedException) { return string.Empty; }
        }
    }
}
