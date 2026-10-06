namespace HCDE.Playsim.Tests;

public class ActorDistanceMethodTests
{
    [Theory]
    [InlineData(3, 4, 12, 5, 13)]
    [InlineData(-3, -4, -12, 5, 13)]
    [InlineData(0, 0, 7, 0, 7)]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0.375, 0.5, 1.5, 0.625, 1.625)]
    public void DistanceQueriesRespectDimensionsAndAreSymmetric(double dx, double dy, double dz, double horizontal, double total)
    {
        var source = new Actor(); source.SetXYZ(10, 20, 30);
        var target = new Actor(); target.SetXYZ(10 + dx, 20 + dy, 30 + dz);
        Assert.Equal(horizontal, source.Distance2D(target));
        Assert.Equal(horizontal * horizontal, source.Distance2DSquared(target));
        Assert.Equal(horizontal, source.Distance2D(target.X.ToDouble(), target.Y.ToDouble()));
        Assert.Equal(total, source.Distance3D(target));
        Assert.Equal(total * total, source.Distance3DSquared(target));
        Assert.Equal(source.Distance2D(target), target.Distance2D(source));
        Assert.Equal(source.Distance3D(target), target.Distance3D(source));
        Assert.Equal((10.0, 20.0, 30.0), source.PosPlusZ(0));
    }

    [Fact]
    public void OffsetDistanceUsesSourceMinusDestinationPlusOffsets()
    {
        var source = new Actor(); source.SetXYZ(10, 20, 30);
        var target = new Actor(); target.SetXYZ(13, 24, 100);
        Assert.Equal(0, source.Distance2D(target, 3, 4));
        Assert.Equal(10, source.Distance2D(target, -3, -4));
        Assert.Equal(4, source.Distance2D(target, 3, 0));
    }

    [Fact]
    public void ActorDistanceOverloadsRejectMissingDestinations()
    {
        var actor = new Actor();
        Assert.Throws<ArgumentNullException>(() => actor.Distance2D(null!));
        Assert.Throws<ArgumentNullException>(() => actor.Distance2DSquared(null!));
        Assert.Throws<ArgumentNullException>(() => actor.Distance2D(null!, 1, 2));
        Assert.Throws<ArgumentNullException>(() => actor.Distance3D(null!));
        Assert.Throws<ArgumentNullException>(() => actor.Distance3DSquared(null!));
    }
}
