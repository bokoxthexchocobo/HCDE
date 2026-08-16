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
}
