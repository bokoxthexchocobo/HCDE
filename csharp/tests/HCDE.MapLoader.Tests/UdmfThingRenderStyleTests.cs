namespace HCDE.MapLoader.Tests;

public class UdmfThingRenderStyleTests
{
    [Theory]
    [InlineData("None", 0)]
    [InlineData("Normal", 1)]
    [InlineData("Fuzzy", 2)]
    [InlineData("SoulTrans", 3)]
    [InlineData("OptFuzzy", 4)]
    [InlineData("Stencil", 5)]
    [InlineData("Translucent", 6)]
    [InlineData("Add", 7)]
    [InlineData("Additive", 7)]
    [InlineData("Shaded", 8)]
    [InlineData("TranslucentStencil", 9)]
    [InlineData("Shadow", 10)]
    [InlineData("Subtract", 11)]
    [InlineData("Subtractive", 11)]
    [InlineData("AddStencil", 12)]
    [InlineData("AddShaded", 13)]
    [InlineData("Multiply", 14)]
    [InlineData("ColorBlend", 16)]
    [InlineData("ColorAdd", 18)]
    public void NativeNamesAndAliasesReachLevel(string name, int expected)
    {
        var text = $$"""namespace = "ZDoom"; thing { renderstyle = "{{name}}"; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, map.Things.Single().RenderStyle);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(expected, level.Things.Single().RenderStyle);
        Assert.Equal(expected, level.CopyForSimulation().Things.Single().RenderStyle);
    }

    [Theory]
    [InlineData("ZDoom")]
    [InlineData("ZDoomTranslated")]
    [InlineData("Doom")]
    [InlineData("Hexen")]
    [InlineData("Strife")]
    [InlineData("Vavoom")]
    public void MapRenderStyleHasNoNamespaceGate(string ns)
    {
        var text = $$"""namespace = "{{ns}}"; thing { RENDERSTYLE = "tRaNsLuCeNt"; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(6, LevelBuilder.FromUdmf(map, "MAP01").Things.Single().RenderStyle);
    }

    [Theory]
    [InlineData("renderstyle = \"unknown\";", null)]
    [InlineData("renderstyle = \"Source\";", null)]
    [InlineData("renderstyle = \"InverseMultiply\";", null)]
    [InlineData("renderstyle = \"Add\"; renderstyle = \"unknown\";", 7)]
    [InlineData("renderstyle = \"Add\"; renderstyle = \"None\";", 0)]
    public void UnsupportedNamesDoNotOverwriteEarlierValidAssignment(string properties, int? expected)
    {
        var text = $$"""namespace = "ZDoom"; thing { {{properties}} } thing { }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, map.Things[0].RenderStyle); Assert.Null(map.Things[1].RenderStyle);
    }
}
