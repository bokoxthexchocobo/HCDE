namespace HCDE.MapLoader.Tests;

public class UdmfThingAlphaTests
{
    [Theory]
    [InlineData("ZDoom")]
    [InlineData("ZDoomTranslated")]
    [InlineData("Doom")]
    [InlineData("Hexen")]
    [InlineData("Strife")]
    [InlineData("Vavoom")]
    public void AlphaReachesLevelWithoutNamespaceGate(string ns)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 58; alpha = 0.1; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(0.1, map.Things.Single().Alpha);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(0.1, level.Things.Single().Alpha);
        Assert.Equal(0.1, level.CopyForSimulation().Things.Single().Alpha);
    }

    [Fact]
    public void MissingAlphaKeepsNativeSentinelAndAssignmentsArePerThing()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { alpha = 0.2; ALPHA = 0.3; } thing { }", out var map, out var error), error);
        Assert.Equal(0.3, map.Things[0].Alpha); Assert.Equal(-1, map.Things[1].Alpha);
        Assert.Equal(-1, LevelBuilder.FromUdmf(map, "MAP01").Things[1].Alpha);
    }
}
