namespace HCDE.Client.Tests;

public class PcmResamplerTests
{
    [Fact]
    public void PitchChangesSourceProgressionAndDuration()
    {
        Assert.Equal(new short[] { 0, 20 }, PcmResampler.Convert(new short[] { 0, 10, 20, 30 }, 11025, 11025, 2));
        Assert.Equal(new short[] { 0, 5, 10, 10 }, PcmResampler.Convert(new short[] { 0, 10 }, 11025, 11025, 0.5f));
        Assert.Equal(new short[] { 0, 10 }, PcmResampler.Convert(new short[] { 0, 10 }, 22050, 11025, 0.5f));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    public void InvalidPitchFails(float pitch)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PcmResampler.Convert(new short[] { 1 }, 11025, 11025, pitch));
    }

    [Fact]
    public void UpsamplingInterpolatesAndHoldsFinalSample()
    {
        Assert.Equal(new short[] { 0, 50, 100, 100 }, PcmResampler.Convert(new short[] { 0, 100 }, 11025, 22050));
    }

    [Fact]
    public void DownsamplingPreservesDurationAndSourcePositions()
    {
        Assert.Equal(new short[] { 10, 30, 50 }, PcmResampler.Convert(new short[] { 10, 20, 30, 40, 50 }, 22050, 11025));
    }

    [Fact]
    public void EqualRatesCopyAndEmptyInputRemainsEmpty()
    {
        var source = new short[] { -32768, 32767 };
        var output = PcmResampler.Convert(source, 11025, 11025);
        Assert.Equal(source, output); Assert.NotSame(source, output);
        Assert.Empty(PcmResampler.Convert([], 11025, 22050));
    }

    [Theory]
    [InlineData(0, 11025)]
    [InlineData(11025, -1)]
    public void InvalidRatesFail(int source, int target)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PcmResampler.Convert(new short[] { 1 }, source, target));
    }

    [Fact]
    public void FractionalRateRatioUsesCeilingDurationAndSignedRounding()
    {
        Assert.Equal(new short[] { -10, 1, 8 }, PcmResampler.Convert(new short[] { -10, 5, 10 }, 3, 4)[..3]);
        Assert.Equal(4, PcmResampler.Convert(new short[] { -10, 5, 10 }, 3, 4).Length);
    }
}
