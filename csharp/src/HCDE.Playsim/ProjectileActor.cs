using HCDE.MapLoader;

namespace HCDE.Playsim;

public enum ProjectileKind { Rocket, Plasma, Bfg, ImpBall, BaronBall, CacodemonBall, CyberRocket, ArachnotronPlasma, MancubusBall, RevenantTracer }

/// <summary>Traveling projectiles with square actor contacts, swept impacts and finite lifetime.</summary>
public sealed class ProjectileActor : Actor
{
    private Actor? _owner;
    public Actor? Owner
    {
        get
        {
            if (_owner is { Destroyed: true }) _owner = null;
            return _owner;
        }
        private set => _owner = value;
    }
    internal void ReassignOwner(Actor? owner)
    {
        if (owner is not null && (owner.Destroyed || Simulation is { } sim &&
            (owner.Simulation != sim || !sim.Actors.Contains(owner))))
            throw new InvalidOperationException("Projectile ownership requires a live actor in the same simulation.");
        Owner = owner;
    }
    internal void OwnerFromSave(Actor? owner) => Owner = owner;
    public ProjectileKind Kind { get; }
    internal string NativeClassName => NativeClassNameOf(Kind);
    internal static string NativeClassNameOf(ProjectileKind kind) => kind switch
    {
        ProjectileKind.Rocket => "Rocket",
        ProjectileKind.CyberRocket => "CyberRocket",
        ProjectileKind.Plasma => "PlasmaBall",
        ProjectileKind.Bfg => "BFGBall",
        ProjectileKind.ImpBall => "DoomImpBall",
        ProjectileKind.BaronBall => "BaronBall",
        ProjectileKind.CacodemonBall => "CacodemonBall",
        ProjectileKind.ArachnotronPlasma => "ArachnotronPlasma",
        ProjectileKind.MancubusBall => "FatShot",
        ProjectileKind.RevenantTracer => "RevenantTracer",
        _ => throw new InvalidOperationException("Unknown projectile class identity."),
    };
    internal IReadOnlyDictionary<ActorSoundType, string> DefaultActorSounds { get; set; }
        = new Dictionary<ActorSoundType, string>();

    internal Dictionary<ActorSoundType, string> CaptureSounds()
    {
        if (ActorSounds.Count == DefaultActorSounds.Count &&
            ActorSounds.All(pair => DefaultActorSounds.TryGetValue(pair.Key, out var value) && value == pair.Value)) return new();
        if (ActorSounds.Count == 0) return DefaultActorSounds.Keys.ToDictionary(key => key, _ => "");
        return new(ActorSounds);
    }
    public int RemainingTics { get; private set; } = 175;
    internal void RestoreRemainingTics(int tics) => RemainingTics = tics;
    public uint? TracerTargetId { get => TracerId; private set => TracerId = value; }
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
    public override int GetMissileDamage(int mask, int add)
    {
        if (DamageExpression is { } expression) return expression(this);
        return base.GetMissileDamage(mask, add);
    }

    private int DefaultDamage => Kind switch
    {
        ProjectileKind.Rocket or ProjectileKind.CyberRocket => 20,
        ProjectileKind.Plasma or ProjectileKind.ArachnotronPlasma or ProjectileKind.CacodemonBall => 5,
        ProjectileKind.Bfg => 100, ProjectileKind.BaronBall or ProjectileKind.MancubusBall => 8,
        ProjectileKind.RevenantTracer => 10, _ => 3
    };
    public int BlastRadius => Kind is ProjectileKind.Rocket or ProjectileKind.CyberRocket ? 128 : 0;

