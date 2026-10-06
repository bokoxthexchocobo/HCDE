namespace HCDE.MapLoader.Tests;

public class UdmfBlockUseTests
{
    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public void ParsesBlockUseIndependentlyOfOtherBlockingFlags(string value, bool expected)
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; linedef { blockuse = " + value + "; }", out var map, out var error), error);
        var line = map!.Linedefs.Single();
        Assert.Equal(expected, line.BlockUse); Assert.False(line.BlockEverything); Assert.False(line.Blocking);
    }
}
