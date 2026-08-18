namespace HCDE.Net.Core.Tests;

public class SnapshotChecksumPresentationEchoPolicyTests
{
    [Fact]
    public void ComputeRollingHash_ReturnsZeroWhenNoAppliedEcho()
    {
        var store = new GuestWorldStateStore();
        Assert.Equal(0u, SnapshotChecksumPresentationEchoPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void CommitAppliedPresentationEcho_UpdatesRollingHashOnGuestApply()
    {
        var store = new GuestWorldStateStore();
        var block = PresentationEchoCodec.CreateExampleBlock();

        store.CommitAppliedPresentationEcho(block);

        Assert.NotEqual(0u, store.PresentationEchoRollingHash);
        Assert.Equal(store.PresentationEchoRollingHash, SnapshotChecksumPresentationEchoPolicy.ComputeRollingHash(store));
    }

    [Fact]
    public void MixBlock_ProducesStableHashForSameBlock()
    {
        var block = PresentationEchoCodec.CreateExampleBlock();
        var first = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, block);
        var second = SnapshotChecksumPresentationEchoPolicy.MixBlock(0, block);
        Assert.Equal(first, second);
        Assert.NotEqual(0u, first);
    }
}
