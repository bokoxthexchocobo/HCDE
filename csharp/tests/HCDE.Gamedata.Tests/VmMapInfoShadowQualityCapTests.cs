namespace HCDE.Gamedata.Tests;

public class VmMapInfoShadowQualityCapTests
{
    [Fact]
    public void Normalize_SnapsToKnownStep()
    {
        Assert.Equal(512, VmMapInfoShadowQualityCap.Normalize(600));
        Assert.Equal(128, VmMapInfoShadowQualityCap.Normalize(10));
    }
}
