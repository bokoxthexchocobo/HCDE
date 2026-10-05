namespace HCDE.Playsim.Tests;

public class VirtualGibHealthTests
{
    [Theory]
    [InlineData(-5, 2)]
    [InlineData(-50, 1)]
    public void DeathSelectionUsesVirtualThreshold(int threshold, int state)
    {
        var actor = Create(threshold); actor.Health = -10;
        Assert.Equal(state, actor.States.Current);
        Assert.Equal(1, actor.Calls);
    }

    [Fact]
    public void ForcedExtremeDeathHealthUsesVirtualThreshold()
    {
        var actor = Create(-50); actor.DeathInflictor = new Actor { ExtremeDeath = true };
        actor.Health = -10;
        Assert.Equal(2, actor.States.Current); Assert.Equal(-51, actor.Health);
        Assert.Equal(1, actor.Calls);
    }

    [Fact]
    public void BaseMethodRetainsConfiguredThreshold()
    {
        Assert.Equal(-37, new Actor { GibHealth = -37 }.GetGibHealth());
    }

    private static CustomActor Create(int threshold)
    {
        var actor = new CustomActor(threshold) { Health = 20, DeathState = 1, ExtremeDeathState = 2 };
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1), new ActorFrame(-1, 2)], 0);
        return actor;
    }

    private sealed class CustomActor(int threshold) : Actor
    {
        public int Calls { get; private set; }
        public override int GetGibHealth() { Calls++; return threshold; }
    }
}
