namespace HCDE.Net.Core.Tests;

public class VmSvMasterNms1WaitTests
{
    [Fact]
    public void MapHeartbeatAckNative_ClassifiesStaleToken()
    {
        var stale = VmSvMasterNms1Wait.MapHeartbeatAckNative(
            VmSvMasterNms1Wait.Nms1ErrorStaleEntry, isError: true);
        Assert.Equal(VmSvMasterNms1Wait.Nms1HeartbeatResult.NeedsRegister, stale);
        var ok = VmSvMasterNms1Wait.MapHeartbeatAckNative(0, isError: false);
        Assert.Equal(VmSvMasterNms1Wait.Nms1HeartbeatResult.Refreshed, ok);
    }

    [Fact]
    public void SplitSelectTimeoutNative_SplitsMilliseconds()
    {
        var (sec, usec) = VmSvMasterNms1Wait.SplitSelectTimeoutNative(1500);
        Assert.Equal(1, sec);
        Assert.Equal(500_000, usec);
    }

    [Fact]
    public void ShouldIgnoreUnexpectedReplyNative_WhenAddressDiffers()
    {
        Assert.True(VmSvMasterNms1Wait.ShouldIgnoreUnexpectedReplyNative(
            sameAddress: false, VmSvMasterNms1Wait.Nms1ParseResult.Ok));
    }
}
