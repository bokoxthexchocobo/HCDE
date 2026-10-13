namespace HCDE.Client.Tests;

public class VmMusicWildMidiMixerOutputTests
{
    [Fact]
    public void ConversionUsesNativeBoostWithoutClippingAndPreservesBufferTail()
    {
        var output = new float[] { 9, 9, 9, 9, 9, 9, 9 };
        VmMusicWildMidiMixerOutput.Convert([32768, -32768, 65536, -65536, 0, 16384], output);
        Assert.Equal(new float[] { 1.3f, -1.3f, 2.6f, -2.6f, 0, 0.65f, 9 }, output);
    }

    [Fact]
    public void ExtremeMixerValuesConvertThroughFloatBeforeMultiplication()
    {
        var output = new float[2];
        VmMusicWildMidiMixerOutput.Convert([int.MaxValue, int.MinValue], output);
        Assert.Equal((float)int.MaxValue * (1.3f / 32768f), output[0]);
        Assert.Equal((float)int.MinValue * (1.3f / 32768f), output[1]);
        Assert.True(float.IsFinite(output[0]));
        Assert.True(float.IsFinite(output[1]));
    }

    [Fact]
    public void InvalidBuffersAreRejectedBeforeMutationAndEmptyInputLeavesTail()
    {
        var output = new float[] { 9, 9 };
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiMixerOutput.Convert([1], output));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiMixerOutput.Convert([1, 2, 3, 4], output));
        VmMusicWildMidiMixerOutput.Convert([], output);
        Assert.Equal(new float[] { 9, 9 }, output);
    }
}
