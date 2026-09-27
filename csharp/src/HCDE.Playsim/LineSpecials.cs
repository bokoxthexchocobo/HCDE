using HCDE.MapLoader;

namespace HCDE.Playsim;

public enum MotionKind
{
    Door,
    Floor,
    Lift,
    Ceiling,
    Stair,
}

public sealed class SectorMotion
{
    public int SectorIndex { get; init; }
    public double ClosedFloor { get; init; }
    public double TargetFloor { get; init; }
    public double ClosedCeiling { get; init; }
    public double TargetCeiling { get; init; }
    public MotionKind Kind { get; init; }
    public bool CloseAfterOpen { get; init; }
    public int Wait { get; set; }
    public bool Closing { get; set; }
    public double Speed { get; init; } = 8;
    public int Delay { get; init; } = 5;
    public int CrushDamage { get; init; }
    public int Tag { get; init; }
    public bool Paused { get; set; }
    public bool Loop { get; init; }
    public double ReturnSpeed { get; init; }
    public bool SlowOnCrush { get; init; }
    public bool StopOnCrush { get; init; }
    public bool Slowed { get; set; }
    public int StairGroup { get; init; }
    public bool Completed { get; set; }
    public bool MovesCeiling => Kind is MotionKind.Door or MotionKind.Ceiling;
}

