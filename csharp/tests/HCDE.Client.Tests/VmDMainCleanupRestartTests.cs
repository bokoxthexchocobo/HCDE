namespace HCDE.Client.Tests;

public class VmDMainCleanupRestartTests
{
    [Fact]
    public void NetgameCleanupWarn() =>
        Assert.True(VmDMainCleanupRestart.NetgameCleanupWarn(true, false));

    [Fact]
    public void IgnoreDebugRestartDuringNetgame() =>
        Assert.True(VmDMainCleanupRestart.IgnoreDebugRestartDuringNetgame(true, false));

    [Fact]
    public void IncrementRestartCounter() =>
        Assert.True(VmDMainCleanupRestart.IncrementRestartCounter(0));

    [Fact]
    public void StripRestartArgs_WhenRunAllowed() =>
        Assert.True(VmDMainCleanupRestart.StripRestartArgs(norun: false));

    [Fact]
    public void ShutdownTagNonEmpty() =>
        Assert.True(VmDMainCleanupRestart.ShutdownTagNonEmpty("cleanup"));
}
