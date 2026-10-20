namespace HCDE.Gamedata.Tests;

public class VmEMapInfoPropertyTailTests
{
    [Fact]
    public void IsNoFlatSky2_DetectsMarker()
    {
        Assert.True(VmEMapInfoPropertyTail.IsNoFlatSky2("-NOFLAT-"));
    }
}
