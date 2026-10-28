namespace HCDE.Client.Tests;

public class VmDDoomMainInternalBootTests
{
    [Fact]
    public void NetIdStart_Offset() =>
        Assert.Equal(33, VmDDoomMainInternalBoot.NetIdStart());

    [Fact]
    public void EarlyExitVersionHelp() =>
        Assert.True(VmDDoomMainInternalBoot.EarlyExitVersionHelp(true, false));

    [Fact]
    public void BatchRunFromErrorLog() =>
        Assert.True(VmDDoomMainInternalBoot.BatchRunFromErrorLog(true));

    [Fact]
    public void ShouldDebug_FromArg() =>
        Assert.True(VmDDoomMainInternalBoot.ShouldDebug(false, true));

    [Fact]
    public void FatalWhenBasewadMissing() =>
        Assert.True(VmDDoomMainInternalBoot.FatalWhenBasewadMissing(false));

    [Fact]
    public void ForceSwNodesFromArg() =>
        Assert.True(VmDDoomMainInternalBoot.ForceSwNodesFromArg(true));

    [Fact]
    public void EnterMainDoLoop() =>
        Assert.True(VmDDoomMainInternalBoot.EnterMainDoLoop());
}
