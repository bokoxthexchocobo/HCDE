namespace HCDE.Client.Tests;

public class VmImgzTextureImageTests
{
    [Fact]
    public void CreatePalettedPixels_UncompressedColumnMajor()
    {
        var lump = new byte[VmImgzTextureImage.HeaderSize + 4];
        "IMGZ"u8.CopyTo(lump);
        lump[4] = 2;
        lump[6] = 2;
        lump[12] = 0;
        lump[VmImgzTextureImage.HeaderSize] = 1;
        lump[VmImgzTextureImage.HeaderSize + 1] = 2;
        lump[VmImgzTextureImage.HeaderSize + 2] = 3;
        lump[VmImgzTextureImage.HeaderSize + 3] = 4;
        Assert.True(VmImgzTextureImage.TryCreate(lump, out var header, out _));
        var pixels = VmImgzTextureImage.CreatePalettedPixels(lump, header, isAlpha: false);
        Assert.Equal(new byte[] { 1, 3, 2, 4 }, pixels);
    }
}
