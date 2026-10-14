using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RewindSelectionTests
{
    [Theory]
    [InlineData(int.MinValue, 0)]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(4, 4)]
    [InlineData(8, 4)]
    [InlineData(10, 10)]
    [InlineData(int.MaxValue, 10)]
    public void SparseHistorySelectsLatestFrameNotAfterRequest(int target, int expectedTic)
    {
        var sim = Room(); var expected = new Dictionary<int, byte[]>();
        for (var tic = 0; tic <= 10; tic++)
        {
            if (tic is 0 or 4 or 10)
            {
                sim.Rewind.Capture(sim);
                expected[tic] = Save(sim);
            }
            sim.Tick();
        }

        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, target));

        Assert.Equal(expectedTic, sim.Thinkers.Clock.Tic);
        Assert.Equal(expected[expectedTic], Save(sim)); Assert.Equal(3, sim.Rewind.Count);
    }

    [Fact]
    public void EqualTickFramesUseFirstCapturedState()
    {
        var sim = Room(); var bot = sim.AddBot(100, 0); bot.Brain!.Enabled = false;
        sim.Rewind.Capture(sim); var expected = Save(sim);
        bot.Health = 7; sim.Rewind.Capture(sim);
        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, 0));
        Assert.Equal(expected, Save(sim)); Assert.Equal(2, sim.Rewind.Count);
    }

    [Fact]
    public void SelectionUsesTickRatherThanInsertionOrderAfterAnEarlierRestore()
    {
        var sim = Room(); sim.Rewind.Capture(sim);
        for (var i = 0; i < 10; i++) sim.Tick();
        sim.Rewind.Capture(sim); var expected = Save(sim);
        Assert.True(sim.Rewind.RestoreOldest(sim));
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim);

        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, 10));

        Assert.Equal(expected, Save(sim)); Assert.Equal(3, sim.Rewind.Count);
    }

    [Fact]
    public void EvictedHistoryFallsBackToOldestRetainedFrame()
    {
        var sim = Room(); var rewind = new RewindBuffer { Capacity = 2 };
        rewind.Capture(sim); sim.Tick(); rewind.Capture(sim); var expected = Save(sim);
        sim.Tick(); rewind.Capture(sim);
        Assert.True(rewind.RestoreAtOrBefore(sim, 0));
        Assert.Equal(expected, Save(sim)); Assert.Equal(2, rewind.Count);
    }

    [Fact]
    public void SelectedFrameReconstructsItsActorsAndReplaysGameplay()
    {
        var sim = Room(); var bot = sim.AddBot(100, 0, 3001);
        sim.Players.Single().Invulnerable = true;
        sim.Rewind.Capture(sim);
        for (var i = 0; i < 4; i++) sim.Tick();
        sim.Rewind.Capture(sim); var expected = Save(sim);
        var replay = Enumerable.Range(0, 20).Select(_ => Advance(sim)).ToArray();
        bot.Destroy(); sim.Tick(); sim.Rewind.Capture(sim);

        Assert.True(sim.Rewind.RestoreAtOrBefore(sim, 8));

        Assert.Equal(expected, Save(sim));
        Assert.Equal(replay, Enumerable.Range(0, 20).Select(_ => Advance(sim)).ToArray());
    }

    [Fact]
    public void UnsupportedSelectedRestoreLeavesSimulationAndRingUnchanged()
    {
        var sim = Room(); sim.Rewind.Capture(sim);
        sim.Players.Single().Destroy(); sim.Tick(); var before = Save(sim);
        Assert.Throws<InvalidOperationException>(() => sim.Rewind.RestoreAtOrBefore(sim, 0));
        Assert.Equal(before, Save(sim)); Assert.Equal(1, sim.Rewind.Count);
    }

    [Fact]
    public void EmptyRingReturnsFalseWithoutMutation()
    {
        var sim = Room(); var before = Save(sim);
        Assert.False(sim.Rewind.RestoreAtOrBefore(sim, 0)); Assert.Equal(before, Save(sim));
    }

    private static byte[] Save(AuthoritySimulation sim) => SimSavegame.Write(sim, includeBotIdentities: true);
    private static uint Advance(AuthoritySimulation sim) { sim.Tick(); return sim.Checksum; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1, X = 800 }]
    });
}
