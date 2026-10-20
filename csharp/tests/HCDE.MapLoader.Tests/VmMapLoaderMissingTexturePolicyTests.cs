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
        Assert.Equal("ABCDEFGH", VmMapLoaderMissingTexturePolicy.TruncateFlatName("ABCDEFGHIJK", true));
    }

    [Fact]
    public void ShouldSummarizeExtraUsage_AfterLimit()
    {
        Assert.True(VmMapLoaderMissingTexturePolicy.ShouldSummarizeExtraUsage(25));
        Assert.Equal(5, VmMapLoaderMissingTexturePolicy.ExtraUsageCount(25));
    }

    [Fact]
    public void FormatSectorMissingMessage_IncludesIndex()
    {
        var msg = VmMapLoaderMissingTexturePolicy.FormatSectorMissingMessage("floor", "MISSING", 4);
        Assert.Contains("sector 4", msg);
        Assert.Contains("MISSING", msg);
    }

    [Fact]
    public void FormatWallMissingMessage_IncludesLinedef()
    {
        var msg = VmMapLoaderMissingTexturePolicy.FormatWallMissingMessage("middle", "BAD", "first", 12);
        Assert.Contains("linedef 12", msg);
        Assert.Contains("first", msg);
    }
}
