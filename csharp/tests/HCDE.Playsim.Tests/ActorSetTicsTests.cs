namespace HCDE.Playsim.Tests;

public class ActorSetTicsTests
{
    [Fact]
    public void ActionOverridesFrameDurationAndTransitionsAfterNewDelay()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(10, 2, Action: self => self.States.SetTics(2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.RemainingTics);
        actor.States.Tick(actor); Assert.Equal(1, actor.States.Current);
        actor.States.Tick(actor); Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void ZeroChainsImmediatelyAndMinusOneHolds()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(10, 2, Action: self => self.States.SetTics(0)),
            new(10, 0, Action: self => self.States.SetTics(-1))], 0);
        actor.States.Enter(actor, 1); Assert.Equal(2, actor.States.Current);
        actor.States.Tick(actor); Assert.Equal(2, actor.States.Current); Assert.Equal(-1, actor.States.RemainingTics);
    }

    [Fact]
    public void NoFunctionEntryDoesNotApplyTimerAction()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(10, 0, Action: self => self.States.SetTics(2))], 0);
        actor.States.Enter(actor, 1, noFunction: true); Assert.Equal(10, actor.States.RemainingTics);
    }

    [Fact]
    public void ReturnedJumpSupersedesTimerChangedOnPreviousFrame()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(10, 0, StateAction: self =>
        { self.States.SetTics(0); return 2; }), new(7, 0)], 0);
        actor.States.Enter(actor, 1); Assert.Equal(2, actor.States.Current); Assert.Equal(7, actor.States.RemainingTics);
        actor.States.SetTics(-2);
        Assert.Equal(-2, actor.States.RemainingTics);
    }
}
