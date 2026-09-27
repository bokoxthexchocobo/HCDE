using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightCompatibilityTests
{
    [Theory]
    [InlineData(false, 220)]
    [InlineData(true, 40)]
    public void CompatibilityCarriesFirstResolvedMaximumAcrossTaggedSectors(bool enabled, int second)
    {
        var sim = Room(enabled, 40, 220); Activate(sim, 234);
        Assert.Equal(40, sim.LightOf(0)); Assert.Equal(second, sim.LightOf(1));
    }

    [Theory]
    [InlineData(false, -72, -50)]
    [InlineData(true, -72, -50)]
    [InlineData(false, -1, -1)]
    [InlineData(true, -1, -1)]
    public void NegativeChangeRetainsRequestedSearchBaseline(bool enabled, int value, int second)
    {
        var sim = Room(enabled, -100, -50); Activate(sim, 112, value);
        Assert.Equal(value, sim.LightOf(0)); Assert.Equal(second, sim.LightOf(1));
    }

    [Fact]
    public void NegativeSharedResultContinuesSearchingUntilNonnegativeResult()
    {
        var sim = Room(true, -40, -60); Activate(sim, 112, -100);
        Assert.Equal(-40, sim.LightOf(0)); Assert.Equal(-40, sim.LightOf(1));
    }

    [Theory]
    [InlineData(110, 20, 148)]
    [InlineData(111, 20, 108)]
    [InlineData(112, 35, 35)]
    [InlineData(233, 0, 128)]
    public void CompatibilityDoesNotAlterOtherLightOperations(int special, int value, int second)
    {
        var sim = Room(true, 40, 220); Activate(sim, special, value);
        Assert.Equal(second, sim.LightOf(1));
    }

    private static void Activate(AuthoritySimulation sim, int special, int value = 0) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = special, Arg0 = 7, Arg1 = value, PlayerUse = true }, true));

    private static AuthoritySimulation Room(bool enabled, short firstNeighbor, short secondNeighbor) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { Index = 1, Tag = 7, LightLevel = 128, CeilingHeight = 128 },
                new LevelSector { Index = 2, LightLevel = firstNeighbor, CeilingHeight = 128 },
                new LevelSector { Index = 3, LightLevel = secondNeighbor, CeilingHeight = 128 }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 },
                new LevelSide { Sector = 2 }, new LevelSide { Sector = 3 }],
            Lines = [new LevelLine { SideFront = 0, SideBack = 2 }, new LevelLine { SideFront = 1, SideBack = 3 }],
            Things = [new LevelThing { Type = 1 }],
        }, compat: enabled ? CompatSurface.SharedLightMaximum : CompatSurface.None);
}
