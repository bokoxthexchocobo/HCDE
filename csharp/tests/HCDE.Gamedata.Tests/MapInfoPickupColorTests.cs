namespace HCDE.Gamedata.Tests;

public class MapInfoPickupColorTests
{
    [Theory]
    [InlineData("d7 ba 45", 0xd7ba45u)]
    [InlineData("00 FF 80", 0x00ff80u)]
    [InlineData("123abc", 0x123abcu)]
    [InlineData("#123abc", 0x123abcu)]
    [InlineData("#aB3", 0xaabb33u)]
    [InlineData("RED", 0xff0000u)]
    [InlineData("alice blue", 0xf0f8ffu)]
    [InlineData("DarkSlateGray", 0x2f4f4fu)]
    [InlineData("#12", 0u)]
    public void ParsesSupportedRgbForms(string color, uint expected)
    {
        Assert.True(MapInfoParser.TryParse("GameInfo { PickupColor = \"" + color + "\" }", out var set, out var error), error);
        Assert.Equal(expected, set.PickupColor);
    }

    [Theory]
    [InlineData("100 00 00")]
    [InlineData("gg 00 00")]
    [InlineData("ff 00")]
    [InlineData("#ggg")]
    [InlineData("not a native color")]
    public void UnsupportedColorRejectsWithoutSuccessShape(string color)
        => Assert.False(MapInfoParser.TryParse("GameInfo { PickupColor = \"" + color + "\" }", out _, out _));

    [Fact]
    public void LaterLumpsInheritAndOverrideColor()
    {
        Assert.True(MapInfoParser.TryParse("GameInfo { PickupColor = \"12 34 56\" }", out var first, out _));
        Assert.True(MapInfoParser.TryParse("GameInfo { titlepage = TITLE }", out var inherited, out _, first));
        Assert.Equal(0x123456u, inherited.PickupColor);
        Assert.True(MapInfoParser.TryParse("GameInfo { PickupColor = \"00 ff 00\" }", out var final, out _, inherited));
        Assert.Equal(0x00ff00u, final.PickupColor);
    }
}
