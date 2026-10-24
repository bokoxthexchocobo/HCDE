namespace HCDE.Client.Tests;

public class VmGameConfigLegacyGammaTests
{
    [Fact]
    public void LegacyGammaToFixGamma_ClampsAndRemaps()
    {
        Assert.Equal(-2.625f, VmGameConfigLegacyGamma.LegacyGammaToFixGamma(0.05f), precision: 3);
        Assert.Equal(1f, VmGameConfigLegacyGamma.LegacyGammaToFixGamma(3.5f));
        Assert.Equal(0f, VmGameConfigLegacyGamma.LegacyGammaToFixGamma(2.2f));
        Assert.Equal(0.5f, VmGameConfigLegacyGamma.LegacyGammaToFixGamma(2.6f), precision: 5);
    }
}
