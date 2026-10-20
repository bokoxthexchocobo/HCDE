namespace HCDE.Client.Tests;

public class VmFlatTextureLayoutTests
{
    [Theory]
    [InlineData(64 * 64, 64)]
    [InlineData(256 * 256, 256)]
    public void TryGetDimensionsFromArea_MatchesNativeSwitch(int area, int dimension)
    {
        Assert.True(VmFlatTextureLayout.TryGetDimensionsFromArea(area, out var width, out var height));
        Assert.Equal(dimension, width);
        Assert.Equal(dimension, height);
    }

    [Fact]
    public void ReadPalettedPixels_PadsShortLumpsWithBb()
    {
        var pixels = VmFlatTextureLayout.ReadPalettedPixels([1, 2], 2, 2);
        Assert.Equal([1, 2, 0xBB, 0xBB], pixels);
    }
}
