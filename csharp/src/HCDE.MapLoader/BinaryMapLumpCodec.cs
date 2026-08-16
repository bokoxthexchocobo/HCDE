using System.Buffers.Binary;

namespace HCDE.MapLoader;

public readonly struct MapThingRecord
{
    public const int RecordSize = 10;

    public MapThingRecord(short x, short y, short angle, short type, short options)
    {
        X = x;
        Y = y;
        Angle = angle;
        Type = type;
        Options = options;
    }

    public short X { get; }
    public short Y { get; }
    public short Angle { get; }
    public short Type { get; }
    public short Options { get; }
}

public readonly struct MapLinedefRecord
{
    public const int RecordSize = 14;

    public MapLinedefRecord(ushort v1, ushort v2, ushort flags, ushort special, short tag, ushort sideFront, ushort sideBack)
    {
        V1 = v1;
        V2 = v2;
        Flags = flags;
        Special = special;
        Tag = tag;
        SideFront = sideFront;
        SideBack = sideBack;
    }

    public ushort V1 { get; }
    public ushort V2 { get; }
    public ushort Flags { get; }
    public ushort Special { get; }
    public short Tag { get; }
    public ushort SideFront { get; }
    public ushort SideBack { get; }
}

public readonly struct MapSectorRecord
{
    public const int RecordSize = 26;

    public MapSectorRecord(
        short floorHeight,
        short ceilingHeight,
        string floorPic,
        string ceilingPic,
        short lightLevel,
        short special,
        short tag)
    {
        FloorHeight = floorHeight;
        CeilingHeight = ceilingHeight;
        FloorPic = floorPic;
        CeilingPic = ceilingPic;
        LightLevel = lightLevel;
        Special = special;
        Tag = tag;
    }

    public short FloorHeight { get; }
    public short CeilingHeight { get; }
    public string FloorPic { get; }
    public string CeilingPic { get; }
    public short LightLevel { get; }
    public short Special { get; }
    public short Tag { get; }
}

public static class MapThingCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapThingRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapThingRecord>();
        rejectReason = null;
        if (lump.Length % MapThingRecord.RecordSize != 0)
        {
            rejectReason = "things-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapThingRecord.RecordSize;
        records = new MapThingRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapThingRecord(
                BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 2)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 4)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 6)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 8)..]));
            cursor += MapThingRecord.RecordSize;
        }

        return true;
    }
}

public static class MapLinedefCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapLinedefRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapLinedefRecord>();
        rejectReason = null;
        if (lump.Length % MapLinedefRecord.RecordSize != 0)
        {
            rejectReason = "linedefs-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapLinedefRecord.RecordSize;
        records = new MapLinedefRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapLinedefRecord(
                BinaryPrimitives.ReadUInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 2)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 4)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 6)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 8)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 10)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 12)..]));
            cursor += MapLinedefRecord.RecordSize;
        }

        return true;
    }
}

public static class MapSectorCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapSectorRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapSectorRecord>();
        rejectReason = null;
        if (lump.Length % MapSectorRecord.RecordSize != 0)
        {
            rejectReason = "sectors-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapSectorRecord.RecordSize;
        records = new MapSectorRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            var floorHeight = BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]);
            var ceilingHeight = BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 2)..]);
            var floorPic = ReadPicName(lump[(cursor + 4)..(cursor + 12)]);
            var ceilingPic = ReadPicName(lump[(cursor + 12)..(cursor + 20)]);
            var lightLevel = BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 20)..]);
            var special = BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 22)..]);
            var tag = BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 24)..]);
            records[i] = new MapSectorRecord(floorHeight, ceilingHeight, floorPic, ceilingPic, lightLevel, special, tag);
            cursor += MapSectorRecord.RecordSize;
        }

        return true;
    }

    private static string ReadPicName(ReadOnlySpan<byte> bytes)
    {
        var end = bytes.IndexOf((byte)0);
        if (end < 0)
            end = bytes.Length;

        return System.Text.Encoding.ASCII.GetString(bytes[..end]).TrimEnd();
    }
}
