using HCDE.MapLoader;

namespace HCDE.Playsim;

/// <summary>Flat-sector cylinder physics. Slopes, portals and 3D floors are not implemented.</summary>
public static class ActorPhysics
{
    public const double GroundFriction = 0xE800 / 65536.0;
    public const double Gravity = 1;
    public const double MaxMove = 30;

    public static int SectorAt(PlayLevel level, double x, double y)
    {
        if (level.Sectors.Count == 0) return -1;
        for (var sector = 0; sector < level.Sectors.Count; sector++)
        {
            var inside = false;
            foreach (var line in level.Lines)
            {
                var front = SideSector(level, line.SideFront);
                var back = SideSector(level, line.SideBack);
                if (front == back || (front != sector && back != sector)) continue;
                if ((line.Y1 > y) != (line.Y2 > y)
                    && x < (line.X2 - line.X1) * (y - line.Y1) / (line.Y2 - line.Y1) + line.X1)
                    inside = !inside;
            }
            if (inside) return sector;
        }
        // Synthetic levels without enclosing geometry may still define one sector.
        return level.Sectors.Count == 1 ? 0 : -1;
    }

    private static int SideSector(PlayLevel level, int side) =>
        (uint)side < (uint)level.Sides.Count ? level.Sides[side].Sector : -1;

    public static void PlaceOnFloor(AuthoritySimulation sim, Actor actor)
    {
        actor.SectorIndex = SectorAt(sim.Level, actor.X.ToDouble(), actor.Y.ToDouble());
        actor.Z = Fixed.FromDouble(sim.FloorOf(actor.SectorIndex));
        actor.OnGround = true;
        actor.RememberPosition();
    }

