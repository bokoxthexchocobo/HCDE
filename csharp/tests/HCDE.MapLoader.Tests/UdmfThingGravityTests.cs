namespace HCDE.MapLoader.Tests;
public class UdmfThingGravityTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", false)]
    [InlineData("Doom", false)]
    [InlineData("Hexen", false)]
    [InlineData("Strife", false)]
    public void ThingGravityRespectsNativeNamespace(string ns, bool enabled)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; gravity = -0.25; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(enabled ? -0.25 : 1, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Gravity);
    }
    [Fact]
    public void MissingGravityDefaultsToOne()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { type = 3004; }", out var map, out var error), error);
        Assert.Equal(1, Assert.Single(map.Things).Gravity);
        Assert.Equal(1, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Gravity);
    }
}