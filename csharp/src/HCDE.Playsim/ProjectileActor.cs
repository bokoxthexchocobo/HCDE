using HCDE.MapLoader;

namespace HCDE.Playsim;

public enum ProjectileKind { Rocket, Plasma, Bfg, ImpBall, BaronBall, CacodemonBall, CyberRocket, ArachnotronPlasma, MancubusBall, RevenantTracer }

/// <summary>Traveling cylinder projectiles with swept impacts and finite lifetime.</summary>
public sealed class ProjectileActor : Actor
{
    public Actor Owner { get; }
    public ProjectileKind Kind { get; }
    public int RemainingTics { get; private set; } = 175;
    internal void RestoreRemainingTics(int tics) => RemainingTics = tics;
    public uint? TracerTargetId { get; private set; }
    internal void RestoreTracerTarget(uint? targetId) => TracerTargetId = targetId;
    public double Speed => MovementSpeed.ToDouble();
    private double DefaultSpeed => Kind switch
    {
        ProjectileKind.Rocket or ProjectileKind.CyberRocket or ProjectileKind.MancubusBall => 20,
        ProjectileKind.Plasma or ProjectileKind.Bfg or ProjectileKind.ArachnotronPlasma => 25,
        ProjectileKind.BaronBall => 15, _ => 10
    };
    // Native Damage is a base multiplied by a random integer from one through eight on impact.
    public int ImpactDamage => Math.Max(0, Damage);
    private int DefaultDamage => Kind switch
    {
        ProjectileKind.Rocket or ProjectileKind.CyberRocket => 20,
        ProjectileKind.Plasma or ProjectileKind.ArachnotronPlasma or ProjectileKind.CacodemonBall => 5,
        ProjectileKind.Bfg => 100, ProjectileKind.BaronBall or ProjectileKind.MancubusBall => 8,
        ProjectileKind.RevenantTracer => 10, _ => 3
    };
    public int BlastRadius => Kind is ProjectileKind.Rocket or ProjectileKind.CyberRocket ? 128 : 0;

