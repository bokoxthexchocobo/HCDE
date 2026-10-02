using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>GetLineHealth</c> / <c>GetSectorHealth</c> with pooled health groups.</summary>
internal static class AcsDestructibleHealth
{
    public const int SectorPartFloor = 0;
    public const int SectorPartCeiling = 1;
    public const int SectorPart3D = 2;

    public static int GetLineHealth(AuthoritySimulation sim, int lineId)
    {
        var line = AcsLineActivation.FirstLineFromId(sim, lineId);
        if (line is null)
            return 0;
        if (line.HealthGroup > 0 && sim.HealthGroups.TryGetValue(line.HealthGroup, out var pooled))
            return pooled;
        return line.Health;
    }

    public static int GetSectorHealth(AuthoritySimulation sim, int tag, int part)
    {
        var sector = FirstSectorFromTag(sim, tag);
        if (sector is null)
            return 0;

        return part switch
        {
            SectorPartCeiling => ResolveSectorPart(sector.HealthCeilingGroup, sector.HealthCeiling, sim),
            SectorPartFloor => ResolveSectorPart(sector.HealthFloorGroup, sector.HealthFloor, sim),
            SectorPart3D => ResolveSectorPart(sector.Health3DGroup, sector.Health3D, sim),
            _ => 0,
        };
    }

    private static int ResolveSectorPart(int groupId, int localHealth, AuthoritySimulation sim)
    {
        if (groupId > 0 && sim.HealthGroups.TryGetValue(groupId, out var pooled))
            return pooled;
        return localHealth;
    }

    private static LevelSector? FirstSectorFromTag(AuthoritySimulation sim, int tag)
    {
        foreach (var sector in sim.Level.Sectors)
        {
            if (sector.MatchesTag(tag))
                return sector;
        }

        return null;
    }
}
