using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>
/// Ray-cylinder combat tracing with pitch and sector openings.
/// Slopes, portals and 3D floors remain outside this subset of P_LineAttack.
/// </summary>
public static class CombatTrace
{
    private const double Epsilon = 1e-9;

    public static Actor? FindTarget(AuthoritySimulation sim, Actor source, double range, double yawOffset = 0, double pitchOffset = 0)
    {
        if (!double.IsFinite(range) || range < 0 || source.IsDead || source.Destroyed)
            return null;
        var radians = source.Angle.Raw * (Math.PI / 0x80000000u) + yawOffset * Math.PI / 180;
        var dx = Math.Cos(radians);
        var dy = Math.Sin(radians);
        var x = source.X.ToDouble();
        var y = source.Y.ToDouble();
        var z = source.Z.ToDouble() + source.Height.ToDouble() / 2;
        var pitch = (source is PlayerPawn player ? player.PitchDegrees : 0) + pitchOffset;
        if (!double.IsFinite(pitch) || !double.IsFinite(yawOffset)) return null;
        var slope = -Math.Tan(Math.Clamp(pitch, -89, 89) * Math.PI / 180);
        var wallDistance = double.PositiveInfinity;
        foreach (var line in sim.Level.Lines)
        {
            var distance = RayLine(x, y, dx, dy, line);
            if (double.IsFinite(distance) && BlocksShot(sim, line, z + slope * distance))
                wallDistance = Math.Min(wallDistance, distance);
        }

        Actor? best = null;
        var bestDistance = double.PositiveInfinity;
        foreach (var actor in sim.Actors)
        {
            if (ReferenceEquals(source, actor) || !actor.CanTakeDamage)
                continue;
            var rx = actor.X.ToDouble() - x;
            var ry = actor.Y.ToDouble() - y;
            var along = rx * dx + ry * dy;
            var side = rx * dy - ry * dx;
            var radius = actor.Radius.ToDouble();
            var discriminant = radius * radius - side * side;
            if (radius < 0 || discriminant < -Epsilon)
                continue;
            var halfChord = Math.Sqrt(Math.Max(0, discriminant));
            if (along + halfChord < 0)
                continue;
            var distance = Math.Max(0, along - halfChord);
            var end = Math.Min(range, along + halfChord);
            var bottom = actor.Z.ToDouble(); var top = bottom + actor.Height.ToDouble();
            if (Math.Abs(slope) < Epsilon)
            {
                if (z < bottom || z >= top) continue;
            }
            else
            {
                var lower = (bottom - z) / slope; var upper = (top - z) / slope;
                distance = Math.Max(distance, Math.Min(lower, upper));
                end = Math.Min(end, Math.Max(lower, upper));
                if (distance > end + Epsilon) continue;
            }
            // Walls win ties; actors at equal distances use stable simulation IDs.
            if (distance > range || distance >= wallDistance - Epsilon)
                continue;
            if (distance < bestDistance || (distance == bestDistance && actor.Id < best!.Id))
            {
                best = actor;
                bestDistance = distance;
            }
        }
        return best;
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

    internal static double RayLine(double x, double y, double dx, double dy, LevelLine line)
    {
        var sx = line.X2 - line.X1;
        var sy = line.Y2 - line.Y1;
        var rx = line.X1 - x;
        var ry = line.Y1 - y;
        var denominator = dx * sy - dy * sx;
        if (Math.Abs(denominator) <= Epsilon)
        {
            if (Math.Abs(rx * dy - ry * dx) > Epsilon)
                return double.PositiveInfinity;
            var first = rx * dx + ry * dy;
            var last = (line.X2 - x) * dx + (line.Y2 - y) * dy;
            return Math.Max(first, last) < 0 ? double.PositiveInfinity : Math.Max(0, Math.Min(first, last));
        }
        var distance = (rx * sy - ry * sx) / denominator;
        var fraction = (rx * dy - ry * dx) / denominator;
        return distance >= -Epsilon && fraction >= -Epsilon && fraction <= 1 + Epsilon
            ? Math.Max(0, distance) : double.PositiveInfinity;
    }
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