    public ProjectileActor(Actor owner, ProjectileKind kind)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Owner = owner; Kind = kind;
        MovementSpeed = Fixed.FromDouble(DefaultSpeed);
        Damage = DefaultDamage;
        DoomEdNum = kind <= ProjectileKind.ImpBall ? 65530 + (int)kind : 65516 + (int)kind;
        // Managed-only class identities, not native spawn indices; keep every identity within ushort.
        Solid = Shootable = false; NoGravity = true; AllowDropOff = true;
        NoTeleport = true;
        Radius = Fixed.FromInt(kind is ProjectileKind.Plasma or ProjectileKind.ArachnotronPlasma ? 13
            : kind == ProjectileKind.RevenantTracer ? 11 : 6);
        Height = Fixed.FromInt(kind == ProjectileKind.BaronBall ? 16 : 8);
    }

    internal void Aim(Actor? target)
    {
        TracerTargetId = Kind == ProjectileKind.RevenantTracer ? target?.Id : null;
        var radians = Angle.Raw * Math.PI / 0x80000000u;
        var dz = Owner is PlayerPawn player ? -Math.Tan(Math.Clamp(player.PitchDegrees, -89, 89) * Math.PI / 180) : 0;
        if (target != null)
        {
            var dx = target.X.ToDouble() - X.ToDouble();
            var dy = target.Y.ToDouble() - Y.ToDouble();
            radians = Math.Atan2(dy, dx);
            var vertical = target.Z.ToDouble() + target.Height.ToDouble() / 2 - Z.ToDouble();
            if (Owner is not PlayerPawn)
            {
                // P_SpawnMissileXYZ derives the direction from the source, then lowers
                // shots whose spawn offset would put them above the destination.
                var sourceZ = Owner.Z.ToDouble() + (Kind == ProjectileKind.RevenantTracer ? 16 : 0);
                vertical = target.Z.ToDouble() - sourceZ;
                var offset = Z.ToDouble() - sourceZ;
                if (offset >= target.Height.ToDouble()) vertical += target.Height.ToDouble() - offset;
            }
            dz = vertical / Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        }
        var horizontalSpeed = Speed / Math.Sqrt(1 + dz * dz);
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * horizontalSpeed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * horizontalSpeed);
        VelocityZ = Fixed.FromDouble(Math.Clamp(dz * horizontalSpeed, -Math.Abs(Speed), Math.Abs(Speed)));
    }

    internal void RotateYaw(double degrees)
    {
        var speed = Math.Sqrt(Math.Pow(VelocityX.ToDouble(), 2) + Math.Pow(VelocityY.ToDouble(), 2));
        var radians = Angle.Raw * Math.PI / 0x80000000u + degrees * Math.PI / 180;
        Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * speed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * speed);
    }

    private void TrackTarget(AuthoritySimulation sim)
    {
        if (Kind != ProjectileKind.RevenantTracer || (sim.Thinkers.Clock.Tic & 3) != 0) return;
        var target = sim.Actors.FirstOrDefault(actor => actor.Id == TracerTargetId && actor.CanTakeDamage);
        if (target == null) return;
        var dx = target.X.ToDouble() - X.ToDouble(); var dy = target.Y.ToDouble() - Y.ToDouble();
        var desired = Math.Atan2(dy, dx) * 180 / Math.PI;
        var current = Angle.Raw * 180.0 / 0x80000000u;
        var delta = (desired - current + 540) % 360 - 180;
        RotateYaw(Math.Clamp(delta, -16.875, 16.875));
        var radians = Angle.Raw * Math.PI / 0x80000000u;
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * Speed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * Speed);
        var travel = Math.Max(1, Math.Sqrt(dx * dx + dy * dy) / Speed);
        // A_Tracer's small-target branch uses the missile's height, matching DoTracer2.
        var aimZ = target.Z.ToDouble() + (target.Height.ToDouble() >= 56 ? 40 : Height.ToDouble() * 2 / 3);
        var desiredZ = (aimZ - Z.ToDouble()) / travel;
        var velocityZ = VelocityZ.ToDouble();
        VelocityZ = Fixed.FromDouble(velocityZ + (desiredZ < velocityZ ? -0.125 : 0.125));
    }

    protected override void TickMovement(AuthoritySimulation sim)
    {
        if (--RemainingTics <= 0) { Destroy(); return; }
        TrackTarget(sim);
        var vx = VelocityX.ToDouble(); var vy = VelocityY.ToDouble(); var vz = VelocityZ.ToDouble();
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        for (var i = 0; i < steps; i++)
        {
            var x = X.ToDouble(); var y = Y.ToDouble(); var z = Z.ToDouble();
            var dx = vx / steps; var dy = vy / steps; var dz = vz / steps;
            var sector = ActorPhysics.SectorAt(sim.Level, x + dx, y + dy);
            if (NoExplodeFloor && sector >= 0 && z + dz <= sim.FloorOf(sector))
            {
                dz = sim.FloorOf(sector) - z;
                vz = 0; VelocityZ = default;
            }
            var fraction = double.PositiveInfinity;
            Actor? victim = null;
            LevelLine? wall = null;
            var planeSector = -1;
            var planePart = -1;
            foreach (var line in sim.Level.Lines)
            {
                var hit = CapsuleFraction(x, y, dx, dy, line, Radius.ToDouble());
                if (hit <= 1 && (CombatTrace.BlocksShot(sim, line, z + dz * hit, LevelLine.BlockProjectileFlag)
                    || CombatTrace.BlocksShot(sim, line, z + dz * hit + Height.ToDouble(), LevelLine.BlockProjectileFlag)) && hit < fraction)
                { fraction = hit; wall = line; }
            }
            if (sector >= 0)
            {
                var floor = sim.FloorOf(sector); var ceiling = sim.CeilingOf(sector) - Height.ToDouble();
                var plane = double.PositiveInfinity;
                var part = -1;
                if (!NoExplodeFloor && (z + dz < floor || dz < 0 && z + dz == floor))
                { plane = dz < 0 ? Math.Clamp((floor - z) / dz, 0, 1) : 0; part = 0; }
                if (z + dz > ceiling)
                {
                    var hit = dz > 0 ? Math.Clamp((ceiling - z) / dz, 0, 1) : 0;
                    if (hit < plane) { plane = hit; part = 1; }
                }
                if (plane < fraction) { fraction = plane; wall = null; planeSector = sector; planePart = part; }
            }
            foreach (var actor in sim.Actors)
            {
                if (ReferenceEquals(actor, Owner) || !actor.IsBlockmapActor || !actor.CanTakeDamage) continue;
                var hit = CylinderFraction(x, y, z, dx, dy, dz, actor);
                if (hit < fraction || hit == fraction && victim != null && actor.Id < victim.Id)
                { fraction = hit; victim = actor; wall = null; planeSector = planePart = -1; }
            }
            if (fraction <= 1)
            {
                X = Fixed.FromDouble(x + dx * fraction); Y = Fixed.FromDouble(y + dy * fraction); Z = Fixed.FromDouble(z + dz * fraction);
                Impact(sim, victim, wall, planeSector, planePart); return;
            }
            X = Fixed.FromDouble(x + dx); Y = Fixed.FromDouble(y + dy); Z = Fixed.FromDouble(z + dz);
        }
        SectorIndex = ActorPhysics.SectorAt(sim.Level, X.ToDouble(), Y.ToDouble());
        // P_ZMovement moves by the current velocity before FallAndSink applies gravity.
        if (!NoGravity && Z.ToDouble() > sim.FloorOf(SectorIndex))
            VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble() - ActorPhysics.Gravity * Gravity.ToDouble()
                * ((uint)SectorIndex < (uint)sim.Level.Sectors.Count ? sim.Level.Sectors[SectorIndex].Gravity : 1));
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

    private void Impact(AuthoritySimulation sim, Actor? victim, LevelLine? wall, int planeSector, int planePart)
    {
        Destroy(); // Commit removal before damage callbacks can spawn or destroy actors.
        // P_ExplodeMissile removes ordinary missiles on Line_Horizon without death actions.
        if (wall?.Special == 9) return;
        if ((uint)planeSector < (uint)sim.Level.Sectors.Count && planePart is 0 or 1)
        {
            var sector = sim.Level.Sectors[planeSector];
            var texture = planePart == 0 ? sector.FloorPic : sector.CeilingPic;
            // Native sky contact destroys ordinary missiles before explosion actions.
            if (string.Equals(texture, "F_SKY1", StringComparison.OrdinalIgnoreCase)) return;
        }
        if (wall is not null) GeometryProjectileImpact.Apply(sim, this, wall);
        else if (planeSector >= 0) GeometryProjectileImpact.ApplyPlane(sim, this, planeSector, planePart);
        if (victim != null) ActorDamage.Apply(victim, ImpactDamage * (1 + (int)(sim.NextCombatRandom() % 8)), Owner, inflictor: this);
        if (BlastRadius > 0)
        {
            foreach (var actor in sim.Actors.ToArray())
            {
                if (!actor.IsBlockmapActor || !actor.CanTakeDamage || actor.NoRadiusDamage) continue;
                var horizontal = Math.Max(0, Math.Sqrt(Math.Pow(actor.X.ToDouble() - X.ToDouble(), 2)
                    + Math.Pow(actor.Y.ToDouble() - Y.ToDouble(), 2)) - actor.Radius.ToDouble());
                var vertical = Math.Max(0, Math.Max(actor.Z.ToDouble() - Z.ToDouble(), Z.ToDouble() - actor.Z.ToDouble() - actor.Height.ToDouble()));
                var distance = Math.Sqrt(horizontal * horizontal + vertical * vertical);
                if (distance >= BlastRadius || !CombatTrace.HasLineOfSight(sim, this, actor)) continue;
                ActorDamage.Apply(actor, Math.Max(1, BlastRadius - (int)distance), Owner, inflictor: this);
            }
        }
        if (Kind == ProjectileKind.Bfg)
        {
            // A_BFGSpray fans out from the owner's position along the missile's yaw.
            // Native vertical autoaim and the explosion-state delay remain separate work.
            for (var ray = 0; ray < 40; ray++)
            {
                var yaw = Angle.ToDegrees() - Owner.Angle.ToDegrees() - 45 + 90.0 * ray / 40;
                var pitch = Owner is PlayerPawn player ? -player.PitchDegrees : 0;
                var target = CombatTrace.FindTarget(sim, Owner, 1024, yaw, pitch);
                if (target == null) continue;
                var damage = 0;
                for (var die = 0; die < 15; die++) damage += 1 + (int)(sim.NextCombatRandom() % 8);
                ActorDamage.Apply(target, damage, Owner);
            }
        }
    }
}
