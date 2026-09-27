using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightActionsTests
{
    [Theory]
    [InlineData(13, false, 255, false)]
    [InlineData(35, false, 35, false)]
    [InlineData(79, false, 35, true)]
    [InlineData(81, false, 255, true)]
    [InlineData(138, true, 255, true)]
    [InlineData(139, true, 35, true)]
    [InlineData(170, true, 35, false)]
    [InlineData(171, true, 255, false)]
    public void DoomLightingUsesActivationAndRepeatRules(int special, bool use, int brightness, bool repeat)
    {
        var sim = Room(); var line = new LevelLine { Special = special, Tag = 7 };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, !use));
        Assert.Equal(128, sim.LightOf(0));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use));
        Assert.Equal(brightness, sim.LightOf(0));
        Assert.Equal(220, sim.LightOf(1));
        Assert.Equal(repeat ? special : 0, line.Special);
        Assert.Equal(128, sim.Level.Sectors[0].LightLevel); // Map definition stays immutable.
    }

    [Theory]
    [InlineData(110, 200, 328)]
    [InlineData(111, 200, -72)]
    [InlineData(110, int.MaxValue, short.MaxValue)]
    [InlineData(111, int.MaxValue, short.MinValue)]
    [InlineData(112, 35, 35)]
    [InlineData(112, -1, 220)]
    [InlineData(233, 0, 128)]
    [InlineData(234, 0, 220)]
    public void ExtendedActionsClampOrReadCurrentNeighbor(int special, int value, int expected)
    {
        var sim = Room(true);
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = value, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void MaximumCanLowerLightAndMinimumIncludesCurrentSector()
    {
        var sim = Room(true); var player = sim.Players.Single();
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 112, Arg0 = 8, Arg1 = 40, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 234, Arg0 = 7, PlayerUse = true }, true));
        Assert.Equal(40, sim.LightOf(0));
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 112, Arg0 = 8, Arg1 = 240, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, player,
            new LevelLine { Special = 233, Arg0 = 7, PlayerUse = true }, true));
        Assert.Equal(40, sim.LightOf(0));
    }

    [Fact]
    public void MissingTagSucceedsWithoutChangingAnySector()
    {
        var sim = Room(); var line = new LevelLine { Special = 13, Tag = 999 };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, false));
        Assert.Equal(0, line.Special); Assert.Equal(128, sim.LightOf(0)); Assert.Equal(220, sim.LightOf(1));
    }

    [Fact]
    public void LightingParticipatesInSimulationChecksum()
    {
        var a = Room(); var b = Room();
        LineSpecials.ActivateMapLine(a, a.Players.Single(), new LevelLine { Special = 13, Tag = 7 }, false);
        a.Tick(); b.Tick();
        Assert.NotEqual(a.Checksum, b.Checksum);
    }

    [Fact]
    public void ZeroTagTargetsUntaggedSectorsAndIsolatedMaximumIsMinusOne()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Index = 0, Tag = 0, LightLevel = 128 },
                new LevelSector { Index = 1, Tag = 7, LightLevel = 220 }],
            Things = [new LevelThing { Type = 1 }],
        });
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 234, Arg0 = 0, PlayerUse = true }, true));
        Assert.Equal(-1, sim.LightOf(0));
        Assert.Equal(220, sim.LightOf(1));
    }

    private static AuthoritySimulation Room(bool extended = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 128, LightLevel = 220 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
