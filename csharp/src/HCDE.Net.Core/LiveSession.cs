using HCDE.Net.Transport;

namespace HCDE.Net.Core;

public sealed class LiveGuestSession
{
    private readonly LiveControlEndpoint _control;
    private readonly LiveGameplayEndpoint _gameplay;
    private readonly NetworkEndpoint _authorityEndpoint;
    private readonly LivePeerRoutingState _routing;
    private readonly LivePeerSlotTracker _peerSlots;
    private readonly LivePeerNetRegistry _netRegistry;
    private readonly PresentationEchoApplySession _echoApply;
    private IPresentationEchoApplySink? _echoSink;
    private IAuthorityEventSink? _authoritySink;
    private IServerSnapshotCommandSink? _snapshotCommandSink;
    private IWorldDeltaApplySink? _worldDeltaSink;
    private IActorDeltaApplySink? _actorDeltaSink;
    private ICoopDeadSpawnsApplySink? _coopDeadSpawnsSink;
    private IInvasionSnapshotApplySink? _invasionSink;
    private SnapshotChecksumSession? _checksumSession;
    private ISnapshotChecksumMismatchSink? _checksumMismatchSink;
    private GuestWorldStateStore? _guestWorldState;
    private int _guestWorldStateRngSeed;
    private ulong _negotiatedCapabilities = LiveConstants.DefaultLocalCapabilities;
    private byte _roomId;
    private uint _gameTic;

    public LiveGuestSession(
        UdpTransport transport,
        ReadOnlySpan<byte> gameId,
        NetworkEndpoint authorityEndpoint,
        int guestPlayerSlot,
        int authoritySlot,
        int maxClients)
    {
        _control = new LiveControlEndpoint(transport, gameId);
        _gameplay = new LiveGameplayEndpoint(transport, gameId);
        _authorityEndpoint = authorityEndpoint;
        _routing = new LivePeerRoutingState(
            consolePlayer: guestPlayerSlot,
            maxClients: maxClients,
            authoritySlot: authoritySlot,
            isLocalAuthority: false,
            usesHcdeService: true);
        _peerSlots = new LivePeerSlotTracker(maxClients);
        _netRegistry = new LivePeerNetRegistry(maxClients);
        _echoApply = new PresentationEchoApplySession(maxClients);
    }

    public LivePeerSlotTracker PeerSlots => _peerSlots;

    public LivePeerNetRegistry NetRegistry => _netRegistry;

    public PresentationEchoApplySession EchoApply => _echoApply;

    public GuestWorldStateStore? GuestWorldState => _guestWorldState;

    public void SetNegotiatedCapabilities(ulong negotiatedCapabilities) =>
        _negotiatedCapabilities = negotiatedCapabilities;

    public void SetChecksumSession(
        SnapshotChecksumSession? checksumSession,
        ISnapshotChecksumMismatchSink? mismatchSink = null)
    {
        _checksumSession = checksumSession;
        _checksumMismatchSink = mismatchSink;
    }

    public void SetGuestWorldState(
        GuestWorldStateStore worldState,
        SnapshotChecksumSession checksumSession,
        int rngSeed = 0,
        ISnapshotChecksumMismatchSink? mismatchSink = null)
    {
        _guestWorldState = worldState;
        _guestWorldStateRngSeed = rngSeed;
        SetChecksumSession(checksumSession, mismatchSink);
        _worldDeltaSink = worldState;
        _actorDeltaSink = worldState;
    }

    public void SetApplySinks(
        IPresentationEchoApplySink? echoSink,
        IAuthorityEventSink? authoritySink,
        IServerSnapshotCommandSink? snapshotCommandSink = null,
        IWorldDeltaApplySink? worldDeltaSink = null,
        IActorDeltaApplySink? actorDeltaSink = null,
        ICoopDeadSpawnsApplySink? coopDeadSpawnsSink = null,
        IInvasionSnapshotApplySink? invasionSink = null)
    {
        _echoSink = echoSink;
        _authoritySink = authoritySink;
        _snapshotCommandSink = snapshotCommandSink;
        _worldDeltaSink = worldDeltaSink;
        _actorDeltaSink = actorDeltaSink;
        _coopDeadSpawnsSink = coopDeadSpawnsSink;
        _invasionSink = invasionSink;
    }

