using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BuiltinSpawnInitializationTests
{
    [Theory]
    [InlineData(85, true, 4)]
    [InlineData(23, false, 6)]
    [InlineData(24, false, -1)]
    public void MapDecorationStartsWithPendingSpawnFrameAndRestoresWithoutEntry(int type, bool bright, int maximumTics)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        Assert.True(actor.HandleNoDelay); Assert.True(actor.JustSpawned);
        Assert.Equal(actor.SpawnState, actor.States.Current); Assert.Equal(bright, actor.FullBright);
        if (maximumTics > 0) Assert.InRange(actor.States.RemainingTics, 1, maximumTics);
        else Assert.Equal(-1, actor.States.RemainingTics);
        var saved = SimSavegame.Write(sim); var tics = actor.States.RemainingTics;
        var restored = Room(type); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored));
        Assert.True(restored.Actors.Single().HandleNoDelay);
        Assert.Equal(tics, restored.Actors.Single().States.RemainingTics);
        restored.Tick();
        if (!restored.Actors.Single().Destroyed) Assert.False(restored.Actors.Single().HandleNoDelay);
    }

    [Fact]
    public void PuffConstructorUsesSpawnFrameWithoutStartupAction()
    {
        var puff = new PuffActor(2);
        Assert.True(puff.HandleNoDelay); Assert.True(puff.FullBright);
        Assert.Equal(0, puff.States.Current); Assert.Equal(2, puff.States.RemainingTics);
        puff.Tick();
        Assert.False(puff.HandleNoDelay); Assert.Equal(1, puff.States.RemainingTics);
        puff.Tick();
        Assert.Equal(1, puff.States.Current); Assert.False(puff.FullBright);
        Assert.Equal(4, puff.States.RemainingTics);
    }

    [Fact]
    public void IceChunkConstructorKeepsSuppliedInitialDurationUntilSuccessor()
    {
        var sim = Room(3004); var control = Room(3004);
        var chunk = new IceChunkActor(2) { Simulation = sim };
        Assert.True(chunk.HandleNoDelay); Assert.Equal(2, chunk.RemainingTics);
        chunk.Tick(); Assert.False(chunk.HandleNoDelay); Assert.Equal(1, chunk.RemainingTics);
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        var expected = control.NextIceTics();
        chunk.Tick();
        Assert.Equal(1, chunk.States.Current); Assert.Equal(expected, chunk.RemainingTics);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = type }] }, rngSeed: 42);
}
