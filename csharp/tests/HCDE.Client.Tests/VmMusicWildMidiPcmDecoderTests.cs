using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiPcmDecoderTests
{
    [Theory]
    [InlineData(0, 0, 32512, -32768, -256)]
    [InlineData(2, -32768, -256, 0, 32512)]
    public void EightBitSignednessAndGuardSamples(byte modes, short a, short b, short c, short d)
    {
        var decoded = VmMusicWildMidiPcmDecoder.DecodeForward([0, 127, 128, 255], 1, 4, modes);
        Assert.Equal(new short[] { a, b, c, d, 0, 0 }, decoded.Data);
        Assert.Equal(4U, decoded.DataLength);
        Assert.Equal(1U, decoded.LoopStart);
        Assert.Equal(4U, decoded.LoopEnd);
        Assert.Equal(0, decoded.Modes & 2);
    }

    [Theory]
    [InlineData(1, 0, 32767, -32768, -1)]
    [InlineData(3, -32768, -1, 0, 32767)]
    public void SixteenBitLittleEndianAndByteBounds(byte modes, short a, short b, short c, short d)
    {
        var decoded = VmMusicWildMidiPcmDecoder.DecodeForward([0, 0, 255, 127, 0, 128, 255, 255], 3, 8, modes);
        Assert.Equal(new short[] { a, b, c, d, 0, 0 }, decoded.Data);
        Assert.Equal(4U, decoded.DataLength);
        Assert.Equal(1U, decoded.LoopStart);
        Assert.Equal(4U, decoded.LoopEnd);
        Assert.Equal(1, decoded.Modes);
    }

    [Fact]
    public void DecodedSamplesCanFeedExistingLinearMixer()
    {
        var decoded = VmMusicWildMidiPcmDecoder.DecodeForward([1, 3], 0, 0, 0);
        var voice = new VmMusicWildMidiNoteReleaseState
        {
            SamplePosition = 512, EnvelopeLevel = 1024 << 12, MixVolume = new(1024, 1024)
        };
        Assert.Equal(new(512, 512), VmMusicWildMidiLinearMixer.Interpolate(decoded.Data, voice));
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    public void UnsupportedTransformsAreExplicit(byte modes)
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.DecodeForward(new byte[] { 1, 2 }, 0, 0, modes));
    }

    [Fact]
    public void MalformedPayloadAndBoundsThrow()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.DecodeForward(Array.Empty<byte>(), 0, 0, 0));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.DecodeForward(new byte[] { 1 }, 0, 0, 1));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.DecodeForward(new byte[] { 1 }, 0, 2, 0));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.DecodeForward(new byte[] { 1 }, 1, 0, 0));
    }
}