/// <summary>
/// Managed flat-sector doors, floors, lifts, ceilings, crushers, stairs, exits, teleport and ACS dispatch.
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
            if (actor.IsDead || actor.Destroyed || actor is ProjectileActor)
                continue;
            if (actor.PreviousX.Raw == actor.X.Raw && actor.PreviousY.Raw == actor.Y.Raw)
                continue;

            foreach (var line in sim.Level.Lines)
            {
                if (line.Special == 0)
                    continue;
                if (!LineSlide.Crosses(actor.PreviousX.ToDouble(), actor.PreviousY.ToDouble(), actor.X.ToDouble(), actor.Y.ToDouble(), line))
                    continue;

                ActivateMapLine(sim, actor, line, use: false);
            }
        }
    }

    public static void ActivateUses(AuthoritySimulation sim)
    {
        foreach (var player in sim.Actors.OfType<PlayerPawn>())
        {
            if (!player.UsePressed || player.IsDead || player.Destroyed) continue;
            player.UsePressed = false;
            var radians = player.Angle.ToDegrees() * Math.PI / 180;
            var hits = sim.Level.Lines.Select(line => (Line: line, Distance: CombatTrace.RayLine(
                player.X.ToDouble(), player.Y.ToDouble(), Math.Cos(radians), Math.Sin(radians), line)))
                .Where(hit => hit.Distance <= 64).OrderBy(hit => hit.Distance);
            foreach (var hit in hits)
            {
                if (ActivateMapLine(sim, player, hit.Line, use: true))
                {
                    if (!hit.Line.UseThrough) break;
                    continue;
                }
                if (CombatTrace.BlocksShot(sim, hit.Line, player.Z.ToDouble() + player.Height.ToDouble() / 2)) break;
            }
        }
    }

    /// <summary>Dispatch map numbers in their own namespace; Execute remains the internal action API.</summary>
    public static bool ActivateMapLine(AuthoritySimulation sim, Actor actor, LevelLine line, bool use)
    {
        if (actor.IsDead || actor.Destroyed || line.Special == 0) return false;
        if (sim.Level.Format == MapDataFormat.Unknown)
        {
            var legacy = Execute(sim, actor, line.Special, line.Tag, line);
            if (legacy && !line.RepeatsSpecial) line.Special = 0;
            return legacy;
        }
        if (actor is not PlayerPawn) return false;
        var doom = sim.Level.Format == MapDataFormat.DoomBinary
            || sim.Level.Namespace.Equals("Doom", StringComparison.OrdinalIgnoreCase)
            || sim.Level.Namespace.Equals("ZDoomTranslated", StringComparison.OrdinalIgnoreCase);
        bool activated;
        bool repeat;
        if (doom)
        {
            repeat = line.Special is 1 or 26 or 27 or 28 or 43 or 60 or 62 or 65 or 69 or 72 or 73 or 74 or 77 or 82 or 88 or 90 or 92 or 94 or 97 or 124 or 150 or 152 or 183 or 184 or 185 or 187 or 188 or 256 or 257 or 258 or 259;
            activated = (line.Special, use) switch
            {
                (1, true) => StartDoor(sim, line, 0, true),
                (31, true) => StartDoor(sim, line, 0, false),
                (26 or 27 or 28 or 32 or 33 or 34, true) => HasDoomKey((PlayerPawn)actor, line.Special)
                    && StartDoor(sim, line, 0, line.Special < 30),
                (2, false) => StartDoor(sim, line, line.Tag, false),
                (4 or 90, false) => StartDoor(sim, line, line.Tag, true),
                (11, true) or (52, false) => Exit(sim, false),
                (51, true) or (124, false) => Exit(sim, true),
                (39 or 97, false) => TeleportActivator(sim, actor, line.Tag),
                (18 or 69, true) => StartMapFloor(sim, line, line.Tag, FloorTarget.NextHigher),
                (23 or 60, true) or (38 or 82, false) => StartMapFloor(sim, line, line.Tag, FloorTarget.Lowest),
                (58 or 92, false) => StartMapFloor(sim, line, line.Tag, FloorTarget.RaiseBy, amount: 24),
                (21 or 62, true) or (10 or 88, false) => StartMapFloor(sim, line, line.Tag, FloorTarget.Lowest, speed: 4, lift: true),
                (55 or 65, true) or (56 or 94, false) => StartMapFloor(sim, line, line.Tag, FloorTarget.CrushCeiling, crush: 10),
                (41 or 43, true) or (145 or 152, false) => CeilingActions.Start(sim, line, line.Tag, CeilingTarget.Floor),
                (6 or 77, false) or (164 or 183, true) => CeilingActions.Start(sim, line, line.Tag, CeilingTarget.Crush,
                    speed: 2, upSpeed: 2, crush: 10, returnToStart: true, loop: true),
                (25 or 73 or 141 or 150, false) or (49 or 165 or 184 or 185, true) => CeilingActions.Start(sim, line, line.Tag, CeilingTarget.Crush,
                    crush: 10, returnToStart: true, loop: true, slow: true),
                (44 or 72, false) or (167 or 187, true) => CeilingActions.Start(sim, line, line.Tag, CeilingTarget.Crush,
                    crush: 0, stop: true),
                (57 or 74, false) or (168 or 188, true) => CeilingActions.Stop(sim, line, line.Tag),
                (7 or 258, true) or (8 or 256, false) => StairActions.Start(sim, line, line.Tag, 0.25, 8),
                (127 or 259, true) or (100 or 257, false) => StairActions.Start(sim, line, line.Tag, 4, 16, crush: true),
                _ => false,
            };
        }
        else
        {
            if (use ? !line.PlayerUse : !line.PlayerCross) return false;
            repeat = line.Repeat;
            activated = line.Special switch
            {
                11 => StartDoor(sim, line, line.Arg0, false, line.Arg1 / 8.0),
                12 => StartDoor(sim, line, line.Arg0, true, line.Arg1 / 8.0, line.Arg2),
                20 => StartMapFloor(sim, line, line.Arg0, FloorTarget.LowerBy, line.Arg1 / 8.0, line.Arg2),
                21 => StartMapFloor(sim, line, line.Arg0, FloorTarget.Lowest, line.Arg1 / 8.0),
                23 => StartMapFloor(sim, line, line.Arg0, FloorTarget.RaiseBy, line.Arg1 / 8.0, line.Arg2),
                25 => StartMapFloor(sim, line, line.Arg0, FloorTarget.NextHigher, line.Arg1 / 8.0),
                62 => StartMapFloor(sim, line, line.Arg0, FloorTarget.Lowest, line.Arg1 / 8.0, lift: true,
                    delay: Math.Max(0, line.Arg2), lip: 8),
                70 => TeleportActivator(sim, actor, line.Arg0, byThingId: true),
                80 => sim.Acs.Enqueue(line.Arg0),
                243 => Exit(sim, false),
                244 => Exit(sim, true),
                40 when line.Arg3 == 0 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.LowerBy, speed: line.Arg1 / 8.0, amount: line.Arg2,
                    crush: line.Arg4 > 0 ? line.Arg4 : -1),
                41 when line.Arg3 == 0 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.RaiseBy, speed: line.Arg1 / 8.0, amount: line.Arg2,
                    crush: line.Arg4 > 0 ? line.Arg4 : -1),
                42 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Crush, speed: line.Arg1 / 8.0, upSpeed: line.Arg1 / 16.0,
                    crush: line.Arg2, returnToStart: true, loop: true, slow: line.Arg3 == 3, stop: line.Arg3 == 2),
                43 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Crush, speed: line.Arg1 / 8.0,
                    crush: line.Arg2, slow: line.Arg3 == 3 || line.Arg3 == 0 && line.Arg1 == 8, stop: line.Arg3 == 2),
                44 => CeilingActions.Stop(sim, line, line.Arg0, remove: line.Arg1 == 2),
                45 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Crush, speed: line.Arg1 / 8.0, upSpeed: line.Arg1 / 16.0,
                    crush: line.Arg2, returnToStart: true, slow: line.Arg3 == 3, stop: line.Arg3 == 2),
                168 or 104 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Crush, speed: line.Arg2 / 8.0, upSpeed: line.Arg2 / 8.0,
                    amount: line.Arg1, crush: line.Arg3, returnToStart: true, loop: true,
                    slow: line.Arg4 == 3 || line.Arg4 == 0 && line.Arg2 == 8, stop: line.Arg4 == 2),
                254 when line.Arg2 == 0 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Floor, speed: line.Arg1 / 8.0, amount: line.Arg4,
                    crush: line.Arg3 > 0 ? line.Arg3 : -1),
                262 when line.Arg2 == 0 => CeilingActions.Start(sim, line, line.Arg0, CeilingTarget.Highest, speed: line.Arg1 / 8.0),
                217 or 270 or 273 when line.Arg3 == 0 && line.Arg4 == 0 => StairActions.Start(sim, line, line.Arg0,
                    line.Arg1 / 8.0, line.Arg2, down: line.Special == 270, crush: line.Special == 273),
                _ => false,
            };
        }
        if (activated && !repeat) line.Special = 0;
        return activated;
    }

    private static bool Exit(AuthoritySimulation sim, bool secret)
    {
        sim.MarkExited(secret);
        return true;
    }

    private static bool HasDoomKey(PlayerPawn player, int special) => special switch
    {
        26 or 32 => player.Inventory.BlueKey,
        27 or 34 => player.Inventory.YellowKey,
        28 or 33 => player.Inventory.RedKey,
        _ => false,
    };

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
            if (motion.Completed) continue;
            if (motion.Kind == MotionKind.Ceiling)
            {
                if (CeilingActions.Tick(sim, motion)) sim.Motions.RemoveAt(i);
                continue;
            }
            if (motion.Wait > 0)
            {
                motion.Wait--;
                if (motion.Wait == 0 && motion.CloseAfterOpen)
                    motion.Closing = true;
                continue;
            }

            var door = motion.Kind == MotionKind.Door;
            var floor = door ? sim.Ceilings[motion.SectorIndex] : sim.Floors[motion.SectorIndex];
            var target = door ? (motion.Closing ? motion.ClosedCeiling : motion.TargetCeiling)
                : motion.Closing
                    ? motion.ClosedFloor
                    : motion.TargetFloor;
            target = door ? Math.Max(target, sim.Floors[motion.SectorIndex])
                : Math.Min(target, sim.Ceilings[motion.SectorIndex]);
            if (floor < target)
                floor = Math.Min(target, floor + motion.Speed);
            else if (floor > target)
                floor = Math.Max(target, floor - motion.Speed);
            if (door && motion.Closing && sim.Actors.Any(actor => actor.BlocksActors
                && actor.SectorIndex == motion.SectorIndex && actor.Z.ToDouble() + actor.Height.ToDouble() > floor))
            {
                motion.Closing = false;
                continue;
            }
            if (!door && floor > sim.Floors[motion.SectorIndex])
            {
                var blocked = sim.Actors.Where(actor => actor.BlocksActors && actor.SectorIndex == motion.SectorIndex
                    && Math.Max(actor.Z.ToDouble(), floor) + actor.Height.ToDouble() > sim.Ceilings[motion.SectorIndex]).ToArray();
                if (blocked.Length > 0)
                {
                    if (motion.CrushDamage > 0)
                    {
                        if ((sim.Thinkers.Clock.Tic & 3) == 0)
                            foreach (var actor in blocked) ActorDamage.Apply(actor, motion.CrushDamage);
                        if (motion.StopOnCrush) continue;
                    }
                    else
                    {
                        if (motion.Kind == MotionKind.Lift && motion.Closing)
                        { motion.Closing = false; motion.Wait = 0; }
                        continue;
                    }
                }
            }
            if (door) sim.Ceilings[motion.SectorIndex] = floor;
            else sim.Floors[motion.SectorIndex] = floor;

            if (floor != target)
                continue;
            if (!motion.Closing && motion.CloseAfterOpen)
            {
                motion.Wait = motion.Delay;
                if (motion.Wait == 0) motion.Closing = true;
                continue;
            }

            if (motion.Kind == MotionKind.Stair) motion.Completed = true;
            else sim.Motions.RemoveAt(i);
        }
        // Keep the entire stair chain locked until its last step finishes.
        var completedStairs = sim.Motions.Where(motion => motion.Kind == MotionKind.Stair)
            .GroupBy(motion => motion.StairGroup).Where(group => group.All(motion => motion.Completed))
            .Select(group => group.Key).ToHashSet();
        sim.Motions.RemoveAll(motion => motion.Kind == MotionKind.Stair && completedStairs.Contains(motion.StairGroup));
    }

    private static bool StartDoor(AuthoritySimulation sim, LevelLine? line, int tag, bool closeAfterOpen, double? speed = null, int? delay = null)
    {
        if (speed <= 0) return false;
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag, backSide: sim.Level.Format != MapDataFormat.Unknown))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && motion.MovesCeiling))
                continue;
            var neighbors = new List<double>();
            foreach (var boundary in sim.Level.Lines)
            {
                if ((uint)boundary.SideFront >= (uint)sim.Level.Sides.Count
                    || (uint)boundary.SideBack >= (uint)sim.Level.Sides.Count) continue;
                var front = sim.Level.Sides[boundary.SideFront].Sector;
                var back = sim.Level.Sides[boundary.SideBack].Sector;
                if (front == sector && back != sector && (uint)back < (uint)sim.Ceilings.Length) neighbors.Add(sim.Ceilings[back]);
                if (back == sector && front != sector && (uint)front < (uint)sim.Ceilings.Length) neighbors.Add(sim.Ceilings[front]);
            }
            if (neighbors.Count == 0) continue;
            var targetCeiling = Math.Clamp(neighbors.Min() - 4, sim.Floors[sector], short.MaxValue);
            if (targetCeiling <= sim.Ceilings[sector]) continue;
            sim.Motions.Add(new SectorMotion
            {
                SectorIndex = sector,
                ClosedFloor = sim.Floors[sector],
                ClosedCeiling = sim.Floors[sector],
                TargetCeiling = targetCeiling,
                CloseAfterOpen = closeAfterOpen,
                Speed = speed ?? (sim.Level.Format == MapDataFormat.Unknown ? DoorSpeed : 2),
                Delay = Math.Max(0, delay ?? (sim.Level.Format == MapDataFormat.Unknown ? DoorWaitTics : 150)),
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
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && !motion.MovesCeiling))
                continue;

            var floor = sim.Floors[sector];
            var target = delta > 0
                ? Math.Min(sim.Ceilings[sector], floor + delta)
                : Math.Max(short.MinValue, floor + delta);
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

    private enum FloorTarget { Lowest, NextHigher, RaiseBy, LowerBy, CrushCeiling }

    private static bool StartMapFloor(AuthoritySimulation sim, LevelLine line, int tag, FloorTarget kind,
        double speed = 1, int amount = 0, bool lift = false, int delay = 105, int lip = 0, int crush = 0)
    {
        if (amount < 0 || speed <= 0) return false;
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && !motion.MovesCeiling)) continue;
            var current = sim.Floors[sector];
            var neighbors = NeighborSectors(sim, sector).Select(index => sim.Floors[index]).ToArray();
            double destination = kind switch
            {
                FloorTarget.Lowest => Math.Min(current, neighbors.DefaultIfEmpty(current).Min() + lip),
                FloorTarget.NextHigher => neighbors.Where(height => height > current).DefaultIfEmpty(current).Min(),
                FloorTarget.RaiseBy => current + amount,
                FloorTarget.LowerBy => current - amount,
                _ => sim.Ceilings[sector] - 8L,
            };
            var target = Math.Clamp(destination, short.MinValue, sim.Ceilings[sector]);
            if (target == current || kind == FloorTarget.CrushCeiling && target < current) continue;
            sim.Motions.Add(new SectorMotion {
                SectorIndex = sector, ClosedFloor = current, TargetFloor = target,
                Kind = lift ? MotionKind.Lift : MotionKind.Floor, CloseAfterOpen = lift,
                Speed = speed, Delay = delay, CrushDamage = crush,
            });
            started = true;
        }
        return started;
    }

    internal static IEnumerable<int> NeighborSectors(AuthoritySimulation sim, int sector)
    {
        foreach (var boundary in sim.Level.Lines)
        {
            if ((uint)boundary.SideFront >= (uint)sim.Level.Sides.Count || (uint)boundary.SideBack >= (uint)sim.Level.Sides.Count) continue;
            var front = sim.Level.Sides[boundary.SideFront].Sector;
            var back = sim.Level.Sides[boundary.SideBack].Sector;
            var other = front == sector ? back : back == sector ? front : -1;
            if (other != sector && (uint)other < (uint)sim.Floors.Length) yield return other;
        }
    }

    private static bool NudgeFloor(AuthoritySimulation sim, LevelLine? line, int tag, int amount)
    {
        var nudged = false;
        foreach (var sector in TargetSectors(sim, line, tag))
        {
            var ceiling = sim.Ceilings[sector];
            var next = Math.Min(ceiling, sim.Floors[sector] + amount);
            if (next == sim.Floors[sector])
                continue;
            sim.Floors[sector] = next;
            nudged = true;
        }

        return nudged;
    }

    private static bool TeleportActivator(AuthoritySimulation sim, Actor? activator, int target = 0, bool byThingId = false)
    {
        if (activator == null)
            return false;
        var dest = sim.Actors.FirstOrDefault(actor => actor.DoomEdNum == TeleportDestType && !actor.Destroyed
            && (target == 0 || (byThingId ? actor.ThingId == target
                : (uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count && sim.Level.Sectors[actor.SectorIndex].Tag == target)));
        if (dest == null)
            return false;
        activator.X = dest.X;
        activator.Y = dest.Y;
        activator.Angle = dest.Angle;
        activator.VelocityX = activator.VelocityY = activator.VelocityZ = default;
        ActorPhysics.PlaceOnFloor(sim, activator);
        return true;
    }

    internal static IEnumerable<int> TargetSectors(AuthoritySimulation sim, LevelLine? line, int tag, bool backSide = false)
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

        var side = line == null ? -1 : backSide ? line.SideBack : line.SideFront;
        if ((uint)side >= (uint)sim.Level.Sides.Count)
            yield break;
        var sector = sim.Level.Sides[side].Sector;
        if (sector >= 0 && sector < sim.Floors.Length)
            yield return sector;
    }
}
