using System.Buffers.Binary;

namespace HCDE.MapLoader;

public readonly struct MapVertexRecord
{
    public const int RecordSize = 4;

    public MapVertexRecord(short x, short y)
    {
        X = x;
        Y = y;
    }

    public short X { get; }
    public short Y { get; }
}

public readonly struct MapSegRecord
{
    public const int RecordSize = 12;

    public MapSegRecord(ushort v1, ushort v2, short angle, ushort linedef, short side, short offset)
    {
        V1 = v1;
        V2 = v2;
        Angle = angle;
        Linedef = linedef;
        Side = side;
        Offset = offset;
    }

    public ushort V1 { get; }
    public ushort V2 { get; }
    public short Angle { get; }
    public ushort Linedef { get; }
    public short Side { get; }
    public short Offset { get; }
}

public readonly struct MapNodeRecord
{
    public const int RecordSize = 28;
    public const ushort SubsectorFlag = 0x8000;

    public MapNodeRecord(
        short x,
        short y,
        short deltaX,
        short deltaY,
        ReadOnlySpan<short> boundingBox,
        ushort childA,
        ushort childB)
    {
        X = x;
        Y = y;
        DeltaX = deltaX;
        DeltaY = deltaY;
        BoundingBox = boundingBox.Length == 8 ? boundingBox.ToArray() : Array.Empty<short>();
        ChildA = childA;
        ChildB = childB;
    }

    public short X { get; }
    public short Y { get; }
    public short DeltaX { get; }
    public short DeltaY { get; }
    public short[] BoundingBox { get; }
    public ushort ChildA { get; }
    public ushort ChildB { get; }

    public bool IsChildASubsector => (ChildA & SubsectorFlag) != 0;
    public bool IsChildBSubsector => (ChildB & SubsectorFlag) != 0;
    public ushort ChildAIndex => (ushort)(ChildA & ~SubsectorFlag);
    public ushort ChildBIndex => (ushort)(ChildB & ~SubsectorFlag);
}

public static class MapVertexCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapVertexRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapVertexRecord>();
        rejectReason = null;
        if (lump.Length % MapVertexRecord.RecordSize != 0)
        {
            rejectReason = "vertexes-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapVertexRecord.RecordSize;
        records = new MapVertexRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapVertexRecord(
                BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 2)..]));
            cursor += MapVertexRecord.RecordSize;
        }

        return true;
    }
}

public static class MapSegCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapSegRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapSegRecord>();
        rejectReason = null;
        if (lump.Length % MapSegRecord.RecordSize != 0)
        {
            rejectReason = "segs-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapSegRecord.RecordSize;
        records = new MapSegRecord[count];
        var cursor = 0;
        for (var i = 0; i < count; i++)
        {
            records[i] = new MapSegRecord(
                BinaryPrimitives.ReadUInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 2)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 4)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 6)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 8)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 10)..]));
            cursor += MapSegRecord.RecordSize;
        }

        return true;
    }
}

public static class MapNodeCodec
{
    public static bool TryReadAll(ReadOnlySpan<byte> lump, out MapNodeRecord[] records, out string? rejectReason)
    {
        records = Array.Empty<MapNodeRecord>();
        rejectReason = null;
        if (lump.Length % MapNodeRecord.RecordSize != 0)
        {
            rejectReason = "nodes-lump-size-mismatch";
            return false;
        }

        var count = lump.Length / MapNodeRecord.RecordSize;
        records = new MapNodeRecord[count];
        var cursor = 0;
        Span<short> bbox = stackalloc short[8];
        for (var i = 0; i < count; i++)
        {
            var bboxBytes = lump.Slice(cursor + 8, 16);
            for (var boxIndex = 0; boxIndex < 8; boxIndex++)
                bbox[boxIndex] = BinaryPrimitives.ReadInt16LittleEndian(bboxBytes[(boxIndex * 2)..]);

            records[i] = new MapNodeRecord(
                BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 2)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 4)..]),
                BinaryPrimitives.ReadInt16LittleEndian(lump[(cursor + 6)..]),
                bbox,
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 24)..]),
                BinaryPrimitives.ReadUInt16LittleEndian(lump[(cursor + 26)..]));
            cursor += MapNodeRecord.RecordSize;
        }

        return true;
    }
}