    public void Pump(ulong nowMs, byte roomId = 0)
    {
        _roomId = roomId;
        _gameTic++;

        if (_routing.ShouldSendControlTo(_routing.AuthoritySlot))
        {
            _control.TrySendScheduledControl(
                nowMs,
                _authorityEndpoint,
                new LiveControlBasePayload(_gameTic, (byte)_routing.ConsolePlayer, (byte)_routing.MaxClients));
        }

        if (_routing.ShouldSendClientInputTo(_routing.AuthoritySlot))
            _gameplay.TrySendClientInput(_authorityEndpoint, _roomId, _gameTic, (byte)_routing.ConsolePlayer);
    }

    public bool TryReceiveAuthorityControl(out LiveControlBasePayload basePayload) =>
        _control.TryReceiveControl(_authorityEndpoint, out basePayload, out _, out _);

    public bool TryReceiveServerSnapshot(
        out ServerSnapshotHeader header,
        out IReadOnlyList<ServerSnapshotPlayerRecord> players,
        out ServerSnapshotTailSections? tailSections)
    {
        header = default;
        players = Array.Empty<ServerSnapshotPlayerRecord>();
        tailSections = null;
        if (!_gameplay.TryReceiveGameplay(
                _authorityEndpoint,
                GameplayPayloadKind.ServerSnapshot,
                _roomId,
                out _,
                out var envelope,
                out var nativePayload))
        {
            return false;
        }

        if (!ServerSnapshotHeader.TryRead(nativePayload.Span, out header))
            return false;

        ReadOnlySpan<byte> quitterPlayerSlots = default;
        if (header.QuitterBytes > 0)
        {
            if (!ServerSnapshotQuitterCodec.TryRead(
                    nativePayload.Span[LiveConstants.ServerSnapshotHeaderSize..],
                    header.QuitterBytes,
                    out var quitters,
                    out _))
            {
                return false;
            }

            quitterPlayerSlots = quitters;
            foreach (var slot in quitterPlayerSlots)
            {
                _echoApply.ResetClient(slot);
                _netRegistry.ResetClient(slot);
            }
        }

        if (!ServerSnapshotBodyCodec.TryReadPlayerRecords(
                nativePayload.Span[(LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes)..],
                header.ConsistencyTics,
                header.CommandTics,
                out players,
                out var hcsrBytes,
                out _))
        {
            return false;
        }

        ServerSnapshotApplySession.TryApply(
            header,
            quitterPlayerSlots,
            players,
            envelope.GameTic,
            _routing.ConsolePlayer,
            _routing,
            _netRegistry,
            _peerSlots,
            _snapshotCommandSink,
            (ulong)Environment.TickCount64,
            out _,
            out _);

        var bodyStart = LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes;
        var tail = nativePayload.Span[(bodyStart + hcsrBytes)..];
        if (tail.Length == 0)
            return true;

        if (!ServerSnapshotTailWalker.TryWalk(tail, out var sections, out _, out _))
            return false;

        tailSections = sections;
        TryApplyTailSections(sections, players);
        return true;
    }

    private void TryApplyTailSections(
        ServerSnapshotTailSections sections,
        IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        var sequenceAck = _netRegistry[_routing.ConsolePlayer].SequenceAck;

        var poses = sections.WorldDeltaPoses ?? Array.Empty<PlayerPoseWorldDelta>();
        var sectors = sections.WorldDeltaSectors ?? Array.Empty<SectorWorldDelta>();
        if (poses.Count > 0 || sectors.Count > 0)
        {
            WorldDeltaApplySession.TryApply(
                sections.WorldDelta,
                poses,
                sectors,
                SnapshotPlayerMask.Build(players),
                _routing.ConsolePlayer,
                sequenceAck,
                _worldDeltaSink,
                out _,
                out _);
        }

        if (sections.InvasionSnapshot is { } invasionHeader)
        {
            InvasionSnapshotApplySession.TryApply(
                invasionHeader,
                sections.AuthorityEventRecords,
                sections.ActorDelta,
                sections.ActorDeltaRecords,
                _negotiatedCapabilities,
                _routing.ConsolePlayer,
                _routing.IsLocalAuthority,
                _invasionSink,
                _authoritySink,
                _actorDeltaSink,
                out _,
                out _);
        }
        else
        {
            if (sections.ActorDeltaRecords is { Count: > 0 })
            {
                ActorDeltasApplySession.TryApply(
                    sections.ActorDelta,
                    sections.ActorDeltaRecords,
                    _routing.ConsolePlayer,
                    _actorDeltaSink,
                    out _,
                    out _);
            }

            if (sections.CoopDeadSpawnIndices is { Length: > 0 } deadSpawns && sections.CoopDeadSpawns is { } deadHeader)
            {
                CoopDeadSpawnsApplySession.TryApply(
                    deadHeader,
                    deadSpawns,
                    _coopDeadSpawnsSink,
                    out _,
                    out _);
            }

            if (sections.AuthorityEventRecords is { Length: > 0 } authorityRecords && _authoritySink != null)
            {
                AuthorityEventsApplySession.TryApply(
                    authorityRecords,
                    _authoritySink,
                    out _,
                    out _);
            }
        }

        if (sections.EchoBlock is { } echoBlock && _echoSink != null)
        {
            _echoApply.TryApply(
                _routing.ConsolePlayer,
                echoBlock,
                _echoSink,
                out _,
                out _);
        }

        TryComputeGuestWorldStateChecksum(sections);

        if (sections.HasChecksum
            && sections.ChecksumHashes is { Length: > 0 }
            && _checksumSession != null)
        {
            SnapshotChecksumApplySession.TryApply(
                sections.ChecksumGameTic,
                sections.ChecksumHashes,
                _checksumSession.Ring,
                checksumEnabled: true,
                SnapshotChecksumRing.DefaultEnabledCategoryMask,
                _checksumMismatchSink,
                out _,
                out _);
        }
    }

