namespace HCDE.Playsim.Tests;

public class ActorReturnedStateTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void ReturnedStateImmediatelyOverridesHoldingOrTimedFrame(int tics)
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(tics, 0, StateAction: _ => 2),
            new(3, 0, _ => calls++, FullBright: true)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        Assert.Equal(3, actor.States.RemainingTics);
        Assert.Equal(1, calls);
        Assert.True(actor.FullBright);
    }

    [Fact]
    public void NullReturnKeepsNormalFrameAndSuppressionSkipsJump()
    {
        var actor = new Actor(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(2, 0, StateAction: _ => { calls++; return null; })], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(1, calls); Assert.Equal(1, actor.States.Current);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.Equal(1, calls); Assert.Equal(2, actor.States.RemainingTics);
    }

    [Fact]
    public void DestroyedActorDoesNotExecuteReturnedTarget()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(2, 0, StateAction: self => { self.Destroy(); return 2; }),
            new(-1, 2, _ => throw new InvalidOperationException("Destroyed actors cannot jump."))], 0);
        actor.States.Enter(actor, 1);
        Assert.True(actor.Destroyed); Assert.Equal(1, actor.States.Current);
    }

    [Fact]
    public void InvalidReturnedTargetFailsExplicitlyAndReleasesEntryGuard()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(2, 0, StateAction: _ => 99)], 0);
        Assert.Throws<InvalidOperationException>(() => actor.States.Enter(actor, 1));
        actor.States.Enter(actor, 0);
        Assert.Equal(0, actor.States.Current);
    }

    [Fact]
    public void FramesRejectTwoActionCallbacks()
    {
        var actor = new Actor();
        Assert.Throws<ArgumentException>(() => actor.States.Configure(actor,
            [new(-1, 0, _ => { }, StateAction: _ => null)], 0));
    }
}
