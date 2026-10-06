namespace HCDE.MapLoader.Tests;

public class UdmfNoAttackTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("noattack = false;", false)]
    [InlineData("noattack = true;", true)]
    public void SectorFlagReachesBuiltLevel(string field, bool expected)
    {
        var text = $$"""namespace = "ZDoom"; sector { {{field}} }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, LevelBuilder.FromUdmf(map, "MAP01").Sectors.Single().NoAttack);
    }
}
