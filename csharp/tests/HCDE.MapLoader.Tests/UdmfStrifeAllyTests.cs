namespace HCDE.MapLoader.Tests;

public class UdmfStrifeAllyTests
{
    [Theory]
    [InlineData("Strife", "strifeally = true;", true)]
    [InlineData("ZDoom", "strifeally = true;", true)]
    [InlineData("ZDoomTranslated", "strifeally = true;", true)]
    [InlineData("Vavoom", "strifeally = true;", true)]
    [InlineData("Doom", "strifeally = true;", false)]
    [InlineData("Hexen", "strifeally = true;", false)]
    [InlineData("ZDoom", "friend = true; strifeally = false;", false)]
    [InlineData("ZDoom", "strifeally = true; friend = false;", false)]
    [InlineData("ZDoom", "friend = false; strifeally = true;", true)]
    [InlineData("ZDoom", "friend = true; strifeally = false; friend = true;", true)]
    [InlineData("Doom", "friend = true; strifeally = false;", true)]
    [InlineData("Strife", "strifeally = true; friend = false;", true)]
    public void AllyKeysFollowNamespaceAndSourceOrder(string ns, string fields, bool expected)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; {{fields}} }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Friendly);
    }
}
