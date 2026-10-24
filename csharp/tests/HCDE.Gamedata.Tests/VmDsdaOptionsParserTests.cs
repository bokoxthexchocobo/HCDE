namespace HCDE.Gamedata.Tests;

public class VmDsdaOptionsParserTests
{
    [Fact]
    public void Parse_ClampedKeyValuePairs()
    {
        Assert.True(VmDsdaOptionsParser.TryParse("# comment\nmonster_infighting 1\ncomp_stairs 2\n", out var opt));
        Assert.Equal(1, opt.MonsterInfighting);
        Assert.Equal(1, opt.CompStairs);
    }
}
