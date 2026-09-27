using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class StairActions
{
    internal static bool Start(AuthoritySimulation sim, LevelLine trigger, int tag, double speed, int step,
        bool down = false, bool crush = false)
    {
        if (speed <= 0 || step <= 0) return false;
        var started = false;
        foreach (var seed in LineSpecials.TargetSectors(sim, trigger, tag, backSide: true))
        {
            if (Busy(sim, seed)) continue;
            var sector = seed;
            var height = sim.Floors[seed];
            var texture = sim.Level.Sectors[seed].FloorPic;
            var visited = new HashSet<int>();
            while (visited.Add(sector))
            {
                height += down ? -step : step;
                sim.Motions.Add(new SectorMotion {
                    Kind = MotionKind.Stair, SectorIndex = sector, StairGroup = seed + 1,
                    ClosedFloor = sim.Floors[sector], TargetFloor = Math.Clamp(height, short.MinValue, sim.Ceilings[sector]),
                    Speed = speed, CrushDamage = sector == seed ? (crush ? 10 : 0) : speed == 4 ? 10 : 0,
                    StopOnCrush = sector == seed,
                });
                started = true;
                var next = -1;
                // Native Doom traversal uses front-to-back sidedness and original linedef order.
                foreach (var line in sim.Level.Lines)
                {
                    if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count
                        || sim.Level.Sides[line.SideFront].Sector != sector) continue;
                    var back = sim.Level.Sides[line.SideBack].Sector;
                    if ((uint)back >= (uint)sim.Level.Sectors.Count || visited.Contains(back) || Busy(sim, back)
                        || !string.Equals(texture, sim.Level.Sectors[back].FloorPic, StringComparison.OrdinalIgnoreCase)) continue;
                    next = back;
                    break;
                }
                if (next < 0) break;
                sector = next;
            }
        }
        return started;
    }

    private static bool Busy(AuthoritySimulation sim, int sector) =>
        sim.Motions.Any(motion => motion.SectorIndex == sector && !motion.MovesCeiling);
}
