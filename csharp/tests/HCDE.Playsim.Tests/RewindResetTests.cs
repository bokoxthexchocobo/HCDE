using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindResetTests
{
    [Fact]
    public void DisableClearsHistoryAndLeavesTheWorldUnchanged()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Tick();
        Assert.Equal(1, sim.Rewind.Count); Assert.True(sim.Rewind.TotalBytes > 0);
        var before = Save(sim);

        sim.RewindEnabled = false;

        Assert.Equal(0, sim.Rewind.Count); Assert.Equal(0, sim.Rewind.TotalBytes);
        Assert.False(sim.Rewind.RestoreOldest(sim)); Assert.False(sim.Rewind.RestoreAtOrBefore(sim, 0));
        Assert.Equal(before, Save(sim));
        sim.Tick(); Assert.Equal(0, sim.Rewind.Count);
    }

    [Fact]
    public void ReenableCapturesImmediatelyRatherThanWaitingForTheOldTimer()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Tick();
        sim.RewindEnabled = false; sim.RewindEnabled = true;
        sim.Tick(); var expected = Save(sim);
        Assert.Equal(1, sim.Rewind.Count);
        sim.Thinkers.Clock.Restore(20);
        Assert.True(sim.Rewind.RestoreOldest(sim)); Assert.Equal(expected, Save(sim));
        Assert.Equal(2, sim.Thinkers.Clock.Tic);
    }

    [Fact]
    public void AssigningDisabledAgainClearsManuallyCapturedFrames()
    {
        var sim = Room(); Assert.False(sim.RewindEnabled);
        sim.Rewind.Capture(sim); Assert.Equal(1, sim.Rewind.Count);
        sim.RewindEnabled = false;
        Assert.Equal(0, sim.Rewind.Count); Assert.Equal(0, sim.Rewind.TotalBytes);
    }

    [Fact]
    public void ExplicitResetRestartsTimingWithoutDisablingAutomaticCapture()
    {
        var sim = Room(); sim.RewindEnabled = true; sim.Rewind.Capture(sim);
        Assert.False(sim.Rewind.CaptureIfDue(sim));
        sim.Rewind.Reset();
        Assert.True(sim.RewindEnabled); Assert.True(sim.Rewind.CaptureIfDue(sim));
        Assert.Equal(1, sim.Rewind.Count); Assert.False(sim.Rewind.CaptureIfDue(sim));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResetPreservesConfigurationAndCumulativeEvictions(bool memoryEviction)
    {
        var sim = Room(); var rewind = new RewindBuffer
        {
            Capacity = 1, CaptureIntervalSeconds = 0.5f,
            MemoryLimitBytes = memoryEviction ? 1 : 32L * 1024 * 1024
        };
        rewind.Capture(sim); sim.Tick(); rewind.Capture(sim);
        var memory = rewind.EvictionsForMemory; var depth = rewind.EvictionsForDepth;
        Assert.True(memoryEviction ? memory == 2 : depth == 1);

        rewind.Reset(); rewind.Reset();

        Assert.Equal(0, rewind.Count); Assert.Equal(0, rewind.TotalBytes);
        Assert.Equal(memory, rewind.EvictionsForMemory); Assert.Equal(depth, rewind.EvictionsForDepth);
        Assert.Equal(1, rewind.Capacity); Assert.Equal(0.5f, rewind.CaptureIntervalSeconds);
        Assert.Equal(memoryEviction ? 1 : 32L * 1024 * 1024, rewind.MemoryLimitBytes);
        sim.RewindEnabled = true; Assert.True(rewind.CaptureIfDue(sim));
    }

    [Fact]
    public void ResetDoesNotPreventCapturingAndReconstructingNewHistory()
    {
        var sim = Room(); var old = sim.AddBot(100, 0); old.Brain!.Enabled = false;
        sim.Rewind.Capture(sim); sim.Rewind.Reset();
        old.Destroy(); sim.Tick();
        var bot = sim.AddBot(300, 0); bot.Brain!.Enabled = false;
        sim.Rewind.Capture(sim); var expected = Save(sim);
        bot.Destroy(); sim.Tick();

        Assert.True(sim.Rewind.RestoreOldest(sim));

        Assert.Equal(expected, Save(sim)); Assert.DoesNotContain(sim.Actors, actor => actor.Id == old.Id);
        Assert.Contains(sim.Actors, actor => actor.Id == bot.Id && !actor.Destroyed);
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
