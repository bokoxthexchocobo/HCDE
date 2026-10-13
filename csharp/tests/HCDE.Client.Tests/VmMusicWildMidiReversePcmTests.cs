using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiReversePcmTests
{
    [Theory]
    [InlineData(16, -256, -32768, 32512, 0)]
    [InlineData(18, 32512, 0, -256, -32768)]
    public void EightBitReversePreservesSignednessAndMirrorsLoop(byte modes, short a, short b, short c, short d)
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode([0, 127, 128, 255], 0, 3, modes, 0x2a);
        Assert.Equal(new short[] { a, b, c, d, 0, 0 }, decoded.Data);
        Assert.Equal(1U, decoded.LoopStart);
        Assert.Equal(4U, decoded.LoopEnd);
        Assert.Equal(4U, decoded.DataLength);
        Assert.Equal(0, decoded.Modes);
        Assert.Equal(0xa2, decoded.LoopFraction);
    }

    [Theory]
    [InlineData(17, -1, -32768, 32767, 0)]
    [InlineData(19, 32767, 0, -1, -32768)]
    public void SixteenBitReverseMirrorsByteOffsetsBeforeTruncating(byte modes, short a, short b, short c, short d)
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode([0, 0, 255, 127, 0, 128, 255, 255], 1, 5, modes, 0x37);
        Assert.Equal(new short[] { a, b, c, d, 0, 0 }, decoded.Data);
        Assert.Equal(1U, decoded.LoopStart);
        Assert.Equal(3U, decoded.LoopEnd);
        Assert.Equal(1, decoded.Modes);
        Assert.Equal(0x73, decoded.LoopFraction);
    }

    [Fact]
    public void ForwardDecodeRetainsFractionAndOtherModes()
    {
        var decoded = VmMusicWildMidiPcmDecoder.Decode([1, 2], 0, 1, 4 | 64, 0x29);
        Assert.Equal(0x29, decoded.LoopFraction);
        Assert.Equal(4 | 64, decoded.Modes);
    }

    [Fact]
    public void ReversePingPongAndInvalidBoundsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(new byte[] { 1, 2 }, 0, 1, 24));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(new byte[] { 1, 2 }, 0, 3, 16));
    }
}
