namespace HCDE.Client.Tests;

public class VmAutomapViewWindowTests
{
    [Fact]
    public void ActivateNewScale_ReanchorsWindow()
    {
        var (x, y, w, h) = VmAutomapViewWindow.ActivateNewScale(10, 20, 100, 80, 320, 200, 0.5);
        Assert.Equal(-20, x, 3);
        Assert.Equal(10, y, 3);
        Assert.Equal(160, w, 3);
        Assert.Equal(100, h, 3);
    }

    [Fact]
    public void RestoreScaleAndLoc_FollowUsesCamera()
    {
        var (x, y, _, _) = VmAutomapViewWindow.RestoreScaleAndLoc(1, 2, 100, 80, true, 500, 600);
        Assert.Equal(450, x);
        Assert.Equal(560, y);
    }

    [Fact]
    public void FindVertexBounds_TracksExtents()
    {
        var b = VmAutomapViewWindow.FindVertexBounds(new[] { (0.0, 5.0), (10.0, -3.0) });
        Assert.Equal(0, b.minX);
        Assert.Equal(10, b.maxX);
    }

    [Fact]
    public void CalcMinMaxMapToFrame_PicksSmallerAxisScale()
    {
        var (min, max) = VmAutomapViewWindow.CalcMinMaxMapToFrame(0, 320, 200, 1000, 500, 180, 16);
        Assert.True(min < max);
    }

    [Fact]
    public void PanningDisablesFollow_WhenEitherAxisNonZero()
    {
        Assert.True(VmAutomapViewWindow.PanningDisablesFollow(1, 0));
        Assert.False(VmAutomapViewWindow.PanningDisablesFollow(0, 0));
    }

    [Fact]
    public void TryAddMarkIndex_RequiresFontOrTexture()
    {
        Assert.Null(VmAutomapViewWindow.TryAddMarkIndex(true, false, 3, 10));
        Assert.Equal(3, VmAutomapViewWindow.TryAddMarkIndex(false, false, 3, 10));
    }

    [Fact]
    public void ClampParchmentScroll_BoundsMagnitude()
    {
        Assert.Equal(40000, VmAutomapViewWindow.ClampParchmentScroll(50000));
        Assert.Equal(-40000, VmAutomapViewWindow.ClampParchmentScroll(-90000));
    }
}
