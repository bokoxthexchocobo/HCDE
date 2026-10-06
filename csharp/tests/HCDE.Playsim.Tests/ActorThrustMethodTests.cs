namespace HCDE.Playsim.Tests;

public class ActorThrustMethodTests
{
    [Theory]
    [InlineData(0, 4, 5, 2)]
    [InlineData(90, 4, 1, 6)]
    [InlineData(180, -4, 5, 2)]
    [InlineData(270, 4, 1, -2)]
    [InlineData(0, 0, 1, 2)]
    public void HorizontalOverloadsAddImpulseAndPreserveVerticalVelocity(double yaw, double speed, double x, double y)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(yaw), MovementSpeed = Fixed.FromDouble(speed) };
        Reset(actor); actor.Thrust(); AssertVelocity(actor, x, y, 3);
        Reset(actor); actor.Thrust(speed); AssertVelocity(actor, x, y, 3);
        Reset(actor); actor.Angle = BamAngle.FromDegrees(45);
        actor.Thrust(BamAngle.FromDegrees(yaw), speed); AssertVelocity(actor, x, y, 3);
        Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
    }

    [Fact]
    public void VectorImpulseAddsAllAxesWithoutChangingOrientation()
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(90) }; Reset(actor);
        actor.Thrust((-2.5, 0.25, -4)); AssertVelocity(actor, -1.5, 2.25, -1);
        Assert.Equal(BamAngle.FromDegrees(90), actor.Angle);
    }

    [Fact]
    public void FrameActionCanApplyRepeatedThrustWithoutSpecialVelocityLimits()
    {
        var actor = new Actor { MovementSpeed = Fixed.FromInt(20) };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 0, Action: self =>
        { self.Thrust(); self.Thrust(); })], 0);
        actor.States.Enter(actor, 1);
        AssertVelocity(actor, 40, 0, 0);
    }

    private static void Reset(Actor actor)
    { actor.VelocityX = Fixed.FromInt(1); actor.VelocityY = Fixed.FromInt(2); actor.VelocityZ = Fixed.FromInt(3); }

    private static void AssertVelocity(Actor actor, double x, double y, double z)
    {
        Assert.InRange(Math.Abs(actor.VelocityX.ToDouble() - x), 0, 1.0 / 65536);
        Assert.InRange(Math.Abs(actor.VelocityY.ToDouble() - y), 0, 1.0 / 65536);
        Assert.InRange(Math.Abs(actor.VelocityZ.ToDouble() - z), 0, 1.0 / 65536);
    }
}
