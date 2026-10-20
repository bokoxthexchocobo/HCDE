namespace HCDE.Client.Tests;

public class VmEmptyTextureImageTests
{
    [Fact]
    public void TryCreateFromLump_ValidatesEightZeroBytes()
    {
        var source = VmEmptyTextureImage.TryCreateFromLump(new byte[8], 7);
        Assert.Equal(7, source.SourceLump);
        Assert.True(source.Masked);
        Assert.Equal(new byte[] { 0 }, VmEmptyTextureImage.CreatePalettedPixels(source).ToArray());
    }

    [Fact]
    public void CreateDefault_MatchesNativeConstructor()
    {
        var source = VmEmptyTextureImage.CreateDefault();
        Assert.Equal(1, source.Width);
        Assert.True(source.UseGamePalette);
    }
}
