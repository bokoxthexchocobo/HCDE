using HCDE.MapLoader;

namespace HCDE.Net.Core;

public static class MapLoadBootstrap
{
    public static bool TrySeedGuestWorldState(
        ReadOnlySpan<byte> wad,
        string mapName,
        GuestWorldStateStore store,
        out string? rejectReason)
    {
        rejectReason = null;
        if (!MapLumpCatalogReader.TryReadMap(wad, mapName, out var catalog, out rejectReason))
            return false;

        if (!MapSectorBootstrap.TryReadSectors(wad, catalog, out var sectors, out rejectReason))
            return false;

        GuestWorldStateBootstrap.SeedFromMapSectors(store, sectors);
        return true;
    }
}
