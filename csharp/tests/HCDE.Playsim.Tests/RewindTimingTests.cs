using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindTimingTests
{
    [Fact]
    public void AutomaticCaptureStartsImmediatelyThenWaitsOneSecond()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Tick();
        var first = Save(sim); Assert.Equal(1, sim.Rewind.Count);
        for (var i = 0; i < 34; i++) sim.Tick();
        Assert.Equal(1, sim.Rewind.Count);
        sim.Tick(); Assert.Equal(2, sim.Rewind.Count);
        Assert.True(sim.Rewind.RestoreOldest(sim)); Assert.Equal(first, Save(sim));
    }

    [Theory]
    [InlineData(0.25f, 8)]
    [InlineData(0.3f, 10)]
    [InlineData(1f, 35)]
    [InlineData(30f, 1050)]
    public void CaptureIntervalUsesNativeTruncatedTickConversion(float seconds, int ticks)
    {
        var sim = Room(); sim.Rewind.CaptureIntervalSeconds = seconds; sim.RewindEnabled = true;
        sim.Rewind.Capture(sim);
        sim.Thinkers.Clock.Restore(ticks - 1); Assert.False(sim.Rewind.CaptureIfDue(sim));
        sim.Thinkers.Clock.Restore(ticks); Assert.True(sim.Rewind.CaptureIfDue(sim));
        Assert.False(sim.Rewind.CaptureIfDue(sim)); Assert.Equal(2, sim.Rewind.Count);
    }

    [Theory]
    [InlineData(-1f, 0.25f)]
    [InlineData(0f, 0.25f)]
    [InlineData(100f, 30f)]
    public void IntervalClampsToNativeRange(float requested, float expected)
    {
        var rewind = new RewindBuffer { CaptureIntervalSeconds = requested };
        Assert.Equal(expected, rewind.CaptureIntervalSeconds);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void NonfiniteIntervalsRejectWithoutChangingConfiguration(float seconds)
    {
        var rewind = new RewindBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => rewind.CaptureIntervalSeconds = seconds);
        Assert.Equal(1f, rewind.CaptureIntervalSeconds);
    }

    [Fact]
    public void DisabledSimulationDoesNotAutomaticallyCaptureButManualCaptureWorks()
    {
        var sim = Room(); sim.Tick(); Assert.False(sim.Rewind.CaptureIfDue(sim));
        Assert.Equal(0, sim.Rewind.Count);
        sim.Rewind.Capture(sim); Assert.Equal(1, sim.Rewind.Count);
        sim.RewindEnabled = true; Assert.False(sim.Rewind.CaptureIfDue(sim));
    }

    [Fact]
    public void ManualCaptureRestartsIntervalEvenWhenNoFrameFitsBudget()
    {
        var sim = Room(); sim.RewindEnabled = true;
        var rewind = new RewindBuffer { MemoryLimitBytes = 1 };
        rewind.Capture(sim); Assert.Equal(0, rewind.Count);
        sim.Thinkers.Clock.Restore(34); Assert.False(rewind.CaptureIfDue(sim));
        sim.Thinkers.Clock.Restore(35); Assert.True(rewind.CaptureIfDue(sim));
        Assert.Equal(0, rewind.Count); Assert.False(rewind.CaptureIfDue(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestoreRestartsIntervalAtSelectedTick(bool selectByTick)
    {
        var sim = Room(); sim.RewindEnabled = true;
        sim.Rewind.Capture(sim); var first = Save(sim);
        sim.Thinkers.Clock.Restore(100); sim.Rewind.Capture(sim);
        Assert.True(selectByTick ? sim.Rewind.RestoreAtOrBefore(sim, 0) : sim.Rewind.RestoreOldest(sim));
        Assert.Equal(first, Save(sim));
        sim.Thinkers.Clock.Restore(34); Assert.False(sim.Rewind.CaptureIfDue(sim));
        sim.Thinkers.Clock.Restore(35); Assert.True(sim.Rewind.CaptureIfDue(sim));
    }

    [Fact]
    public void FailedCaptureDoesNotDelayNextAttemptOrEvictHistory()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Rewind.Capture(sim);
        sim.Thinkers.Clock.Restore(35); var unsupported = new Thinker(); sim.Thinkers.Add(unsupported);
        Assert.Throws<InvalidOperationException>(() => sim.Rewind.CaptureIfDue(sim));
        Assert.Equal(1, sim.Rewind.Count);
        sim.Thinkers.Remove(unsupported);
        Assert.True(sim.Rewind.CaptureIfDue(sim)); Assert.Equal(2, sim.Rewind.Count);
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
