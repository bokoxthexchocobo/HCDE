namespace HCDE.Gamedata.Tests;

public class DecoratePropertyParamParserTests
{
    [Fact]
    public void EmptySpecAcceptsNoTokens()
    {
        Assert.True(DecoratePropertyParamParser.TryParse("0", [], out var result, out _));
        Assert.NotNull(result);
        Assert.Empty(result!.Values);
    }

    [Fact]
    public void ParsesIntegerFloatStringTriple()
    {
        var tokens = new[] { "12", "1.5", "pickup" };
        Assert.True(DecoratePropertyParamParser.TryParse("IFS", tokens, out var result, out _));
        Assert.Equal(3, result!.Values.Count);
        Assert.Equal(DecoratePropertyKind.Integer, result.Values[0].Kind);
        Assert.Equal(12, result.Values[0].IntValue);
        Assert.Equal(DecoratePropertyKind.Float, result.Values[1].Kind);
        Assert.Equal(1.5, result.Values[1].DoubleValue);
        Assert.Equal("pickup", result.Values[2].StringValue);
    }

    [Fact]
    public void ParsesColorTuple()
    {
        var tokens = new[] { "10", ",", "20", ",", "30" };
        Assert.True(DecoratePropertyParamParser.TryParse("C", tokens, out var result, out _));
        var color = Assert.Single(result!.Values);
        Assert.Equal(DecoratePropertyKind.ColorRgb, color.Kind);
        Assert.Equal(10, color.ColorR);
        Assert.Equal(20, color.ColorG);
        Assert.Equal(30, color.ColorB);
    }

    [Fact]
    public void ParsesMorphStyleParameter()
    {
        var tokens = new[] { "MRF_FULLHEALTH | MRF_UNDOBYDEATH" };
        Assert.True(DecoratePropertyParamParser.TryParse("M", tokens, out var result, out _));
        Assert.Equal((1 << 1) | (1 << 9), Assert.Single(result!.Values).IntValue);
    }

    [Fact]
    public void ParsesStringListParameter()
    {
        var tokens = new[] { "A", ",", "B", ",", "C" };
        Assert.True(DecoratePropertyParamParser.TryParse("L", tokens, out var result, out _));
        var list = Assert.Single(result!.Values);
        Assert.Equal(DecoratePropertyKind.StringList, list.Kind);
        Assert.Equal(new[] { "A", "B", "C" }, list.StringList);
    }

    [Fact]
    public void ParsesExpressionOrInteger()
    {
        var tokens = new[] { "(", "a", "+", "1", ")" };
        Assert.True(DecoratePropertyParamParser.TryParse("X", tokens, out var result, out _));
        var expr = Assert.Single(result!.Values);
        Assert.Equal(DecoratePropertyKind.Expression, expr.Kind);
        Assert.Equal(new[] { "a", "+", "1" }, expr.ExpressionTokens);
    }

    [Fact]
    public void RejectsMissingToken()
    {
        Assert.False(DecoratePropertyParamParser.TryParse("II", new[] { "1" }, out _, out var error));
        Assert.Contains("Expected token", error, StringComparison.Ordinal);
    }
}
