using HCDE.MapLoader;

namespace HCDE.Net.Core;

public static class MapLoadBootstrap
{
    public static bool TrySeedGuestWorldState(
        ReadOnlySpan<byte> wad,
        string mapName,
        GuestWorldStateStore store,
        out string? rejectReason,
        BinaryThingFlagFormat thingFlagFormat = BinaryThingFlagFormat.Doom)
    {
        rejectReason = null;
        if (!LevelBuilder.TryFromWad(wad, mapName, out var map, out rejectReason, thingFlagFormat))
            return false;
        if (map.Sectors.Count > ushort.MaxValue + 1)
        { rejectReason = "map-sector-network-index-limit"; return false; }
        for (var index = 0; index < map.Sectors.Count; index++)
        {
            var sector = map.Sectors[index];
            store.SeedMapSector((ushort)index, sector.FloorHeight, sector.CeilingHeight, sector.LightLevel, sector.Special);
        }
        foreach (var thing in map.Things)
        {
            if (thing.Type is < 1 or > 4) continue;
            store.SeedPlayer((byte)(thing.Type - 1));
            store.SeedActor((uint)thing.Type, classId: (ushort)thing.Type);
        }
        return true;
    }
}
