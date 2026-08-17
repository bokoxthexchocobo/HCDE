using HCDE.MapLoader;

namespace HCDE.Net.Core;

public static class GuestWorldStateBootstrap
{
    public static int SeedFromMapSectors(GuestWorldStateStore store, MapSectorRecord[] sectors)
    {
        for (ushort i = 0; i < sectors.Length; i++)
        {
            var sector = sectors[i];
            store.SeedMapSector(
                i,
                sector.FloorHeight,
                sector.CeilingHeight,
                sector.LightLevel,
                sector.Special);
        }

        return sectors.Length;
    }

    public static int SeedPlayersFromMapThings(GuestWorldStateStore store, MapThingRecord[] things)
    {
        var count = 0;
        foreach (var thing in things)
        {
            if (thing.Type is < 1 or > 4)
                continue;

            store.SeedPlayer((byte)(thing.Type - 1));
            count++;
        }

        return count;
    }
}
