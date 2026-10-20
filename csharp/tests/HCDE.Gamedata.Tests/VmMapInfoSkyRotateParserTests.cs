namespace HCDE.Gamedata.Tests;

public class VmMapInfoSkyRotateParserTests
{
    [Fact]
    public void TryParseVector_Normalizes()
    {
        var tokens = new List<string> { "0", "0", "2" };
        var i = 0;
        Assert.True(VmMapInfoSkyRotateParser.TryParseVector(tokens, ref i, out var v));
        Assert.Equal(1f, v.Z, 3);
    }
}
