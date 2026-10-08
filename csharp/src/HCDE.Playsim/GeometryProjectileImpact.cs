using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class GeometryProjectileImpact
{
    public static void ApplyPlane(AuthoritySimulation sim, ProjectileActor missile, int sectorIndex, int part)
    {
        if ((uint)sectorIndex >= (uint)sim.Level.Sectors.Count || part is not (0 or 1)) return;
        var sector = sim.Level.Sectors[sectorIndex];
        var health = part == 0 ? sector.HealthFloor : sector.HealthCeiling;
        var texture = part == 0 ? sector.FloorPic : sector.CeilingPic;
        if (health <= 0 || string.Equals(texture, "F_SKY1", StringComparison.OrdinalIgnoreCase)) return;
        var damage = missile.GetMissileDamage(missile.StrifeDamage ? 3 : 7, 1);
        LevelDestructibleDamage.DamageSectorPart(sim, sector, part, damage);
    }

    public static void Apply(AuthoritySimulation sim, ProjectileActor missile, LevelLine line)
    {
        var side = (line.X2 - line.X1) * (missile.Y.ToDouble() - line.Y1)
            - (line.Y2 - line.Y1) * (missile.X.ToDouble() - line.X1) > 0 ? 1 : 0;
        var impactSide = side == 0 ? line.SideFront : line.SideBack;
        var otherSide = side == 0 ? line.SideBack : line.SideFront;
        if ((uint)impactSide >= (uint)sim.Level.Sides.Count || line.Special == 9) return;
        int Damage() => missile.GetMissileDamage(missile.StrifeDamage ? 3 : 7, 1);
        if ((uint)otherSide < (uint)sim.Level.Sides.Count)
        {
            var sectorIndex = sim.Level.Sides[otherSide].Sector;
            if ((uint)sectorIndex < (uint)sim.Level.Sectors.Count)
            {
                var sector = sim.Level.Sectors[sectorIndex];
                // Native cylinder impacts can reach both lower and upper wall parts.
                if (missile.Z.ToDouble() < sim.Floors[sectorIndex] + 1 / 65536.0 && sector.HealthFloor > 0)
                    LevelDestructibleDamage.DamageSectorPart(sim, sector, 0, Damage());
                if (missile.Z.ToDouble() + missile.Height.ToDouble() > sim.Ceilings[sectorIndex] - 1 / 65536.0 && sector.HealthCeiling > 0)
                    LevelDestructibleDamage.DamageSectorPart(sim, sector, 1, Damage());
            }
        }
        if (line.Health > 0) LevelDestructibleDamage.DamageLine(sim, line, Damage());
    }
}
