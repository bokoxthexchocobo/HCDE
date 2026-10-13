namespace HCDE.Client.Tests;

public class VmMusicFmOutputTests
{
    [Fact]
    public void StereoSamplesUseGainWithoutClippingAndPreserveBufferOutsideRequestedFrames()
    {
        var device = new Device([1, -1, 0.5f, -0.25f], 4);
        float[] buffer = [9, 9, 9, 9, 99, 100];
        Assert.Equal(4, VmMusicFmOutput.ComputeOutput(device, buffer, 2, 3.8f));
        Assert.Equal(new float[] { 3.8f, -3.8f, 1.9f, -0.95f, 99, 100 }, buffer);
        Assert.Equal(4, device.RequestedSamples);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void ShortRenderScalesOnlyProducedSamplesAndSilencesUnwrittenFrames(int written)
    {
        var device = new Device([1, -1], written);
        float[] buffer = [9, 9, 9, 9, 99];
        Assert.Equal(written, VmMusicFmOutput.ComputeOutput(device, buffer, 2, 2.5f));
        Assert.Equal(written == 0 ? new float[] { 0, 0, 0, 0, 99 } : new float[] { 2.5f, -2.5f, 0, 0, 99 }, buffer);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(6)]
    public void InvalidSampleCountsFailExplicitly(int written)
    {
        var error = Assert.Throws<InvalidOperationException>(() =>
            VmMusicFmOutput.ComputeOutput(new Device([], written), new float[4], 2, 1));
        Assert.Contains(written.ToString(), error.Message);
    }

    [Fact]
    public void InvalidFrameCountAndCapacityFailBeforeCallingRenderer()
    {
        var device = new Device([], 0);
        Assert.Throws<ArgumentOutOfRangeException>(() => VmMusicFmOutput.ComputeOutput(device, new float[4], -1, 1));
        Assert.Throws<ArgumentException>(() => VmMusicFmOutput.ComputeOutput(device, new float[3], 2, 1));
        Assert.Throws<OverflowException>(() => VmMusicFmOutput.ComputeOutput(device, [], int.MaxValue, 1));
        Assert.Equal(0, device.Calls);
    }

    [Fact]
    public void ZeroFramesPreserveOutputOutsideTheRequest()
    {
        float[] buffer = [99];
        var device = new Device([], 0);
        Assert.Equal(0, VmMusicFmOutput.ComputeOutput(device, buffer, 0, 4));
        Assert.Equal(0, device.RequestedSamples);
        Assert.Equal(new float[] { 99 }, buffer);
    }

    [Fact]
    public void RendererExceptionPropagatesWithoutSilentFallback()
    {
        var original = new InvalidOperationException("render failed");
        var device = new Device([], 0) { Failure = original };
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() =>
            VmMusicFmOutput.ComputeOutput(device, new float[4], 2, 1)));
    }

    private sealed class Device(float[] samples, int written) : IVmMusicFmAudioDevice
    {
        public int RequestedSamples { get; private set; }
        public int Calls { get; private set; }
        public Exception? Failure { get; init; }
        public int Generate(Span<float> stereoSamples)
        {
            Calls++;
            RequestedSamples = stereoSamples.Length;
            if (Failure is not null) throw Failure;
            samples.AsSpan().CopyTo(stereoSamples);
            return written;
        }
    }
}
