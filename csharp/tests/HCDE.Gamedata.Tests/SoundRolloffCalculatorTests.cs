namespace HCDE.Gamedata.Tests;

public class SoundRolloffCalculatorTests
{
    [Theory]
    [InlineData(5f, 1f)]
    [InlineData(10f, 1f)]
    [InlineData(15f, 0.5f)]
    [InlineData(20f, 0f)]
    [InlineData(30f, 0f)]
    public void LinearCurveHonorsDistanceBoundaries(float distance, float expected)
        => Assert.Equal(expected, SoundRolloffCalculator.Calculate(new(SoundRolloffType.Linear, 10, 20), distance));

    [Fact]
    public void LogCurveUsesFactorAndHasNoMaximumCutoff()
    {
        var rolloff = new SoundRolloff(SoundRolloffType.Log, 10, 2);
        Assert.Equal(1f / 3, SoundRolloffCalculator.Calculate(rolloff, 20));
        Assert.Equal(1f / 19, SoundRolloffCalculator.Calculate(rolloff, 100));
    }

    [Fact]
    public void DoomAndEmptyCustomCurveUseExponentialFalloff()
    {
        var expected = (MathF.Sqrt(10) - 1) / 9;
        Assert.Equal(expected, SoundRolloffCalculator.Calculate(new(SoundRolloffType.Doom, 10, 20), 15), 6);
        Assert.Equal(expected, SoundRolloffCalculator.Calculate(new(SoundRolloffType.Custom, 10, 20), 15), 6);
    }

    [Fact]
    public void CustomCurveUsesByteIndexAndNative127Divisor()
    {
        byte[] curve = [127, 64, 32, 255];
        var rolloff = new SoundRolloff(SoundRolloffType.Custom, 10, 20);
        Assert.Equal(32f / 127, SoundRolloffCalculator.Calculate(rolloff, 15, curve));
        Assert.Equal(255f / 127, SoundRolloffCalculator.Calculate(rolloff, 19, curve));
        Assert.Equal(0, SoundRolloffCalculator.Calculate(rolloff, 20, curve));
    }

    [Fact]
    public void MissingAndReversedRangesPreserveNativeBranchOrder()
    {
        Assert.Equal(0, SoundRolloffCalculator.Calculate(null, 10));
        Assert.Equal(1, SoundRolloffCalculator.Calculate(new(MinDistance: 20, MaxDistance: 10), 15));
        Assert.Equal(0, SoundRolloffCalculator.Calculate(new(MinDistance: 20, MaxDistance: 10), 21));
    }

    [Fact]
    public void InvalidInputsAndUndefinedLogGainFailExplicitly()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SoundRolloffCalculator.Calculate(null, float.NaN));
        Assert.Throws<ArgumentException>(() => SoundRolloffCalculator.Calculate(new(MinDistance: float.PositiveInfinity), 1));
        Assert.Throws<ArgumentException>(() => SoundRolloffCalculator.Calculate(new(SoundRolloffType.Log, 0, 0), 1));
    }
}
