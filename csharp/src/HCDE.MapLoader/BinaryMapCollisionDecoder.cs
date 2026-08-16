namespace HCDE.MapLoader;

public readonly struct BinaryMapCollision
{
    public BinaryMapCollision(MapBlockmapRecord blockmap, MapRejectMatrix reject)
    {
        Blockmap = blockmap;
        Reject = reject;
    }

    public MapBlockmapRecord Blockmap { get; }
    public MapRejectMatrix Reject { get; }
}

public static class BinaryMapCollisionDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        int sectorCount,
        out BinaryMapCollision collision,
        out string? rejectReason)
    {
        collision = default;
        rejectReason = null;
        if (catalog.Format != MapDataFormat.DoomBinary)
        {
            rejectReason = "map-not-binary";
            return false;
        }

        if (!catalog.TryGetLump(MapLumpKind.Blockmap, out var blockmapLump)
            || !catalog.TryGetLump(MapLumpKind.Reject, out var rejectLump))
        {
            rejectReason = "map-missing-collision-lumps";
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, blockmapLump.Entry, out var blockmapData, out rejectReason)
            || !MapBlockmapCodec.TryRead(blockmapData, out var blockmap, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, rejectLump.Entry, out var rejectData, out rejectReason)
            || !MapRejectCodec.TryReadForSectorCount(rejectData, sectorCount, out var reject, out rejectReason))
        {
            return false;
        }

        collision = new BinaryMapCollision(blockmap, reject);
        return true;
    }
}