    private void TryComputeGuestWorldStateChecksum(ServerSnapshotTailSections sections)
    {
        if (_guestWorldState is null || _checksumSession is null)
            return;

        var gameTic = sections.HasChecksum
            ? (int)sections.ChecksumGameTic
            : (int)sections.WorldDelta.GameTic;
        if (gameTic < 0)
            return;

        SnapshotChecksumPlaysimInputs.ComputeAndStore(
            _checksumSession,
            _guestWorldState,
            gameTic,
            _guestWorldStateRngSeed);
    }

    public bool TryReceiveServerSnapshot(out ServerSnapshotHeader header, out IReadOnlyList<ServerSnapshotPlayerRecord> players)
    {
        var ok = TryReceiveServerSnapshot(out header, out players, out _);
        return ok;
    }
}

public sealed class LiveAuthoritySession
{
    private readonly LiveControlEndpoint _control;
    private readonly LiveGameplayEndpoint _gameplay;
    private readonly LivePeerRoutingState _routing;
    private readonly LiveAuthorityClientRegistry _clients = new();
    private readonly LivePeerNetRegistry _netRegistry;
    private IClientInputCommandSink? _clientInputCommandSink;
    private GuestWorldStateStore? _authorityWorldState;
    private SnapshotChecksumSession? _checksumSession;
    private int _authorityWorldStateRngSeed;
    private bool _replicateSectorMetadata;
    private byte _roomId;
    private uint _gameTic;

    public LiveAuthoritySession(
        UdpTransport transport,
        ReadOnlySpan<byte> gameId,
        int authoritySlot,
        int maxClients)
    {
        _control = new LiveControlEndpoint(transport, gameId);
        _gameplay = new LiveGameplayEndpoint(transport, gameId);
        _routing = new LivePeerRoutingState(
            consolePlayer: authoritySlot,
            maxClients: maxClients,
            authoritySlot: authoritySlot,
            isLocalAuthority: true,
            usesHcdeService: true);
        _netRegistry = new LivePeerNetRegistry(maxClients);
    }

    public LivePeerNetRegistry NetRegistry => _netRegistry;

    public void SetClientInputSink(IClientInputCommandSink? sink) => _clientInputCommandSink = sink;

    public void SetAuthorityWorldState(
        GuestWorldStateStore worldState,
        SnapshotChecksumSession checksumSession,
        int rngSeed = 0,
        bool replicateSectorMetadata = false)
    {
        _authorityWorldState = worldState;
        _checksumSession = checksumSession;
        _authorityWorldStateRngSeed = rngSeed;
        _replicateSectorMetadata = replicateSectorMetadata;
    }

    public LiveAuthorityClientRegistry Clients => _clients;

    public uint GameTic => _gameTic;

    public void TrackClient(NetworkEndpoint clientEndpoint, int clientSlot) =>
        _clients.Track(clientEndpoint, clientSlot);

    public bool UntrackClient(int clientSlot) => _clients.Remove(clientSlot);

