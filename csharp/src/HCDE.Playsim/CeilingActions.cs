using HCDE.MapLoader;

namespace HCDE.Playsim;

internal enum CeilingTarget { Floor, Highest, RaiseBy, LowerBy, Crush, NextHigher, NextLower, RaiseInstant, LowerInstant, Absolute, Lowest, RaiseLowest, LowerHighestFloor, RaiseHighestFloor, LowerHighest, RaiseFloor }

internal static class CeilingActions
{
    internal static bool Start(AuthoritySimulation sim, LevelLine? line, int tag, CeilingTarget kind,
        double speed = 1, double? upSpeed = null, int? amount = null, int crush = -1,
        bool returnToStart = false, bool loop = false, bool slow = false, bool stop = false)
    {
        var distance = amount ?? (kind == CeilingTarget.Crush ? 8 : 0);
        var instant = kind is CeilingTarget.RaiseInstant or CeilingTarget.LowerInstant;
        if (instant) speed = distance;
        if (speed < 0 || upSpeed < 0
            || amount < 0 && kind is not (CeilingTarget.Absolute or CeilingTarget.Floor or CeilingTarget.RaiseFloor or CeilingTarget.LowerHighestFloor or CeilingTarget.Crush)) return false;
        var started = false;
        if (tag != 0 && loop) started = Resume(sim, tag);
        foreach (var sector in LineSpecials.TargetSectors(sim, line, tag, backSide: true))
        {
            var creationTag = tag == 0 ? sector | 0x1000000 : tag;
            // Native manual activation resumes by its synthetic tag but returns only creation success.
            if (tag == 0) Resume(sim, creationTag);
            var existing = sim.Motions.FirstOrDefault(motion => motion.SectorIndex == sector && motion.MovesCeiling);
            if (existing != null)
            {
                continue;
            }
            var current = sim.Ceilings[sector];
            var target = kind switch
            {
                CeilingTarget.Floor or CeilingTarget.RaiseFloor => sim.Floors[sector] + distance,
                CeilingTarget.Absolute => distance,
                CeilingTarget.Highest or CeilingTarget.LowerHighest => LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Ceilings[index]).DefaultIfEmpty(current).Max(),
                CeilingTarget.Lowest or CeilingTarget.RaiseLowest => LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Ceilings[index]).DefaultIfEmpty(current).Min(),
                CeilingTarget.LowerHighestFloor or CeilingTarget.RaiseHighestFloor => LineSpecials.NeighborSectors(sim, sector)
                    .Select(index => sim.Floors[index]).DefaultIfEmpty(sim.Floors[sector]).Max() + distance,
                CeilingTarget.NextHigher => LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Ceilings[index])
                    .Where(height => height > current).DefaultIfEmpty(current).Min(),
                CeilingTarget.NextLower => LineSpecials.NeighborSectors(sim, sector).Select(index => sim.Ceilings[index])
                    .Where(height => height < current).DefaultIfEmpty(current).Max(),
                CeilingTarget.RaiseBy or CeilingTarget.RaiseInstant => current + distance,
                CeilingTarget.LowerBy or CeilingTarget.LowerInstant => current - distance,
                _ => sim.Floors[sector] + distance,
            };
            // Keep the requested target; native downward movement clamps against the
            // floor at each tick, while upward movement does not apply that limit.
            target = Math.Min(target, short.MaxValue);
            // Native creation retains zero-travel thinkers too: they own the plane
            // and complete (or change crusher legs) through the normal tick path.
            sim.Motions.Add(new SectorMotion {
                SectorIndex = sector, Kind = MotionKind.Ceiling, Tag = creationTag, ClosedCeiling = current,
                CeilingDirection = kind switch {
                    CeilingTarget.Highest or CeilingTarget.NextHigher or CeilingTarget.RaiseBy or CeilingTarget.RaiseInstant
                        or CeilingTarget.RaiseLowest or CeilingTarget.RaiseHighestFloor or CeilingTarget.RaiseFloor => 1,
                    CeilingTarget.Absolute => target < current ? -1 : 1,
                    _ => -1,
                },
                TargetCeiling = target, Speed = speed, ReturnSpeed = upSpeed ?? speed,
                CloseAfterOpen = returnToStart, Loop = loop, CrushDamage = crush,
                SlowOnCrush = slow, StopOnCrush = stop,
            });
            started = true;
        }
        return started;
    }

    private static bool Resume(AuthoritySimulation sim, int tag)
    {
        var resumed = false;
        foreach (var motion in sim.Motions)
        {
            if (motion.Kind != MotionKind.Ceiling || motion.Tag != tag || !motion.Paused) continue;
            motion.Paused = false;
            resumed = true;
        }
        return resumed;
    }

    internal static bool Stop(AuthoritySimulation sim, LevelLine? line, int tag, bool remove = false)
    {
        var stopped = false;
        for (var i = sim.Motions.Count - 1; i >= 0; i--)
        {
            var motion = sim.Motions[i];
            if (motion.Kind != MotionKind.Ceiling || motion.Tag != tag || motion.Paused) continue;
            if (remove) { sim.Motions.RemoveAt(i); stopped = true; }
            else if (!motion.Paused) { motion.Paused = true; stopped = true; }
        }
        return stopped;
    }

    internal static bool StopAll(AuthoritySimulation sim, LevelLine? line, int tag)
    {
        var targets = LineSpecials.TargetSectors(sim, line, tag, backSide: true).ToHashSet();
        sim.Motions.RemoveAll(motion => motion.MovesCeiling && targets.Contains(motion.SectorIndex));
        return true;
    }

    // Return true when the thinker has completed. Closing denotes the return leg.
    internal static bool Tick(AuthoritySimulation sim, SectorMotion motion)
    {
        if (motion.Paused) return false;
        var current = sim.Ceilings[motion.SectorIndex];
        var speed = motion.Closing ? motion.ReturnSpeed : motion.Slowed ? 0.125 : motion.Speed;
        var direction = motion.Closing ? 1 : motion.CeilingDirection;
        var target = motion.Closing ? motion.ClosedCeiling : motion.TargetCeiling;
        if (direction < 0) target = Math.Max(sim.Floors[motion.SectorIndex], target);
        // Native upward thinker calls omit crush damage, even for a target below the current plane.
        var crushDamage = direction > 0 ? -1 : motion.CrushDamage;
        var next = direction > 0 ? Math.Min(target, current + speed) : Math.Max(target, current - speed);
        if (next < current)
        {
            var blocked = sim.Actors.Where(actor => actor.BlocksActors && actor.SectorIndex == motion.SectorIndex
                && actor.Z.ToDouble() + actor.Height.ToDouble() > next).ToArray();
            if (blocked.Length > 0)
            {
                if (crushDamage >= 0 && (sim.Thinkers.Clock.Tic & 3) == 0)
                {
                    var tic = sim.Thinkers.Clock.Tic;
                    foreach (var actor in blocked)
                    {
                        actor.MarkMoverCrush(tic);
                        ActorDamage.Apply(actor, crushDamage);
                        ActorPhysics.CrushStandingRiders(sim, actor, crushDamage, tic);
                    }
                }
                // Native MoveCeiling completes a destination-clamped leg even when
                // obstruction rolls the plane back; an overshoot also forces rollback.
                if (crushDamage < 0 || motion.StopOnCrush
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
