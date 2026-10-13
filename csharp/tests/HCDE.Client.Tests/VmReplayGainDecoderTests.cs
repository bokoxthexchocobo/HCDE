namespace HCDE.Client.Tests;

public class VmReplayGainDecoderTests
{
    private sealed class Decoder : IVmReplayGainDecoder
    {
        public List<string> Calls { get; } = [];
        public bool Starts { get; init; } = true;
        public int BufferBytes { get; init; } = 800;
        public int SampleRate { get; init; } = 8000;
        public bool FailsFill { get; init; }
        public bool Start(int subsong, bool loop)
        {
            Assert.Equal(0, subsong);
            Assert.False(loop);
            Calls.Add("start");
            return Starts;
        }
        public VmMusicStreamFormat GetFormat() { Calls.Add("format"); return new(BufferBytes, SampleRate, 1, false); }
        public bool FillFloat(Span<float> samples) => throw new InvalidOperationException("unexpected float");
        public bool FillInt16(Span<short> samples)
        {
            Calls.Add("fill");
            if (FailsFill) throw new InvalidOperationException("decoder failed");
            return false;
        }
        public void Dispose() => Calls.Add("close");
    }

    [Fact]
    public void SuccessfulAnalysisStartsNonloopingAndClosesDecoder()
    {
        var decoder = new Decoder();
        Assert.Equal(-24601, VmReplayGainDecoder.Calculate(() => decoder));
        Assert.Equal(new[] { "start", "format", "fill", "close" }, decoder.Calls);
    }

    [Fact]
    public void MissingDecoderIsUnavailable()
        => Assert.Null(VmReplayGainDecoder.Calculate(() => null));

    [Theory]
    [InlineData(false, 800)]
    [InlineData(true, 0)]
    public void FailedStartAndDeviceOwnedOutputCloseWithoutCollecting(bool starts, int bufferBytes)
    {
        var decoder = new Decoder { Starts = starts, BufferBytes = bufferBytes };
        Assert.Null(VmReplayGainDecoder.Calculate(() => decoder));
        Assert.Equal(starts ? new[] { "start", "format", "close" } : new[] { "start", "close" }, decoder.Calls);
    }

    [Fact]
    public void UnsupportedAnalysisRateIsRejectedOnlyAfterCollectionAndClose()
    {
        var decoder = new Decoder { SampleRate = 12345 };
        Assert.Throws<ArgumentOutOfRangeException>(() => VmReplayGainDecoder.Calculate(() => decoder));
        Assert.Equal(new[] { "start", "format", "fill", "close" }, decoder.Calls);
    }

    [Fact]
    public void FillExceptionPropagatesAfterClosingDecoder()
    {
        var decoder = new Decoder { FailsFill = true };
        Assert.Throws<InvalidOperationException>(() => VmReplayGainDecoder.Calculate(() => decoder));
        Assert.Equal(new[] { "start", "format", "fill", "close" }, decoder.Calls);
    }
}
