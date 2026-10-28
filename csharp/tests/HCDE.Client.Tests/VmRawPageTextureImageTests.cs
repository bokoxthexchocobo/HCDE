namespace HCDE.Client.Tests;

public class VmRawPageTextureImageTests
{
    [Fact]
    public void TryCreate_Accepts64000ByteRawPage()
    {
        var lump = new byte[VmRawPageTextureImage.PageSize];
        lump[0] = 0xFF;
        lump[2] = 0xFF;
        Assert.True(VmRawPageTextureImage.TryCreate(lump, out var width, out var height));
        Assert.Equal(320, width);
        Assert.Equal(200, height);
    }

    [Fact]
    public void CreatePalettedPixels_TransposesColumnMajor()
    {
        var lump = new byte[VmRawPageTextureImage.PageSize];
        lump[0] = 1;
        lump[1] = 2;
        var pixels = VmRawPageTextureImage.CreatePalettedPixels(lump);
        Assert.Equal(VmRawPageTextureImage.PageSize, pixels.Length);
        Assert.Equal(1, pixels[0]);
    }
}
