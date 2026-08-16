namespace HCDE.MapLoader;

public readonly struct BinaryMapSurface
{
    public BinaryMapSurface(MapSidedefRecord[] sidedefs, MapSubsectorRecord[] subsectors)
    {
        Sidedefs = sidedefs;
        Subsectors = subsectors;
    }

    public MapSidedefRecord[] Sidedefs { get; }
    public MapSubsectorRecord[] Subsectors { get; }
}

public static class BinaryMapSurfaceDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out BinaryMapSurface surface,
        out string? rejectReason)
    {
        surface = default;
        rejectReason = null;
        if (catalog.Format != MapDataFormat.DoomBinary)
        {
            rejectReason = "map-not-binary";
            return false;
        }

        if (!catalog.TryGetLump(MapLumpKind.Sidedefs, out var sidedefLump)
            || !catalog.TryGetLump(MapLumpKind.Ssectors, out var subsectorLump))
        {
            rejectReason = "map-missing-surface-lumps";
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, sidedefLump.Entry, out var sidedefData, out rejectReason)
            || !MapSidedefCodec.TryReadAll(sidedefData, out var sidedefs, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, subsectorLump.Entry, out var subsectorData, out rejectReason)
            || !MapSubsectorCodec.TryReadAll(subsectorData, out var subsectors, out rejectReason))
        {
            return false;
        }

        surface = new BinaryMapSurface(sidedefs, subsectors);
        return true;
    }
}
