namespace HCDE.MapLoader.Tests;

public class VmMapLoaderMissingTexturePolicyTests
{
    [Fact]
    public void ShouldPrintWarning_UntilLimit()
    {
        Assert.True(VmMapLoaderMissingTexturePolicy.ShouldPrintWarning(20));
        Assert.False(VmMapLoaderMissingTexturePolicy.ShouldPrintWarning(21));
    }

    [Fact]
    public void TruncateFlatName_EightChars()
    {
        Assert.Equal("ABCDEFGH", VmMapLoaderMissingTexturePolicy.TruncateFlatName("ABCDEFGHIJ", true));
    }
}
