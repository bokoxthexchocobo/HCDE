using HCDE.MapLoader;

namespace HCDE.Playsim;

public enum ProjectileKind { Rocket, Plasma, Bfg, ImpBall }

/// <summary>Traveling cylinder projectiles with swept impacts and finite lifetime.</summary>
public sealed class ProjectileActor : Actor
{
    public Actor Owner { get; }
    public ProjectileKind Kind { get; }
    public int RemainingTics { get; private set; } = 175;
    public double Speed => Kind switch { ProjectileKind.Rocket => 20, ProjectileKind.Plasma or ProjectileKind.Bfg => 25, _ => 10 };
    public int ImpactDamage => Kind switch { ProjectileKind.Rocket => 20, ProjectileKind.Plasma => 20, ProjectileKind.Bfg => 100, _ => 12 };
    public int BlastRadius => Kind == ProjectileKind.Rocket ? 128 : 0;

    public ProjectileActor(Actor owner, ProjectileKind kind)
    {
        Owner = owner; Kind = kind;
        DoomEdNum = 65530 + (int)kind; // Managed-only class identities, not native spawn indices.
        Solid = Shootable = false; NoGravity = true;
        Radius = Fixed.FromInt(kind == ProjectileKind.Plasma ? 13 : 6);
        Height = Fixed.FromInt(8);
    }

