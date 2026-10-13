namespace HCDE.Client.Tests;

public class VmReplayGainSamplesTests
{
    [Theory]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void AnalysisPreparationRetainsPcmScaleAndSplitsChannelsAcrossBlocks(int channels, bool floating)
    {
        var samples = new VmReplayGainSamples(44100, channels);
        for (var i = 0; i < 2; i++)
        {
            if (floating) samples.AppendFloat([-.5f, .25f]);
            else samples.AppendInt16([-16384, 8192]);
        }
        Assert.Equal(channels == 1 ? new float[] { -16384, 8192, -16384, 8192 }
            : new float[] { -16384, -16384 }, samples.Left);
        Assert.Equal(channels == 1 ? Array.Empty<float>() : new float[] { 8192, 8192 }, samples.Right);
    }

    [Fact]
    public void LimitIsStrictlyGreaterThanEightMinutesAndTrailingPartialFrameIsIgnored()
    {
        var samples = new VmReplayGainSamples(1, 2);
        samples.AppendInt16(new short[960]);
        Assert.False(samples.ExceedsAnalysisLimit);
        samples.AppendInt16([1]);
        Assert.Equal(480, samples.Left.Count);
        samples.AppendFloat([1, -1]);
        Assert.True(samples.ExceedsAnalysisLimit);
        Assert.Equal(32768, samples.Left[^1]);
        Assert.Equal(-32768, samples.Right[^1]);
    }

    [Fact]
    public void UnsupportedFormatIsRejectedBeforeAccumulation()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new VmReplayGainSamples(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new VmReplayGainSamples(44100, 6));
    }
}
