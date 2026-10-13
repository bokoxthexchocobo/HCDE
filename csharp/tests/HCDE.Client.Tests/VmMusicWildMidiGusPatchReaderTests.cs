using System.Buffers.Binary;
using System.Text;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiGusPatchReaderTests
{
    private static byte[] Patch(string version = "110", int count = 2)
    {
        var bytes = new byte[239 + count * 98];
        Encoding.ASCII.GetBytes($"GF1PATCH{version}\0ID#000002\0").CopyTo(bytes, 0);
        bytes[198] = (byte)count;
        for (var i = 0; i < count; i++)
        {
            var offset = 239 + i * 98;
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 8), 2);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset + 20), 22050);
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 30), 440000);
            bytes[offset + 37] = (byte)(10 + i);
            bytes[offset + 43] = (byte)(20 + i);
            bytes[offset + 96] = (byte)(1 + i);
            bytes[offset + 97] = (byte)(3 + i);
        }
        return bytes;
    }

    [Theory]
    [InlineData("100")]
    [InlineData("110")]
    public void ReadsBothNativeFormatsAndAdvancesByEncodedPayloadLength(string version)
    {
        var patch = Patch(version);
        var samples = VmMusicWildMidiGusPatchReader.Read(patch);
        Assert.Equal(2, samples.Count);
        Assert.Equal(new short[] { 256, 768, 0, 0 }, samples[0].Pcm.Data);
        Assert.Equal(new short[] { 512, 1024, 0, 0 }, samples[1].Pcm.Data);
        Assert.Equal(10, samples[0].EnvelopeRates.Span[0]);
        Assert.Equal(21, samples[1].EnvelopeLevels.Span[0]);
        Assert.Equal(2048U, samples[1].MixerSample.Bounds.DataLength);
        Array.Clear(patch);
        Assert.Equal(10, samples[0].EnvelopeRates.Span[0]);
    }

    [Fact]
    public void EmptyPatchAndTrailingDataFollowNativeSampleCount()
    {
        Assert.Empty(VmMusicWildMidiGusPatchReader.Read(Patch(count: 0)));
        Assert.Equal(2, VmMusicWildMidiGusPatchReader.Read(Patch().Concat(new byte[] { 99 }).ToArray()).Count);
    }

    [Fact]
    public void TruncatedHeadersAndPayloadsThrow()
    {
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusPatchReader.Read(new byte[238]));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusPatchReader.Read(Patch()[..250]));
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusPatchReader.Read(Patch()[..^1]));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(82)]
    [InlineData(151)]
    public void InvalidSignatureOrUnsupportedStructureThrows(int offset)
    {
        var patch = Patch();
        patch[offset] = 2;
        Assert.Throws<ArgumentException>(() => VmMusicWildMidiGusPatchReader.Read(patch));
    }
}
