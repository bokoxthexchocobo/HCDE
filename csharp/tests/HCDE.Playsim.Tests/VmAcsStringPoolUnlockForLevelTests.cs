namespace HCDE.Playsim.Tests;

public class VmAcsStringPoolUnlockForLevelTests
{
    [Fact]
    public void UnlockForLevelNative_RemovesMatchingLocks()
    {
        var pool = new[]
        {
            new VmAcsStringPoolUnlockForLevel.PoolEntry(VmAcsStringPool.FreeEntry, []),
            new VmAcsStringPoolUnlockForLevel.PoolEntry(1, [3, 7]),
        };
        var removed = new List<(int, int)>();
        VmAcsStringPoolUnlockForLevel.UnlockForLevelNative(pool, 7, (i, ndx) => removed.Add((i, ndx)));
        Assert.Equal([(1, 1)], removed);
    }
}
