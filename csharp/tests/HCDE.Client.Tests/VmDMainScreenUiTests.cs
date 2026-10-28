namespace HCDE.Client.Tests;

public class VmDMainScreenUiTests
{
    [Fact]
    public void ScreenSize_ReloadsCrosshairWhenBarPresent()
    {
        Assert.True(VmDMainScreenUi.ShouldReloadCrosshair());
    }

    [Fact]
    public void SceneRect_UsesFullScreenWhenBlocksTen()
    {
        Assert.True(VmDMainScreenUi.SceneUsesFullScreen(10));
    }

    [Fact]
    public void ConsoleToggled_FallsHudWhenConfigured()
    {
        Assert.True(VmDMainScreenUi.ConsoleToggleFallsHud(2, 1));
    }

    [Fact]
    public void AppendCrashText_NullBufferNoOp()
    {
        Assert.True(VmDMainScreenUi.AppendCrashTextNoOp(null, null));
    }

    [Fact]
    public void LocationDescription_NullLevelReturnsEmpty()
    {
        Assert.Equal(string.Empty, VmDMainScreenUi.LocationDescriptionForLevel(false));
    }

    [Fact]
    public void CrashInfo_RejectsShortBuffer()
    {
        Assert.True(VmDMainScreenUi.CrashInfoRejectShortBuffer(2));
    }
}
