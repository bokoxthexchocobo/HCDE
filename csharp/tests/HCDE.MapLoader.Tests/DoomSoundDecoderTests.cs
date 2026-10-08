using System.Buffers.Binary;

namespace HCDE.MapLoader.Tests;

public class DoomSoundDecoderTests
{
    [Theory]
    [InlineData(0, 11025)]
    [InlineData(22050, 22050)]
    public void DecodesUnsignedSamplesAndNativeDefaultFrequency(ushort frequency, int expected)
    {
        var data = Sound(frequency, 4, [0, 128, 255, 64, 99]);
        Assert.True(DoomSoundDecoder.TryDecode(data, out var decoded, out var error), error);
        Assert.Equal(expected, decoded!.SampleRate);
        Assert.Equal(new short[] { -32768, 0, 32512, -16384 }, decoded.Samples);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3)]
    public void InvalidDeclaredLengthsFail(int count)
    {
        Assert.False(DoomSoundDecoder.TryDecode(Sound(11025, count, [1, 2]), out var decoded, out var error));
        Assert.Null(decoded); Assert.Equal("sound-dmx-length-invalid", error);
    }

    [Fact]
    public void PaddingIsRetainedAndTrailingBytesAreIgnored()
    {
        var bytes = Enumerable.Repeat((byte)128, 40).ToArray(); bytes[16] = 255;
        Assert.True(DoomSoundDecoder.TryDecode(Sound(11025, 39, bytes), out var decoded, out var error), error);
        Assert.Equal(39, decoded!.Samples.Length); Assert.Equal(32512, decoded.Samples[16]);
    }

    [Fact]
    public void HeaderOnlyAndUnsupportedFormatsFail()
    {
        Assert.False(DoomSoundDecoder.TryDecode(Sound(11025, 0, []), out _, out _));
        var bytes = Sound(11025, 1, [1]); bytes[0] = 2;
        Assert.False(DoomSoundDecoder.TryDecode(bytes, out _, out var error)); Assert.Equal("sound-format-unsupported", error);
    }

    [Fact]
    public void ReadsAliasedWadSoundThroughDecoder()
    {
        var wad = MapsModsTests.Wad(("DSPISTOL", Sound(11025, 2, [0, 255])));
        var level = new PlayLevel
        {
            SoundAliases = new Dictionary<string, string> { ["alias"] = "sound" },
            SoundDefinitions = new Dictionary<string, string> { ["sound"] = "DSPISTOL", ["missing"] = "ABSENT" },
        };
        Assert.True(SoundResourceResolver.TryReadDoomSound(wad, level, "alias", out var decoded, out var error), error);
        Assert.Equal(new short[] { -32768, 32512 }, decoded!.Samples);
        Assert.True(SoundResourceResolver.TryReadDoomSound(wad, level, "missing", out decoded, out error), error);
        Assert.Null(decoded);
    }

    private static byte[] Sound(ushort frequency, int count, byte[] samples)
    {
        var data = new byte[8 + samples.Length];
        BinaryPrimitives.WriteUInt16LittleEndian(data, 3);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(2), frequency);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), count);
        samples.CopyTo(data, 8);
        return data;
    }
}
