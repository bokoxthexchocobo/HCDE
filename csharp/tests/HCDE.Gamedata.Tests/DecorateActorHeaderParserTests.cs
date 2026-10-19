namespace HCDE.Gamedata.Tests;

public class DecorateActorHeaderParserTests
{
    [Fact]
    public void ParsesTypeParentColonSyntax()
    {
        var tokens = new[] { "Imp: DoomImp", "replaces", "OldImp", "3001" };
        var index = 0;
        var result = new DecorateActorHeaderParser.ActorHeaderParseResult();
        Assert.True(DecorateActorHeaderParser.TryParse(tokens, ref index, result));
        Assert.Equal("Imp", result.TypeName);
        Assert.Equal("DoomImp", result.ParentName);
        Assert.Equal("OldImp", result.ReplacesType);
        Assert.Equal(3001, result.DoomEdNum);
    }

    [Fact]
    public void RejectsSelfReplacement()
    {
        var tokens = new[] { "Actor", "replaces", "Actor" };
        var index = 0;
        var result = new DecorateActorHeaderParser.ActorHeaderParseResult();
        Assert.False(DecorateActorHeaderParser.TryParse(tokens, ref index, result));
        Assert.Contains(result.Messages, m => m.Contains("itself", StringComparison.OrdinalIgnoreCase));
    }
}
