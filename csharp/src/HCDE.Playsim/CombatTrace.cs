using HCDE.MapLoader;

namespace HCDE.Playsim;

[Flags]
public enum ActorLineTraceFlags
{
    AbsolutePosition = 1, AbsoluteOffset = 2, ThroughSpecies = 4, ThroughActors = 8,
    ThroughBlock = 16, ThroughHitscan = 32, NoSky = 64, AllActors = 128, SolidActors = 256, BlockUse = 512, BlockSelf = 1024,
}

public enum ActorLineTraceHitType { None = 0, Floor = 1, Ceiling = 2, Wall = 3, Actor = 4 }

[Flags]
public enum ActorSightFlags { IgnoreVisibility = 1, SeePastShootableLines = 2, SeePastBlockEverything = 4 }

/// <summary>
/// Actor-box combat tracing with pitch and sector openings.
/// Slopes, portals and 3D floors remain outside this subset of P_LineAttack.
/// </summary>
public static class CombatTrace
{
    private const double Epsilon = 1e-9;
    private const double EqualEpsilon = 1.0 / 65536;

    private const int MfShootable = 0x00000004;

    /// <summary>Flat-sector P_LineTrace subset with explicit source-relative or absolute offsets.</summary>
    public static LineAttackHit LineTrace(AuthoritySimulation sim, Actor source, BamAngle angle, double range,
        BamAngle pitch = default, ActorLineTraceFlags flags = 0, double offsetZ = 0, double offsetForward = 0, double offsetSide = 0)
    {
        ArgumentNullException.ThrowIfNull(sim); ArgumentNullException.ThrowIfNull(source);
        const ActorLineTraceFlags supported = ActorLineTraceFlags.AbsolutePosition | ActorLineTraceFlags.AbsoluteOffset
            | ActorLineTraceFlags.ThroughSpecies | ActorLineTraceFlags.ThroughActors | ActorLineTraceFlags.ThroughBlock
            | ActorLineTraceFlags.ThroughHitscan | ActorLineTraceFlags.AllActors | ActorLineTraceFlags.SolidActors | ActorLineTraceFlags.BlockUse
            | ActorLineTraceFlags.BlockSelf | ActorLineTraceFlags.NoSky;
        if ((flags & ~supported) != 0) throw new NotSupportedException("The requested native line-trace flags are not yet supported.");
        if (!double.IsFinite(range) || !double.IsFinite(offsetZ) || !double.IsFinite(offsetForward) || !double.IsFinite(offsetSide))
            throw new ArgumentOutOfRangeException(nameof(range), "Trace range and offsets must be finite.");
        var x = source.X.ToDouble(); var y = source.Y.ToDouble(); var z = source.Z.ToDouble() - source.FloorClip + offsetZ;
        if (flags.HasFlag(ActorLineTraceFlags.AbsolutePosition)) { x = offsetForward; y = offsetSide; z = offsetZ; }
        else if (flags.HasFlag(ActorLineTraceFlags.AbsoluteOffset)) { x += offsetForward; y += offsetSide; }
        else
        {
            var radians = angle.ToDegrees() * Math.PI / 180;
            x += offsetForward * Math.Cos(radians) + offsetSide * Math.Sin(radians);
            y += offsetForward * Math.Sin(radians) - offsetSide * Math.Cos(radians);
        }
        var actors = flags.HasFlag(ActorLineTraceFlags.AllActors) ? -1 : MfShootable;
        if (flags.HasFlag(ActorLineTraceFlags.SolidActors)) actors |= 2;
        var walls = LevelLine.BlockEverythingFlag | LevelLine.BlockHitscanFlag;
        if (flags.HasFlag(ActorLineTraceFlags.BlockUse)) walls |= LevelLine.BlockUseFlag;
        if (flags.HasFlag(ActorLineTraceFlags.BlockSelf))
        {
            var projectile = source is ProjectileActor;
            var notBlocked = source.NoBlockMonsters || source.Friendly && sim.Compat.HasFlag(CompatSurface.NoBlockFriends);
            if (projectile) walls |= LevelLine.BlockProjectileFlag;
            if (!projectile || source.BlockAsPlayer) walls |= LevelLine.BlockingFlag;
            if (!notBlocked) walls |= LevelLine.BlockMonstersFlag;
            if (source is PlayerPawn || source.BlockAsPlayer) walls |= LevelLine.BlockingFlag | LevelLine.BlockPlayersFlag;
            if (source.Floating) walls |= LevelLine.BlockFloatersFlag;
        }
        if (flags.HasFlag(ActorLineTraceFlags.ThroughBlock)) walls &= ~LevelLine.BlockEverythingFlag;
        if (flags.HasFlag(ActorLineTraceFlags.ThroughHitscan)) walls &= ~LevelLine.BlockHitscanFlag;
        var hit = TraceCore(sim, source, angle, pitch, range, actors, walls, false, true, true, x, y, z,
            flags.HasFlag(ActorLineTraceFlags.ThroughActors), flags.HasFlag(ActorLineTraceFlags.ThroughSpecies),
            flags.HasFlag(ActorLineTraceFlags.NoSky));
        return DescribeLineTrace(sim, hit, angle, pitch, x, y, z);
    }

