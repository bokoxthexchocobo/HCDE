using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiGusSampleHeaderTests
{
    [Theory]
    [InlineData(0U)]
    [InlineData(1U)]
    [InlineData(8388608U)]
    public void ZeroTruncatedOrWrappedPitchDivisorFailsBeforePatchPublication(uint root)
    {
        var header = Header();
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(30), root);
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusSampleHeader.Parse(header));
        var bytes = new byte[340];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1;
        header.CopyTo(bytes, 239);
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        Assert.Throws<ArgumentException>(() => patch.Load(bytes));
        Assert.False(patch.Loaded);
        Assert.False(patch.HasSamples);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(269), 440000);
        patch.Load(bytes);
        Assert.True(patch.Loaded);
        Assert.True(patch.HasSamples);
    }

    private static byte[] Header()
    {
        var header = new byte[96];
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(8), 5);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(12), 1);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(16), 3);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(20), 22050);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(22), 100000);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(26), 900000);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(30), 440000);
        header[7] = 0xa3;
        header[55] = 4;
        return header;
    }

    [Fact]
    public void ParsesNativeOffsetsAndTruncatedDivisorArithmetic()
    {
        var parsed = VmMusicWildMidiGusSampleHeader.Parse(Header());
        Assert.Equal(5U, parsed.ByteLength);
        Assert.Equal(22050, parsed.Rate);
        Assert.Equal(100000U, parsed.FrequencyLow);
        Assert.Equal(900000U, parsed.FrequencyHigh);
        Assert.Equal(440000U, parsed.FrequencyRoot);
        Assert.Equal(20432U, parsed.IncrementDivisor);
        Assert.Equal(4, parsed.Modes);
    }

    [Fact]
    public void ReversedHeaderBoundsSwapFractionBeforeDecoding()
    {
        var bytes = Header();
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(12), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(16), 1);
        var header = VmMusicWildMidiGusSampleHeader.Parse(bytes);
        Assert.Equal(1U, header.LoopStart);
        Assert.Equal(3U, header.LoopEnd);
        Assert.Equal(0x3a, header.LoopFraction);
        var sample = VmMusicWildMidiPcmDecoder.ToMixerSample(header.Decode([1, 2, 3, 4, 5, 99]));
        Assert.Equal(new(5120, 1664, 3264, 1600), sample.Bounds);
        Assert.Equal(7, sample.Data.Length);
    }

    [Fact]
    public void HeaderReverseModeComposesWithBoundsNormalization()
    {
        var bytes = Header();
        bytes[55] = 20;
        var decoded = VmMusicWildMidiGusSampleHeader.Parse(bytes).Decode([1, 2, 3, 4, 5]);
        Assert.Equal(new short[] { 1280, 1024, 768, 512, 256, 0, 0 }, decoded.Data);
        Assert.Equal(0x3a, decoded.LoopFraction);
    }

    [Fact]
    public void InvalidHeaderRateAndTruncatedPayloadThrow()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusSampleHeader.Parse(new byte[95]));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusSampleHeader.Parse(new byte[96]));
        var header = VmMusicWildMidiGusSampleHeader.Parse(Header());
        Assert.Throws<ArgumentException>(() => header.Decode(new byte[4]));
    }
}
