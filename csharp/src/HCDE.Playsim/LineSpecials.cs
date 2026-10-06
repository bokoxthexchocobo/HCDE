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
    public bool CloseOnly { get; init; }
    public bool OpenAfterClose { get; init; }
    public int Wait { get; set; }
    public bool Closing { get; set; }
    public double Speed { get; init; } = 8;
    public int Delay { get; init; } = 5;
    public int CrushDamage { get; init; }
    public bool CrushWithoutDamage { get; init; }
    public bool FloorCrushStopEligible { get; init; }
    public int Tag { get; init; }
    public bool Paused { get; set; }
    public bool Loop { get; init; }
    public double ReturnSpeed { get; init; }
    public bool SlowOnCrush { get; init; }
    public bool StopOnCrush { get; init; }
    public bool Slowed { get; set; }
    public int StairGroup { get; init; }
    public int StairSeedIndex { get; init; }
    public bool Completed { get; set; }
    public bool StairStopped { get; set; }
    public int ResetCountdown { get; set; }
    public bool WaitingForReset { get; set; }
    public int StepPeriod { get; init; }
    public int StepCountdown { get; set; }
    public int StairPause { get; set; }
    public bool MovesCeiling => Kind is MotionKind.Door or MotionKind.Ceiling;
    public int CeilingDirection { get; init; }
    public int FloorDirection { get; init; }
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
    public const int SectorCopyScroller = 58;
    public const int ScrollFloor = 223;
    public const int ScrollCeiling = 224;
    public const int SectorSetRotation = 185;
    /// <summary>Eternity <c>Scroll_Wall</c> (Hexen/UDMF action 52 — not Doom binary exit).</summary>
    public const int ScrollWall = WallScrollActions.ScrollWall;
    public const int FloorStep = 8;
    public const int TeleportDestType = 14;
    private const int DoorSpeed = 8;
    private const int DoorWaitTics = 5;

    internal static bool IsBackSide(LevelLine line, double x, double y, bool vanilla = false)
    {
        var dx = line.X2 - line.X1;
        var dy = line.Y2 - line.Y1;
        if (!vanilla && (line.Flags & LevelLine.CompatSideFlag) == 0)
            return dx * (y - line.Y1) - dy * (x - line.X1) > 1.0 / 65536;
        if (dx == 0) return x <= line.X1 ? dy > 0 : dy < 0;
        if (dy == 0) return y <= line.Y1 ? dx < 0 : dx > 0;
        // Native vanilla classification deliberately retains fixed-point imprecision.
        var xOffset = Fixed.FromDouble(x - line.X1).Raw;
        var yOffset = Fixed.FromDouble(y - line.Y1).Raw;
        var left = unchecked((int)(((long)(int)(dy * 256) * xOffset) >> 16));
        var right = unchecked((int)(((long)yOffset * (int)(dx * 256)) >> 16));
        return right >= left;
    }

    public static void ActivateCrossings(AuthoritySimulation sim)
    {
        foreach (var actor in sim.Actors)
        {
            if (actor.IsDead || actor.Destroyed || actor.NoTrigger || actor is ProjectileActor)
                continue;
            if (actor.PreviousX.Raw == actor.X.Raw && actor.PreviousY.Raw == actor.Y.Raw)
                continue;

            foreach (var line in sim.Level.Lines)
            {
                if (line.Special == 0)
                    continue;
                if (!LineSlide.Crosses(actor.PreviousX.ToDouble(), actor.PreviousY.ToDouble(), actor.X.ToDouble(), actor.Y.ToDouble(), line))
                    continue;

                ActivateMapLine(sim, actor, line, use: false, backSide: IsBackSide(line, actor.PreviousX.ToDouble(), actor.PreviousY.ToDouble(), sim.Compat.HasFlag(CompatSurface.PointOnLine)));
            }
        }
    }

    public static void ActivateUses(AuthoritySimulation sim)
    {
        foreach (var player in sim.Actors.OfType<PlayerPawn>().ToArray())
        {
            if (!player.UsePressed || player.IsDead || player.Destroyed) continue;
            player.UsePressed = false;
            // P_UseLines traces yaw for Player.UseRange (64). Portals are not applied.
            var range = double.IsFinite(player.UseRange) ? Math.Abs(player.UseRange) : 0;
            if (!double.IsFinite(player.UseRange)) continue;
            var radians = player.Angle.ToDegrees() * Math.PI / 180;
            var direction = player.UseRange < 0 ? -1 : 1;
            var x = player.X.ToDouble();
            var y = player.Y.ToDouble();
            var hits = sim.Level.Lines.Select(line => (Line: (LevelLine?)line, Thing: (Actor?)null, Distance: CombatTrace.RayLine(
                x, y, Math.Cos(radians) * direction, Math.Sin(radians) * direction, line, range)))
                .Concat(sim.Actors.Where(actor => actor != player && !actor.Destroyed && !actor.NoBlockmap).Select(actor =>
                    (Line: (LevelLine?)null, Thing: (Actor?)actor, Distance: CombatTrace.ActorBoxEntry(
                        actor.X.ToDouble() - x, actor.Y.ToDouble() - y, actor.Radius.ToDouble(),
                        Math.Cos(radians) * direction, Math.Sin(radians) * direction, range))))
                .Where(hit => double.IsFinite(hit.Distance) && hit.Distance <= range).OrderBy(hit => hit.Distance).ToArray();
            foreach (var hit in hits)
            {
                if (hit.Thing is { } thing)
                {
                    if (!thing.Destroyed && thing.UseSpecial && ActorSpecialActions.ActivateSpecial(sim, thing, player)) break;
                    if (!thing.Destroyed && thing.Used(player)) break;
                    continue;
                }
                if (hit.Line is null) continue;
                var back = IsBackSide(hit.Line, x, y, sim.Compat.HasFlag(CompatSurface.PointOnLine));
                var consumes = back || (sim.Compat.HasFlag(CompatSurface.UseBlocking)
                    ? !hit.Line.UseThrough : hit.Line.PlayerUse || !hit.Line.UseThrough);
                // The back side activates only with SPAC_UseBack. UseBack-only lines ignore the front.
                var usable = back ? hit.Line.PlayerUseBack : !hit.Line.PlayerUseBack || hit.Line.PlayerUse || hit.Line.UseThrough;
                if (!usable)
                {
                    if (BlocksUse(sim, hit.Line)) break;
                    continue;
                }
                if (ActivateMapLine(sim, player, hit.Line, use: true, backSide: back))
                {
                    // Back use consumes; front precedence depends on use-blocking compatibility.
                    if (consumes) break;
                    continue;
                }
                // A recognized use trigger consumes traversal even when its action fails.
                if (hit.Line.Special != 0 && (hit.Line.PlayerUseBack || hit.Line.PlayerUse || hit.Line.UseThrough))
                {
                    if (consumes) break;
                    continue;
                }
                if (BlocksUse(sim, hit.Line)) break;
            }
        }
    }

    private static bool BlocksUse(AuthoritySimulation sim, LevelLine line)
    {
        if (line.Special != 0 && sim.Compat.HasFlag(CompatSurface.UseBlocking)) return true;
        if (line.OneSided || (line.Flags & (LevelLine.BlockUseFlag | LevelLine.BlockEverythingFlag)) != 0) return true;
        var sides = sim.Level.Sides;
        if ((uint)line.SideFront >= (uint)sides.Count || (uint)line.SideBack >= (uint)sides.Count) return true;
        var front = sides[line.SideFront].Sector; var back = sides[line.SideBack].Sector;
        if ((uint)front >= (uint)sim.Floors.Length || (uint)back >= (uint)sim.Floors.Length) return true;
        // Native blocked-use traversal checks opening range, not the player's trace height.
        return Math.Min(sim.Ceilings[front], sim.Ceilings[back]) <= Math.Max(sim.Floors[front], sim.Floors[back]);
    }

    /// <summary>Dispatch map numbers in their own namespace; Execute remains the internal action API.</summary>
    public static bool ActivateMapLine(AuthoritySimulation sim, Actor actor, LevelLine line, bool use, bool? backSide = null)
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
                (17 or 156, false) or (172 or 193, true) => LightActions.DoomStrobe(sim, line.Tag, 5, 35),
                (12 or 80, false) or (169 or 192, true) => LightActions.Execute(sim, line.Tag, LightAction.Maximum),
                (104 or 157, false) or (173 or 194, true) => LightActions.Execute(sim, line.Tag, LightAction.Minimum),
                (13 or 81, false) or (138 or 171, true) => LightActions.Execute(sim, line.Tag, LightAction.Change, 255),
                (35 or 79, false) or (139 or 170, true) => LightActions.Execute(sim, line.Tag, LightAction.Change, 35),
                (1, true) => StartDoor(sim, line, 0, true),
                (31, true) => StartDoor(sim, line, 0, false),
                (26 or 27 or 28 or 32 or 33 or 34, true) => HasDoomKey((PlayerPawn)actor, line.Special)
                    && StartDoor(sim, line, 0, line.Special < 30),
                (2, false) => StartDoor(sim, line, line.Tag, false),
                (4 or 90, false) => StartDoor(sim, line, line.Tag, true),
                (11, true) or (52, false) => Exit(sim, false, actor),
                (51, true) or (124, false) => Exit(sim, true, actor),
                (39 or 97, false) => !(backSide ?? IsBackSide(line, actor.X.ToDouble(), actor.Y.ToDouble(), sim.Compat.HasFlag(CompatSurface.PointOnLine)))
                    && line.Tag != 0 && TeleportActivator(sim, actor, line.Tag),
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
            repeat |= line.Special is 79 or 80 or 81 or 138 or 139 or 156 or 157 or 192 or 193 or 194;
        }
        else
        {
            var allowsUse = backSide == true ? line.PlayerUseBack : line.PlayerUse || line.UseThrough;
            if (use ? !allowsUse : !line.PlayerCross) return false;
            repeat = line.Repeat;
            activated = line.Special switch
            {
                HealthActions.HealThing or HealthActions.DamageThing =>
                    HealthActions.Execute(line.Special, actor, line.Arg0, line.Arg1) == true,
                ThingDamage.Special or 133 => ThingDamage.Execute(sim, line.Special, actor, line.Arg0, line.Arg1, line.Arg2) == true,
                17 => ThingRaise.Execute(sim, line.Special, actor, line.Arg0, line.Arg1) == true,
                SectorSetRotation => ExecuteSectorRotation(sim, line.Special, line.Arg0, line.Arg1, line.Arg2) == true,
                WallTextureOffset.Special => WallTextureOffset.Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                WallTextureScale.Special => WallTextureScale.Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                SectorTextureScale.CeilingFixed or SectorTextureScale.FloorFixed or SectorTextureScale.Ceiling or SectorTextureScale.Floor =>
                    SectorTextureScale.Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                SectorTexturePanning.Floor or SectorTexturePanning.Ceiling =>
                    SectorTexturePanning.Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                SectorTextureAlignment.AlignFloor or SectorTextureAlignment.AlignCeiling =>
                    SectorTextureAlignment.Execute(sim, line.Special, line.Arg0, line.Arg1) == true,
                GeometryHealthActions.LineSetHealth or GeometryHealthActions.SectorSetHealth =>
                    GeometryHealthActions.Execute(sim, line.Special, line.Arg0, line.Arg1, line.Arg2) == true,
                214 => SectorDamage.ExecuteSpecial(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                110 or 111 or 112 or 113 or 114 or 115 or 116 or 117 or 232 or 233 or 234 =>
                    LightActions.ExecuteSpecial(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                10 or 11 or 12 or 249 => ExecuteDoorSpecial(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line) == true,
                20 or 21 or 22 or 23 or 24 or 25 or 28 or 35 or 36 or 37 or 46 or 62 or 66 or 67 or 68 or 99 or 238 or 239 or 242 or 256 or 257 or 258 or 259 or 260 or 275 or 279 or ScrollFloor or ScrollCeiling => ExecuteFloorSpecial(sim, line.Special,
                    line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4, line) == true,
                70 or 154 => !(backSide ?? IsBackSide(line, actor.X.ToDouble(), actor.Y.ToDouble(), sim.Compat.HasFlag(CompatSurface.PointOnLine)))
                    && ExecuteTeleportSpecial(sim, line.Special, line.Arg0, line.Arg1, actor, false) == true,
                216 => SectorGravity.ExecuteSpecial(sim, 216, line.Arg0, line.Arg1, line.Arg2) == true,
                132 => ThingRemove.ExecuteSpecial(sim, 132, actor, line.Arg0) == true,
                72 => ThingThrust.ExecuteSpecial(sim, 72, actor, line.Arg0, line.Arg1, line.Arg2, line.Arg3) == true,
                128 => ThingThrustZ.ExecuteSpecial(sim, 128, actor, line.Arg0, line.Arg1, line.Arg2, line.Arg3) == true,
                176 => ThingChangeTid.ExecuteSpecial(sim, 176, actor, line.Arg0, line.Arg1) == true,
                19 => ThingStop.ExecuteSpecial(sim, 19, actor, line.Arg0) == true,
                127 => ThingSetSpecial.Execute(sim, 127, actor, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4) == true,
                130 or 131 => ThingActivation.Execute(sim, actor, line.Arg0, line.Special == 130),
                80 or 81 or 82 or 226 => ExecuteScriptControl(sim, line.Special, line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4, actor, line, backSide ?? IsBackSide(line, actor.X.ToDouble(), actor.Y.ToDouble(), sim.Compat.HasFlag(CompatSurface.PointOnLine))) == true,
                243 => Exit(sim, false, actor),
                244 => Exit(sim, true, actor),
                40 or 41 or 42 or 43 or 44 or 45 or 47 or 69 or 97 or 104 or 168 or 192 or 193 or 194 or 195 or 196 or 197 or 198 or 199 or 252 or 253 or 254 or 255 or 262 or 263 or 264 or 265 or 266 or 267 or 276 or 280 => ExecuteCeilingSpecial(sim, line.Special,
                    line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4, line) == true,
                26 or 27 or 31 or 32 or 204 or 217 or 270 or 271 or 272 or 273 => ExecuteStairSpecial(sim, line.Special,
                    line.Arg0, line.Arg1, line.Arg2, line.Arg3, line.Arg4, line) == true,
                ScrollWall or WallScrollActions.ScrollBoth => WallScrollActions.ExecuteSpecial(sim, line),
                _ => false,
            };
        }
        if (activated && !repeat) line.Special = 0;
        return activated;
    }

    internal static bool? ExecuteSectorRotation(AuthoritySimulation sim, int special, int tag, int floorDegrees, int ceilingDegrees)
    {
        if (special != SectorSetRotation) return null;
        var floor = LevelBuilder.TextureAngleFromDegrees(floorDegrees);
        var ceiling = LevelBuilder.TextureAngleFromDegrees(ceilingDegrees);
        // Native uses the tag iterator without a trigger line: zero selects untagged sectors.
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag)) continue;
            sector.FloorTextureAngle = floor;
            sector.CeilingTextureAngle = ceiling;
        }
        return true;
    }

    internal static bool? ExecuteScriptControl(AuthoritySimulation sim, int special, int script, int map, int arg1, int arg2, int arg3, Actor? activator = null, LevelLine? triggerLine = null, bool backSide = false)
    {
        if (special is not (80 or 81 or 82 or 226)) return null;
        if (map != 0) return false;
        return special switch
        {
            80 => sim.Acs.TryExecute(script, [arg1, arg2, arg3], activator, triggerLine, backSide),
            81 => sim.Acs.SuspendScript(script),
            226 => sim.Acs.ExecuteAlways(script, [arg1, arg2, arg3], activator, triggerLine, backSide),
            _ => sim.Acs.TerminateScript(script),
        };
    }

    internal static bool? ExecuteDoorSpecial(AuthoritySimulation sim, int special, int tag, int speed, int arg2, int arg3,
        LevelLine? line = null)
    {
        if (special is not (10 or 11 or 12 or 249)) return null;
        if (tag == 0 && line is null) return false; // Manual doors need an activation line.
        if (special == 10) return arg2 == 0 && StartClosingDoor(sim, line, tag, speed / 8.0);
        if (special == 11) return arg2 == 0 && StartDoor(sim, line, tag, false, speed / 8.0);
        if (special == 12) return arg3 == 0 && arg2 >= 0 && StartDoor(sim, line, tag, true, speed / 8.0, arg2);
        return arg3 == 0 && arg2 > 0 && arg2 <= int.MaxValue / 35
            && StartClosingDoor(sim, line, tag, speed / 8.0, arg2 * 35 / 8);
    }

    internal static bool? ExecuteStairSpecial(AuthoritySimulation sim, int special, int tag, int speed, int step,
        int arg3, int arg4, LevelLine? line = null)
    {
        if (special is not (26 or 27 or 31 or 32 or 204 or 217 or 270 or 271 or 272 or 273)) return null;
        if (tag == 0 && line is null) return false;
        if (special == 204)
        {
            var started = StairActions.Start(sim, line, tag, speed / 8.0, step,
                down: (arg3 & 1) == 0, reset: arg4, ignoreTexture: (arg3 & 2) != 0);
            if (started && line is { Special: 204, Repeat: true }) line.Arg3 ^= 1;
            return started;
        }
        var sync = special is 31 or 32 or 271 or 272;
        return StairActions.Start(sim, line, tag, speed / 8.0, step, down: special is 26 or 31 or 270 or 272,
            crush: special == 273, sync: sync, useSpecials: special is 26 or 27 or 31 or 32, reset: sync ? arg3 : arg4, delay: sync ? 0 : arg3);
    }

    internal static bool? ExecuteFloorSpecial(AuthoritySimulation sim, int special, int tag, int speed, int arg2,
        int arg3, int arg4, LevelLine? line = null)
    {
        if (special is not (20 or 21 or 22 or 23 or 24 or 25 or 28 or 35 or 36 or 37 or 46 or 62 or 66 or 67 or 68 or 99 or 238 or 239 or 242 or 256 or 257 or 258 or 259 or 260 or 275 or 279 or ScrollFloor or ScrollCeiling)) return null;
        if (special is ScrollFloor or ScrollCeiling)
            return ScrollActions.ExecuteSpecial(sim, special, tag, speed, arg2, arg3, line);
        if (tag == 0 && line is null) return false;
        if (special == 239)
        {
            LevelSector? model = null;
            if (line is not null)
            {
                if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count) return false;
                var front = sim.Level.Sides[line.SideFront].Sector;
                if ((uint)front >= (uint)sim.Level.Sectors.Count) return false;
                model = sim.Level.Sectors[front];
            }
            return StartMapFloor(sim, line, tag, FloorTarget.RaiseBy, speed / 8.0, arg2,
                changeModel: model, clearSpecial: line is null);
        }
        if (special == 275)
        {
            var targets = TargetSectors(sim, line, tag, backSide: true).ToHashSet();
            foreach (var motion in sim.Motions.Where(motion => motion.Kind == MotionKind.Stair
                && motion.SectorIndex != motion.StairSeedIndex && !motion.Completed && targets.Contains(motion.SectorIndex)))
            {
                // Destroying a native stair mover bypasses its normal lock-release path.
                motion.Completed = true;
                motion.StairStopped = true;
            }
            sim.Motions.RemoveAll(motion => !motion.MovesCeiling && (motion.Kind != MotionKind.Stair
                || !motion.Completed && motion.SectorIndex == motion.StairSeedIndex)
                && targets.Contains(motion.SectorIndex));
            return true;
        }
        if (special == 46)
        {
            var targets = TargetSectors(sim, line, tag, backSide: true).ToHashSet();
            sim.Motions.RemoveAll(motion => motion.FloorCrushStopEligible && targets.Contains(motion.SectorIndex));
            return true;
        }
        if (special is 28 or 99)
            return StartMapFloor(sim, line, tag, special == 28 ? FloorTarget.RaiseCeiling : FloorTarget.DoomCrushCeiling,
                speed / 8.0, 8, crush: Math.Max(0, arg2), crushWithoutDamage: arg2 == 0, floorCrushStopEligible: special == 28,
                stopOnCrush: arg3 == 2 || arg3 != 1 && sim.Compat.HasFlag(CompatSurface.HexenCrushDefaults));
        if (special == 257)
            return arg2 == 0 && StartMapFloor(sim, line, tag, FloorTarget.RaiseLowest, 2,
                crush: Math.Max(0, arg3), stopOnCrush: true);
        if (special == 260)
            return speed == 0 && StartMapFloor(sim, line, tag, FloorTarget.LowerCeiling, 0, arg3,
                crush: Math.Max(0, arg2), stopOnCrush: true);
        if (special is 238 or 258 or 259)
            return arg2 == 0 && StartMapFloor(sim, line, tag,
                special == 238 ? FloorTarget.RaiseLowestCeiling : special == 258 ? FloorTarget.LowerLowestCeiling : FloorTarget.RaiseCeiling,
                speed / 8.0, special == 259 ? arg4 : 0,
                crush: special == 258 ? 0 : Math.Max(0, arg3), stopOnCrush: true);
        if (special == 242)
            return StartMapFloor(sim, line, tag, FloorTarget.LowerHighest, speed / 8.0,
                highestAdjustment: (double)arg2 - 128, adjustEqualHighest: arg3 == 1);
        if (special is 24 or 256)
            return arg2 == 0 && StartMapFloor(sim, line, tag,
                special == 24 ? FloorTarget.RaiseHighest : FloorTarget.LowerHighest, speed / 8.0,
                crush: special == 24 ? Math.Max(0, arg3) : 0, stopOnCrush: special == 24);
        if (special == 279)
            return StartMapFloor(sim, line, tag, FloorTarget.Absolute, speed / 8.0, arg2,
                crush: arg3 > 0 ? arg3 - 1 : 0, crushWithoutDamage: arg3 == 1,
                stopOnCrush: arg4 == 2 || arg4 != 1 && sim.Compat.HasFlag(CompatSurface.HexenCrushDefaults));
        if (special is 37 or 68)
        {
            var height = (long)arg2 * (special == 68 ? 8 : 1) * (arg3 != 0 ? -1 : 1);
            return arg4 == 0 && height >= int.MinValue && height <= int.MaxValue
                && StartMapFloor(sim, line, tag, FloorTarget.Absolute, speed / 8.0, (int)height);
        }
        if (special is 35 or 36 or 66 or 67)
            return arg3 == 0 && arg2 >= 0 && arg2 <= int.MaxValue / 8
                && StartMapFloor(sim, line, tag, special is 35 or 67 ? FloorTarget.RaiseBy : FloorTarget.LowerBy,
                    special is 66 or 67 ? arg2 * 8 : speed / 8.0, arg2 * 8,
                    crush: special is 35 or 67 ? Math.Max(0, arg4) : 0,
                    stopOnCrush: special is 35 or 67);
        return special switch
        {
            20 => arg3 == 0 && StartMapFloor(sim, line, tag, FloorTarget.LowerBy, speed / 8.0, arg2),
            21 => arg2 == 0 && StartMapFloor(sim, line, tag, FloorTarget.Lowest, speed / 8.0),
            22 => arg2 == 0 && StartMapFloor(sim, line, tag, FloorTarget.NextLower, speed / 8.0),
            23 => arg3 == 0 && StartMapFloor(sim, line, tag, FloorTarget.RaiseBy, speed / 8.0, arg2,
                crush: Math.Max(0, arg4), stopOnCrush: true),
            25 => arg2 == 0 && StartMapFloor(sim, line, tag, FloorTarget.NextHigher, speed / 8.0,
                crush: Math.Max(0, arg3), stopOnCrush: true),
            _ => arg2 >= 0 && StartMapFloor(sim, line, tag, FloorTarget.Lowest, speed / 8.0, lift: true, delay: arg2, lip: 8),
        };
    }

    internal static bool? ExecuteCeilingSpecial(AuthoritySimulation sim, int special, int tag, int speed, int amount,
        int change, int crush, LevelLine? line = null)
    {
        if (special is not (40 or 41 or 42 or 43 or 44 or 45 or 47 or 69 or 97 or 104 or 168 or 192 or 193 or 194 or 195 or 196 or 197 or 198 or 199 or 252 or 253 or 254 or 255 or 262 or 263 or 264 or 265 or 266 or 267 or 276 or 280)) return null;
        if (special == 44) return CeilingActions.Stop(sim, line, tag,
            remove: speed == 2 || speed != 1 && sim.Compat.HasFlag(CompatSurface.HexenCrushDefaults));
        if (tag == 0 && line is null) return false;
        if (special == 276) return CeilingActions.StopAll(sim, line, tag);
        if (special is 263 or 267)
            return speed == 0 && CeilingActions.Start(sim, line, tag,
                special == 263 ? CeilingTarget.LowerHighest : CeilingTarget.RaiseFloor, speed: 2,
                amount: special == 267 ? change : 0, crush: amount > 0 ? amount : -1);
        if (special is 192 or 265 or 266)
            return amount == 0 && CeilingActions.Start(sim, line, tag,
                special == 192 ? CeilingTarget.LowerHighestFloor : special == 265 ? CeilingTarget.RaiseLowest : CeilingTarget.RaiseHighestFloor,
                speed: speed / 8.0, amount: special == 192 ? crush : 0,
                crush: special == 192 && change > 0 ? change : -1);
        if (special is 47 or 69)
        {
            var height = (long)amount * (special == 69 ? 8 : 1) * (change != 0 ? -1 : 1);
            return crush == 0 && height >= int.MinValue && height <= int.MaxValue
                && CeilingActions.Start(sim, line, tag, CeilingTarget.Absolute, speed: speed / 8.0, amount: (int)height);
        }
        if (special == 280)
            return CeilingActions.Start(sim, line, tag, CeilingTarget.Absolute, speed: speed / 8.0,
                amount: amount, crush: change > 0 ? change : -1, stop: CeilingHexenMode(sim, crush));
        if (special is 193 or 194)
            return change == 0 && amount >= 0 && amount <= int.MaxValue / 8
                && CeilingActions.Start(sim, line, tag,
                    special == 193 ? CeilingTarget.LowerInstant : CeilingTarget.RaiseInstant,
                    amount: amount * 8, crush: special == 193 && crush > 0 ? crush : -1);
        if (special is 252 or 253 or 264)
            return amount == 0 && CeilingActions.Start(sim, line, tag,
                special == 252 ? CeilingTarget.NextHigher : special == 253 ? CeilingTarget.Lowest : CeilingTarget.NextLower,
                speed: speed / 8.0, crush: special != 252 && change > 0 ? change : -1);
        if (special is 198 or 199)
            return change == 0 && amount >= 0 && amount <= int.MaxValue / 8
                && CeilingActions.Start(sim, line, tag,
                    special == 198 ? CeilingTarget.RaiseBy : CeilingTarget.LowerBy,
                    speed: speed / 8.0, amount: amount * 8);
        if (special == 97)
            return CeilingActions.Start(sim, line, tag, CeilingTarget.Crush,
                speed: speed / 8.0, amount: change, crush: amount,
                slow: CeilingSlowdownMode(sim, crush, speed == 8), stop: CeilingHexenMode(sim, crush));
        if (special is 195 or 196 or 197 or 255)
        {
            var loop = special is 196 or 197;
            return CeilingActions.Start(sim, line, tag, CeilingTarget.Crush,
                speed: speed / 8.0, upSpeed: amount / 8.0, amount: 0, crush: change,
                returnToStart: true, loop: loop,
                slow: loop && CeilingSlowdownMode(sim, crush, speed == 8 && amount == 8), stop: CeilingHexenMode(sim, crush));
        }
        if (special is 104 or 168)
            return CeilingActions.Start(sim, line, tag, CeilingTarget.Crush,
                speed: amount / 8.0, upSpeed: amount / 8.0, amount: speed, crush: change,
                returnToStart: true, loop: true,
                slow: CeilingSlowdownMode(sim, crush, amount == 8), stop: CeilingHexenMode(sim, crush));
        if (special == 254)
            return amount == 0 && CeilingActions.Start(sim, line, tag, CeilingTarget.Floor,
                speed: speed / 8.0, amount: crush, crush: change > 0 ? change : -1);
        if (special == 262)
            return amount == 0 && CeilingActions.Start(sim, line, tag, CeilingTarget.Highest, speed: speed / 8.0);
        if (special is 42 or 43 or 45)
            return CeilingActions.Start(sim, line, tag, CeilingTarget.Crush,
                speed: speed / 8.0, upSpeed: special == 43 ? speed / 8.0 : speed / 16.0,
                crush: amount, returnToStart: special != 43, loop: special == 42,
                slow: special != 45 && CeilingSlowdownMode(sim, change, special == 43 && speed == 8), stop: CeilingHexenMode(sim, change));
        return change == 0 && CeilingActions.Start(sim, line, tag,
            special == 40 ? CeilingTarget.LowerBy : CeilingTarget.RaiseBy,
            speed: speed / 8.0, amount: amount, crush: crush > 0 ? crush : -1);
    }

    private static bool CeilingHexenMode(AuthoritySimulation sim, int mode) =>
        mode == 2 || (mode < 1 || mode > 3) && sim.Compat.HasFlag(CompatSurface.HexenCrushDefaults);

    private static bool CeilingSlowdownMode(AuthoritySimulation sim, int mode, bool defaultSlowdown) =>
        mode == 3 || (mode < 1 || mode > 3) && defaultSlowdown && !CeilingHexenMode(sim, mode);

    private static bool Exit(AuthoritySimulation sim, bool secret, Actor? activator)
    {
        if (activator is not null && !sim.DamageExitAllowed)
        {
            ActorDamage.Apply(activator, 1000000, activator,
                DamageFlags.BypassArmor | DamageFlags.BypassInvulnerability);
            return false;
        }
        sim.MarkExited(secret);
        return true;
    }

    internal static bool? ExecuteExitSpecial(AuthoritySimulation sim, int special, Actor? activator) =>
        special is 243 or 244 ? Exit(sim, special == 244, activator) : null;

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
            case 130:
            case 131:
                return ThingActivation.Execute(sim, activator, tag, special == 130);
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
                return Exit(sim, false, activator);
            case Teleport:
                return TeleportActivator(sim, activator);
            case AcsExecute:
                var script = line != null && line.Arg0 != 0 ? line.Arg0 : tag;
                return sim.Acs.Enqueue(script, [], activator, line);
            case CompatSurfaceRules.Id24SecretExit:
                if (!sim.Compat.HasFlag(CompatSurface.Id24))
                    return false;
                return Exit(sim, true, activator);
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
            if (motion.Kind == MotionKind.Stair && !motion.Closing)
            {
                if (motion.ResetCountdown > 0 && --motion.ResetCountdown == 0)
                {
                    motion.Closing = true;
                    motion.WaitingForReset = false;
                }
                // Native evaluates the old build/wait state's pause even on the reset transition tick.
                if (motion.StairPause > 0) { motion.StairPause--; continue; }
                if (motion.StepCountdown > 0 && --motion.StepCountdown == 0)
                {
                    motion.StairPause = motion.Delay;
                    motion.StepCountdown = motion.StepPeriod;
                }
            }
            if (motion.WaitingForReset) continue;
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
                if (motion.Wait == 0 && motion.OpenAfterClose)
                    motion.Closing = false;
                continue;
            }

            var door = motion.Kind == MotionKind.Door;
            var floor = door ? sim.Ceilings[motion.SectorIndex] : sim.Floors[motion.SectorIndex];
            var target = door ? (motion.Closing ? motion.ClosedCeiling : motion.TargetCeiling)
                : motion.Closing
                    ? motion.ClosedFloor
                    : motion.TargetFloor;
            var direction = motion.Kind == MotionKind.Stair && motion.Closing ? -motion.FloorDirection : motion.FloorDirection;
            target = door ? Math.Max(target, sim.Floors[motion.SectorIndex])
                : direction < 0 ? target : Math.Min(target, sim.Ceilings[motion.SectorIndex]);
            if (!door && direction != 0)
                floor = direction > 0 ? Math.Min(target, floor + motion.Speed) : Math.Max(target, floor - motion.Speed);
            else if (floor < target)
                floor = Math.Min(target, floor + motion.Speed);
            else if (floor > target)
                floor = Math.Max(target, floor - motion.Speed);
            var reachedDestination = floor == target;
            if (door && motion.Closing && sim.Actors.Any(actor => actor.BlocksActors
                && actor.SectorIndex == motion.SectorIndex && actor.Z.ToDouble() + actor.Height.ToDouble() > floor))
            {
                if (!reachedDestination)
                {
                    if (!motion.CloseOnly) motion.Closing = false;
                    continue;
                }
                floor = sim.Ceilings[motion.SectorIndex];
            }
            if (!door && floor > sim.Floors[motion.SectorIndex])
            {
                var blocked = sim.Actors.Where(actor => actor.BlocksActors && actor.SectorIndex == motion.SectorIndex
                    && Math.Max(actor.Z.ToDouble(), floor) + actor.Height.ToDouble() > sim.Ceilings[motion.SectorIndex]).ToArray();
                if (blocked.Length > 0)
                {
                    if (motion.CrushDamage > 0 || motion.CrushWithoutDamage)
                    {
                        if ((sim.Thinkers.Clock.Tic & 3) == 0)
                        {
                            var tic = sim.Thinkers.Clock.Tic;
                            foreach (var actor in blocked)
                            {
                                actor.MarkMoverCrush(tic);
                                ActorDamage.Apply(actor, motion.CrushDamage);
                                ActorPhysics.CrushStandingRiders(sim, actor, motion.CrushDamage, tic);
                            }
                        }
                        if (motion.StopOnCrush)
                        {
                            if (!reachedDestination) continue;
                            floor = sim.Floors[motion.SectorIndex];
                        }
                        else if (reachedDestination
                            && motion.Speed > Math.Abs(target - sim.Floors[motion.SectorIndex]))
                            floor = sim.Floors[motion.SectorIndex];
                    }
                    else
                    {
                        if (!reachedDestination)
                        {
                            if (motion.Kind == MotionKind.Lift && motion.Closing)
                            { motion.Closing = false; motion.Wait = 0; }
                            continue;
                        }
                        floor = sim.Floors[motion.SectorIndex];
                    }
                }
            }
            if (door) sim.Ceilings[motion.SectorIndex] = floor;
            else sim.Floors[motion.SectorIndex] = floor;

            if (!reachedDestination)
                continue;
            if (motion.Closing && motion.OpenAfterClose)
            {
                motion.Wait = motion.Delay;
                continue;
            }
            if (!motion.Closing && motion.CloseAfterOpen)
            {
                motion.Wait = motion.Delay;
                if (motion.Wait == 0) motion.Closing = true;
                continue;
            }

            if (motion.Kind == MotionKind.Stair)
            {
                // A built step retains plane ownership until its reset countdown and return finish.
                if (!motion.Closing && motion.ResetCountdown > 0) { motion.WaitingForReset = true; continue; }
                motion.Completed = true;
            }
            else sim.Motions.RemoveAt(i);
        }
        // Completed records retain only the stair retrigger lock, not floor ownership.
        // Keep the entire stair chain locked until its last step finishes.
        var completedStairs = sim.Motions.Where(motion => motion.Kind == MotionKind.Stair)
            .GroupBy(motion => motion.StairGroup).Where(group => group.All(motion => motion.Completed && !motion.StairStopped))
            .Select(group => group.Key).ToHashSet();
        sim.Motions.RemoveAll(motion => motion.Kind == MotionKind.Stair && completedStairs.Contains(motion.StairGroup));
    }

    private static bool StartClosingDoor(AuthoritySimulation sim, LevelLine? line, int tag, double speed, int? reopenDelay = null)
    {
        if (speed <= 0) return false;
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && motion.MovesCeiling)) continue;
            sim.Motions.Add(new SectorMotion
            {
                Kind = MotionKind.Door, SectorIndex = sector, Closing = true, CloseOnly = !reopenDelay.HasValue,
                OpenAfterClose = reopenDelay.HasValue, Delay = reopenDelay ?? 0,
                ClosedCeiling = sim.Floors[sector], TargetCeiling = sim.Ceilings[sector], Speed = speed,
            });
            started = true;
        }
        return started;
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
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && !motion.MovesCeiling && !motion.Completed))
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

    private enum FloorTarget { Lowest, NextHigher, RaiseBy, LowerBy, CrushCeiling, NextLower, Absolute, RaiseHighest, LowerHighest, RaiseLowestCeiling, LowerLowestCeiling, RaiseCeiling, RaiseLowest, LowerCeiling, DoomCrushCeiling }

    private static bool StartMapFloor(AuthoritySimulation sim, LevelLine? line, int tag, FloorTarget kind,
        double speed = 1, int amount = 0, bool lift = false, int delay = 105, int lip = 0, int crush = 0, bool stopOnCrush = false,
        bool crushWithoutDamage = false, double highestAdjustment = 0, bool adjustEqualHighest = false, bool floorCrushStopEligible = false, LevelSector? changeModel = null, bool clearSpecial = false)
    {
        if (amount < 0 && kind is not (FloorTarget.Absolute or FloorTarget.RaiseCeiling or FloorTarget.LowerCeiling) || speed < 0 || lift && speed == 0) return false;
        var started = false;
        foreach (var sector in TargetSectors(sim, line, tag, backSide: true))
        {
            if (sim.Motions.Any(motion => motion.SectorIndex == sector && !motion.MovesCeiling && !motion.Completed)) continue;
            var current = sim.Floors[sector];
            var neighbors = NeighborSectors(sim, sector).Select(index => sim.Floors[index]).ToArray();
            double destination = kind switch
            {
                FloorTarget.Lowest => lift ? Math.Min(current, neighbors.DefaultIfEmpty(current).Min() + lip)
                    : neighbors.DefaultIfEmpty(current).Min(),
                FloorTarget.RaiseLowest => neighbors.DefaultIfEmpty(current).Min(),
                FloorTarget.NextHigher => neighbors.Where(height => height > current).DefaultIfEmpty(current).Min(),
                FloorTarget.RaiseHighest or FloorTarget.LowerHighest => neighbors.DefaultIfEmpty(current).Max(),
                FloorTarget.NextLower => neighbors.Where(height => height < current).DefaultIfEmpty(current).Max(),
                FloorTarget.RaiseBy => current + amount,
                FloorTarget.LowerBy => current - amount,
                FloorTarget.Absolute => amount,
                FloorTarget.RaiseCeiling or FloorTarget.LowerCeiling => sim.Ceilings[sector] - (double)amount,
                FloorTarget.RaiseLowestCeiling => Math.Min(sim.Ceilings[sector],
                    NeighborSectors(sim, sector).Select(index => sim.Ceilings[index]).DefaultIfEmpty(sim.Ceilings[sector]).Min()),
                FloorTarget.LowerLowestCeiling => NeighborSectors(sim, sector).Select(index => sim.Ceilings[index])
                    .DefaultIfEmpty(sim.Ceilings[sector]).Min(),
                _ => sim.Ceilings[sector] - 8L,
            };
            if (kind == FloorTarget.DoomCrushCeiling)
            {
                destination = NeighborSectors(sim, sector).Select(index => sim.Ceilings[index])
                    .DefaultIfEmpty(sim.Ceilings[sector]).Min() - 8;
                if (destination > sim.Ceilings[sector]) destination = sim.Ceilings[sector] - 8;
            }
            if (kind == FloorTarget.LowerHighest && (adjustEqualHighest || destination != current))
                destination += highestAdjustment;
            var target = lift ? Math.Clamp(destination, short.MinValue, sim.Ceilings[sector]) : Math.Max(destination, short.MinValue);
            // Native floor creation succeeds at an equal target and owns the plane
            // until its first tick. Lift creation has a separate native lifecycle.
            if (target == current && lift || kind == FloorTarget.CrushCeiling && target < current) continue;
            if (changeModel is not null)
            {
                sim.Level.Sectors[sector].FloorPic = changeModel.FloorPic;
                sim.Level.Sectors[sector].Special = changeModel.Special;
                sim.Level.Sectors[sector].DamageAmount = changeModel.DamageAmount;
                sim.Level.Sectors[sector].DamageType = changeModel.DamageType;
                sim.Level.Sectors[sector].DamageInterval = changeModel.DamageInterval;
                sim.Level.Sectors[sector].Leakiness = changeModel.Leakiness;
                sim.Level.Sectors[sector].DamageEndsGodMode = changeModel.DamageEndsGodMode;
                sim.Level.Sectors[sector].DamageEndsLevel = changeModel.DamageEndsLevel;
            }
            else if (clearSpecial)
            {
                sim.Level.Sectors[sector].Special = 0;
                sim.Level.Sectors[sector].DamageAmount = 0;
                sim.Level.Sectors[sector].DamageType = "None";
                sim.Level.Sectors[sector].DamageInterval = 0;
                sim.Level.Sectors[sector].Leakiness = 0;
                sim.Level.Sectors[sector].DamageEndsGodMode = false;
                sim.Level.Sectors[sector].DamageEndsLevel = false;
            }
            sim.Motions.Add(new SectorMotion {
                SectorIndex = sector, ClosedFloor = current, TargetFloor = target,
                FloorDirection = lift ? 0 : kind switch {
                    FloorTarget.Lowest or FloorTarget.NextLower or FloorTarget.LowerBy or FloorTarget.LowerHighest or FloorTarget.LowerLowestCeiling or FloorTarget.LowerCeiling => -1,
                    FloorTarget.Absolute => destination < current ? -1 : 1,
                    _ => 1,
                },
                Kind = lift ? MotionKind.Lift : MotionKind.Floor, CloseAfterOpen = lift,
                Speed = speed, Delay = delay, CrushDamage = crush, StopOnCrush = stopOnCrush, CrushWithoutDamage = crushWithoutDamage, FloorCrushStopEligible = floorCrushStopEligible,
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

    internal static bool? ExecuteTeleportSpecial(AuthoritySimulation sim, int special, int tid, int sectorTag,
        Actor? activator, bool backSide) => special is 70 or 154
        ? !backSide && (tid != 0 || sectorTag != 0) && activator is { Destroyed: false }
            && TeleportActivator(sim, activator, tid, byThingId: true, sectorTag: sectorTag, keepVelocity: special == 154)
        : null;

    private static bool TeleportActivator(AuthoritySimulation sim, Actor? activator, int target = 0, bool byThingId = false, int sectorTag = 0, bool keepVelocity = false)
    {
        if (activator == null || activator.NoTeleport)
            return false;
        var destinations = sim.Actors.Where(actor => actor.DoomEdNum == TeleportDestType && !actor.Destroyed);
        if (byThingId && target != 0)
            destinations = destinations.Where(actor => actor.ThingId == target);
        var tag = byThingId ? sectorTag : target;
        if (tag != 0)
        {
            destinations = destinations.Where(actor => (uint)actor.SectorIndex < (uint)sim.Level.Sectors.Count
                && sim.Level.Sectors[actor.SectorIndex].MatchesTag(tag));
            // Native zero-TID searches visit tagged sectors in sector order first.
            if (!byThingId || target == 0)
                destinations = destinations.OrderBy(actor => actor.SectorIndex);
        }
        var dest = destinations.FirstOrDefault();
        if (dest == null)
            return false;
        var aboveFloor = activator.Z.ToDouble() - sim.FloorOf(activator.SectorIndex);
        var missileSpeed = activator.VelXYToSpeed();
        var verticalVelocity = activator.VelocityZ;
        activator.X = dest.X;
        activator.Y = dest.Y;
        activator.Angle = dest.Angle;
        if (!keepVelocity) activator.VelocityX = activator.VelocityY = activator.VelocityZ = default;
        ActorPhysics.PlaceOnFloor(sim, activator);
        if (activator is ProjectileActor || activator is PlayerPawn && activator.NoGravity && aboveFloor != 0)
        {
            var floor = sim.FloorOf(activator.SectorIndex);
            var ceiling = sim.CeilingOf(activator.SectorIndex) - activator.Height.ToDouble();
            activator.Z = Fixed.FromDouble(Math.Min(floor + aboveFloor, ceiling));
            activator.OnGround = activator.Z.ToDouble() <= floor;
            activator.OnMobj = false;
        }
        if (activator is ProjectileActor)
        {
            activator.VelFromAngle(missileSpeed);
            activator.VelocityZ = verticalVelocity;
        }
        if (activator is PlayerPawn && !keepVelocity) activator.ReactionTime = 18;
        activator.ClearInterpolation();
        return true;
    }

    internal static IEnumerable<int> TargetSectors(AuthoritySimulation sim, LevelLine? line, int tag, bool backSide = false)
    {
        if (tag != 0)
        {
            for (var i = 0; i < sim.Level.Sectors.Count; i++)
            {
                if (sim.Level.Sectors[i].MatchesTag(tag))
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
