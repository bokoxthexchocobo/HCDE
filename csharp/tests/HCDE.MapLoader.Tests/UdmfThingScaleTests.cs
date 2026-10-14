namespace HCDE.MapLoader.Tests;

public class UdmfThingScaleTests
{
    [Theory]
    [InlineData("scale = 2; scalex = 3;", 3, 2)]
    [InlineData("scalex = 3; scale = 2;", 2, 2)]
    [InlineData("scale = 2; scalex = 3; scale = 4;", 4, 4)]
    [InlineData("scale = 2; scaley = 0;", 2, 0)]
    [InlineData("SCALEY = -2; SCALEX = 0.5;", 0.5, -2)]
    [InlineData("", 0, 0)]
    public void ScaleAssignmentsRespectSourceOrder(string properties, double x, double y)
    {
        var text = $$"""namespace = "ZDoom"; thing { type = 3004; {{properties}} }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var thing = Assert.Single(map.Things); Assert.Equal(x, thing.ScaleX); Assert.Equal(y, thing.ScaleY);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        Assert.Equal(x, level.Things.Single().ScaleX); Assert.Equal(y, level.CopyForSimulation().Things.Single().ScaleY);
    }

    [Theory]
    [InlineData("ZDoom")]
    [InlineData("ZDoomTranslated")]
    [InlineData("Doom")]
    [InlineData("Hexen")]
    [InlineData("Strife")]
    [InlineData("Vavoom")]
    public void ScaleHasNoNativeNamespaceGate(string ns)
    {
        var text = $$"""namespace = "{{ns}}"; thing { type = 3004; scale = 2; }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(2, LevelBuilder.FromUdmf(map, "MAP01").Things.Single().ScaleX);
    }

    [Fact]
    public void ScaleUsesNativeFloatPrecisionAndDoesNotLeakBetweenThings()
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; thing { scale = 0.1; } thing { }", out var map, out var error), error);
        Assert.Equal((double)(float)0.1, map.Things[0].ScaleX);
        Assert.Equal(0, map.Things[1].ScaleX); Assert.Equal(0, map.Things[1].ScaleY);
    }
}
