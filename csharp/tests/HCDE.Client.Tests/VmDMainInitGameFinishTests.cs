namespace HCDE.Client.Tests;

public class VmDMainInitGameFinishTests
{
    [Fact]
    public void NorunBatchEarlyExit() =>
        Assert.True(VmDMainInitGameFinish.NorunBatchEarlyExit(true, false));

    [Fact]
    public void NorunExitCode() =>
        Assert.Equal(1337, VmDMainInitGameFinish.NorunExitCode());

    [Fact]
    public void AutostartWarp() =>
        Assert.True(VmDMainInitGameFinish.AutostartWarp(true, false));

    [Fact]
    public void DedicatedIgnoresStoredWarp() =>
        Assert.True(VmDMainInitGameFinish.DedicatedIgnoresStoredWarp(true));

    [Fact]
    public void LoadgameFatalWhenMissing() =>
        Assert.True(VmDMainInitGameFinish.LoadgameFatalWhenMissing(false));

    [Fact]
    public void CreateStartScreenWhenMissing() =>
        Assert.True(VmDMainInitGameFinish.CreateStartScreenWhenMissing(false, null));

    [Fact]
    public void RecordDemoFromArg() =>
        Assert.True(VmDMainInitGameFinish.RecordDemoFromArg("demo1"));

    [Fact]
    public void RestartUsesStartTitle() =>
        Assert.True(VmDMainInitGameFinish.RestartUsesStartTitle(true));
}
