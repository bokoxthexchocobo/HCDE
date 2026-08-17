namespace HCDE.Net.Core.Tests;

public class WorldStateTailBuilderTests
{
    [Fact]
    public void WriteCoopTailFromStore_IncludesPlayerAndSectorDeltas()
    {
        var store = new GuestWorldStateStore();
        store.ApplyPose(
            1,
            new PlayerPoseWorldDelta(
                1,
                LiveConstants.ServerWorldDeltaPoseHasActor | LiveConstants.ServerWorldDeltaPoseOnGround,
                health: 55,
                armor: 0,
                posX: 0,
                posY: 0,
                posZ: 0,
                velX: 0,
                velY: 0,
                velZ: 0,
                yawBams: 0,
                pitchBams: 0),
            sequenceAck: 0);
        store.ApplySector(new SectorWorldDelta(2, flags: 0, floor: 16, ceiling: 96));

        Span<byte> tail = stackalloc byte[512];
        var written = WorldStateTailBuilder.WriteCoopTailFromStore(tail, store, gameTic: 9);
        Assert.True(written > ServerSnapshotTailCodec.MinimalTailSize);
        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..written], out var sections, out _, out _));
        Assert.Single(sections.WorldDeltaPoses!);
        Assert.Single(sections.WorldDeltaSectors!);
        Assert.Equal(55, sections.WorldDeltaPoses![0].Health);
    }

    [Fact]
    public void TryBuildCoopTailFromStore_ReturnsBytesWrittenWhenStoreHasSectors()
    {
        var store = new GuestWorldStateStore();
        store.ApplySector(new SectorWorldDelta(0, flags: 0, floor: 0, ceiling: 128, lightLevel: 160, special: 0));

        Span<byte> tail = stackalloc byte[512];
        var build = WorldStateTailBuilder.TryBuildCoopTailFromStore(tail, store, gameTic: 3);
        Assert.True(build.HasTail);
        Assert.True(build.BytesWritten > 0);
        Assert.Equal(build.BytesWritten, WorldStateTailBuilder.WriteCoopTailFromStore(tail, store, gameTic: 3));
    }

    [Fact]
    public void WriteCoopTailFromStore_IncludesPendingCoopDeadSpawns()
    {
        var store = new GuestWorldStateStore();
        store.QueueCoopDeadSpawn(42);
        store.QueueCoopDeadSpawn(99);

        Span<byte> tail = stackalloc byte[512];
        var written = WorldStateTailBuilder.WriteCoopTailFromStore(tail, store, gameTic: 4);
        Assert.True(written > 0);
        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..written], out var sections, out _, out _));
        Assert.NotNull(sections.CoopDeadSpawnIndices);
        Assert.Equal(new uint[] { 42, 99 }, sections.CoopDeadSpawnIndices);
        Assert.False(store.HasPendingCoopDeadSpawns);
    }

    [Fact]
    public void TryBuildInvasionTail_WritesHcivBeforeEcho()
    {
        var invasionHeader = new InvasionSnapshotHeader(
            flags: 0,
            state: LiveConstants.InvasionStateSpawning,
            stateTics: 2,
            wave: 3,
            maxWaves: 10,
            waveBudget: 8,
            waveSpawned: 4,
            waveCleared: 1,
            activeMonsters: 6);

        Span<byte> tail = stackalloc byte[256];
        var build = WorldStateTailBuilder.TryBuildInvasionTail(tail, gameTic: 7, invasionHeader);
        Assert.True(build.HasTail);
        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..build.BytesWritten], out var sections, out _, out _));
        Assert.NotNull(sections.InvasionSnapshot);
        Assert.Equal(3u, sections.InvasionSnapshot!.Value.Wave);
        Assert.NotNull(sections.EchoBlock);
    }
}
