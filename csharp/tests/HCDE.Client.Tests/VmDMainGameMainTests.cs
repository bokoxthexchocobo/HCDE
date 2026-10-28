namespace HCDE.Client.Tests;

public class VmDMainGameMainTests
{
    [Fact]
    public void CreateDisplayWhenInteractive() =>
        Assert.True(VmDMainGameMain.CreateDisplayWhenInteractive(false, false));

    [Fact]
    public void SkipCloseWidgetWhenNorun() =>
        Assert.True(VmDMainGameMain.SkipCloseWidgetWhenNorun(1337));

    [Fact]
    public void GameStateAllowsMenuCapture() =>
        Assert.True(VmDMainGameMain.GameStateAllowsMenuCapture(1, 0, 2));
}
