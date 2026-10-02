using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UdmfRotatedScrollTests
{
    [Fact]
    public void LoadedMapScrollsFloorAndCeilingUsingImportedRotations()
    {
        var text = $$"""
            namespace = "ZDoom";
            vertex { x = 0; y = 0; }
            vertex { x = 64; y = 0; }
            sector { id = 7; heightceiling = 128; rotationfloor = 90; rotationceiling = -90; }
            sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0;
                special = {{LineSpecials.ScrollFloor}}; arg0 = 7; arg3 = 160; arg4 = 128; }
            linedef { v1 = 0; v2 = 1; sidefront = 0;
                special = {{LineSpecials.ScrollCeiling}}; arg0 = 7; arg3 = 160; arg4 = 128; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        sim.Tick();
        var sector = Assert.Single(sim.Level.Sectors);
        Assert.Equal(0, sector.FloorTextureOffsetX, 6);
        Assert.Equal(1, sector.FloorTextureOffsetY, 6);
        Assert.Equal(0, sector.CeilingTextureOffsetX, 6);
        Assert.Equal(-1, sector.CeilingTextureOffsetY, 6);
    }
}
