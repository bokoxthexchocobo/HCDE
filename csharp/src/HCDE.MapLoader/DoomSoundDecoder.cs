using System.Buffers.Binary;

namespace HCDE.MapLoader;

public sealed record DoomSound(int SampleRate, short[] Samples);

/// <summary>Decodes the represented mono unsigned 8-bit DMX sound format.</summary>
public static class DoomSoundDecoder
{
    public static bool TryDecode(ReadOnlySpan<byte> data, out DoomSound? sound, out string? error)
    {
        sound = null;
        error = null;
        if (data.Length <= 8 || BinaryPrimitives.ReadUInt16LittleEndian(data) != 3)
        { error = "sound-format-unsupported"; return false; }
        var count = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        if (count < 0 || count > data.Length - 8)
        { error = "sound-dmx-length-invalid"; return false; }
        var frequency = BinaryPrimitives.ReadUInt16LittleEndian(data[2..]);
        var samples = new short[count];
        for (var index = 0; index < count; index++) samples[index] = (short)((data[index + 8] - 128) * 256);
        sound = new DoomSound(frequency == 0 ? 11025 : frequency, samples);
        return true;
    }
}
