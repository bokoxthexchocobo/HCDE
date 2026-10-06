using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGravityActionTests
{
    [Theory]
    [InlineData(-4, 0)]
    [InlineData(0, 0)]
    [InlineData(0.5, 0.5)]
    [InlineData(14, 10)]
    public void SetGravityClampsWithoutEnablingGravity(double value, double expected)
    {
        var actor = new Actor { NoGravity = true };
        ActorPropertyActions.SetGravity(actor, value);
        Assert.Equal(expected, actor.Gravity.ToDouble());
        Assert.True(actor.NoGravity);
    }

    [Fact]
    public void GravityActionsResetMultiplierAndNoGravityPreservesIt()
    {
        var actor = new Actor { NoGravity = true, Gravity = Fixed.FromInt(7) };
        ActorPropertyActions.LowGravity(actor);
        Assert.False(actor.NoGravity); Assert.Equal(0.125, actor.Gravity.ToDouble());
        ActorPropertyActions.NoGravity(actor);
        Assert.True(actor.NoGravity); Assert.Equal(0.125, actor.Gravity.ToDouble());
        ActorPropertyActions.Gravity(actor);
        Assert.False(actor.NoGravity); Assert.Equal(1, actor.Gravity.ToDouble());
    }

    [Theory]
    [InlineData(false, -1)]
    [InlineData(true, -0.125)]
    public void FrameActionChangesActualFallingAcceleration(bool low, double expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0);
        actor.Z = Fixed.FromInt(64); actor.NoGravity = true;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
        { if (low) ActorPropertyActions.LowGravity(self); else ActorPropertyActions.Gravity(self); })], 0);
        actor.States.Enter(actor, 1);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(expected, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteInputFailsBeforeMutation(double value)
    {
        var actor = new Actor { Gravity = Fixed.FromInt(2) };
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.SetGravity(actor, value));
        Assert.Equal(2, actor.Gravity.ToDouble());
    }
}
