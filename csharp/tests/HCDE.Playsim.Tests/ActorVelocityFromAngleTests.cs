namespace HCDE.Playsim.Tests;

public class ActorVelocityFromAngleTests
{
    [Theory]
    [InlineData(0, 4, 4, 0)]
    [InlineData(90, 4, 0, 4)]
    [InlineData(180, -4, 4, 0)]
    [InlineData(270, 4, 0, -4)]
    [InlineData(0, 0, 0, 0)]
    public void HorizontalOverloadsReplaceXYAndPreserveZ(double yaw, double speed, double x, double y)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(yaw), MovementSpeed = Fixed.FromDouble(speed),
            VelocityX = Fixed.FromInt(99), VelocityY = Fixed.FromInt(99), VelocityZ = Fixed.FromInt(7) };
        actor.VelFromAngle(); AssertVelocity(actor, x, y, 7);
        actor.VelFromAngle(speed); AssertVelocity(actor, x, y, 7);
        actor.Angle = BamAngle.FromDegrees(45);
        actor.VelFromAngle(speed, BamAngle.FromDegrees(yaw)); AssertVelocity(actor, x, y, 7);
        Assert.Equal(BamAngle.FromDegrees(45), actor.Angle);
    }

    [Theory]
    [InlineData(0, 4, 4, 0)]
    [InlineData(90, 4, 0, -4)]
    [InlineData(-90, 4, 0, 4)]
    [InlineData(30, -4, -3.4641016151377544, 2)]
    public void ThreeDimensionalOverloadsUseNativeDownwardPositivePitch(double pitch, double speed, double horizontal, double vertical)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(90) };
        actor.Vel3DFromAngle(pitch, speed); AssertVelocity(actor, 0, horizontal, vertical);
        actor.Vel3DFromAngle(BamAngle.FromDegrees(0), pitch, speed);
        AssertVelocity(actor, horizontal, 0, vertical);
        Assert.Equal(BamAngle.FromDegrees(90), actor.Angle);
    }

    private static void AssertVelocity(Actor actor, double x, double y, double z)
    {
        Assert.InRange(Math.Abs(actor.VelocityX.ToDouble() - x), 0, 1.0 / 65536);
        Assert.InRange(Math.Abs(actor.VelocityY.ToDouble() - y), 0, 1.0 / 65536);
        Assert.InRange(Math.Abs(actor.VelocityZ.ToDouble() - z), 0, 1.0 / 65536);
    }
}