    private static LineAttackHit DescribeLineTrace(AuthoritySimulation sim, LineAttackHit hit, BamAngle angle, BamAngle pitch,
        double x, double y, double z)
    {
        var yaw = angle.ToDegrees() * Math.PI / 180; var elevation = pitch.ToDegrees() * Math.PI / 180;
        var horizontal = Math.Cos(elevation); if (Math.Abs(horizontal) < 1e-12) horizontal = 0;
        var direction = (X: horizontal * Math.Cos(yaw), Y: horizontal * Math.Sin(yaw), Z: -Math.Sin(elevation));
        var distance = (hit.X - x) * direction.X + (hit.Y - y) * direction.Y + (hit.Z - z) * direction.Z;
        var type = !hit.Hit ? ActorLineTraceHitType.None : hit.Victim is not null ? ActorLineTraceHitType.Actor
            : hit.Wall is not null ? ActorLineTraceHitType.Wall
            : hit.PlanePart == 0 ? ActorLineTraceHitType.Floor : ActorLineTraceHitType.Ceiling;
        var sector = hit.PlaneSector >= 0 ? hit.PlaneSector : ActorPhysics.SectorAt(sim.Level, hit.X, hit.Y);
        var part = -1; string? texture = null;
        if (hit.Wall is { } wall)
        {
            var sideIndex = hit.Side == 0 ? wall.SideFront : wall.SideBack;
            var farSideIndex = hit.Side == 0 ? wall.SideBack : wall.SideFront;
            part = 1;
            if ((uint)sideIndex < (uint)sim.Level.Sides.Count)
            {
                var side = sim.Level.Sides[sideIndex]; sector = side.Sector;
                if ((uint)farSideIndex < (uint)sim.Level.Sides.Count)
                {
                    var farSector = sim.Level.Sides[farSideIndex].Sector;
                    if ((uint)farSector < (uint)sim.Level.Sectors.Count)
                        part = hit.Z <= sim.FloorOf(farSector) ? 2 : hit.Z >= sim.CeilingOf(farSector) ? 0 : 1;
                }
                texture = part == 0 ? side.TopTexture : part == 2 ? side.BottomTexture : side.MidTexture;
            }
        }
        else if (hit.PlaneSector >= 0)
            texture = hit.PlanePart == 0 ? sim.Level.Sectors[hit.PlaneSector].FloorPic : sim.Level.Sectors[hit.PlaneSector].CeilingPic;
        return hit with { Distance = distance, Direction = direction, HitType = type, HitSector = sector, LinePart = part, HitTexture = texture };
    }

    private static bool ClipAxis(double lower, double upper, double direction, ref double start, ref double end)
    {
        if (direction == 0) return lower <= 0 && upper >= 0;
        var first = lower / direction;
        var last = upper / direction;
        start = Math.Max(start, Math.Min(first, last));
        end = Math.Min(end, Math.Max(first, last));
        return start <= end;
    }

