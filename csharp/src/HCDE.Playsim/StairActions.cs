using HCDE.MapLoader;

namespace HCDE.Playsim;

internal static class StairActions
{
    internal static bool Start(AuthoritySimulation sim, LevelLine? trigger, int tag, double speed, int step,
        bool down = false, bool crush = false, bool sync = false, int reset = 0, int delay = 0, bool useSpecials = false, bool ignoreTexture = false)
    {
        if (speed <= 0 || step <= 0 || reset < 0 || delay < 0) return false;
        var period = step / speed;
        if (period > int.MaxValue) return false;
        var pending = new List<SectorMotion>();
        bool Busy(int index) => sim.Motions.Concat(pending).Any(motion => motion.SectorIndex == index && !motion.MovesCeiling);
        foreach (var seed in LineSpecials.TargetSectors(sim, trigger, tag, backSide: true))
        {
            if (Busy(seed)) continue;
            var highestGroup = sim.Motions.Concat(pending).Where(motion => motion.Kind == MotionKind.Stair)
                .Select(motion => motion.StairGroup).DefaultIfEmpty(0).Max();
            if (highestGroup == int.MaxValue) return false;
            var group = highestGroup + 1;
            var sector = seed;
            var height = sim.Floors[seed];
            var texture = sim.Level.Sectors[seed].FloorPic;
            var visited = new HashSet<int>();
            while (visited.Add(sector))
            {
                height += down ? -step : step;
                if (!useSpecials || !Busy(sector))
                {
                    var motionSpeed = sync && sector != seed ? speed * (height - sim.Floors[sector]) / (down ? -step : step) : speed;
                    // Native synchronized stairs retain the signed rise/step speed.
                    // Movement and reset use the separately assigned direction.
                    pending.Add(new SectorMotion {
                        Kind = MotionKind.Stair, SectorIndex = sector, StairGroup = group, StairSeedIndex = seed,
                        ClosedFloor = sim.Floors[sector], TargetFloor = Math.Max(height, short.MinValue), FloorDirection = down ? -1 : 1,
                        Speed = motionSpeed, CrushDamage = sector == seed ? (crush ? 10 : 0) : !useSpecials && speed == 4 ? 10 : 0,
                        StopOnCrush = sector == seed,
                        ResetCountdown = reset,
                        Delay = delay, StepPeriod = (int)period, StepCountdown = (int)period,
                    });
                }
                var next = -1;
                if (useSpecials)
                {
                    var required = sim.Level.Sectors[sector].Special == 26 ? 27 : 26;
                    foreach (var neighbor in LineSpecials.NeighborSectors(sim, sector))
                    {
                        if (visited.Contains(neighbor) || sim.Level.Sectors[neighbor].Special != required) continue;
                        next = neighbor;
                        break;
                    }
                    if (next < 0) break;
                    sector = next;
                    continue;
                }
                // Native Doom traversal uses front-to-back sidedness and original linedef order.
                foreach (var line in sim.Level.Lines)
                {
                    if ((line.Flags & LevelLine.TwoSidedFlag) == 0) continue;
                    if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count
                        || sim.Level.Sides[line.SideFront].Sector != sector) continue;
                    var back = sim.Level.Sides[line.SideBack].Sector;
                    if ((uint)back >= (uint)sim.Level.Sectors.Count || visited.Contains(back) || Busy(back)
                        || !ignoreTexture && !string.Equals(texture, sim.Level.Sectors[back].FloorPic, StringComparison.OrdinalIgnoreCase)) continue;
                    next = back;
                    break;
                }
                if (next < 0) break;
                sector = next;
            }
        }
        sim.Motions.AddRange(pending);
        return pending.Count != 0;
    }

}
