using System.Text;

namespace HCDE.Net.Core.Tests;

public class InvasionSnapshotWavePolicyTests
{
    [Fact]
    public void ResolveWaveCounts_UsesIncomingWhenWaveChanges()
    {
        var previous = new InvasionMirrorState(
            LiveConstants.InvasionStateSpawning,
            wave: 1,
            waveSpawned: 10,
            waveCleared: 4);
        var incoming = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 1,
            wave: 2,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 3,
            waveCleared: 1,
            activeMonsters: 6);

        var (spawned, cleared) = InvasionSnapshotWavePolicy.ResolveWaveCounts(previous, incoming, isLocalAuthority: false);
        Assert.Equal(3u, spawned);
        Assert.Equal(1u, cleared);
    }

    [Fact]
    public void ResolveWaveCounts_MonotonicWhenSameActiveWave()
    {
        var previous = new InvasionMirrorState(
            LiveConstants.InvasionStateSpawning,
            wave: 2,
            waveSpawned: 10,
            waveCleared: 4);
        var incoming = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 2,
            wave: 2,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 6,
            waveCleared: 2,
            activeMonsters: 6);

        var (spawned, cleared) = InvasionSnapshotWavePolicy.ResolveWaveCounts(previous, incoming, isLocalAuthority: false);
        Assert.Equal(10u, spawned);
        Assert.Equal(4u, cleared);
    }
}

public class InvasionSnapshotApplySessionTests
{
    private sealed class RecordingInvasionSink : IInvasionSnapshotApplySink
    {
        public InvasionMirrorState MirrorState { get; private set; }

        public int ApplyCalls { get; private set; }

        public bool ApplyMirror(InvasionSnapshotHeader header, uint waveSpawned, uint waveCleared)
        {
            ApplyCalls++;
            MirrorState = new InvasionMirrorState(header.State, (int)header.Wave, waveSpawned, waveCleared);
            return true;
        }

        public bool ApplySpawnDirectory(InvasionSpawnDirectory directory)
        {
            SpawnDirectory = directory;
            return true;
        }

        public InvasionSpawnDirectory? SpawnDirectory { get; private set; }
    }

    private sealed class RecordingAuthoritySink : IAuthorityEventSink
    {
        public int Routed { get; private set; }

        public bool TryApplyInvasionSpawn(AuthorityEventRecord record)
        {
            Routed++;
            return true;
        }

        public bool TryApplyPickupSpawn(AuthorityEventRecord record) => false;
        public bool TryApplyInvasionDespawn(AuthorityEventRecord record) => false;
        public bool TryApplyPickupRetire(AuthorityEventRecord record) => false;
        public bool TryApplyInvasionDamage(AuthorityEventRecord record) => false;
        public bool TryApplyCoopProjectileSpawn(AuthorityEventRecord record) => false;
        public bool TryApplyCoopProjectileRetire(AuthorityEventRecord record) => false;
        public bool TryApplyCoopCosmeticSpawn(AuthorityEventRecord record) => false;
    }

    private sealed class RecordingActorSink : IActorDeltaApplySink
    {
        public int Applied { get; private set; }

        public bool TryApply(int recipientClientSlot, ActorDeltaRecord record)
        {
            Applied++;
            return true;
        }
    }

    [Fact]
    public void Apply_RejectsInvalidState()
    {
        var header = new InvasionSnapshotHeader(
            flags: 0,
            state: 99,
            stateTics: 0,
            wave: 0,
            maxWaves: 0,
            waveBudget: 0,
            waveSpawned: 0,
            waveCleared: 0,
            activeMonsters: 0);

        Assert.False(InvasionSnapshotApplySession.TryApply(
            header,
            embeddedAuthorityRecords: null,
            embeddedActorHeader: default,
            embeddedActorRecords: null,
            LiveConstants.DefaultLocalCapabilities,
            recipientClientSlot: 1,
            isLocalAuthority: false,
            invasionSink: null,
            authoritySink: null,
            actorSink: null,
            out _,
            out var rejectReason));

        Assert.Equal("invasion-snapshot-invalid-state", rejectReason);
    }

