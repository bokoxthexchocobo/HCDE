namespace HCDE.Playsim.Tests;

public class ActorDisplacementMethodTests
{
    [Theory]
    [InlineData(3, 4, 12)]
    [InlineData(-3, -4, -12)]
    [InlineData(0, 0, 0)]
    [InlineData(0, 0, 10)]
    [InlineData(0.125, -0.25, 0.5)]
    public void DisplacementUsesDestinationMinusSourceWithoutMutation(double x, double y, double z)
    {
        var source = new Actor(); source.SetXYZ(10, 20, 30); source.RememberPosition();
        var target = new Actor(); target.SetXYZ(10 + x, 20 + y, 30 + z);
        Assert.Equal((x, y), source.Vec2To(target)); Assert.Equal((x, y, z), source.Vec3To(target));
        Assert.Equal((-x, -y, -z), target.Vec3To(source));
        Assert.Equal((0.0, 0.0, 0.0), source.Vec3To(source));
        Assert.Equal((10.0, 20.0, 30.0), source.PosPlusZ(0));
        Assert.Equal(30, source.PreviousZ.ToDouble());
        Assert.Equal(Math.Max(1, Math.Sqrt(x * x + y * y) / 2), source.DistanceBySpeed(target, 2));
    }

    [Fact]
    public void MissingDestinationIsRejectedByBothMethods()
    {
        var actor = new Actor();
        Assert.Throws<ArgumentNullException>(() => actor.Vec2To(null!));
        Assert.Throws<ArgumentNullException>(() => actor.Vec3To(null!));
    }
}
