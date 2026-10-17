namespace HCDE.Gamedata.Tests;

public class MapMonsterTelefragTests
{
    [Theory]
    [InlineData("map MAP01 { allowmonstertelefrags }", true)]
    [InlineData("map MAP01 { ALLOWMONSTERTELEFRAGS }", true)]
    [InlineData("defaultmap { allowmonstertelefrags } map MAP01 { }", true)]
    [InlineData("map MAP01 { }", false)]
    public void ParsesNativeUnaryFlagAndDefaultInheritance(string text, bool expected)
    {
        Assert.True(MapInfoParser.TryParse(text, out var data, out var error), error);
        Assert.Equal(expected, data.FindMap("MAP01")!.AllowMonsterTelefrags);
    }

    [Fact]
    public void DefaultPersistsAcrossLumpsWithoutMutatingEarlierSet()
    {
        Assert.True(MapInfoParser.TryParse("defaultmap { allowmonstertelefrags } map MAP01 { }", out var first, out var error), error);
        Assert.True(MapInfoParser.TryParse("map MAP02 { }", out var next, out error, first), error);
        Assert.True(next.FindMap("MAP02")!.AllowMonsterTelefrags);
        Assert.Null(first.FindMap("MAP02"));
    }
}