    [Fact]
    public void Apply_AppliesMirrorAndEmbeddedPayloads()
    {
        var invasionSink = new RecordingInvasionSink();
        var authoritySink = new RecordingAuthoritySink();
        var actorSink = new RecordingActorSink();
        var header = new InvasionSnapshotHeader(
            flags: LiveConstants.InvasionSnapshotFlagBossWave,
            state: LiveConstants.InvasionStateCountdown,
            stateTics: 35,
            wave: 3,
            maxWaves: 10,
            waveBudget: 12,
            waveSpawned: 4,
            waveCleared: 1,
            activeMonsters: 8,
            spawnSpotCount: 6,
            activeSpawnSpotCount: 2,
            spawnPlanBudget: 20,
            spawnActiveTag: 99,
            spawnFlags: LiveConstants.InvasionSnapshotSpawnFlagUsingFallback,
            spawnFallbackSource: LiveConstants.InvasionSpawnSourceMapSpot);

        var authorityRecord = new AuthorityEventRecord(
            AuthorityEventType.Spawn,
            ReplicatedActorSource.Invasion,
            ReplicatedActorCategory.Monster,
            actorFlags: 0,
            actorId: 12,
            eventTic: 7,
            classId: 3,
            health: 100,
            wave: 3,
            Encoding.UTF8.GetBytes("Imp"),
            posX: 1,
            posY: 2,
            posZ: 3,
            velX: 0,
            velY: 0,
            velZ: 0,
            yaw: 0,
            pitch: 0);

        var actorRecord = new ActorDeltaRecord
        {
            ActorId = 12,
            ClassId = 3,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Category = (byte)ReplicatedActorCategory.Monster,
            Health = 90,
        };

        Span<byte> actorChunk = stackalloc byte[64];
        var actorWritten = ActorDeltasCodec.Write(actorChunk, new[] { actorRecord });
        Assert.True(ActorDeltasCodec.TryRead(actorChunk[..actorWritten], out var actorHeader, out var actorRecords, out _, out _));

        Assert.True(InvasionSnapshotApplySession.TryApply(
            header,
            new[] { authorityRecord },
            actorHeader,
            actorRecords,
            LiveConstants.DefaultLocalCapabilities,
            recipientClientSlot: 1,
            isLocalAuthority: false,
            invasionSink,
            authoritySink,
            actorSink,
            out var result,
            out _));

        Assert.True(result.MirrorApplied);
        Assert.True(result.SpawnDirectoryApplied);
        Assert.Equal(1, result.AuthorityApplied);
        Assert.Equal(1, result.ActorApplied);
        Assert.Equal(1, invasionSink.ApplyCalls);
        Assert.Equal(1, authoritySink.Routed);
        Assert.Equal(1, actorSink.Applied);
        Assert.Equal(LiveConstants.InvasionStateCountdown, invasionSink.MirrorState.State);
        Assert.Equal(3, invasionSink.MirrorState.Wave);
        Assert.NotNull(invasionSink.SpawnDirectory);
        Assert.Equal(6, invasionSink.SpawnDirectory!.Value.TotalSpotCount);
        Assert.Equal(2, invasionSink.SpawnDirectory!.Value.ActiveSpotCount);
        Assert.Equal(4u, invasionSink.SpawnDirectory!.Value.SpawnedThisWave);
        Assert.True(invasionSink.SpawnDirectory!.Value.UsingFallback);
    }

    [Fact]
    public void Apply_RejectsActiveSpawnCountAboveTotal()
    {
        var header = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 1,
            wave: 1,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 1,
            waveCleared: 0,
            activeMonsters: 1,
            spawnSpotCount: 2,
            activeSpawnSpotCount: 5);

        Assert.False(InvasionSnapshotApplySession.TryApply(
            header,
            embeddedAuthorityRecords: null,
            embeddedActorHeader: default,
            embeddedActorRecords: null,
            LiveConstants.DefaultLocalCapabilities,
            recipientClientSlot: 0,
            isLocalAuthority: false,
            invasionSink: new RecordingInvasionSink(),
            authoritySink: null,
            actorSink: null,
            out _,
            out var rejectReason));

        Assert.Equal("invasion-spawn-active-count-overflow", rejectReason);
    }
}

public class InvasionSpawnDirectoryCodecTests
{
    [Fact]
    public void ParseFromHeader_BuildsV2Directory()
    {
        var header = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 2,
            wave: 4,
            maxWaves: 12,
            waveBudget: 10,
            waveSpawned: 7,
            waveCleared: 3,
            activeMonsters: 9,
            spawnSpotCount: 8,
            activeSpawnSpotCount: 3,
            spawnPlanBudget: 15,
            spawnActiveTag: 42,
            spawnFlags: LiveConstants.InvasionSnapshotSpawnFlagUsingFallback,
            spawnFallbackSource: LiveConstants.InvasionSpawnSourceDeathmatch);

