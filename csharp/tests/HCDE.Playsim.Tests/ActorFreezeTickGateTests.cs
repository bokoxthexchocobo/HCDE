using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFreezeTickGateTests
{
    [Fact]
    public void FreezeTics_DecrementUntilZero()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
        });
        var actor = sim.Actors.First();
        actor.FreezeTics = 2;
        Assert.True(ActorFreezeTickGate.TryConsumeFreezeTick(actor));
        Assert.Equal(1, actor.FreezeTics);
        Assert.True(ActorFreezeTickGate.TryConsumeFreezeTick(actor));
        Assert.Equal(0, actor.FreezeTics);
        Assert.False(ActorFreezeTickGate.TryConsumeFreezeTick(actor));
    }

    [Fact]
    public void ActorTick_ReturnsEarlyWhileFrozen()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            MapName = "MAP01",
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
        });
        var actor = sim.Actors.First();
        actor.FreezeTics = 1;
        actor.PickupDelay = 5;
        actor.Tick();
        Assert.Equal(5, actor.PickupDelay);
        Assert.Equal(0, actor.FreezeTics);
    }
}
