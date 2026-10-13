namespace HCDE.MapLoader.Tests;

public class UdmfDropActorsTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BinaryDefaultsFollowBehaviorFormat(bool hexen)
    {
        var wad = hexen ? HexenLevelTests.HexenWad() : TestWadBuilder.BuildMinimalMapWad("MAP01");
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.All(level.Sectors, sector => Assert.Equal(!hexen, sector.DropActors));
        Assert.All(level.CopyForSimulation().Sectors, sector => Assert.Equal(!hexen, sector.DropActors));
    }

    [Theory]
    [InlineData("ZDoom", "", false)]
    [InlineData("ZDoom", "dropactors = true;", true)]
    [InlineData("ZDoom", "dropactors = false;", false)]
    [InlineData("ZDoomTranslated", "dropactors = true;", true)]
    [InlineData("Vavoom", "dropactors = true;", true)]
    [InlineData("Doom", "", true)]
    [InlineData("Doom", "dropactors = false;", true)]
    [InlineData("Heretic", "", true)]
    [InlineData("Strife", "", true)]
    [InlineData("Hexen", "", false)]
    [InlineData("Hexen", "dropactors = true;", false)]
    public void SectorFlagUsesNativeNamespaceDefaults(string ns, string field, bool expected)
    {
        var text = $$"""namespace = "{{ns}}"; sector { {{field}} }""";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        Assert.Equal(expected, LevelBuilder.FromUdmf(map, "MAP01").Sectors.Single().DropActors);
    }
}
