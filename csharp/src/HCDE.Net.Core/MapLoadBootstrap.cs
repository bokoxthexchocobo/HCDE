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
        if (!BinaryMapDecoder.TryReadMap(wad, mapName, out var map, out _, out rejectReason))
            return false;

        GuestWorldStateBootstrap.SeedFromMapSectors(store, map.Core.Sectors);
        return true;
    }
}
