namespace HCDE.Playsim.Tests;

public class ActorVelocityInspectionTests
{
    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(3, 4, 12, 5, 13)]
    [InlineData(-3, -4, -12, 5, 13)]
    [InlineData(0, 0, -7, 0, 7)]
    [InlineData(0.375, 0.5, 1.5, 0.625, 1.625)]
    public void SpeedQueriesUseVelocityRatherThanActorSpeed(double x, double y, double z, double horizontal, double total)
    {
        var actor = new Actor { MovementSpeed = Fixed.FromInt(99),
            VelocityX = Fixed.FromDouble(x), VelocityY = Fixed.FromDouble(y), VelocityZ = Fixed.FromDouble(z) };
        Assert.Equal(horizontal, actor.VelXYToSpeed()); Assert.Equal(total, actor.VelToSpeed());
        Assert.Equal(Fixed.FromDouble(x), actor.VelocityX);
        Assert.Equal(Fixed.FromDouble(y), actor.VelocityY);
        Assert.Equal(Fixed.FromDouble(z), actor.VelocityZ);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 90)]
    [InlineData(-1, 0, 180)]
    [InlineData(0, -1, 270)]
    [InlineData(-1, -1, 225)]
    [InlineData(0, 0, 0)]
    public void AngleFromVelocitySetsYawWithoutChangingVelocityOrPitch(double x, double y, double expected)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(37), PitchDegrees = 20,
            VelocityX = Fixed.FromDouble(x), VelocityY = Fixed.FromDouble(y), VelocityZ = Fixed.FromInt(10) };
        actor.AngleFromVel();
        Assert.Equal(BamAngle.FromDegrees(expected), actor.Angle); Assert.Equal(20, actor.PitchDegrees);
        Assert.Equal(Fixed.FromDouble(x), actor.VelocityX); Assert.Equal(Fixed.FromDouble(y), actor.VelocityY);
        Assert.Equal(Fixed.FromInt(10), actor.VelocityZ);
    }
}
