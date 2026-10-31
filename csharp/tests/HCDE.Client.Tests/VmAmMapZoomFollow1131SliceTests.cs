namespace HCDE.Client.Tests;

public class VmAmMapZoomFollow1131SliceTests
{
    [Fact]
    public void ResponderResult_ReleasesBareKeysAndKeepsPlusBinds()
    {
        Assert.False(VmAmMapZoomFollow1131Slice.ResponderResult(false, true, true, false, null));
        Assert.False(VmAmMapZoomFollow1131Slice.ResponderResult(true, true, true, false, "+forward"));
        Assert.True(VmAmMapZoomFollow1131Slice.ResponderResult(true, true, true, false, "map"));
        Assert.True(VmAmMapZoomFollow1131Slice.ResponderResult(true, true, false, true, "+forward"));
    }

    [Fact]
    public void ApplyZoom_UsesLegacyDirectionThenClearsIt()
    {
        var zoomIn = VmAmMapZoomFollow1131Slice.ApplyZoom(1, 0.5, 4, 2, false, false, 1);
        Assert.Equal(1.02 * 2, zoomIn.ScaleMapToFrame);
        Assert.Equal(0, zoomIn.ZoomDirection);
        var capped = VmAmMapZoomFollow1131Slice.ApplyZoom(1, 0.5, 4, 0, true, false, 10);
        Assert.Equal(4, capped.ScaleMapToFrame);
        var zoomOut = VmAmMapZoomFollow1131Slice.ZoomMultiplier(-2, false, false, 0);
        Assert.Equal((1 / 1.02) / 2, zoomOut);
    }

    [Fact]
    public void FollowPlayer_ScrollsWhenTheCameraMoves()
    {
        var still = VmAmMapZoomFollow1131Slice.FollowPlayer(true, 10, 20, 10, 20, 8, 6, 0, false, null);
        Assert.False(still.Moved);
        var step = VmAmMapZoomFollow1131Slice.FollowPlayer(true, 14, 20, 10, 18, 8, 6, 0, false, null);
        Assert.True(step.Moved);
        Assert.Equal(10, step.X);
        Assert.Equal(17, step.Y);
        Assert.Equal(4, step.ScrollX);
        Assert.Equal(-2, step.ScrollY);
        Assert.Equal(5, VmAmMapZoomFollow1131Slice.TickClock(true, 4));
        Assert.Equal(4, VmAmMapZoomFollow1131Slice.TickClock(false, 4));
    }

    [Fact]
    public void ShouldDrawMapBackground_RavenModeTwoStillBlits()
    {
        Assert.False(VmAmMapZoomFollow1131Slice.ShouldDrawMapBackground(true, 0, true, true, 1, 1));
        Assert.False(VmAmMapZoomFollow1131Slice.ShouldDrawMapBackground(true, 2, false, false, 1, 1));
        Assert.True(VmAmMapZoomFollow1131Slice.ShouldDrawMapBackground(true, 2, false, false, -1, VmAmMapZoomFollow1131Slice.GameRaven));
        var tiles = VmAmMapZoomFollow1131Slice.BackgroundTiles(-2, -2, 15, 12, 10, 10);
        Assert.Equal(new[] { (-2, -2), (8, -2), (-2, 8), (8, 8) }, tiles);
    }
}
