namespace HCDE.Client.Tests;

public class VmDDoomMainLoopIterTests
{
    [Fact]
    public void RestartConsoleWhenRestart() =>
        Assert.True(VmDDoomMainLoopIter.RestartConsoleWhenRestart(true));

    [Fact]
    public void LoopIterationBegin() =>
        Assert.True(VmDDoomMainLoopIter.LoopIterationBegin());

    [Fact]
    public void AfterPClassStaticInit() =>
        Assert.True(VmDDoomMainLoopIter.AfterPClassStaticInit());
}
