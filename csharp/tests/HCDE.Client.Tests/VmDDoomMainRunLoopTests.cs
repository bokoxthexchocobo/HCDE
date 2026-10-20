namespace HCDE.Client.Tests;

public class VmDDoomMainRunLoopTests
{
    [Fact]
    public void DebugPortInRange_Valid() =>
        Assert.True(VmDDoomMainRunLoop.DebugPortInRange(8080));

    [Fact]
    public void DebugPortFromArg_OverridesCvar() =>
    {
        Assert.True(VmDDoomMainRunLoop.DebugPortFromArg("1234", 9999, out var port));
        Assert.Equal(1234, port);
    }

    [Fact]
    public void DebugPortInRange_RejectsHigh() =>
        Assert.False(VmDDoomMainRunLoop.DebugPortInRange(70000));
}
