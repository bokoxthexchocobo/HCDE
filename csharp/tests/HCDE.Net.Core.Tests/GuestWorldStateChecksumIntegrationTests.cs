namespace HCDE.Net.Core.Tests;

public class GuestWorldStateChecksumIntegrationTests
{
    private sealed class RecordingMismatchSink : ISnapshotChecksumMismatchSink
    {
        public List<SnapshotChecksumMismatch> Reported { get; } = new();

        public void ReportMismatch(SnapshotChecksumMismatch mismatch, uint remoteTic) => Reported.Add(mismatch);
    }

    [Fact]
    public void GuestReceive_AppliesTailToWorldStoreAndMatchesChecksum()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 7;
        const int rngSeed = 3;

        var pose = new PlayerPoseWorldDelta(
            playerNum: 1,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor,
            health: 90,
            armor: 0,
            posX: 1,
            posY: 2,
            posZ: 3,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0,
            pitchBams: 0);
        var actor = new ActorDeltaRecord
        {
            ActorId = 5,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 80,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.ApplyPose(1, pose, sequenceAck: 0);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: new[] { pose },
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var mismatchSink = new RecordingMismatchSink();
        var guestStore = new GuestWorldStateStore();
        var guestSession = new SnapshotChecksumSession();
        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed, mismatchSink);

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out var tailSections));
        Assert.NotNull(tailSections);
        Assert.True(guestStore.Players.ContainsKey(1));
        Assert.True(guestStore.Actors.ContainsKey(5u));
        Assert.True(guestSession.Ring.TryFind(gameTic, out var localHashes));
        Assert.Equal(remoteHashes, localHashes);
        Assert.Empty(mismatchSink.Reported);
    }

    [Fact]
    public void GuestReceive_ReportsChecksumMismatchWhenHashesDiffer()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 7;
        const int rngSeed = 3;

        var pose = new PlayerPoseWorldDelta(
            playerNum: 1,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor,
            health: 90,
            armor: 0,
            posX: 0,
            posY: 0,
            posZ: 0,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0,
            pitchBams: 0);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.ApplyPose(1, pose, sequenceAck: 0);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[0] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: new[] { pose },
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var mismatchSink = new RecordingMismatchSink();
        var guestStore = new GuestWorldStateStore();
        guestStore.ApplyPose(1, pose, sequenceAck: 0);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed, mismatchSink);

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.Compared);
        Assert.True(guest.LastChecksumApplyState.MismatchCount > 0);
        Assert.False(guest.LastChecksumApplyValid);
        Assert.NotEmpty(mismatchSink.Reported);
    }

    [Fact]
    public void GuestReceive_ResetsNetStateOnChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 7;
        const int rngSeed = 3;

        var pose = new PlayerPoseWorldDelta(
            playerNum: 1,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor,
            health: 90,
            armor: 0,
            posX: 0,
            posY: 0,
            posZ: 0,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0,
            pitchBams: 0);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.ApplyPose(1, pose, sequenceAck: 0);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[0] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: new[] { pose },
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestStore = new GuestWorldStateStore();
        guestStore.ApplyPose(1, pose, sequenceAck: 0);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.NeedsChecksumResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnActorChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 7;
        const int rngSeed = 3;

        var pose = new PlayerPoseWorldDelta(
            playerNum: 1,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor,
            health: 90,
            armor: 0,
            posX: 0,
            posY: 0,
            posZ: 0,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0,
            pitchBams: 0);
        var actor = new ActorDeltaRecord
        {
            ActorId = 5,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 80,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.ApplyPose(1, pose, sequenceAck: 0);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: new[] { pose },
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.ApplyPose(1, pose, sequenceAck: 0);
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 9;
        const int rngSeed = 5;

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.True(guest.NeedsNetGapResync);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionActorDeltaChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 11;
        const int rngSeed = 7;

        var actor = new ActorDeltaRecord
        {
            ActorId = 44,
            ClassId = 7,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 55,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionActorDeltaLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 12;
        const int rngSeed = 8;

        var actor = new ActorDeltaRecord
        {
            ActorId = 45,
            ClassId = 6,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 60,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionActorDeltaPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 26;
        const int rngSeed = 24;

        var actor = new ActorDeltaRecord
        {
            ActorId = 48,
            ClassId = 6,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 61,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopActorDeltaLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 22;
        const int rngSeed = 20;

        var actor = new ActorDeltaRecord
        {
            ActorId = 46,
            ClassId = 7,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 62,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopActorDeltaPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 25;
        const int rngSeed = 23;

        var actor = new ActorDeltaRecord
        {
            ActorId = 47,
            ClassId = 7,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 63,
        };

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        authorityStore.TryApply(1, actor);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 3, special: 6, success: true);
        guestStore.TryApply(1, actor);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 13;
        const int rngSeed = 11;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 55);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 23;
        const int rngSeed = 21;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 55);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 24;
        const int rngSeed = 22;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 56);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionPresentationEchoChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 17;
        const int rngSeed = 15;

        var echoBlock = PresentationEchoCodec.CreateExampleBlock();
        var authorityStore = new GuestWorldStateStore();
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 18;
        const int rngSeed = 16;

        var echoBlock = PresentationEchoCodec.CreateExampleBlock();
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 5, special: 8, success: true);
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 5, special: 8, success: true);
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionLineSpecActorChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 19;
        const int rngSeed = 17;

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopLineSpecActorChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 21;
        const int rngSeed = 19;

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 4, special: 7, success: true);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopPresentationEchoChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 19;
        const int rngSeed = 17;

        var echoBlock = PresentationEchoCodec.CreateExampleBlock();
        var authorityStore = new GuestWorldStateStore();
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 20;
        const int rngSeed = 18;

        var echoBlock = PresentationEchoCodec.CreateExampleBlock();
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 7, special: 10, success: true);
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: default,
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 7, special: 10, success: true);
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 15;
        const int rngSeed = 13;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 77);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 16;
        const int rngSeed = 14;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 88);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 23;
        const int rngSeed = 21;

        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 90);
        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: ReadOnlySpan<ActorDeltaRecord>.Empty,
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 27;
        const int rngSeed = 25;

        var actor = new ActorDeltaRecord
        {
            ActorId = 49,
            ClassId = 7,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 64,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 91);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 6, special: 9, success: true);
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 31;
        const int rngSeed = 29;

        var actor = new ActorDeltaRecord
        {
            ActorId = 53,
            ClassId = 11,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 76,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 95);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 32;
        const int rngSeed = 30;

        var actor = new ActorDeltaRecord
        {
            ActorId = 54,
            ClassId = 12,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 79,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 96);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 30;
        const int rngSeed = 28;

        var actor = new ActorDeltaRecord
        {
            ActorId = 52,
            ClassId = 10,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 73,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 94);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 9, special: 12, success: true);
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 9, special: 12, success: true);
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 28;
        const int rngSeed = 26;

        var actor = new ActorDeltaRecord
        {
            ActorId = 50,
            ClassId = 8,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 67,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 92);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 7, special: 10, success: true);
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 7, special: 10, success: true);
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.SetNegotiatedCapabilities(LiveConstants.DefaultLocalCapabilities);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 29;
        const int rngSeed = 27;

        var actor = new ActorDeltaRecord
        {
            ActorId = 51,
            ClassId = 9,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 70,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 93);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 8, special: 11, success: true);
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 8, special: 11, success: true);
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 33;
        const int rngSeed = 31;

        var actor = new ActorDeltaRecord
        {
            ActorId = 55,
            ClassId = 13,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 82,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 97);
        var echoBlock = PresentationEchoCodec.CreateExampleBlock();

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: ReadOnlySpan<uint>.Empty,
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaCoopDeadSpawnChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 35;
        const int rngSeed = 33;
        const uint deadSpawnIndex = 77;

        var actor = new ActorDeltaRecord
        {
            ActorId = 57,
            ClassId = 15,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 88,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 99);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.TryRetireSpawnIndex(deadSpawnIndex);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { deadSpawnIndex },
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
        Assert.Contains(deadSpawnIndex, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 37;
        const int rngSeed = 35;
        const uint deadSpawnIndex = 79;

        var actor = new ActorDeltaRecord
        {
            ActorId = 59,
            ClassId = 17,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 93,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 101);
        var echoBlock = PresentationEchoCodec.CreateExampleBlock();

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        authorityStore.TryRetireSpawnIndex(deadSpawnIndex);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { deadSpawnIndex },
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
        Assert.Contains(deadSpawnIndex, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnCoopAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnLineSpecChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 39;
        const int rngSeed = 37;
        const uint deadSpawnIndex = 91;

        var actor = new ActorDeltaRecord
        {
            ActorId = 61,
            ClassId = 19,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 99,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 103);
        var echoBlock = PresentationEchoCodec.CreateExampleBlock();

        var authorityStore = new GuestWorldStateStore();
        authorityStore.NoteLineSpec(lineIndex: 9, special: 12, success: true);
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        authorityStore.TryRetireSpawnIndex(deadSpawnIndex);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.LineSpec] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: (uint)gameTic,
            poses: ReadOnlySpan<PlayerPoseWorldDelta>.Empty,
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { deadSpawnIndex },
            authorityEvents: new[] { authorityRecord },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.NoteLineSpec(lineIndex: 9, special: 12, success: true);
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasLineSpecCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
        Assert.Contains(deadSpawnIndex, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoCoopDeadSpawnChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 38;
        const int rngSeed = 36;
        const uint deadSpawnIndex = 90;

        var actor = new ActorDeltaRecord
        {
            ActorId = 60,
            ClassId = 18,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 96,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 102);
        var echoBlock = PresentationEchoCodec.CreateExampleBlock();

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        authorityStore.TryRetireSpawnIndex(deadSpawnIndex);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { deadSpawnIndex },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
        Assert.Contains(deadSpawnIndex, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaCoopDeadSpawnChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 36;
        const int rngSeed = 34;
        const uint deadSpawnIndex = 88;

        var actor = new ActorDeltaRecord
        {
            ActorId = 58,
            ClassId = 16,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 91,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 100);

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.TryRetireSpawnIndex(deadSpawnIndex);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { deadSpawnIndex },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
        Assert.Contains(deadSpawnIndex, guestStore.RetiredCoopDeadSpawns);
    }

    [Fact]
    public void GuestReceive_TriggersNetGapResyncOnInvasionAuthorityEventActorDeltaPresentationEchoChecksumMismatchWhenPolicySet()
    {
        var gameId = new byte[] { 0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44 };
        const int gameTic = 34;
        const int rngSeed = 32;

        var actor = new ActorDeltaRecord
        {
            ActorId = 56,
            ClassId = 14,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 85,
        };
        var authorityRecord = AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 98);
        var echoBlock = PresentationEchoCodec.CreateExampleBlock();

        var authorityStore = new GuestWorldStateStore();
        authorityStore.TryApply(1, actor);
        authorityStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        authorityStore.CommitAppliedPresentationEcho(echoBlock);
        var authoritySession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(authoritySession, authorityStore, gameTic, rngSeed);
        Assert.True(authoritySession.Ring.TryFind(gameTic, out var remoteHashes));
        remoteHashes[(int)SnapshotChecksumCategory.Actors] ^= 0xFFFF;

        Span<byte> tail = stackalloc byte[512];
        var tailWritten = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic: (uint)gameTic,
            new InvasionSnapshotHeader(
                flags: 0,
                state: LiveConstants.InvasionStateSpawning,
                stateTics: 1,
                wave: 1,
                maxWaves: 10,
                waveBudget: 8,
                waveSpawned: 0,
                waveCleared: 0,
                activeMonsters: 2),
            embeddedAuthorityEvents: new[] { authorityRecord },
            embeddedActorDeltas: new[] { actor },
            checksumHashes: remoteHashes);

        using var authorityTransport = new HCDE.Net.Transport.UdpTransport();
        using var guestTransport = new HCDE.Net.Transport.UdpTransport();
        authorityTransport.Bind(0);
        guestTransport.Bind(0);
        authorityTransport.SetNonBlocking(true);
        guestTransport.SetNonBlocking(true);

        var authorityEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, authorityTransport.BoundPort);
        var guestEndpoint = new HCDE.Net.Transport.NetworkEndpoint(System.Net.IPAddress.Loopback, guestTransport.BoundPort);

        var guestStore = new GuestWorldStateStore();
        guestStore.TryApply(1, actor);
        guestStore.CommitAppliedAuthorityEvents(new[] { authorityRecord });
        guestStore.CommitAppliedPresentationEcho(echoBlock);
        var guestSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(guestSession, guestStore, gameTic, rngSeed);

        var guest = new LiveGuestSession(guestTransport, gameId, authorityEndpoint, guestPlayerSlot: 1, authoritySlot: 0, maxClients: 4);
        guest.ChecksumMismatchPolicy = SnapshotChecksumMismatchPolicyKind.ResyncNetStateOnMismatch;
        guest.SetGuestWorldState(guestStore, guestSession, rngSeed);
        guest.NetRegistry.GetOrCreate(1).CurrentSequence = 42;

        var gameplay = new LiveGameplayEndpoint(authorityTransport, gameId);
        Assert.True(gameplay.TrySendServerSnapshotWithExternalTail(
            guestEndpoint,
            roomId: 0,
            gameTic: (uint)gameTic,
            playerNum: 1,
            externalTail: tail[..tailWritten]));

        Assert.True(guest.TryReceiveServerSnapshot(out _, out _, out _));
        Assert.True(guest.LastChecksumApplyState.HasActorCategoryMismatch);
        Assert.True(guest.NeedsChecksumResync);
        Assert.True(guest.NeedsNetGapResync);
        Assert.Equal(0, guest.NetRegistry.GetOrCreate(1).CurrentSequence);
    }

    [Fact]
    public void GuestReceive_AppliesSectorOnlyWorldDelta()
    {
        var store = new GuestWorldStateStore();
        var header = new ServerWorldDeltaHeader(flags: 0, gameTic: 4, recordCount: 0);
        var sectors = new[] { new SectorWorldDelta(sectorIndex: 9, flags: 0, floor: 32, ceiling: 128) };

        Assert.True(WorldDeltaApplySession.TryApply(
            header,
            Array.Empty<PlayerPoseWorldDelta>(),
            sectors,
            snapshotPlayersMask: 0,
            recipientClientSlot: 1,
            sequenceAck: 0,
            store,
            out var result,
            out _));

        Assert.Equal(1, result.SectorsApplied);
        Assert.True(store.Sectors.TryGetValue(9, out var sector));
        Assert.Equal(32, sector.Floor);
        Assert.Equal(128, sector.Ceiling);
    }
}