    public void AdvanceTick(byte roomId = 0)
    {
        _roomId = roomId;
        _gameTic++;
    }

    public void PumpClient(ulong nowMs, NetworkEndpoint clientEndpoint, int clientSlot, byte roomId = 0)
    {
        AdvanceTick(roomId);
        SendToClient(nowMs, clientEndpoint, clientSlot);
    }

    public void PumpAllClients(ulong nowMs, byte roomId = 0)
    {
        AdvanceTick(roomId);
        foreach (var client in _clients.Clients)
            SendToClient(nowMs, client.Endpoint, client.ClientSlot);
    }

    public void Pump(ulong nowMs, byte roomId = 0)
    {
        foreach (var client in _clients.Clients)
            TryReceiveClientInput(client.Endpoint, out _, out _);

        PumpAllClients(nowMs, roomId);
    }

    public void SendToClient(ulong nowMs, NetworkEndpoint clientEndpoint, int clientSlot)
    {
        if (_routing.ShouldSendControlTo(clientSlot))
        {
            _control.TrySendControl(
                clientEndpoint,
                new LiveControlBasePayload(_gameTic, (byte)_routing.AuthoritySlot, (byte)_routing.MaxClients),
                new LiveControlCapabilities(LiveConstants.DefaultLocalCapabilities));
        }

        if (_routing.ShouldSendServerSnapshotTo(clientSlot))
        {
            uint[]? checksumHashes = null;
            if (_authorityWorldState is not null && _checksumSession is not null)
            {
                SnapshotChecksumPlaysimInputs.ComputeAndStore(
                    _checksumSession,
                    _authorityWorldState,
                    (int)_gameTic,
                    _authorityWorldStateRngSeed);
                _checksumSession.Ring.TryFind((int)_gameTic, out checksumHashes);
            }

            if (_authorityWorldState is not null
                && WorldStateTailBuilder.HasWorldDeltaPayload(_authorityWorldState))
            {
                Span<byte> tail = stackalloc byte[512];
                var tailWritten = WorldStateTailBuilder.WriteCoopTailFromStore(
                    tail,
                    _authorityWorldState,
                    _gameTic,
                    checksumHashes,
                    _replicateSectorMetadata);
                if (tailWritten > 0)
                {
                    _gameplay.TrySendServerSnapshotWithExternalTail(
                        clientEndpoint,
                        _roomId,
                        _gameTic,
                        playerNum: (byte)clientSlot,
                        externalTail: tail[..tailWritten]);
                    return;
                }
            }

            _gameplay.TrySendServerSnapshot(
                clientEndpoint,
                _roomId,
                _gameTic,
                playerNum: (byte)clientSlot,
                checksumHashes: checksumHashes);
        }
    }

    public bool TryReceiveClientInput(
        NetworkEndpoint clientEndpoint,
        out ClientInputHeader header,
        out IReadOnlyList<ClientInputPlayerRecord> players,
        int maxAttempts = 8)
    {
        header = default;
        players = Array.Empty<ClientInputPlayerRecord>();
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            if (!_gameplay.TryReceiveGameplay(
                    clientEndpoint,
                    GameplayPayloadKind.ClientInputs,
                    _roomId,
                    out var liveHeader,
                    out var envelope,
                    out var nativePayload))
            {
                continue;
            }

            if (liveHeader.MessageType != LiveMessageType.ClientCommands)
                continue;

            if (!ClientInputHeader.TryRead(nativePayload.Span, out header))
                continue;

            if (!ClientInputBodyCodec.TryRead(
                    nativePayload.Span[LiveConstants.ClientInputHeaderSize..],
                    header.ConsistencyTics,
                    header.CommandTics,
                    out players,
                    out _))
            {
                continue;
            }

            var clientSlot = FindClientSlot(clientEndpoint);
            if (clientSlot >= 0)
            {
                ClientInputApplySession.TryApply(
                    header,
                    players,
                    clientSlot,
                    _routing,
                    _netRegistry,
                    _clientInputCommandSink,
                    (int)envelope.GameTic,
                    out _,
                    out _);
            }

            return true;
        }

        return false;
    }

    private int FindClientSlot(NetworkEndpoint clientEndpoint)
    {
        foreach (var client in _clients.Clients)
        {
            if (client.Endpoint.Equals(clientEndpoint))
                return client.ClientSlot;
        }

        return -1;
    }
}
