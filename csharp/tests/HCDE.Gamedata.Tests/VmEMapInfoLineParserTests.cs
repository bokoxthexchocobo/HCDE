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
}
