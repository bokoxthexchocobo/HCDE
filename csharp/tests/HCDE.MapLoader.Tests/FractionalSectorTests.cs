using System.Text;

namespace HCDE.MapLoader.Tests;

public class FractionalSectorTests
{
    [Fact]
    public void WadUdmfLoaderPreservesFractionalPlaneHeights()
    {
        var text = """
            namespace = "ZDoom";
            sector { heightfloor = -0.125; heightceiling = 128.875; }
            """;
        var wad = MapsModsTests.Wad(("MAP01", []), ("TEXTMAP", Encoding.UTF8.GetBytes(text)), ("ENDMAP", []));
        Assert.True(LevelBuilder.TryFromWad(wad, "MAP01", out var level, out var error), error);
        Assert.Equal((-0.125, 128.875), (level.Sectors[0].FloorHeight, level.Sectors[0].CeilingHeight));
    }

    [Theory]
    [InlineData(double.NaN, 128)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(-32769, 128)]
    [InlineData(0, 32768)]
    public void NonFiniteOrUnrepresentableSectorPlanesAreRejected(double floor, double ceiling)
    {
        Assert.False(LevelValidation.TryValidate(new PlayLevel {
            Sectors = [new LevelSector { FloorHeight = floor, CeilingHeight = ceiling }],
        }, out _));
    }
}
