namespace HCDE.MapLoader.Tests;

public class UdmfThingPitchTests
{
    [Theory]
    [InlineData("Doom", 90, 90)]
    [InlineData("ZDoom", -90, -90)]
    [InlineData("ZDoomTranslated", 65537, 1)]
    [InlineData("Vavoom", 32768, -32768)]
    [InlineData("ZDoom", -65537, -1)]
    [InlineData("ZDoom", 450, 450)]
    public void PitchUsesNativeSignedShortConversion(string ns, int input, int expected)
    {
        var text = $"namespace = \"{ns}\"; thing {{ type = 2035; pitch = {input}; }}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(input, Assert.Single(map.Things).Pitch);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Pitch);
    }

    [Fact]
    public void MissingPitchDefaultsToZero()
    {
        Assert.True(UdmfTextMapParser.TryParse("thing { type = 2035; }", out var map, out var error), error);
        Assert.Equal(0, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Things).Pitch);
    }

    [Theory]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    public void InvalidPitchIntegerIsRejected(string value)
    {
        Assert.False(UdmfTextMapParser.TryParse($"thing {{ pitch = {value}; }}", out _, out _));
    }
}
