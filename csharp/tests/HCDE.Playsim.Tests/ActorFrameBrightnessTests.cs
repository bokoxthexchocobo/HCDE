namespace HCDE.Playsim.Tests;

public class ActorFrameBrightnessTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EntryAppliesBrightnessBeforeOptionalAction(bool suppress)
    {
        var actor = new Actor(); var observed = false;
        actor.States.Configure(actor,
            [new(-1, 0, FullBright: false), new(1, 0, self => observed = self.FullBright, FullBright: true)], 0);
        actor.States.Enter(actor, 1, noFunction: suppress);
        Assert.True(actor.FullBright);
        Assert.Equal(!suppress, observed);
        actor.States.Tick(actor);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void ImmediateChainUsesFinalFrameBrightness()
    {
        var actor = new Actor();
        actor.States.Configure(actor,
            [new(-1, 0), new(0, 2, FullBright: true), new(-1, 2, FullBright: false)], 0);
        actor.States.Enter(actor, 1, noFunction: true);
        Assert.Equal(2, actor.States.Current);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void DefaultFramesClearGameplayBrightnessOnEntry()
    {
        var actor = new Actor { FullBright = true };
        actor.States.Enter(actor, ActorStateMachine.Spawn);
        Assert.False(actor.FullBright);
    }

    [Fact]
    public void PuffBrightnessSurvivesSuppressedEntryAndClearsAtNextFrame()
    {
        var puff = new PuffActor();
        puff.States.Enter(puff, 1);
        Assert.False(puff.FullBright);
        puff.States.Enter(puff, 0, noFunction: true);
        Assert.True(puff.FullBright);
        for (var i = 0; i < 4; i++) puff.States.Tick(puff);
        Assert.False(puff.FullBright);
    }
}
