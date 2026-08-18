namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumLineSpecPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoLineSpecs()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumLineSpecPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void NoteLineSpec_UpdatesRollingHashOnGuestStore()
    {
        var store = new GuestWorldStateStore();
        store.NoteLineSpec(lineIndex: 9, special: 11, success: true);

        Assert.NotEqual(0u, store.LineSpecRollingHash);
        Assert.Equal(store.LineSpecRollingHash, SnapshotChecksumLineSpecPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void MixRecord_ProducesStableHashForSameInputs()
    {
        var first = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 4, special: 7, success: false);
        var second = SnapshotChecksumLineSpecPolicy.MixRecord(0, lineIndex: 4, special: 7, success: false);
        Assert.Equal(first, second);
        Assert.NotEqual(0u, first);
    }
}
