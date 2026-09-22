using HCDE.MapLoader;

namespace HCDE.Playsim;

public enum MotionKind
{
    Door,
    Floor,
    Lift,
}

public sealed class SectorMotion
{
    public int SectorIndex { get; init; }
    public short ClosedFloor { get; init; }
    public short TargetFloor { get; init; }
    public MotionKind Kind { get; init; }
    public bool CloseAfterOpen { get; init; }
    public int Wait { get; set; }
    public bool Closing { get; set; }
}

/// <summary>
/// Doom linedef specials used by the headless server: doors, an 8-unit floor step, a lift, exit, teleport, and ACS execute.
/// MBF21 floor nudge and the ID24 secret exit stay behind <see cref="CompatSurface"/>.
/// </summary>
public static class LineSpecials
{
    public const int DoorRaise = 1;
    public const int DoorOpen = 11;
    public const int FloorRaise = 18;
    public const int FloorLower = 19;
    public const int ExitNormal = 52;
    public const int Lift = 62;
    public const int Teleport = 70;
    public const int AcsExecute = 80;
    public const int FloorStep = 8;
    public const int TeleportDestType = 14;
    private const int DoorSpeed = 8;
    private const int DoorWaitTics = 5;

    public static void ActivateCrossings(AuthoritySimulation sim)
    {
        foreach (var actor in sim.Actors)
        {
            if (actor.PreviousX.Raw == actor.X.Raw && actor.PreviousY.Raw == actor.Y.Raw)
                continue;

            foreach (var line in sim.Level.Lines)
            {
                if (line.Special == 0)
                    continue;
                if (!LineSlide.Crosses(actor.PreviousX.ToDouble(), actor.PreviousY.ToDouble(), actor.X.ToDouble(), actor.Y.ToDouble(), line))
                    continue;

                var activated = Execute(sim, actor, line.Special, line.Tag, line);
                if (activated && !line.RepeatsSpecial)
                    line.Special = 0;
            }
        }
    }

    public static bool Execute(AuthoritySimulation sim, Actor? activator, int special, int tag, LevelLine? line = null)
    {
        switch (special)
        {
            case DoorRaise:
                return StartDoor(sim, line, tag, closeAfterOpen: true);
            case DoorOpen:
                return StartDoor(sim, line, tag, closeAfterOpen: false);
            case FloorRaise:
                return StartFloor(sim, line, tag, FloorStep, lift: false);
            case FloorLower:
                return StartFloor(sim, line, tag, -FloorStep, lift: false);
            case Lift:
                return StartFloor(sim, line, tag, -FloorStep, lift: true);
            case ExitNormal:
                sim.MarkExited(secret: false);
                return true;
            case Teleport:
                return TeleportActivator(sim, activator);
            case AcsExecute:
                var script = line != null && line.Arg0 != 0 ? line.Arg0 : tag;
                return sim.Acs.Enqueue(script);
            case CompatSurfaceRules.Id24SecretExit:
                if (!sim.Compat.HasFlag(CompatSurface.Id24))
                    return false;
                sim.MarkExited(secret: true);
                return true;
            case CompatSurfaceRules.Mbf21FloorNudge:
                if (!sim.Compat.HasFlag(CompatSurface.Mbf21))
                    return false;
                return NudgeFloor(sim, line, tag, DoorSpeed);
            default:
                return false;
        }
    }

    public static void TickMotions(AuthoritySimulation sim)
    {
        for (var i = sim.Motions.Count - 1; i >= 0; i--)
        {
            var motion = sim.Motions[i];
            if (motion.Wait > 0)
            {
                motion.Wait--;
                if (motion.Wait == 0 && motion.CloseAfterOpen)
                    motion.Closing = true;
                continue;
            }

            var floor = sim.Floors[motion.SectorIndex];
            var target = motion.Kind == MotionKind.Door && !motion.Closing
                ? (short)Math.Max(motion.ClosedFloor, sim.Ceilings[motion.SectorIndex] - 4)
                : motion.Closing
                    ? motion.ClosedFloor
                    : motion.TargetFloor;
            if (floor < target)
                floor = (short)Math.Min(target, floor + DoorSpeed);
            else if (floor > target)
                floor = (short)Math.Max(target, floor - DoorSpeed);
            sim.Floors[motion.SectorIndex] = floor;

            if (floor != target)
                continue;
            if (!motion.Closing && motion.CloseAfterOpen)
            {
                motion.Wait = DoorWaitTics;
                continue;
            }

            sim.Motions.RemoveAt(i);
        }
    }

    private static bool StartDoor(AuthoritySimulation sim, LevelLine? line, int tag, bool closeAfterOpen)
    {
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector))
                continue;
            sim.Motions.Add(new SectorMotion
            {
                SectorIndex = sector,
                ClosedFloor = sim.Floors[sector],
                CloseAfterOpen = closeAfterOpen,
            });
            started = true;
        }

        return started;
    }

    private static bool StartFloor(AuthoritySimulation sim, LevelLine? line, int tag, int delta, bool lift)
    {
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector))
                continue;

            var floor = sim.Floors[sector];
            var target = delta > 0
                ? (short)Math.Min(sim.Ceilings[sector], floor + delta)
                : (short)Math.Max(short.MinValue, floor + delta);
            if (target == floor)
                continue;

            sim.Motions.Add(new SectorMotion
            {
                SectorIndex = sector,
                ClosedFloor = floor,
                TargetFloor = target,
                Kind = lift ? MotionKind.Lift : MotionKind.Floor,
                CloseAfterOpen = lift,
            });
            started = true;
        }

        return started;
    }

    private static bool NudgeFloor(AuthoritySimulation sim, LevelLine? line, int tag, int amount)
    {
        var nudged = false;
        foreach (var sector in TargetSectors(sim, line, tag))
        {
            var ceiling = sim.Ceilings[sector];
            var next = (short)Math.Min(ceiling, sim.Floors[sector] + amount);
            if (next == sim.Floors[sector])
                continue;
            sim.Floors[sector] = next;
            nudged = true;
        }

        return nudged;
    }

    private static bool TeleportActivator(AuthoritySimulation sim, Actor? activator)
    {
        if (activator == null)
            return false;
        var dest = sim.Actors.FirstOrDefault(actor => actor.DoomEdNum == TeleportDestType);
        if (dest == null)
            return false;
        activator.X = dest.X;
        activator.Y = dest.Y;
        activator.Angle = dest.Angle;
        return true;
    }

    private static IEnumerable<int> TargetSectors(AuthoritySimulation sim, LevelLine? line, int tag)
    {
        if (tag != 0)
        {
            for (var i = 0; i < sim.Level.Sectors.Count; i++)
            {
                if (sim.Level.Sectors[i].Tag == tag)
                    yield return i;
            }

            yield break;
        }

        if (line == null || line.SideFront < 0 || line.SideFront >= sim.Level.Sides.Count)
            yield break;
        var sector = sim.Level.Sides[line.SideFront].Sector;
        if (sector >= 0 && sector < sim.Floors.Length)
            yield return sector;
    }
}
