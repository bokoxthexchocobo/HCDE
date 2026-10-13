namespace HCDE.Client.Tests;

public class VmReplayGainFilterTests
{
    [Fact]
    public void ButterImpulseResponseRetainsFeedbackAcrossCallsAndResetClearsIt()
    {
        var filter = new VmReplayGainFilter([1, -.5f, 0, 0, 0], false);
        var first = new float[2];
        filter.Process([1, 0], first);
        Assert.Equal(new float[] { 1, .5f }, first);
        var next = new float[2];
        filter.Process([0, 0], next);
        Assert.Equal(new float[] { .25f, .125f }, next);
        filter.Reset();
        filter.Process([0, 0], next);
        Assert.Equal(new float[2], next);
    }

    [Fact]
    public void YuleIncludesTenthDelayedInputAndNativeDenormalBias()
    {
        var kernel = new float[21];
        kernel[20] = 1;
        var filter = new VmReplayGainFilter(kernel, true);
        kernel[20] = 0;
        var input = new float[11];
        input[0] = 2;
        var output = new float[11];
        filter.Process(input, output);
        Assert.All(output.Take(10), value => Assert.Equal(1e-10f, value));
        Assert.Equal(2, output[10]);
    }

    [Fact]
    public void ExactInPlaceProcessingIsSupportedButPartialOverlapAndShortOutputFail()
    {
        var filter = new VmReplayGainFilter([1, 0, 1, 0, 0], false);
        float[] samples = [1, 2, 3];
        filter.Process(samples, samples);
        Assert.Equal(new float[] { 1, 3, 5 }, samples);
        Assert.Throws<ArgumentException>(() => filter.Process(samples.AsSpan(0, 2), samples.AsSpan(1, 2)));
        Assert.Throws<ArgumentException>(() => filter.Process(samples, new float[2]));
        Assert.Throws<ArgumentException>(() => new VmReplayGainFilter(new float[5], true));
    }
}
