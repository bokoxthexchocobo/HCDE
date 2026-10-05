using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeadLostSoulDecorationTests
{
    [Fact]
    public void FinalInheritedDeathFrameRemovesAfterSixTics()
    {
        var sim = Room(); var actor = sim.Actors.Single();
        Assert.Equal(20, actor.Radius.ToDouble()); Assert.Equal(16, actor.Height.ToDouble());
        Assert.False(actor.Solid); Assert.False(actor.Shootable); Assert.False(actor.IsMonster);
        Assert.Equal(6, actor.States.RemainingTics);
        for (var i = 0; i < 5; i++) sim.Tick();
        Assert.False(actor.Destroyed); Assert.Single(sim.Actors); Assert.Equal(1, actor.States.RemainingTics);
        sim.Tick(); Assert.True(actor.Destroyed); Assert.Empty(sim.Actors);
    }

    [Fact]
    public void ArchiveRestoresRemainingDeathFrameDuration()
    {
        var sim = Room(); sim.Tick(); sim.Tick();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        var restored = Room(); restored.RestoreState(state);
        Assert.Equal(4, restored.Actors.Single().States.RemainingTics);
        for (var i = 0; i < 3; i++) restored.Tick();
        Assert.Single(restored.Actors); restored.Tick(); Assert.Empty(restored.Actors);
    }

    [Fact]
    public void SpawnDoesNotRunEarlierDeathActions()
    {
        var sim = Room(); var actor = sim.Actors.Single(); var health = actor.Health;
        sim.Tick(); Assert.Equal(health, actor.Health); Assert.False(actor.Destroyed);
        Assert.Null(actor.Brain); Assert.Equal(5, actor.States.RemainingTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 23 }],
    });
}
