using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Native <c>LS_Plat_*</c> and <c>Generic_Lift</c> plat paths from <c>p_lnspec.cpp</c>.</summary>
internal static class PlatLineSpecials
{
    internal static bool Execute(AuthoritySimulation sim, int special, int tag, int speed, int delay, int arg3, int arg4, LevelLine? line)
    {
        if (tag == 0 && line is null) return false;
        var platSpeed = speed / 8.0;
        if (platSpeed <= 0 && special is not 61 and not 231) return false;
        var wait = Tics(delay);
        return special switch
        {
            61 => Stop(sim, tag, arg3),
            60 or 207 => StartLift(sim, line, tag, platSpeed, wait, lip: special == 207 ? arg3 : 8, loop: true),
            62 or 206 => StartLift(sim, line, tag, platSpeed, wait, lip: special == 206 ? arg3 : 8, stone: special == 206 && arg4 != 0),
            63 => StartLift(sim, line, tag, platSpeed, wait, absolute: sim.Floors[FirstSector(sim, line, tag)] - arg3 * 8.0),
            64 => StartLift(sim, line, tag, platSpeed, wait, upFirst: true),
            65 or 230 => StartLift(sim, line, tag, platSpeed, 0, absolute: sim.Floors[FirstSector(sim, line, tag)] + arg3 * 8.0, stay: special == 230),
            172 => StartLift(sim, line, tag, platSpeed, wait, nearestUp: true),
            228 => StartLift(sim, line, tag, platSpeed, 0, raiseAndStay: arg3),
            231 => Toggle(sim, tag, line),
            _ => false,
        };
    }

    private static int Tics(int delay) => delay * 35 / 8;

    private static int FirstSector(AuthoritySimulation sim, LevelLine? line, int tag)
    {
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
            return sector;
        return 0;
    }

    private static bool Stop(AuthoritySimulation sim, int tag, int mode)
    {
        var remove = mode switch { 1 => false, 2 => true, _ => sim.Level.Format != MapDataFormat.DoomBinary };
        var targets = LineSpecials.TargetSectors(sim, null, tag).ToHashSet();
        if (remove)
            sim.Motions.RemoveAll(motion => motion.Kind == MotionKind.Lift && targets.Contains(motion.SectorIndex));
        else
            foreach (var motion in sim.Motions.Where(motion => motion.Kind == MotionKind.Lift && targets.Contains(motion.SectorIndex)))
                motion.Paused = true;
        return true;
    }

    private static bool Toggle(AuthoritySimulation sim, int tag, LevelLine? line)
    {
        var sector = FirstSector(sim, line, tag);
        var motion = sim.Motions.FirstOrDefault(m => m.SectorIndex == sector && m.Kind == MotionKind.Lift && !m.Completed);
        if (motion is null) return StartLift(sim, line, tag, 2, 105, lip: 8);
        motion.Closing = !motion.Closing;
        motion.Wait = 0;
        return true;
    }

    private static bool StartLift(AuthoritySimulation sim, LevelLine? line, int tag, double speed, int wait, int lip = 8,
        bool loop = false, double? absolute = null, bool upFirst = false, bool nearestUp = false, bool stay = false,
        int raiseAndStay = 0, bool stone = false)
    {
        var started = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && motion.Kind == MotionKind.Lift && !motion.Completed))
                continue;
            var current = sim.Floors[sector];
            var neighbors = LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Floors[index]).ToArray();
            double destination = absolute ?? (nearestUp
                ? neighbors.Where(height => height > current).DefaultIfEmpty(current).Min()
                : Math.Min(current, neighbors.DefaultIfEmpty(current).Min() + lip));
            if (stay && absolute is { } stayHeight)
                destination = stayHeight;
            if (raiseAndStay != 0)
            {
                destination = neighbors.DefaultIfEmpty(current).Min();
                if (raiseAndStay == 2) { /* lockout: leave plat up */ }
            }
            if (destination == current && !loop && !stay) continue;
            var closing = upFirst;
            sim.Motions.Add(new SectorMotion
            {
                SectorIndex = sector,
                Tag = tag,
                ClosedFloor = current,
                TargetFloor = destination,
                Kind = MotionKind.Lift,
                CloseAfterOpen = !stay && raiseAndStay == 0,
                Closing = closing,
                Speed = speed,
                Delay = wait,
                Loop = loop,
                FloorDirection = destination < current ? -1 : 1,
            });
            started = true;
        }
        return started;
    }
}
