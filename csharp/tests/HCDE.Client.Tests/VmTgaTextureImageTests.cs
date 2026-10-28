namespace HCDE.Client.Tests;

public class VmTgaTextureImageTests
{
    [Fact]
    public void TryParseHeader_RejectsInvalidDescriptor()
    {
        var lump = new byte[VmTgaTextureImage.HeaderSize];
        lump[1] = 1;
        lump[2] = 1;
        lump[12] = 2;
        lump[14] = 2;
        lump[16] = 8;
        lump[17] = 16;
        Assert.False(VmTgaTextureImage.TryParseHeader(lump, out _));
    }

    [Fact]
    public void CreatePalettedPixels_Grayscale8_UsesRemap()
    {
        var lump = new byte[VmTgaTextureImage.HeaderSize + 2];
        lump[2] = 3;
        lump[12] = 1;
        lump[14] = 2;
        lump[16] = 8;
        lump[17] = 32;
        lump[VmTgaTextureImage.HeaderSize] = 4;
        lump[VmTgaTextureImage.HeaderSize + 1] = 5;
        Assert.True(VmTgaTextureImage.TryParseHeader(lump, out var header));
        var pixels = VmTgaTextureImage.CreatePalettedPixels(lump, header);
        Assert.Equal(new byte[] { 4, 5 }, pixels);
    }

    [Fact]
    public void DecodePaletted_FlipsUncompressedImage()
    {
        var lump = BuildPaletted2x2();
        Assert.True(VmTgaTextureImage.TryParseHeader(lump, out var header));
        var pixels = VmTgaTextureImage.DecodePaletted(lump, header, luminanceConversion: false);
        Assert.Equal(4, pixels.Length);
        Assert.Equal(pixels[0], pixels[2]);
    }

    private static byte[] BuildPaletted2x2()
    {
        var lump = new byte[VmTgaTextureImage.HeaderSize + 12 + 4];
        lump[1] = 1;
        lump[2] = 1;
        lump[5] = 4;
        lump[7] = 24;
        lump[12] = 2;
        lump[14] = 2;
        lump[16] = 8;
        lump[17] = 32;
        var imageOffset = VmTgaTextureImage.HeaderSize + 12;
        lump[imageOffset] = 0;
        lump[imageOffset + 1] = 1;
        lump[imageOffset + 2] = 0;
        lump[imageOffset + 3] = 1;
        return lump;
    }
}
