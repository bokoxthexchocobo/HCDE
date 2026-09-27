using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomStrobeTests
{
    [Theory]
    [InlineData(17, false, false)]
    [InlineData(156, false, true)]
    [InlineData(172, true, false)]
    [InlineData(193, true, true)]
    public void DoomTranslationsEnforceTriggerAndRepeat(int special, bool use, bool repeat)
    {
        var sim = Room(false); var line = new LevelLine { Special = special, Tag = 7 };
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, !use));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use));
        Assert.Equal(repeat ? special : 0, line.Special);
        WaitForDark(sim, 40);
        for (var i = 0; i < 34; i++) { sim.Tick(); Assert.Equal(40, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        for (var i = 0; i < 4; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(40, sim.LightOf(0));
    }

    [Theory]
    [InlineData(40, 40)]
    [InlineData(200, 0)]
    [InlineData(128, 0)]
    [InlineData(-72, -72)]
    public void NeighborMinimumIncludesCurrentAndFallsBackToZero(short neighbor, int expected)
    {
        var sim = Room(neighbor: neighbor); Start(sim);
        WaitForDark(sim, expected);
        sim.Tick(); Assert.Equal(expected, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.LightOf(0));
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(expected, sim.LightOf(0));
    }

    [Fact]
    public void MissingSectorDoesNotConsumeRandomnessOrReplaceEffect()
    {
        var a = Room(); var b = Room(); Start(a); Start(b);
        Start(a, tag: 999);
        for (var i = 0; i < 40; i++)
        {
            a.Tick(); b.Tick(); Assert.Equal(b.Checksum, a.Checksum);
        }
    }

    [Fact]
    public void LightingDoesNotConsumeCombatRandomnessAndSameSeedReplays()
    {
        var a = Room(); var b = Room(); var control = Room(); Start(a); Start(b);
        for (var i = 0; i < 100; i++)
        {
            a.Tick(); b.Tick();
            Assert.Equal(b.Checksum, a.Checksum);
            Assert.Equal(control.NextCombatRandom(), a.NextCombatRandom());
            b.NextCombatRandom();
        }
    }

    [Fact]
    public void UsesRuntimeNeighborLightAndStopAllowsRestart()
    {
        var sim = Room();
        Activate(sim, new LevelLine { Special = 112, Arg0 = 8, Arg1 = 16, PlayerUse = true });
        Start(sim); WaitForDark(sim, 16);
        Activate(sim, new LevelLine { Special = 117, Arg0 = 7, PlayerUse = true });
        for (var i = 0; i < 10; i++) { sim.Tick(); Assert.Equal(16, sim.LightOf(0)); }
        Start(sim); // Current and minimum now match, so restart uses zero.
        var delay = 0;
        while (sim.LightOf(0) != 0 && delay < 8) { sim.Tick(); delay++; }
        Assert.Equal(0, sim.LightOf(0));
    }

    private static void WaitForDark(AuthoritySimulation sim, int expected)
    {
        Assert.Equal(128, sim.LightOf(0));
        var delay = 0;
        while (sim.LightOf(0) == 128 && delay < 8) { sim.Tick(); delay++; }
        Assert.InRange(delay, 1, 8); Assert.Equal(expected, sim.LightOf(0));
    }

    private static void Start(AuthoritySimulation sim, int tag = 7) => Activate(sim,
        new LevelLine { Special = 232, Arg0 = tag, Arg1 = 2, Arg2 = 3, PlayerUse = true });

    private static void Activate(AuthoritySimulation sim, LevelLine line) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));

    private static AuthoritySimulation Room(bool extended = true, short neighbor = 40) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = extended ? MapDataFormat.HexenBinary : MapDataFormat.DoomBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 },
            new LevelSector { Index = 1, Tag = 8, LightLevel = neighbor, CeilingHeight = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
