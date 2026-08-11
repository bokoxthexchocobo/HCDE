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
    public PregameSessionSnapshot Session { get; set; } = new();
}

/// <summary>
/// Host-side pregame pump through CONNECTING and WAITING setup states.
/// </summary>
public sealed class PregameHost
{
    private readonly UdpTransport _transport;
    private readonly PregameHostOptions _options;
    private readonly PregameServiceReceiver _receiver = new();
    private readonly PregameClient[] _clients;
    private readonly byte[] _netBuffer = new byte[NetConstants.MaxMessageLength];
    private readonly byte[] _payloadBuffer = new byte[NetConstants.MaxMessageLength];
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
        DriveWaitingClients(nowMilliseconds);
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
            if (client is null || span.Length < 2 || span[1] != (byte)PregameSetupType.HcdeService)
                continue;

            if (_receiver.TryAccept(span, client.Connection, nowMilliseconds) != PregameServiceReceiveResult.Accepted)
                continue;

            if (!HcdeServicePacket.TryRead(span, out var service))
                continue;

            HandleServiceFromClient(client, service, nowMilliseconds);
        }
    }

    private void HandleServiceFromClient(PregameClient client, HcdeServicePacket service, ulong nowMilliseconds)
    {
        switch (service.Service)
        {
            case PregameServiceType.ClientUserInfo:
                client.ReceivedClientUserInfo = true;
                client.UserInfo = System.Text.Encoding.ASCII.GetString(service.Payload.Span);
                client.Status = ConnectionStatus.Waiting;
                break;
            case PregameServiceType.UserInfoAck:
                if (service.Payload.Length >= 1 && service.Payload.Span[0] == client.ClientSlot)
                    client.ReceivedUserInfoAck = true;
                break;
            case PregameServiceType.MapLoadAck:
                client.HasMapLoadAck = true;
                break;
            case PregameServiceType.GameInfoAck:
                client.HasGameInfoAck = true;
                break;
            case PregameServiceType.RosterAck:
                client.HasRosterAck = true;
                break;
        }

        FlushClient(client, nowMilliseconds);
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

        var verification = EngineInfoVerifier.Verify(connect.EngineInfo, _options.Session.RequiredWadCrcs);
        if (!verification.IsSuccess)
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
            client.Sender.TryQueueReliable(
                PregameServiceType.ConsolePlayer,
                client.Connection,
                client.ClientSlot,
                consolePayload);
            FlushClient(client, nowMilliseconds, force: true);
        }
    }

    private void DriveWaitingClients(ulong nowMilliseconds)
    {
        foreach (var client in _clients)
        {
            if (client.Status != ConnectionStatus.Waiting)
                continue;

            if (!client.ReceivedUserInfoAck)
            {
                QueueUserInfoAck(client);
            }
            else if (!client.HasMapLoadAck)
            {
                QueueMapLoad(client);
            }
            else if (!client.HasGameInfoAck)
            {
                QueueGameInfo(client);
            }
            else if (!client.HasRosterAck)
            {
                QueueRoster(client);
            }
            else
            {
                client.Status = ConnectionStatus.Ready;
            }

            FlushClient(client, nowMilliseconds, force: true);
        }
    }

    private void QueueUserInfoAck(PregameClient client)
    {
        if (!client.ReceivedClientUserInfo)
            return;

        var payload = new byte[] { client.ClientSlot };
        client.Sender.TryQueueReliable(PregameServiceType.UserInfoAck, client.Connection, client.ClientSlot, payload);
    }

    private void QueueMapLoad(PregameClient client)
    {
        var payloadLength = PregameServicePayloads.WriteMapLoadInfo(_payloadBuffer, _options.Session.MapLoad);
        if (payloadLength == 0)
            return;
        client.Sender.TryQueueReliable(
            PregameServiceType.MapLoad,
            client.Connection,
            key: 0,
            _payloadBuffer.AsSpan(0, payloadLength));
    }

    private void QueueGameInfo(PregameClient client)
    {
        var gameInfo = _options.Session.GameInfo;
        if (gameInfo.GameId.Length < 8)
            gameInfo = new GameInfoPayload { TicDup = gameInfo.TicDup, GameId = _options.GameId, ServerInfo = gameInfo.ServerInfo };

        var payloadLength = PregameServicePayloads.WriteGameInfo(_payloadBuffer, gameInfo);
        if (payloadLength == 0)
            return;
        client.Sender.TryQueueReliable(
            PregameServiceType.GameInfo,
            client.Connection,
            key: 0,
            _payloadBuffer.AsSpan(0, payloadLength));
    }

    private void QueueRoster(PregameClient client)
    {
        var entries = new List<RosterEntry>
        {
            new()
            {
                ClientSlot = 0,
                UserInfo = _options.Session.HostUserInfo,
            },
            new()
            {
                ClientSlot = client.ClientSlot,
                Address = CreateSockAddrPlaceholder(client.Address),
                UserInfo = client.UserInfo,
            },
        };

        var payloadLength = PregameServicePayloads.WriteRoster(_payloadBuffer, entries);
        if (payloadLength == 0)
            return;
        client.Sender.TryQueueReliable(
            PregameServiceType.Roster,
            client.Connection,
            key: 0,
            _payloadBuffer.AsSpan(0, payloadLength));
    }

    private static byte[] CreateSockAddrPlaceholder(NetworkEndpoint endpoint)
    {
        var bytes = new byte[PregameServicePayloads.SockAddrInSize];
        if (endpoint.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            endpoint.Address.GetAddressBytes().CopyTo(bytes, 4);
        bytes[2] = (byte)(endpoint.Port >> 8);
        bytes[3] = (byte)endpoint.Port;
        return bytes;
    }

    private void FlushClient(PregameClient client, ulong nowMilliseconds, bool force = false)
    {
        if (client.Sender.TryFlush(client.Connection, nowMilliseconds, _netBuffer, out var length, force))
            PregameWire.TrySend(_transport, _netBuffer.AsSpan(0, length), client.Address);
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
