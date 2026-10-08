namespace HCDE.Playsim.Tests;

public class ActorStateBrightnessTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void EntrySetsFrameBrightnessBeforeAction(bool? brightness, bool expected)
    {
        var actor = new Actor();
        bool? seen = null;
        actor.States.Configure(actor, [new(-1, 0),
            new(-1, 1, self => seen = self.FullBright, FullBright: brightness)], 0);
        actor.FullBright = true;
        actor.States.Enter(actor, 1);
        Assert.Equal(expected, actor.FullBright);
        Assert.Equal(expected, seen);
    }

    [Fact]
    public void SuppressedActionsStillApplyFrameBrightness()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0),
            new(-1, 1, _ => throw new InvalidOperationException("Action must be suppressed."))], 0);
        actor.FullBright = true;
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.False(actor.FullBright);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ImmediateChainsUseDestinationFrameBrightness(bool returnedState)
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0),
            new(returnedState ? -1 : 0, 2, FullBright: true,
                StateAction: returnedState ? _ => 2 : null), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void ActionBrightnessPersistsUntilNextFrameEntry()
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0),
            new(2, 2, self => self.FullBright = true), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        actor.States.Tick(actor);
        Assert.True(actor.FullBright);
        actor.States.Tick(actor);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void RestoreDoesNotPerformFrameEntryOrClearRuntimeBrightness()
    {
        var actor = new Actor { FullBright = true };
        actor.States.Restore(actor, 0, -1);
        Assert.True(actor.FullBright);
        actor.States.Enter(actor, 0);
        Assert.False(actor.FullBright);
    }
}
