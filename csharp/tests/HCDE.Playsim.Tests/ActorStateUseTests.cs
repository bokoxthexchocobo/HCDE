using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorStateUseTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ForbiddenEntryDestroysBeforeTimingBrightnessOrAction(bool noFunction)
    {
        var sim = Room(); var actor = sim.Actors[0]; var calls = 0;
        actor.States.Configure(actor, [new(-1, 0),
            new(5, 0, _ => calls++, FullBright: false, TicRange: 9, ActorUsable: false)], 0);
        actor.FullBright = true;
        actor.States.Enter(actor, 1, noFunction);
        Assert.True(actor.Destroyed); Assert.Equal(-1, actor.States.Current);
        Assert.Equal(-1, actor.States.RemainingTics); Assert.Equal(0, calls);
        Assert.True(actor.FullBright);
        Assert.Equal(Room().NextStateRandom(), sim.NextStateRandom());
    }

    [Fact]
    public void ImmediateSuccessorChecksUseBeforeExecutingItsAction()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(0, 2, _ => calls++),
            new(4, 0, _ => calls++, ActorUsable: false)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(1, calls); Assert.True(actor.Destroyed); Assert.Equal(-1, actor.States.Current);
    }

    [Fact]
    public void ReturnedStateChecksActorUse()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(4, 0, StateAction: _ => 2),
            new(-1, 2, _ => calls++, ActorUsable: false)], 0);
        actor.States.Enter(actor, 1);
        Assert.True(actor.Destroyed); Assert.Equal(0, calls);
    }

    [Fact]
    public void NoDelayReturnedStateReportsDestruction()
    {
        var actor = new Actor();
        actor.States.ConfigureSpawn(actor, [new(-1, 0, StateAction: _ => 1, NoDelay: true),
            new(-1, 1, ActorUsable: false)], 0);
        Assert.False(actor.CheckNoDelay()); Assert.True(actor.Destroyed);
        Assert.False(actor.HandleNoDelay);
    }

    [Fact]
    public void SpawnInitializationBypassesSetStateUseCheckUntilLaterEntry()
    {
        var actor = new Actor();
        actor.States.ConfigureSpawn(actor, [new(3, 0, ActorUsable: false)], 0);
        Assert.False(actor.Destroyed); Assert.Equal(3, actor.States.RemainingTics);
        actor.States.Tick(actor); actor.States.Tick(actor); actor.States.Tick(actor);
        Assert.True(actor.Destroyed); Assert.Equal(-1, actor.States.Current);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
