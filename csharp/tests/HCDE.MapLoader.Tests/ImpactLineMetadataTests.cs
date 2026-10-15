using System.Text;

namespace HCDE.MapLoader.Tests;

public class ImpactLineMetadataTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UdmfImpactSurvivesConversionAndCopy(bool impact)
    {
        var text = "namespace=\"ZDoom\"; vertex{x=0;y=-64;} vertex{x=0;y=64;} "
            + "sector{heightceiling=128;} sidedef{sector=0;} linedef{v1=0;v2=1;sidefront=0;impact="
            + impact.ToString().ToLowerInvariant() + ";}";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map!, "MAP01");
        Assert.Equal(impact, level.Lines.Single().Impact);
        Assert.Equal(impact, level.CopyForSimulation().Lines.Single().Impact);
    }

    [Fact]
    public void MapinfoLevelNumberSurvivesLoadAndCopy()
    {
        var bytes = HexenLevelTests.HexenWad();
        Assert.True(WadArchiveReader.TryReadDirectory(bytes, out var entries, out _));
        var lumps = entries.Select(e => (e.Name, bytes.AsSpan((int)e.FilePosition, (int)e.Size).ToArray()))
            .Append(("MAPINFO", Encoding.UTF8.GetBytes("map MAP01 { levelnum = 42 }"))).ToArray();
        Assert.True(LevelBuilder.TryFromWad(MapsModsTests.Wad(lumps), "MAP01", out var level, out var error), error);
        Assert.Equal(42, level.LevelNumber); Assert.Equal(42, level.CopyForSimulation().LevelNumber);
    }
}
