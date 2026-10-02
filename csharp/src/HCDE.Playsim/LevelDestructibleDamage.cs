using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>P_DamageLinedef</c> / <c>P_DamageSector</c> health sync (specials and events absent).</summary>
internal static class LevelDestructibleDamage
{
    public static void DamageLine(AuthoritySimulation sim, LevelLine line, int damage)
    {
        if (damage <= 0)
            return;

        line.Health = Math.Max(0, line.Health - damage);
        if (line.HealthGroup <= 0)
            return;

        LevelHealthGroups.SetHealth(sim, line.HealthGroup, line.Health);
    }

    public static void DamageSectorPart(AuthoritySimulation sim, LevelSector sector, int part, int damage)
    {
        if (damage <= 0)
            return;

        var (health, groupId) = ReadPart(sector, part);
        health = Math.Max(0, health - damage);
        WritePart(sector, part, health);
        if (groupId <= 0)
            return;

        LevelHealthGroups.SetHealth(sim, groupId, health);
    }

    private static (int Health, int GroupId) ReadPart(LevelSector sector, int part) => part switch
    {
        AcsDestructibleHealth.SectorPartCeiling => (sector.HealthCeiling, sector.HealthCeilingGroup),
        AcsDestructibleHealth.SectorPartFloor => (sector.HealthFloor, sector.HealthFloorGroup),
        AcsDestructibleHealth.SectorPart3D => (sector.Health3D, sector.Health3DGroup),
        _ => (0, 0),
    };

    private static void WritePart(LevelSector sector, int part, int health)
    {
        switch (part)
        {
            case AcsDestructibleHealth.SectorPartCeiling:
                sector.HealthCeiling = health;
                break;
            case AcsDestructibleHealth.SectorPartFloor:
                sector.HealthFloor = health;
                break;
            case AcsDestructibleHealth.SectorPart3D:
                sector.Health3D = health;
                break;
        }
    }
}
