namespace HCDE.Gamedata.Tests;

public class DecorateFlagExpressionTests
{
    [Fact]
    public void ParsesIntegerLiteral()
    {
        Assert.True(DecorateFlagExpression.TryParse("42", DecorateFlagExpression.MorphStyleFlags, out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void ParsesPipeCombinedMorphFlags()
    {
        Assert.True(DecorateFlagExpression.TryParse(
            "MRF_FULLHEALTH | MRF_UNDOBYDEATH",
            DecorateFlagExpression.MorphStyleFlags,
            out var value));
        Assert.Equal((1 << 1) | (1 << 9), value);
    }

    [Fact]
    public void ParsesParenthesizedThingActivation()
    {
        Assert.True(DecorateFlagExpression.TryParse(
            "(THINGSPEC_Activate | THINGSPEC_Switch)",
            DecorateFlagExpression.ThingActivationFlags,
            out var value));
        Assert.Equal((1 << 8) | (1 << 10), value);
    }

    [Fact]
    public void RejectsUnknownFlagName()
    {
        Assert.False(DecorateFlagExpression.TryParse("NOT_A_FLAG", DecorateFlagExpression.MorphStyleFlags, out _));
    }
}
