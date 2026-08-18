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
    private GuestPresentationEchoState? _presentationEchoState;
    private GuestInvasionState? _invasionState;
    private GuestAuthorityEventState? _authorityEventState;
    private int _guestWorldStateRngSeed;
    private SnapshotChecksumMismatchPolicyKind _checksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ReportAllCompared;
    private GuestChecksumApplyState _lastChecksumApplyState;
    private bool _needsChecksumResync;
    private bool _needsNetGapResync;
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

    public GuestChecksumApplyState LastChecksumApplyState => _lastChecksumApplyState;

    public SnapshotChecksumMismatchPolicyKind ChecksumMismatchPolicy
    {
        get => _checksumMismatchPolicy;
        set => _checksumMismatchPolicy = value;
    }

    public bool LastChecksumApplyValid =>
        SnapshotChecksumMismatchPolicy.ShouldTreatAsValid(
            new SnapshotChecksumApplyResult(
                _lastChecksumApplyState.Compared,
                _lastChecksumApplyState.MismatchCount,
                _lastChecksumApplyState.LocalBucketMissing),
            _checksumMismatchPolicy);

    public bool NeedsChecksumResync => _needsChecksumResync;

    public bool NeedsNetGapResync => _needsNetGapResync;

    public void SetNegotiatedCapabilities(ulong negotiatedCapabilities) =>
        _negotiatedCapabilities = negotiatedCapabilities;

    public void SetChecksumSession(
        SnapshotChecksumSession? checksumSession,
        ISnapshotChecksumMismatchSink? mismatchSink = null)
    {
        _checksumSession = checksumSession;
        _checksumMismatchSink = mismatchSink;
    }

    public GuestPresentationEchoState? PresentationEchoState => _presentationEchoState;

    public GuestInvasionState? InvasionState => _invasionState;

    public GuestAuthorityEventState? AuthorityEventState => _authorityEventState;

    public void SetGuestWorldState(
        GuestWorldStateStore worldState,
        SnapshotChecksumSession checksumSession,
        int rngSeed = 0,
        ISnapshotChecksumMismatchSink? mismatchSink = null,
        GuestPresentationEchoState? presentationEchoState = null,
        GuestInvasionState? invasionState = null,
        GuestAuthorityEventState? authorityEventState = null)
    {
        _guestWorldState = worldState;
        _guestWorldStateRngSeed = rngSeed;
        SetChecksumSession(checksumSession, mismatchSink);
        _worldDeltaSink = worldState;
        _actorDeltaSink = worldState;
        _coopDeadSpawnsSink = worldState;
        _presentationEchoState = presentationEchoState ?? new GuestPresentationEchoState();
        _echoSink = _presentationEchoState;
        _invasionState = invasionState ?? new GuestInvasionState();
        _invasionSink = _invasionState;
        _authorityEventState = authorityEventState ?? new GuestAuthorityEventState();
        _authoritySink = _authorityEventState;
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
            out var applyResult,
            out _);

        if (applyResult.SnapshotGapResynced)
        {
            _needsNetGapResync = true;
            _netRegistry.ResetClient(_routing.ConsolePlayer);
        }

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

        var appliedInvasionCoopDeadSpawns = false;
        var appliedInvasionAuthorityEvents = false;
        var appliedInvasionActorDeltas = false;
        var appliedInvasionPresentationEcho = false;
        var appliedInvasionLineSpec = false;
        var appliedCoopLineSpec = false;
        var appliedPresentationEcho = false;
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

            if (sections.CoopDeadSpawnIndices is { Length: > 0 } invasionDeadSpawns
                && sections.CoopDeadSpawns is { } invasionDeadHeader)
            {
                CoopDeadSpawnsApplySession.TryApply(
                    invasionDeadHeader,
                    invasionDeadSpawns,
                    _coopDeadSpawnsSink,
                    out _,
                    out _);
                appliedInvasionCoopDeadSpawns = true;
            }

            if (sections.AuthorityEventRecords is { Length: > 0 } invasionAuthorityRecords)
            {
                appliedInvasionAuthorityEvents = true;
                _guestWorldState?.CommitAppliedAuthorityEvents(invasionAuthorityRecords);
            }

            if (sections.ActorDeltaRecords is { Count: > 0 } invasionActorRecords)
            {
                appliedInvasionActorDeltas = true;
                _guestWorldState?.CommitAppliedActorDeltas(invasionActorRecords);
            }

            if (_guestWorldState is { LineSpecRollingHash: not 0 })
                appliedInvasionLineSpec = true;
        }
        else
        {
            if (sections.ActorDeltaRecords is { Count: > 0 } actorRecords)
            {
                ActorDeltasApplySession.TryApply(
                    sections.ActorDelta,
                    actorRecords,
                    _routing.ConsolePlayer,
                    _actorDeltaSink,
                    out _,
                    out _);

                _guestWorldState?.CommitAppliedActorDeltas(actorRecords);
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

                _guestWorldState?.CommitAppliedAuthorityEvents(authorityRecords);
            }

            if (_guestWorldState is { LineSpecRollingHash: not 0 })
                appliedCoopLineSpec = true;
        }

        if (sections.EchoBlock is { } echoBlock && _echoSink != null)
        {
            _echoApply.TryApply(
                _routing.ConsolePlayer,
                echoBlock,
                _echoSink,
                out _,
                out _);

            _guestWorldState?.CommitAppliedPresentationEcho(echoBlock);
            appliedPresentationEcho = true;
            if (sections.InvasionSnapshot is not null)
                appliedInvasionPresentationEcho = true;
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
                out var checksumResult,
                out _);
            _lastChecksumApplyState = new GuestChecksumApplyState(checksumResult);
            _needsChecksumResync = SnapshotChecksumMismatchPolicy.ShouldResyncNetState(
                checksumResult,
                _checksumMismatchPolicy);
            if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopDeadSpawnMismatch(
                    checksumResult,
                    _checksumMismatchPolicy))
            {
                _needsNetGapResync = true;
            }

            if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnLineSpecMismatch(
                    checksumResult,
                    _checksumMismatchPolicy))
            {
                _needsNetGapResync = true;
            }

            if (_needsChecksumResync)
                _netRegistry.ResetClient(_routing.ConsolePlayer);
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionCoopDeadSpawnApply(
                appliedInvasionCoopDeadSpawns,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionAuthorityEventApply(
                appliedInvasionAuthorityEvents,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionActorDeltaApply(
                appliedInvasionActorDeltas,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionPresentationEchoApply(
                appliedInvasionPresentationEcho,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnPresentationEchoApply(
                appliedPresentationEcho,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecApply(
                appliedInvasionLineSpec,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecApply(
                appliedCoopLineSpec,
                sections.HasChecksum,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (_lastChecksumApplyState.Compared
            && SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnInvasionLineSpecMismatch(
                new SnapshotChecksumApplyResult(
                    _lastChecksumApplyState.Compared,
                    _lastChecksumApplyState.MismatchCount,
                    _lastChecksumApplyState.LocalBucketMissing,
                    hasActorCategoryMismatch: _lastChecksumApplyState.HasActorCategoryMismatch,
                    hasLineSpecCategoryMismatch: _lastChecksumApplyState.HasLineSpecCategoryMismatch),
                appliedInvasionLineSpec,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
        }

        if (_lastChecksumApplyState.Compared
            && SnapshotChecksumMismatchPolicy.ShouldTriggerNetGapResyncOnCoopLineSpecMismatch(
                new SnapshotChecksumApplyResult(
                    _lastChecksumApplyState.Compared,
                    _lastChecksumApplyState.MismatchCount,
                    _lastChecksumApplyState.LocalBucketMissing,
                    hasActorCategoryMismatch: _lastChecksumApplyState.HasActorCategoryMismatch,
                    hasLineSpecCategoryMismatch: _lastChecksumApplyState.HasLineSpecCategoryMismatch),
                appliedCoopLineSpec,
                _checksumMismatchPolicy))
        {
            _needsNetGapResync = true;
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
    private InvasionSnapshotHeader? _authorityInvasionSnapshot;
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

    public void SetAuthorityInvasionSnapshot(InvasionSnapshotHeader? invasionSnapshot) =>
        _authorityInvasionSnapshot = invasionSnapshot;

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
            var checksumHashes = SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(
                _authorityWorldState,
                _checksumSession,
                (int)_gameTic,
                _authorityWorldStateRngSeed);

            if (_authorityInvasionSnapshot is { } invasionSnapshot)
            {
                Span<byte> tail = stackalloc byte[512];
                var tailBuild = WorldStateTailMergePolicy.ShouldMergeCoopIntoInvasion(
                        invasionSnapshot,
                        _authorityWorldState)
                    ? WorldStateTailBuilder.TryBuildMergedInvasionCoopTail(
                        tail,
                        _authorityWorldState!,
                        _checksumSession,
                        _gameTic,
                        invasionSnapshot,
                        _authorityWorldStateRngSeed,
                        _replicateSectorMetadata)
                    : WorldStateTailBuilder.TryBuildInvasionTailWithChecksum(
                        tail,
                        _authorityWorldState,
                        _checksumSession,
                        _gameTic,
                        invasionSnapshot,
                        _authorityWorldStateRngSeed);
                if (tailBuild.HasTail)
                {
                    _gameplay.TrySendServerSnapshotWithExternalTail(
                        clientEndpoint,
                        _roomId,
                        _gameTic,
                        playerNum: (byte)clientSlot,
                        externalTail: tail[..tailBuild.BytesWritten]);
                    return;
                }
            }

            if (_authorityWorldState is not null)
            {
                Span<byte> tail = stackalloc byte[512];
                var tailBuild = WorldStateTailBuilder.TryBuildCoopTailFromStore(
                    tail,
                    _authorityWorldState,
                    _gameTic,
                    checksumHashes,
                    _replicateSectorMetadata);
                if (tailBuild.HasTail)
                {
                    _gameplay.TrySendServerSnapshotWithExternalTail(
                        clientEndpoint,
                        _roomId,
                        _gameTic,
                        playerNum: (byte)clientSlot,
                        externalTail: tail[..tailBuild.BytesWritten]);
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
                    out var applyResult,
                    out _);

                if (applyResult.InputGapResynced)
                    _netRegistry.ResetClient(clientSlot);
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
