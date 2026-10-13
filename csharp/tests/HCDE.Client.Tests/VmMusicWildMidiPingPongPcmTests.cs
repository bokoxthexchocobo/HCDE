using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiPingPongPcmTests
{
    [Theory]
    [InlineData(0, 4194304, 0U, 0U)]
    [InlineData(1, 8388608, 0U, 0U)]
    [InlineData(8, 2097152, 1U, 1048577U)]
    [InlineData(24, 2097152, 1U, 1048577U)]
    [InlineData(25, 4194304, 2U, 2097154U)]
    public void DecodingAndLoopExpansionRejectSamplesBeyondFixedPointRange(byte modes, int length, uint start, uint end)
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(new byte[length], start, end, modes));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SixteenBitReversePingPongUsesCompleteSamplesAndSampleUnitBounds(bool unsigned)
    {
        short[] original = [0x101, 0x202, 0x303, 0x404, 0x505, 0x606, 0x707];
        var bytes = new byte[original.Length * 2];
        for (var i = 0; i < original.Length; i++)
            System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(i * 2),
                (ushort)(unsigned ? original[i] ^ 0x8000 : original[i]));
        var decoded = VmMusicWildMidiPcmDecoder.Decode(bytes, 4, 8, (byte)(unsigned ? 31 : 29), 0x39);
        Assert.Equal(new short[] { 0x707, 0x606, 0x505, 0x404, 0x303, 0x404, 0x505,
            0x404, 0x303, 0x202, 0x101, 0, 0 }, decoded.Data);
        Assert.Equal(11U, decoded.DataLength);
        Assert.Equal(4U, decoded.LoopStart);
        Assert.Equal(8U, decoded.LoopEnd);
        Assert.Equal(5, decoded.Modes);
        Assert.Equal(0x39, decoded.LoopFraction);
        var sample = VmMusicWildMidiPcmDecoder.ToMixerSample(decoded);
        Assert.Equal(11U << 10, sample.Bounds.DataLength);
        var patchBytes = new byte[335 + bytes.Length];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(patchBytes);
        patchBytes[198] = 1;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(247), (uint)bytes.Length);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(251), 4);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(255), 8);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(patchBytes.AsSpan(259), 44100);
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(269), 440000);
        patchBytes[294] = (byte)(unsigned ? 31 : 29);
        bytes.CopyTo(patchBytes, 335);
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        patch.Load(patchBytes);
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, _ => patch, _ => 0);
        renderer.ShortEvent(0x90, 60, 100);
        var output = new float[128];
        renderer.ComputeOutput(output, 64);
        Assert.Contains(output, value => value != 0);
        Assert.All(output, value => Assert.True(float.IsFinite(value)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EightBitReversePingPongPreservesNativeExpansionAndTailSpacing(bool unsigned)
    {
        var bytes = Enumerable.Range(1, 7).Select(value => (byte)(unsigned ? value ^ 128 : value)).ToArray();
        var decoded = VmMusicWildMidiPcmDecoder.Decode(bytes, 2, 4, (byte)(unsigned ? 30 : 28), 0x39);
        short[] expected = unsigned ? [7, 6, 5, 4, 3, 4, 5, 4, 3, 2, 1, 0, 0]
            : [7, 6, 5, 4, 3, 4, 5, 4, 3, 2, 0, 1, 0];
        Assert.Equal(expected.Select(value => (short)(value << 8)), decoded.Data);
        Assert.Equal(11U, decoded.DataLength);
        Assert.Equal(4U, decoded.LoopStart);
        Assert.Equal(8U, decoded.LoopEnd);
        Assert.Equal(4, decoded.Modes);
        Assert.Equal(0x39, decoded.LoopFraction);
        Assert.Equal(11U << 10, VmMusicWildMidiPcmDecoder.ToMixerSample(decoded).Bounds.DataLength);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(9)]
    [InlineData(11)]
    public void ForwardPingPongExpandsLoopWithNativeEndpointOrdering(byte modes)
    {
        var sixteen = (modes & 1) != 0;
        var unsigned = (modes & 2) != 0;
        var bytes = new List<byte>();
        for (var i = 1; i <= 7; i++)
        {
            if (sixteen) bytes.Add(0);
            bytes.Add((byte)(unsigned ? i ^ 128 : i));
        }
        var scale = sixteen ? 2U : 1U;
        var decoded = VmMusicWildMidiPcmDecoder.Decode(bytes.ToArray(), scale, 4 * scale, modes, 0x39);
        Assert.Equal(new short[] { 256, 512, 768, 1024, 1280, 1024, 768, 512, 768, 1024, 1280, 1536, 1792, 0, 0 }, decoded.Data);
        Assert.Equal(13U, decoded.DataLength);
        Assert.Equal(4U, decoded.LoopStart);
        Assert.Equal(10U, decoded.LoopEnd);
        Assert.Equal(sixteen ? modes & 3 : 0, decoded.Modes);
        Assert.Equal(0x39, decoded.LoopFraction);
    }

    [Fact]
    public void UnsafeNativeBoundaryCasesAndMisalignmentThrow()
    {
        var bytes = new byte[8];
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 0, 3, 8));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 1, 2, 8));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 1, 8, 8));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 1, 6, 9));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 1, 6, 25));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 2, 5, 25));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(new byte[9], 2, 6, 25));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(bytes, 1, 7, 24));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiPcmDecoder.Decode(new byte[10], 4, 6, 24));
    }
}
