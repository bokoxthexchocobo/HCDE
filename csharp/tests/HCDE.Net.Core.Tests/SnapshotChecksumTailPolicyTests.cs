namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumTailPolicyTests
{
    [Fact]
    public void TryResolveTailChecksumHashes_ReturnsNullWhenSessionMissing()
    {
        var store = new GuestWorldStateStore();
        store.SeedPlayer(playerNum: 0, health: 80);

        Assert.Null(SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(
            store,
            checksumSession: null,
            gameTic: 5,
            rngSeed: 0));
    }

    [Fact]
    public void TryResolveTailChecksumHashes_ReturnsRingHashesAfterCompute()
    {
        var store = new GuestWorldStateStore();
        store.SeedPlayer(playerNum: 0, health: 80);
        var checksumSession = new SnapshotChecksumSession();
        SnapshotChecksumPlaysimInputs.ComputeAndStore(checksumSession, store, gameTic: 5, rngSeed: 0);

        var resolved = SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(
            store,
            checksumSession,
            gameTic: 5,
            rngSeed: 0);
        Assert.NotNull(resolved);
        Assert.True(checksumSession.Ring.TryFind(5, out var ringHashes));
        Assert.Equal(ringHashes, resolved);
    }
}
