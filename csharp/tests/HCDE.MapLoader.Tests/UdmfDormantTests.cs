namespace HCDE.MapLoader.Tests;
public class UdmfDormantTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Hexen", true)]
    [InlineData("Doom", false)]
    [InlineData("Strife", false)]
    public void DormantRespectsNativeNamespace(string ns, bool expected)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; dormant = true; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Dormant);
    }
    [Fact]
    public void MissingDormantDefaultsToFalse()
    {
        Assert.True(UdmfTextMapParser.TryParse("thing { type = 3004; }", out var map, out var error), error);
        Assert.False(Assert.Single(map.Things).Dormant);
    }
}