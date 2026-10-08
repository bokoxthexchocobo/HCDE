namespace HCDE.Playsim.Tests;

public class ActorFrameTimingTests
{
    private sealed class TimedActor(bool fast, bool slow) : Actor
    {
        public override bool IsFast() => fast;
        public override bool IsSlow() => slow;
    }

    [Theory]
    [InlineData(5, true, false, true, false, 3)]
    [InlineData(6, true, false, true, false, 3)]
    [InlineData(1, true, false, true, false, 1)]
    [InlineData(5, false, true, false, true, 10)]
    [InlineData(5, true, true, true, true, 3)]
    [InlineData(5, true, true, false, true, 10)]
    [InlineData(5, false, false, true, true, 5)]
    [InlineData(-1, false, false, true, true, -1)]
    public void EntryUsesNativeDurationAdjustment(int tics, bool fast, bool slow,
        bool fastFrame, bool slowFrame, int expected)
    {
        var actor = new TimedActor(fast, slow); var observed = 0;
        actor.States.Configure(actor, [new(-1, 0),
            new(tics, 0, self => observed = self.States.RemainingTics, Fast: fastFrame, Slow: slowFrame)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(expected, actor.States.RemainingTics); Assert.Equal(expected, observed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SuppressedEntryAndRestoreDoNotRepeatScaling(bool suppress)
    {
        var actor = new TimedActor(true, false); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, _ => calls++, Fast: true)], 0);
        actor.States.Enter(actor, 1, noFunction: suppress);
        Assert.Equal(3, actor.States.RemainingTics); Assert.Equal(suppress ? 0 : 1, calls);
        actor.States.Tick(actor); Assert.Equal(2, actor.States.RemainingTics);
        actor.States.Restore(actor, 1, 2); Assert.Equal(2, actor.States.RemainingTics);
        Assert.Equal(suppress ? 0 : 1, calls);
    }

    [Fact]
    public void FastHoldingFrameUsesNativeZeroTicChain()
    {
        var actor = new TimedActor(true, false);
        actor.States.Configure(actor, [new(-1, 0), new(-1, 2, Fast: true), new(-1, 2)], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(2, actor.States.Current); Assert.Equal(-1, actor.States.RemainingTics);
    }
}
