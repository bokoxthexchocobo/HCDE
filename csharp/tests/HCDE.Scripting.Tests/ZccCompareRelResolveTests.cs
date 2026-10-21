namespace HCDE.Scripting.Tests;

public class ZccCompareRelResolveTests
{
    [Fact]
    public void NeedsTranslationCast_Below412() =>
        Assert.True(ZccCompareRelResolve.NeedsTranslationCastToInt(true, true, 40000));

    [Fact]
    public void StringComparisonPath_EitherSideString() =>
        Assert.True(ZccCompareRelResolve.StringComparisonPath(true, false));

    [Fact]
    public void FirstNumericStringCoerce_PrefersRightReplacement()
    {
        var action = ZccCompareRelResolve.FirstNumericStringCoerce(
            leftNumeric: true,
            rightNumeric: false,
            leftString: false,
            rightString: true,
            leftStringConstToChar: false,
            rightStringConstToChar: true);
        Assert.Equal(ZccCompareRelResolve.CoerceAction.ReplaceRightViaStringConstToChar, action);
    }

    [Fact]
    public void PlanStringPathCoercions_IncludesCastsAndResultType()
    {
        var steps = ZccCompareRelResolve.PlanStringPathCoercions(leftString: false, rightString: true);
        Assert.Equal(2, steps.Count);
        Assert.Equal(ZccCompareRelResolve.CoerceAction.ResultTypeString, steps[^1]);
    }

    [Fact]
    public void EntersNumericCompareBody_OnlyWhenBothNumericNonString()
    {
        Assert.True(ZccCompareRelResolve.EntersNumericCompareBody(false, false, true, true));
        Assert.False(ZccCompareRelResolve.EntersNumericCompareBody(true, false, true, true));
    }
}
