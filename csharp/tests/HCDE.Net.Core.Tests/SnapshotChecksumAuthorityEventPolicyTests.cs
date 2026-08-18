namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumAuthorityEventPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoAppliedEvents()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void TakePendingAuthorityEventsForTail_UpdatesRollingHashOnShip()
    {
        var store = new GuestWorldStateStore();
        store.QueueAuthorityEvent(AuthorityEventsCodec.CreateSpawnExample("Imp", actorId: 12));

        _ = store.TakePendingAuthorityEventsForTail();

        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
        Assert.Equal(store.AuthorityEventRollingHash, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void CommitAppliedAuthorityEvents_UpdatesRollingHashOnGuestApply()
    {
        var store = new GuestWorldStateStore();
        var record = AuthorityEventsCodec.CreateSpawnExample("ZombieMan", actorId: 33);

        store.CommitAppliedAuthorityEvents(new[] { record });

        Assert.NotEqual(0u, store.AuthorityEventRollingHash);
        Assert.Equal(store.AuthorityEventRollingHash, SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store));
    }
}
