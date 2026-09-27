using HCDE.MapLoader;

namespace HCDE.Playsim;

internal enum CeilingTarget { Floor, Highest, RaiseBy, LowerBy, Crush }

internal static class CeilingActions
{
    internal static bool Start(AuthoritySimulation sim, LevelLine line, int tag, CeilingTarget kind,
        double speed = 1, double? upSpeed = null, int? amount = null, int crush = -1,
        bool returnToStart = false, bool loop = false, bool slow = false, bool stop = false)
    {
        if (speed <= 0 || upSpeed <= 0 || amount < 0) return false;
        var distance = amount ?? (kind == CeilingTarget.Crush ? 8 : 0);
        var started = false;
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            var existing = sim.Motions.FirstOrDefault(motion => motion.SectorIndex == sector && motion.MovesCeiling);
            if (existing != null)
            {
                if (loop && existing.Kind == MotionKind.Ceiling && existing.Paused)
                { existing.Paused = false; started = true; }
                continue;
            }
            var current = sim.Ceilings[sector];
            var target = kind switch
            {
                CeilingTarget.Floor => sim.Floors[sector] + distance,
                CeilingTarget.Highest => LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Ceilings[index]).DefaultIfEmpty(current).Max(),
                CeilingTarget.RaiseBy => current + distance,
                CeilingTarget.LowerBy => current - distance,
                _ => sim.Floors[sector] + distance,
            };
            target = Math.Clamp(target, sim.Floors[sector], short.MaxValue);
            if (target == current || kind == CeilingTarget.Crush && target > current) continue;
            sim.Motions.Add(new SectorMotion {
                SectorIndex = sector, Kind = MotionKind.Ceiling, Tag = tag, ClosedCeiling = current,
                TargetCeiling = target, Speed = speed, ReturnSpeed = upSpeed ?? speed,
                CloseAfterOpen = returnToStart, Loop = loop, CrushDamage = crush,
                SlowOnCrush = slow, StopOnCrush = stop,
            });
            started = true;
        }
        return started;
    }

    internal static bool Stop(AuthoritySimulation sim, LevelLine line, int tag, bool remove = false)
    {
        var targets = LineSpecials.TargetSectors(sim, line, tag, backSide: true).ToHashSet();
        var stopped = false;
        for (var i = sim.Motions.Count - 1; i >= 0; i--)
        {
            var motion = sim.Motions[i];
            if (motion.Kind != MotionKind.Ceiling || !targets.Contains(motion.SectorIndex)) continue;
            if (remove) { sim.Motions.RemoveAt(i); stopped = true; }
            else if (!motion.Paused) { motion.Paused = true; stopped = true; }
        }
        return stopped;
    }

    // Return true when the thinker has completed. Closing denotes the return leg.
    internal static bool Tick(AuthoritySimulation sim, SectorMotion motion)
    {
        if (motion.Paused) return false;
        var current = sim.Ceilings[motion.SectorIndex];
        var target = Math.Max(sim.Floors[motion.SectorIndex], motion.Closing ? motion.ClosedCeiling : motion.TargetCeiling);
        var speed = motion.Closing ? motion.ReturnSpeed : motion.Slowed ? 0.125 : motion.Speed;
        var next = current < target ? Math.Min(target, current + speed) : Math.Max(target, current - speed);
        if (next < current)
        {
            var blocked = sim.Actors.Where(actor => actor.BlocksActors && actor.SectorIndex == motion.SectorIndex
                && actor.Z.ToDouble() + actor.Height.ToDouble() > next).ToArray();
            if (blocked.Length > 0)
            {
                if (motion.CrushDamage >= 0 && (sim.Thinkers.Clock.Tic & 3) == 0)
                    foreach (var actor in blocked) ActorDamage.Apply(actor, motion.CrushDamage);
                // Native MoveCeiling completes a destination-clamped leg even when
                // obstruction rolls the plane back; an overshoot also forces rollback.
                if (motion.CrushDamage < 0 || motion.StopOnCrush
                    || next == target && speed > Math.Abs(target - current))
                    return next == target && FinishLeg(motion);
                if (motion.SlowOnCrush) motion.Slowed = true;
            }
        }
        sim.Ceilings[motion.SectorIndex] = Fixed.FromDouble(next).ToDouble();
        if (next != target) return false;
        return FinishLeg(motion);
    }

    private static bool FinishLeg(SectorMotion motion)
    {
        motion.Slowed = false;
        if (!motion.Closing && motion.CloseAfterOpen) { motion.Closing = true; return false; }
        if (motion.Closing && motion.Loop) { motion.Closing = false; return false; }
        return true;
    }
}
