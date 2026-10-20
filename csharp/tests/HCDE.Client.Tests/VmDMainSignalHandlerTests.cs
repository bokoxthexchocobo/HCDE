namespace HCDE.Client.Tests;

public class VmDMainSignalHandlerTests
{
    [Fact]
    public void ExitImmediatelyWhenAlreadyAborting() =>
        Assert.True(VmDMainSignalHandler.ExitImmediatelyWhenAlreadyAborting(true));

    [Fact]
    public void RequestShutdownWhenRunning() =>
        Assert.True(VmDMainSignalHandler.RequestShutdownWhenRunning(false));
}
