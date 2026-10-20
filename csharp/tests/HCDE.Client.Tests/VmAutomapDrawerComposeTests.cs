namespace HCDE.Client.Tests;

public class VmAutomapDrawerComposeTests
{
    [Fact]
    public void ComputeKeyboardPanDelta_AccumulatesDirections()
    {
        var (x, y) = VmAutomapDrawerCompose.ComputeKeyboardPanDelta(true, false, false, true, 10, 1, 35);
        Assert.True(x < 0);
        Assert.True(y < 0);
    }

    [Fact]
    public void ShouldDrawThingsLayer_MatchesCheatGate()
    {
        Assert.True(VmAutomapDrawerCompose.ShouldDrawThingsLayer(2, false));
        Assert.False(VmAutomapDrawerCompose.ShouldDrawThingsLayer(4, false));
    }

    [Fact]
    public void CrosshairPixel_CentersHorizontally()
    {
        var (cx, cy) = VmAutomapDrawerCompose.CrosshairPixel(640, 480);
        Assert.Equal(320, cx);
        Assert.Equal(240, cy);
    }

    [Fact]
    public void TryResolveAuthorMarkerTexture_RejectsInvalidFrame()
    {
        Assert.False(VmAutomapDrawerCompose.TryResolveAuthorMarkerTexture(false, 1, 5, 3, out _, out var bad));
        Assert.True(bad);
    }

    [Fact]
    public void ResolveAuthorMarkerScale_AppliesMtofWhenArg2()
    {
        var (x, y) = VmAutomapDrawerCompose.ResolveAuthorMarkerScale(2, 3, 1, v => v * 10);
        Assert.Equal(20, x);
        Assert.Equal(30, y);
    }

    [Fact]
    public void ShouldFlipMarkerForNegativeScale_TogglesFlip()
    {
        Assert.True(VmAutomapDrawerCompose.ShouldFlipMarkerForNegativeScale(-1, false));
    }
}
