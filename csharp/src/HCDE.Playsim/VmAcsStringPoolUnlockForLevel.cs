namespace HCDE.Playsim;

/// <summary><c>ACSStringPool::UnlockForLevel</c> from <c>p_acs.cpp</c> (1408–1421).</summary>
public static class VmAcsStringPoolUnlockForLevel
{
    public readonly record struct PoolEntry(uint Next, int[] Locks);

    public static int FindLevelLockIndex(ReadOnlySpan<int> locks, int levelNum)
    {
        for (var i = 0; i < locks.Length; i++)
        {
            if (locks[i] == levelNum)
                return i;
        }

        return locks.Length;
    }

    public static void UnlockForLevelNative(ReadOnlySpan<PoolEntry> pool, int levelNum, Action<int, int> deleteLockAt)
    {
        for (var i = 0; i < pool.Length; i++)
        {
            if (pool[i].Next == VmAcsStringPool.FreeEntry)
                continue;

            var ndx = FindLevelLockIndex(pool[i].Locks, levelNum);
            if (ndx < pool[i].Locks.Length)
                deleteLockAt(i, ndx);
        }
    }
}
