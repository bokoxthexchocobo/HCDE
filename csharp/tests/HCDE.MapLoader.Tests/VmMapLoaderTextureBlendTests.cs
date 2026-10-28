namespace HCDE.MapLoader.Tests;

public class VmMapLoaderTextureBlendTests
{
    [Fact]
    public void TryParseHexBlend_ReadsEightChars()
    {
        Assert.True(VmMapLoaderTextureBlend.TryParseHexBlend("00FF80AB", out var blend));
        Assert.Equal(0x00FF80ABu, blend);
    }

    [Fact]
    public void ResolveBlendName_UsesColormapWhenPresent()
    {
        var r = VmMapLoaderTextureBlend.ResolveBlendName(
            "WATERMAP",
            _ => 7u,
            _ => false);
        Assert.True(r.UseColormap);
        Assert.Equal(7u, r.Blend);
    }

    [Fact]
    public void ResolveBlendName_FallsBackToHex()
    {
        var r = VmMapLoaderTextureBlend.ResolveBlendName(
            "12AB",
            _ => 0u,
            _ => false);
        Assert.Equal(0x12ABu, r.Blend);
    }
}
