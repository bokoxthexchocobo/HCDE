namespace HCDE.Playsim.Tests;

public class ActorNormalizeAngleTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(180, -180)]
    [InlineData(-180, -180)]
    [InlineData(360, 0)]
    [InlineData(-360, 0)]
    [InlineData(540, -180)]
    [InlineData(-540, -180)]
    [InlineData(450, 90)]
    [InlineData(-450, -90)]
    [InlineData(179, 179)]
    public void NormalizationUsesSignedBinaryAngleWrap(double input, double expected)
    {
        var result = Actor.Normalize180(input);
        Assert.InRange(result, -180, 180);
        Assert.InRange(Math.Abs(result - expected), 0, 360.0 / 4294967296);
        Assert.Equal(result, Actor.Normalize180(result));
    }

    [Fact]
    public void FractionalAnglesRoundToBinaryAngleResolution()
    {
        var unit = 360.0 / 4294967296;
        Assert.Equal(0, Actor.Normalize180(unit * 0.25));
        Assert.Equal(unit, Actor.Normalize180(unit * 0.75));
        Assert.Equal(2 * unit, Actor.Normalize180(unit * 1.5));
        Assert.Equal(2 * unit, Actor.Normalize180(unit * 2.5));
        Assert.Equal(-unit, Actor.Normalize180(-unit * 0.75));
    }
}
