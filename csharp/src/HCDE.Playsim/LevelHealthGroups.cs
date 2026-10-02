using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>P_InitHealthGroups</c> pooled health at map start.</summary>
internal static class LevelHealthGroups
{
    public static void SetHealth(AuthoritySimulation sim, int id, int health)
    {
        if (!sim.HealthGroups.ContainsKey(id)) return;
        sim.HealthGroups[id] = health;
        foreach (var line in sim.Level.Lines)
            if (line.HealthGroup == id) line.Health = health;
        foreach (var sector in sim.Level.Sectors)
        {
            // Native group sector membership is registered by floor/ceiling only.
            if (sector.HealthFloorGroup != id && sector.HealthCeilingGroup != id) continue;
            if (sector.HealthFloorGroup == id) sector.HealthFloor = health;
            if (sector.HealthCeilingGroup == id) sector.HealthCeiling = health;
            if (sector.Health3DGroup == id) sector.Health3D = health;
        }
    }

    public static Dictionary<int, int> Build(PlayLevel level)
    {
        var groups = new Dictionary<int, int>();
        foreach (var line in level.Lines)
        {
            if (line.HealthGroup > 0)
                Register(groups, line.HealthGroup, line.Health);
        }

        foreach (var sector in level.Sectors)
        {
            if (sector.HealthCeilingGroup > 0)
                Register(groups, sector.HealthCeilingGroup, sector.HealthCeiling);
            if (sector.HealthFloorGroup > 0)
                Register(groups, sector.HealthFloorGroup, sector.HealthFloor);
        }

        return groups;
    }

    private static void Register(Dictionary<int, int> groups, int id, int health)
    {
        if (!groups.TryGetValue(id, out var existing))
            groups[id] = health;
        else if (health > existing)
            groups[id] = health;
    }
}
