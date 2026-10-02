using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkFrameLifecycleTests
{
    [Theory]
    [InlineData(70)]
    [InlineData(133)]
    public void IceDebrisRunsAllFourFramesBeforeRemoval(int duration)
    {
        var chunk = new IceChunkActor(duration);
        for (var frame = 0; frame < 4; frame++)
        {
            Assert.Equal(frame, chunk.States.Current); Assert.Equal(duration, chunk.RemainingTics);
            for (var tic = 0; tic < duration - 1; tic++) { chunk.Tick(); Assert.False(chunk.Destroyed); }
            chunk.Tick();
            Assert.Equal(frame == 3, chunk.Destroyed);
        }
        Assert.Equal(0, chunk.RemainingTics);
    }

    [Fact]
    public void SimulatedFrameTransitionsResampleDurationWithinNativeBounds()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var chunk = new IceChunkActor(1) { Simulation = sim, Level = sim.Level };
        for (var frame = 1; frame < 4; frame++)
        {
            var random = sim.CombatRandomState;
            chunk.States.ForceRemainingTics(1); chunk.Tick();
            Assert.Equal(frame, chunk.States.Current); Assert.False(chunk.Destroyed);
            Assert.InRange(chunk.RemainingTics, 70, 133); Assert.NotEqual(random, sim.CombatRandomState);
        }
    }

    [Fact]
    public void RestoredFinalFrameDeterminesExpiry()
    {
        var chunk = new IceChunkActor(133); chunk.States.Restore(3, 2);
        Assert.Equal(2, chunk.RemainingTics); chunk.Tick(); Assert.False(chunk.Destroyed);
        chunk.Tick(); Assert.True(chunk.Destroyed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void InvalidDurationIsRejected(int duration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IceChunkActor(duration));
    }
}
