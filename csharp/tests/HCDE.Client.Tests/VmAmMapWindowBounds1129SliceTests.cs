namespace HCDE.Client.Tests;

public class VmAmMapWindowBounds1129SliceTests
{
    [Fact]
    public void ConsiderVertex_GrowsBoxAndPlayerMinSizeIsThirtyTwo()
    {
        double minX = 5, minY = 5, maxX = 5, maxY = 5;
        VmAmMapWindowBounds1129Slice.ConsiderVertex(ref minX, ref minY, ref maxX, ref maxY, 1, 9);
        VmAmMapWindowBounds1129Slice.ConsiderVertex(ref minX, ref minY, ref maxX, ref maxY, 8, 2);
        Assert.Equal((1d, 2d, 8d, 9d), (minX, minY, maxX, maxY));
        var extents = VmAmMapWindowBounds1129Slice.ExtentsAfterVertices(minX, minY, maxX, maxY);
        Assert.Equal((7d, 7d, 32d, 32d), extents);
    }

    [Fact]
    public void ZoomScales_UsesTighterFitAndPlayerHeightCap()
    {
        var (minScale, maxScale) = VmAmMapWindowBounds1129Slice.ZoomScales(10, 320, 168, 200, 100, 80);
        Assert.Equal(0.9 * (168 / 80d), minScale);
        Assert.Equal(200 / 32d, maxScale);
    }

    [Fact]
    public void ClipCenter_LocksWhenNotRotatingAndScrollWrapsPatch()
    {
        Assert.True(VmAmMapWindowBounds1129Slice.ShouldLockToLevelBounds(0, true));
        Assert.False(VmAmMapWindowBounds1129Slice.ShouldLockToLevelBounds(1, false));
        Assert.True(VmAmMapWindowBounds1129Slice.ShouldLockToLevelBounds(2, false));
        double x = -30, y = -40, x2 = 0, y2 = 0;
        VmAmMapWindowBounds1129Slice.ClipCenterToBounds(ref x, ref y, ref x2, ref y2, 20, 10, 0, 0, 100, 50, true);
        Assert.Equal(-10, x);
        Assert.Equal(-5, y);
        Assert.Equal(10, x2);
        var scrolled = VmAmMapWindowBounds1129Slice.ScrollParchment(30, -5, 0, 0, 1, true, 16, 8);
        Assert.Equal(-2, scrolled.X);
        Assert.Equal(-5, scrolled.Y);
    }

    [Fact]
    public void ChangeWindowLoc_DisablesFollowAndBeginDisplayResetsClock()
    {
        var pan = VmAmMapWindowBounds1129Slice.ChangeWindowLoc(
            true, 2, 0, 10, 20, 40, 30, -1000, -1000, 1000, 1000, 320, 200, 0, false, null);
        Assert.False(pan.FollowPlayer);
        Assert.Equal(float.MaxValue, pan.OldLocationX);
        Assert.Equal(12, pan.X);
        Assert.Equal(2, pan.ScrollX);
        Assert.Equal(0, pan.ScrollY);
        var display = VmAmMapWindowBounds1129Slice.BeginDisplay(320, 200, 0.5);
        Assert.Equal(0, display.Clock);
        Assert.Equal(1, display.ZoomMul);
        Assert.Equal(160, display.W);
        Assert.Equal(100, display.H);
    }
}
