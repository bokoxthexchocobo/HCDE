namespace HCDE.MapLoader.Tests;

public class VmMapLoaderSetTextureVariantsTests
{
    [Fact]
    public void ShouldPrintMissingTextureWarning_UntilLimit()
    {
        Assert.True(VmMapLoaderSetTextureVariants.ShouldPrintMissingTextureWarning(8));
        Assert.False(VmMapLoaderSetTextureVariants.ShouldPrintMissingTextureWarning(9));
    }

    [Fact]
    public void TryParseLegacyColorHash_ValidSixChar()
    {
        Assert.True(VmMapLoaderSetTextureVariants.TryParseLegacyColorHash("ff00ff", out var color, out var valid));
        Assert.True(valid);
        Assert.Equal(0xff00ffu, color);
    }

    [Fact]
    public void IsWritableUserVar_RejectsNative()
    {
        Assert.False(VmMapLoaderSetTextureVariants.IsWritableUserVar(true, false, false, false, true));
    }

    [Fact]
    public void ResolveDeepWaterBlend_PrefersColormap()
    {
        var r = VmMapLoaderSetTextureVariants.ResolveDeepWaterBlend("WATERMAP", _ => 1u, _ => false);
        Assert.True(r.UseColormap);
    }

    [Fact]
    public void ShouldScanLinedefs_WhenTextureMissing()
    {
        Assert.True(VmMapLoaderSetTextureVariants.ShouldScanLinedefsForMissingSide(true));
    }

    [Fact]
    public void TruncateFlatName_EightChars()
    {
        Assert.Equal("ABCDEFGH", VmMapLoaderSetTextureVariants.TruncateFlatName("ABCDEFGHIJK"));
    }

    [Fact]
    public void ShouldSummarizeExtraUsage_AfterLimit()
    {
        Assert.True(VmMapLoaderSetTextureVariants.ShouldSummarizeExtraUsage(9));
    }

    [Fact]
    public void FormatBadSegMessage_Vertex_IncludesIndices()
    {
        var msg = VmMapLoaderLoadSegsBadseg.FormatBadSegMessage(
            new VmMapLoaderLoadSegsBadseg.BadSeg(0, 3, 99), 0, 50);
        Assert.Contains("Seg 3", msg);
        Assert.Contains("vertex 99", msg);
    }

    [Fact]
    public void CountSegs_DividesLumpByRecordSize()
    {
        Assert.Equal(4, VmMapLoaderLoadSegsBadseg.ComputeSegCount(48, 12));
    }

    [Fact]
    public void ShouldSnapSplitVertex_WhenAngleDeltaOneDegree()
    {
        Assert.True(VmMapLoaderLoadSegsBadseg.ShouldSnapSplitVertex(1.0));
    }
}
