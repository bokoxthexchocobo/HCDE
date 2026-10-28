namespace HCDE.Gamedata.Tests;

public class DecorateBuiltinPropertyHandlersTests
{
    [Fact]
    public void AppliesTagThresholdAndRenderStyle()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecoratePropertyParamParser.TryParse("S", ["MyTag"], out var tagParsed, out _));
        Assert.True(DecorateActorBuiltinPropertyHandlers.TryApply("tag", def, tagParsed!.Values, out _));
        Assert.Equal("MyTag", def.Tag);

        Assert.True(DecoratePropertyParamParser.TryParse("I", ["4"], out var thresholdParsed, out _));
        Assert.True(DecorateActorBuiltinPropertyHandlers.TryApply("threshold", def, thresholdParsed!.Values, out _));
        Assert.Equal(4, def.Threshold);

        Assert.True(DecoratePropertyParamParser.TryParse("S", ["ADD"], out var styleParsed, out _));
        Assert.True(DecorateActorBuiltinPropertyHandlers.TryApply("renderstyle", def, styleParsed!.Values, out _));
        Assert.Equal("Add", def.RenderStyle);
    }

    [Fact]
    public void RouterAppliesExtendedBuiltinProperties()
    {
        var def = new DecorateActorDefinition();
        Assert.True(DecorateActorPropertyRouter.TryApplyProperty("gravity", "Actor", ["0.5"], def, out _));
        Assert.Equal(0.5, def.Gravity);
    }
}
