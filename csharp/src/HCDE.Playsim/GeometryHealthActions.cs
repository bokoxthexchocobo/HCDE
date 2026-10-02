namespace HCDE.Playsim;

internal static class GeometryHealthActions
{
    public const int LineSetHealth = 150;
    public const int SectorSetHealth = 151;

    public static bool? Execute(AuthoritySimulation sim, int special, int id, int arg1, int arg2)
    {
        if (special == LineSetHealth)
        {
            var health = Math.Max(0, arg1);
            foreach (var line in AcsLineActivation.LinesFromId(sim, id))
            {
                line.Health = health;
                if (line.HealthGroup != 0) LevelHealthGroups.SetHealth(sim, line.HealthGroup, health);
            }
        }
        else if (special == SectorSetHealth)
        {
            var health = Math.Max(0, arg2);
            foreach (var sector in sim.Level.Sectors)
            {
                if (!sector.MatchesTag(id)) continue;
                int group;
                switch (arg1)
                {
                    case AcsDestructibleHealth.SectorPartFloor:
                        sector.HealthFloor = health; group = sector.HealthFloorGroup; break;
                    case AcsDestructibleHealth.SectorPartCeiling:
                        sector.HealthCeiling = health; group = sector.HealthCeilingGroup; break;
                    case AcsDestructibleHealth.SectorPart3D:
                        sector.Health3D = health; group = sector.Health3DGroup; break;
                    default: continue;
                }
                if (group != 0) LevelHealthGroups.SetHealth(sim, group, health);
            }
        }
        else return null;
        return true;
    }
}
