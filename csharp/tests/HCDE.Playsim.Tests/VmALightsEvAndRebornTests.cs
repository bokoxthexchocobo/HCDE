namespace HCDE.Playsim.Tests;

public class VmALightsEvAndRebornTests
{
    [Fact]
    public void Strobe_SkipsLitSector()
    {
        Assert.True(VmALightsEvStrobe.ShouldSkipSectorWithLightingData(true));
    }

    [Fact]
    public void LightTurnOn_SearchesWhenBrightNegative()
    {
        Assert.True(VmALightsEvLightTurnOn.ShouldSearchNeighborBrightness(-1));
    }

    [Fact]
    public void LightGlowing_SwapsBounds()
    {
        var (upper, lower) = VmALightsEvStartLightGlowing.NormalizeBounds(10, 40);
        Assert.Equal((40, 10), (upper, lower));
    }
}
