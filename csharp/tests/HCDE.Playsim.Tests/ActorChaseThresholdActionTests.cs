using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorChaseThresholdActionTests
{
    [Theory]
    [InlineData(-10, 0)]
    [InlineData(0, 0)]
    [InlineData(27, 27)]
    [InlineData(int.MaxValue, int.MaxValue)]
    public void CurrentThresholdClampsWithoutChangingDefault(int value, int expected)
    {
        var actor = Room().AddBot(0, 0);
        ActorPropertyActions.SetChaseThreshold(actor, value);
        Assert.Equal(expected, actor.Brain!.Threshold); Assert.Equal(100, actor.Brain.DefThreshold);
    }

    [Fact]
    public void DefaultDoesNotChangeCurrentUntilDamageWakeup()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        actor.Brain!.WakeOnDamage(actor, sim.Players.Single(), 1, false);
        ActorPropertyActions.SetChaseThreshold(actor, 7);
        ActorPropertyActions.SetChaseThreshold(actor, 25, true);
        Assert.Equal(7, actor.Brain!.Threshold);
        actor.Brain.WakeOnDamage(actor, sim.Players.Single(), 1, false);
        Assert.Equal(25, actor.Brain.Threshold);
        ActorPropertyActions.SetChaseThreshold(actor, -1, true);
        Assert.Equal(0, actor.Brain.DefThreshold); Assert.Equal(25, actor.Brain.Threshold);
    }

    [Fact]
    public void FrameActionCanChangeSelectedTargetsThreshold()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        actor.Brain!.SetTargetThingId(sim, 7);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
            ActorPropertyActions.SetChaseThreshold(self, 42, false, AcsActorPointer.Target))], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(42, target.Brain!.Threshold); Assert.Equal(0, actor.Brain.Threshold);
        ActorPropertyActions.SetChaseThreshold(actor, 8, false, AcsActorPointer.Null);
        Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void UnsupportedBrainStorageFailsExplicitly()
    {
        Assert.Throws<NotSupportedException>(() => ActorPropertyActions.SetChaseThreshold(new Actor(), 1));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }] });
}
