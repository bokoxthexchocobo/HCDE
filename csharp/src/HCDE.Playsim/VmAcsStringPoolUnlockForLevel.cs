namespace HCDE.Playsim;

/// <summary><c>ACSStringPool::UnlockForLevel</c> from <c>p_acs.cpp</c> (1408–1421).</summary>
public static class VmAcsStringPoolUnlockForLevel
{
    public static int RemoveLevelLock(ReadOnlySpan<int> locks, int levelNum)
    {
        for (var i = 0; i < locks.Length; i++)
        {
            if (locks[i] == levelNum)
                return i;
        }

        return -1;
    }

    public static bool ShouldRetainEntry(uint next, ReadOnlySpan<int> locksAfterRemoval) =>
        next != VmAcsStringPool.FreeEntry && locksAfterRemoval.Length > 0;
}
