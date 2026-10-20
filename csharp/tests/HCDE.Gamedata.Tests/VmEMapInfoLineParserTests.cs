namespace HCDE.Gamedata.Tests;

public class VmEMapInfoLineParserTests
{
    [Fact]
    public void SplitPropertyLine_ParsesEqualsForm()
    {
        Assert.True(VmEMapInfoLineParser.SplitPropertyLine("sky = BLUE", out var key, out var value));
        Assert.Equal("sky", key);
        Assert.Equal("BLUE", value);
    }

    [Fact]
    public void StripLineComment_HonorsQuotes()
    {
        Assert.Equal("name \"foo#bar\"", VmEMapInfoLineParser.StripLineComment("name \"foo#bar\" # tail"));
    }

    [Fact]
    public void NormalizeResourceName_StripsLeadingSlashes()
    {
        Assert.Equal("textures/wall.png", VmEMapInfoLineParser.NormalizeResourceName("\\textures/wall.png"));
    }

    [Fact]
    public void SplitValueList_SplitsOnComma()
    {
        Assert.Equal(["A", "B"], VmEMapInfoLineParser.SplitValueList("A,B"));
    }

    [Fact]
    public void TryParseBool_TreatsEmptyAsTrue()
    {
        Assert.True(VmEMapInfoLineParser.TryParseBool("", out var v) && v);
    }
}
