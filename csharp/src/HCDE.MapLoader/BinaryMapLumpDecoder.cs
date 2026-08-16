namespace HCDE.MapLoader;

public readonly struct BinaryMapRecords
{
    public BinaryMapRecords(MapThingRecord[] things, MapLinedefRecord[] linedefs, MapSectorRecord[] sectors)
    {
        Things = things;
        Linedefs = linedefs;
        Sectors = sectors;
    }

    public MapThingRecord[] Things { get; }
    public MapLinedefRecord[] Linedefs { get; }
    public MapSectorRecord[] Sectors { get; }
}

public static class BinaryMapLumpDecoder
{
    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out BinaryMapRecords records,
        out string? rejectReason)
    {
        records = default;
        rejectReason = null;
        if (catalog.Format != MapDataFormat.DoomBinary)
        {
            rejectReason = "map-not-binary";
            return false;
        }

        if (!catalog.TryGetLump(MapLumpKind.Things, out var thingsLump)
            || !catalog.TryGetLump(MapLumpKind.Linedefs, out var linedefsLump)
            || !catalog.TryGetLump(MapLumpKind.Sectors, out var sectorsLump))
        {
            rejectReason = "map-missing-core-lumps";
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, thingsLump.Entry, out var thingsData, out rejectReason)
            || !MapThingCodec.TryReadAll(thingsData, out var things, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, linedefsLump.Entry, out var linedefsData, out rejectReason)
            || !MapLinedefCodec.TryReadAll(linedefsData, out var linedefs, out rejectReason))
        {
            return false;
        }

        if (!WadArchiveReader.TryReadLumpData(wad, sectorsLump.Entry, out var sectorsData, out rejectReason)
            || !MapSectorCodec.TryReadAll(sectorsData, out var sectors, out rejectReason))
        {
            return false;
        }

        records = new BinaryMapRecords(things, linedefs, sectors);
        return true;
    }
}
