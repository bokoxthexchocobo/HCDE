namespace HCDE.Net.Core.Tests;

public class VmDNetTrySkipUserCmdTests
{
    [Fact]
    public void HasButtonsFlagNative_DetectsFlag() =>
        Assert.True(VmDNetTrySkipUserCmd.HasButtonsFlagNative(VmDNetTrySkipUserCmd.UcmdfButtons));

    [Fact]
    public void AllowImplicitEmptyAtEndNative_RequiresEvent() =>
        Assert.True(VmDNetTrySkipUserCmd.AllowImplicitEmptyAtEndNative(true, true));

    [Fact]
    public void CommandOffsetInRangeNative_RejectsHighOffset() =>
        Assert.False(VmDNetTrySkipUserCmd.CommandOffsetInRangeNative(64, 35));

    [Fact]
    public void PlausibleNextCommandOffsetNative_RejectsSeen() =>
        Assert.False(VmDNetTrySkipUserCmd.PlausibleNextCommandOffsetNative(
            2, 35, 1ul << 2, false));
}
