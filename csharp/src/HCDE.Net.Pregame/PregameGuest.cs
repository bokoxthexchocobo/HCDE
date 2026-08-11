using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public sealed class PregameGuestOptions
{
    public NetworkEndpoint ServerAddress { get; set; }
    public string Password { get; set; } = "";
    public EngineInfoSnapshot EngineInfo { get; set; } = new();
    public HcdeConnectFlags ConnectFlags { get; set; } = HcdeConnectFlags.ServerAuthority;
    public string UserInfo { get; set; } = "name\\guest";
}

public enum PregameGuestPhase
{
    Disconnected,
    SentConnect,
    WaitingForAssignment,
    Assigned,
    WaitingForStart,
    Synchronizing,
    Ready,
    Starting,
    Rejected,
}

/// <summary>
/// Guest-side pregame pump through console-player assignment and WAITING setup services.
/// </summary>
public sealed class PregameGuest
{
    private readonly UdpTransport _transport;
    private readonly PregameGuestOptions _options;
    private readonly PregameConnectionState _connection = new();
    private readonly PregameServiceSender _sender = new();
    private readonly PregameServiceReceiver _receiver = new();
    private readonly byte[] _netBuffer = new byte[NetConstants.MaxMessageLength];
    private readonly byte[] _payloadBuffer = new byte[NetConstants.MaxMessageLength];

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
    public MapLoadInfo? ReceivedMapLoad { get; private set; }
    public GameInfoPayload? ReceivedGameInfo { get; private set; }
    public IReadOnlyList<RosterEntry> ReceivedRoster { get; private set; } = Array.Empty<RosterEntry>();
    public VerificationErrorPacket? VerificationError { get; private set; }

    public void Pump(ulong nowMilliseconds)
    {
        if (Phase == PregameGuestPhase.Disconnected)
            SendConnect();

        if (Phase == PregameGuestPhase.Assigned)
            TrySendClientUserInfo();

        DrainInbound(nowMilliseconds);
        FlushOutbound(nowMilliseconds);
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

    private void TrySendClientUserInfo()
    {
        var userInfoBytes = System.Text.Encoding.ASCII.GetBytes(_options.UserInfo);
        if (_sender.TryQueueReliable(
                PregameServiceType.ClientUserInfo,
                _connection,
                AssignedClientSlot,
                userInfoBytes))
        {
            Phase = PregameGuestPhase.Synchronizing;
        }
    }

    private void DrainInbound(ulong nowMilliseconds)
    {
        while (PregameWire.TryReceive(_transport, _netBuffer, out var length, out var remote, TimeSpan.Zero)
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
                or PregameSetupType.Kicked
                or PregameSetupType.Banned
                or PregameSetupType.ProtocolError
                or PregameSetupType.SetupTimeout)
            {
                RejectReason = setupType;
                Phase = PregameGuestPhase.Rejected;
                continue;
            }

            if (setupType == PregameSetupType.VerificationError)
            {
                if (VerificationErrorCodec.TryRead(span, out var verificationError))
                    VerificationError = verificationError;
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

            HandleService(service, remote);
        }
    }

    private void HandleService(HcdeServicePacket service, NetworkEndpoint remote)
    {
        switch (service.Service)
        {
            case PregameServiceType.ConsolePlayer when service.Payload.Length >= 4:
                AssignedClientSlot = service.Payload.Span[0];
                MaxClients = service.Payload.Span[2];
                Phase = PregameGuestPhase.Assigned;
                break;
            case PregameServiceType.UserInfoAck when service.Payload.Length >= 1
                && service.Payload.Span[0] == AssignedClientSlot:
                Phase = PregameGuestPhase.WaitingForStart;
                QueueUserInfoAckMirror();
                break;
            case PregameServiceType.MapLoad:
                if (PregameServicePayloads.TryReadMapLoadInfo(service.Payload.Span, out var mapLoad))
                    ReceivedMapLoad = mapLoad;
                QueueHeaderOnlyAck(PregameServiceType.MapLoadAck, key: 0);
                break;
            case PregameServiceType.GameInfo:
                if (PregameServicePayloads.TryReadGameInfo(service.Payload.Span, out var gameInfo))
                    ReceivedGameInfo = gameInfo;
                QueueHeaderOnlyAck(PregameServiceType.GameInfoAck, key: 0);
                break;
            case PregameServiceType.Roster:
                if (PregameServicePayloads.TryReadRoster(service.Payload.Span, out var roster))
                    ReceivedRoster = roster;
                QueueHeaderOnlyAck(PregameServiceType.RosterAck, key: 0);
                Phase = PregameGuestPhase.Ready;
                break;
            case PregameServiceType.StartGame:
                QueueHeaderOnlyAck(PregameServiceType.StartGameAck, key: 0);
                Phase = PregameGuestPhase.Starting;
                break;
        }
    }

    private void QueueUserInfoAckMirror()
    {
        var payload = new byte[] { AssignedClientSlot };
        _sender.TryQueueReliable(PregameServiceType.UserInfoAck, _connection, AssignedClientSlot, payload);
    }

    private void QueueHeaderOnlyAck(PregameServiceType service, byte key)
    {
        _sender.TryQueueReliable(service, _connection, key, ReadOnlySpan<byte>.Empty);
    }

    private void FlushOutbound(ulong nowMilliseconds)
    {
        if (_connection.SessionToken == 0)
            return;

        if (_sender.TryFlush(_connection, nowMilliseconds, _netBuffer, out var length, force: true))
            PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, length), _options.ServerAddress);
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
