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
    /// <summary>Managed boundary for native DamageFunc; its result is used without impact dice.</summary>
    public Func<ProjectileActor, int>? DamageExpression { get; set; }

    public override void SetDamage(int damage)
    {
        base.SetDamage(damage);
        DamageExpression = null;
    }

    public override bool IsZeroDamage() => base.IsZeroDamage() && DamageExpression == null;

    /// <summary>Native GetMissileDamage calculation; a zero mask does not consume randomness.</summary>
    public int GetMissileDamage(int mask, int add)
    {
        if (DamageExpression is { } expression) return expression(this);
        if (mask == 0) return unchecked(add * ImpactDamage);
        var sim = Simulation ?? throw new InvalidOperationException("Random missile damage requires a simulation.");
        return unchecked(((int)sim.NextCombatRandom() & mask) + add) * ImpactDamage;
    }

    /// <summary>Managed direct-damage subset of P_DoMissileDamage, including ripper dice.</summary>
    public DamageResult DoMissileDamage(Actor victim, bool ripper = false)
    {
        ArgumentNullException.ThrowIfNull(victim);
        var damage = ripper ? GetMissileDamage(3, 2) : GetMissileDamage(StrifeDamage ? 3 : 7, 1);
        if (damage > 0 || ForcePain)
            return ActorDamage.Apply(victim, damage, Owner, damageType: DamageType, inflictor: this);
        victim.GiveBody((int)Math.Min(-(long)damage, int.MaxValue));
        return default;
    }
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
        var speed = VelXYToSpeed();
        var radians = Angle.Raw * Math.PI / 0x80000000u + degrees * Math.PI / 180;
        Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * speed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * speed);
    }

    private void TrackTarget(AuthoritySimulation sim)
    {
        if (Kind != ProjectileKind.RevenantTracer || (sim.Thinkers.Clock.Tic & 3) != 0 || Speed == 0) return;
        var target = sim.Actors.FirstOrDefault(actor => actor.Id == TracerTargetId && !actor.IsDead && !actor.Destroyed);
        if (target == null) return;
        var desired = AngleTo(target);
        var current = Angle.Raw * 180.0 / 0x80000000u;
        var delta = Normalize180(desired - current);
        RotateYaw(Math.Clamp(delta, -16.875, 16.875));
        VelFromAngle();
        if (FloorHugger || CeilingHugger) return;
        var travel = DistanceBySpeed(target, Speed);
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
        // Native AActor::Tick nudges vertical damaging missiles before collision movement.
        if (VelocityX.Raw == 0 && VelocityY.Raw == 0 && !IsZeroDamage())
            VelFromAngle(1.0 / Fixed.Unit);
        var oldFloor = sim.FloorOf(ActorPhysics.SectorAt(sim.Level, X.ToDouble(), Y.ToDouble()));
        var vx = VelocityX.ToDouble(); var vy = VelocityY.ToDouble(); var vz = VelocityZ.ToDouble();
        var steps = Math.Max(1, (int)Math.Ceiling(Math.Max(Math.Max(Math.Abs(vx), Math.Abs(vy)), Math.Abs(vz)) / 2));
        var huggerSector = -1;
        var ripped = new HashSet<uint>();
        for (var i = 0; i < steps; i++)
        {
            var x = X.ToDouble(); var y = Y.ToDouble(); var z = Z.ToDouble();
            var dx = vx / steps; var dy = vy / steps; var dz = vz / steps;
            var sector = ActorPhysics.SectorAt(sim.Level, x + dx, y + dy);
            // P_TryMove gives floor hugging precedence when both hugger flags are set.
            if ((FloorHugger || CeilingHugger) && sector >= 0 && sector != huggerSector && (dx != 0 || dy != 0))
            {
                z = FloorHugger ? sim.FloorOf(sector) : sim.CeilingOf(sector) - Height.ToDouble();
                Z = Fixed.FromDouble(z);
                huggerSector = sector;
            }
            if ((NoExplodeFloor || FloorHugger && !NoDropOff) && sector >= 0 && z + dz <= sim.FloorOf(sector))
            {
                dz = sim.FloorOf(sector) - z;
                if (NoExplodeFloor) { vz = 0; VelocityZ = default; }
            }
            if (CeilingHugger && sector >= 0 && z + dz > sim.CeilingOf(sector) - Height.ToDouble())
            {
                dz = sim.CeilingOf(sector) - Height.ToDouble() - z;
                if (vz > 0) { vz = 0; VelocityZ = default; }
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
                if (!NoExplodeFloor && (!FloorHugger || NoDropOff) && z + dz <= floor)
                { plane = dz < 0 ? Math.Clamp((floor - z) / dz, 0, 1) : 0; part = 0; }
                if (!CeilingHugger && z + dz > ceiling)
                {
                    var hit = dz > 0 ? Math.Clamp((ceiling - z) / dz, 0, 1) : 0;
                    if (hit < plane) { plane = hit; part = 1; }
                }
                if (plane < fraction) { fraction = plane; wall = null; planeSector = sector; planePart = part; }
            }
            var ripContacts = new List<(Actor Actor, double Fraction)>();
            foreach (var actor in sim.Actors)
            {
                if (ThruActors || actor.ThruActors || actor.NonShootable || !HitOwner && ReferenceEquals(actor, Owner)
                    || !actor.IsBlockmapActor || (!actor.CanTakeDamage && !actor.BlocksActors)) continue;
                if (MThruSpecies && Owner.SharesContactSpecies(actor)) continue;
                if (SharesEnabledThruBits(actor)) continue;
                if (ThruSpecies && SharesContactSpecies(actor)) continue;
                if (ThruGhost && actor.Ghost) continue;
                // Native checks attack eligibility and non-shootable solidity before SPECTRAL passage.
                if (actor.Shootable && actor.Spectral && !Spectral && actor.CanAttackHurtFrom(Owner)) continue;
                var hit = CylinderFraction(x, y, z, dx, dy, dz, actor);
                if (Rip && actor.CanBeRippedBy(this) && actor.Shootable && actor.CanAttackHurtFrom(Owner))
                {
                    if (hit <= 1) ripContacts.Add((actor, hit));
                    continue;
                }
                if (hit < fraction || hit == fraction && victim != null && actor.Id < victim.Id)
                { fraction = hit; victim = actor; wall = null; planeSector = planePart = -1; }
            }
            foreach (var contact in ripContacts.Where(contact => contact.Fraction < fraction)
                .OrderBy(contact => contact.Fraction).ThenBy(contact => contact.Actor.Id))
            {
                if (contact.Actor.Destroyed) continue;
                if (!ripped.Add(contact.Actor.Id)) continue;
                X = Fixed.FromDouble(x + dx * contact.Fraction); Y = Fixed.FromDouble(y + dy * contact.Fraction);
                Z = Fixed.FromDouble(z + dz * contact.Fraction);
                DoMissileDamage(contact.Actor, ripper: true);
                if (Destroyed) return;
                if (contact.Actor.Pushable && !CannotPush)
                {
                    contact.Actor.VelocityX = Fixed.FromDouble(contact.Actor.VelocityX.ToDouble() + VelocityX.ToDouble() * contact.Actor.PushFactor);
                    contact.Actor.VelocityY = Fixed.FromDouble(contact.Actor.VelocityY.ToDouble() + VelocityY.ToDouble() * contact.Actor.PushFactor);
                }
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
        {
            var gravity = ActorPhysics.Gravity * Gravity.ToDouble()
                * ((uint)SectorIndex < (uint)sim.Level.Sectors.Count ? sim.Level.Sectors[SectorIndex].Gravity : 1);
            var leavingFloor = VelocityZ.Raw == 0 && oldFloor > sim.FloorOf(SectorIndex) && Z.ToDouble() == oldFloor;
            VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble() - (leavingFloor ? gravity + gravity : gravity));
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
        var passHeight = target.ProjectilePassHeight.ToDouble();
        var clipHeight = passHeight > 0 ? passHeight
            : passHeight < 0 && Simulation is { } sim && sim.Compat.HasFlag(CompatSurface.MissileClip)
                ? -passHeight : target.Height.ToDouble();
        var low = target.Z.ToDouble() - Height.ToDouble(); var high = target.Z.ToDouble() + clipHeight;
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
        if (victim != null)
            DoMissileDamage(victim, Rip);
        if (BlastRadius > 0)
        {
            foreach (var actor in sim.Actors.ToArray())
            {
                if (!actor.IsBlockmapActor || !actor.CanTakeDamage || actor.NoRadiusDamage || actor.SplashImmune(this)) continue;
                var distance = RadiusDamageGeometry.Distance(this, actor);
                if (distance >= BlastRadius || !CombatTrace.HasLineOfSight(sim, this, actor)) continue;
                var damage = (int)(BlastRadius * (1 - distance / BlastRadius));
                if (damage > 0) ActorDamage.Apply(actor, damage, Owner, damageType: DamageType, inflictor: this);
            }
        }
        if (Kind == ProjectileKind.Bfg)
            BfgSprayActions.Apply(sim, this, Owner);
    }

    internal void ExplodeForAction()
    {
        var sim = Simulation ?? throw new InvalidOperationException("Projectile explosion requires a simulation.");
        if (!Destroyed) Impact(sim, null, null, -1, -1);
    }
}
