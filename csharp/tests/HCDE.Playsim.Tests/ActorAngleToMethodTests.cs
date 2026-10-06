namespace HCDE.Playsim.Tests;

public class ActorAngleToMethodTests
{
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 90)]
    [InlineData(-1, 0, 180)]
    [InlineData(0, -1, -90)]
    [InlineData(-1, -1, -135)]
    [InlineData(0, 0, 0)]
    public void AngleToUsesWorldDisplacementAndIgnoresHeightAndFacing(double dx, double dy, double expected)
    {
        var source = new Actor { Angle = BamAngle.FromDegrees(37) }; source.SetXYZ(10, 20, 30);
        var target = new Actor(); target.SetXYZ(10 + dx, 20 + dy, 100);
        Assert.Equal(expected, source.AngleTo(target));
        Assert.Equal(expected, source.AngleTo(target, 0, 0));
        Assert.Equal((10.0, 20.0, 30.0), source.PosPlusZ(0));
        Assert.Equal(BamAngle.FromDegrees(37), source.Angle);
    }

    [Fact]
    public void OffsetOverloadAddsWorldOffsetsToDestinationDisplacement()
    {
        var source = new Actor(); source.SetXYZ(10, 20, 30);
        var target = new Actor(); target.SetXYZ(13, 24, 100);
        Assert.Equal(90, source.AngleTo(target, -3, 2));
        Assert.Equal(0, source.AngleTo(target, 2, -4));
        Assert.Equal(45, source.AngleTo(target, 1, 0));
    }

    [Fact]
    public void MissingDestinationIsRejectedByBothOverloads()
    {
        var actor = new Actor();
        Assert.Throws<ArgumentNullException>(() => actor.AngleTo(null!));
        Assert.Throws<ArgumentNullException>(() => actor.AngleTo(null!, 1, 2));
    }
}
