namespace HCDE.Client.Tests;

public class VmAutomapCommandsTests
{
    [Fact]
    public void ToggleBool_FlipsValue()
    {
        var v = false;
        Assert.True(VmAutomapCommands.ToggleBool(ref v));
        Assert.True(v);
    }

    [Fact]
    public void ShouldRunToggleMap_OnlyWhenIdle()
    {
        Assert.True(VmAutomapCommands.ShouldRunToggleMap(0));
        Assert.False(VmAutomapCommands.ShouldRunToggleMap(1));
    }

    [Fact]
    public void ParseZoomDir_UsesArgument()
    {
        Assert.Equal(2.5f, VmAutomapCommands.ParseZoomDir("2.5", 0));
    }
}
