namespace HCDE.Client.Tests;

public class VmPngTextureImageTests
{
    [Fact]
    public void TryCreate_ValidatesSignatureAndIhdr()
    {
        var png = TestPaletted2x2Png();
        Assert.True(VmPngTextureImage.TryCreate(png, out var info));
        Assert.Equal(2, info.Width);
        Assert.Equal(2, info.Height);
        Assert.Equal((byte)8, info.BitDepth);
        Assert.Equal((byte)3, info.ColorType);
    }

    [Fact]
    public void CreatePalettedPixels_DecodesInterlaced2x2()
    {
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAMAAAEyb82AAAAADFBMVEUAAAD/AAAA/wAAAP+bwBPcAAAAD0lEQVR4nGNgYGBkYGIGAAASAAe4hQWPAAAAAElFTkSuQmCC");
        Assert.True(VmPngTextureImage.TryCreate(png, out var info));
        Assert.Equal((byte)1, info.Interlace);
        var pixels = VmPngTextureImage.CreatePalettedPixels(png, info);
        Assert.Equal(4, pixels.Length);
    }

    [Fact]
    public void CreatePalettedPixels_Decodes2x2Paletted()
    {
        var png = TestPaletted2x2Png();
        Assert.True(VmPngTextureImage.TryCreate(png, out var info));
        var pixels = VmPngTextureImage.CreatePalettedPixels(png, info);
        Assert.Equal(4, pixels.Length);
        Assert.Contains(pixels, value => value != 0);
    }

    private static byte[] TestPaletted2x2Png() =>
        Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAMAAABFaP0WAAAADFBMVEUKFB4oMjxGUFpkbnjGSHffAAAADklEQVR4nGNgZGJgYgYAABoACQ++MQQAAAAASUVORK5CYII=");
}
