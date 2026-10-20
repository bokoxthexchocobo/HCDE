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

    [Fact]
    public void IncrementRestartCounter() =>
        Assert.True(VmDMainCleanupRestart.IncrementRestartCounter(0));

    [Fact]
    public void StopMusicFirst() =>
        Assert.True(VmDMainCleanupRestart.StopMusicFirst());

    [Fact]
    public void ClearMapinfoOnCleanup() =>
        Assert.True(VmDMainCleanupRestart.ClearMapinfoOnCleanup());

    [Fact]
    public void FullGcBeforeRestart() =>
        Assert.True(VmDMainCleanupRestart.FullGcBeforeRestart());
}