    /// <summary>Native <c>P_LinePickActor</c> subset for ACS <c>PickActor</c>.</summary>
    public static Actor? PickActor(
        AuthoritySimulation sim,
        Actor source,
        BamAngle angle,
        BamAngle pitch,
        double range,
        int actorMask = MfShootable,
        int wallMask = LevelLine.BlockEverythingFlag | LevelLine.BlockHitscanFlag)
    {
        if (!double.IsFinite(range) || source.IsDead || source.Destroyed)
            return null;
        return TraceLineAttack(sim, source, angle, pitch, range, actorMask, wallMask, requireDamageable: false).Victim;
    }

    public static Actor? FindTarget(AuthoritySimulation sim, Actor source, double range, double yawOffset = 0, double pitchOffset = 0)
    {
        if (!double.IsFinite(yawOffset) || !double.IsFinite(pitchOffset))
            return null;
        var pitch = (source is PlayerPawn player ? player.PitchDegrees : 0) + pitchOffset;
        return TraceLineAttack(sim, source, BamAngle.FromDegrees(source.Angle.ToDegrees() + yawOffset),
            BamAngle.FromDegrees(pitch), range).Victim;
    }
    private static bool MatchesActorMask(Actor actor, int actorMask)
    {
        if (actorMask == -1) return true;
        var flags = (actor.Solid ? 0x2 : 0) | (actor.Shootable ? MfShootable : 0)
            | (actor.NoGravity ? 0x200 : 0) | (actor.Floating ? 0x4000 : 0)
            | (actor is ProjectileActor ? 0x10000 : 0);
        return (actorMask & flags) != 0;
    }

    internal static bool BlocksPick(AuthoritySimulation sim, LevelLine line, double z, int wallMask, int impactSide,
        double vertical, double startZ, double range, double enterDistance)
    {
        if (line.OneSided)
            return true;
        var sides = sim.Level.Sides;
        if ((uint)line.SideFront >= (uint)sides.Count || (uint)line.SideBack >= (uint)sides.Count)
            return true;
        var front = sides[line.SideFront].Sector;
        var back = sides[line.SideBack].Sector;
        if ((uint)front >= (uint)sim.Floors.Length || (uint)back >= (uint)sim.Floors.Length)
            return true;
        var near = impactSide == 0 ? front : back;
        var far = impactSide == 0 ? back : front;
        var sampleZ = vertical < 0 ? z - EqualEpsilon : z;
        if (z <= sim.Floors[near] || z >= sim.Ceilings[near])
        {
            var planeZ = z <= sim.Floors[near] ? sim.Floors[near] : sim.Ceilings[near];
            var planeDistance = vertical == 0 ? double.NaN : (planeZ - startZ) / vertical;
            if (planeDistance <= enterDistance + EqualEpsilon)
                return false;
            if (!(planeDistance > enterDistance && planeDistance < range))
                return sampleZ <= sim.Floors[far] || z >= sim.Ceilings[far];
        }
        return (line.Flags & wallMask) != 0 || z <= sim.Floors[near] || z >= sim.Ceilings[near]
            || sampleZ < sim.Floors[far] || z > sim.Ceilings[far];
    }

    internal static bool BlocksShot(AuthoritySimulation sim, LevelLine line, double z, int flag = LevelLine.BlockHitscanFlag,
        bool ignoreBlockEverything = false)
    {
        if (line.OneSided || (line.Flags & ((ignoreBlockEverything ? 0 : LevelLine.BlockEverythingFlag) | flag)) != 0)
            return true;
        // ML_BLOCKING restricts movement, not gunfire through an open two-sided line.
        var sides = sim.Level.Sides;
        if ((uint)line.SideFront >= (uint)sides.Count || (uint)line.SideBack >= (uint)sides.Count)
            return true;
        var front = sides[line.SideFront].Sector;
        var back = sides[line.SideBack].Sector;
        if ((uint)front >= (uint)sim.Floors.Length || (uint)back >= (uint)sim.Floors.Length)
            return true;
        return z < Math.Max(sim.Floors[front], sim.Floors[back]) || z >= Math.Min(sim.Ceilings[front], sim.Ceilings[back]);
    }

    internal static double RayLine(double x, double y, double dx, double dy, LevelLine line, double traversalScale = 1)
        => RaySegment(x, y, dx, dy, line.X1, line.Y1, line.X2, line.Y2, traversalScale);

