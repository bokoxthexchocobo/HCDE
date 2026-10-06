using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Actor-box combat tracing with pitch and sector openings.
/// Slopes, portals and 3D floors remain outside this subset of P_LineAttack.
/// </summary>
public static class CombatTrace
{
    private const double Epsilon = 1e-9;

    private const int MfShootable = 0x00000004;

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
        if (z <= sim.Floors[near] || z >= sim.Ceilings[near])
        {
            var planeZ = z <= sim.Floors[near] ? sim.Floors[near] : sim.Ceilings[near];
            var planeDistance = vertical == 0 ? double.NaN : (planeZ - startZ) / vertical;
            if (!(planeDistance > enterDistance && planeDistance < range))
                return z <= sim.Floors[far] || z >= sim.Ceilings[far];
        }
        return (line.Flags & wallMask) != 0 || z <= sim.Floors[near] || z >= sim.Ceilings[near]
            || z < sim.Floors[far] || z > sim.Ceilings[far];
    }

    internal static bool BlocksShot(AuthoritySimulation sim, LevelLine line, double z, int flag = LevelLine.BlockHitscanFlag)
    {
        if (line.OneSided || (line.Flags & (LevelLine.BlockEverythingFlag | flag)) != 0)
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
    {
        if (!double.IsFinite(range) || source.IsDead && !allowDeadSource || source.Destroyed && !allowDestroyedSource)
            return default;

        var radians = angle.Raw * (Math.PI / 0x80000000u);
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        var x = source.X.ToDouble();
        var y = source.Y.ToDouble();
        var z = source.Z.ToDouble() + source.Height.ToDouble() / 2
            + (source is PlayerPawn firingPlayer ? firingPlayer.AttackZOffset.ToDouble() * firingPlayer.CrouchFactor : 8);
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
        var intercepts = sim.Level.Lines.Select(line => (Line: line, Distance: RayLine(x, y, dx, dy, line, range)))
            .Where(intercept => double.IsFinite(intercept.Distance) && intercept.Distance <= range)
            .OrderBy(intercept => intercept.Distance);
        foreach (var intercept in intercepts)
        {
            var line = intercept.Line;
            var distance = intercept.Distance;
            var front = SideSector(line.SideFront);
            var back = SideSector(line.SideBack);
            var side = (line.X2 - line.X1) * (y - line.Y1) - (line.Y2 - line.Y1) * (x - line.X1) > 0 ? 1 : 0;
            if (ValidSector(currentSector) && front == currentSector) side = 0;
            else if (ValidSector(currentSector) && back == currentSector) side = 1;
            else if (line.OneSided) side = 0;
            currentSector = side == 0 ? front : back;
            var hitZ = z + vertical * distance;
            var part = ValidSector(currentSector)
                ? hitZ <= sim.Floors[currentSector] ? 0 : hitZ >= sim.Ceilings[currentSector] ? 1 : -1 : -1;
            var planeDistance = part >= 0 ? PlaneDistance(currentSector, part) : double.NaN;
            if (part >= 0 && planeDistance > enterDistance && planeDistance < signedRange)
            {
                wallDistance = planeDistance * travelSign;
                blockingLineDistance = distance;
                planeSector = currentSector;
                planePart = part;
                break;
            }
            if (BlocksPick(sim, line, hitZ, wallMask, side, nativeVertical, z, signedRange, enterDistance))
            {
                wallDistance = blockingLineDistance = distance;
                hitWall = line;
                hitSide = side;
                break;
            }
            currentSector = side == 0 ? back : front;
            enterDistance = distance * travelSign;
        }

        if (!double.IsFinite(wallDistance) && ValidSector(currentSector))
        {
            for (var part = 0; part < 2; part++)
            {
                var distance = PlaneDistance(currentSector, part);
                if (!(distance > enterDistance && distance < signedRange)) continue;
                wallDistance = distance * travelSign;
                planeSector = currentSector;
                planePart = part;
                break;
            }
        }

        Actor? best = null;
        var bestDistance = double.PositiveInfinity;
        var bestEntryDistance = double.PositiveInfinity;
        foreach (var actor in sim.Actors)
        {
            if (actor.Destroyed || !actor.IsBlockmapActor || ReferenceEquals(source, actor) || !MatchesActorMask(actor, actorMask)
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
            var entryDistance = distance;
            if (entryDistance >= blockingLineDistance) continue;
            var bottom = actor.Z.ToDouble();
            var top = bottom + actor.Height.ToDouble();
            var entryZ = z + vertical * distance;
            if (entryZ > top || entryZ < bottom)
            {
                // Native height adjustment retains the original direction and signed limit.
                if (entryZ > top ? nativeVertical >= 0 : nativeVertical <= 0) continue;
                var adjusted = ((entryZ > top ? top : bottom) - z) / nativeVertical;
                if (adjusted > signedRange) continue;
                distance = adjusted * travelSign;
                if (Math.Abs(dx * distance - rx) > radius || Math.Abs(dy * distance - ry) > radius) continue;
            }

            // Ordinary sector planes are resolved after unsuccessful actor traversal.
            if (signedRange > 0 && distance > range)
                continue;
            if (entryDistance < bestEntryDistance || (entryDistance == bestEntryDistance && actor.Id < best!.Id))
            {
                best = actor;
                bestDistance = distance;
                bestEntryDistance = entryDistance;
            }
        }

        if (best != null)
        {
            var hitX = x + dx * bestDistance;
            var hitY = y + dy * bestDistance;
            var hitZ = z + vertical * bestDistance;
            return new LineAttackHit(true, best, hitX, hitY, hitZ);
        }

        if (!double.IsFinite(wallDistance) || wallDistance > range)
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
        if (sky)
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

    internal static bool ClearPath(AuthoritySimulation sim, double x, double y, double z, double tx, double ty, double tz)
    {
        if (Math.Abs(tx - x) + Math.Abs(ty - y) < Epsilon) return true;
        foreach (var line in sim.Level.Lines)
        {
            var fraction = RayLine(x, y, tx - x, ty - y, line);
            if (fraction <= 1 && BlocksShot(sim, line, z + (tz - z) * fraction, LevelLine.BlockSightFlag)) return false;
        }
        return true;
    }
}
