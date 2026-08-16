namespace HCDE.MapLoader;

public readonly struct BinaryMap
{
    public BinaryMap(
        BinaryMapRecords core,
        BinaryMapGeometry geometry,
        BinaryMapSurface surface,
        BinaryMapCollision collision)
    {
        Core = core;
        Geometry = geometry;
        Surface = surface;
        Collision = collision;
    }

    public BinaryMapRecords Core { get; }
    public BinaryMapGeometry Geometry { get; }
    public BinaryMapSurface Surface { get; }
    public BinaryMapCollision Collision { get; }
}

public static class BinaryMapDecoder
{
    public static bool TryReadMap(
        ReadOnlySpan<byte> wad,
        string mapName,
        out BinaryMap map,
        out MapLumpCatalog catalog,
        out string? rejectReason)
    {
        map = default;
        catalog = default;
        rejectReason = null;
        if (!MapLumpCatalogReader.TryReadMap(wad, mapName, out catalog, out rejectReason))
            return false;

        return TryDecode(wad, catalog, out map, out rejectReason);
    }

    public static bool TryDecode(
        ReadOnlySpan<byte> wad,
        MapLumpCatalog catalog,
        out BinaryMap map,
        out string? rejectReason)
    {
        map = default;
        rejectReason = null;
        if (!BinaryMapLumpDecoder.TryDecode(wad, catalog, out var core, out rejectReason))
            return false;

        if (!BinaryMapGeometryDecoder.TryDecode(wad, catalog, out var geometry, out rejectReason)
            || !BinaryMapSurfaceDecoder.TryDecode(wad, catalog, out var surface, out rejectReason)
            || !BinaryMapCollisionDecoder.TryDecode(wad, catalog, core.Sectors.Length, out var collision, out rejectReason))
        {
            return false;
        }

        map = new BinaryMap(core, geometry, surface, collision);
        return true;
    }
}