    private static double RaySegment(double x, double y, double dx, double dy,
        double x1, double y1, double x2, double y2, double traversalScale)
    {
        var sx = x2 - x1;
        var sy = y2 - y1;
        var rx = x1 - x;
        var ry = y1 - y;
        // Native divline side tests use the full traversal delta and EQUAL_EPSILON.
        var firstSide = (ry * dx - rx * dy) * traversalScale > 1.0 / 65536;
        var secondSide = ((y2 - y) * dx - (x2 - x) * dy) * traversalScale > 1.0 / 65536;
        if (firstSide == secondSide) return double.PositiveInfinity;
        var denominator = dx * sy - dy * sx;
        if (denominator == 0)
            return double.PositiveInfinity;
        var distance = (rx * sy - ry * sx) / denominator;
        return distance >= 0
            ? distance : double.PositiveInfinity;
    }

    internal static double ActorBoxEntry(double rx, double ry, double radius, double dx, double dy, double range)
    {
        var fronts = 0;
        double Edge(double x1, double y1, double x2, double y2)
        {
            fronts++;
            return RaySegment(0, 0, dx, dy, x1, y1, x2, y2, range);
        }
        double distance;
        if (ry + radius <= 0 && double.IsFinite(distance = Edge(rx + radius, ry + radius, rx - radius, ry + radius))) return distance;
        if (rx + radius <= 0 && double.IsFinite(distance = Edge(rx + radius, ry - radius, rx + radius, ry + radius))) return distance;
        if (ry - radius >= 0 && double.IsFinite(distance = Edge(rx - radius, ry - radius, rx + radius, ry - radius))) return distance;
        if (rx - radius >= 0 && double.IsFinite(distance = Edge(rx - radius, ry + radius, rx - radius, ry - radius))) return distance;
        return fronts == 0 ? 0 : double.PositiveInfinity;
    }
    public readonly record struct LineAttackHit(bool Hit, Actor? Victim, double X, double Y, double Z)
    {
        public LevelLine? Wall { get; init; }
        public int Side { get; init; }
        public int PlaneSector { get; init; } = -1;
        public int PlanePart { get; init; } = -1;
        /// <summary>Additional P_LineTrace metadata, populated by LineTrace.</summary>
        public double Distance { get; init; }
        public (double X, double Y, double Z) Direction { get; init; }
        public ActorLineTraceHitType HitType { get; init; }
        public int HitSector { get; init; } = -1;
        public int LinePart { get; init; } = -1;
        public string? HitTexture { get; init; }
    }

    /// <summary>Native <c>P_LineAttack</c> subset: first actor, wall or flat sector plane within range.</summary>
    public static LineAttackHit TraceLineAttack(
        AuthoritySimulation sim,
        Actor source,
        BamAngle angle,
        BamAngle pitch,
        double range,
        int actorMask = MfShootable,
        int wallMask = LevelLine.BlockEverythingFlag | LevelLine.BlockHitscanFlag,
        bool requireDamageable = true,
        bool allowDestroyedSource = false,
        bool allowDeadSource = false)
        => TraceCore(sim, source, angle, pitch, range, actorMask, wallMask, requireDamageable, allowDestroyedSource,
            allowDeadSource, source.X.ToDouble(), source.Y.ToDouble(), source.AttackOriginZ());

