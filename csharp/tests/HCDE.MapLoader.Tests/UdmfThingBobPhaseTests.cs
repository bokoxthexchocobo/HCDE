namespace HCDE.MapLoader.Tests;

public class UdmfThingBobPhaseTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Strife", false)]
    [InlineData("Vavoom", false)]
    public void MapPhaseRespectsNativeNamespace(string ns, bool enabled)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; floatbobphase = 42; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(42, Assert.Single(map.Things).FloatBobPhase);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(enabled ? 42 : -1, Assert.Single(level.Things).FloatBobPhase);
        Assert.Equal(enabled ? 42 : -1, Assert.Single(level.CopyForSimulation().Things).FloatBobPhase);
    }

    [Fact]
    public void MissingPhaseKeepsNativeSentinel()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 3004; }", out var map, out var error), error);
        Assert.Equal(-1, Assert.Single(map.Things).FloatBobPhase);
        Assert.Equal(-1, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).FloatBobPhase);
    }
}
