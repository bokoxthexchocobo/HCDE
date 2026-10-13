using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiPcmBoundsTests
{
    [Theory]
    [InlineData(0, 1024, 3072)]
    [InlineData(0xa3, 1216, 3712)]
    [InlineData(0xff, 1984, 4032)]
    public void FractionNibblesBecomeTenBitPositions(byte fraction, uint start, uint end)
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode(new byte[5], 1, 3, 4, fraction);
        var sample = VmMusicWildMidiPcmDecoder.ToMixerSample(decoded);
        Assert.Equal(new(5120, start, end, end - start), sample.Bounds);
        Assert.True(sample.Data.Equals(new ReadOnlyMemory<short>(decoded.Data)));
    }

    [Fact]
    public void ReverseFractionMetadataFlowsIntoMixerBounds()
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode(new byte[5], 1, 3, 20, 0xa3);
        var sample = VmMusicWildMidiPcmDecoder.ToMixerSample(decoded);
        Assert.Equal(new(5120, 2688, 4288, 1600), sample.Bounds);
    }

    [Fact]
    public void DecodedLoopCanRenderThroughMixerWithoutManualBounds()
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode([1, 2, 3, 4, 5], 1, 3, 4, 0);
        var sample = VmMusicWildMidiPcmDecoder.ToMixerSample(decoded);
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            Modes = decoded.Modes, SamplePosition = 3072, SampleIncrement = 1024,
            EnvelopeLevel = 1024 << 12, MixVolume = new(1024, 1024)
        };
        var output = new int[4];
        VmMusicWildMidiLinearMixer.Mix(new List<VmMusicWildMidiNoteReleaseState> { voice }, output, _ => sample);
        Assert.Equal(new[] { 1024, 1024, 768, 768 }, output);
    }

    [Fact]
    public void InvalidLengthGuardsAndLoopBoundsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.ToMixerSample(new(new short[2], 1, 0, 0, 0)));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.ToMixerSample(new([], 1U << 22, 0, 0, 0)));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.ToMixerSample(new(new short[4], 2, 1, 1, 4)));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.ToMixerSample(new(new short[4], 2, 0, 2, 4)));
    }
}