    public static void Step(AuthoritySimulation sim, Actor actor)
    {
        if (actor.Brain?.Charging == true) { StepCharge(sim, actor); return; }
        var vx = Math.Clamp(actor.VelocityX.ToDouble(), -MaxMove, MaxMove);
        var vy = Math.Clamp(actor.VelocityY.ToDouble(), -MaxMove, MaxMove);
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Abs(vx), Math.Abs(vy)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var x = actor.X.ToDouble();
            var y = actor.Y.ToDouble();
            var dx = vx / steps;
            var dy = vy / steps;
            if (!TryMove(sim, actor, x + dx, y + dy, out var wall))
            {
                if (wall != null)
                {
                    var lx = wall.X2 - wall.X1;
                    var ly = wall.Y2 - wall.Y1;
                    var length = lx * lx + ly * ly;
                    var scale = length > 0 ? (vx * lx + vy * ly) / length : 0;
                    vx = lx * scale;
                    vy = ly * scale;
                    if (!TryMove(sim, actor, x + vx / steps, y + vy / steps, out _)) vx = vy = 0;
                }
                else
                {
                    if (!TryMove(sim, actor, x + dx, y, out _)) vx = 0;
                    if (!TryMove(sim, actor, actor.X.ToDouble(), y + dy, out _)) vy = 0;
                }
            }
        }
        var floor = sim.FloorOf(actor.SectorIndex);
        var z = actor.Z.ToDouble();
        var vz = actor.VelocityZ.ToDouble();
        if (!actor.NoGravity && (z > floor || vz != 0)) vz -= Gravity;
        actor.Z = Fixed.FromDouble(z + vz);
        actor.VelocityZ = Fixed.FromDouble(vz);
        FloatTowardTarget(sim, actor);
        FitToSector(sim, actor, carryFloor: false);
        var friction = actor.OnGround ? GroundFriction : 1;
        actor.VelocityX = Fixed.FromDouble(Math.Abs(vx * friction) < 0.0625 ? 0 : vx * friction);
        actor.VelocityY = Fixed.FromDouble(Math.Abs(vy * friction) < 0.0625 ? 0 : vy * friction);
    }

    private static void FloatTowardTarget(AuthoritySimulation sim, Actor actor)
    {
        if (!actor.Floating || actor.IsDead || actor.Destroyed || actor.Brain is not { Enabled: true, Charging: false } brain
            || !double.IsFinite(actor.FloatSpeed) || actor.FloatSpeed <= 0) return;
        var target = sim.Actors.FirstOrDefault(candidate => candidate.Id == brain.TargetId && candidate.CanTakeDamage);
        if (target == null) return;
        var dx = target.X.ToDouble() - actor.X.ToDouble(); var dy = target.Y.ToDouble() - actor.Y.ToDouble();
        var distance = Math.Sqrt(dx * dx + dy * dy);
        var delta = target.Z.ToDouble() + target.Height.ToDouble() / 2 - actor.Z.ToDouble();
        // P_ZMovement compares the target's center to the floater's base, not its center.
        if (delta != 0 && distance < Math.Abs(delta) * 3)
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + Math.Sign(delta) * actor.FloatSpeed);
    }

    public static void FitToSector(AuthoritySimulation sim, Actor actor, bool carryFloor = true)
    {
        var floor = sim.FloorOf(actor.SectorIndex);
        var ceiling = actor.SectorIndex < 0 ? double.PositiveInfinity : sim.CeilingOf(actor.SectorIndex);
        var z = actor.Z.ToDouble();
        if (carryFloor && actor.OnGround) z = floor;
        if (z + actor.Height.ToDouble() > ceiling)
        {
            z = ceiling - actor.Height.ToDouble();
            if (actor.VelocityZ.Raw > 0) actor.VelocityZ = default;
        }
        if (z <= floor)
        {
            z = floor;
            if (actor.VelocityZ.Raw < 0) actor.VelocityZ = default;
        }
        actor.Z = Fixed.FromDouble(z);
        actor.OnGround = z <= floor && actor.VelocityZ.Raw <= 0;
    }

    private static void StepCharge(AuthoritySimulation sim, Actor actor)
    {
        var vx = actor.VelocityX.ToDouble(); var vy = actor.VelocityY.ToDouble(); var vz = actor.VelocityZ.ToDouble();
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var previousZ = actor.Z;
            actor.Z = Fixed.FromDouble(actor.Z.ToDouble() + vz / steps);
            var floor = sim.FloorOf(actor.SectorIndex);
            var ceiling = actor.SectorIndex >= 0 ? sim.CeilingOf(actor.SectorIndex) : double.PositiveInfinity;
            if (actor.Z.ToDouble() < floor)
            {
                actor.Z = Fixed.FromDouble(floor);
                vz = 0; actor.VelocityZ = default;
            }
            if (actor.Z.ToDouble() + actor.Height.ToDouble() > ceiling)
            {
                actor.Z = Fixed.FromDouble(ceiling - actor.Height.ToDouble());
                vz = -Math.Abs(vz); actor.VelocityZ = Fixed.FromDouble(vz);
            }
            Actor? victim = null;
            if (actor.Z.ToDouble() < floor
                || !TryMove(sim, actor, actor.X.ToDouble() + vx / steps, actor.Y.ToDouble() + vy / steps, out _, out victim))
            {
                actor.Z = previousZ;
                actor.Brain!.StopCharge(actor);
                if (victim?.CanTakeDamage == true)
                    ActorDamage.Apply(victim, 3 * (1 + (int)(sim.NextCombatRandom() % 8)), actor);
                return;
            }
            actor.OnGround = actor.Z.ToDouble() <= sim.FloorOf(actor.SectorIndex);
        }
    }

    internal static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall) =>
        TryMove(sim, actor, x, y, out wall, out _);

    internal static bool CanOccupy(AuthoritySimulation sim, Actor actor)
    {
        var x = actor.X.ToDouble(); var y = actor.Y.ToDouble(); var z = actor.Z.ToDouble();
        var radius = actor.Radius.ToDouble(); var height = actor.Height.ToDouble();
        var sector = SectorAt(sim.Level, x, y);
        if (z < sim.FloorOf(sector) || sector >= 0 && z + height > sim.CeilingOf(sector)) return false;
        if (sim.Level.Lines.Any(line => Blocks(sim, actor, line)
            && DistanceSquared(x, y, line.X1, line.Y1, line.X2, line.Y2) < radius * radius)) return false;
        return !sim.Actors.Any(other => !ReferenceEquals(actor, other) && other.BlocksActors
            && z < other.Z.ToDouble() + other.Height.ToDouble() && z + height > other.Z.ToDouble()
            && Math.Pow(x - other.X.ToDouble(), 2) + Math.Pow(y - other.Y.ToDouble(), 2)
                < Math.Pow(radius + other.Radius.ToDouble(), 2));
    }

    private static bool TryMove(AuthoritySimulation sim, Actor actor, double x, double y, out LevelLine? wall, out Actor? blocker)
    {
        wall = null;
        blocker = null;
        var ox = actor.X.ToDouble();
        var oy = actor.Y.ToDouble();
        var radius = Math.Max(0, actor.Radius.ToDouble());
        foreach (var line in sim.Level.Lines)
        {
            if (!Blocks(sim, actor, line)) continue;
            var before = DistanceSquared(ox, oy, line.X1, line.Y1, line.X2, line.Y2);
            var after = DistanceSquared(x, y, line.X1, line.Y1, line.X2, line.Y2);
            var swept = Math.Min(Math.Min(before, after), Math.Min(
                DistanceSquared(line.X1, line.Y1, ox, oy, x, y),
                DistanceSquared(line.X2, line.Y2, ox, oy, x, y)));
            // Permit actors already touching a wall to move parallel or away from it.
            if ((swept < radius * radius - 1e-8 && (before >= radius * radius || after < before - 1e-8))
                || LineSlide.Crosses(ox, oy, x, y, line)) { wall = line; return false; }
        }
        var sector = SectorAt(sim.Level, x, y);
        var z = actor.Brain?.Charging == true ? actor.Z.ToDouble() : Math.Max(actor.Z.ToDouble(), sim.FloorOf(sector));
        if (sector >= 0 && (z < sim.FloorOf(sector) || z - actor.Z.ToDouble() > actor.MaxStepHeight.ToDouble()
            || z + actor.Height.ToDouble() > sim.CeilingOf(sector))) return false;
        if (actor.BlocksActors)
        {
            foreach (var other in sim.Actors)
            {
                if (ReferenceEquals(actor, other) || !other.BlocksActors
                    || z >= other.Z.ToDouble() + other.Height.ToDouble()
                    || z + actor.Height.ToDouble() <= other.Z.ToDouble()) continue;
                var reach = radius + other.Radius.ToDouble();
                var cx = other.X.ToDouble();
                var cy = other.Y.ToDouble();
                var before = (ox - cx) * (ox - cx) + (oy - cy) * (oy - cy);
                var after = (x - cx) * (x - cx) + (y - cy) * (y - cy);
                if (actor.Brain?.Charging == true && after < reach * reach - 1e-8
                    || DistanceSquared(cx, cy, ox, oy, x, y) < reach * reach - 1e-8
                    && (before >= reach * reach || after < before - 1e-8)) { blocker = other; return false; }
            }
        }
        actor.X = Fixed.FromDouble(x);
        actor.Y = Fixed.FromDouble(y);
        actor.Z = Fixed.FromDouble(z);
        if (sector != actor.SectorIndex)
        {
            actor.SectorIndex = sector;
            actor.OnGround = z <= sim.FloorOf(sector) && actor.VelocityZ.Raw <= 0;
        }
        return true;
    }

    private static bool Blocks(AuthoritySimulation sim, Actor actor, LevelLine line)
    {
        if (line.BlocksMovement) return true;
        var front = SideSector(sim.Level, line.SideFront);
        var back = SideSector(sim.Level, line.SideBack);
        if (front < 0 || back < 0) return false;
        var floor = Math.Max(sim.FloorOf(front), sim.FloorOf(back));
        var ceiling = Math.Min(sim.CeilingOf(front), sim.CeilingOf(back));
        var bottom = Math.Max(actor.Z.ToDouble(), floor);
        return floor - actor.Z.ToDouble() > actor.MaxStepHeight.ToDouble()
            || bottom + actor.Height.ToDouble() > ceiling;
    }

    private static double DistanceSquared(double x, double y, double ax, double ay, double bx, double by)
    {
        var dx = bx - ax;
        var dy = by - ay;
        var length = dx * dx + dy * dy;
        var t = length == 0 ? 0 : Math.Clamp(((x - ax) * dx + (y - ay) * dy) / length, 0, 1);
        dx = x - ax - t * dx;
        dy = y - ay - t * dy;
        return dx * dx + dy * dy;
    }
}
