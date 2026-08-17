using System.Buffers.Binary;

namespace HCDE.MapLoader;

public enum MapBehaviorFormat : byte
{
    Unknown = 0,
    AcsOld = 1,
    AcsEnhanced = 2,
    AcsLittleEnhanced = 3,
}

public readonly struct MapBehaviorRecord
{
    public const int MinLumpSize = 32;

    public MapBehaviorRecord(MapBehaviorFormat format, uint directoryOffset, byte[] data)
    {
        Format = format;
        DirectoryOffset = directoryOffset;
        Data = data;
    }

    public MapBehaviorFormat Format { get; }
    public uint DirectoryOffset { get; }
    public byte[] Data { get; }
}

public static class MapBehaviorCodec
{
    public static bool TryProbe(ReadOnlySpan<byte> lump, out MapBehaviorRecord record, out string? rejectReason)
    {
        record = default;
        rejectReason = null;
        if (lump.Length < MapBehaviorRecord.MinLumpSize)
        {
            rejectReason = "behavior-lump-too-small";
            return false;
        }

        if (lump[0] != (byte)'A' || lump[1] != (byte)'C' || lump[2] != (byte)'S')
        {
            rejectReason = "behavior-magic-mismatch";
            return false;
        }

        var format = lump[3] switch
        {
            0 => MapBehaviorFormat.AcsOld,
            (byte)'E' => MapBehaviorFormat.AcsEnhanced,
            (byte)'e' => MapBehaviorFormat.AcsLittleEnhanced,
            _ => MapBehaviorFormat.Unknown,
        };

        if (format == MapBehaviorFormat.Unknown)
        {
            rejectReason = "behavior-format-unknown";
            return false;
        }

        var directoryOffset = BinaryPrimitives.ReadUInt32LittleEndian(lump[4..]);
        record = new MapBehaviorRecord(format, directoryOffset, lump.ToArray());
        return true;
    }
}
