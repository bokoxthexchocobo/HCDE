namespace HCDE.Gamedata.Tests;

public class NativeColorTableTests
{
    [Fact]
    public void TableUsesDecimalComponentsFirstNameAndNullTermination()
    {
        Assert.True(NativeColorParser.TryReadNames("! comment\r\n1 2 3 Custom Shade\n4 5 6 CUSTOM SHADE\n\0invalid", out var names, out var error), error);
        Assert.Equal(0x010203u, Assert.Single(names).Value);
        Assert.True(NativeColorParser.TryParse("custom shade", out var color, names)); Assert.Equal(0x010203u, color);
    }

    [Fact]
    public void SuppliedTableReplacesBundledNamesButRetainsHexSyntax()
    {
        Assert.True(NativeColorParser.TryReadNames("10 20 30 custom", out var names, out _));
        Assert.False(NativeColorParser.TryParse("AliceBlue", out _, names));
        Assert.True(NativeColorParser.TryParse("#abc", out var color, names)); Assert.Equal(0xaabbccu, color);
        Assert.True(MapInfoParser.TryParse("GameInfo { PickupColor = \"custom\" }", out var set, out _, colorNames: names));
        Assert.Equal(0x0a141eu, set.PickupColor);
    }

    [Theory]
    [InlineData("256 0 0 invalid")]
    [InlineData("a 0 0 invalid")]
    [InlineData("0 0 0")]
    public void MalformedTableRejectsWithoutPartialNames(string row)
    {
        Assert.False(NativeColorParser.TryReadNames("1 2 3 valid\n" + row, out var names, out var error));
        Assert.Empty(names); Assert.NotNull(error);
    }
}
