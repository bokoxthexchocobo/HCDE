namespace HCDE.Playsim.Tests;

public class BinaryAngleRoundingTests
{
    [Theory]
    [InlineData(-0.5, 0u)]
    [InlineData(-1.5, 0xfffffffeu)]
    [InlineData(-2.5, 0xfffffffeu)]
    [InlineData(0.5, 0u)]
    [InlineData(1.5, 2u)]
    [InlineData(2.5, 2u)]
    [InlineData(-0.50000001, uint.MaxValue)]
    [InlineData(-0.49999999, 0u)]
    public void SignedValuesRoundBeforeUnsignedWrap(double units, uint expected)
    {
        var degrees = units * (360.0 / 4294967296);
        Assert.Equal(expected, BamAngle.FromDegrees(degrees).Raw);
        Assert.Equal(unchecked((int)expected) * (360.0 / 4294967296), Actor.Normalize180(degrees));
    }

    [Theory]
    [InlineData(-720, 0u)]
    [InlineData(720, 0u)]
    [InlineData(-450, 0xc0000000u)]
    [InlineData(450, 0x40000000u)]
    public void SignedRoundingRetainsMultipleTurnWrap(double degrees, uint expected)
    {
        Assert.Equal(expected, BamAngle.FromDegrees(degrees).Raw);
    }
}
