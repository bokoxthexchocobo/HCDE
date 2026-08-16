using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader;

public readonly struct MapSidedefRecord
{
    public const int RecordSize = 30;

    public MapSidedefRecord(
        short textureOffset,
        short rowOffset,
        string topTexture,
        string bottomTexture,
        string midTexture,
        short sector)
    {
        TextureOffset = textureOffset;
        RowOffset = rowOffset;
        TopTexture = topTexture;
        BottomTexture = bottomTexture;
        MidTexture = midTexture;
        Sector = sector;
    }

    public short TextureOffset { get; }
    public short RowOffset { get; }
    public string TopTexture { get; }
    public string BottomTexture { get; }
    public string MidTexture { get; }
    public short Sector { get; }
}

public readonly struct MapSubsectorRecord
{
    public const int RecordSize = 4;

    public MapSubsectorRecord(ushort numSegs, ushort firstSeg)
    {
        NumSegs = numSegs;
        FirstSeg = firstSeg;
    }

    public ushort NumSegs { get; }
    public ushort FirstSeg { get; }
}

public static class MapSidedefCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapSidedefRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapSidedefRecord>();
        rejectReason = null;
        if (lump.Length % MapSidedefRecord.RecordSize != 0)
        {
            rejectReason = "sidedefs-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapSidedefRecord.RecordSize;
        records = new MapSidedefRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapSidedefRecord(
                BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 2)..]),
                ReadTextureName(lump[(cursor + 4)..(cursor + 12)]),
                ReadTextureName(lump[(cursor + 12)..(cursor + 20)]),
                ReadTextureName(lump[(cursor + 20)..(cursor + 28)]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 28)..]));
            cursor += MapSidedefRecord.RecordSize;
        }

        return true;
    }

    private static string ReadTextureName(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        if (end < 0)
            end = bytes.Length;

        return Encoding.ASCII.GetString(bytes[..end]).TrimEnd();
    }
}

public static class MapSubsectorCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapSubsectorRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapSubsectorRecord>();
        rejectReason = null;
        if (lump.Length % MapSubsectorRecord.RecordSize != 0)
        {
            rejectReason = "ssectors-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapSubsectorRecord.RecordSize;
        records = new MapSubsectorRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapSubsectorRecord(
                BinaryPrimitives.ReadUInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 2)..]));
            cursor += MapSubsectorRecord.RecordSize;
        }

        return true;
    }
}
