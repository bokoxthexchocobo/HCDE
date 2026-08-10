using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public sealed class PregameClient
{
    public NetworkEndpoint Address { get; set; }
    public byte ClientSlot { get; set; }
    public ConnectionStatus Status { get; set; } = ConnectionStatus.None;
    public PregameConnectionState Connection { get; } = new();
    public PregameServiceSender Sender { get; } = new();
    public bool HcdeConnect { get; set; }
    public HcdeConnectFlags ConnectFlags { get; set; }
}
