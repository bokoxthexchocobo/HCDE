namespace HCDE.Gamedata.Tests;

public class VmUMapInfoTokenParserTests
{
    [Fact]
    public void TryParseMultiString_JoinsQuotedParts()
    {
        var tokens = new List<string> { "\"line1\"", ",", "\"line2\"" };
        var i = 0;
        Assert.True(VmUMapInfoTokenParser.TryParseMultiString(tokens, ref i, out var text, []));
        Assert.Contains("line1", text);
        Assert.Contains('\n', text);
    }

    [Fact]
    public void TryParsePlayerActionOption_ParsesAllow()
    {
        Assert.True(VmUMapInfoTokenParser.TryParsePlayerActionOption("allow", out var opt));
        Assert.Equal(VmUMapPlayerActionOption.Allow, opt);
    }
}
