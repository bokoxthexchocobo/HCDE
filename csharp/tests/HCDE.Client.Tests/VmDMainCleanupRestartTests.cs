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
    public void SetWantToRestart() =>
        Assert.True(VmDMainCleanupRestart.SetWantToRestart());
}
