using HCDE.MapLoader;

namespace HCDE.Net.Core;

public static class GuestWorldStateBootstrap
{
    public static int SeedFromMapSectors(GuestWorldStateStore store, MapSectorRecord[] sectors)
    {
        if (sectors.Length > ushort.MaxValue + 1) throw new ArgumentOutOfRangeException(nameof(sectors));
        for (var i = 0; i < sectors.Length; i++)
        {
            var sector = sectors[i];
            store.SeedMapSector(
                (ushort)i,
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
            store.SeedActor((uint)thing.Type, classId: (ushort)thing.Type);
            count++;
        }

        return count;
    }
}
