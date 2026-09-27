using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AnimatedLightTests
{
    [Fact]
    public void FadeHasNativeFirstTickAndFinalWriteWithoutLockingImmediateChanges()
    {
        var sim = Room();
        Activate(sim, 113, 228, 4);
        Assert.Equal(128, sim.LightOf(0));
        AssertTimeline(sim, 128, 153, 178, 203, 228);
        Activate(sim, 113, 40, 0);
        Assert.Equal(40, sim.LightOf(0));
        sim.Tick();
        Assert.Equal(228, sim.LightOf(0)); // The existing thinker performs its final write.
        Activate(sim, 113, 40, 0);
        Assert.Equal(40, sim.LightOf(0));
    }

    [Fact]
    public void DescendingFadeTruncatesTowardZero()
    {
        var sim = Room(); Activate(sim, 113, 121, 3);
        AssertTimeline(sim, 128, 126, 124, 121, 121);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveFadeIsImmediateAndDoesNotLock(int duration)
    {
        var sim = Room(); Activate(sim, 113, -72, duration);
        Assert.Equal(-72, sim.LightOf(0));
        Activate(sim, 113, 328, duration);
        AssertTimeline(sim, 328, 328);
    }

    [Theory]
    [InlineData(200, 100)]
    [InlineData(100, 200)]
    public void GlowSortsBoundsAndReversesWithoutEndpointPause(int upper, int lower)
    {
        var sim = Room(); Activate(sim, 114, upper, lower, 2);
        AssertTimeline(sim, 200, 150, 100, 150, 200, 150, 100);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveGlowDoesNotChangeOrLockSector(int duration)
    {
        var sim = Room(); Activate(sim, 114, 200, 100, duration);
        AssertTimeline(sim, 128, 128);
        Activate(sim, 113, 40, 0);
        Assert.Equal(40, sim.LightOf(0));
    }

    [Fact]
    public void StrobeBeginsDarkAndUsesSeparateDurations()
    {
        var sim = Room(); Activate(sim, 116, 220, 40, 2, 3);
        Assert.Equal(128, sim.LightOf(0));
        AssertTimeline(sim, 40, 40, 40, 220, 220, 40, 40, 40, 220);
    }

    [Fact]
    public void StrobeStartingAtLowerBoundBeginsBrightWithoutSortingBounds()
    {
        var sim = Room(); Activate(sim, 116, 40, 128, 2, 3);
        AssertTimeline(sim, 40, 40, 128, 128, 128, 40);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveStrobeDurationStaysInThatPhase(int duration)
    {
        var sim = Room(); Activate(sim, 116, 220, 40, 2, duration);
        AssertTimeline(sim, 40, 40, 40, 40, 40, 40);
        Activate(sim, 117);
        Activate(sim, 116, 220, 40, duration, 2);
        AssertTimeline(sim, 220, 220, 220, 220);
    }

    [Fact]
    public void OverlappingEffectsRunInCreationOrderAndStopRemovesAll()
    {
        var sim = Room(); Activate(sim, 113, 228, 4);
        AssertTimeline(sim, 128, 153);
        Activate(sim, 114, 500, 0, 1);
        Activate(sim, 116, 500, 0, 1, 1);
        AssertTimeline(sim, 0);
        Activate(sim, 117);
        AssertTimeline(sim, 0, 0, 0);
        Activate(sim, 114, 200, 100, 1);
        AssertTimeline(sim, 200, 100, 200);
    }

    [Fact]
    public void StopOnlyAffectsMatchingTag()
    {
        var sim = Room(); Activate(sim, 114, 200, 100, 2);
        Activate(sim, 114, 250, 50, 2, tag: 8);
        sim.Tick(); Activate(sim, 117);
        sim.Tick();
        Assert.Equal(200, sim.LightOf(0)); Assert.Equal(150, sim.LightOf(1));
    }

    [Fact]
    public void EqualTargetFadeDoesNotReserveSector()
    {
        var sim = Room(); Activate(sim, 113, 128, 4);
        Activate(sim, 113, 35, 0);
        Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void PendingEffectAffectsChecksumBeforeBrightnessChanges()
    {
        var a = Room(); var b = Room();
        Activate(a, 113, 200, 4);
        a.Tick(); b.Tick();
        Assert.Equal(a.LightOf(0), b.LightOf(0));
        Assert.NotEqual(a.Checksum, b.Checksum);
    }

    [Fact]
    public void ExtremeGlowBoundsClampAndInterpolateWithoutOverflow()
    {
        var sim = Room(); Activate(sim, 114, int.MinValue, int.MaxValue, 2);
        AssertTimeline(sim, short.MaxValue, 0, short.MinValue, -1, short.MaxValue);
    }

    [Fact]
    public void UnmatchedTagSucceedsAndLeavesExistingEffectsAlone()
    {
        var sim = Room(); Activate(sim, 114, 200, 100, 1);
        Activate(sim, 117, tag: 999);
        Activate(sim, 113, 0, 0, tag: 999);
        AssertTimeline(sim, 200, 100, 200);
    }

    private static void Activate(AuthoritySimulation sim, int special, int a = 0, int b = 0,
        int c = 0, int d = 0, int tag = 7)
    {
        var line = new LevelLine { Special = special, Arg0 = tag, Arg1 = a, Arg2 = b,
            Arg3 = c, Arg4 = d, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special);
    }

    private static void AssertTimeline(AuthoritySimulation sim, params int[] lights)
    {
        foreach (var light in lights) { sim.Tick(); Assert.Equal(light, sim.LightOf(0)); }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Index = 0, Tag = 7, CeilingHeight = 128, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 128, LightLevel = 220 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
