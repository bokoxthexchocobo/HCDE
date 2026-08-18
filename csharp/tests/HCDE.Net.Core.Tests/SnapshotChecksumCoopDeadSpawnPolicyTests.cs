namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumCoopDeadSpawnPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoRetiredSpawns()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumCoopDeadSpawnPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void ComputeRollingHash_IncludesRetiredSpawnIndices()
    {
        var store = new GuestWorldStateStore();
        store.QueueCoopDeadSpawn(12);
        store.QueueCoopDeadSpawn(7);
        _ = store.TakePendingCoopDeadSpawnsForTail();

        Assert.NotEqual(0u, SnapshotChecksumCoopDeadSpawnPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void TakePendingCoopDeadSpawnsForTail_RetiresIndicesForChecksum()
    {
        var store = new GuestWorldStateStore();
        store.QueueCoopDeadSpawn(44);

        var pending = store.TakePendingCoopDeadSpawnsForTail();

        Assert.Equal(new uint[] { 44 }, pending);
        Assert.Contains(44u, store.RetiredCoopDeadSpawns);
        Assert.False(store.HasPendingCoopDeadSpawns);
    }
}
