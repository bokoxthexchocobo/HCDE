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

    public bool ReceivedClientUserInfo { get; set; }
    public bool ReceivedUserInfoAck { get; set; }
    public bool HasMapLoadAck { get; set; }
    public bool HasGameInfoAck { get; set; }
    public bool HasRosterAck { get; set; }
    public bool HasStartGameAck { get; set; }
    public bool RuntimeJoin { get; set; }
    public bool HasBootstrapAck { get; set; }
    public string UserInfo { get; set; } = "";
}
