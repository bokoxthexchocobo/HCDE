using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorRotationTests
{
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.UdmfText,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 7, CeilingHeight = 128 },
            new LevelSector { Index = 2, CeilingHeight = 128 }],
    });

    [Fact]
    public void MapActivationReplacesAllTaggedRotationsAndPreservesBase()
    {
        var sim = Room();
        sim.Level.Sectors[0].FloorTextureBaseAngle = 0x40000000u;
        var line = new LevelLine { Special = 185, Arg0 = 7, Arg1 = 450, Arg2 = -90, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.Equal(0, line.Special);
        foreach (var sector in sim.Level.Sectors.Take(2))
        {
            Assert.Equal(0x40000000u, sector.FloorTextureAngle);
            Assert.Equal(0xc0000000u, sector.CeilingTextureAngle);
        }
        Assert.Equal(0x40000000u, sim.Level.Sectors[0].FloorTextureBaseAngle);
        Assert.Equal(0u, sim.Level.Sectors[2].FloorTextureAngle);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcsDirectAndStackCallsSelectUntaggedSectors(bool stack)
    {
        var sim = Room();
        int[] words = stack ? [3, 0, 3, 180, 3, 90, 6, 185, 1] : [11, 185, 0, 180, 90, 1];
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
        Assert.True(sim.Acs.TryExecute(1, []));
        sim.Acs.Tick(sim);
        Assert.Equal(0u, sim.Level.Sectors[0].FloorTextureAngle);
        Assert.Equal(0x80000000u, sim.Level.Sectors[2].FloorTextureAngle);
        Assert.Equal(0x40000000u, sim.Level.Sectors[2].CeilingTextureAngle);
    }

    [Fact]
    public void MissingTagStillReturnsNativeSuccess()
    {
        var sim = Room();
        var line = new LevelLine { Special = 185, Arg0 = 999, Arg1 = 90, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        Assert.All(sim.Level.Sectors, sector => Assert.Equal(0u, sector.FloorTextureAngle));
    }

    [Fact]
    public void RuntimeRotationChangesExistingScrollerOnNextTick()
    {
        var sim = Room();
        var sector = sim.Level.Sectors[0];
        sector.FloorTextureBaseAngle = 0x40000000u;
        sector.CeilingTextureBaseAngle = 0xc0000000u;
        sim.SetSectorTextureScroll(0, 1, 0, SectorTextureScrollPlane.Floor);
        sim.SetSectorTextureScroll(0, 1, 0, SectorTextureScrollPlane.Ceiling);
        sim.Tick();
        Assert.Equal(-1, sector.FloorTextureOffsetY, 6);
        Assert.Equal(1, sector.CeilingTextureOffsetY, 6);
        var line = new LevelLine { Special = 185, Arg0 = 7, Arg1 = -90, Arg2 = 90, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
        sim.Tick();
        Assert.Equal(1, sector.FloorTextureOffsetX, 6);
        Assert.Equal(1, sector.CeilingTextureOffsetX, 6);
        Assert.Equal(-1, sector.FloorTextureOffsetY, 6);
        Assert.Equal(1, sector.CeilingTextureOffsetY, 6);
    }
}
