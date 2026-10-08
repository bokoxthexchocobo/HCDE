using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorNoDelayTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PendingActionRunsOnceWithoutReenteringFrame(bool noDelay)
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(4, 1, _ => calls++, NoDelay: noDelay), new(-1, 1)], 0, noFunction: true);
        actor.FullBright = true;
        Assert.True(actor.CheckNoDelay());
        Assert.Equal(noDelay ? 1 : 0, calls); Assert.False(actor.HandleNoDelay);
        Assert.Equal(4, actor.States.RemainingTics); Assert.True(actor.FullBright);
        Assert.True(actor.CheckNoDelay()); Assert.Equal(noDelay ? 1 : 0, calls);
    }

    [Fact]
    public void DormancyDefersPendingActionUntilActiveTick()
    {
        var actor = new Actor { Dormant = true }; var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++, NoDelay: true)], 0, noFunction: true);
        actor.Tick(); Assert.Equal(0, calls); Assert.True(actor.HandleNoDelay);
        actor.Dormant = false; actor.Tick(); actor.Tick();
        Assert.Equal(1, calls); Assert.False(actor.HandleNoDelay);
    }

    [Fact]
    public void ReturnedStateRunsDestinationAndDestructionStopsTick()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, StateAction: _ => 1, NoDelay: true),
            new(-1, 1, _ => calls++)], 0, noFunction: true);
        Assert.True(actor.CheckNoDelay()); Assert.Equal(1, calls); Assert.Equal(1, actor.States.Current);
        actor.States.Configure(actor, [new(-1, 0, self => self.Destroy(), NoDelay: true)], 0, noFunction: true);
        Assert.False(actor.CheckNoDelay()); Assert.True(actor.Destroyed);
    }

    [Fact]
    public void InvalidReturnFailsAndConsumesPendingFlag()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0, StateAction: _ => 99, NoDelay: true)], 0, noFunction: true);
        Assert.Throws<InvalidOperationException>(() => actor.CheckNoDelay());
        Assert.False(actor.HandleNoDelay); Assert.Equal(0, actor.States.Current);
    }

    [Fact]
    public void SaveRetainsPendingActionAndOlderSaveClearsIt()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = sim.Actors[0]; actor.Brain = null; var calls = 0;
        actor.States.Configure(actor, [new(-1, 0, _ => calls++, NoDelay: true)], 0, noFunction: true);
        var pending = SimSavegame.Write(sim);
        actor.CheckNoDelay(); var completed = SimSavegame.Write(sim);
        Assert.Equal(1, calls);
        SimSavegame.Apply(sim, pending); Assert.True(actor.HandleNoDelay); Assert.Equal(1, calls);
        actor.CheckNoDelay(); Assert.Equal(2, calls);
        actor.HandleNoDelay = true; SimSavegame.Apply(sim, completed);
        Assert.False(actor.HandleNoDelay); actor.CheckNoDelay(); Assert.Equal(2, calls);
        actor.HandleNoDelay = true; ActorRaise.ReviveSupported(actor);
        Assert.False(actor.HandleNoDelay); Assert.Equal(2, calls);
    }
}
