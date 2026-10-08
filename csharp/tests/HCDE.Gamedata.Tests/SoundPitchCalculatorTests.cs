namespace HCDE.Gamedata.Tests;

public class SoundPitchCalculatorTests
{
    [Theory]
    [InlineData(1.5f, 0f)]
    [InlineData(1.5f, 1.5f)]
    public void FixedPitchOverridesShiftWithoutRandomDraws(float pitch, float maximum)
    {
        var setting = new SndInfoSoundSettings(PitchMask: 127, DefPitch: pitch, DefPitchMax: maximum);
        Assert.Equal(pitch, SoundPitchCalculator.Calculate(setting, false,
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException()));
    }

    [Theory]
    [InlineData(1f, 2f, 0.25f, 1.25f)]
    [InlineData(2f, 1f, 0.25f, 1.75f)]
    public void RangedPitchInterpolatesWithOneFloatDraw(float pitch, float maximum, float draw, float expected)
    {
        var count = 0;
        Assert.Equal(expected, SoundPitchCalculator.Calculate(new(DefPitch: pitch, DefPitchMax: maximum), false,
            nextFloat: () => { count++; return draw; }));
        Assert.Equal(1, count);
    }

    [Fact]
    public void ShiftUsesTwoMaskedBytesInNativeOrder()
    {
        var draws = new Queue<byte>(new byte[] { 255, 248 });
        Assert.Equal(121 / 128f, SoundPitchCalculator.Calculate(new(PitchMask: 7, DefPitch: -1), true, () => draws.Dequeue()));
        Assert.Empty(draws);
    }

    [Theory]
    [InlineData(false, 7)]
    [InlineData(true, 0)]
    public void DisabledVariationDoesNotDraw(bool pitched, int mask)
    {
        Assert.Equal(1, SoundPitchCalculator.Calculate(new(PitchMask: mask), pitched, () => throw new InvalidOperationException()));
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1f)]
    [InlineData(float.NaN)]
    public void InvalidFloatDrawFailsExplicitly(float draw)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SoundPitchCalculator.Calculate(new(DefPitch: 1, DefPitchMax: 2), true, nextFloat: () => draw));
    }

    [Fact]
    public void RequiredProvidersCannotBeOmitted()
    {
        Assert.Throws<ArgumentException>(() => SoundPitchCalculator.Calculate(new(PitchMask: 7), true));
        Assert.Throws<ArgumentException>(() => SoundPitchCalculator.Calculate(new(DefPitch: 1, DefPitchMax: 2), true));
    }
}
