namespace HCDE.MapLoader.Tests;

public class UdmfDestructibleImportTests
{
    [Fact]
    public void LoadsLineAndSectorDestructibleHealthFromUdmf()
    {
        const string text = """
            namespace = "ZDoom";
            vertex { x = 0; y = 0; }
            vertex { x = 64; y = 0; }
            sector { id = 3; healthfloor = 40; healthceiling = 50; health3d = 60;
                healthfloorgroup = 1; healthceilinggroup = 2; health3dgroup = 3; }
            sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; health = 90; healthgroup = 7; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var level = LevelBuilder.FromUdmf(map, "MAP01");
        var sector = Assert.Single(level.Sectors);
        Assert.Equal(40, sector.HealthFloor);
        Assert.Equal(50, sector.HealthCeiling);
        Assert.Equal(60, sector.Health3D);
        Assert.Equal(1, sector.HealthFloorGroup);
        Assert.Equal(2, sector.HealthCeilingGroup);
        Assert.Equal(3, sector.Health3DGroup);
        var line = Assert.Single(level.Lines);
        Assert.Equal(90, line.Health);
        Assert.Equal(7, line.HealthGroup);
    }

    [Fact]
    public void LoadsUdmfSideScrollAndPerPartOffsets()
    {
        const string text = """
            namespace = "ZDoom";
            sidedef { sector = 0; offsetx = 1; offsety = 2;
                offsetx_top = 10; offsety_top = 10; xscrolltop = 0.5; yscrolltop = -0.25; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var side = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sides);
        Assert.Equal(11, side.TopTextureOffsetX);
        Assert.Equal(12, side.TopTextureOffsetY);
        var scroll = Assert.Single(side.MapLoadWallScrolls);
        Assert.Equal(0.5, scroll.Dx);
        Assert.Equal(-0.25, scroll.Dy);
        Assert.Equal(1, scroll.PartsMask);
    }
}
