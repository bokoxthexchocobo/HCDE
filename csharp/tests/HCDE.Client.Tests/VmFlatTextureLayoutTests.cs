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
    public void CreatePalettedPixels_PadsShortLumpsAndFlipsWithRemap()
    {
        var descriptor = new VmFlatTextureDescriptor(1, 2, 2, true, false, false);
        var pixels = VmFlatTextureImage.CreatePalettedPixels([1, 2, 3, 4], descriptor, conversion: 0);
        Assert.Equal(new byte[] { 1, 3, 2, 4 }, pixels);
    }

    [Fact]
    public void FlipSquareBlockRemap_TransposesTwoByTwo()
    {
        Span<byte> block = [1, 2, 3, 4];
        VmImageHelpers.FlipSquareBlockRemap(block, 2, VmImageHelpers.GetRemap(false));
        Assert.Equal(new byte[] { 1, 3, 2, 4 }, block.ToArray());
    }
}