    private static LineAttackHit TraceCore(AuthoritySimulation sim, Actor source, BamAngle angle, BamAngle pitch, double range,
        int actorMask, int wallMask, bool requireDamageable, bool allowDestroyedSource, bool allowDeadSource,
        double x, double y, double z, bool throughActors = false, bool throughSpecies = false, bool noSky = true)
    {
        if (!double.IsFinite(range) || source.IsDead && !allowDeadSource || source.Destroyed && !allowDestroyedSource)
            return default;

        var radians = angle.Raw * (Math.PI / 0x80000000u);
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        var pitchDegrees = pitch.ToDegrees();
        while (pitchDegrees > 180)
            pitchDegrees -= 360;
        while (pitchDegrees < -180)
            pitchDegrees += 360;
        if (!double.IsFinite(pitchDegrees))
            return default;
        var pitchRadians = pitchDegrees * Math.PI / 180;
        var horizontal = Math.Cos(pitchRadians);
        if (Math.Abs(horizontal) < 1e-12) horizontal = 0;
        dx *= horizontal;
        dy *= horizontal;
        var vertical = -Math.Sin(pitchRadians);
        var nativeVertical = vertical;
        var signedRange = range;
        var travelSign = range < 0 ? -1 : 1;
        range = Math.Abs(range);
        dx *= travelSign;
        dy *= travelSign;
        vertical *= travelSign;

        var wallDistance = double.PositiveInfinity;
        LevelLine? hitWall = null;
        var hitSide = 0;
        var currentSector = ActorPhysics.SectorAt(sim.Level, x, y);
        var enterDistance = 0.0;
        var planeSector = -1;
        var planePart = -1;
        var blockingLineDistance = double.PositiveInfinity;
        int SideSector(int side) => (uint)side < (uint)sim.Level.Sides.Count ? sim.Level.Sides[side].Sector : -1;
        bool ValidSector(int sector) => (uint)sector < (uint)sim.Floors.Length;
        double PlaneDistance(int sector, int part) => nativeVertical == 0 ? double.NaN
            : ((part == 0 ? sim.Floors[sector] : sim.Ceilings[sector]) - z) / nativeVertical;
        bool PlaneAlongRay(int sector, int part, out double planeDistance)
        {
            planeDistance = PlaneDistance(sector, part);
            return planeDistance > enterDistance + EqualEpsilon && planeDistance < signedRange;
        }
        bool PlaneBeatsWallAtLine(int part, double planeDistance, double lineDistance) =>
            part == 0 ? planeDistance <= lineDistance : planeDistance < lineDistance;
        var intercepts = new List<(double Distance, bool IsLine, LevelLine? Line, Actor? Actor)>();
        foreach (var line in sim.Level.Lines)
        {
            var distance = RayLine(x, y, dx, dy, line, range);
            if (double.IsFinite(distance) && distance <= range)
                intercepts.Add((distance, true, line, null));
        }
        foreach (var actor in sim.Actors)
        {
            if (throughActors || throughSpecies && source.SharesContactSpecies(actor)
                || actor.Destroyed || !actor.IsBlockmapActor || ReferenceEquals(source, actor) || !MatchesActorMask(actor, actorMask)
                || requireDamageable && !actor.CanTakeDamage)
                continue;
            var rx = actor.X.ToDouble() - x;
            var ry = actor.Y.ToDouble() - y;
            var radius = actor.Radius.ToDouble();
            if (radius < 0) continue;
            double distance = 0, end = range;
            if (!ClipAxis(rx - radius, rx + radius, dx, ref distance, ref end)
                || !ClipAxis(ry - radius, ry + radius, dy, ref distance, ref end))
                continue;
            distance = ActorBoxEntry(rx, ry, radius, dx, dy, range);
            if (!double.IsFinite(distance) || distance > end) continue;
            intercepts.Add((distance, false, null, actor));
        }
        intercepts.Sort((left, right) =>
        {
            var order = left.Distance.CompareTo(right.Distance);
            return order != 0 ? order : left.IsLine ? -1 : 1;
        });

        var geometryResolved = false;
        foreach (var intercept in intercepts)
        {
            if (intercept.IsLine)
            {
                var line = intercept.Line!;
                var distance = intercept.Distance;
                var front = SideSector(line.SideFront);
                var back = SideSector(line.SideBack);
                var side = (line.X2 - line.X1) * (y - line.Y1) - (line.Y2 - line.Y1) * (x - line.X1) > 0 ? 1 : 0;
                if (ValidSector(currentSector) && front == currentSector) side = 0;
                else if (ValidSector(currentSector) && back == currentSector) side = 1;
                else if (line.OneSided) side = 0;
                var nearSector = currentSector;
                var hitZ = z + vertical * distance;
                var part = ValidSector(nearSector)
                    ? hitZ <= sim.Floors[nearSector] + EqualEpsilon ? 0
                    : hitZ >= sim.Ceilings[nearSector] - EqualEpsilon ? 1 : -1 : -1;
                if (part >= 0 && PlaneAlongRay(nearSector, part, out var planeDistance)
                    && PlaneBeatsWallAtLine(part, planeDistance, distance))
                {
                    wallDistance = planeDistance * travelSign;
                    blockingLineDistance = distance;
                    planeSector = nearSector;
                    planePart = part;
                    geometryResolved = true;
                    break;
                }
                if (BlocksPick(sim, line, hitZ, wallMask, side, nativeVertical, z, signedRange, enterDistance))
                {
                    wallDistance = blockingLineDistance = distance;
                    hitWall = line;
                    hitSide = side;
                    geometryResolved = true;
                    break;
                }
                currentSector = side == 0 ? back : front;
                enterDistance = distance * travelSign;
                continue;
            }

            if (intercept.Distance >= blockingLineDistance) continue;
            var actor = intercept.Actor!;
            var rx = actor.X.ToDouble() - x;
            var ry = actor.Y.ToDouble() - y;
            var radius = actor.Radius.ToDouble();
            var actorDistance = intercept.Distance;
            var bottom = actor.Z.ToDouble();
            var top = bottom + actor.Height.ToDouble();
            var entryZ = z + vertical * actorDistance;
            if (entryZ > top || entryZ < bottom)
            {
                if (entryZ > top ? nativeVertical >= 0 : nativeVertical <= 0) continue;
                var adjusted = ((entryZ > top ? top : bottom) - z) / nativeVertical;
                if (adjusted > signedRange) continue;
                actorDistance = adjusted * travelSign;
            }
            else if (Math.Abs(dx * actorDistance - rx) > radius || Math.Abs(dy * actorDistance - ry) > radius)
                continue;
            if (signedRange > 0 && actorDistance > range) continue;
            return new LineAttackHit(true, actor, x + dx * actorDistance, y + dy * actorDistance, z + vertical * actorDistance);
        }

        if (!geometryResolved && !double.IsFinite(wallDistance) && ValidSector(currentSector))
        {
            for (var part = 0; part < 2; part++)
            {
                if (!PlaneAlongRay(currentSector, part, out var distance)) continue;
                wallDistance = distance * travelSign;
                planeSector = currentSector;
                planePart = part;
                break;
            }
        }

        if (!double.IsFinite(wallDistance) || Math.Abs(wallDistance) > range)
            return new LineAttackHit(false, null, x + dx * range, y + dy * range, z + vertical * range);

        var impactZ = z + vertical * wallDistance;
        var sky = planeSector >= 0 && IsSkyTexture(planePart == 0
            ? sim.Level.Sectors[planeSector].FloorPic : sim.Level.Sectors[planeSector].CeilingPic);
        if (hitWall is { } skyWall)
        {
            var front = SideSector(skyWall.SideFront);
            var back = SideSector(skyWall.SideBack);
            var far = hitSide == 0 ? back : front;
            sky = !skyWall.OneSided && ValidSector(front) && ValidSector(back) && ValidSector(far)
                && impactZ > sim.Floors[far] && impactZ >= sim.Ceilings[far]
                && IsSkyTexture(sim.Level.Sectors[front].CeilingPic)
                && IsSkyTexture(sim.Level.Sectors[back].CeilingPic);
        }
        if (sky && noSky)
            return new LineAttackHit(false, null, x + dx * wallDistance, y + dy * wallDistance, impactZ);

        return new LineAttackHit(
            true,
            null,
            x + dx * wallDistance,
            y + dy * wallDistance,
            z + vertical * wallDistance)
        {
            Wall = hitWall,
            PlaneSector = planeSector,
            PlanePart = planeSector >= 0 ? planePart : -1,
            Side = hitSide,
        };
    }

