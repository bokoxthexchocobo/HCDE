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
}
