namespace HCDE.Playsim.Tests;

public class ActorDistanceBySpeedTests
{
    [Theory]
    [InlineData(3, 4, 2, 2.5)]
    [InlineData(3, 4, 10, 1)]
    [InlineData(0, 0, 2, 1)]
    [InlineData(3, 4, -2, 1)]
    [InlineData(0, 0, 0, 1)]
    [InlineData(-3, -4, 0.5, 10)]
    public void NativeTravelTimeUsesHorizontalDistanceAndMinimumOne(double dx, double dy, double speed, double expected)
    {
        var source = new Actor { X = Fixed.FromInt(10), Y = Fixed.FromInt(20), Z = Fixed.FromInt(100) };
        var destination = new Actor { X = Fixed.FromDouble(10 + dx), Y = Fixed.FromDouble(20 + dy) };
        Assert.Equal(expected, source.DistanceBySpeed(destination, speed));
    }

    [Fact]
    public void SeparatedZeroSpeedReturnsPositiveInfinity()
    {
        Assert.Equal(double.PositiveInfinity,
            new Actor().DistanceBySpeed(new Actor { X = Fixed.FromInt(1) }, 0));
    }

    [Fact]
    public void MissingDestinationIsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new Actor().DistanceBySpeed(null!, 1));
    }
}
