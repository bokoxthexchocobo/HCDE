using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FlickerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(1234)]
    public void NativeCountdownRangesIncludeTransitionTick(int seed)
    {
        var sim = Room(seed); Flicker(sim);
        Assert.Equal(220, sim.LightOf(0));
        var initial = Until(sim, 40, 66);
        Assert.Contains(initial, new[] { 2, 66 }); // Count is (random & 64)+1; transition is the following tick.
        for (var i = 0; i < 12; i++)
        {
            Assert.InRange(Until(sim, 220, 9), 2, 9);
            Assert.InRange(Until(sim, 40, 33), 2, 33);
        }
    }

    [Theory]
    [InlineData(int.MaxValue, int.MinValue, 32767, -32768)]
    [InlineData(40, 220, 40, 220)]
    public void BoundsClampIndependentlyWithoutSorting(int upper, int lower, int start, int end)
    {
        var sim = Room(); Flicker(sim, upper, lower);
        Assert.Equal(start, sim.LightOf(0));
        Until(sim, end, 66);
    }

    [Fact]
    public void RetriggerCreatesAnotherEffectAndStopRemovesEveryEffect()
    {
        var sim = Room(); Flicker(sim);
        Flicker(sim, 180, 20);
        Assert.Equal(180, sim.LightOf(0));
        Activate(sim, 117);
        for (var i = 0; i < 150; i++) { sim.Tick(); Assert.Equal(180, sim.LightOf(0)); }
    }

    [Fact]
    public void GlowSurvivesOverlappingFlicker()
    {
        var sim = Room();
        Activate(sim, 114, 200, 100, 1);
        Flicker(sim); // Does not destroy the glow; its first flicker tick only decrements.
        sim.Tick(); Assert.Equal(200, sim.LightOf(0));
    }

    [Fact]
    public void MissingTagDoesNotAdvanceRandomStreamAndReplayIsDeterministic()
    {
        var a = Room(); var b = Room();
        Activate(a, 115, 220, 40, tag: 999);
        Flicker(a); Flicker(b);
        for (var i = 0; i < 200; i++) { a.Tick(); b.Tick(); Assert.Equal(b.Checksum, a.Checksum); }
    }

    [Fact]
    public void FlickerDoesNotConsumeCombatOrStrobeRandomness()
    {
        var a = Room(); var b = Room(); Flicker(a);
        for (var i = 0; i < 200; i++) a.Tick();
        for (var i = 0; i < 20; i++) Assert.Equal(b.NextCombatRandom(), a.NextCombatRandom());
        Activate(a, 117);
        Activate(a, 112, 128); Activate(b, 112, 128);
        Activate(a, 232, 2, 3); Activate(b, 232, 2, 3);
        for (var i = 0; i < 40; i++) { a.Tick(); b.Tick(); Assert.Equal(b.LightOf(0), a.LightOf(0)); }
    }

    private static int Until(AuthoritySimulation sim, int target, int limit)
    {
        for (var ticks = 1; ticks <= limit; ticks++)
        {
            sim.Tick(); if (sim.LightOf(0) == target) return ticks;
        }
        Assert.Fail($"Light did not reach {target} within {limit} ticks."); return 0;
    }

    private static void Flicker(AuthoritySimulation sim, int upper = 220, int lower = 40) => Activate(sim, 115, upper, lower);
    private static void Activate(AuthoritySimulation sim, int special, int a = 0, int b = 0, int c = 0, int tag = 7) =>
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = special, Arg0 = tag, Arg1 = a, Arg2 = b, Arg3 = c, PlayerUse = true }, true));

    private static AuthoritySimulation Room(int seed = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, seed);
}
