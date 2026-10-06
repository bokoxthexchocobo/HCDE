namespace HCDE.Playsim.Tests;

public class ActorPositionOffsetTests
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(-2.5, 1.25, -4)]
    [InlineData(3, -7, 100)]
    public void OffsetZIsAbsoluteWhileThreeDimensionalOffsetAddsZ(double dx, double dy, double z)
    {
        var actor = Setup();
        Assert.Equal((10 + dx, 20 + dy), actor.Vec2Offset(dx, dy));
        Assert.Equal((10 + dx, 20 + dy, z), actor.Vec2OffsetZ(dx, dy, z));
        Assert.Equal((10 + dx, 20 + dy, 30 + z), actor.Vec3Offset(dx, dy, z));
        Assert.Equal(actor.Vec3Offset(dx, dy, z), actor.Vec3Offset((dx, dy, z)));
        AssertUnchanged(actor);
    }

    [Theory]
    [InlineData(0, 4, 14, 20)]
    [InlineData(90, 4, 10, 24)]
    [InlineData(-90, -4, 10, 24)]
    [InlineData(180, 0, 10, 20)]
    public void AngleOffsetsUseWorldDirectionAndReturnPositions(double angle, double length, double x, double y)
    {
        var actor = Setup();
        var horizontal = actor.Vec2Angle(length, angle); var position = actor.Vec3Angle(length, angle, -2.5);
        Assert.InRange(Math.Abs(horizontal.X - x), 0, 1e-12);
        Assert.InRange(Math.Abs(horizontal.Y - y), 0, 1e-12);
        Assert.Equal(horizontal.X, position.X); Assert.Equal(horizontal.Y, position.Y);
        Assert.Equal(27.5, position.Z); AssertUnchanged(actor);
    }

    private static Actor Setup()
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(37), VelocityX = Fixed.FromInt(7) };
        actor.SetXYZ(10, 20, 30); actor.RememberPosition(); return actor;
    }

    private static void AssertUnchanged(Actor actor)
    {
        Assert.Equal((10.0, 20.0, 30.0), actor.PosPlusZ(0));
        Assert.Equal(30, actor.PreviousZ.ToDouble()); Assert.Equal(BamAngle.FromDegrees(37), actor.Angle);
        Assert.Equal(7, actor.VelocityX.ToDouble());
    }
}
