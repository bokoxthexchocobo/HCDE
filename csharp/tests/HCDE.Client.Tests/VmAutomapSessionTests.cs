namespace HCDE.Client.Tests;

public class VmAutomapSessionTests
{
    [Fact]
    public void ResolveInitialPlayer_FallsBackToFirstInGame()
    {
        var idx = VmAutomapSession.ResolveInitialPlayer(new[] { false, true, false }, 0);
        Assert.Equal(1, idx);
    }

    [Fact]
    public void PickInitialScale_CapsAtMax()
    {
        Assert.Equal(2.0, VmAutomapSession.PickInitialScale(2, 2.5));
        Assert.Equal(2.0, VmAutomapSession.PickInitialScale(2, 1.0));
    }

    [Fact]
    public void ShouldIgnorePanBindInFollow_MatchesPrefix()
    {
        Assert.True(VmAutomapSession.ShouldIgnorePanBindInFollow(true, "+am_panleft"));
        Assert.False(VmAutomapSession.ShouldIgnorePanBindInFollow(false, "+am_panleft"));
    }

    [Fact]
    public void ResolveZoomMultiplier_UsesZoomDirWhenSet()
    {
        Assert.Equal(3.0, VmAutomapSession.ResolveZoomMultiplier(2, 1.5, 0.5, false, false, 1));
    }

    [Fact]
    public void RescaleAfterResolution_ZeroOldMin_ReturnsInput()
    {
        Assert.Equal(4.0, VmAutomapSession.RescaleAfterResolution(4, 0, 1));
    }

    [Fact]
    public void CenterOnCamera_OffsetsByHalfWindow()
    {
        var (x, y) = VmAutomapSession.CenterOnCamera(100, 200, 40, 30);
        Assert.Equal(80, x);
        Assert.Equal(185, y);
    }

    [Fact]
    public void ClearMarkArray_FillsEmptySentinel()
    {
        Span<double> xs = stackalloc double[3];
        VmAutomapSession.ClearMarkArray(xs);
        Assert.All(xs.ToArray(), v => Assert.Equal(-1, v));
    }

    [Fact]
    public void ClampScale_ClampsToBounds()
    {
        Assert.Equal(2, VmAutomapSession.ClampScale(5, 1, 2));
    }

    [Fact]
    public void ScaleLineThickness_MinimumOne()
    {
        Assert.Equal(1, VmAutomapSession.ScaleLineThickness(0.2, 0.1));
        Assert.Equal(2, VmAutomapSession.ScaleLineThickness(2.4, 3.1));
    }

    [Fact]
    public void ShouldReleaseMainBinding_RejectsHoldBinds()
    {
        Assert.False(VmAutomapSession.ShouldReleaseMainBinding("+jump"));
        Assert.True(VmAutomapSession.ShouldReleaseMainBinding("jump"));
    }

    [Fact]
    public void FollowPlayerMapCenter_AppliesRotation()
    {
        var (x, y) = VmAutomapSession.FollowPlayerMapCenter(10, 0, 0, 4, 4, 0, true, 0, 1);
        Assert.Equal(8, x, 3);
        Assert.Equal(-2, y, 3);
    }
}
