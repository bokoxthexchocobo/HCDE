using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public static class LiveWire
{
    public static bool TrySend(UdpTransport transport, ReadOnlySpan<byte> netBuffer, ReadOnlySpan<byte> gameId, NetworkEndpoint remote)
    {
        Span<byte> wire = stackalloc byte[GameplayWireCodec.CrcPrefixSize + netBuffer.Length];
        var length = GameplayWireCodec.Encode(netBuffer, gameId, wire);
        if (length == 0)
            return false;
        return transport.Send(wire[..length], remote) == length;
    }

    public static GameplayWireDecodeStatus TryReceive(
        UdpTransport transport,
        ReadOnlySpan<byte> gameId,
        Span<byte> netBuffer,
        out int netLength,
        out NetworkEndpoint remote,
        TimeSpan? timeout = null)
    {
        netLength = 0;
        remote = default;
        Span<byte> wire = stackalloc byte[NetConstants.MaxTransmitSize];
        if (!transport.TryReceive(wire, out var received, out remote, timeout))
            return GameplayWireDecodeStatus.TooShort;

        return GameplayWireCodec.TryDecode(wire[..received], gameId, netBuffer, out netLength);
    }
}

public sealed class LiveControlEndpoint
{
    private readonly UdpTransport _transport;
    private readonly byte[] _gameId;
    private readonly LiveSequenceTracker _sequenceTracker = new();
    private readonly LiveControlScheduler _scheduler = new();
    private uint _txSequence;

    public LiveControlEndpoint(UdpTransport transport, ReadOnlySpan<byte> gameId)
    {
        _transport = transport;
        _gameId = gameId.Length >= GameplayWireCodec.GameIdSize
            ? gameId[..GameplayWireCodec.GameIdSize].ToArray()
            : new byte[GameplayWireCodec.GameIdSize];
    }

    public ReadOnlySpan<byte> GameId => _gameId;
    public LiveSequenceTracker SequenceTracker => _sequenceTracker;

    public bool TrySendScheduledControl(
        ulong nowMs,
        NetworkEndpoint remote,
        LiveControlBasePayload basePayload,
        LiveControlCapabilities? capabilities = null)
    {
        if (!_scheduler.ShouldSendControl(nowMs))
            return false;
        return TrySendControl(remote, basePayload, capabilities);
    }

    public bool TrySendControl(
        NetworkEndpoint remote,
        LiveControlBasePayload basePayload,
        LiveControlCapabilities? capabilities = null)
    {
        _txSequence++;
        if (_txSequence == 0)
            _txSequence++;

        var packet = LiveControlPacketBuilder.BuildControl(
            _txSequence,
            _sequenceTracker.RxSequence,
            basePayload,
            capabilities);

        Span<byte> netBuffer = stackalloc byte[LiveConstants.HeaderSize + LiveConstants.ControlFullPayloadSize];
        var length = LivePacket.Write(netBuffer, packet);
        if (length == 0)
            return false;

        return LiveWire.TrySend(_transport, netBuffer[..length], _gameId, remote);
    }

    public bool TryReceiveControl(
        NetworkEndpoint expectedRemote,
        out LiveControlBasePayload basePayload,
        out LiveControlCapabilities? capabilities,
        out LiveCapabilityNegotiation? negotiation,
        TimeSpan? timeout = null)
    {
        basePayload = default;
        capabilities = null;
        negotiation = null;

        Span<byte> netBuffer = stackalloc byte[NetConstants.MaxMessageLength];
        var status = LiveWire.TryReceive(_transport, _gameId, netBuffer, out var netLength, out var remote, timeout);
        if (status != GameplayWireDecodeStatus.Ok)
            return false;

        if (!remote.Equals(expectedRemote))
            return false;

        if (!LiveHeader.TryRead(netBuffer[..netLength], out var header))
            return false;

        if (!_sequenceTracker.IsFresh(header.MessageType, header.TxSequence))
            return false;

        _sequenceTracker.Accept(header.MessageType, header.TxSequence);

        if (!LivePacket.TryRead(netBuffer[..netLength], out var packet))
            return false;

        return LiveControlPacketBuilder.TryParseControl(
            packet,
            out basePayload,
            out capabilities,
            out negotiation);
    }
}