    internal static bool IsSkyTexture(string texture) => string.Equals(texture, "F_SKY1", StringComparison.OrdinalIgnoreCase);

    public static bool HasLineOfSight(AuthoritySimulation sim, Actor source, Actor target) =>
        ClearPath(sim, source.X.ToDouble(), source.Y.ToDouble(), source.Z.ToDouble() + source.Height.ToDouble() / 2,
            target.X.ToDouble(), target.Y.ToDouble(), target.Z.ToDouble() + target.Height.ToDouble() / 2);

    internal static bool HasMissileLineOfSight(AuthoritySimulation sim, Actor source, Actor target)
        => CheckSight(sim, source, target, ActorSightFlags.SeePastBlockEverything);

    /// <summary>Represented native P_CheckSight flags, visibility and flat-sector aperture traversal.</summary>
    public static bool CheckSight(AuthoritySimulation sim, Actor? source, Actor? target, ActorSightFlags flags = 0)
    {
        ArgumentNullException.ThrowIfNull(sim);
        if ((flags & ~(ActorSightFlags)7) != 0) throw new NotSupportedException("The requested sight flags are not yet supported.");
        if (source is null || target is null) return false;
        if ((flags & ActorSightFlags.IgnoreVisibility) == 0)
        {
            if (target.MVisBlocked) return false;
            if ((target.Invisible || target.MinVisible || !target.RenderStyle.IsVisible(target.Alpha))
                && sim.NextCheckSightByte() > 50) return false;
        }
        var x = source.X.ToDouble(); var y = source.Y.ToDouble();
        var dx = target.X.ToDouble() - x; var dy = target.Y.ToDouble() - y;
        if (Math.Abs(dx) + Math.Abs(dy) < Epsilon) return true;
        var eye = source.Z.ToDouble() + source.Height.ToDouble() * 0.75;
        var bottom = target.Z.ToDouble() - eye;
        var top = bottom + target.Height.ToDouble();
        var levelNumber = sim.Level.LevelNumber
            ?? HCDE.Gamedata.MapInfoParser.DefaultLevelNum(sim.Level.MapName, out _);
        foreach (var line in sim.Level.Lines)
        {
            var fraction = RayLine(x, y, dx, dy, line);
            if (fraction > 1) continue;
            if (line.OneSided || (line.Flags & LevelLine.BlockSightFlag) != 0
                || (line.Flags & LevelLine.BlockEverythingFlag) != 0
                && ((flags & ActorSightFlags.SeePastBlockEverything) == 0
                    || (flags & ActorSightFlags.SeePastShootableLines) == 0 && !MissileSightPassesLine(line, levelNumber))) return false;
            if ((uint)line.SideFront >= (uint)sim.Level.Sides.Count || (uint)line.SideBack >= (uint)sim.Level.Sides.Count) return false;
            var front = sim.Level.Sides[line.SideFront].Sector; var back = sim.Level.Sides[line.SideBack].Sector;
            if ((uint)front >= (uint)sim.Floors.Length || (uint)back >= (uint)sim.Floors.Length) return false;
            var floor = Math.Max(sim.Floors[front], sim.Floors[back]);
            var ceiling = Math.Min(sim.Ceilings[front], sim.Ceilings[back]);
            if (floor >= ceiling) return false;
            if (fraction == 0) continue;
            bottom = Math.Max(bottom, (floor - eye) / fraction);
            top = Math.Min(top, (ceiling - eye) / fraction);
            if (top <= bottom) return false;
        }
        return true;
    }

    private static bool MissileSightPassesLine(LevelLine line, int levelNumber)
        => line.Impact && line.Special is 80 or 226 && (line.Arg1 == 0 || line.Arg1 == levelNumber);

    internal static bool ClearPath(AuthoritySimulation sim, double x, double y, double z, double tx, double ty, double tz,
        bool missileSight = false)
    {
        if (Math.Abs(tx - x) + Math.Abs(ty - y) < Epsilon) return true;
        var levelNumber = sim.Level.LevelNumber
            ?? HCDE.Gamedata.MapInfoParser.DefaultLevelNum(sim.Level.MapName, out _);
        foreach (var line in sim.Level.Lines)
        {
            var fraction = RayLine(x, y, tx - x, ty - y, line);
            var seePast = missileSight && MissileSightPassesLine(line, levelNumber);
            if (fraction <= 1 && BlocksShot(sim, line, z + (tz - z) * fraction, LevelLine.BlockSightFlag, seePast)) return false;
        }
        return true;
    }
}
