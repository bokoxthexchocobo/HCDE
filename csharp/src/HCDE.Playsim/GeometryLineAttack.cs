namespace HCDE.Playsim;

internal static class GeometryLineAttack
{
    public static void Apply(AuthoritySimulation sim, CombatTrace.LineAttackHit hit, int damage)
    {
        if (damage <= 0 || !hit.Hit) return;
        if ((uint)hit.PlaneSector < (uint)sim.Level.Sectors.Count && hit.PlanePart is 0 or 1)
        {
            var sector = sim.Level.Sectors[hit.PlaneSector];
            var health = hit.PlanePart == 0 ? sector.HealthFloor : sector.HealthCeiling;
            var texture = hit.PlanePart == 0 ? sector.FloorPic : sector.CeilingPic;
            if (health > 0 && !CombatTrace.IsSkyTexture(texture))
                LevelDestructibleDamage.DamageSectorPart(sim, sector, hit.PlanePart, damage);
            return;
        }
        if (hit.Wall is not { } line || line.Special == 9) return; // Line_Horizon.
        var side = hit.Side == 0 ? line.SideFront : line.SideBack;
        if ((uint)side >= (uint)sim.Level.Sides.Count) return;
        var otherSide = hit.Side == 0 ? line.SideBack : line.SideFront;
        if ((uint)otherSide < (uint)sim.Level.Sides.Count)
        {
            var sectorIndex = sim.Level.Sides[otherSide].Sector;
            if ((uint)sectorIndex < (uint)sim.Level.Sectors.Count)
            {
                var sector = sim.Level.Sectors[sectorIndex];
                if (hit.Z <= sim.Floors[sectorIndex] && sector.HealthFloor > 0)
                    LevelDestructibleDamage.DamageSectorPart(sim, sector, AcsDestructibleHealth.SectorPartFloor, damage);
                else if (hit.Z >= sim.Ceilings[sectorIndex] && sector.HealthCeiling > 0)
                    LevelDestructibleDamage.DamageSectorPart(sim, sector, AcsDestructibleHealth.SectorPartCeiling, damage);
            }
        }
        if (line.Health > 0) LevelDestructibleDamage.DamageLine(sim, line, damage);
    }
}
