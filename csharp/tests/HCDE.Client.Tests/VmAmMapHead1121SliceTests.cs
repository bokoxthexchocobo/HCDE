namespace HCDE.Client.Tests;

public class VmAmMapHead1121SliceTests
{
    [Fact]
    public void ZoomIn_DoublesScaleEachSecond()
    {
        Assert.Equal(8, VmAmMapHead1121Slice.ZoomIn(2, 2));
        Assert.Equal(16, VmAmMapHead1121Slice.PlayerRadius);
        Assert.Equal(10, VmAmMapHead1121Slice.MarkPointCount);
    }

    [Fact]
    public void ApplyLegacyZoom_UsesOldFactors()
    {
        var zoomed = VmAmMapHead1121Slice.ApplyLegacyZoom(100, zoomIn: true, steps: 1);
        Assert.Equal(102, zoomed, 3);
    }

    [Fact]
    public void ClampAutomapCheat_BlocksNetWithoutCheats()
    {
        Assert.Equal(0, VmAmMapHead1121Slice.ClampAutomapCheat(2, netGame: true, cheatsEnabled: false));
        Assert.Equal(2, VmAmMapHead1121Slice.ClampAutomapCheat(2, netGame: true, cheatsEnabled: true));
    }

    [Fact]
    public void ApplyOverlayChange_TurnsAutomapOffWhenOverlayCleared()
    {
        var ctx = new VmAmMapHead1121Slice.OverlayContext
        {
            AutomapActive = true,
            ViewActive = true,
            Overlay = 0
        };
        VmAmMapHead1121Slice.ApplyOverlayChange(ctx);
        Assert.False(ctx.AutomapActive);
        Assert.True(ctx.ViewActive);
        var defaults = VmAmMapHead1121Slice.CreateDefaults();
        Assert.Equal(1f, defaults.LineAlpha);
        Assert.Equal(-1, defaults.ShowAllLines);
        Assert.Equal(2, VmAmMapHead1121Slice.RefreshShowAllLines(false, true, 2, _ => Assert.Fail("no level")));
    }
}
