namespace HCDE.Net.Core.Tests;

public class SnapshotWriteFailureTests
{
    private static InvasionSnapshotHeader Invasion => new(0, LiveConstants.InvasionStateSpawning, 1, 1, 10, 8, 1, 0, 1);
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FailedBuildPreservesEventsRetirementsAndHashesForRetry(int mode)
    {
        var store = CreateStore();
        var fresh = CreateStore();
        var session = new SnapshotChecksumSession();
        Assert.Equal(0, Build(mode, new byte[1], store, session));
        Assert.True(store.HasPendingAuthorityEvents);
        Assert.True(store.HasPendingCoopDeadSpawns);
        Assert.Empty(store.RetiredCoopDeadSpawns);
        Assert.Equal(0u, store.ActorDeltaRollingHash);
        Assert.Equal(0u, store.AuthorityEventRollingHash);
        Assert.False(session.Ring.TryFind(42, out _));

        var retry = new byte[4096];
        var expected = new byte[4096];
        var written = Build(mode, retry, store, session);
        var expectedWritten = Build(mode, expected, fresh, new SnapshotChecksumSession());
        Assert.True(written > 0);
        Assert.Equal(expectedWritten, written);
        Assert.Equal(expected[..written], retry[..written]);
        Assert.False(store.HasPendingAuthorityEvents);
        Assert.False(store.HasPendingCoopDeadSpawns);
        Assert.Equal(fresh.ActorDeltaRollingHash, store.ActorDeltaRollingHash);
        Assert.Equal(fresh.AuthorityEventRollingHash, store.AuthorityEventRollingHash);
        Assert.True(ServerSnapshotTailWalker.TryWalk(retry[..written], out var sections, out _, out var error), error);
        Assert.Single(sections.AuthorityEventRecords!);
        Assert.Equal(new uint[] { 17 }, sections.CoopDeadSpawnIndices);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void OversizedActorCountDoesNotConsumePendingRecords(int mode)
    {
        var store = CreateStore();
        for (uint id = 1; id <= 256; id++) store.SeedActor(id, 3001);
        var session = new SnapshotChecksumSession();
        Assert.Equal(0, Build(mode, new byte[65536], store, session));
        Assert.True(store.HasPendingAuthorityEvents);
        Assert.True(store.HasPendingCoopDeadSpawns);
        Assert.Equal(0u, store.ActorDeltaRollingHash);
        Assert.Equal(0u, store.AuthorityEventRollingHash);
        Assert.False(session.Ring.TryFind(42, out _));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WorldCountsCannotWrap(bool players)
    {
        var buffer = Enumerable.Repeat((byte)0xCC, 65536).ToArray();
        var poses = new PlayerPoseWorldDelta[players ? 256 : 0];
        var sectors = new SectorWorldDelta[players ? 0 : 256];
        Assert.Equal(0, WorldDeltaChunkCodec.Write(buffer, 0, 1, poses, sectors));
        Assert.All(buffer, value => Assert.Equal(0xCC, value));
        Assert.Equal(0, ServerSnapshotTailCodec.WriteCoopShipping(buffer, 1, poses, sectors, [], []));
        Assert.Equal(0, ServerSnapshotTailCodec.WriteInvasionShipping(buffer, 1, default, poses, sectors));
    }

    [Fact]
    public void FailedInvasionOnlyBuildDoesNotStoreChecksum()
    {
        var session = new SnapshotChecksumSession();
        var result = WorldStateTailBuilder.TryBuildInvasionTailWithChecksum(new byte[1], CreateStore(), session, 42, Invasion);
        Assert.False(result.HasTail);
        Assert.False(session.Ring.TryFind(42, out _));
    }

    private static GuestWorldStateStore CreateStore()
    {
        var store = new GuestWorldStateStore();
        store.SeedActor(1, 3001);
        store.SeedPlayer(0);
        store.QueueAuthorityEvent(AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 1));
        store.QueueCoopDeadSpawn(17);
        return store;
    }

    [Fact]
    public void EventPayloadLengthCannotWrapAtUshortBoundary()
    {
        var buffer = new byte[65541];
        Assert.Equal(65540, EventRecordsCodec.Write(buffer, [new EventRecord(78, new byte[65535])]));
        var cursor = 0;
        Assert.True(EventRecordsCodec.TryRead(buffer.AsSpan(0, 65540), ref cursor, out var count, out _));
        Assert.Equal(1, count);
        Assert.Equal(65540, cursor);
        Assert.Equal(0, EventRecordsCodec.Write(buffer, [new EventRecord(78, new byte[65536])]));
    }

    [Fact]
    public void InvalidInvasionVersionDoesNotConsumeQueuedEvents()
    {
        var store = CreateStore();
        var session = new SnapshotChecksumSession();
        var result = WorldStateTailBuilder.TryBuildMergedInvasionCoopTail(new byte[4096], store, session, 42, default);
        Assert.False(result.HasTail);
        Assert.True(store.HasPendingAuthorityEvents);
        Assert.True(store.HasPendingCoopDeadSpawns);
        Assert.Equal(0u, store.ActorDeltaRollingHash);
        Assert.False(session.Ring.TryFind(42, out _));
    }

    private static int Build(int mode, byte[] buffer, GuestWorldStateStore store, SnapshotChecksumSession session) => mode switch
    {
        0 => WorldStateTailBuilder.WriteCoopTailFromStore(buffer, store, 42),
        1 => WorldStateTailBuilder.TryBuildCoopTailWithChecksum(buffer, store, session, 42).BytesWritten,
        _ => WorldStateTailBuilder.TryBuildMergedInvasionCoopTail(buffer, store, session, 42, Invasion).BytesWritten,
    };
}
