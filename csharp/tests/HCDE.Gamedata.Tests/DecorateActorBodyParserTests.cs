namespace HCDE.Gamedata.Tests;

public class DecorateActorBodyParserTests
{
    [Fact]
    public void Body_AppliesHealthAndFlags()
    {
        var tokens = new List<string>
        {
            "{", "Health", "50", ";", "+", "SOLID", ";", "states", "}",
        };
        var context = new DecorateActorBodyParser.BodyParseContext();
        Assert.True(DecorateActorBodyParser.TryParseBody(tokens, context, out _));
        Assert.Equal(50, context.Definition.Flags.Health);
        Assert.True(context.Definition.Flags.Solid);
        Assert.True(context.StateSet);
    }

    [Fact]
    public void Body_RoutesTranslationProperty()
    {
        var tokens = new List<string> { "{", "translation", "0", "2", ";", "}" };
        var context = new DecorateActorBodyParser.BodyParseContext();
        Assert.True(DecorateActorBodyParser.TryParseBody(tokens, context, out _));
        Assert.Equal(2, context.Definition.TranslationId);
    }
}
