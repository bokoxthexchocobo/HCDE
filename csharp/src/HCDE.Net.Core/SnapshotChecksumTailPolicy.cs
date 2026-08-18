namespace HCDE.Net.Core;

public static class SnapshotChecksumTailPolicy
{
    public static uint[]? TryResolveTailChecksumHashes(
        GuestWorldStateStore? store,
        SnapshotChecksumSession? checksumSession,
        int gameTic,
        int rngSeed = 0)
    {
        if (store is null || checksumSession is null)
            return null;

        SnapshotChecksumPlaysimInputs.ComputeAndStore(checksumSession, store, gameTic, rngSeed);
        if (!checksumSession.Ring.TryFind(gameTic, out var ringHashes))
            return null;

        return ringHashes;
    }
}
