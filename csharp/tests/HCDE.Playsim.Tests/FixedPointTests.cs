namespace HCDE.Playsim.Tests;

public class FixedPointTests
{
    [Fact]
    public void FromDouble_UsesHalfEvenAndSixteenFractionBits()
    {
        Assert.Equal(65536, Fixed.FromInt(1).Raw);
        Assert.Equal(32768, Fixed.FromDouble(0.5).Raw);
        Assert.Equal(0, Fixed.RoundHalfEven(0.5));
        Assert.Equal(2, Fixed.RoundHalfEven(2.5));
        Assert.Equal(1.0, Fixed.FromInt(1).ToDouble());
        Assert.Equal(2, Fixed.FromDouble(1.6).ToInt());
    }

    [Fact]
    public void MulAndDiv_ShiftLikeMFixed()
    {
        var two = Fixed.FromInt(2);
        var half = Fixed.FromDouble(0.5);
        Assert.Equal(Fixed.FromInt(1).Raw, Fixed.Mul(two, half).Raw);
        Assert.Equal(Fixed.FromInt(4).Raw, Fixed.Div(two, half).Raw);
    }

    [Fact]
    public void BamAngle_MapsNinetyDegreesToTheHighBit()
    {
        Assert.Equal(BamAngle.Angle90, BamAngle.FromDegrees(90).Raw);
        Assert.Equal(BamAngle.Angle180, BamAngle.FromDegrees(180).Raw);
        Assert.Equal(90, BamAngle.FromDegrees(90).ToDegrees(), precision: 6);
    }

    [Fact]
    public void GameTicClock_AdvancesAtThirtyFiveHertz()
    {
        var clock = new GameTicClock();
        clock.Advance();
        clock.Advance();
        Assert.Equal(2, clock.Tic);
        Assert.Equal(2 / 35.0, clock.Seconds, precision: 8);
        Assert.Equal(35, GameTicClock.TicRate);
    }
}