    internal void Aim(Actor? target)
    {
        var radians = Angle.Raw * Math.PI / 0x80000000u;
        var dz = Owner is PlayerPawn player ? -Math.Tan(Math.Clamp(player.PitchDegrees, -89, 89) * Math.PI / 180) : 0;
        if (target != null)
        {
            var dx = target.X.ToDouble() - X.ToDouble();
            var dy = target.Y.ToDouble() - Y.ToDouble();
            radians = Math.Atan2(dy, dx);
            dz = (target.Z.ToDouble() + target.Height.ToDouble() / 2 - Z.ToDouble()) / Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        }
        var horizontalSpeed = target == null && Owner is PlayerPawn ? Speed / Math.Sqrt(1 + dz * dz) : Speed;
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * horizontalSpeed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * horizontalSpeed);
        VelocityZ = Fixed.FromDouble(Math.Clamp(dz * horizontalSpeed, -Speed, Speed));
    }

    protected override void TickMovement(AuthoritySimulation sim)
    {
        if (--RemainingTics <= 0) { Destroy(); return; }
        var vx = VelocityX.ToDouble(); var vy = VelocityY.ToDouble(); var vz = VelocityZ.ToDouble();
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var x = X.ToDouble(); var y = Y.ToDouble(); var z = Z.ToDouble();
            var dx = vx / steps; var dy = vy / steps; var dz = vz / steps;
            var fraction = double.PositiveInfinity;
            Actor? victim = null;
            foreach (var line in sim.Level.Lines)
            {
                var hit = CapsuleFraction(x, y, dx, dy, line, Radius.ToDouble());
                if (hit <= 1 && (CombatTrace.BlocksShot(sim, line, z + dz * hit, LevelLine.BlockProjectileFlag)
                    || CombatTrace.BlocksShot(sim, line, z + dz * hit + Height.ToDouble(), LevelLine.BlockProjectileFlag))) fraction = Math.Min(fraction, hit);
            }
            var sector = ActorPhysics.SectorAt(sim.Level, x + dx, y + dy);
            if (sector >= 0)
            {
                var floor = sim.FloorOf(sector); var ceiling = sim.CeilingOf(sector) - Height.ToDouble();
                if (z + dz < floor) fraction = Math.Min(fraction, dz < 0 ? Math.Clamp((floor - z) / dz, 0, 1) : 0);
                if (z + dz > ceiling) fraction = Math.Min(fraction, dz > 0 ? Math.Clamp((ceiling - z) / dz, 0, 1) : 0);
            }
            foreach (var actor in sim.Actors)
            {
                if (ReferenceEquals(actor, Owner) || !actor.CanTakeDamage) continue;
                var hit = CylinderFraction(x, y, z, dx, dy, dz, actor);
                if (hit < fraction || hit == fraction && victim != null && actor.Id < victim.Id)
                { fraction = hit; victim = actor; }
            }
            if (fraction <= 1)
            {
                X = Fixed.FromDouble(x + dx * fraction); Y = Fixed.FromDouble(y + dy * fraction); Z = Fixed.FromDouble(z + dz * fraction);
                Impact(sim, victim); return;
            }
            X = Fixed.FromDouble(x + dx); Y = Fixed.FromDouble(y + dy); Z = Fixed.FromDouble(z + dz);
        }
    }

    private double CylinderFraction(double x, double y, double z, double dx, double dy, double dz, Actor target)
    {
        var reach = Radius.ToDouble() + target.Radius.ToDouble();
        var rx = x - target.X.ToDouble(); var ry = y - target.Y.ToDouble();
        var a = dx * dx + dy * dy; var b = rx * dx + ry * dy; var c = rx * rx + ry * ry - reach * reach;
        double enter = 0, leave = 1;
        if (a == 0) { if (c > 0) return double.PositiveInfinity; }
        else
        {
            var disc = b * b - a * c;
            if (disc < 0) return double.PositiveInfinity;
            enter = Math.Max(0, (-b - Math.Sqrt(disc)) / a);
            leave = Math.Min(1, (-b + Math.Sqrt(disc)) / a);
        }
        var low = target.Z.ToDouble() - Height.ToDouble(); var high = target.Z.ToDouble() + target.Height.ToDouble();
        if (dz == 0) { if (z < low || z > high) return double.PositiveInfinity; }
        else
        {
            var t1 = (low - z) / dz; var t2 = (high - z) / dz;
            enter = Math.Max(enter, Math.Min(t1, t2)); leave = Math.Min(leave, Math.Max(t1, t2));
        }
        return enter <= leave ? enter : double.PositiveInfinity;
    }

    private static double CapsuleFraction(double x, double y, double dx, double dy, LevelLine line, double radius)
    {
        var lx = line.X2 - line.X1; var ly = line.Y2 - line.Y1;
        var length = Math.Sqrt(lx * lx + ly * ly);
        var best = double.PositiveInfinity;
        foreach (var point in new[] { (line.X1, line.Y1), (line.X2, line.Y2) })
        {
            var rx = x - point.Item1; var ry = y - point.Item2;
            var a = dx * dx + dy * dy; var b = rx * dx + ry * dy;
            var c = rx * rx + ry * ry - radius * radius;
            if (c <= 0) best = 0;
            else if (a > 0 && b * b >= a * c)
            {
                var t = (-b - Math.Sqrt(b * b - a * c)) / a;
                if (t >= 0) best = Math.Min(best, t);
            }
        }
        if (length == 0) return best;
        var distance = ((x - line.X1) * ly - (y - line.Y1) * lx) / length;
        var rate = (dx * ly - dy * lx) / length;
        var hit = Math.Abs(distance) <= radius ? 0 : rate == 0 ? double.PositiveInfinity : ((distance > 0 ? radius : -radius) - distance) / rate;
        if (hit >= 0 && hit <= 1)
        {
            var along = ((x + dx * hit - line.X1) * lx + (y + dy * hit - line.Y1) * ly) / (length * length);
            if (along >= 0 && along <= 1) best = Math.Min(best, hit);
        }
        return best;
    }

    private void Impact(AuthoritySimulation sim, Actor? victim)
    {
        Destroy(); // Commit removal before damage callbacks can spawn or destroy actors.
        if (victim != null) ActorDamage.Apply(victim, ImpactDamage, Owner);
        if (BlastRadius > 0)
        {
            foreach (var actor in sim.Actors.ToArray())
            {
                if (!actor.CanTakeDamage) continue;
                var horizontal = Math.Max(0, Math.Sqrt(Math.Pow(actor.X.ToDouble() - X.ToDouble(), 2)
                    + Math.Pow(actor.Y.ToDouble() - Y.ToDouble(), 2)) - actor.Radius.ToDouble());
                var vertical = Math.Max(0, Math.Max(actor.Z.ToDouble() - Z.ToDouble(), Z.ToDouble() - actor.Z.ToDouble() - actor.Height.ToDouble()));
                var distance = Math.Sqrt(horizontal * horizontal + vertical * vertical);
                if (distance >= BlastRadius || !CombatTrace.HasLineOfSight(sim, this, actor)) continue;
                ActorDamage.Apply(actor, Math.Max(1, BlastRadius - (int)distance), Owner);
            }
        }
        if (Kind == ProjectileKind.Bfg)
        {
            // Managed approximation of the native owner-origin BFG spray.
            for (var ray = 0; ray < 40; ray++)
            {
                var target = CombatTrace.FindTarget(sim, Owner, 1024, -45 + 90.0 * ray / 39);
                if (target != null) ActorDamage.Apply(target, 15 + (int)(sim.NextCombatRandom() % 105), Owner);
            }
        }
    }
}
