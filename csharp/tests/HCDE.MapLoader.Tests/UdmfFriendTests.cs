namespace HCDE.MapLoader.Tests;

public class UdmfFriendTests
{
    [Theory]
    [InlineData("ZDoom", true)]
    [InlineData("ZDoomTranslated", true)]
    [InlineData("Vavoom", true)]
    [InlineData("Doom", true)]
    [InlineData("Hexen", false)]
    [InlineData("Strife", false)]
    public void FriendRespectsNativeNamespace(string ns, bool expected)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; friend = true; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Friendly);
    }
}