        Assert.True(InvasionSpawnDirectoryCodec.TryParseFromHeader(header, spawnedThisWave: 7, out var directory, out _));
        Assert.Equal(8, directory.TotalSpotCount);
        Assert.Equal(3, directory.ActiveSpotCount);
        Assert.Equal(15u, directory.SpawnPlanBudget);
        Assert.Equal(42u, directory.ActiveTag);
        Assert.Equal(7u, directory.SpawnedThisWave);
        Assert.True(directory.UsingFallback);
        Assert.Equal(LiveConstants.InvasionSpawnSourceDeathmatch, directory.FallbackSource);
    }
}

public class SnapshotChecksumApplySessionTests
{
    private sealed class RecordingMismatchSink : ISnapshotChecksumMismatchSink
    {
        public List<SnapshotChecksumMismatch> Reported { get; } = new();

        public void ReportMismatch(SnapshotChecksumMismatch mismatch, uint remoteTic) => Reported.Add(mismatch);
    }

    [Fact]
    public void Apply_SkipsWhenLocalBucketMissing()
    {
        var ring = new SnapshotChecksumRing();
        var remoteHashes = new uint[] { 1, 2, 3, 4, 5, 6 };

        Assert.True(SnapshotChecksumApplySession.TryApply(
            remoteTic: 99,
            remoteHashes,
            ring,
            checksumEnabled: true,
            SnapshotChecksumRing.DefaultEnabledCategoryMask,
            mismatchSink: null,
            out var result,
            out _));

        Assert.False(result.Compared);
        Assert.True(result.LocalBucketMissing);
        Assert.Equal(0, result.MismatchCount);
    }

    [Fact]
    public void Apply_ReportsCategoryMismatch()
    {
        var ring = new SnapshotChecksumRing();
        var localHashes = new uint[] { 10, 20, 30, 40, 50, 60 };
        ring.Store(50, localHashes);
        var remoteHashes = localHashes.ToArray();
        remoteHashes[(int)SnapshotChecksumCategory.Actors] = 999;

        var sink = new RecordingMismatchSink();
        Assert.True(SnapshotChecksumApplySession.TryApply(
            remoteTic: 50,
            remoteHashes,
            ring,
            checksumEnabled: true,
            SnapshotChecksumRing.DefaultEnabledCategoryMask,
            sink,
            out var result,
            out _));

        Assert.True(result.Compared);
        Assert.Equal(1, result.MismatchCount);
        Assert.Single(sink.Reported);
        Assert.Equal(SnapshotChecksumCategory.Actors, sink.Reported[0].Category);
        Assert.Equal(999u, sink.Reported[0].ServerHash);
        Assert.Equal(40u, sink.Reported[0].LocalHash);
    }
}

public class InvasionSnapshotTailParsedBlocksTests
{
    [Fact]
    public void Walker_ExposesEmbeddedAuthorityAndActorBlocks()
    {
        var invasionHeader = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 3,
            wave: 2,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 4,
            waveCleared: 1,
            activeMonsters: 6);

        var authorityRecord = new AuthorityEventRecord(
            AuthorityEventType.Spawn,
            ReplicatedActorSource.Invasion,
            ReplicatedActorCategory.Monster,
            actorFlags: 0,
            actorId: 5,
            eventTic: 2,
            classId: 1,
            health: 50,
            wave: 2,
            Encoding.UTF8.GetBytes("Demon"),
            posX: 0,
            posY: 0,
            posZ: 0,
            velX: 0,
            velY: 0,
            velZ: 0,
            yaw: 0,
            pitch: 0);

        var actorRecord = new ActorDeltaRecord
        {
            ActorId = 5,
            ClassId = 1,
            FieldMask = LiveConstants.ActorDeltaFieldCategory,
            Category = (byte)ReplicatedActorCategory.Monster,
        };

        Span<byte> tail = stackalloc byte[512];
        var cursor = 0;
        cursor += WorldDeltaChunkCodec.WriteEmpty(tail[cursor..], gameTic: 5);
        cursor += InvasionSnapshotHeader.Write(tail[cursor..], invasionHeader);
        cursor += AuthorityEventsCodec.Write(tail[cursor..], new[] { authorityRecord });
        cursor += ActorDeltasCodec.Write(tail[cursor..], new[] { actorRecord });
        cursor += PresentationEchoCodec.WriteMinimal(tail[cursor..]);

        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..cursor], out var sections, out var consumed, out _));
        Assert.Equal(cursor, consumed);
        Assert.NotNull(sections.InvasionSnapshot);
        Assert.NotNull(sections.AuthorityEventRecords);
        Assert.Single(sections.AuthorityEventRecords!);
        Assert.NotNull(sections.ActorDeltaRecords);
        Assert.Single(sections.ActorDeltaRecords!);
        Assert.Equal(5u, sections.ActorDeltaRecords![0].ActorId);
    }
}
