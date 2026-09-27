using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LightSequenceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChainUsesInheritedBasesAndEvenPhaseSpacing(bool doom)
    {
        var sim = Room(doom, [2, 3, 4], [64, 0, 100], [(0, 1), (1, 2)]);
        sim.Tick(); // Three phases: 42, 21, 0.
        AssertLights(sim, 64, 95, 100);
        sim.Tick(); AssertLights(sim, 64, 111, 100);
        Assert.Equal(0, sim.Level.Sectors[1].LightLevel);
    }

    [Fact]
    public void BranchFollowsFirstMatchingLineOnly()
    {
        var sim = Room(false, [2, 3, 3], [64, 80, 100], [(0, 2), (0, 1)]);
        for (var i = 0; i < 44; i++) sim.Tick();
        Assert.NotEqual(100, sim.LightOf(2));
        Assert.Equal(80, sim.LightOf(1));
    }

    [Fact]
    public void CyclicChainTerminatesAndDoesNotDuplicateEffects()
    {
        // 1-2-3-4-1 closes an alternating sequence cycle.
        var sim = Room(false, [2, 3, 4, 3, 4], [64, 0, 0, 0, 0],
            [(0, 1), (1, 2), (2, 3), (3, 4), (4, 1)]);
        sim.Tick(); AssertLights(sim, 64, 64, 64, 239, 64);
    }

    [Fact]
    public void SecondStartCannotReuseConsumedSequence()
    {
        var sim = Room(false, [2, 3, 2], [64, 0, 100], [(0, 1), (2, 1)]);
        sim.Tick(); AssertLights(sim, 64, 64, 100);
        for (var i = 0; i < 52; i++) sim.Tick();
        Assert.Equal(239, sim.LightOf(1));
    }

    [Fact]
    public void IsolatedZeroBaseStartsAtZeroAndStillPulses()
    {
        var sim = Room(false, [2], [0], []);
        sim.Tick(); Assert.Equal(0, sim.LightOf(0));
        for (var i = 0; i < 52; i++) sim.Tick();
        Assert.Equal(233, sim.LightOf(0));
    }

    [Fact]
    public void ExtendedGeneralizedFlagsDoNotHideLightingSpecial()
    {
        var sim = Room(false, [258, 3], [64, 0], [(0, 1)]);
        for (var i = 0; i < 53; i++) sim.Tick();
        Assert.Equal(239, sim.LightOf(1));
    }

    [Fact]
    public void UninitializedFlaggedNeighborDoesNotMatchNativeExactSpecialSearch()
    {
        var sim = Room(false, [2, 259], [64, 0], [(0, 1)]);
        for (var i = 0; i < 64; i++) sim.Tick();
        Assert.Equal(0, sim.LightOf(1));
    }

    [Fact]
    public void StopRemovesEveryMemberWithMatchingTag()
    {
        var sim = Room(false, [2, 3, 4], [64, 0, 100], [(0, 1), (1, 2)]);
        sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(),
            new LevelLine { Special = 117, Arg0 = 7, PlayerUse = true }, true));
        for (var i = 0; i < 128; i++) sim.Tick();
        AssertLights(sim, 64, 95, 100);
    }

    private static void AssertLights(AuthoritySimulation sim, params int[] expected)
    { for (var i = 0; i < expected.Length; i++) Assert.Equal(expected[i], sim.LightOf(i)); }

    private static AuthoritySimulation Room(bool doom, int[] specials, short[] lights, (int A, int B)[] edges) =>
        AuthoritySimulation.Start(new PlayLevel
        {
            Format = doom ? MapDataFormat.DoomBinary : MapDataFormat.HexenBinary,
            Sectors = specials.Select((s, i) => new LevelSector { Index = i, Tag = 7, Special = (short)(doom ? s + 20 : s),
                LightLevel = lights[i], CeilingHeight = 128 }).ToArray(),
            Sides = specials.Select((_, i) => new LevelSide { Sector = i }).ToArray(),
            Lines = edges.Select(e => new LevelLine { SideFront = e.A, SideBack = e.B }).ToArray(),
            Things = [new LevelThing { Type = 1 }],
        });
}
