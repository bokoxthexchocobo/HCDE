using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public sealed class PregameGuestOptions
{
    public NetworkEndpoint ServerAddress { get; set; }
    public string Password { get; set; } = "";
    public EngineInfoSnapshot EngineInfo { get; set; } = new();
    public HcdeConnectFlags ConnectFlags { get; set; } = HcdeConnectFlags.ServerAuthority;
}

public enum PregameGuestPhase
{
    Disconnected,
    SentConnect,
    WaitingForAssignment,
    Assigned,
    Rejected,
}

/// <summary>
/// Minimal guest-side pregame pump: send PRE_CONNECT, accept PRE_CONNECT_ACK and console-player service.
/// </summary>
public sealed class PregameGuest
{
    private readonly UdpTransport _transport;
    private readonly PregameGuestOptions _options;
    private readonly PregameConnectionState _connection = new();
    private readonly PregameServiceReceiver _receiver = new();
    private readonly byte[] _netBuffer = new byte[NetConstants.MaxMessageLength];

    public PregameGuest(UdpTransport transport, PregameGuestOptions options)
    {
        _transport = transport;
        _options = options;
    }

    public PregameGuestPhase Phase { get; private set; } = PregameGuestPhase.Disconnected;
    public PregameConnectionState Connection => _connection;
    public byte AssignedClientSlot { get; private set; }
    public byte MaxClients { get; private set; }
    public PregameSetupType? RejectReason { get; private set; }

    public void Pump(ulong nowMilliseconds)
    {
        if (Phase == PregameGuestPhase.Disconnected)
            SendConnect();

        DrainInbound(nowMilliseconds);
    }

    private void SendConnect()
    {
        var length = ConnectPacketCodec.Write(
            _netBuffer,
            _options.EngineInfo,
            _options.Password,
            _options.ConnectFlags);
        if (length == 0)
            return;

        if (PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, length), _options.ServerAddress))
            Phase = PregameGuestPhase.SentConnect;
    }

    private void DrainInbound(ulong nowMilliseconds)
    {
        while (PregameWire.TryReceive(_transport, _netBuffer, out var length, out _, TimeSpan.Zero)
               == SetupPacketDecodeStatus.Ok)
        {
            var span = _netBuffer.AsSpan(0, length);
            if (span.Length < 2)
                continue;

            var setupType = (PregameSetupType)span[1];
            if (setupType == PregameSetupType.ConnectAck)
            {
                HandleConnectAck(span);
                continue;
            }

            if (setupType is PregameSetupType.Full
                or PregameSetupType.InProgress
                or PregameSetupType.WrongPassword
                or PregameSetupType.VerificationError
                or PregameSetupType.Kicked
                or PregameSetupType.Banned
                or PregameSetupType.ProtocolError
                or PregameSetupType.SetupTimeout)
            {
                RejectReason = setupType;
                Phase = PregameGuestPhase.Rejected;
                continue;
            }

            if (setupType != PregameSetupType.HcdeService)
                continue;

            if (_connection.SessionToken == 0)
                continue;

            if (_receiver.TryAccept(span, _connection, nowMilliseconds) != PregameServiceReceiveResult.Accepted)
                continue;

            if (!HcdeServicePacket.TryRead(span, out var service))
                continue;

            if (service.Service != PregameServiceType.ConsolePlayer || service.Payload.Length < 4)
                continue;

            AssignedClientSlot = service.Payload.Span[0];
            MaxClients = service.Payload.Span[2];
            Phase = PregameGuestPhase.Assigned;
        }
    }

    private void HandleConnectAck(ReadOnlySpan<byte> netBuffer)
    {
        if (!ConnectAckPacket.TryRead(netBuffer, out var ack))
            return;
        if (!ack.Flags.HasFlag(PreConnectAckFlags.HcdeService))
            return;

        _connection.SessionToken = ack.SessionToken;
        MaxClients = ack.MaxClients;
        Phase = PregameGuestPhase.WaitingForAssignment;
    }
}
