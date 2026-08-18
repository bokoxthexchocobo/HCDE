namespace HCDE.Net.Core;

public static class SnapshotChecksumCoopDeadSpawnPolicy
{
    public static uint ComputeRollingHash(GuestWorldStateStore store)
    {
        if (store.RetiredCoopDeadSpawns.Count == 0)
            return 0;

        var hash = 0u;
        foreach (var spawnIndex in store.RetiredCoopDeadSpawns.OrderBy(static index => index))
            hash = SnapshotChecksumMixer.MixU32(hash, spawnIndex);

        return hash;
    }
}
