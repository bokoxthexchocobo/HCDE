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
}
