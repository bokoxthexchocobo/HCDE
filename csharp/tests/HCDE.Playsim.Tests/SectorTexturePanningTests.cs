using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorTexturePanningTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, FloorTextureOffsetX = 99,
            CeilingTextureOffsetX = 99, FloorTextureBaseOffsetY = 12 },
            new LevelSector { Index = 1, Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
    });

    [Theory]
    [InlineData(186)]
    [InlineData(187)]
    public void MapActionReplacesSelectedOffsetsOnAllTaggedSectors(int special)
    {
        var sim = Room();
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = -2, Arg2 = 25,
            Arg3 = 3, Arg4 = -150, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        foreach (var sector in sim.Level.Sectors.Take(2))
        {
            Assert.Equal(-1.75, special == 187 ? sector.FloorTextureOffsetX : sector.CeilingTextureOffsetX);
            Assert.Equal(1.5, special == 187 ? sector.FloorTextureOffsetY : sector.CeilingTextureOffsetY);
        }
        Assert.Equal(99, special == 187 ? sim.Level.Sectors[0].CeilingTextureOffsetX : sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(12, sim.Level.Sectors[0].FloorTextureBaseOffsetY);
        Assert.Equal(0, sim.Level.Sectors[2].FloorTextureOffsetX);
        Assert.Equal(0, line.Special);
    }

    [Theory]
    [InlineData(false, 186)]
    [InlineData(true, 186)]
    [InlineData(false, 187)]
    [InlineData(true, 187)]
    public void AcsDirectAndStackPanningSelectUntaggedSectors(bool stack, int special)
    {
        var sim = Room();
        int[] words = stack ? [3, 0, 3, 4, 3, 50, 3, -3, 3, -25, 8, special, 1]
            : [13, special, 0, 4, 50, -3, -25, 1];
        var code = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(code.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = code });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        var sector = sim.Level.Sectors[2];
        Assert.Equal(4.5, special == 187 ? sector.FloorTextureOffsetX : sector.CeilingTextureOffsetX);
        Assert.Equal(-3.25, special == 187 ? sector.FloorTextureOffsetY : sector.CeilingTextureOffsetY);
        Assert.Equal(99, sim.Level.Sectors[0].FloorTextureOffsetX);
    }

    [Fact]
    public void LoadedMapPanningAndRuntimeReplacementRetainExistingScroller()
    {
        var text = $$"""
            namespace = "ZDoom";
            vertex { x = 0; y = 0; }
            vertex { x = 64; y = 0; }
            sector { id = 7; heightceiling = 128; xpanningfloor = 10.5; ypanningfloor = -2.25;
                xpanningceiling = -7.75; ypanningceiling = 3.5; }
            sidedef { sector = 0; }
            linedef { v1 = 0; v2 = 1; sidefront = 0; special = {{LineSpecials.ScrollFloor}}; arg0 = 7; arg3 = 160; arg4 = 128; }
            """;
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sim = AuthoritySimulation.Start(LevelBuilder.FromUdmf(map, "MAP01"));
        var sector = Assert.Single(sim.Level.Sectors);
        sim.Tick();
        Assert.Equal(9.5, sector.FloorTextureOffsetX);
        Assert.Equal(-2.25, sector.FloorTextureOffsetY);
        Assert.Equal(-7.75, sector.CeilingTextureOffsetX);
        Assert.Equal(3.5, sector.CeilingTextureOffsetY);
        var line = new LevelLine { Special = 187, Arg0 = 7, Arg1 = 20, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        sim.Tick();
        Assert.Equal(19, sector.FloorTextureOffsetX);
        Assert.Equal(0, sector.FloorTextureOffsetY);
    }

    [Fact]
    public void MissingTagReturnsSuccessWithoutChangingOffsets()
    {
        var sim = Room();
        var line = new LevelLine { Special = 187, Arg0 = 999, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(99, sim.Level.Sectors[0].FloorTextureOffsetX);
    }
}
