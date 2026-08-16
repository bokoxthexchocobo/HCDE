namespace HCDE.MapLoader;

public static class MapSectorBootstrap
{
    public static int CountSectors(BinaryMapRecords records) => records.Sectors.Length;

    public static bool TryReadSectors(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out MapSectorRecord[] sectors,
        out string? rejectReason)
    {
        sectors = Array.Empty<MapSectorRecord>();
        rejectReason = null;
        if (!BinaryMapDecoder.TryDecode(wad, catalog, out var map, out rejectReason))
            return false;

        sectors = map.Core.Sectors;
        return true;
    }
}
