namespace HCDE.Scripting.Tests;

public class ZccFunctionCallMathBuiltinsTests
{
    [Fact]
    public void TryGetRandomDefaultBounds_MatchesNativeZeroArgRandom()
    {
        Assert.True(ZccFunctionCallMathBuiltins.TryGetRandomDefaultBounds(out var min, out var max));
        Assert.Equal(0, min);
        Assert.Equal(255, max);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(1, false)]
    public void TryValidateRandom_MatchesNativeArity(int argCount, bool expected)
        => Assert.Equal(expected, ZccFunctionCallMathBuiltins.TryValidateRandom("Random", argCount));

    [Fact]
    public void TryValidateClamp_RequiresThreeArguments()
    {
        Assert.True(ZccFunctionCallMathBuiltins.TryValidateClamp(3));
        Assert.False(ZccFunctionCallMathBuiltins.TryValidateClamp(2));
    }

    [Fact]
    public void TryFoldRandom_WrapsInclusiveRange()
    {
        Assert.True(ZccFunctionCallMathBuiltins.TryFoldRandom(0, 255, 300, out var value));
        Assert.Equal(44, value);
    }
}
