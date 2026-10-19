using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightEvParityTests
{
    [Fact]
    public void LightChangeRaisesAndLowersTaggedSectors()
    {
        var sim = Room();
        Assert.True(LightActions.LightChange(sim, 7, 40));
        Assert.Equal(168, sim.LightOf(0));
        Assert.True(LightActions.LightChange(sim, 7, -80));
        Assert.Equal(88, sim.LightOf(0));
    }

    [Fact]
    public void TurnTagLightsOffUsesNeighborMinimum()
    {
        var sim = Room();
        sim.Lights[0] = 200;
        sim.Lights[1] = 96;
        Assert.True(LightActions.TurnTagLightsOff(sim, 7));
        Assert.Equal(96, sim.LightOf(0));
    }

    [Fact]
    public void LightTurnOnPartwayInterpolatesBetweenNeighborBounds()
    {
        var sim = Room();
        sim.Lights[0] = 64;
        Assert.True(LightActions.LightTurnOnPartway(sim, 7, 0.5));
        Assert.Equal(142, sim.LightOf(0));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors =
        [
            new LevelSector { Index = 0, Tag = 7, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, LightLevel = 220 },
        ],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
