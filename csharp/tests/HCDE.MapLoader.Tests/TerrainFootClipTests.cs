namespace HCDE.MapLoader.Tests;

public class TerrainFootClipTests
{
    [Theory]
    [InlineData("4.5", 4.5)]
    [InlineData("-2", -2)]
    public void ParsesSignedDepthAndModifyRetainsIt(string value, double expected)
    {
        Assert.True(TerrainDefinitionParser.TryParse($"terrain Water {{ footclip {value} }} terrain Water modify {{ damageamount 2 }}",
            out var definitions, out var error), error);
        Assert.Equal(expected, definitions.Single().FootClip);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("nope")]
    [InlineData("")]
    public void InvalidDepthRejects(string value)
        => Assert.False(TerrainDefinitionParser.TryParse($"terrain Water {{ footclip {value} }}", out _, out _));
}
