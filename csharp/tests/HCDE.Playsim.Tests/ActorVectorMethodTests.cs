namespace HCDE.Playsim.Tests;

public class ActorVectorMethodTests
{
    [Theory]
    [InlineData(0, 4, 4, 0)]
    [InlineData(90, 4, 0, 4)]
    [InlineData(-90, 4, 0, -4)]
    [InlineData(450, -2, 0, -2)]
    [InlineData(37, 0, 0, 0)]
    public void AngleVectorUsesDegreesAndSignedLength(double angle, double length, double x, double y)
    {
        var vector = Actor.AngleToVector(angle, length);
        AssertClose(x, vector.X); AssertClose(y, vector.Y);
    }

    [Theory]
    [InlineData(3, 4, 90, -4, 3)]
    [InlineData(3, 4, -90, 4, -3)]
    [InlineData(-3, 4, 180, 3, -4)]
    [InlineData(0, 0, 123, 0, 0)]
    public void RotationUsesNativeOrientationAndPreservesLength(double x, double y, double angle, double expectedX, double expectedY)
    {
        var rotated = Actor.RotateVector(x, y, angle);
        AssertClose(expectedX, rotated.X); AssertClose(expectedY, rotated.Y);
        AssertClose(x * x + y * y, rotated.X * rotated.X + rotated.Y * rotated.Y);
    }

    [Fact]
    public void ArbitraryFractionalRotationReversesWithoutFixedPointQuantization()
    {
        var rotated = Actor.RotateVector(0.123456789, -0.987654321, 37.25);
        var restored = Actor.RotateVector(rotated.X, rotated.Y, -37.25);
        AssertClose(0.123456789, restored.X); AssertClose(-0.987654321, restored.Y);
        AssertClose(1, Actor.AngleToVector(0).X);
    }

    private static void AssertClose(double expected, double actual) => Assert.InRange(Math.Abs(expected - actual), 0, 1e-12);
}
