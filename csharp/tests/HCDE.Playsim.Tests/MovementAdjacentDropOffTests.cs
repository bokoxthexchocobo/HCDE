namespace HCDE.Playsim.Tests;

public class MovementAdjacentDropOffTests
{
    [Theory]
    [InlineData(24, false, false, true)]
    [InlineData(25, false, false, false)]
    [InlineData(25, true, false, true)]
    [InlineData(25, false, true, true)]
    public void DestinationBoundsDetectLedgeBeforeCenterCrosses(short drop, bool floating, bool blasted, bool passes)
    {
        var sim = GameplayFoundationTests.TwoRooms((short)-drop, 128);
        var actor = sim.AddBot(-40, 80);
        actor.AllowDropOff = false;
        actor.Floating = floating;
        actor.Blasted = blasted;
        Assert.Equal(passes, ActorPhysics.TryMove(sim, actor, -1, 80, out _));
        Assert.Equal(passes ? -1 : -40, actor.X.ToDouble());
        Assert.Equal(0, actor.SectorIndex);
        Assert.Equal(0, actor.Z.ToDouble());
    }

    [Fact]
    public void BoundsClearOfLedgeDoNotBlock()
    {
        var sim = GameplayFoundationTests.TwoRooms(-48, 128);
        var actor = sim.AddBot(-60, 80);
        actor.AllowDropOff = false;
        Assert.True(ActorPhysics.TryMove(sim, actor, -40, 80, out _));
    }
}
