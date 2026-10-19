namespace HCDE.Gamedata.Tests;

public class DecorateTranslationPropertyHandlerTests
{
    [Fact]
    public void StandardTranslation_AppliesId()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateTranslationPropertyHandler.TryParsePropertyTokens(["0", "3"], out var values, out _));
        Assert.True(DecorateTranslationPropertyHandler.TryApply(values, def, out _));
        Assert.Equal(3, def.TranslationId);
        Assert.False(def.TranslationUsesCustomRanges);
    }

    [Fact]
    public void CustomTranslation_CollectsRanges()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateTranslationPropertyHandler.TryParsePropertyTokens(["1", "112:128=144:160"], out var values, out _));
        Assert.True(DecorateTranslationPropertyHandler.TryApply(values, def, out _));
        Assert.True(def.TranslationUsesCustomRanges);
        Assert.Equal(-1, def.TranslationId);
        Assert.Single(def.TranslationCustomRanges);
    }

    [Fact]
    public void StandardTranslation_RejectsOutOfRange()
    {
        var result = new DecorateTranslationPropertyHandler.TranslationParseResult();
        Assert.False(DecorateTranslationPropertyHandler.TryParse(
            [DecoratePropertyValue.FromInt(0), DecoratePropertyValue.FromInt(9)],
            result,
            out _));
    }
}
