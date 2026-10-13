namespace HCDE.Client.Tests;

public class VmMusicSoftSynthStreamInfoTests
{
    [Theory]
    [InlineData(44100, 2, false, 176400)]
    [InlineData(44100, 2, true, 88200)]
    [InlineData(48000, 4, false, 96000)]
    [InlineData(44101, 2, false, 176400)]
    [InlineData(1, 2, false, 0)]
    public void FormatMatchesNativeIntegerDivisionAndChannelSize(int rate, int block, bool mono, int bytes)
    {
        Assert.Equal(new VmMusicStreamFormat(bytes, rate, mono ? 1 : 2, true),
            VmMusicSoftSynthStreamInfo.Get(rate, block, mono));
    }

    [Fact]
    public void InvalidInputsAndOverflowAreReported()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicSoftSynthStreamInfo.Get(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicSoftSynthStreamInfo.Get(44100, 0));
        Assert.Throws<OverflowException>(() => VmMusicSoftSynthStreamInfo.Get(int.MaxValue, 1));
    }
}