public sealed class LiveGameplayEndpoint
{
    private readonly UdpTransport _transport;
    private readonly byte[] _gameId;
    private readonly LiveSequenceTracker _sequenceTracker = new();
    private uint _txSequence;

    public LiveGameplayEndpoint(UdpTransport transport, ReadOnlySpan<byte> gameId)
    {
        _transport = transport;
        _gameId = gameId.Length >= GameplayWireCodec.GameIdSize
            ? gameId[..GameplayWireCodec.GameIdSize].ToArray()
            : new byte[GameplayWireCodec.GameIdSize];
    }

    public bool TrySendEmptyClientInputs(NetworkEndpoint remote, byte roomId, uint gameTic) =>
        TrySendClientInput(remote, roomId, gameTic, playerNum: 0);

    public bool TrySendClientInput(NetworkEndpoint remote, byte roomId, uint gameTic, byte playerNum, UserCmd command = default)
    {
        Span<byte> inputPayload = stackalloc byte[512];
        var length = GameplayPayloadBuilders.BuildClientInputSinglePlayer(inputPayload, playerNum, command);
        if (length == 0)
            return false;

        return TrySendGameplay(
            remote,
            LiveMessageType.ClientCommands,
            GameplayPayloadKind.ClientInputs,
            roomId,
            gameTic,
            inputPayload[..length]);
    }

    public bool TrySendEmptyServerSnapshot(NetworkEndpoint remote, byte roomId, uint gameTic) =>
        TrySendServerSnapshot(remote, roomId, gameTic, playerNum: 0);

    public bool TrySendServerSnapshot(NetworkEndpoint remote, byte roomId, uint gameTic, byte playerNum, UserCmd command = default)
    {
        Span<byte> snapshotPayload = stackalloc byte[512];
        var length = GameplayPayloadBuilders.BuildServerSnapshotSinglePlayer(snapshotPayload, playerNum, command);
        if (length == 0)
            return false;

        return TrySendGameplay(
            remote,
            LiveMessageType.ServerSnapshot,
            GameplayPayloadKind.ServerSnapshot,
            roomId,
            gameTic,
            snapshotPayload[..length]);
    }

    public bool TryReceiveGameplay(
        NetworkEndpoint expectedRemote,
        GameplayPayloadKind expectedKind,
        byte currentRoomId,
        out LiveHeader header,
        out GameplayEnvelope envelope,
        out ReadOnlyMemory<byte> nativePayload,
        TimeSpan? timeout = null)
    {
        header = default;
        envelope = default;
        nativePayload = default;

        Span<byte> netBuffer = stackalloc byte[NetConstants.MaxMessageLength];
        var status = LiveWire.TryReceive(_transport, _gameId, netBuffer, out var netLength, out var remote, timeout);
        if (status != GameplayWireDecodeStatus.Ok || !remote.Equals(expectedRemote))
            return false;

        if (!LiveHeader.TryRead(netBuffer[..netLength], out header))
            return false;

        if (!_sequenceTracker.IsFresh(header.MessageType, header.TxSequence))
            return false;

        _sequenceTracker.Accept(header.MessageType, header.TxSequence);

        if (!LivePacket.TryRead(netBuffer[..netLength], out var packet))
            return false;

        return LiveGameplayPacketBuilder.TryUnwrap(
            packet,
            expectedKind,
            currentRoomId,
            out envelope,
            out nativePayload,
            out _);
    }

    private bool TrySendGameplay(
        NetworkEndpoint remote,
        LiveMessageType messageType,
        GameplayPayloadKind payloadKind,
        byte roomId,
        uint gameTic,
        ReadOnlySpan<byte> nativePayload)
    {
        _txSequence++;
        if (_txSequence == 0)
            _txSequence++;

        var packet = LiveGameplayPacketBuilder.BuildWrapped(
            messageType,
            payloadKind,
            _txSequence,
            _sequenceTracker.RxSequence,
            roomId,
            gameTic,
            nativePayload);

        Span<byte> netBuffer = stackalloc byte[LiveConstants.HeaderSize + LiveConstants.GameplayHeaderSize + nativePayload.Length];
        var length = LivePacket.Write(netBuffer, packet);
        if (length == 0)
            return false;

        return LiveWire.TrySend(_transport, netBuffer[..length], _gameId, remote);
    }
}
