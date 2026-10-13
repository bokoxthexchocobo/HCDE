namespace HCDE.Client.Tests;

public class VmReplayGainCollectionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CollectsSuccessfulBlocksAndDiscardsFailedFillOutput(bool floating)
    {
        var calls = 0;
        var gain = VmReplayGainCollection.Analyze(new(400 * (floating ? 4 : 2), 8000, 1, floating),
            output => { Assert.True(floating); output.Fill(calls++ == 0 ? 0 : 1); return calls == 1; },
            output => { Assert.False(floating); output.Fill(calls++ == 0 ? (short)0 : short.MaxValue); return calls == 1; });
        Assert.Equal(2, calls);
        Assert.Equal(64.82f, gain);
    }

    [Fact]
    public void LoopingDecoderStopsOnlyAfterDurationExceedsEightMinutes()
    {
        var calls = 0;
        var gain = VmReplayGainCollection.Analyze(new(8000 * 2 * 240, 8000, 1, false),
            _ => throw new InvalidOperationException(), output => { calls++; output.Clear(); return true; });
        Assert.Equal(3, calls);
        Assert.Equal(64.82f, gain);
    }

    [Fact]
    public void IncompleteStereoFrameIsRejectedBeforeDecoderInvocation()
        => Assert.Throws<ArgumentOutOfRangeException>(() => VmReplayGainCollection.Analyze(new(2, 8000, 2, false),
            _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException()));

    [Fact]
    public void NoDecodedSamplesReturnNativeInsufficientSamplesSentinel()
        => Assert.Equal(-24601, VmReplayGainCollection.Analyze(new(800, 8000, 1, false),
            _ => throw new InvalidOperationException(), _ => false));
}
