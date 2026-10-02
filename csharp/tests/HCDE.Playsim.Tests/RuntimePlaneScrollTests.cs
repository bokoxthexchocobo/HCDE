using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RuntimePlaneScrollTests
{
    [Fact]
    public void TagZeroSelectsAllUntaggedSectorsWithoutTriggerSideFallback()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector(), new LevelSector { Index = 1, Tag = 7 }] });
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 0, 1, 0, 1, 0));
        sim.Tick();
        Assert.Equal(1.0 / 32, sim.SectorScrollX[0]);
        Assert.Equal(0, sim.SectorScrollX[1]);
    }

    [Fact]
    public void CarryOnlyModeClearsExistingTextureScrollEvenWithLowSpeedBits()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { Tag = 7 }] });
        sim.SetSectorTextureScroll(0, 99, 0, SectorTextureScrollPlane.Floor);
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 7, 1, 0, 3, 0));
        sim.Tick(); sim.Tick();
        Assert.Equal(0, sim.Level.Sectors[0].FloorTextureOffsetX);
        Assert.Equal(1.0 / 32, sim.SectorScrollX[0]);
    }

    [Fact]
    public void MissingTagsReturnNativeSuccess()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 223, 999, 1, 0, 2, 0));
        Assert.True(LineSpecials.ExecuteFloorSpecial(sim, 224, 999, 1, 0, 0, 0));
    }
}
