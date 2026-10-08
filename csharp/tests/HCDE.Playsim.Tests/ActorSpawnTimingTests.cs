using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSpawnTimingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SpawnTimingIsUnscaledButLaterEntryUsesSkillTiming(bool fast)
    {
        var actor = new Actor { AlwaysFast = fast };
        var sim = Room(slow: !fast); actor.Simulation = sim;
        actor.States.ConfigureSpawn(actor, [new(5, 0, Fast: true, Slow: true)], 0);
        Assert.Equal(5, actor.States.RemainingTics);
        actor.States.Enter(actor, 0);
        Assert.Equal(fast ? 3 : 10, actor.States.RemainingTics);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ZeroTicSpawnRetainsFrameAndDefersOnlyNoDelayAction(bool noDelay)
    {
        var actor = new Actor(); var calls = 0; var nextCalls = 0;
        actor.States.ConfigureSpawn(actor, [new(0, 1, _ => calls++, FullBright: true, NoDelay: noDelay),
            new(-1, 1, _ => nextCalls++)], 0);
        Assert.Equal(0, calls); Assert.Equal(0, nextCalls);
        Assert.Equal(0, actor.States.Current); Assert.Equal(0, actor.States.RemainingTics);
        Assert.True(actor.FullBright); Assert.True(actor.HandleNoDelay);
        actor.Tick();
        Assert.Equal(noDelay ? 1 : 0, calls); Assert.Equal(1, nextCalls);
        Assert.Equal(1, actor.States.Current); Assert.False(actor.HandleNoDelay);
    }

    [Fact]
    public void RandomSpawnConsumesOneUnscaledDrawAndSaveDoesNotReplayAction()
    {
        var sim = Room(); var control = Room(); var actor = sim.Actors[0];
        actor.Brain = null; actor.AlwaysFast = true; var calls = 0;
        actor.States.ConfigureSpawn(actor, [new(5, 0, _ => calls++, Fast: true, NoDelay: true, TicRange: 9)], 0);
        Assert.Equal(5 + (int)(control.NextStateRandom() % 10), actor.States.RemainingTics);
        Assert.Equal(control.NextStateRandom(), sim.NextStateRandom());
        var saved = SimSavegame.Write(sim); var tics = actor.States.RemainingTics;
        actor.CheckNoDelay(); Assert.Equal(1, calls);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, calls); Assert.Equal(tics, actor.States.RemainingTics);
        Assert.True(actor.HandleNoDelay); Assert.Equal(saved, SimSavegame.Write(sim));
        actor.CheckNoDelay(); Assert.Equal(2, calls);
    }

    [Fact]
    public void HoldingSpawnWithoutNoDelayNeverRunsItsInitialAction()
    {
        var actor = new Actor(); var calls = 0;
        actor.FullBright = true;
        actor.States.ConfigureSpawn(actor, [new(-1, 0, _ => calls++)], 0);
        actor.Tick(); actor.Tick();
        Assert.Equal(0, calls); Assert.Equal(-1, actor.States.RemainingTics);
        Assert.False(actor.FullBright); Assert.False(actor.HandleNoDelay);
    }

    private static AuthoritySimulation Room(bool slow = false) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] },
        spawnOptions: new SpawnOptions(SlowMonsters: slow), rngSeed: 42);
}
