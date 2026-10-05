namespace HCDE.Playsim.Tests;

public class ActorStateActionSuppressionTests
{
    [Fact]
    public void SuppressionSkipsActionsThroughoutImmediateChain()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor,
            [new(-1, 0), new(0, 2, _ => calls++), new(3, 0, _ => calls++)], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.Equal(0, calls);
        Assert.Equal(2, actor.States.Current);
        Assert.Equal(3, actor.States.RemainingTics);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void SuppressionDoesNotPersistIntoTimedSuccessor()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor,
            [new(-1, 0), new(1, 2, _ => calls++), new(-1, 2, _ => calls++)], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        actor.States.Tick(actor);
        Assert.Equal(1, calls);
        Assert.Equal(2, actor.States.Current);
    }

    [Fact]
    public void SuppressedImmediateChainStillRemovesActor()
    {
        var actor = new Actor();
        actor.States.Configure(actor,
            [new(-1, 0), new(0, -1, _ => throw new InvalidOperationException("Action should be suppressed."))], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.True(actor.Destroyed);
        Assert.Equal(-1, actor.States.Current);
    }

    [Fact]
    public void SuppressionPreventsActionRedirect()
    {
        var actor = new Actor();
        actor.States.Configure(actor,
            [new(-1, 0), new(2, 0, self => self.States.Enter(self, 2)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.Equal(1, actor.States.Current);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
    }
}
