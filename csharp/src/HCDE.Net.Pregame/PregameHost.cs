using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public sealed class PregameHostOptions
{
    public int MaxClients { get; set; } = 8;
    public byte[] GameId { get; set; } = new byte[8];
    public bool RequirePassword { get; set; }
    public string Password { get; set; } = "";
    public bool AdvertiseDedicated { get; set; }
    public EngineInfoSnapshot ExpectedEngineInfo { get; set; } = new();
    public bool RequireHcdeConnectInfo { get; set; } = true;
}

/// <summary>
/// Minimal host-side pregame pump: admit PRE_CONNECT, emit PRE_CONNECT_ACK and console-player service.
/// </summary>
public sealed class PregameHost
{
    private readonly UdpTransport _transport;
    private readonly PregameHostOptions _options;
    private readonly PregameServiceReceiver _receiver = new();
    private readonly PregameClient[] _clients;
    private readonly byte[] _netBuffer = new byte[NetConstants.MaxMessageLength];
    private int _connectedPlayers;

    public PregameHost(UdpTransport transport, PregameHostOptions? options = null)
    {
        _transport = transport;
        _options = options ?? new PregameHostOptions();
        _clients = new PregameClient[_options.MaxClients];
        for (var i = 0; i < _clients.Length; i++)
            _clients[i] = new PregameClient { ClientSlot = (byte)i };
    }

    public IReadOnlyList<PregameClient> Clients => _clients;

    public void Pump(ulong nowMilliseconds)
    {
        DrainInbound(nowMilliseconds);
        DriveConnectingClients(nowMilliseconds);
    }

    private void DrainInbound(ulong nowMilliseconds)
    {
        while (PregameWire.TryReceive(_transport, _netBuffer, out var length, out var remote, TimeSpan.Zero)
               == SetupPacketDecodeStatus.Ok)
        {
            var span = _netBuffer.AsSpan(0, length);
            if (span.Length >= 2 && span[1] == (byte)PregameSetupType.Connect)
            {
                TryAdmitConnect(span, remote, nowMilliseconds);
                continue;
            }

            var client = FindClientByAddress(remote);
            if (client is null)
                continue;

            if (span.Length >= 2 && span[1] == (byte)PregameSetupType.HcdeService)
            {
                _receiver.TryAccept(span, client.Connection, nowMilliseconds);
                if (span.Length >= PregameConstants.ServiceHeaderSize + 1
                    && span[2] == (byte)PregameServiceType.ClientUserInfo)
                {
                    client.Status = ConnectionStatus.Waiting;
                }
            }
        }
    }

    private void TryAdmitConnect(ReadOnlySpan<byte> netBuffer, NetworkEndpoint remote, ulong nowMilliseconds)
    {
        if (!ConnectPacketCodec.TryRead(netBuffer, out var connect))
            return;

        if (_options.RequireHcdeConnectInfo && !connect.HasConnectInfo)
        {
            SendReject(remote, PregameSetupType.ProtocolError);
            return;
        }

        if (connect.HasConnectInfo && connect.ConnectVersion != PregameConstants.ConnectProtocolVersion)
        {
            SendReject(remote, PregameSetupType.ProtocolError);
            return;
        }

        if (!EngineInfoCodec.Matches(_options.ExpectedEngineInfo, connect.EngineInfo))
        {
            SendReject(remote, PregameSetupType.ProtocolError);
            return;
        }

        if (_options.RequirePassword && connect.Password != _options.Password)
        {
            SendReject(remote, PregameSetupType.WrongPassword);
            return;
        }

        if (_connectedPlayers >= _options.MaxClients - 1)
        {
            SendReject(remote, PregameSetupType.Full);
            return;
        }

        var slot = FindFreeClientSlot();
        if (slot is null)
        {
            SendReject(remote, PregameSetupType.Full);
            return;
        }

        slot.Address = remote;
        slot.Status = ConnectionStatus.Connecting;
        slot.HcdeConnect = connect.HasConnectInfo;
        slot.ConnectFlags = connect.ConnectFlags;
        slot.Connection.Reset();
        slot.Connection.SessionToken = SessionToken.Mint(remote, slot.ClientSlot, _options.GameId, nowMilliseconds);
        _connectedPlayers++;
    }

    private void DriveConnectingClients(ulong nowMilliseconds)
    {
        foreach (var client in _clients)
        {
            if (client.Status != ConnectionStatus.Connecting || client.Connection.SessionToken == 0)
                continue;

            if (client.Connection.RuntimeLastConnectAckTime == 0
                || nowMilliseconds - client.Connection.RuntimeLastConnectAckTime
                >= PregameConstants.RuntimeConnectAckResendMilliseconds)
            {
                client.Connection.RuntimeLastConnectAckTime = nowMilliseconds;
                SendConnectAck(client);
            }

            var consolePayload = new byte[]
            {
                client.ClientSlot,
                (byte)_connectedPlayers,
                (byte)_options.MaxClients,
                (byte)HcdeConnectFlags.ServerAuthority,
            };
            if (client.Sender.TryQueueReliable(
                    PregameServiceType.ConsolePlayer,
                    client.Connection,
                    client.ClientSlot,
                    consolePayload))
            {
                // queued
            }

            if (client.Sender.TryFlush(client.Connection, nowMilliseconds, _netBuffer, out var length, force: true))
                PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, length), client.Address);
        }
    }

    private void SendConnectAck(PregameClient client)
    {
        var flags = PreConnectAckFlags.HcdeService;
        if (_options.AdvertiseDedicated)
            flags |= PreConnectAckFlags.Dedicated | PreConnectAckFlags.ServerAuthority;

        var length = ConnectAckPacket.Write(
            _netBuffer,
            client.ClientSlot,
            (byte)_connectedPlayers,
            (byte)_options.MaxClients,
            client.Connection.SessionToken,
            flags,
            PregameConstants.ConnectProtocolVersion,
            HcdeConnectFlags.ServerAuthority);
        PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, length), client.Address);
    }

    private void SendReject(NetworkEndpoint remote, PregameSetupType reason)
    {
        _netBuffer[0] = (byte)NetCommandFlags.Setup;
        _netBuffer[1] = (byte)reason;
        PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, 2), remote);
    }

    private PregameClient? FindClientByAddress(NetworkEndpoint remote)
    {
        foreach (var client in _clients)
        {
            if (client.Status != ConnectionStatus.None && client.Address == remote)
                return client;
        }

        return null;
    }

    private PregameClient? FindFreeClientSlot()
    {
        for (var i = 1; i < _clients.Length; i++)
        {
            if (_clients[i].Status == ConnectionStatus.None)
                return _clients[i];
        }

        return null;
    }
}
