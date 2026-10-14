namespace HCDE.MapLoader.Tests;

public class UdmfThingScoreTests
{
    [Theory]
    [InlineData("ZDoom")]
    [InlineData("ZDoomTranslated")]
    [InlineData("Doom")]
    [InlineData("Hexen")]
    [InlineData("Strife")]
    [InlineData("Vavoom")]
    public void ScoreReachesLevelInAllSupportedNamespaces(string ns)
    {
        var text = $$"""namespace = "{{ns}}"; thing { score = -7; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(-7, map.Things.Single().Score);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(-7, level.Things.Single().Score); Assert.Equal(-7, level.CopyForSimulation().Things.Single().Score);
    }

    [Fact]
    public void ScoreDefaultsToZeroAndLastAssignmentWins()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { score = 7; SCORE = -3; } thing { }", out var map, out var error), error);
        Assert.Equal(-3, map.Things[0].Score); Assert.Equal(0, map.Things[1].Score);
    }
}
