using System.Buffers.Binary;

namespace HCDE.MapLoader;

public readonly struct MapBlockmapHeader
{
    public const int Size = 8;

    public MapBlockmapHeader(short originX, short originY, ushort width, ushort height)
    {
        OriginX = originX;
        OriginY = originY;
        Width = width;
        Height = height;
    }

    public short OriginX { get; }
    public short OriginY { get; }
    public ushort Width { get; }
    public ushort Height { get; }
}

public readonly struct MapBlockmapRecord
{
    public MapBlockmapRecord(MapBlockmapHeader header, int[] cells)
    {
        Header = header;
        Cells = cells;
    }

    public MapBlockmapHeader Header { get; }
    public int[] Cells { get; }
}

public readonly struct MapRejectMatrix
{
    public MapRejectMatrix(byte[] bytes, int sectorCount)
    {
        Bytes = bytes;
        SectorCount = sectorCount;
    }

    public byte[] Bytes { get; }
    public int SectorCount { get; }
}

public static class MapBlockmapCodec
{
    public const int Terminator = unchecked((int)0xFFFFFFFF);

    public static bool TryRead(ReadOnlySpan<byte> lump, out MapBlockmapRecord record, out string? rejectReason)
    {
        record = default;
        rejectReason = null;
        if (lump.Length < MapBlockmapHeader.Size)
        {
            rejectReason = "blockmap-lump-too-small";
            return false;
        }

        if (lump.Length % 2 != 0)
        {
            rejectReason = "blockmap-lump-size-mismatch";
            return false;
        }

        var header = new MapBlockmapHeader(
            BinaryPrimitives.ReadInt16LittleEndian(lump[..]),
            BinaryPrimitives.ReadInt16LittleEndian(lump[2..]),
            BinaryPrimitives.ReadUInt16LittleEndian(lump[4..]),
            BinaryPrimitives.ReadUInt16LittleEndian(lump[6..]));

        var shortCount = lump.Length / 2;
        var cells = new int[shortCount - 4];
        var cursor = MapBlockmapHeader.Size;
        for (var i = 0; i < cells.Length; i++)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(lump[cursor..]);
            cells[i] = value == -1 ? Terminator : value & 0xFFFF;
            cursor += 2;
        }

        record = new MapBlockmapRecord(header, cells);
        return true;
    }
}

public static class MapRejectCodec
{
    public static bool TryReadForSectorCount(
        ReadOnlySpan<byte> lump,
        int sectorCount,
        out MapRejectMatrix matrix,
        out string? rejectReason)
    {
        matrix = default;
        rejectReason = null;
        if (sectorCount <= 0)
        {
            rejectReason = "reject-sector-count-invalid";
            return false;
        }

        var expectedBytes = (sectorCount * sectorCount + 7) / 8;
        if (lump.Length != expectedBytes)
        {
            rejectReason = "reject-lump-size-mismatch";
            return false;
        }

        matrix = new MapRejectMatrix(lump.ToArray(), sectorCount);
        return true;
    }
}
