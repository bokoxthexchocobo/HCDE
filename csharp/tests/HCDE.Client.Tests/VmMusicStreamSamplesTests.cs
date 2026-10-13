namespace HCDE.Client.Tests;

public class VmMusicStreamSamplesTests
{
    [Fact]
    public void Int16ConversionPreservesSignedRangeAndGainWithoutClipping()
    {
        short[] source = [short.MinValue, -16384, 0, 16384, short.MaxValue];
        var output = new float[source.Length];
        Assert.True(VmMusicStreamSamples.ConvertInt16(source, output, 2, true));
        Assert.Equal(new float[] { -2, -1, 0, 1, 65534f / 32768f }, output);
    }

    [Fact]
    public void FloatGainProcessesEveryInterleavedSample()
    {
        float[] samples = [-2, .5f, 1, -1];
        Assert.True(VmMusicStreamSamples.ApplyFloat(samples, .5f, true));
        Assert.Equal(new float[] { -1, .25f, .5f, -.5f }, samples);
    }

    [Fact]
    public void FailedFillClearsTheEntireOutputForEitherFormat()
    {
        float[] samples = [1, 2, 3];
        Assert.False(VmMusicStreamSamples.ApplyFloat(samples, 2, false));
        Assert.Equal(new float[3], samples);
        samples.AsSpan().Fill(4);
        Assert.False(VmMusicStreamSamples.ConvertInt16([], samples, 2, false));
        Assert.Equal(new float[3], samples);
    }

    [Fact]
    public void ShortSuccessfulInputFailsBeforeChangingOutput()
    {
        float[] samples = [1, 2];
        Assert.Throws<ArgumentException>(() => VmMusicStreamSamples.ConvertInt16(new short[1], samples, 1, true));
        Assert.Equal(new float[] { 1, 2 }, samples);
    }
}
