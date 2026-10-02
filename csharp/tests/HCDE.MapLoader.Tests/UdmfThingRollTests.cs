namespace HCDE.MapLoader.Tests;

public class UdmfThingRollTests
{
    [Theory]
    [InlineData("Doom", 90, 90)]
    [InlineData("ZDoom", -90, -90)]
    [InlineData("ZDoomTranslated", 65537, 1)]
    [InlineData("Vavoom", 32768, -32768)]
    [InlineData("ZDoom", -65537, -1)]
    public void RollUsesNativeSignedShortConversion(string ns, int input, int expected)
    {
        var text = $"namespace = \"{ns}\"; thing {{ type = 2035; roll = {input}; }}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(input, Assert.Single(map.Things).Roll);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Roll);
    }

    [Fact]
    public void MissingRollDefaultsToZero()
    {
        Assert.True(UdmfTextMapParser.TryParse("thing { type = 2035; }", out var map, out var error), error);
        Assert.Equal(0, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Roll);
    }

    [Fact]
    public void FractionalRollIsRejected()
    {
        Assert.False(UdmfTextMapParser.TryParse("thing { roll = 1.5; }", out _, out _));
    }
}