    public ProjectileActor(Actor? owner, ProjectileKind kind)
    {
        if (!Enum.IsDefined(kind)) throw new ArgumentOutOfRangeException(nameof(kind));
        Owner = owner; Kind = kind;
        var (seeSound, deathSound) = kind switch
        {
            ProjectileKind.Rocket or ProjectileKind.CyberRocket => ("weapons/rocklf", "weapons/rocklx"),
            ProjectileKind.Plasma => ("weapons/plasmaf", "weapons/plasmax"),
            ProjectileKind.Bfg => ("", "weapons/bfgx"),
            ProjectileKind.ImpBall => ("imp/attack", "imp/shotx"),
            ProjectileKind.BaronBall => ("baron/attack", "baron/shotx"),
            ProjectileKind.CacodemonBall => ("caco/attack", "caco/shotx"),
            ProjectileKind.ArachnotronPlasma => ("baby/attack", "baby/shotx"),
            ProjectileKind.MancubusBall => ("fatso/attack", "fatso/shotx"),
            ProjectileKind.RevenantTracer => ("skeleton/attack", "skeleton/tracex"),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        var sounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Death] = deathSound };
        if (seeSound.Length != 0) sounds[ActorSoundType.See] = seeSound;
        ActorSounds = sounds;
        DefaultActorSounds = new Dictionary<ActorSoundType, string>(sounds);
        States.SetSpawnFullBright(this, true);
        RenderStyle = kind is ProjectileKind.Rocket or ProjectileKind.CyberRocket
            ? ActorRenderStyle.Normal : ActorRenderStyle.FromNative(7);
        Alpha = kind is ProjectileKind.Plasma or ProjectileKind.Bfg or ProjectileKind.ArachnotronPlasma ? 0.75 : 1;
        DefaultRenderStyle = RenderStyle;
        DefaultAlpha = Alpha;
        MovementSpeed = Fixed.FromDouble(DefaultSpeed);
        Damage = DefaultDamage;
        DoomEdNum = kind <= ProjectileKind.ImpBall ? 65530 + (int)kind : 65516 + (int)kind;
        // Managed-only class identities, not native spawn indices; keep every identity within ushort.
        Solid = Shootable = false; NoGravity = true; AllowDropOff = true;
        DefaultSeeker = SeekerMissile = kind == ProjectileKind.RevenantTracer;
        NoTeleport = true;
        Radius = Fixed.FromInt(kind is ProjectileKind.Plasma or ProjectileKind.ArachnotronPlasma or ProjectileKind.Bfg ? 13
            : kind is ProjectileKind.RevenantTracer or ProjectileKind.Rocket or ProjectileKind.CyberRocket ? 11 : 6);
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
                var sourceZ = (Owner?.Z.ToDouble() ?? Z.ToDouble()) + (Kind == ProjectileKind.RevenantTracer ? 16 : 0);
                vertical = target.Z.ToDouble() - sourceZ;
                var offset = Z.ToDouble() - sourceZ;
                if (FloorHugger || CeilingHugger) vertical = 0;
                else if (offset >= target.Height.ToDouble()) vertical += target.Height.ToDouble() - offset;
                var length = Math.Sqrt(dx * dx + dy * dy + vertical * vertical);
                var scale = length != 0 ? Speed / length : 0;
                var vx = dx * scale; var vy = dy * scale;
                VelocityX = Fixed.FromDouble(vx);
                VelocityY = Fixed.FromDouble(vy);
                VelocityZ = Fixed.FromDouble(vertical * scale);
                Angle = BamAngle.FromDegrees(Math.Atan2(vy, vx) * 180 / Math.PI);
                return;
            }
            dz = vertical / Math.Max(1, Math.Sqrt(dx * dx + dy * dy));
            Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        }
        if (FloorHugger || CeilingHugger) dz = 0;
        var horizontalSpeed = Speed / Math.Sqrt(1 + dz * dz);
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * horizontalSpeed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * horizontalSpeed);
        VelocityZ = Fixed.FromDouble(Math.Clamp(dz * horizontalSpeed, -Math.Abs(Speed), Math.Abs(Speed)));
    }

    internal void RotateYaw(double degrees, bool useMovementSpeed = false)
    {
        var speed = useMovementSpeed ? Speed : VelXYToSpeed();
        var radians = Angle.Raw * Math.PI / 0x80000000u + degrees * Math.PI / 180;
        Angle = BamAngle.FromDegrees(radians * 180 / Math.PI);
        VelocityX = Fixed.FromDouble(Math.Cos(radians) * speed);
        VelocityY = Fixed.FromDouble(Math.Sin(radians) * speed);
    }

    private void TrackTarget(AuthoritySimulation sim)
    {
        if (Kind != ProjectileKind.RevenantTracer || (sim.Thinkers.Clock.Tic & 3) != 0 || Speed == 0) return;
        Trace(16.875);
    }

    /// <summary>Managed A_Tracer2 steering and vertical adjustment, without periodic A_Tracer gating.</summary>
    public void Trace(double turnAngle)
    {
        if (!double.IsFinite(turnAngle)) throw new ArgumentOutOfRangeException(nameof(turnAngle));
        var sim = Simulation ?? throw new InvalidOperationException("Tracer steering requires an attached simulation.");
        if (Speed == 0 || Destroyed) return;
        var target = sim.Actors.FirstOrDefault(actor => actor.Id == TracerTargetId && !actor.IsDead && !actor.Destroyed);
        if (target == null || !CanSeek(target)) return;
        var desired = AngleTo(target);
        var current = Angle.Raw * 180.0 / 0x80000000u;
        var delta = Normalize180(desired - current);
        if (delta != 0)
        {
            var next = current + (delta < 0 ? -turnAngle : turnAngle);
            var remaining = Normalize180(desired - next);
            if (delta < 0 ? remaining > 0 : remaining < 0) next = desired;
            Angle = BamAngle.FromDegrees(next);
        }
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
        var doRipping = Rip;
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
            if (NoClip && sector >= 0)
            {
                var floor = sim.FloorOf(sector); var ceiling = sim.CeilingOf(sector) - Height.ToDouble();
                if (z + dz <= floor)
                {
                    dz = floor - z;
                    if (vz < 0) { vz = 0; VelocityZ = default; }
                }
                if (z + dz > ceiling)
                {
                    dz = ceiling - z;
                    if (vz > 0) { vz = 0; VelocityZ = default; }
                }
            }
            var fraction = double.PositiveInfinity;
            Actor? victim = null;
            Actor? hitActor = null;
            LevelLine? wall = null;
            var planeSector = -1;
            var planePart = -1;
            foreach (var line in sim.Level.Lines)
            {
                if (NoClip) break;
                var hit = CapsuleFraction(x, y, dx, dy, line, Radius.ToDouble());
                var blockingMask = LevelLine.BlockProjectileFlag | (BlockAsPlayer ? LevelLine.BlockingFlag | LevelLine.BlockPlayersFlag : 0);
                if (hit <= 1 && (CombatTrace.BlocksShot(sim, line, z + dz * hit, blockingMask)
                    || CombatTrace.BlocksShot(sim, line, z + dz * hit + Height.ToDouble(), blockingMask)) && hit < fraction)
                { fraction = hit; wall = line; }
            }
            if (!NoClip && sector >= 0)
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
            foreach (var actor in sim.Actors.ToArray())
            {
                if (NoClip) break;
                if (ReferenceEquals(actor, this) || actor.Destroyed || ThruActors || actor.ThruActors
                    || !actor.IsBlockmapActor || SharesEnabledThruBits(actor)
                    || ThruSpecies && SharesContactSpecies(actor)) continue;
                // PIT_CheckThing rejects this overlap before missile passage and damage checks.
                if ((actor.Solid || actor.Shootable || actor.SpecialPickup || actor.Touchy)
                    && ActorPhysics.MissileOverlapBlocks(sim, this, actor))
                {
                    var overlapHit = ActorContactFraction(x, y, z, dx, dy, dz, actor, ignoreHeight: true);
                    if (overlapHit < fraction)
                    { fraction = overlapHit; victim = null; hitActor = actor; wall = null; planeSector = planePart = -1; }
                    continue;
                }
                if ((actor.BumpSpecial || Solid && actor.Touchy && !actor.IsDead)
                    && (actor.Solid || actor.Shootable || actor.SpecialPickup || actor.Touchy))
                {
                    var contact = ActorContactFraction(x, y, z, dx, dy, dz, actor, beforeMissilePassHeight: true);
                    if (contact <= 1 && contact <= fraction)
                    {
                        if (ActorPhysics.TryTouchyContact(this, actor, z + dz * contact))
                        {
                            if (Destroyed) return;
                            continue;
                        }
                        if (actor.BumpSpecial) ActorSpecialActions.TryBump(sim, actor, this);
                        if (Destroyed) return;
                        if (actor.Destroyed) continue;
                    }
                }
                if (actor.NonShootable || !(actor.Solid || actor.Shootable || actor.SpecialPickup || actor.Touchy)) continue;
                if (MThruSpecies && Owner?.SharesContactSpecies(actor) == true) continue;
                if (ThruGhost && actor.Ghost) continue;
                if (Rip && actor.Corpse && !actor.Shootable) continue;
                var hit = ActorContactFraction(x, y, z, dx, dy, dz, actor);
                if (hit > 1 || hit > fraction) continue;
                var specialHit = SpecialMissileHit(actor);
                if (Destroyed) return;
                if (specialHit == 1 || actor.Destroyed) continue;
                if (specialHit == 0)
                {
                    if (hit < fraction)
                    { fraction = hit; victim = null; hitActor = actor; wall = null; planeSector = planePart = -1; }
                    continue;
                }
                if (!HitOwner && ReferenceEquals(actor, Owner)) continue;
                var canHurt = actor.CanAttackHurtFrom(Owner);
                // Callback mutations take effect here; attack immunity blocks before solidity passage.
                if (canHurt && !actor.Shootable && !actor.Solid) continue;
                // Native checks attack eligibility and non-shootable solidity before SPECTRAL passage.
                if (actor.Shootable && actor.Spectral && !Spectral && canHurt) continue;
                if (doRipping && actor.CanBeRippedBy(this) && actor.Shootable && canHurt)
                {
                    if (hit <= 1) ripContacts.Add((actor, hit));
                    continue;
                }
                if (hit < fraction || hit == fraction && victim != null && actor.Id < victim.Id)
                { fraction = hit; victim = hitActor = actor; wall = null; planeSector = planePart = -1; }
            }
            foreach (var contact in ripContacts.Where(contact => contact.Fraction < fraction)
                .OrderBy(contact => contact.Fraction).ThenBy(contact => contact.Actor.Id))
            {
                if (contact.Actor.Destroyed) continue;
                if (!ripped.Add(contact.Actor.Id)) continue;
                X = Fixed.FromDouble(x + dx * contact.Fraction); Y = Fixed.FromDouble(y + dy * contact.Fraction);
                Z = Fixed.FromDouble(z + dz * contact.Fraction);
                DoMissileDamage(contact.Actor);
                if (Destroyed) return;
                if (contact.Actor.Pushable && !CannotPush)
                {
                    contact.Actor.VelocityX = Fixed.FromDouble(contact.Actor.VelocityX.ToDouble() + VelocityX.ToDouble() * contact.Actor.PushFactor);
                    contact.Actor.VelocityY = Fixed.FromDouble(contact.Actor.VelocityY.ToDouble() + VelocityY.ToDouble() * contact.Actor.PushFactor);
                    contact.Actor.LastPushFromMovement = true;
                }
            }
            if (fraction <= 1)
            {
                X = Fixed.FromDouble(x + dx * fraction); Y = Fixed.FromDouble(y + dy * fraction); Z = Fixed.FromDouble(z + dz * fraction);
                if (!Impact(sim, victim, wall, planeSector, planePart, hitActor)) return;
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

    private double ActorContactFraction(double x, double y, double z, double dx, double dy, double dz, Actor target,
        bool ignoreHeight = false, bool beforeMissilePassHeight = false)
    {
        var reach = Radius.ToDouble() + target.Radius.ToDouble();
        var rx = x - target.X.ToDouble(); var ry = y - target.Y.ToDouble();
        double enter = 0, leave = 1;
        // PIT_CheckThing requires strict overlap on both axes, including diagonal contacts.
        bool ClipAxis(double offset, double movement)
        {
            if (movement == 0) return Math.Abs(offset) < reach;
            var first = (-reach - offset) / movement; var last = (reach - offset) / movement;
            enter = Math.Max(enter, Math.Min(first, last));
            leave = Math.Min(leave, Math.Max(first, last));
            return enter < leave;
        }
        if (!ClipAxis(rx, dx) || !ClipAxis(ry, dy)) return double.PositiveInfinity;
        if (ignoreHeight) return enter <= leave ? enter : double.PositiveInfinity;
        if (Simulation is { } simulation && ActorPhysics.MissileUsesActorHeightPassage(simulation, this, target))
        {
            var bottom = target.Z.ToDouble() - Height.ToDouble();
            var top = target.Z.ToDouble() + target.Height.ToDouble();
            if (dz == 0)
            {
                if (target.SpecialPickup ? z < bottom || z > top : z <= bottom || z >= top)
                    return double.PositiveInfinity;
            }
            else
            {
                var first = (bottom - z) / dz; var last = (top - z) / dz;
                enter = Math.Max(enter, Math.Min(first, last));
                leave = Math.Min(leave, Math.Max(first, last));
                if (!target.SpecialPickup && enter >= leave) return double.PositiveInfinity;
            }
        }
        if (beforeMissilePassHeight)
        {
            if (enter == leave && (Math.Abs(rx + dx * enter) >= reach || Math.Abs(ry + dy * enter) >= reach))
                return double.PositiveInfinity;
            return enter <= leave ? enter : double.PositiveInfinity;
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
        if (enter > leave) return double.PositiveInfinity;
        // Height clipping can reduce the interval to an excluded XY edge.
        if (enter == leave && (Math.Abs(rx + dx * enter) >= reach || Math.Abs(ry + dy * enter) >= reach))
            return double.PositiveInfinity;
        return enter;
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

    private bool Impact(AuthoritySimulation sim, Actor? victim, LevelLine? wall, int planeSector, int planePart,
        Actor? hitActor = null)
    {
        // Native geometry damage precedes explosion removal and motion cleanup.
        if (wall is not null) GeometryProjectileImpact.Apply(sim, this, wall);
        else if (planeSector >= 0) GeometryProjectileImpact.ApplyPlane(sim, this, planeSector, planePart);
        if (Destroyed) return false;
        var missileHit = victim is { Shootable: true } && victim.CanAttackHurtFrom(Owner);
        if (missileHit && victim is not null)
            ActorMissileDamage.Apply(this, victim, Rip,
                detachRipSound: !ExplodeCount || ChaseThreshold >= DefaultChaseThreshold - 1);
        if (Destroyed) return false;
        if (missileHit && ThruReflect && victim is { Reflective: true, Destroyed: false })
        {
            if (SeekerMissile) TracerId = Owner?.Id;
            ReassignOwner(victim);
            return true;
        }
        if (hitActor is { Destroyed: false } reflector)
        {
            var reflected = ActorReflectionActions.ReflectOffActor(this, reflector);
            if (Destroyed || reflected) return false;
        }
        if (ExplodeCount)
        {
            SetChaseThreshold(unchecked(ChaseThreshold + 1), setDefault: false);
            if (ChaseThreshold < DefaultChaseThreshold) return false;
        }
        ClearImpactMotion();
        var pointerTarget = hitActor ?? victim;
        if (pointerTarget is { Destroyed: false })
        {
            if (HitTarget) SetTargetPointer(pointerTarget);
            if (HitMaster) MasterId = pointerTarget.Id;
            if (HitTracer) TracerId = pointerTarget.Id;
        }
        // SKYEXPLODE permits death handling on sky and horizon contacts.
        if (!SkyExplode && wall?.Special == 9) { Destroy(); return false; }
        if ((uint)planeSector < (uint)sim.Level.Sectors.Count && planePart is 0 or 1)
        {
            var sector = sim.Level.Sectors[planeSector];
            var texture = planePart == 0 ? sector.FloorPic : sector.CeilingPic;
            // Native sky contact destroys ordinary missiles before explosion actions.
            if (!SkyExplode && string.Equals(texture, "F_SKY1", StringComparison.OrdinalIgnoreCase)) { Destroy(); return false; }
        }
        if (ActorSounds.TryGetValue(ActorSoundType.Death, out var deathSound) && !string.IsNullOrEmpty(deathSound)
            && !deathSound.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            var position = SoundPos();
            sim.QueueSoundRequest(new SimulationSoundRequest(Id, deathSound, 2, 0, 1, false,
                FullVolumeDeath ? 0 : 1, false, DetachedOrigin:
                new SimulationSoundOrigin(position.X, position.Y, position.Z,
                    planeSector >= 0 ? planeSector : ActorPhysics.SectorAt(sim.Level, X.ToDouble(), Y.ToDouble()))), immediate: true);
        }
        if (BlastRadius > 0)
        {
            foreach (var actor in sim.Actors.ToArray())
            {
                if (!actor.IsBlockmapActor || !actor.CanTakeDamage || actor.NoRadiusDamage || actor.SplashImmune(this)) continue;
                if (actor.RadiusSourceImmune(Owner)) continue;
                var distance = RadiusDamageGeometry.Distance(this, actor);
                if (distance >= BlastRadius || !CombatTrace.CheckSight(sim, actor, this, ActorSightFlags.IgnoreVisibility)) continue;
                var damage = (int)(BlastRadius * (1 - distance / BlastRadius));
                if (damage > 0) ActorDamage.Apply(actor, damage, Owner, damageType: DamageType, inflictor: this);
            }
        }
        if (Kind == ProjectileKind.Bfg)
            BfgSprayActions.Apply(sim, this, Owner);
        Destroy();
        return false;
    }

    private void ClearImpactMotion()
    {
        VelocityX = VelocityY = VelocityZ = default;
        Shootable = false;
    }

    internal void ExplodeForAction()
    {
        var sim = Simulation ?? throw new InvalidOperationException("Projectile explosion requires a simulation.");
        if (!Destroyed) Impact(sim, null, null, -1, -1);
    }
}
