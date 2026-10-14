using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindMemoryTests
{
    [Fact]
    public void LoweredRuntimeLimitsApplyOnNextCapture()
    {
        var sim = Room(); var size = Save(sim).Length; var rewind = new RewindBuffer();
        rewind.Capture(sim); rewind.Capture(sim);
        rewind.MemoryLimitBytes = size; rewind.Capacity = 1;
        Assert.Equal(2, rewind.Count);
        rewind.Capture(sim);
        Assert.Equal(1, rewind.Count); Assert.Equal(size, rewind.TotalBytes);
        Assert.Equal(2, rewind.EvictionsForMemory);
        Assert.True(rewind.RestoreOldest(sim));
    }

    [Fact]
    public void PayloadBytesMatchRetainedArchivesAndRestoreDoesNotChangeAccounting()
    {
        var sim = Room(); var rewind = new RewindBuffer();
        var first = Save(sim); rewind.Capture(sim);
        sim.Tick(); var second = Save(sim); rewind.Capture(sim);
        Assert.Equal((long)first.Length + second.Length, rewind.TotalBytes);
        Assert.Equal(0, rewind.EvictionsForDepth); Assert.Equal(0, rewind.EvictionsForMemory);
        Assert.True(rewind.RestoreAtOrBefore(sim, 0)); Assert.Equal(first, Save(sim));
        Assert.Equal((long)first.Length + second.Length, rewind.TotalBytes);
    }

    [Fact]
    public void ExactBudgetFitsAndOneByteLessEvictsOldest()
    {
        var sim = Room(); var first = Save(sim); sim.Tick(); var second = Save(sim);
        var exact = new RewindBuffer { MemoryLimitBytes = first.Length + second.Length };
        var shortBudget = new RewindBuffer { MemoryLimitBytes = first.Length + second.Length - 1 };
        SimSavegame.Apply(sim, first); exact.Capture(sim); shortBudget.Capture(sim);
        SimSavegame.Apply(sim, second); exact.Capture(sim); shortBudget.Capture(sim);
        Assert.Equal(2, exact.Count); Assert.Equal(0, exact.EvictionsForMemory);
        Assert.Equal(1, shortBudget.Count); Assert.Equal(1, shortBudget.EvictionsForMemory);
        Assert.Equal(second.Length, shortBudget.TotalBytes);
        Assert.True(shortBudget.RestoreOldest(sim)); Assert.Equal(second, Save(sim));
    }

    [Fact]
    public void LargerCaptureCanEvictMultipleFramesAndRetainNewest()
    {
        var sim = Room(); var first = Save(sim);
        for (var i = 0; i < 4; i++) { var bot = sim.AddBot(i * 100, 200); bot.Brain!.Enabled = false; }
        sim.Tick();
        var larger = Save(sim); Assert.True(larger.Length > first.Length);
        var rewind = new RewindBuffer { MemoryLimitBytes = Math.Max(larger.Length, first.Length * 3L) };
        SimSavegame.Apply(sim, first);
        for (var i = 0; i < 3; i++) rewind.Capture(sim);
        SimSavegame.Apply(sim, larger); rewind.Capture(sim);
        Assert.True(rewind.EvictionsForMemory >= 2);
        Assert.True(rewind.TotalBytes <= rewind.MemoryLimitBytes);
        Assert.True(rewind.RestoreAtOrBefore(sim, 1)); Assert.Equal(larger, Save(sim));
        Assert.Equal((rewind.Count - 1L) * first.Length + larger.Length, rewind.TotalBytes);
    }

    [Fact]
    public void OversizedFrameEvictsExistingHistoryAndItself()
    {
        var sim = Room(); var size = Save(sim).Length;
        var rewind = new RewindBuffer { MemoryLimitBytes = size };
        rewind.Capture(sim);
        sim.AddBot(100, 0); Assert.True(Save(sim).Length > size);
        rewind.Capture(sim);
        Assert.Equal(0, rewind.Count); Assert.Equal(0, rewind.TotalBytes);
        Assert.Equal(2, rewind.EvictionsForMemory); Assert.Equal(0, rewind.EvictionsForDepth);
        Assert.False(rewind.RestoreOldest(sim));
    }

    [Fact]
    public void DepthAndMemoryEvictionsAreAccountedSeparately()
    {
        var sim = Room(); var rewind = new RewindBuffer { Capacity = 1 };
        rewind.Capture(sim); sim.Tick(); var expected = Save(sim); rewind.Capture(sim);
        Assert.Equal(1, rewind.Count); Assert.Equal(expected.Length, rewind.TotalBytes);
        Assert.Equal(1, rewind.EvictionsForDepth); Assert.Equal(0, rewind.EvictionsForMemory);
        Assert.True(rewind.RestoreOldest(sim)); Assert.Equal(expected, Save(sim));
    }

    [Fact]
    public void FailedSerializationPreservesHistoryBytesAndEvictionCounts()
    {
        var sim = Room(); var expected = Save(sim);
        var rewind = new RewindBuffer { Capacity = 1, MemoryLimitBytes = expected.Length };
        rewind.Capture(sim); var unsupported = new Thinker(); sim.Thinkers.Add(unsupported);
        Assert.Throws<InvalidOperationException>(() => rewind.Capture(sim));
        Assert.Equal(1, rewind.Count); Assert.Equal(expected.Length, rewind.TotalBytes);
        Assert.Equal(0, rewind.EvictionsForDepth); Assert.Equal(0, rewind.EvictionsForMemory);
        sim.Thinkers.Remove(unsupported);
        Assert.True(rewind.RestoreOldest(sim)); Assert.Equal(expected, Save(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DepthBelowOneRetainsOneFrameLikeNative(int capacity)
    {
        var sim = Room(); var rewind = new RewindBuffer { Capacity = capacity };
        rewind.Capture(sim); sim.Tick(); var expected = Save(sim); rewind.Capture(sim);
        Assert.Equal(1, rewind.Count); Assert.Equal(1, rewind.EvictionsForDepth);
        Assert.True(rewind.RestoreOldest(sim)); Assert.Equal(expected, Save(sim));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidMemoryLimitIsRejected(long limit)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RewindBuffer { MemoryLimitBytes = limit });
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
