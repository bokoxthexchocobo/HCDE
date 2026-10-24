namespace HCDE.Client.Tests;

public class VmAutomapArrowParserTests
{
    [Fact]
    public void ComputeArrowRadius_MatchesNativeFormula()
    {
        Assert.Equal(18, VmAutomapArrowParser.ComputeArrowRadius(16));
    }

    [Fact]
    public void ParseArrowLump_ReadsOneSegment()
    {
        const string lump = "(0.5, -0.7), (1, 0)";
        var lines = VmAutomapArrowParser.ParseArrowLump(lump, 18);
        Assert.Single(lines);
        Assert.Equal(9, lines[0].Ax, 3);
    }

    [Fact]
    public void FormatMarkTextureName_MatchesAmmnumPattern()
    {
        Assert.Equal("AMMNUM3", VmAutomapArrowParser.FormatMarkTextureName(3));
    }

    [Fact]
    public void ValidatePrimaryArrowDefined_ThrowsWhenEmpty()
    {
        Assert.Throws<InvalidOperationException>(() => VmAutomapArrowParser.ValidatePrimaryArrowDefined(0));
    }

    [Fact]
    public void DefaultThinTriangle_HasThreeSegments()
    {
        Assert.Equal(3, VmAutomapArrowParser.DefaultThinTriangle.Length);
    }
}
