namespace HCDE.Gamedata.Tests;

public class VmMapInfoLightBlendTonemapTests
{
    [Fact]
    public void TryParseLightBlendMode_Unclamped()
    {
        Assert.True(VmMapInfoLightBlendTonemap.TryParseLightBlendMode("Unclamped", out var mode, out _));
        Assert.Equal(VmMapInfoLightBlendMode.NoClamp, mode);
    }

    [Fact]
    public void TryParseTonemap_Reinhard()
    {
        Assert.True(VmMapInfoLightBlendTonemap.TryParseTonemap("Reinhard", out var mode));
        Assert.Equal(VmMapInfoTonemapMode.Reinhard, mode);
    }
}
