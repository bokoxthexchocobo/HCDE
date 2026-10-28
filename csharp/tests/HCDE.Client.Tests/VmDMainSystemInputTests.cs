namespace HCDE.Client.Tests;

public class VmDMainSystemInputTests
{
    [Fact]
    public void MouseCaptureMode_ClampsHigh()
    {
        Assert.Equal(2, VmDMainSystemInput.ClampMouseCaptureMode(9));
    }

    [Fact]
    public void DirectMouseLook_RequiresMenuOff()
    {
        Assert.True(VmDMainSystemInput.ShouldApplyDirectMouseLook(true, 0, false, false));
    }

    [Fact]
    public void CaptureMode_InGameOnlyForModeZero()
    {
        Assert.True(VmDMainSystemInput.CaptureModeInGame(0, VmDMainSystemInput.GsLevel, false, false));
    }

    [Fact]
    public void WantGuiCapture_UsesUiProcessorsWhenIdle()
    {
        Assert.True(VmDMainSystemInput.WantGuiCapture(VmDMainSystemInput.MenuOff, 0, false, true));
    }

    [Fact]
    public void DirectMouseLook_ConsumesWhenAxesZeroed()
    {
        var result = VmDMainSystemInput.ApplyDirectMouseLook(4, 2, true, false, false, false, 1.0, 1.0);
        Assert.True(result.Consumed);
    }
}
