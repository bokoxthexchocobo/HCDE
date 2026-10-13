namespace HCDE.Client.Tests;

public class VmReplayGainKernelsTests
{
    [Theory]
    [InlineData(96000, 0)]
    [InlineData(88200, 1)]
    [InlineData(64000, 2)]
    [InlineData(48000, 3)]
    [InlineData(49716, 3)]
    [InlineData(44100, 4)]
    [InlineData(32000, 5)]
    [InlineData(24000, 6)]
    [InlineData(22050, 7)]
    [InlineData(16000, 8)]
    [InlineData(12000, 9)]
    [InlineData(11025, 10)]
    [InlineData(11111, 10)]
    [InlineData(8000, 11)]
    [InlineData(18900, 12)]
    [InlineData(37800, 13)]
    public void SupportedRatesSelectCompleteNativeKernels(int rate, int index)
    {
        Assert.Equal(index, VmReplayGainKernels.IndexFor(rate));
        Assert.Equal(21, VmReplayGainKernels.YuleFor(rate).Length);
        Assert.Equal(5, VmReplayGainKernels.ButterFor(rate).Length);
    }

    [Fact]
    public void AliasesShareCoefficientsButWindowUsesActualRate()
    {
        Assert.True(VmReplayGainKernels.YuleFor(48000).SequenceEqual(VmReplayGainKernels.YuleFor(49716)));
        Assert.True(VmReplayGainKernels.ButterFor(11025).SequenceEqual(VmReplayGainKernels.ButterFor(11111)));
        Assert.Equal(2400, VmReplayGainKernels.WindowSamples(48000));
        Assert.Equal(2486, VmReplayGainKernels.WindowSamples(49716));
        Assert.Equal(552, VmReplayGainKernels.WindowSamples(11025));
        Assert.Equal(556, VmReplayGainKernels.WindowSamples(11111));
        Assert.Equal(.05418656406430f, VmReplayGainKernels.YuleFor(44100)[0]);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmReplayGainKernels.WindowSamples(12345));
    }
}
