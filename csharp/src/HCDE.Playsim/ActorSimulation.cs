using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public readonly struct PlayerCommand
{
    public short ForwardMove { get; init; }
    public short SideMove { get; init; }
    public short UpMove { get; init; }
    public short YawDelta { get; init; }
    public short PitchDelta { get; init; }
    public bool Attack { get; init; }
    public bool Jump { get; init; }
    public bool Use { get; init; }
    /// <summary>Native BT_CROUCH (1&lt;&lt;3).</summary>
    public bool Crouch { get; init; }
    /// <summary>Native BT_TURN180 (1&lt;&lt;4).</summary>
    public bool Turn180 { get; init; }
    public ReadOnlyMemory<byte> WeaponSelections { get; init; }
}

public class Actor : Thinker
{
    public const int PlayerStartMin = 1;
    public const int PlayerStartMax = 4;

    public uint Id { get; init; }
    public IReadOnlyDictionary<ActorSoundType, string> ActorSounds { get; set; }
        = new Dictionary<ActorSoundType, string>();
    /// <summary>Native <c>GetNetworkID</c>. Assigned by netplay; ACS may query it offline.</summary>
    public uint NetworkId { get; set; }
    public int DoomEdNum { get; init; }
    internal int DefinitionDoomEdNum { get; set; }
    internal int ClassDoomEdNum => DefinitionDoomEdNum > 0 ? DefinitionDoomEdNum : DoomEdNum;
    public int ThingId { get; internal set; }
    /// <summary>Native <c>AActor::IsMapActor</c> for represented non-inventory actors.</summary>
    public virtual bool IsMapActor() => true;
    public bool NoBlockmap { get; set; }
    internal bool HasBlockmapOverride { get; set; }
    private double _floorClip;
    public double FloorClip
    {
        get => _floorClip;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            _floorClip = value;
            HasFloorClipOverride = true;
        }
    }
    internal bool HasFloorClipOverride { get; set; }
    private bool _invisible;
    public bool Invisible
    {
        get => _invisible;
        set { _invisible = value; HasVisibilityOverride = true; }
    }
    internal bool HasVisibilityOverride { get; set; }
    private int _special;
    public int Special { get => _special; set { _special = value; SpecialChanged = true; } }
    public int[] SpecialArgs { get; } = new int[5];
    internal bool SpecialChanged { get; set; }
    private int _activationType;
    public virtual void Activate(Actor? activator) => ThingActivation.Apply(this, true);
    public virtual void Deactivate(Actor? activator) => ThingActivation.Apply(this, false);
    public Func<Actor, bool>? UseAction { get; set; }
    private uint? _masterId;
    public uint? MasterId { get => _masterId; set { _masterId = value == 0 ? null : value; HasMasterOverride = true; } }
    internal bool HasMasterOverride { get; set; }
    private bool _useSpecial;
    public bool UseSpecial { get => _useSpecial; set { _useSpecial = value; HasUseSpecialOverride = true; } }
    internal bool HasUseSpecialOverride { get; set; }
    public virtual bool Used(Actor user) => UseAction?.Invoke(user) ?? false;

    public virtual void BeginPlay()
    {
        if (!Dormant) return;
        Dormant = false;
        Deactivate(null);
    }

    public override void PostBeginPlay()
    {
        base.PostBeginPlay();
        HandleNoDelay = true;
    }

    public override void CallPostBeginPlay()
    {
        base.CallPostBeginPlay();
        Simulation?.NotifyActorSpawned(this);
    }

    internal bool SpawnDormant { get; set; }
    internal bool SpawnAmbush { get; set; }
    public virtual void HandleSpawnFlags()
    {
        if (SpawnAmbush) Ambush = true;
        if (SpawnDormant) Deactivate(null);
        if (SpawnFriendly) Friendly = true;
    }

    internal bool SpawnDropped { get; set; }
    public bool Synchronized { get; set; }
    public void LevelSpawned() => LevelSpawned(null);

    internal void LevelSpawned(Func<uint>? mapSpawnRandom)
    {
        if (States.RemainingTics > 0 && !Synchronized)
        {
            var random = mapSpawnRandom is not null ? mapSpawnRandom()
                : (Simulation ?? throw new InvalidOperationException("Map spawn timing requires a simulation.")).NextMapSpawnRandom();
            States.SetTics(1 + (int)(random % (uint)States.RemainingTics));
        }
        if (!SpawnDropped) Dropped = false;
        HandleSpawnFlags();
    }

    public bool ActivateSpecial(Actor? activator, bool death = false)
    {
        if (Simulation is not { } simulation)
            throw new InvalidOperationException("Actor special activation requires a simulation.");
        return ActorSpecialActions.ActivateSpecial(simulation, this, activator, death);
    }

    public int ActivationType
    {
        get => _activationType;
        set { _activationType = value; SpecialChanged = true; }
    }
    internal virtual bool IsBlockmapActor => !NoBlockmap;
    /// <summary>Native actor gravity multiplier; ACS reads/writes signed 16.16 values.</summary>
    public Fixed Gravity { get; set; } = Fixed.FromInt(1);
    public double DistanceBySpeed(Actor destination, double speed)
    {
        var travel = Distance2D(destination) / speed;
        // Native max(1., travel) also returns one for coincident points at zero speed.
        return travel > 1 ? travel : 1;
    }

    public (double X, double Y) Vec2To(Actor destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        return (destination.X.ToDouble() - X.ToDouble(), destination.Y.ToDouble() - Y.ToDouble());
    }

    public double Distance2DSquared(Actor destination)
    {
        var displacement = Vec2To(destination);
        return displacement.X * displacement.X + displacement.Y * displacement.Y;
    }
    public double Distance2D(Actor destination) => Math.Sqrt(Distance2DSquared(destination));
    public double Distance2D(double x, double y)
    {
        var dx = X.ToDouble() - x; var dy = Y.ToDouble() - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    public double Distance2D(Actor destination, double xOffset, double yOffset)
    {
        var displacement = Vec2To(destination);
        var dx = xOffset - displacement.X; var dy = yOffset - displacement.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
    public double Distance3DSquared(Actor destination)
    {
        var displacement = Vec3To(destination);
        return displacement.X * displacement.X + displacement.Y * displacement.Y + displacement.Z * displacement.Z;
    }
    public double Distance3D(Actor destination) => Math.Sqrt(Distance3DSquared(destination));

    public double AngleTo(Actor destination) => AngleTo(destination, 0, 0);
    public double AngleTo(Actor destination, double xOffset, double yOffset)
    {
        var displacement = Vec2To(destination);
        return Math.Atan2(displacement.Y + yOffset, displacement.X + xOffset) * 180 / Math.PI;
    }

    public (double X, double Y, double Z) Vec3To(Actor destination)
    {
        var horizontal = Vec2To(destination);
        return (horizontal.X, horizontal.Y, destination.Z.ToDouble() - Z.ToDouble());
    }

    /// <summary>Position offsets in the managed flat world; native portal transforms are not represented.</summary>
    public (double X, double Y) Vec2Offset(double dx, double dy) => (X.ToDouble() + dx, Y.ToDouble() + dy);
    public (double X, double Y, double Z) Vec2OffsetZ(double dx, double dy, double z)
    {
        var position = Vec2Offset(dx, dy);
        return (position.X, position.Y, z);
    }
    public (double X, double Y, double Z) Vec3Offset(double dx, double dy, double dz) => Vec2OffsetZ(dx, dy, Z.ToDouble() + dz);
    public (double X, double Y, double Z) Vec3Offset((double X, double Y, double Z) offset) => Vec3Offset(offset.X, offset.Y, offset.Z);
    public (double X, double Y) Vec2Angle(double length, double angleDegrees)
    {
        var offset = AngleToVector(angleDegrees, length);
        return Vec2Offset(offset.X, offset.Y);
    }
    public (double X, double Y, double Z) Vec3Angle(double length, double angleDegrees, double dz)
    {
        var position = Vec2Angle(length, angleDegrees);
        return (position.X, position.Y, Z.ToDouble() + dz);
    }
    internal bool HasGravityOverride { get; set; }
    public void VelFromAngle() => VelFromAngle(MovementSpeed.ToDouble());
    public double VelXYToSpeed()
    {
        var x = VelocityX.ToDouble(); var y = VelocityY.ToDouble();
        return Math.Sqrt(x * x + y * y);
    }

    public double VelToSpeed()
    {
        var x = VelocityX.ToDouble(); var y = VelocityY.ToDouble(); var z = VelocityZ.ToDouble();
        return Math.Sqrt(x * x + y * y + z * z);
    }

    public void AngleFromVel() => Angle = BamAngle.FromDegrees(
        Math.Atan2(VelocityY.ToDouble(), VelocityX.ToDouble()) * 180 / Math.PI);
    public void Thrust() => Thrust(MovementSpeed.ToDouble());
    public void Thrust(double speed) => Thrust(Angle, speed);
    public void Thrust(BamAngle angle, double speed)
    {
        var impulse = AngleToVector(angle.ToDegrees(), speed);
        VelocityX = Fixed.FromDouble(VelocityX.ToDouble() + impulse.X);
        VelocityY = Fixed.FromDouble(VelocityY.ToDouble() + impulse.Y);
    }

    public void Thrust((double X, double Y, double Z) velocity)
    {
        VelocityX = Fixed.FromDouble(VelocityX.ToDouble() + velocity.X);
        VelocityY = Fixed.FromDouble(VelocityY.ToDouble() + velocity.Y);
        VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble() + velocity.Z);
    }

    public void VelFromAngle(double speed) => VelFromAngle(speed, Angle);
    public void VelFromAngle(double speed, BamAngle angle)
    {
        var velocity = AngleToVector(angle.ToDegrees(), speed);
        VelocityX = Fixed.FromDouble(velocity.X);
        VelocityY = Fixed.FromDouble(velocity.Y);
    }

    public static (double X, double Y) AngleToVector(double angleDegrees, double length = 1)
    {
        var radians = angleDegrees * Math.PI / 180;
        return (length * Math.Cos(radians), length * Math.Sin(radians));
    }

    public static (double X, double Y) RotateVector(double x, double y, double angleDegrees)
    {
        var rotation = AngleToVector(angleDegrees);
        return (x * rotation.X - y * rotation.Y, y * rotation.X + x * rotation.Y);
    }

    public static double Normalize180(double angleDegrees) =>
        unchecked((int)BamAngle.FromDegrees(angleDegrees).Raw) * (90.0 / BamAngle.Angle90);

    public void Vel3DFromAngle(double pitchDegrees, double speed) => Vel3DFromAngle(Angle, pitchDegrees, speed);
    public void Vel3DFromAngle(BamAngle angle, double pitchDegrees, double speed)
    {
        var pitch = pitchDegrees * Math.PI / 180;
        VelFromAngle(speed * Math.Cos(pitch), angle);
        VelocityZ = Fixed.FromDouble(-speed * Math.Sin(pitch));
    }
    /// <summary>Native DamageFactor, applied to incoming ordinary damage before armor.</summary>
    public Fixed DamageFactor { get; set; } = Fixed.FromInt(1);
    private readonly Dictionary<string, double> _damageFactors = new(StringComparer.OrdinalIgnoreCase);

    public void SetDamageFactor(string damageType, double factor)
    {
        ArgumentException.ThrowIfNullOrEmpty(damageType);
        if (!double.IsFinite(factor)) throw new ArgumentOutOfRangeException(nameof(factor));
        _damageFactors[damageType] = factor;
    }

    internal Dictionary<string, double> CaptureDamageFactors() => new(_damageFactors, StringComparer.OrdinalIgnoreCase);

    internal void RestoreDamageFactors(IReadOnlyDictionary<string, double>? factors)
    {
        _damageFactors.Clear();
        if (factors is null) return;
        foreach (var entry in factors) SetDamageFactor(entry.Key, entry.Value);
    }

    internal uint MixDamageFactorChecksum(uint hash)
    {
        if (_damageFactors.Count == 0) return hash;
        hash = DamageRuleChecksum.Mix(hash, 0x41444641u);
        hash = DamageRuleChecksum.Mix(hash, (uint)_damageFactors.Count);
        foreach (var entry in _damageFactors.OrderBy(pair => pair.Key.ToUpperInvariant(), StringComparer.Ordinal))
            hash = DamageRuleChecksum.Entry(hash, entry.Key, entry.Value);
        return hash;
    }

    internal int ApplyTypedDamageFactor(int damage, string? damageType)
    {
        var type = string.IsNullOrEmpty(damageType) ? "None" : damageType;
        if (!_damageFactors.TryGetValue(type, out var factor))
        {
            if (type.Equals("None", StringComparison.OrdinalIgnoreCase)) return damage;
            var hasFallback = _damageFactors.TryGetValue("None", out factor) && factor >= 0;
            var definition = Simulation?.DamageTypes.Find(type);
            if (definition is { } global)
                factor = hasFallback && !global.ReplaceFactor ? factor * global.Factor : global.Factor;
            else if (!hasFallback) return damage;
        }
        return (int)Math.Clamp(damage * factor, int.MinValue, int.MaxValue);
    }
    /// <summary>Native DamageMultiply, applied to this source's outgoing ordinary damage.</summary>
    public Fixed DamageMultiplier { get; set; } = Fixed.FromInt(1);
    /// <summary>Native MeleeRange default: 64 minus MELEEDELTA (20).</summary>
    public Fixed MeleeRange { get; set; } = Fixed.FromInt(44);
    public bool NoVerticalMeleeRange { get; set; }
    public bool Killed { get; set; }
    internal bool HasMovementActionOverride { get; set; }
    internal bool HasTargetMemoryOverride { get; set; }
    /// <summary>Native actor friction multiplier, applied to surface friction.</summary>
    public Fixed Friction { get; set; } = Fixed.FromInt(1);
    /// <summary>Native MF6_NOTRIGGER: suppress automatic movement line triggers.</summary>
    public bool NoTrigger { get; set; }
    /// <summary>Native actor Score property; independent of player frag statistics.</summary>
    public int Score { get; set; }
    /// <summary>Native DamageVal; projectiles may also supply a damage expression.</summary>
    public int Damage { get; set; }
    internal int? SpawnDamage { get; set; }
    /// <summary>Native SetDamage; projectile overrides also clear their damage expression.</summary>
    public virtual void SetDamage(int damage) => Damage = damage;
    /// <summary>Native IsZeroDamage; projectile overrides also check for a damage expression.</summary>
    public virtual bool IsZeroDamage() => Damage == 0;
    /// <summary>Native MBF sentience query: alive and has a See state, independent of brain or monster flags.</summary>
    public bool IsSentient() => Health > 0 && States.HasState(SeeState);
    /// <summary>Native CheckMeleeRange against this actor's current target.</summary>
    public bool CheckMeleeRange(double range = -1) => ActorJumpActions.CheckMeleeRange(this, range);
    private uint? _goalId;
    /// <summary>Native goal pointer identity used by the melee arrival check.</summary>
    public uint? GoalId
    {
        get
        {
            // Native object read barriers clear references to destroyed objects.
            if (_goalId.HasValue && Simulation is { } sim
                && !sim.Actors.Any(actor => actor.Id == _goalId && !actor.Destroyed))
                _goalId = null;
            return _goalId;
        }
        set => _goalId = value == 0 ? null : value;
    }
    /// <summary>Native TriggerPainChance action result: whether a pain animation was entered.</summary>
    public bool TriggerPainChance(string? damageType = null, bool forcedPain = false) =>
        ActorDamage.TriggerPainChance(this, damageType, forcedPain);
    /// <summary>Native MF_DROPPED; independent of ammo skill handling and pickup amount.</summary>
    public bool Dropped { get; set; }
    private int _reactionTime;
    internal int? SpawnReactionTime { get; set; }
    private bool _reactionTimeInitialized;
    internal bool ReactionTimeInitialized => _reactionTimeInitialized;
    /// <summary>Native actor reaction counter; attached managed brains use this same state.</summary>
    public int ReactionTime
    {
        get => _reactionTime;
        set { _reactionTime = value; _reactionTimeInitialized = true; }
    }
    /// <summary>Native <c>TIDtoHate</c>. Teammates share this value; a shooter may hurt or wake actors whose <see cref="ThingId"/> matches.</summary>
    private int _tidToHate;
    public int TidToHate
    {
        get => _tidToHate;
        set { _tidToHate = value; HasFriendshipOverride = true; }
    }
    internal bool HasFriendshipOverride { get; set; }
    /// <summary>Native <c>MF3_NOTARGET</c>. Wake-up ignores this actor unless <see cref="TidToHate"/> matches its <see cref="ThingId"/> or it is hostile.</summary>
    public bool NoTarget { get; set; }
    /// <summary>Native <c>MF2_ONMOBJ</c>. The actor is standing on another solid actor's top.</summary>
    public bool OnMobj { get; set; }
    /// <summary>Native <c>MF3_ISMONSTER</c>. Infighting-off damage gates apply only between monsters.</summary>
    public bool IsMonster { get; set; }
    public bool NoBlockMonsters { get; set; }
    /// <summary>Native <c>MF7_HARMFRIENDS</c>. At standard infighting, this shooter may hurt friendlies.</summary>
    public bool HarmFriends { get; set; }
    public double SpawnZOffset { get; init; }
    public bool SpawnCeiling { get; set; }
    public bool NoTeleport { get; set; }
    public bool CanSlide { get; set; }
    public bool Dormant { get; set; }
    public Fixed X { get; set; }
    public Fixed Y { get; set; }
    public Fixed Z { get; set; }
    public Fixed VelocityX { get; set; }
    public Fixed VelocityY { get; set; }
    public Fixed VelocityZ { get; set; }
    public Fixed PreviousZ { get; private set; }
    public int SectorIndex { get; internal set; } = -1;
    public bool OnGround { get; internal set; } = true;
    public bool NoGravity { get; set; }
    public bool Fly { get; set; }
    public bool NoFriction { get; set; }
    public bool Corpse { get; set; }
    public bool DontCorpse { get; set; }
    public bool Falling { get; set; }
    public bool DontFall { get; set; }
    /// <summary>Native MF_PICKUP. Allows movement contact to collect inventory.</summary>
    public bool CanPickupItems { get; set; }
    /// <summary>Native MF_SPECIAL. Enables this item's movement-contact pickup.</summary>
    public bool SpecialPickup { get; set; }
    internal bool SpawnCanPickupItems { get; set; }
    internal bool SpawnSpecialPickup { get; set; }
    public bool Floating { get; set; }
    public bool InFloat { get; set; }
    public bool VerticalFriction { get; set; }
    public double FloatSpeed { get; set; } = 4;
    internal bool HasTuningOverride { get; set; }
    internal SimActorTuning? SpawnTuning { get; set; }
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    internal bool HasScaleOverride { get; set; }
    public int FloatBobPhase { get; set; }
    internal bool HasFloatBobPhaseOverride { get; set; }
    public double SpriteAngle { get; set; }
    public double SpriteRotation { get; set; }
    internal bool HasSpriteOrientationOverride { get; set; }
    internal bool HasSizeOverride { get; set; }
    public bool NoRadiusDamage { get; set; }
    public bool NoSectorDamage { get; set; }
    public bool ForceSectorDamage { get; set; }
    public bool Ambush { get; set; }
    public int PainChance { get; set; } = 256;
    /// <summary>Native <c>PainThreshold</c>. A surviving hit below this post-armor amount does not flinch. 0 flinches on any loss.</summary>
    public int PainThreshold { get; set; }
    public int ResurrectionHealth { get; internal set; }
    public int SpawnHealth() => ResurrectionHealth;
    public int RaiseDuration { get; internal set; }
    internal Fixed? ResurrectionRadius { get; set; }
    internal Fixed? ResurrectionHeight { get; set; }
    internal int? ResurrectionCollisionFlags { get; set; }
    internal int? ResurrectionDefenseFlags { get; set; }
    internal int? ResurrectionMovementFlags { get; set; }
    public int Mass { get; set; } = 100;
    internal int SpawnMass { get; set; } = 100;
    internal bool HasDefensePropertyOverride { get; set; }
    /// <summary>Native actor Speed in ACS signed 16.16 units.</summary>
    public Fixed MovementSpeed { get; set; } = Fixed.FromInt(4);
    public double ChaseSpeed { get => MovementSpeed.ToDouble() / 4; set => MovementSpeed = Fixed.FromDouble(value * 4); }
    public bool Solid { get; set; } = true;
    public bool Shootable { get; set; } = true;
    public bool Invulnerable { get; set; }
    /// <summary>Native MF3_FOILINVUL on a damage inflictor.</summary>
    public bool FoilInvul { get; set; }
    /// <summary>Native MF5_PIERCEARMOR on a damage inflictor.</summary>
    public bool PierceArmor { get; set; }
    /// <summary>Native MF4_STRIFEDAMAGE missile dice: one through four.</summary>
    public bool StrifeDamage { get; set; }
    public bool Rip { get; set; }
    public bool DontRip { get; set; }
    public bool NoBossRip { get; set; }
    public bool Pushable { get; set; }
    public bool CannotPush { get; set; }
    private double _pushFactor = 0.25;
    public double PushFactor
    {
        get => _pushFactor;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            _pushFactor = value;
        }
    }
    public int RipperLevel { get; set; }
    public int RipLevelMin { get; set; }
    public int RipLevelMax { get; set; }
    public Fixed ProjectilePassHeight { get; set; }
    public bool CanBeRippedBy(Actor projectile) => !DontRip
        && !(projectile.NoBossRip && Boss)
        && (RipLevelMin <= 0 || projectile.RipperLevel >= RipLevelMin)
        && (RipLevelMax <= 0 || projectile.RipperLevel <= RipLevelMax);
    /// <summary>Native <c>MF7_BUDDHA</c>. A killing blow stops at 1 health unless it is a telefrag or forced.</summary>
    public bool Buddha { get; set; }
    /// <summary>Native <c>MF7_FOILBUDDHA</c>. As an inflictor, this kills a non-player Buddha.</summary>
    public bool FoilBuddha { get; set; }
    /// <summary>Native <c>MF6_FORCEPAIN</c>. As an inflictor, this flinches through the pain threshold and the pain roll.</summary>
    public bool ForcePain { get; set; }
    /// <summary>Native <c>MF5_NOPAIN</c>. This actor does not flinch, even when the inflictor forces pain.</summary>
    public bool NoPain { get; set; }
    /// <summary>Native <c>MF5_PAINLESS</c>. As an inflictor, this hit does not flinch, even when forced pain is also set.</summary>
    public bool Painless { get; set; }
    /// <summary>Native <c>MF4_EXTREMEDEATH</c>. As an inflictor, this killing blow can use the extreme death frame without passing the gib line.</summary>
    public bool ExtremeDeath { get; set; }
    /// <summary>Native <c>MF4_NOEXTREMEDEATH</c>. As an inflictor, this hit never uses an extreme death frame, even past the gib line.</summary>
    public bool NoExtremeDeath { get; set; }
    /// <summary>Native <c>RF_FULLBRIGHT</c> from an electric hit that passed the pain roll but missed the pain flicker.</summary>
    public bool FullBright { get; set; }
    /// <summary>Native <c>MF_JUSTHIT</c>. Set when a pain flinch lands from the current or no chase target.</summary>
    public bool JustHit { get; set; }
    public Fixed MaxStepHeight { get; set; } = Fixed.FromInt(24);
    /// <summary>Native MaxDropOffHeight. A drop strictly taller than this is refused unless <see cref="AllowDropOff"/> or <see cref="Floating"/>.</summary>
    public Fixed MaxDropOffHeight { get; set; } = Fixed.FromInt(24);
    /// <summary>Native MF_DROPOFF. Players and projectiles may walk off ledges. Monsters may not.</summary>
    public bool AllowDropOff { get; set; }
    /// <summary>Native <c>MF4_ACTLIKEBRIDGE</c>. A grounded monster can step onto this solid actor.</summary>
    public bool ActsLikeBridge { get; set; }
    /// <summary>Native <c>MF_ICECORPSE</c>. This actor collides with corpses and can stand on them.</summary>
    public bool IceCorpse { get; set; }
    /// <summary>Native <c>MF8_INSCROLLSEC</c>. Set each tic for non-players touching an active carry scroller.</summary>
    public bool InScrollSector { get; set; }
    /// <summary>Native <c>MF6_SHATTERING</c>. A frozen corpse is breaking apart.</summary>
    public bool Shattering { get; set; }
    /// <summary>Native <c>MF7_ICESHATTER</c>. As an inflictor, ice damage does not shatter a frozen corpse.</summary>
    public bool IceShatter { get; set; }
    /// <summary>Native <c>MF7_NEVERTARGET</c>. Wake-up will not chase this actor.</summary>
    public bool NeverTarget { get; set; }
    /// <summary>Native MF9_NOAUTOOFFSKULLFLY: zero horizontal velocity does not automatically end a skull charge.</summary>
    public bool NoAutoOffSkullFly { get; set; }
    /// <summary>Native MF3_NOEXPLODEFLOOR: a missile stops vertically on the floor without exploding.</summary>
    public bool NoExplodeFloor { get; set; }
    /// <summary>Native MF3_CEILINGHUGGER: missiles clamp to the ceiling without exploding.</summary>
    public bool CeilingHugger { get; set; }
    /// <summary>Native MF3_FLOORHUGGER: missiles follow destination floors.</summary>
    public bool FloorHugger { get; set; }
    /// <summary>Native MF5_NODROPOFF: enforces the supported flat dropoff limit and missile floor-hugger exception.</summary>
    public bool NoDropOff { get; set; }
    /// <summary>Native MF2_BLASTED: bypasses the supported dropoff limit until horizontal motion stops.</summary>
    public bool Blasted { get; set; }
    public bool Boss { get; set; }
    public bool DontBlast { get; set; }
    /// <summary>Native MF2_THRUACTORS: skip supported actor contact checks on either participant.</summary>
    public bool ThruActors { get; set; }
    /// <summary>Native MF6_MTHRUSPECIES: missiles skip direct contact with their owner's species.</summary>
    public bool MThruSpecies { get; set; }
    /// <summary>Native MF6_THRUSPECIES: this mover passes actors of its own supported species.</summary>
    public bool ThruSpecies { get; set; }
    public bool Ghost { get; set; }
    public bool ThruGhost { get; set; }
    /// <summary>Native MF2_NONSHOOTABLE: skip supported direct missile contact.</summary>
    public bool NonShootable { get; set; }
    /// <summary>Native MF8_HITOWNER: a missile may contact its owner.</summary>
    public bool HitOwner { get; set; }
    public bool Spectral { get; set; }
    public bool AllowThruBits { get; set; }
    public uint ThruBits { get; set; }
    internal bool SharesEnabledThruBits(Actor other) => (ThruBits & other.ThruBits) != 0
        && (AllowThruBits || other.AllowThruBits);
    /// <summary>Native <c>MF4_NOTARGETSWITCH</c>. Wake-up will not pick a new chase target while one is alive.</summary>
    public bool NoTargetSwitch { get; set; }
    /// <summary>Native <c>MF4_NOHATEPLAYERS</c>. <see cref="OkayToSwitchTarget"/> ignores player sources.</summary>
    public bool NoHatePlayers { get; set; }
    /// <summary>Native <c>MF4_QUICKTORETALIATE</c>. Wake-up may switch targets while chase threshold is still positive.</summary>
    public bool QuickToRetaliate { get; set; }
    /// <summary>Native <c>MF_FRIENDLY</c>. Used with <see cref="FriendPlayer"/> for <see cref="IsFriend"/>.</summary>
    public bool Friendly { get; set; }
    internal bool SpawnFriendly { get; set; }
    /// <summary>Native <c>FriendPlayer</c>. 0 means any friendly. 1 is the first player.</summary>
    private int _friendPlayer;
    public int FriendPlayer
    {
        get => _friendPlayer;
        set { _friendPlayer = value; HasFriendshipOverride = true; }
    }
    /// <summary>Native <c>MF5_NOINFIGHTING</c>. Wake-up treats infighting as off for this actor.</summary>
    public bool NoInfighting { get; set; }
    /// <summary>Native MF7_NOINFIGHTSPECIES target-switch restriction.</summary>
    public bool NoInfightSpecies { get; set; }
    public int InfightingGroup { get; set; }
    public int ProjectileGroup { get; set; }
    public int SplashGroup { get; set; }
    public bool SplashImmune(Actor explosion) => SplashGroup != 0 && SplashGroup == explosion.SplashGroup;
    /// <summary>Native <c>MF7_FORCEINFIGHTING</c>. Standard infighting applies when the level is set to none.</summary>
    public bool ForceInfighting { get; set; }
    /// <summary>Native <c>MF6_DOHARMSPECIES</c>. Same-species projectile immunity does not apply.</summary>
    public bool DoHarmSpecies { get; set; }
    /// <summary>Native <c>IsFriend</c> subset. Deathmatch teamplay and designated teams are absent.</summary>
    public bool IsFriend(Actor other)
    {
        if (!Friendly || !other.Friendly) return false;
        if (Simulation is { GameMode: not SpawnGameMode.Deathmatch }) return true;
        if (FriendPlayer == 0 || other.FriendPlayer == 0) return true;
        return FriendPlayer == other.FriendPlayer;
    }
    /// <summary>Native <c>IsHostile</c> subset. Two plain monsters are never hostile to each other.</summary>
    public bool IsHostile(Actor other)
    {
        if (!Friendly && !other.Friendly) return false;
        if ((Friendly && other.Friendly))
        {
            if (Simulation is { GameMode: not SpawnGameMode.Deathmatch }) return false;
            return FriendPlayer != 0 && other.FriendPlayer != 0 && FriendPlayer != other.FriendPlayer;
        }
        return true;
    }
    /// <summary>Native species subset for default <c>P_ProjectileImmune</c>. Uses resolved class identity and vanilla monster ancestry.</summary>
    public bool IsSameSpecies(Actor other) =>
        SharesContactSpecies(other) && this is not PlayerPawn && other is not PlayerPawn;
    public string? Species { get; set; }
    internal bool HasSpeciesOverride { get; set; }
    internal string SpeciesName => !string.IsNullOrEmpty(Species) && !Species.Equals("None", StringComparison.OrdinalIgnoreCase)
        ? Species : this is PlayerPawn ? "DoomPlayer"
        : DoomActorCatalog.TrySpawnClassName(DefaultSpecies, out var name) ? name
        : "Class:" + DefaultSpecies.ToString(System.Globalization.CultureInfo.InvariantCulture);
    // GetSpecies climbs monster parents: Spectre : Demon and HellKnight : BaronOfHell.
    private int DefaultSpecies => ClassDoomEdNum switch { 58 => 3002, 69 => 3003, _ => ClassDoomEdNum };
    // All supported Doom player starts share DoomPlayer's collision species.
    // Damage species immunity deliberately excludes players; contact filtering does not.
    internal bool SharesContactSpecies(Actor other) => SpeciesName.Equals(other.SpeciesName, StringComparison.OrdinalIgnoreCase);
    /// <summary>Native <c>P_ProjectileImmune</c> group and default species rules.</summary>
    public bool ProjectileImmune(Actor source) =>
        (ProjectileGroup != -1 || ReferenceEquals(this, source))
        && (ProjectileGroup == 0 ? SharesContactSpecies(source) && !DoHarmSpecies : ProjectileGroup == source.ProjectileGroup);
    /// <summary>Native <c>CanAttackHurt</c> subset for monster-monster damage.</summary>
    internal bool CanAttackHurtFrom(Actor shooter)
    {
        if (this is PlayerPawn || shooter is PlayerPawn) return true;
        var sim = Simulation;
        if (sim == null) return true;
        var infight = sim.GetInfightLevel(this);
        if (infight < 0)
        {
            if (shooter.Shootable && IsMonster && !IsHostile(shooter)
                && (ThingId == 0 || shooter.TidToHate != ThingId))
                return false;
        }
        else if (infight == 0)
        {
            if (IsFriend(shooter) && !shooter.HarmFriends) return false;
            if (TidToHate != 0 && TidToHate == shooter.TidToHate) return false;
            if (ProjectileImmune(shooter) && !IsHostile(shooter)
                && (ThingId == 0 || shooter.TidToHate != ThingId))
                return false;
        }
        return true;
    }

    /// <summary>Native <c>OkayToSwitchTarget</c> subset for represented actor classes.</summary>
    internal bool OkayToSwitchTarget(Actor other, MonsterBrain brain)
    {
        if (!other.Shootable || other.Destroyed || other.Id == Id || other.NeverTarget)
            return false;
        if (NoTargetSwitch && brain.TargetId != null)
            return false;
        var sim = Simulation;
        var master = sim?.Actors.FirstOrDefault(candidate => candidate.Id == MasterId && !candidate.Destroyed);
        var otherMaster = sim?.Actors.FirstOrDefault(candidate => candidate.Id == other.MasterId && !candidate.Destroyed);
        var masterClass = master != null && master.ClassDoomEdNum > 0 && other.ClassDoomEdNum == master.ClassDoomEdNum;
        var minionClass = otherMaster != null && otherMaster.ClassDoomEdNum > 0 && ClassDoomEdNum == otherMaster.ClassDoomEdNum;
        if ((masterClass || minionClass) && !IsHostile(other)
            && (other.ThingId != TidToHate || TidToHate == 0) && other.TidToHate == TidToHate)
            return false;
        if (NoInfightSpecies && SharesContactSpecies(other))
            return false;
        if (InfightingGroup != 0 && InfightingGroup == other.InfightingGroup)
            return false;
        if (other.NoTarget && (other.ThingId != TidToHate || TidToHate == 0) && !IsHostile(other))
            return false;
        if (brain.Threshold != 0 && !QuickToRetaliate)
            return false;
        if (IsFriend(other))
            return false;
        var infight = sim?.GetInfightLevel(this) ?? 1;
        if (infight < 0 && other is not PlayerPawn && !IsHostile(other))
            return false;
        if (TidToHate != 0 && TidToHate == other.TidToHate)
            return false;
        if (other is PlayerPawn && NoHatePlayers)
            return false;
        if (other is not PlayerPawn && infight == 0 && ProjectileImmune(other) && !IsHostile(other)
            && (other.TidToHate == 0 || other.TidToHate != ThingId))
            return false;
        if (sim != null && brain.TargetId is { } currentId && other.Id != currentId && TidToHate != 0)
        {
            var current = sim.Actors.FirstOrDefault(candidate => candidate.Id == currentId);
            if (current != null && !current.IsDead && !current.Destroyed && current.ThingId == TidToHate
                && sim.NextSwitchTargetRandom() % 256 < 128
                && CombatTrace.HasLineOfSight(sim, this, current))
                return false;
        }
        return true;
    }
    /// <summary>Native <c>MF6_NOTELEFRAG</c>. A spawn stomp skips this actor.</summary>
    public bool NoTelefrag { get; set; }
    /// <summary>Native <c>MF7_ALWAYSTELEFRAG</c>. A spawn stomp kills this actor even when <see cref="NoTelefrag"/> is set.</summary>
    public bool AlwaysTelefrag { get; set; }
    /// <summary>Native <c>MF5_DONTDRAIN</c>. A draining player gains nothing from this actor.</summary>
    public bool DontDrain { get; set; }
    internal AuthoritySimulation? Simulation { get; set; }
    private MonsterBrain? _brain;
    public MonsterBrain? Brain
    {
        get => _brain;
        set
        {
            if (ReferenceEquals(_brain, value)) return;
            value?.ValidateOwner(this);
            if (!_reactionTimeInitialized && value != null) ReactionTime = value.ReactionTics;
            _brain?.DetachOwner(this);
            _brain = value;
            _brain?.AttachOwner(this);
        }
    }
    public Fixed PreviousX { get; private set; }
    public Fixed PreviousY { get; private set; }
    public BamAngle Angle { get; set; }
    /// <summary>Native <c>AActor::Angles.Roll</c> for ACS roll helpers.</summary>
    public BamAngle Roll { get; set; }
    /// <summary>Native <c>TeleFogSourceType</c> override from ACS <c>SetActorTeleFog</c>.</summary>
    public string? TeleFogSource { get; set; }
    internal bool HasTeleFogOverride { get; set; }
    /// <summary>Native <c>TeleFogDestType</c> override from ACS <c>SetActorTeleFog</c>.</summary>
    public string? TeleFogDest { get; set; }
    private double _pitchDegrees;
    public double PitchDegrees
    {
        get => _pitchDegrees;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            _pitchDegrees = Fixed.FromDouble(this is PlayerPawn ? Math.Clamp(value, -89, 89) : value).ToDouble();
        }
    }

    private int _health = 100;
    public int Health
    {
        get => _health;
        set => SetHealth(value, null, false);
    }

    internal void SetDamageHealth(int value, Actor? source, Func<int> afterDamage) => SetHealth(value, source, true, afterDamage);

    private void SetHealth(int value, Actor? source, bool executeDeathSpecial, Func<int>? afterDamage = null)
    {
        var dead = _health <= 0;
        _health = value;
        // Native drain observes the damaged health before death specials and state selection.
        if (afterDamage != null) _health = afterDamage();
        if (dead && !IsDead)
        {
            DeathDamageType = null;
            Corpse = false;
            if (ResurrectionCollisionFlags is { } collisionFlags)
                Shootable = (collisionFlags & 2) != 0;
        }
        if (!dead && IsDead)
        {
            if (executeDeathSpecial)
            {
                Shootable = false;
                Floating = false;
                Brain?.ClearDeathCharge();
                AllowDropOff = true;
                if (!DontFall) NoGravity = false;
            }
            if (!DontCorpse && (IsMonster || this is PlayerPawn || RaiseDuration > 0))
                Corpse = true;
            if (executeDeathSpecial) Killed = true;
            if (executeDeathSpecial)
            {
                var incomingType = ResolveDeathType(DamageTypeReceived, DeathInflictor);
                if (!(this is PlayerPawn && string.Equals(incomingType, "Fire", StringComparison.OrdinalIgnoreCase)
                    && DeathInflictor is { SpecialFireDamage: true } && (_health <= -50 || DeathDamageAmount <= 25)))
                    DamageType = incomingType;
            }
            // Native Die queries gib health before death specials, then again for state selection.
            if (executeDeathSpecial) _ = GetGibHealth();
            DeathDamageType = string.Equals(DamageType, "Massacre", StringComparison.OrdinalIgnoreCase) ? "Massacre" : null;
            DeathCount++;
            if (executeDeathSpecial && Simulation is { } deathSimulation)
                ActorSpecialActions.OnDeath(deathSimulation, this, source);
            var gibHealth = GetGibHealth();
            var death = ChooseDeathState(gibHealth, out var extremeDeath);
            if (extremeDeath && this is not PlayerPawn && _health >= gibHealth)
                _health = gibHealth - 1;
            if (States.HasState(death)) States.Enter(this, death);
            if (this is PlayerPawn player)
            {
                player.ClearDamagePowers();
                player.ExtremelyDead = extremeDeath;
                player.ClearCommands();
                player.AttackPressed = false;
                player.NoteDeath(Simulation);
            }
        }
        else if (dead && !IsDead && States.HasState(SpawnState))
        {
            States.Enter(this, SpawnState);
            if (this is PlayerPawn revived)
                revived.ExtremelyDead = false;
        }
    }
    public ActorStateMachine States { get; } = new();
    public int SpawnState { get; set; } = ActorStateMachine.Spawn;
    internal bool HasOnlySpawnLabel { get; set; }
    internal int GenericCrushState { get; set; } = -1;
    internal int NullState { get; set; } = -1;
    public int ActiveState { get; set; } = -1;
    public int InactiveState { get; set; } = -1;
    /// <summary>Native See state. A waking monster in <see cref="SpawnState"/> can enter this frame. -1 means absent.</summary>
    public int SeeState { get; set; } = -1;
    public int PainState { get; set; } = ActorStateMachine.Pain;
    public int RaiseState { get; set; } = -1;

    public int? GetRaiseState() => Corpse && this is not PlayerPawn
        && (States.RemainingTics == -1 || States.CurrentCanRaise)
        && States.HasState(RaiseState) ? RaiseState : null;
    public int DeathState { get; set; } = ActorStateMachine.Death;
    /// <summary>Optional Death.Extreme state. Absent until a table actually contains it.</summary>
    public int ExtremeDeathState { get; set; } = -1;
    /// <summary>Native GenericFreezeDeath. An Ice kill uses this when no Ice death is registered. -1 means absent.</summary>
    public int GenericFreezeDeath { get; set; } = -1;
    /// <summary>Native <c>MF4_NOICEDEATH</c>. An Ice kill does not fall back to <see cref="GenericFreezeDeath"/>.</summary>
    public bool NoIceDeath { get; set; }
    /// <summary>Native inflictor DeathType override. None leaves the hit's damage type unchanged.</summary>
    public string? DeathType { get; set; }
    /// <summary>Native MF5_SPECIALFIREDAMAGE. Player flame death requires shallow overkill and damage above 25.</summary>
    public bool SpecialFireDamage { get; set; }
    /// <summary>Native persistent actor DamageType, also used by death-state selection.</summary>
    public string? DamageType { get; set; }
    internal int DeathDamageAmount { get; set; }
    /// <summary>Damage type of the hit currently being applied. The setter consumes it.</summary>
    internal string? DamageTypeReceived { get; set; }
    public string? DeathDamageType { get; internal set; }
    /// <summary>Inflictor for the hit currently being applied. The health setter consumes it.</summary>
    internal Actor? DeathInflictor { get; set; }
    private readonly Dictionary<string, int> _typedDeaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedExtremeDeaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedPain = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedPainChance = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedWounds = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Native <c>WoundHealth</c>. A surviving hit at or below this health can enter a typed wound frame.</summary>
    public int WoundHealth { get; set; }
    /// <summary>Native GetGibHealth. A killing blow below this value can enter <see cref="ExtremeDeathState"/>.</summary>
    public int GibHealth { get; set; } = -100;
    public virtual int GetGibHealth() => GibHealth;
    public virtual int GetMaxHealth(bool withUpgrades) => 100;
    /// <summary>
    /// BasicArmor amount for a non-player. Players absorb from <see cref="PlayerPawn.Inventory"/> instead.
    /// Spawned monsters start at 0, which is why a bare hit is not reduced.
    /// </summary>
    public int Armor { get; set; }
    /// <summary>BasicArmor save percent. 33 is Doom green armor's exact one-third. 0 saves nothing.</summary>
    public int ArmorSavePercent { get; set; }
    /// <summary>BasicArmor total save cap. 0 means no cap.</summary>
    public int MaxAbsorb { get; set; }
    /// <summary>BasicArmor amount saved at 100% before the percent applies.</summary>
    public int MaxFullAbsorb { get; set; }
    /// <summary>Saved so far. A new suit does not clear it.</summary>
    public int AbsorbCount { get; set; }
    /// <summary>Inventory.bIgnoreSkill for supported ammo and armor pickup grants.</summary>
    public bool IgnoreAmmoSkill { get; set; }
    /// <summary>Native <c>BackpackItem.bDepleted</c>. A tossed backpack raises caps and gives no ammo.</summary>
    public bool Depleted { get; set; }
    /// <summary>ACS <c>DropItem</c> amount override. Zero uses the catalog default.</summary>
    public int PickupAmount { get; set; }
    public int PickupDelay { get; internal set; }
    public bool SuppressWeaponPickupAmmo { get; internal set; }
    public bool? AlwaysPickupOverride { get; internal set; }
    public int DeathCount { get; private set; }
    public uint? LastDamageSourceId { get; internal set; }
    public uint? LastHeardTargetId { get; internal set; }
    /// <summary>Mover crush on this tic. Suppresses duplicate sector pinch crush the same tic.</summary>
    internal int MoverCrushTic { get; private set; }
    internal void MarkMoverCrush(int tic) => MoverCrushTic = tic;
    internal void RestoreHealth(int health) => _health = health;
    public Fixed Radius { get; set; } = Fixed.FromInt(20);
    public Fixed Height { get; set; } = Fixed.FromInt(56);
    public PlayLevel? Level { get; init; }

    public bool IsDead => Health <= 0;
    /// <summary>Native AActor.Massacre: attempt repeated Massacre damage and report whether the actor died.</summary>
    public bool Massacre() => ThingDamage.Massacre(this);
    /// <summary>Native GiveBody: negative amounts raise health to a percentage of the resolved maximum.</summary>
    public bool GiveBody(int amount, int maximum = 0) => this is PlayerPawn player
        ? PickupCatalog.GiveHealth(player, amount, maximum)
        : PickupCatalog.GiveHealth(this, amount);

    public bool BlocksActors =>
        Solid && !IsDead && !Destroyed && DoomEdNum != LineSpecials.TeleportDestType
        && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public bool CanTakeDamage => Shootable && !IsDead && !Destroyed
        && DoomEdNum != LineSpecials.TeleportDestType && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public static bool IsPlayerStart(int type) => type is >= PlayerStartMin and <= PlayerStartMax;

    /// <summary>
    /// <c>Death.Fire</c> or <c>Death.Extreme.Fire</c>. The name match follows native case-insensitive FName identity.
    /// A missing frame falls through. An Ice kill can still use <see cref="GenericFreezeDeath"/>.
    /// </summary>
    public void SetTypedDeath(string damageType, int state, bool extreme = false)
    {
        if (string.IsNullOrEmpty(damageType) || damageType.Equals("None", StringComparison.OrdinalIgnoreCase)
            || damageType.Equals("Extreme", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A typed death needs a damage type other than None or Extreme.", nameof(damageType));
        (extreme ? _typedExtremeDeaths : _typedDeaths)[damageType] = state;
    }

    internal IEnumerable<(string Type, int State, bool Extreme)> TypedDeaths()
    {
        foreach (var entry in _typedDeaths.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            yield return (entry.Key, entry.Value, false);
        foreach (var entry in _typedExtremeDeaths.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            yield return (entry.Key, entry.Value, true);
    }

    private static string? ResolveDeathType(string? damageType, Actor? inflictor) =>
        !string.IsNullOrEmpty(inflictor?.DeathType) && !string.Equals(inflictor.DeathType, "None", StringComparison.OrdinalIgnoreCase)
            ? inflictor.DeathType : damageType;

    internal bool AcceptsSpecialDamage(string? damageType, Actor? inflictor)
    {
        if (States.HasState(DeathState)
            || !HasSpecialDeathStates()
            || string.Equals(damageType, "Massacre", StringComparison.OrdinalIgnoreCase)) return true;
        var type = ResolveDeathType(damageType, inflictor);
        if (string.Equals(type, "Extreme", StringComparison.OrdinalIgnoreCase) && States.HasState(ExtremeDeathState)) return true;
        if (type is not null && _typedDeaths.TryGetValue(type, out var death) && States.HasState(death)) return true;
        return string.Equals(type, "Ice", StringComparison.OrdinalIgnoreCase) && !NoIceDeath && Simulation?.DehackedNoAutofreeze != true
            && (this is PlayerPawn || IsMonster) && States.HasState(GenericFreezeDeath);
    }

    public virtual int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags) =>
        AcceptsSpecialDamage(damageType, inflictor) ? damage : -1;
    public virtual int OnDrain(Actor target, int amount, string? damageType) => amount;
    public virtual bool CanResurrect(Actor? other, bool passive) => true;
    public virtual void OnRevive() { }
    public bool HandleNoDelay { get; set; }
    public bool CheckNoDelay()
    {
        if (!HandleNoDelay || Dormant) return !Destroyed;
        HandleNoDelay = false;
        return States.CheckNoDelay(this);
    }
    public bool AlwaysFast { get; set; }
    public bool NeverFast { get; set; }
    public virtual bool IsFast() => AlwaysFast || !NeverFast && Simulation?.FastMonsters == true;
    public virtual bool IsSlow() => Simulation?.SlowMonsters == true;
    public virtual bool IsClientSide() => false;
    /// <summary>Native active/passive inventory damage modifier boundary.</summary>
    public InventoryDamageModifier? DamageModifiers { get; set; }

    public virtual int GetModifiedDamage(string? damageType, int damage, bool passive, Actor? inflictor, Actor? other, DamageFlags flags) =>
        InventoryDamageModifier.Apply(DamageModifiers, damage, damageType, passive, inflictor, other, flags);

    public virtual int DoSpecialDamage(Actor target, int damage, string? damageType, DamageFlags flags) =>
        target is PlayerPawn { GodMode: true } && damage < 1000 ? -1 : damage;

    public bool HasSpecialDeathStates() => States.HasState(ExtremeDeathState)
        || _typedDeaths.Values.Any(States.HasState);

    /// <summary>
    /// Native death-state order. An extreme typed frame wins, then the plain typed
    /// frame, then generic ice freeze, then <see cref="ExtremeDeathState"/>, then <see cref="DeathState"/>.
    /// A plain typed frame is used even when health is past the gib line.
    /// Damage type <c>Extreme</c> forces the gib state and then clears the type.
    /// Ice freeze is not extreme. It applies to a player or a monster, and <see cref="NoIceDeath"/> blocks it.
    /// Inflictor <see cref="ExtremeDeath"/> and <see cref="NoExtremeDeath"/> follow native <c>MF4_*</c> on the inflictor only.
    /// </summary>
    private int ChooseDeathState(int gibHealth, out bool usedExtremeDeath)
    {
        usedExtremeDeath = false;
        var type = DamageType;
        var inflictor = DeathInflictor;
        var extreme = (_health < gibHealth || inflictor is { ExtremeDeath: true })
            && inflictor is not { NoExtremeDeath: true };
        if (string.Equals(type, "Extreme", StringComparison.OrdinalIgnoreCase))
        {
            extreme = true;
            type = null;
            DamageType = null;
        }
        if (!string.IsNullOrEmpty(type) && !string.Equals(type, "None", StringComparison.OrdinalIgnoreCase))
        {
            if (extreme && _typedExtremeDeaths.TryGetValue(type, out var extremeState) && States.HasState(extremeState))
            {
                usedExtremeDeath = true;
                return extremeState;
            }
            if (_typedDeaths.TryGetValue(type, out var typed) && States.HasState(typed))
                return typed;
            if (string.Equals(type, "Ice", StringComparison.OrdinalIgnoreCase) && !NoIceDeath && Simulation?.DehackedNoAutofreeze != true
                && (this is PlayerPawn || IsMonster) && States.HasState(GenericFreezeDeath))
                return GenericFreezeDeath;
        }
        if (!string.Equals(type, "Massacre", StringComparison.OrdinalIgnoreCase)) DamageType = null;
        if (extreme && States.HasState(ExtremeDeathState))
        {
            usedExtremeDeath = true;
            return ExtremeDeathState;
        }
        return DeathState;
    }

    /// <summary>
    /// <c>Pain.Fire</c>. The name match follows native case-insensitive FName identity. A missing frame uses <see cref="PainState"/>.
    /// <paramref name="chance"/> replaces <see cref="PainChance"/> for that type only.
    /// Electric flicker uses <see cref="AuthoritySimulation.NextFlickerRandom"/>. Poison howling is absent.
    /// </summary>
    /// <summary><c>Wound.Fire</c>. The name match follows native case-insensitive FName identity. A missing frame is ignored.</summary>
    public void SetTypedWound(string damageType, int state)
    {
        if (string.IsNullOrEmpty(damageType) || string.Equals(damageType, "None", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A typed wound needs a damage type other than None.", nameof(damageType));
        _typedWounds[damageType] = state;
    }

    internal IEnumerable<(string Type, int State)> TypedWounds()
    {
        foreach (var entry in _typedWounds.OrderBy(pair => pair.Key, StringComparer.Ordinal))
            yield return (entry.Key, entry.Value);
    }

    internal int WoundStateFor(string? damageType)
    {
        if (!string.IsNullOrEmpty(damageType)
            && !string.Equals(damageType, "None", StringComparison.OrdinalIgnoreCase)
            && _typedWounds.TryGetValue(damageType, out var state)
            && States.HasState(state))
            return state;
        return -1;
    }

    public void SetTypedPain(string damageType, int state, int? chance = null)
    {
        if (string.IsNullOrEmpty(damageType) || string.Equals(damageType, "None", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A typed pain needs a damage type other than None.", nameof(damageType));
        _typedPain[damageType] = state;
        if (chance is { } value)
            _typedPainChance[damageType] = value;
    }

    internal int PainStateFor(string? damageType)
    {
        if (!string.IsNullOrEmpty(damageType)
            && !string.Equals(damageType, "None", StringComparison.OrdinalIgnoreCase)
            && _typedPain.TryGetValue(damageType, out var state)
            && States.HasState(state))
            return state;
        return PainState;
    }

    internal int PainChanceFor(string? damageType)
    {
        if (!string.IsNullOrEmpty(damageType)
            && _typedPainChance.TryGetValue(damageType, out var chance))
            return chance;
        return PainChance;
    }

    internal IEnumerable<(string Type, int State, int? Chance)> TypedPains()
    {
        foreach (var entry in _typedPain.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            var stored = _typedPainChance.TryGetValue(entry.Key, out var chance) ? chance : (int?)null;
            yield return (entry.Key, entry.Value, stored);
        }
    }

    public void RememberPosition()
    {
        PreviousX = X;
        PreviousY = Y;
        PreviousZ = Z;
    }

    /// <summary>Native ClearInterpolation position branch; other render history is not represented.</summary>
    public void ClearInterpolation() => RememberPosition();

    public (double X, double Y, double Z) PosPlusZ(double offset) => (X.ToDouble(), Y.ToDouble(), Z.ToDouble() + offset);
    public (double X, double Y, double Z) Pos() => (X.ToDouble(), Y.ToDouble(), Z.ToDouble());
    /// <summary>Native audio coordinates swap world Y/Z and use single precision.</summary>
    public (float X, float Y, float Z) SoundPos() => ((float)X.ToDouble(), (float)Z.ToDouble(), (float)Y.ToDouble());
    public bool IsAbove(double z) => Z.ToDouble() > z + 1.0 / Fixed.Unit;
    public bool IsBelow(double z) => Z.ToDouble() < z - 1.0 / Fixed.Unit;
    public bool IsAtZ(double z) => Math.Abs(Z.ToDouble() - z) < 1.0 / Fixed.Unit;
    /// <summary>Native linear position interpolation; render-flag overrides are not represented.</summary>
    public (double X, double Y, double Z) InterpolatedPosition(double ticFraction) => (
        PreviousX.ToDouble() * (1 - ticFraction) + X.ToDouble() * ticFraction,
        PreviousY.ToDouble() * (1 - ticFraction) + Y.ToDouble() * ticFraction,
        PreviousZ.ToDouble() * (1 - ticFraction) + Z.ToDouble() * ticFraction);
    public (double X, double Y, double Z) PosAtZ(double z) => (X.ToDouble(), Y.ToDouble(), z);
    public double Top() => Z.ToDouble() + Height.ToDouble();
    public double CenterOffset() => Height.ToDouble() / 2;
    public double Center() => Z.ToDouble() + CenterOffset();
    public void SetZ(double z, bool moving = true) => Z = Fixed.FromDouble(z);
    public void AddZ(double offset, bool moving = true)
    {
        Z = Fixed.FromDouble(Z.ToDouble() + offset);
        if (!moving) PreviousZ = Z;
    }
    public void SetXY((double X, double Y) position)
    { X = Fixed.FromDouble(position.X); Y = Fixed.FromDouble(position.Y); }
    public void SetXYZ(double x, double y, double z)
    { X = Fixed.FromDouble(x); Y = Fixed.FromDouble(y); Z = Fixed.FromDouble(z); }
    public void SetXYZ((double X, double Y, double Z) position) => SetXYZ(position.X, position.Y, position.Z);

    public override void Tick()
    {
        AbsorbCount = 0;
        RememberPosition();
        if (!CheckNoDelay()) return;
        States.Tick(this);
        if (!Destroyed && PickupDelay > 0 && --PickupDelay == 0)
        {
            SpecialPickup = PickupCatalog.IsPickup(DoomEdNum);
            Solid = false;
        }
        if (!Destroyed && Simulation != null)
        {
            if (!Dormant) Brain?.Tick(Simulation, this);
            else if (!IsDead) Brain?.AdvanceRaiseFrame();
            TickMovement(Simulation);
            if (VelocityX.Raw == 0 && VelocityY.Raw == 0) Blasted = false;
            SectorDamage.Tick(Simulation, this);
            TerrainDamage.Tick(Simulation, this);
        }
        base.Tick();
    }

    protected virtual void TickMovement(AuthoritySimulation sim) => ActorPhysics.Step(sim, this);
}

public class PlayerPawn : Actor
{
    public override int GetMaxHealth(bool withUpgrades) => withUpgrades
        ? checked(EffectiveMaxHealth + Stamina + BonusHealth) : EffectiveMaxHealth;
    public int Stamina { get; set; }
    public int BonusHealth { get; set; }
    public int MaxPickupHealth { get; set; }
    public override void BeginPlay()
    {
        base.BeginPlay();
        FullHeight = Height.ToDouble();
    }

    public PlayerPawn()
    {
        Inventory = new PlayerInventory(this);
        CanSlide = true;
        MovementSpeed = Fixed.FromInt(1);
        CanPickupItems = true;
        SpawnCanPickupItems = true;
        AllowDropOff = true;
        NoBlockMonsters = true;
    }

    public const double CrouchSpeed = 1.0 / 12;
    public const double MinimumCrouchFactor = 0.5;
    public const double StandingViewHeight = 41;
    internal bool HasMovementInput { get; private set; }
    public bool ClassicFlight { get; set; }
    public int JumpTics { get; internal set; }
    public Fixed JumpZ { get; set; } = Fixed.FromInt(8);
    public int MaxHealth { get; set; }
    public int EffectiveMaxHealth => MaxHealth > 0 ? MaxHealth
        : Simulation is { } sim && !sim.Compat.HasFlag(CompatSurface.DehHealth) ? sim.DehackedMaxHealth : 100;
    /// <summary>Native DeathThink leaves the view here.</summary>
    public const double DeathViewHeight = 6;
    public const double DeathPitchStep = 3;
    public const double DeathTurnStep = 5;
    /// <summary>Vanilla maximum health for drain when there is no custom override.</summary>
    public const int DrainMaxHealth = 100;
    /// <summary>PowerDrain strength. 0 is off. 0.5 is the usual half of the post-armor hit.</summary>
    public double DrainStrength { get; set; }

    /// <summary>Height before crouch. <c>BeginPlay</c> copies the spawned height.</summary>
    public double FullHeight { get; internal set; }
    /// <summary>Native player.crouchfactor. 1 is standing. 0.5 is fully crouched.</summary>
    public double CrouchFactor { get; private set; } = 1;
    /// <summary>Native player.viewheight. Standing view is 41, scaled by <see cref="CrouchFactor"/>.</summary>
    public double ViewHeight { get; internal set; } = StandingViewHeight;
    /// <summary>Native PlayerPawn ViewHeight property, separate from the changing eye height.</summary>
    public Fixed DefaultViewHeight { get; set; } = Fixed.FromDouble(StandingViewHeight);
    /// <summary>Native PlayerPawn attack height relative to its center.</summary>
    public Fixed AttackZOffset { get; set; } = Fixed.FromInt(8);
    /// <summary>Native <c>MAXBOB</c>.</summary>
    public const double MaxBob = 16;
    /// <summary>Native <c>movebob</c> default.</summary>
    public const double MoveBob = 0.25;
    /// <summary>Native <c>ViewBobSpeed</c> default. The angle is <c>BobTimer / speed * 360</c>.</summary>
    public const double ViewBobSpeed = 20;
    /// <summary>Native <c>BobTimer</c>. It advances once per player tic.</summary>
    public int BobTimer { get; set; }
    /// <summary>Added to view Z. <see cref="ViewHeight"/> stays the standing or crouched eye height.</summary>
    public double ViewBobOffset { get; internal set; }
    /// <summary>Native <c>player.bob</c>. Horizontal speed squared times <see cref="MoveBob"/>, capped at <see cref="MaxBob"/>.</summary>
    public double MovementBob { get; internal set; }
    /// <summary>Native weapon <c>BobSpeed</c>. Doom weapons leave this at 1.</summary>
    public const double WeaponBobSpeed = 1;
    /// <summary>Native <c>BobRangeX</c> / <c>BobRangeY</c>. Doom weapons leave both at 1.</summary>
    public const double WeaponBobRange = 1;
    /// <summary><c>BobWeapon</c> X at the end of the tic. Other bob styles are absent.</summary>
    public double WeaponBobX { get; internal set; }
    /// <summary><c>BobWeapon</c> Y. Normal style uses the absolute sine.</summary>
    public double WeaponBobY { get; internal set; }
    /// <summary>Native player.crouching == 1. Jumping while crouched stands the player back up.</summary>
    public bool UncrouchLocked { get; private set; }

    /// <summary>Native <c>WEAPONTOP</c>. The weapon can fire only at this offset.</summary>
    public const int WeaponTop = 32;
    /// <summary>Native <c>WEAPONBOTTOM</c>.</summary>
    public const int WeaponBottom = 128;
    /// <summary>Default <c>A_Raise</c> / <c>A_Lower</c> speed.</summary>
    public const int WeaponMoveSpeed = 6;
    /// <summary>Native <c>TURN180_TICKS</c>. <c>(TICRATE / 4) + 1</c> is 9.</summary>
    public const int Turn180Ticks = (GameTicClock.TicRate / 4) + 1;
    /// <summary>
    /// Psprite Y for the ready weapon. A spawned weapon starts here, so it can fire.
    /// A slot change lowers it to <see cref="WeaponBottom"/> and then raises it.
    /// </summary>
    public int WeaponOffsetY { get; internal set; } = WeaponTop;
    public bool WeaponLowering { get; internal set; }
    /// <summary>Native <c>CF_INSTANTWEAPSWITCH</c>. A lower finishes and the new weapon is ready on that tic.</summary>
    public bool InstantWeaponSwitch { get; set; }
    /// <summary>Tics left in a <c>BT_TURN180</c>. Each tic adds <c>180 / Turn180Ticks</c> degrees and ignores yaw.</summary>
    public int TurnTicks { get; set; }
    private bool _turnHeld;
    internal bool TurnHeld => _turnHeld;
    internal void ClearTurnHeld() => _turnHeld = false;
    public bool WeaponReady => Inventory.Selected != 0 && WeaponOffsetY == WeaponTop && !WeaponLowering;

    public const int CommandQueueCapacity = 128;
    private readonly Queue<PlayerCommand> _commands = new();
    public byte PlayerNum { get; init; }
    /// <summary>
    /// Native <c>CF_BUDDHA2</c>. A killing blow, including a telefrag, leaves health at 1.
    /// <c>DMG_FORCED</c> no longer skips armor or god mode.
    /// </summary>
    public bool Buddha2 { get; set; }
    /// <summary>Native <c>CF_EXTREMELYDEAD</c>. Set when an extreme death frame kills the player.</summary>
    public bool ExtremelyDead { get; set; }
    /// <summary>Native inventory <c>BLINKTHRESHOLD</c>. A second PowerBuddha does not extend past this.</summary>
    public const int PowerBuddhaBlinkThreshold = 4 * 32;
    /// <summary>Native <c>Powerup.Duration -60</c>. A negative duration is seconds, so this is 60 * 35 tics.</summary>
    public const int PowerBuddhaDuration = 60 * GameTicClock.TicRate;
    /// <summary>Tics of <c>PowerBuddha</c> left. Zero means the item is gone.</summary>
    public int PowerBuddhaTics { get; set; }
    public const int DamagePowerDuration = 25 * GameTicClock.TicRate;
    public int PowerDamageTics { get; set; }
    public int PowerProtectionTics { get; set; }

    internal void ClearDamagePowers()
    {
        PowerBuddhaTics = 0;
        PowerDamageTics = 0;
        PowerProtectionTics = 0;
    }

    public void GivePowerDamage(int durationTics = DamagePowerDuration, bool additiveTime = false, bool alwaysPickup = false)
    {
        PowerDamageTics = PowerupDuration.Merge(PowerDamageTics, durationTics, additiveTime, alwaysPickup);
    }

    /// <summary>Grants a supported power with native Powerup.Duration units: negative seconds, otherwise tics.</summary>
    public void GivePowerup(ManagedPowerupKind kind, int definitionDuration, bool additiveTime = false, bool alwaysPickup = false)
    {
        var duration = PowerupDuration.FromDefinition(definitionDuration);
        switch (kind)
        {
            case ManagedPowerupKind.Damage: GivePowerDamage(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Protection: GivePowerProtection(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Buddha: GivePowerBuddha(duration, additiveTime, alwaysPickup); break;
            default: throw new ArgumentOutOfRangeException(nameof(kind));
        }
    }

    public void GivePowerProtection(int durationTics = DamagePowerDuration, bool additiveTime = false, bool alwaysPickup = false)
    {
        PowerProtectionTics = PowerupDuration.Merge(PowerProtectionTics, durationTics, additiveTime, alwaysPickup);
    }

    public override int GetModifiedDamage(string? damageType, int damage, bool passive, Actor? inflictor, Actor? other, DamageFlags flags)
    {
        damage = base.GetModifiedDamage(damageType, damage, passive, inflictor, other, flags);
        if (!passive && PowerDamageTics > 0)
            damage = new PowerDamage().ModifyDamage(damage, damageType, false, inflictor, other, flags);
        if (passive && PowerProtectionTics > 0)
            damage = new PowerProtection().ModifyDamage(damage, damageType, true, inflictor, other, flags);
        return damage;
    }
    /// <summary>Native <c>player_t::fragcount</c> for ACS <c>PlayerFrags</c>.</summary>
    public int FragCount { get; set; }

    /// <summary>
    /// Grants <c>PowerBuddha</c>. Above <see cref="PowerBuddhaBlinkThreshold"/> the new
    /// item is taken and the timer stays. At or below that line it resets to
    /// <see cref="PowerBuddhaDuration"/>.
    /// </summary>
    public void GivePowerBuddha(int durationTics = PowerBuddhaDuration, bool additiveTime = false, bool alwaysPickup = false)
    {
        PowerBuddhaTics = PowerupDuration.Merge(PowerBuddhaTics, durationTics, additiveTime, alwaysPickup);
    }
    /// <summary>Compatibility slot for direct input/restore; assignment replaces buffered input.</summary>
    public PlayerCommand Pending
    {
        get => _commands.TryPeek(out var command) ? command : default;
        set { _commands.Clear(); _commands.Enqueue(value); }
    }
    public int BufferedCommandCount => _commands.Count;
    public bool TryQueueCommand(PlayerCommand command)
    {
        if (Destroyed) return true;
        if (IsDead)
        {
            // The press can arm respawn. Weapon, movement, and the command itself are not kept for revival.
            if ((command.Use && !UseHeld) || (command.Attack && !_attackHeld))
                RespawnArmed = true;
            UseHeld = command.Use;
            _attackHeld = command.Attack;
            return true;
        }
        if (_commands.Count >= CommandQueueCapacity) return false;
        _commands.Enqueue(command);
        return true;
    }
    public void ClearCommands() => _commands.Clear();
    public PlayerInventory Inventory { get; }
    public bool GodMode { get; set; }
    public bool AttackPressed { get; set; }
    public bool UsePressed { get; set; }
    public bool UseHeld { get; internal set; }
    /// <summary>Native Player.UseRange. The use trace follows yaw for this distance.</summary>
    private double _useRange = 64;
    public double UseRange { get => _useRange; set { _useRange = value; HasUseRangeOverride = true; } }
    internal bool HasUseRangeOverride { get; set; }
    public int WeaponCooldown { get; internal set; }
    /// <summary>First tic on which a press may respawn. <see cref="int.MaxValue"/> until the player dies.</summary>
    public int RespawnEarliestTic { get; private set; } = int.MaxValue;
    /// <summary>Set by a rising use or attack edge while dead. Cleared when the respawn is taken.</summary>
    public bool RespawnArmed { get; private set; }
    private bool _attackHeld;
    internal bool AttackHeld => _attackHeld;

    internal void NoteDeath(AuthoritySimulation? sim)
    {
        RespawnArmed = false;
        if (sim == null)
        {
            RespawnEarliestTic = int.MaxValue;
            return;
        }
        var tic = sim.Thinkers.Clock.Tic;
        RespawnEarliestTic = tic > int.MaxValue - GameTicClock.TicRate ? int.MaxValue : tic + GameTicClock.TicRate;
        sim.DropSelectedWeapon(this);
        sim.StartPlayerScripts(death: true, this);
    }

    internal void DisarmRespawn() => RespawnArmed = false;

    private void ApplyCrouch(PlayerCommand command)
    {
        if (FullHeight <= 0) FullHeight = Height.ToDouble();
        // A jump clears BT_CROUCH for this tic, then a locked stand-up ignores the button.
        var crouch = command.Crouch && !command.Jump;
        var direction = !UncrouchLocked ? (crouch ? -1 : 1) : 1;
        if (UncrouchLocked && crouch)
            UncrouchLocked = false;
        var before = CrouchFactor;
        if (direction > 0 && CrouchFactor < 1
            && (Simulation == null || Z.ToDouble() + Height.ToDouble() < ActorPhysics.CeilingAtActor(Simulation, this)))
            CrouchMove(1);
        else if (direction < 0 && CrouchFactor > MinimumCrouchFactor)
            CrouchMove(-1);
        // Leave an externally chosen height alone while the player is not crouching.
        if (Math.Abs(CrouchFactor - before) > 1e-12)
            Height = Fixed.FromDouble(FullHeight * CrouchFactor);
        ViewHeight = DefaultViewHeight.ToDouble() * CrouchFactor;
    }

    internal void RestoreCrouch(SimCrouchState? saved, double? savedFullHeight)
    {
        CrouchFactor = saved?.Factor ?? 1;
        if (saved is { } crouch) FullHeight = crouch.FullHeight;
        else
        {
            FullHeight = savedFullHeight ?? (ResurrectionHeight ?? Height).ToDouble();
            Height = Fixed.FromDouble(FullHeight);
        }
        DefaultViewHeight = saved is { } value ? new Fixed(value.DefaultViewHeightRaw) : Fixed.FromDouble(StandingViewHeight);
        ViewHeight = saved?.ViewHeight ?? DefaultViewHeight.ToDouble();
        UncrouchLocked = saved?.Locked ?? false;
    }

    private void CrouchMove(int direction)
    {
        var next = CrouchFactor + direction * CrouchSpeed;
        if (Simulation != null)
        {
            var fits = ActorPhysics.FitsAtHeight(Simulation, this, FullHeight * next);
            if (!fits && direction > 0) return;
        }
        CrouchFactor = Math.Clamp(next, MinimumCrouchFactor, 1);
    }

    private void Uncrouch()
    {
        if (FullHeight <= 0) FullHeight = Height.ToDouble();
        // Native Uncrouch copies the standing view only while a crouch is actually cleared.
        var wasCrouched = CrouchFactor != 1;
        CrouchFactor = 1;
        UncrouchLocked = false;
        if (wasCrouched) ViewHeight = DefaultViewHeight.ToDouble();
        Height = Fixed.FromDouble(FullHeight);
    }

    public override void Tick()
    {
        HasMovementInput = false;
        Inventory.AbsorbCount = 0;
        if (PowerBuddhaTics > 0) PowerBuddhaTics--;
        if (PowerDamageTics > 0) PowerDamageTics--;
        if (PowerProtectionTics > 0) PowerProtectionTics--;
        BobTimer++;
        RememberPosition();
        if (WeaponCooldown > 0) WeaponCooldown--;
        if (IsDead)
        {
            AttackPressed = false;
            ViewBobOffset = 0;
            MovementBob = 0;
            WeaponBobX = 0;
            WeaponBobY = 0;
            Uncrouch();
            WatchKiller();
            Simulation?.TryPlayerRespawn(this);
            base.Tick();
            return;
        }
        var command = _commands.TryDequeue(out var queued) ? queued : default;
        HasMovementInput = command.ForwardMove != 0 || command.SideMove != 0;
        var freshUse = command.Use && !UseHeld;
        UseHeld = command.Use;
        _attackHeld = command.Attack;
        UsePressed = freshUse;
        foreach (var slot in command.WeaponSelections.Span)
            Inventory.SelectSlot(slot);
        if (Inventory.Pending != null)
            WeaponLowering = true;
        AdvanceWeapon();
        if (command.Attack && WeaponReady)
            AttackPressed = true;
        var freshTurn = command.Turn180 && !_turnHeld;
        _turnHeld = command.Turn180;
        if (freshTurn)
            TurnTicks = Turn180Ticks;
        ApplyCrouch(command);
        if (JumpTics != 0)
        {
            JumpTics = unchecked(JumpTics - 1);
            if (OnGround && JumpTics < -18) JumpTics = 0;
        }
        PitchDegrees = Math.Clamp(PitchDegrees + command.PitchDelta * (360.0 / 65536), -89, 89);
        if (ReactionTime != 0)
            ReactionTime = unchecked(ReactionTime - 1);
        else
        {
            if (TurnTicks > 0)
            {
                TurnTicks--;
                Angle = BamAngle.FromDegrees(Angle.ToDegrees() + 180.0 / Turn180Ticks);
            }
            else
                Angle = new BamAngle(unchecked(Angle.Raw + (uint)(command.YawDelta << 16)));
            if (Level != null)
            {
                var (dx, dy) = Movement.Thrust(Angle, command.ForwardMove, command.SideMove);
                var thrustScale = MovementSpeed.ToDouble() * CrouchFactor;
                if (Simulation is { } movementSimulation)
                    thrustScale *= ActorPhysics.TerrainMovementScale(movementSimulation, this);
                if (!OnGround && !NoGravity && (Simulation?.AirControl ?? Level.AirControl) is { } airControl)
                {
                    thrustScale *= airControl;
                }
                if (NoGravity && PitchDegrees != 0 && !ClassicFlight)
                {
                    var pitch = PitchDegrees * Math.PI / 180;
                    var (forwardX, forwardY) = Movement.Thrust(Angle, command.ForwardMove, 0);
                    dx += forwardX * (Math.Cos(pitch) - 1);
                    dy += forwardY * (Math.Cos(pitch) - 1);
                    VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble()
                        - command.ForwardMove / 8192.0 * thrustScale * Math.Sin(pitch));
                }
                VelocityX = Fixed.FromDouble(VelocityX.ToDouble() + dx * thrustScale);
                VelocityY = Fixed.FromDouble(VelocityY.ToDouble() + dy * thrustScale);
            }
            // Native CheckCrouch precedes movement and jump; a fully standing pawn can jump.
            var crouched = CrouchFactor < 1;
            if (command.Jump && crouched)
                UncrouchLocked = true;
            else if (command.Jump && NoGravity)
                VelocityZ = Fixed.FromInt(3);
            else if (command.Jump && OnGround && JumpTics == 0 && Level != null)
            {
                VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble() + JumpZ.ToDouble());
                OnMobj = false;
                JumpTics = -1;
                OnGround = false;
            }
            if (command.UpMove == short.MinValue)
            {
                if (NoGravity) NoGravity = false;
            }
            else if (command.UpMove != 0 && Fly)
            {
                var upMove = Math.Clamp((int)command.UpMove, -768, 768);
                VelocityZ = Fixed.FromDouble(MovementSpeed.ToDouble() * upMove / 128.0);
                NoGravity = true;
            }
        }

        base.Tick();
        UpdateViewBob();
    }

    /// <summary>
    /// <c>CalcHeight</c> movement bob. Horizontal speed squared, times <see cref="MoveBob"/>,
    /// capped at <see cref="MaxBob"/>. The view term is that amount times
    /// <c>sin(BobTimer / ViewBobSpeed * 360) * 0.5</c>. Still bob is 0.
    /// <c>BobWeapon</c> then swings the ready weapon in the normal style.
    /// </summary>
    public void UpdateViewBob()
    {
        var speed = VelocityX.ToDouble() * VelocityX.ToDouble() + VelocityY.ToDouble() * VelocityY.ToDouble();
        MovementBob = speed <= 0 ? 0 : Math.Min(MaxBob, speed * MoveBob);
        var angle = BobTimer / ViewBobSpeed * 360 * Math.PI / 180;
        ViewBobOffset = MovementBob * Math.Sin(angle) * 0.5;
        UpdateWeaponBob();
    }

    /// <summary>
    /// <c>BobWeapon</c> for <c>Bob_Normal</c> at the end of the tic.
    /// The angle is <c>BobTimer * BobSpeed * 128 * 360 / 8192</c> degrees.
    /// X is the cosine. Y is the absolute sine. A weapon that is lowering,
    /// raising, or firing stays at 0. <c>wbobfire</c> is 0.
    /// </summary>
    private void UpdateWeaponBob()
    {
        var bobbing = WeaponReady && WeaponCooldown == 0 && !AttackPressed;
        if (MovementBob == 0 || !bobbing)
        {
            WeaponBobX = 0;
            WeaponBobY = 0;
            return;
        }

        var degrees = BobTimer * (WeaponBobSpeed * 128) * (360.0 / 8192);
        var radians = degrees * Math.PI / 180;
        WeaponBobX = MovementBob * WeaponBobRange * Math.Cos(radians);
        WeaponBobY = MovementBob * WeaponBobRange * Math.Abs(Math.Sin(radians));
    }

    /// <summary>
    /// <c>DeathThink</c> watches the killer and settles the view. Turn at most
    /// <see cref="DeathTurnStep"/> degrees toward <see cref="Actor.LastDamageSourceId"/>.
    /// View height falls to <see cref="DeathViewHeight"/> and pitch falls to 0,
    /// unless this pawn is an ice corpse.
    /// </summary>
    private void WatchKiller()
    {
        if (!IceCorpse)
        {
            if (ViewHeight > DeathViewHeight) ViewHeight -= 1;
            if (ViewHeight < DeathViewHeight) ViewHeight = DeathViewHeight;
            if (PitchDegrees > 0) PitchDegrees -= DeathPitchStep;
            else if (PitchDegrees < 0) PitchDegrees += DeathPitchStep;
            if (Math.Abs(PitchDegrees) < DeathPitchStep) PitchDegrees = 0;
        }
        if (LastDamageSourceId is not uint sourceId || Simulation == null) return;
        var attacker = Simulation.Actors.FirstOrDefault(actor => actor.Id == sourceId);
        if (attacker == null || ReferenceEquals(attacker, this)) return;
        var aim = Math.Atan2(attacker.Y.ToDouble() - Y.ToDouble(), attacker.X.ToDouble() - X.ToDouble()) * 180 / Math.PI;
        var diff = AimDelta(Angle.ToDegrees(), aim);
        Angle = BamAngle.FromDegrees(Angle.ToDegrees() + Math.Clamp(diff, -DeathTurnStep, DeathTurnStep));
    }

    private static double AimDelta(double fromDegrees, double toDegrees)
    {
        var diff = (toDegrees - fromDegrees) % 360;
        if (diff > 180) diff -= 360;
        else if (diff < -180) diff += 360;
        return diff;
    }

    /// <summary>
    /// One tic of <c>A_Lower</c> then <c>A_Raise</c>. Reaching the bottom commits
    /// <see cref="PlayerInventory.Pending"/> and does not raise on that same tic.
    /// <see cref="InstantWeaponSwitch"/> finishes that handoff at <see cref="WeaponTop"/>
    /// in the same tic. Sprites, flash, and bob are not represented.
    /// </summary>
    internal void BringUpWeapon(WeaponKind weapon)
    {
        Inventory.Selected = weapon;
        Inventory.Pending = null;
        WeaponLowering = false;
        WeaponOffsetY = InstantWeaponSwitch ? WeaponTop : WeaponBottom - WeaponMoveSpeed;
    }

    private void AdvanceWeapon()
    {
        if (Inventory.Selected == 0 && Inventory.Pending is { } firstWeapon)
        {
            BringUpWeapon(firstWeapon);
            return;
        }
        if (WeaponLowering && InstantWeaponSwitch)
        {
            if (Inventory.Pending is { } instant)
            {
                Inventory.Selected = instant;
                Inventory.Pending = null;
            }
            WeaponLowering = false;
            WeaponOffsetY = WeaponTop;
            return;
        }
        if (WeaponLowering)
        {
            WeaponOffsetY = Math.Min(WeaponBottom, WeaponOffsetY + WeaponMoveSpeed);
            if (WeaponOffsetY >= WeaponBottom)
            {
                WeaponLowering = false;
                if (Inventory.Pending is { } pending)
                {
                    Inventory.Selected = pending;
                    Inventory.Pending = null;
                }
            }
            return;
        }
        if (WeaponOffsetY > WeaponTop)
            WeaponOffsetY = Math.Max(WeaponTop, WeaponOffsetY - WeaponMoveSpeed);
    }
}

public static class Movement
{
    /// <summary>
    /// One tic of ground thrust from <c>PlayerPawn.MovePlayer</c>.
    /// The wire value is the classic move shifted left by 8. It is then scaled by
    /// <c>Speed / 256</c> and <c>ORIG_FRICTION_FACTOR</c> (<c>2048/65536</c>), which is
    /// <c>command / 8192</c> map units. Angle 0 faces east. Side thrust uses angle minus 90 degrees.
    /// </summary>
    public static (double X, double Y) Thrust(BamAngle angle, short forwardMove, short sideMove)
    {
        var radians = angle.Raw * (Math.PI / 0x80000000u);
        var forward = forwardMove / 8192.0;
        var side = sideMove / 8192.0;
        var cos = Math.Cos(radians);
        var sin = Math.Sin(radians);
        return (forward * cos + side * sin, forward * sin - side * cos);
    }
}

public static class LineSlide
{
    public static (double X, double Y) Move(PlayLevel level, double x, double y, double radius, double dx, double dy)
    {
        if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9)
            return (x, y);

        if (CanOccupy(level, x + dx, y + dy, radius, x, y))
            return (x + dx, y + dy);

        LevelLine? hit = null;
        foreach (var line in level.Lines)
        {
            if (!line.BlocksMovement)
                continue;
            if (Hits(x + dx, y + dy, radius, x, y, line))
            {
                hit = line;
                break;
            }
        }

        if (hit == null)
            return (x, y);

        var lx = hit.X2 - hit.X1;
        var ly = hit.Y2 - hit.Y1;
        var lengthSquared = lx * lx + ly * ly;
        if (lengthSquared < 1e-9)
            return (x, y);

        var scale = (dx * lx + dy * ly) / lengthSquared;
        var slideX = lx * scale;
        var slideY = ly * scale;
        if (CanOccupy(level, x + slideX, y + slideY, radius, x, y))
            return (x + slideX, y + slideY);

        return (x, y);
    }

    private static bool CanOccupy(PlayLevel level, double x, double y, double radius, double fromX, double fromY)
    {
        if (level.Blockmap is MapBlockmapRecord blockmap)
        {
            foreach (var index in BlockmapLines.Near(blockmap, x, y, radius).Concat(BlockmapLines.Near(blockmap, fromX, fromY, radius)))
            {
                if ((uint)index >= (uint)level.Lines.Count)
                    continue;
                var line = level.Lines[index];
                if (line.BlocksMovement && Hits(x, y, radius, fromX, fromY, line))
                    return false;
            }

            return true;
        }

        foreach (var line in level.Lines)
        {
            if (line.BlocksMovement && Hits(x, y, radius, fromX, fromY, line))
                return false;
        }

        return true;
    }

    private static bool Hits(double x, double y, double radius, double fromX, double fromY, LevelLine line)
    {
        if (DistanceToSegment(x, y, line) <= radius)
            return true;
        return SegmentsCross(fromX, fromY, x, y, line.X1, line.Y1, line.X2, line.Y2);
    }

    private static double DistanceToSegment(double x, double y, LevelLine line)
    {
        var dx = line.X2 - line.X1;
        var dy = line.Y2 - line.Y1;
        var lengthSquared = dx * dx + dy * dy;
        if (lengthSquared < 1e-9)
            return Distance(x, y, line.X1, line.Y1);

        var t = Math.Clamp(((x - line.X1) * dx + (y - line.Y1) * dy) / lengthSquared, 0, 1);
        return Distance(x, y, line.X1 + t * dx, line.Y1 + t * dy);
    }

    private static double Distance(double ax, double ay, double bx, double by)
    {
        var dx = ax - bx;
        var dy = ay - by;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static bool Crosses(double fromX, double fromY, double toX, double toY, LevelLine line) =>
        SegmentsCross(fromX, fromY, toX, toY, line.X1, line.Y1, line.X2, line.Y2);

    private static bool SegmentsCross(
        double ax, double ay, double bx, double by,
        double cx, double cy, double dx, double dy)
    {
        var d1 = Cross(cx, cy, dx, dy, ax, ay);
        var d2 = Cross(cx, cy, dx, dy, bx, by);
        var d3 = Cross(ax, ay, bx, by, cx, cy);
        var d4 = Cross(ax, ay, bx, by, dx, dy);
        return d1 * d2 < 0 && d3 * d4 < 0;
    }

    private static double Cross(double ax, double ay, double bx, double by, double px, double py) =>
        (bx - ax) * (py - ay) - (by - ay) * (px - ax);
}

public static class ActorSpawner
{
    public static IReadOnlyList<Actor> Spawn(
        PlayLevel level,
        ThinkerCollection thinkers,
        DehackedPatchResult? dehacked = null, SpawnOptions? spawnOptions = null, Func<uint>? mapSpawnRandom = null)
    {
        var actors = new List<Actor>();
        uint nextId = 1;
        foreach (var thing in level.Things)
        {
            if (thing.Type == 0 || thing.Type == DeathmatchStarts.ThingType
                || !(spawnOptions ?? new SpawnOptions()).Includes(thing))
                continue;

            var defaults = dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == thing.Type && actor.Patched);
            var definitionType = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : thing.Type;
            var definition = DoomActorCatalog.Find(definitionType);
            var playerStart = Actor.IsPlayerStart(thing.Type);
            Actor actor = playerStart
                ? new PlayerPawn
                {
                    PlayerNum = (byte)(thing.Type - 1),
                    Id = nextId,
                    DoomEdNum = thing.Type,
                    X = Fixed.FromDouble(thing.X),
                    Y = Fixed.FromDouble(thing.Y),
                    ThingId = thing.Id, SpawnZOffset = thing.Z,
                    Angle = BamAngle.FromDegrees(thing.Angle),
                    Health = dehacked?.Actors.FirstOrDefault(actor => actor.Index == 1 && actor.Patched)?.Health ?? 100,
                    Radius = RadiusOf(defaults, player: true),
                    Height = HeightOf(defaults),
                    Level = level,
                }
                : new Actor
                {
                    Id = nextId,
                    DoomEdNum = thing.Type,
                    X = Fixed.FromDouble(thing.X),
                    Y = Fixed.FromDouble(thing.Y),
                    ThingId = thing.Id, SpawnZOffset = thing.Z,
                    Angle = BamAngle.FromDegrees(thing.Angle),
                    Health = defaults?.Health ?? definition?.Health ?? 30,
                    Radius = defaults == null && definition != null ? Fixed.FromInt(definition.Radius) : RadiusOf(defaults, player: false),
                    Height = defaults == null && PickupCatalog.IsPickup(thing.Type) ? PickupCatalog.HeightOf(thing.Type)
                        : defaults == null && definition != null ? Fixed.FromInt(definition.Height) : HeightOf(defaults),
                    PainChance = defaults?.PainChance ?? definition?.PainChance ?? 256,
                    ChaseSpeed = Math.Clamp((defaults?.Speed ?? definition?.Speed ?? 4) / 4.0, 0, ActorPhysics.MaxMove),
                    NoGravity = definition?.Floating ?? false,
                    Floating = definition?.Floating ?? false,
                    NoRadiusDamage = definition?.NoRadiusDamage ?? false,
                    Level = level,
                };

            if (!playerStart)
            {
                actor.PitchDegrees = thing.Pitch;
                actor.Roll = BamAngle.FromDegrees(thing.Roll);
            }
            actor.DefinitionDoomEdNum = definitionType;
            nextId++;
            actor.SpecialPickup = PickupCatalog.IsPickup(actor.DoomEdNum);
            ApplyPrimaryFlags(actor, defaults, playerStart);
            DoomDecorationDefaults.Apply(actor, definitionType, defaults);
            actor.SpawnCanPickupItems = actor.CanPickupItems;
            actor.SpawnSpecialPickup = actor.SpecialPickup;
            actor.ResurrectionHealth = actor.Health;
            if (actor.ResurrectionHealth > 0) actor.GibHealth = -actor.ResurrectionHealth;
            actor.Mass = defaults is { MassPatched: true } ? defaults.Mass : DoomActorCatalog.MassOf(definitionType);
            actor.RaiseDuration = ArchvileActions.RaiseDuration(definitionType);
            actor.Boss = definitionType is 7 or 16;
            ApplyExtendedDefaults(actor, defaults);
            actor.SpawnDropped = actor.Dropped;
            actor.SpawnFriendly = thing.Friendly;
            actor.SpawnAmbush = thing.Ambush;
            if (thing.Special != 0 || thing.Args.Any(arg => arg != 0))
            {
                actor.Special = thing.Special;
                Array.Copy(thing.Args, actor.SpecialArgs, Math.Min(5, thing.Args.Length));
            }
            if (thing.Gravity < 0) actor.Gravity = Fixed.FromDouble(-thing.Gravity);
            else if (thing.Gravity > 0) actor.Gravity = Fixed.FromDouble(actor.Gravity.ToDouble() * thing.Gravity);
            else { actor.Gravity = default; actor.NoGravity = true; }
            if (defaults is { ReactionTimePatched: true }) actor.ReactionTime = defaults.ReactionTime;
            if (defaults is { InfightingGroupPatched: true }) actor.InfightingGroup = defaults.InfightingGroup;
            if (defaults is { ProjectileGroupPatched: true }) actor.ProjectileGroup = defaults.ProjectileGroup;
            if (defaults is { SplashGroupPatched: true }) actor.SplashGroup = defaults.SplashGroup;
            actor.Brain = MonsterBrain.ForType(definitionType);
            if (!playerStart) actor.Damage = definition?.Damage ?? 0;
            if (defaults is { MissileDamagePatched: true }) actor.Damage = defaults.MissileDamage;
            actor.IsMonster = !playerStart && (defaults is { BitsPatched: true }
                ? (defaults.Bits & 0x00400000) != 0
                : actor.Brain != null);
            if (actor.IsMonster && Math.Clamp(spawnOptions?.Skill ?? 2, 0, 4) == 4) actor.ReactionTime = 0;
            ThingActivation.InitializeSpawn(actor, thing.Dormant, mapSpawnRandom: mapSpawnRandom);
            if (thing.Health != 1)
            {
                if (thing.Health == 0)
                    throw new NotSupportedException("Zero spawn health requires delayed spawn death.");
                var health = thing.Health > 0 ? actor.Health * thing.Health : -thing.Health;
                if (!double.IsFinite(health) || health < 1 || health > int.MaxValue)
                    throw new InvalidDataException("Actor spawn health is outside the supported range.");
                if (actor.GibHealth == -actor.ResurrectionHealth) actor.GibHealth = -(int)health;
                actor.Health = (int)health;
                actor.ResurrectionHealth = actor.Health;
            }
            actor.RememberPosition();
            thinkers.Add(actor, playerStart ? ThinkerStat.Player : ThinkerStat.Default);
            actors.Add(actor);
        }

        return actors;
    }

    internal static void ApplyPrimaryFlags(Actor actor, DehackedActor? defaults, bool playerStart)
    {
        actor.DontFall = actor.ClassDoomEdNum == 3006;
        if (defaults is { BitsPatched: true })
        {
            actor.SpecialPickup = (defaults.Bits & 0x00000001) != 0;
            actor.CanPickupItems = (defaults.Bits & 0x00000800) != 0;
            actor.Solid = (defaults.Bits & 0x00000002) != 0;
            actor.Shootable = (defaults.Bits & 0x00000004) != 0;
            actor.NoBlockmap = (defaults.Bits & 0x00000010) != 0;
            actor.SpawnCeiling = (defaults.Bits & 0x00000100) != 0;
            actor.NoGravity = (defaults.Bits & 0x00000200) != 0;
            actor.AllowDropOff = (defaults.Bits & 0x00000400) != 0;
            actor.Floating = (defaults.Bits & 0x00004000) != 0;
            actor.Dropped = (defaults.Bits & 0x00020000) != 0;
            actor.Friendly = playerStart || !defaults.BitsUseStealth && (defaults.Bits & 0x40000000) != 0;
            actor.NoBlockMonsters = playerStart || defaults.NoBlockMonsters;
        }
    }

    internal static void ApplyExtendedDefaults(Actor actor, DehackedActor? defaults)
    {
        actor.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { Bits2Patched: true })
        {
            actor.NoTeleport = (defaults.Bits2 & 0x80) != 0;
            actor.CanSlide = (defaults.Bits2 & 0x400) != 0;
            actor.Invulnerable = (defaults.Bits2 & 0x08000000) != 0;
            actor.Dormant = (defaults.Bits2 & 0x10000000) != 0;
        }
        if (defaults is { GravityPatched: true }) actor.Gravity = Fixed.FromDouble(defaults.Gravity);
        actor.ResurrectionDefenseFlags = DefenseFlagsOf(actor);
        actor.SpawnMass = actor.Mass;
        actor.SpawnTuning = new(actor.MovementSpeed.Raw, actor.FloatSpeed, actor.PainThreshold);
        actor.ResurrectionMovementFlags = MovementFlagsOf(actor);
    }

    internal static int DefenseFlagsOf(Actor actor) => (actor.Invulnerable ? 1 : 0) | (actor.Dormant ? 2 : 0)
        | (actor.NoRadiusDamage ? 4 : 0) | (actor.Friendly ? 8 : 0) | (actor.Ambush ? 16 : 0) | (actor.Boss ? 32 : 0);

    internal static int MovementFlagsOf(Actor actor) => (actor.NoGravity ? 1 : 0) | (actor.Floating ? 2 : 0)
        | (actor.NoTeleport ? 4 : 0) | (actor.CanSlide ? 8 : 0) | (actor.AllowDropOff ? 16 : 0);

    internal static int CollisionFlagsOf(Actor actor) => (actor.Solid ? 1 : 0) | (actor.Shootable ? 2 : 0)
        | (actor.NoBlockmap ? 4 : 0) | (actor.NoBlockMonsters ? 8 : 0) | (actor.Dropped ? 16 : 0)
        | (actor.IsMonster ? 32 : 0) | (actor.SpawnCeiling ? 64 : 0);

    internal static Fixed RadiusOf(DehackedActor? defaults, bool player)
    {
        if (defaults != null && (defaults.WidthPatched || defaults.Radius > 0))
            return Fixed.FromDouble(defaults.Radius);
        return Fixed.FromInt(player ? 16 : 20);
    }

    internal static Fixed HeightOf(DehackedActor? defaults) =>
        defaults != null && (defaults.HeightPatched || defaults.Height > 0) ? Fixed.FromDouble(defaults.Height) : Fixed.FromInt(56);
}

public sealed class AuthoritySimulation
{
    private double? _airControlOverride;
    private double? _levelGravityOverride;
    public double LevelGravity => _levelGravityOverride ?? Level.LevelGravity;
    public void SetLevelGravity(int fixedValue) => _levelGravityOverride = fixedValue / 65536.0;
    public double? AirControl => _airControlOverride ?? Level.AirControl;
    public void SetAirControl(int fixedValue) => _airControlOverride = fixedValue / 65536.0;
    public event Action<Actor>? WorldThingRevived;

    public event Action<Actor>? WorldThingSpawned;

    internal void NotifyActorSpawned(Actor actor) => WorldThingSpawned?.Invoke(actor);

    internal void NotifyActorRevived(Actor actor) => WorldThingRevived?.Invoke(actor);

    public DamageTypeCatalog DamageTypes { get; } = new();
    private readonly List<Actor> _actors;
    private uint _nextActorId;
    public uint CombatRandomState { get; private set; }
    /// <summary>Native <c>pr_switcher</c> for <see cref="Actor.OkayToSwitchTarget"/> hate stickiness.</summary>
    public uint SwitchTargetRandomState { get; private set; }
    /// <summary>Rolls for a deathmatch respawn. Not the native <c>DMSpawn</c> table, and not in the save pose.</summary>
    public uint DmSpawnRandomState { get; set; }
    private readonly DehackedPatchResult? _dehacked;
    internal int DehackedMaxHealth => _dehacked?.MaxHealth ?? 100;
    internal int DehackedInitialBullets => _dehacked?.InitialBullets ?? 50;
    internal bool DehackedNoAutofreeze => _dehacked?.NoAutofreeze ?? false;
    internal int DehackedMaxArmor => _dehacked?.MaxArmor ?? 200;
    internal int DehackedGreenArmorClass => _dehacked?.GreenArmorClass ?? 1;
    internal int DehackedBlueArmorClass => _dehacked?.BlueArmorClass ?? 2;
    internal bool DehackedHealthBonusCapPatched => _dehacked?.HealthBonusCapPatched ?? false;
    internal int DehackedMaxSoulsphere => _dehacked?.MaxSoulsphere ?? 200;
    internal int DehackedSoulsphereHealth => _dehacked?.SoulsphereHealth ?? 100;
    internal int DehackedMegasphereHealth => _dehacked?.MegasphereHealth ?? 200;
    private uint _uniqueTidRandomState;
    private uint _strobeRandomState;
    private uint _jumpRandomState;
    private bool _hasJumpRandomState;
    private ulong _stateRandomState;
    private readonly ulong _initialStateRandomState;
    private bool _hasStateRandomState;
    private ulong _clientStateRandomState;
    private readonly ulong _initialClientStateRandomState;
    private ulong _mapSpawnRandomState;
    private readonly ulong _initialMapSpawnRandomState;
    private bool _hasMapSpawnRandomState;
    private ulong _iceTicsRandomState;
    private readonly ulong _initialIceTicsRandomState;
    private bool _hasIceTicsRandomState;
    private bool _hasIceChunkLifecycle;
    private ulong _freezeChunksRandomState;
    private readonly ulong _initialFreezeChunksRandomState;
    private bool _hasFreezeChunksRandomState;
    private ulong _freezeDeathRandomState;
    private readonly ulong _initialFreezeDeathRandomState;
    private bool _hasFreezeDeathRandomState;
    private ulong _dropItemRandomState;
    private readonly ulong _initialDropItemRandomState;
    private bool _hasDropItemRandomState;
    private uint _flickerRandomState;
    private uint _lightFlashRandomState;
    private uint _fireFlickerRandomState;
    private readonly List<SectorMotion> _motions = new();

    private AuthoritySimulation(
        PlayLevel level,
        ThinkerCollection thinkers,
        List<Actor> actors,
        int rngSeed,
        CompatSurface compat, bool damageExitAllowed, SpawnOptions spawnOptions, DehackedPatchResult? dehacked)
    {
        _dehacked = dehacked;
        Level = level;
        if (!double.IsFinite(level.LevelGravity))
            throw new ArgumentOutOfRangeException(nameof(level), "Level gravity must be finite.");
        if (level.AirControl is { } airControl && !double.IsFinite(airControl))
            throw new ArgumentOutOfRangeException(nameof(level), "Air control must be finite.");
        foreach (var definition in level.DamageTypes)
            DamageTypes.Define(definition.Name, definition.Factor, definition.ReplaceFactor, definition.NoArmor);
        Thinkers = thinkers;
        _actors = actors;
        _nextActorId = actors.Count == 0 ? 1 : checked(actors.Max(actor => actor.Id) + 1);
        RngSeed = rngSeed;
        CombatRandomState = unchecked((uint)rngSeed) ^ 0x9e3779b9u;
        SwitchTargetRandomState = unchecked((uint)rngSeed) ^ 0x73776974u;
        DmSpawnRandomState = unchecked((uint)rngSeed) ^ 0x646d7370u;
        _uniqueTidRandomState = unchecked((uint)rngSeed) ^ 0x756e6974u;
        _strobeRandomState = unchecked((uint)rngSeed) ^ 0x7374726fu;
        _jumpRandomState = unchecked((uint)rngSeed) ^ 0x63616a75u;
        _flickerRandomState = unchecked((uint)rngSeed) ^ 0x666c6963u;
        _lightFlashRandomState = unchecked((uint)rngSeed) ^ 0x666c6173u;
        _fireFlickerRandomState = unchecked((uint)rngSeed) ^ 0x66697265u;
        _stateRandomState = _initialStateRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed));
        _clientStateRandomState = _initialClientStateRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xac77feb9u);
        _mapSpawnRandomState = _initialMapSpawnRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x46c478d0u);
        _iceTicsRandomState = _initialIceTicsRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x17f81aa5u);
        _freezeDeathRandomState = _initialFreezeDeathRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x9ee1e1b9u);
        _freezeChunksRandomState = _initialFreezeChunksRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xd9e163dbu);
        _dropItemRandomState = _initialDropItemRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x2d1fda00u);
        Compat = compat;
        DamageExitAllowed = damageExitAllowed;
        GameMode = spawnOptions.Mode;
        Skill = Math.Clamp(spawnOptions.Skill, 0, 4);
        FastMonsters = spawnOptions.FastMonsters ?? Skill == 4;
        SlowMonsters = spawnOptions.SlowMonsters;
        DefaultDropStyle = spawnOptions.DefaultDropStyle ?? level.DefaultDropStyle ?? 1;
        if (!double.IsFinite(spawnOptions.HealthFactor))
            throw new ArgumentOutOfRangeException(nameof(spawnOptions), "Health factor must be finite.");
        HealthFactor = spawnOptions.HealthFactor;
        if (!double.IsFinite(spawnOptions.ArmorFactor))
            throw new ArgumentOutOfRangeException(nameof(spawnOptions), "Armor factor must be finite.");
        ArmorFactor = spawnOptions.ArmorFactor;
        Floors = level.Sectors.Select(sector => Fixed.FromDouble(sector.FloorHeight).ToDouble()).ToArray();
        Ceilings = level.Sectors.Select(sector => Fixed.FromDouble(sector.CeilingHeight).ToDouble()).ToArray();
        Lights = level.Sectors.Select(sector => sector.LightLevel).ToArray();
        SectorScrollX = new double[level.Sectors.Count];
        SectorScrollY = new double[level.Sectors.Count];
        _sectorCarryScrolls = [];
        LightActions.Initialize(this);
        SectorDamage.Initialize(this);
        HealthGroups = LevelHealthGroups.Build(Level);
        ScrollCarryInitialize.ApplyUdmfWallScrolls(this);
        ScrollCarryInitialize.ApplyMapLoadScrollers(this);
        foreach (var actor in _actors)
        {
            actor.Simulation = this;
            if (actor is PlayerPawn player) player.Inventory.Bullets = Math.Clamp(DehackedInitialBullets, 0, player.Inventory.MaxBullets);
            actor.ResurrectionRadius ??= actor.Radius;
            actor.ResurrectionHeight ??= actor.Height;
            actor.ResurrectionCollisionFlags ??= ActorSpawner.CollisionFlagsOf(actor);
            actor.SpawnReactionTime ??= actor.ReactionTime;
            actor.SpawnDamage ??= actor.Damage;
            actor.SpawnTuning ??= new(actor.MovementSpeed.Raw, actor.FloatSpeed, actor.PainThreshold);
            ActorPhysics.PlaceOnFloor(this, actor);
            var spawnZ = actor.SpawnCeiling
                ? CeilingOf(actor.SectorIndex) - actor.Height.ToDouble() - actor.SpawnZOffset
                : actor.Z.ToDouble() + actor.SpawnZOffset;
            actor.Z = Fixed.FromDouble(Math.Clamp(spawnZ, short.MinValue, short.MaxValue));
            actor.OnGround = actor.SpawnCeiling ? actor.Z.ToDouble() <= FloorOf(actor.SectorIndex) : actor.SpawnZOffset <= 0;
            ActorPhysics.FitToSector(this, actor, carryFloor: false);
            actor.RememberPosition();
        }
        Acs = new AcsVm();
        var catalog = AcsStartupCatalog.Empty;
        if (level.HasBehavior && !AcsBehaviorBinder.TryBind(Acs, level.BehaviorData.Span, out catalog, out var behaviorError))
            throw new InvalidDataException(behaviorError ?? "acs-map-bind-failed");
        _deathScripts = catalog.Death;
        _respawnScripts = catalog.Respawn;
        // p_mobj.cpp starts SCRIPT_Enter with ACS_ALWAYS when a player enters, after OPEN was queued.
        // One zero argument matches the native arg1. Death and respawn wait for those events.
        foreach (var player in Players)
        {
            foreach (var number in catalog.Enter)
            {
                if (!Acs.ExecuteAlways(number, [0], player))
                    throw new InvalidDataException("acs-enter-script-start-failed");
            }
        }
        StrArgs = spawnOptions.StrArgs ?? Array.Empty<string>();
        Invasion = new InvasionDirector();
        Rewind = new RewindBuffer();
        ReverbActive = compat.HasFlag(CompatSurface.Eternity)
            && level.Sectors.Any(sector => sector.Special == CompatSurfaceRules.EternityReverbSpecial);
        RecomputeChecksum();
        PublishStatus();
    }

    public PlayLevel Level { get; }
    private readonly List<SimulationSoundRequest> _soundRequests = new();
    public IReadOnlyList<SimulationSoundRequest> PendingSoundRequests => _soundRequests.AsReadOnly();
    internal void QueueSoundRequest(SimulationSoundRequest request) => _soundRequests.Add(request);
    public SimulationSoundRequest[] DrainSoundRequests()
    {
        var requests = _soundRequests.ToArray();
        _soundRequests.Clear();
        return requests;
    }
    internal Dictionary<int, int> HealthGroups { get; }
    /// <summary>Native command-line str args for ACS <c>StrArg</c>.</summary>
    public IReadOnlyList<string> StrArgs { get; }
    public ThinkerCollection Thinkers { get; }
    public IReadOnlyList<Actor> Actors => _actors;
    public IEnumerable<PlayerPawn> Players => _actors.OfType<PlayerPawn>();
    public int RngSeed { get; }
    public uint Checksum { get; private set; }
    public string StatusLine { get; private set; } = "";
    public CompatSurface Compat { get; }
    public SpawnGameMode GameMode { get; }
    /// <summary>
    /// Single-player death sets this instead of moving the corpse. Native <c>G_DoReborn</c> reloads the map.
    /// There is no <c>G_InitNew</c> here.
    /// </summary>
    public bool ReloadRequested { get; private set; }
    /// <summary>Native <c>sv_singleplayerrespawn</c>. When set, single-player death revives at the player start.</summary>
    public bool AllowSinglePlayerRespawn { get; set; }
    /// <summary>Native <c>sv_forcerespawn</c>. Deathmatch only: revive after the wait without a button press.</summary>
    public bool ForceRespawn { get; set; }
    /// <summary>Native <c>sv_norespawn</c>. Off until set. A buffered press is kept and used once this clears.</summary>
    public bool NoRespawn { get; set; }
    /// <summary>Native <c>sv_weapondrop</c>. Off until set. Fist has no pickup.</summary>
    public bool WeaponDrop { get; set; }
    /// <summary>Native level Massacre subset. Excludes dormant monsters; baddies excludes friendlies.</summary>
    public int Massacre(bool baddies = false)
    {
        var killed = 0;
        foreach (var actor in _actors.Where(a => !a.Destroyed && a.IsMonster && !a.Dormant
            && (!baddies || !a.Friendly)).ToArray())
            if (actor.Massacre()) killed++;
        return killed;
    }
    /// <summary>Native sv_weaponstay: non-dropped weapons stay available.</summary>
    public bool WeaponStay { get; set; }
    /// <summary>Native alwaysapplydmflags: use deathmatch weapon retention rules in cooperative play.</summary>
    public bool AlwaysApplyDmFlags { get; set; }
    internal bool NonDroppedWeaponsStay =>
        (GameMode == SpawnGameMode.Cooperative && !AlwaysApplyDmFlags) || WeaponStay;
    /// <summary>Native <c>sv_cooploseinventory</c>. Off until set. Cooperative and single-player respawn only.</summary>
    public bool CoopLoseInventory { get; set; }
    /// <summary>Native <c>sv_cooplosekeys</c>. Drops every key unless <see cref="CoopShareKeys"/> is set.</summary>
    public bool CoopLoseKeys { get; set; }
    /// <summary>Native <c>sv_coopsharekeys</c>. Off until set. Cooperative key pickups are copied to every player.</summary>
    public bool CoopShareKeys { get; set; }
    /// <summary>Native <c>sv_cooploseweapons</c>. Fist and pistol stay.</summary>
    public bool CoopLoseWeapons { get; set; }
    /// <summary>Native <c>sv_cooplosearmor</c>. There is no default armor, so this clears it.</summary>
    public bool CoopLoseArmor { get; set; }
    /// <summary>Native <c>sv_cooploseammo</c>. Bullets return to 50. Other pools return to 0.</summary>
    public bool CoopLoseAmmo { get; set; }
    /// <summary>Native <c>sv_coophalveammo</c>. Ignored when <see cref="CoopLoseAmmo"/> is set.</summary>
    public bool CoopHalveAmmo { get; set; }
    /// <summary>Native <c>sv_spawnfarthest</c>. Deathmatch respawn only, and only while another player is alive.</summary>
    public bool SpawnFarthest { get; set; }
    private readonly int[] _deathScripts = [];
    private readonly int[] _respawnScripts = [];
    public int Skill { get; }
    public bool FastMonsters { get; }
    public bool SlowMonsters { get; }
    public double HealthFactor { get; }
    public double ArmorFactor { get; }
    /// <summary>Native <c>sv_ammofactor</c>. Multiplies the skill ammo factor. 1 leaves the skill value alone.</summary>
    public double AmmoFactor { get; set; } = 1;
    /// <summary>Native <c>sv_doubleammo</c>. Off until set. Replaces the skill factor with 2.</summary>
    public bool DoubleAmmo { get; set; }
    /// <summary>Native sv_noextraammo: disable the Doom deathmatch bonus for newly acquired weapons.</summary>
    public bool NoExtraAmmo { get; set; }
    private double _dropAmmoFactor = -1;
    /// <summary>Native skill DropAmmoFactor. -1 uses half ammo followed by ordinary skill scaling.</summary>
    public double DropAmmoFactor
    {
        get => _dropAmmoFactor;
        set
        {
            if (!double.IsFinite(value) || (value < 0 && value != -1))
                throw new ArgumentOutOfRangeException(nameof(value));
            _dropAmmoFactor = value;
        }
    }
    public int DefaultDropStyle { get; }
    /// <summary>Native <c>sv_dropstyle</c>; 0 uses the configured game default, 2 selects the Strife toss.</summary>
    public int DropStyle { get; set; }
    internal int EffectiveDropStyle => DropStyle == 0 ? DefaultDropStyle : DropStyle;
    /// <summary>Native <c>infighting</c> cvar. -1 never, 0 standard Doom, 1 always.</summary>
    public int Infighting { get; set; }
    public bool Exited { get; private set; }
    public bool SecretExit { get; private set; }
    public bool ReverbActive { get; }
    public bool RewindEnabled { get; set; }
    /// <summary>Native ACS <c>SetMusicVolume</c> scale. Audio playback is absent.</summary>
    public double MusicVolume { get; set; } = 1;
    public AcsVm Acs { get; }
    public InvasionDirector Invasion { get; }
    public RewindBuffer Rewind { get; }
    internal double[] Floors { get; }
    internal double[] Ceilings { get; }
    internal short[] Lights { get; }
    internal double[] SectorScrollX { get; }
    internal double[] SectorScrollY { get; }
    private List<SectorCarryScroll> _sectorCarryScrolls;
    private readonly List<ControlSectorCarryScroll> _controlCarryScrolls = [];
    private readonly List<AcceleratingSectorCarryScroll> _acceleratingCarryScrolls = [];
    private readonly List<SectorCarryScroll> _displacementCarryScrolls = [];
    private readonly List<SectorTextureScroll> _textureScrolls = [];
    private readonly List<SideTextureScroll> _sideTextureScrolls = [];
    private readonly List<ControlPlaneScroll> _controlPlaneScrolls = [];
    private readonly List<ControlWallScroll> _controlWallScrolls = [];

    internal void AppendSideTextureScroll(int side, double dx, double dy, WallScrollParts parts = WallScrollParts.All) =>
        _sideTextureScrolls.Add(new SideTextureScroll(side, dx, dy, parts));

    internal void AppendControlWallScroll(int side, int control, double dx, double dy, bool accelerating,
        WallScrollParts parts = WallScrollParts.All) =>
        _controlWallScrolls.Add(new ControlWallScroll(side, control, dx, dy,
            Floors[control] + Ceilings[control], accelerating, parts));

    internal void AppendControlPlaneScroll(int target, int control, double dx, double dy, bool accelerating, SectorTextureScrollPlane plane) =>
        _controlPlaneScrolls.Add(new ControlPlaneScroll(target, control, dx, dy,
            Floors[control] + Ceilings[control], accelerating, plane));
    /// <summary>Native <c>netgame</c> for ACS. Client-hosted sessions are absent.</summary>
    public bool IsNetworkGame => false;
    internal List<LightEffect> LightEffects { get; } = [];
    internal bool HasLightOverride { get; set; }
    internal bool HasLightAnimationOverride { get; set; }
    public short LightOf(int sector) => (uint)sector < (uint)Lights.Length ? Lights[sector] : (short)0;
    internal List<SectorMotion> Motions => _motions;

    public double FloorOf(int sector) => sector >= 0 && sector < Floors.Length ? Floors[sector] : (short)0;
    public double CeilingOf(int sector) => sector >= 0 && sector < Ceilings.Length ? Ceilings[sector] : (short)0;

    /// <summary>Replace constant texture scrollers on one sector. Runtime actions use tag-wide SetScroller semantics.</summary>
    public void SetSectorTextureScroll(int sector, double dx, double dy, SectorTextureScrollPlane plane)
    {
        if ((uint)sector >= (uint)Level.Sectors.Count)
            return;
        _textureScrolls.RemoveAll(scroll => scroll.SectorIndex == sector && scroll.Plane == plane);
        if (dx != 0 || dy != 0)
            _textureScrolls.Add(new SectorTextureScroll(sector, dx, dy, plane));
    }

    internal void AppendSectorTextureScroll(int sector, double dx, double dy, SectorTextureScrollPlane plane) =>
        _textureScrolls.Add(new SectorTextureScroll(sector, dx, dy, plane));

    // Native SetScroller updates every existing thinker of the selected type/tag.
    // Finding even one suppresses creation on other sectors with that tag.
    internal void SetTaggedScroller(int tag, double dx, double dy, SectorTextureScrollPlane? plane)
    {
        bool Matches(int sector) => Level.Sectors[sector].HasTag(tag);
        var updated = false;
        if (plane is { } texturePlane)
        {
            for (var i = 0; i < _textureScrolls.Count; i++)
            {
                var scroll = _textureScrolls[i];
                if (scroll.Plane != texturePlane || !Matches(scroll.SectorIndex)) continue;
                _textureScrolls[i] = scroll with { Dx = dx, Dy = dy };
                updated = true;
            }
            foreach (var scroll in _controlPlaneScrolls)
            {
                if (scroll.Plane != texturePlane || !Matches(scroll.Target)) continue;
                scroll.Dx = dx; scroll.Dy = dy;
                updated = true;
            }
        }
        else
        {
            for (var i = 0; i < _sectorCarryScrolls.Count; i++)
            {
                var scroll = _sectorCarryScrolls[i];
                if (!Matches(scroll.SectorIndex)) continue;
                _sectorCarryScrolls[i] = scroll with { Dx = dx, Dy = dy };
                updated = true;
            }
            foreach (var scroll in _acceleratingCarryScrolls)
            {
                if (!Matches(scroll.SectorIndex)) continue;
                scroll.BaseDx = dx; scroll.BaseDy = dy;
                updated = true;
            }
            foreach (var scroll in _controlCarryScrolls)
            {
                if (!Matches(scroll.TargetSector)) continue;
                scroll.ScaleX = dx; scroll.ScaleY = dy;
                updated = true;
            }
        }
        if (updated || dx == 0 && dy == 0) return;
        for (var sector = 0; sector < Level.Sectors.Count; sector++)
        {
            if (!Level.Sectors[sector].MatchesTag(tag)) continue;
            if (plane is { } targetPlane) AppendSectorTextureScroll(sector, dx, dy, targetPlane);
            else AppendSectorCarryScroll(sector, dx, dy);
        }
    }

    /// <summary>Native <c>SetWallScroller</c> for linedefs with matching id (tag).</summary>
    public void SetWallTextureScroll(int lineId, int sideChoice, double dx, double dy, WallScrollParts parts)
    {
        parts &= WallScrollParts.All;
        if (parts == 0)
            return;

        if (dx == 0 && dy == 0)
        {
            foreach (var line in AcsLineActivation.LinesFromId(this, lineId))
                RemoveWallScrollForLine(line, sideChoice, parts);
            return;
        }

        foreach (var line in AcsLineActivation.LinesFromId(this, lineId))
            UpsertWallScrollForLine(line, sideChoice, dx, dy, parts);
    }

    private void RemoveWallScrollForLine(LevelLine line, int sideChoice, WallScrollParts parts)
    {
        var sideIndex = sideChoice == 0 ? line.SideFront : line.SideBack;
        if (sideIndex < 0)
            return;
        _sideTextureScrolls.RemoveAll(scroll => scroll.SideIndex == sideIndex && scroll.Parts == parts);
        _controlWallScrolls.RemoveAll(scroll => scroll.Side == sideIndex && scroll.Parts == parts);
    }

    internal void UpsertWallScrollForSide(int sideIndex, double dx, double dy, WallScrollParts parts)
    {
        if (sideIndex < 0)
            return;
        parts &= WallScrollParts.All;
        if (parts == 0)
            return;
        var updated = false;
        for (var i = 0; i < _sideTextureScrolls.Count; i++)
        {
            var scroll = _sideTextureScrolls[i];
            if (scroll.SideIndex != sideIndex || scroll.Parts != parts)
                continue;
            _sideTextureScrolls[i] = scroll with { Dx = dx, Dy = dy };
            updated = true;
        }

        foreach (var scroll in _controlWallScrolls)
            {
                if (scroll.Side != sideIndex || scroll.Parts != parts) continue;
                scroll.Dx = dx; scroll.Dy = dy;
                updated = true;
            }

        if (!updated)
            _sideTextureScrolls.Add(new SideTextureScroll(sideIndex, dx, dy, parts));
    }

    private void UpsertWallScrollForLine(LevelLine line, int sideChoice, double dx, double dy, WallScrollParts parts)
    {
        var sideIndex = sideChoice == 0 ? line.SideFront : line.SideBack;
        UpsertWallScrollForSide(sideIndex, dx, dy, parts);
    }

    /// <summary>Replace constant/accelerative carry on one sector. Runtime actions use tag-wide SetScroller semantics.</summary>
    public void SetSectorScroll(int sector, double dx, double dy, ScrollCarryAffect affect = ScrollCarryAffect.All)
    {
        if ((uint)sector >= (uint)SectorScrollX.Length) return;
        _sectorCarryScrolls.RemoveAll(scroll => scroll.SectorIndex == sector);
        _acceleratingCarryScrolls.RemoveAll(scroll => scroll.SectorIndex == sector);
        if (dx != 0 || dy != 0)
            _sectorCarryScrolls.Add(new SectorCarryScroll(sector, dx, dy, affect));
    }

    /// <summary>Replace constant/accelerative carry on one sector with an accelerative scroller.</summary>
    public void SetAcceleratingSectorScroll(int sector, double baseDx, double baseDy, ScrollCarryAffect affect = ScrollCarryAffect.All)
    {
        if ((uint)sector >= (uint)SectorScrollX.Length) return;
        _sectorCarryScrolls.RemoveAll(scroll => scroll.SectorIndex == sector);
        _acceleratingCarryScrolls.RemoveAll(scroll => scroll.SectorIndex == sector);
        if (baseDx == 0 && baseDy == 0)
            return;
        _acceleratingCarryScrolls.Add(new AcceleratingSectorCarryScroll(sector, baseDx, baseDy, affect));
    }

    /// <summary>Map-load accelerative carry (does not clear other scrollers on the sector).</summary>
    public void AppendAcceleratingSectorCarryScroll(int sector, double baseDx, double baseDy, ScrollCarryAffect affect = ScrollCarryAffect.All)
    {
        if ((uint)sector >= (uint)SectorScrollX.Length) return;
        if (baseDx == 0 && baseDy == 0) return;
        _acceleratingCarryScrolls.Add(new AcceleratingSectorCarryScroll(sector, baseDx, baseDy, affect));
    }

    /// <summary>Add another carry scroller on the sector. Multiple entries sum each tic like separate <c>DScroller</c> thinkers.</summary>
    public void AppendSectorCarryScroll(int sector, double dx, double dy, ScrollCarryAffect affect = ScrollCarryAffect.All)
    {
        if ((uint)sector >= (uint)SectorScrollX.Length) return;
        _sectorCarryScrolls.Add(new SectorCarryScroll(sector, dx, dy, affect));
    }

    internal void RegisterControlSectorCarry(
        int targetSector,
        int controlSector,
        double scaleX,
        double scaleY,
        double lastCenter,
        bool accelerative = false)
    {
        if ((uint)targetSector >= (uint)SectorScrollX.Length || (uint)controlSector >= (uint)Floors.Length)
            return;
        _controlCarryScrolls.Add(new ControlSectorCarryScroll(
            targetSector, controlSector, scaleX, scaleY, lastCenter, accelerative));
    }

    internal void MarkInScrollSectorActors()
    {
        foreach (var actor in _actors)
            actor.InScrollSector = false;
        if (_sectorCarryScrolls.Count == 0 && _controlCarryScrolls.Count == 0 && _acceleratingCarryScrolls.Count == 0)
            return;

        foreach (var actor in _actors)
        {
            if (actor.Destroyed || actor.Floating || !actor.OnGround || actor is ProjectileActor or PlayerPawn)
                continue;
            var radius = actor.Radius.ToDouble();
            var x = actor.X.ToDouble();
            var y = actor.Y.ToDouble();
            foreach (var sector in ActorPhysics.TouchingSectorIndices(Level, x, y, radius))
            {
                foreach (var scroll in _sectorCarryScrolls.Concat(_displacementCarryScrolls))
                {
                    if (scroll.SectorIndex != sector)
                        continue;
                    if (!SectorCarryScroll.Affects(actor, scroll.Affect))
                        continue;
                    if (Math.Abs(scroll.Dx) < 1e-9 && Math.Abs(scroll.Dy) < 1e-9)
                        continue;
                    actor.InScrollSector = true;
                    break;
                }
                if (actor.InScrollSector)
                    break;
            }
        }
    }

    internal (double X, double Y) SumCarryScrollForActor(int sector, Actor actor)
    {
        var x = 0.0;
        var y = 0.0;
        foreach (var scroll in _sectorCarryScrolls.Concat(_displacementCarryScrolls))
        {
            if (scroll.SectorIndex != sector)
                continue;
            if (!SectorCarryScroll.Affects(actor, scroll.Affect))
                continue;
            x += scroll.Dx;
            y += scroll.Dy;
        }
        return (x, y);
    }

    internal void ApplySectorTextureScrolls()
    {
        foreach (var scroll in _controlPlaneScrolls)
        {
            var height = Floors[scroll.Control] + Ceilings[scroll.Control];
            var delta = height - scroll.LastHeight;
            scroll.LastHeight = height;
            var dx = scroll.Dx * delta;
            var dy = scroll.Dy * delta;
            if (scroll.Accelerating)
            {
                scroll.Vdx += dx; scroll.Vdy += dy;
                dx = scroll.Vdx; dy = scroll.Vdy;
            }
            if (dx == 0 && dy == 0) continue;
            var sector = Level.Sectors[scroll.Target];
            var (tx, ty) = SectorTextureRotation.Compensate(sector, scroll.Plane, dx, dy);
            if (scroll.Plane == SectorTextureScrollPlane.Floor)
            {
                sector.FloorTextureOffsetX += tx; sector.FloorTextureOffsetY += ty;
            }
            else
            {
                sector.CeilingTextureOffsetX += tx; sector.CeilingTextureOffsetY += ty;
            }
        }
        foreach (var scroll in _textureScrolls)
        {
            if ((uint)scroll.SectorIndex >= (uint)Level.Sectors.Count)
                continue;
            var sector = Level.Sectors[scroll.SectorIndex];
            var (tdx, tdy) = SectorTextureRotation.Compensate(sector, scroll.Plane, scroll.Dx, scroll.Dy);
            switch (scroll.Plane)
            {
                case SectorTextureScrollPlane.Floor:
                    sector.FloorTextureOffsetX += tdx;
                    sector.FloorTextureOffsetY += tdy;
                    break;
                case SectorTextureScrollPlane.Ceiling:
                    sector.CeilingTextureOffsetX += tdx;
                    sector.CeilingTextureOffsetY += tdy;
                    break;
            }
        }
    }

    internal void ApplySideTextureScrolls()
    {
        foreach (var scroll in _controlWallScrolls)
        {
            var height = Floors[scroll.Control] + Ceilings[scroll.Control];
            var delta = height - scroll.LastHeight;
            scroll.LastHeight = height;
            var dx = scroll.Dx * delta; var dy = scroll.Dy * delta;
            if (scroll.Accelerating)
            {
                scroll.Vdx += dx; scroll.Vdy += dy;
                dx = scroll.Vdx; dy = scroll.Vdy;
            }
            if (dx == 0 && dy == 0) continue;
            WallScrollActions.ApplySideOffsets(Level.Sides[scroll.Side],
                WallScrollActions.FindLineForSide(Level, scroll.Side), dx, dy, scroll.Parts);
        }
        foreach (var scroll in _sideTextureScrolls)
        {
            if ((uint)scroll.SideIndex >= (uint)Level.Sides.Count)
                continue;
            var side = Level.Sides[scroll.SideIndex];
            var line = WallScrollActions.FindLineForSide(Level, scroll.SideIndex);
            WallScrollActions.ApplySideOffsets(side, line, scroll.Dx, scroll.Dy, scroll.Parts);
        }
    }

    internal void RebuildSectorCarryScrolls()
    {
        _displacementCarryScrolls.Clear();
        Array.Clear(SectorScrollX, 0, SectorScrollX.Length);
        Array.Clear(SectorScrollY, 0, SectorScrollY.Length);
        foreach (var scroll in _sectorCarryScrolls)
        {
            SectorScrollX[scroll.SectorIndex] += scroll.Dx;
            SectorScrollY[scroll.SectorIndex] += scroll.Dy;
        }

        foreach (var scroll in _controlCarryScrolls)
        {
            var center = Floors[scroll.ControlSector] + Ceilings[scroll.ControlSector];
            var delta = center - scroll.LastCenter;
            scroll.LastCenter = center;
            var dx = delta * scroll.ScaleX;
            var dy = delta * scroll.ScaleY;
            if (scroll.Accelerative)
            {
                scroll.Vdx += dx;
                scroll.Vdy += dy;
                dx = scroll.Vdx;
                dy = scroll.Vdy;
            }

            if (dx == 0 && dy == 0) continue;

            _displacementCarryScrolls.Add(new SectorCarryScroll(scroll.TargetSector, dx, dy));
            SectorScrollX[scroll.TargetSector] += dx;
            SectorScrollY[scroll.TargetSector] += dy;
        }

        foreach (var scroll in _acceleratingCarryScrolls)
        {
            scroll.Vdx += scroll.BaseDx;
            scroll.Vdy += scroll.BaseDy;
            _displacementCarryScrolls.Add(new SectorCarryScroll(scroll.SectorIndex, scroll.Vdx, scroll.Vdy, scroll.Affect));
            SectorScrollX[scroll.SectorIndex] += scroll.Vdx;
            SectorScrollY[scroll.SectorIndex] += scroll.Vdy;
        }
    }

    /// <summary>Native <c>Level-&gt;GetInfighting()</c> with per-actor overrides.</summary>
    internal int GetInfightLevel(Actor actor)
    {
        if (actor.NoInfighting) return -1;
        var level = Infighting;
        if (level < 0 && actor.ForceInfighting) return 0;
        return level;
    }

    public static AuthoritySimulation Start(
        PlayLevel level,
        int rngSeed = 0,
        DehackedPatchResult? dehacked = null,
        CompatSurface compat = CompatSurface.None, SpawnOptions? spawnOptions = null, bool noExit = false)
    {
        level = level.CopyForSimulation();
        spawnOptions ??= new SpawnOptions();
        var thinkers = new ThinkerCollection();
        var mapRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x46c478d0u);
        var mapRandomUsed = false;
        uint MapRandom() { mapRandomUsed = true; return NativeStateRandom.Next(ref mapRandomState) & 255; }
        var actors = ActorSpawner.Spawn(level, thinkers, dehacked, spawnOptions, MapRandom).ToList();
        var simulation = new AuthoritySimulation(level, thinkers, actors, rngSeed, compat,
            !noExit || spawnOptions.Mode != SpawnGameMode.Deathmatch, spawnOptions, dehacked);
        simulation._mapSpawnRandomState = mapRandomState;
        simulation._hasMapSpawnRandomState = mapRandomUsed;
        return simulation;
    }

    /// <summary>
    /// <c>sv_weapondrop</c> copy of the ready weapon. The corpse keeps its inventory.
    /// The pickup copies held primary ammo and ignores the skill ammo factor.
    /// </summary>
    internal void DropSelectedWeapon(PlayerPawn player)
    {
        if (!WeaponDrop || !PickupCatalog.TryWeaponEdNum(player.Inventory.Selected, out var type))
            return;
        var ammo = WeaponCatalog.Find(player.Inventory.Selected)?.Ammo;
        var amount = ammo is { } kind ? Math.Max(0, player.Inventory.Ammo(kind)) : 0;
        // Native A_DropItem evaluates its chance even when 256 guarantees success.
        NextCombatRandom();
        SpawnDroppedPickup(player, type, ignoreAmmoSkill: true, pickupAmount: amount,
            suppressWeaponAmmo: ammo.HasValue && amount == 0);
    }

    internal bool SpawnDroppedPickup(Actor dropper, int doomEdNum, bool ignoreAmmoSkill = false, int pickupAmount = 0,
        bool inventoryToss = false, bool depleted = false, bool suppressWeaponAmmo = false)
    {
        if (dropper.Destroyed || !PickupCatalog.IsPickup(doomEdNum))
            return false;
        var adjustedAmount = ignoreAmmoSkill ? pickupAmount
            : PickupCatalog.DropPickupAmount(doomEdNum, pickupAmount, DropAmmoFactor);
        var customDropFactor = !inventoryToss && !ignoreAmmoSkill && DropAmmoFactor != -1
            && PickupCatalog.UsesDropAmmoFactor(doomEdNum);
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var drop = new Actor
        {
            Id = id,
            DoomEdNum = doomEdNum,
            X = dropper.X,
            Y = dropper.Y,
            Level = Level,
            Solid = false,
            Shootable = false,
            IgnoreAmmoSkill = ignoreAmmoSkill || customDropFactor,
            Dropped = true,
            Depleted = depleted,
            SuppressWeaponPickupAmmo = suppressWeaponAmmo
                || customDropFactor && adjustedAmount == 0,
            PickupAmount = adjustedAmount,
            SpecialPickup = true,
            Height = PickupCatalog.HeightOf(doomEdNum),
        };
        drop.Simulation = this;
        ActorPhysics.PlaceOnFloor(this, drop);
        if (inventoryToss)
        {
            drop.Angle = dropper.Angle;
            drop.Z = Fixed.FromDouble(dropper.Z.ToDouble() + 10);
            var radians = dropper.Angle.Raw * (2 * Math.PI / 4294967296.0);
            drop.VelocityX = Fixed.FromDouble(5 * Math.Cos(radians) + dropper.VelocityX.ToDouble());
            drop.VelocityY = Fixed.FromDouble(5 * Math.Sin(radians) + dropper.VelocityY.ToDouble());
            drop.VelocityZ = Fixed.FromDouble(1 + dropper.VelocityZ.ToDouble());
            drop.PickupDelay = 30;
            drop.SpecialPickup = false;
        }
        var toss = !Compat.HasFlag(CompatSurface.NoTossDrops);
        var strifeStyle = EffectiveDropStyle == 2;
        if (!inventoryToss)
            drop.Z = Fixed.FromDouble(dropper.Z.ToDouble() + (toss ? strifeStyle ? 24 : dropper.Height.ToDouble() / 2 : 0));
        if (toss && !inventoryToss)
        {
            var mask = strifeStyle ? 7u : 255u;
            var divisor = strifeStyle ? 1.0 : 256.0;
            drop.VelocityX = Fixed.FromDouble(((int)(NextDropItemByte() & mask) - (int)(NextDropItemByte() & mask)) / divisor);
            drop.VelocityY = Fixed.FromDouble(((int)(NextDropItemByte() & mask) - (int)(NextDropItemByte() & mask)) / divisor);
            if (!strifeStyle) drop.VelocityZ = Fixed.FromDouble(5 + NextDropItemByte() / 64.0);
        }
        drop.OnGround = drop.Z.ToDouble() <= FloorOf(drop.SectorIndex);
        drop.RememberPosition();
        _actors.Add(drop);
        Thinkers.Add(drop, ThinkerStat.Default);
        return true;
    }

    /// <summary>
    /// <c>BackpackItem.CreateTossable</c>. The pack leaves the player, the caps fall
    /// back to 200/50/50/300, and ammo above those caps is cut. The tossed thing is
    /// depleted, so picking it up restores the caps and gives no ammo. Pickup
    /// eligibility returns after the native 30-tic delay. A player who already has a pack
    /// still receives ammo from a depleted one, matching <c>HandlePickup</c>.
    /// </summary>
    public Actor? DropBackpack(PlayerPawn player)
    {
        if (!player.Inventory.HasBackpack || !SpawnDroppedPickup(player, PickupCatalog.Backpack,
            inventoryToss: true, depleted: true))
            return null;
        player.Inventory.RemoveBackpack();
        return _actors[^1];
    }

    public BotPawn AddBot(double x, double y, int doomEdNum = 3004, int thingId = 0)
    {
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == doomEdNum && actor.Patched);
        var definitionType = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : doomEdNum;
        var definition = DoomActorCatalog.Find(definitionType);
        var catalogHealth = definition?.Health;
        var health = defaults?.Health ?? catalogHealth ?? 30;
        var bot = new BotPawn
        {
            Id = id,
            DoomEdNum = doomEdNum,
            DefinitionDoomEdNum = definitionType,
            ThingId = thingId,
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Angle = new BamAngle(0),
            Health = health,
            ResurrectionHealth = health,
            GibHealth = health > 0 ? -health : -1,
            RaiseDuration = ArchvileActions.RaiseDuration(definitionType),
            Level = Level,
            Brain = MonsterBrain.ForType(definitionType) ?? new MonsterBrain(MonsterAttack.Hitscan),
            IsMonster = true,
            Radius = definition != null ? Fixed.FromInt(definition.Radius) : Fixed.FromInt(20),
            Height = definition != null ? Fixed.FromInt(definition.Height) : Fixed.FromInt(56),
            ChaseSpeed = Math.Clamp((definition?.Speed ?? 4) / 4.0, 0, ActorPhysics.MaxMove),
            PainChance = definition?.PainChance ?? 256,
            Damage = definition?.Damage ?? 0,
            NoGravity = definition?.Floating ?? false,
            Floating = definition?.Floating ?? false,
            NoRadiusDamage = definition?.NoRadiusDamage ?? false,
        };
        if (defaults != null)
        {
            bot.Radius = ActorSpawner.RadiusOf(defaults, player: false);
            bot.Height = ActorSpawner.HeightOf(defaults);
            bot.ChaseSpeed = Math.Clamp(defaults.Speed / 4.0, 0, ActorPhysics.MaxMove);
            bot.PainChance = defaults.PainChance;
        }
        if (defaults is { ReactionTimePatched: true }) bot.ReactionTime = defaults.ReactionTime;
        if (defaults is { InfightingGroupPatched: true }) bot.InfightingGroup = defaults.InfightingGroup;
        if (defaults is { ProjectileGroupPatched: true }) bot.ProjectileGroup = defaults.ProjectileGroup;
        if (defaults is { SplashGroupPatched: true }) bot.SplashGroup = defaults.SplashGroup;
        bot.Mass = defaults is { MassPatched: true } ? defaults.Mass : DoomActorCatalog.MassOf(definitionType);
        bot.Boss = definitionType is 7 or 16;
        if (defaults is { MissileDamagePatched: true }) bot.Damage = defaults.MissileDamage;
        ActorSpawner.ApplyPrimaryFlags(bot, defaults, playerStart: false);
        bot.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { BitsPatched: true }) bot.IsMonster = (defaults.Bits & 0x00400000) != 0;
        bot.SpawnCanPickupItems = bot.CanPickupItems;
        bot.SpawnSpecialPickup = bot.SpecialPickup;
        ActorSpawner.ApplyExtendedDefaults(bot, defaults);
        if (bot.IsMonster && Skill == 4) bot.ReactionTime = 0;
        ThingActivation.InitializeSpawn(bot, false, mapSpawn: false);
        bot.RememberPosition();
        bot.Simulation = this;
        bot.ResurrectionRadius ??= bot.Radius;
        bot.ResurrectionHeight ??= bot.Height;
        bot.ResurrectionCollisionFlags ??= ActorSpawner.CollisionFlagsOf(bot);
        bot.SpawnReactionTime ??= bot.ReactionTime;
        bot.SpawnDamage ??= bot.Damage;
        ActorPhysics.PlaceOnFloor(this, bot);
        if (bot.SpawnCeiling)
        {
            bot.Z = Fixed.FromDouble(CeilingOf(bot.SectorIndex) - bot.Height.ToDouble());
            bot.OnGround = bot.Z.ToDouble() <= FloorOf(bot.SectorIndex);
        }
        _actors.Add(bot);
        Thinkers.Add(bot, ThinkerStat.Default);
        return bot;
    }

    public uint NextCombatRandom()
    {
        CombatRandomState = unchecked(1664525u * CombatRandomState + 1013904223u);
        return CombatRandomState;
    }

    internal uint NextSwitchTargetRandom()
    {
        SwitchTargetRandomState = unchecked(1664525u * SwitchTargetRandomState + 1013904223u);
        return SwitchTargetRandomState;
    }

    internal uint NextUniqueTidRandom()
    {
        _uniqueTidRandomState = unchecked(1664525u * _uniqueTidRandomState + 1013904223u);
        return _uniqueTidRandomState;
    }

    private uint NextDmSpawnRandom()
    {
        DmSpawnRandomState = unchecked(1664525u * DmSpawnRandomState + 1013904223u);
        return DmSpawnRandomState;
    }

    internal double NextCombatSpread() => ((int)(NextCombatRandom() >> 24) - (int)(NextCombatRandom() >> 24)) / 255.0;
    private double NextIceChunkVelocity() => ((int)NextFreezeChunkByte() - (int)NextFreezeChunkByte()) / 128.0;

    /// <summary>Native <c>A_FreezeDeathChunks</c> moving-corpse delay and debris spawning subset.</summary>
    internal void SpawnIceChunks(Actor corpse)
    {
        if (corpse.Destroyed) return;
        if (!corpse.Shattering && (corpse.VelocityX.Raw != 0 || corpse.VelocityY.Raw != 0 || corpse.VelocityZ.Raw != 0))
        {
            corpse.States.ForceRemainingTics(105);
            return;
        }
        corpse.VelocityX = corpse.VelocityY = corpse.VelocityZ = default;
        var radius = corpse.Radius.ToDouble();
        var height = corpse.Height.ToDouble();
        var numChunks = Math.Max(4, (int)(radius * height / 32));
        var jitterSpan = Math.Max(1u, (uint)(numChunks / 4));
        var jitter = (int)NextFreezeChunkRange(jitterSpan);
        var spawnCount = Math.Max(24, numChunks + jitter);
        var baseX = corpse.X.ToDouble();
        var baseY = corpse.Y.ToDouble();
        var baseZ = corpse.Z.ToDouble();
        for (var i = spawnCount; i >= 0; i--)
        {
            var xo = ((int)NextFreezeChunkByte() - 128) * radius / 128;
            var yo = ((int)NextFreezeChunkByte() - 128) * radius / 128;
            var zo = NextFreezeChunkByte() * height / 255;
            _hasIceChunkLifecycle = true;
            var chunk = new IceChunkActor(10)
            {
                Id = _nextActorId,
                Level = Level,
                Simulation = this,
                X = Fixed.FromDouble(baseX + xo),
                Y = Fixed.FromDouble(baseY + yo),
                Z = Fixed.FromDouble(baseZ + zo),
            };
            _nextActorId = checked(_nextActorId + 1);
            chunk.States.Enter(chunk, (int)NextFreezeChunkRange(3));
            var spread = NextIceChunkVelocity();
            chunk.VelocityX = Fixed.FromDouble(spread);
            chunk.VelocityY = Fixed.FromDouble(NextIceChunkVelocity());
            chunk.VelocityZ = Fixed.FromDouble(height > 0 ? zo / height * 4 : 0);
            ActorPhysics.PlaceOnFloor(this, chunk);
            chunk.Z = Fixed.FromDouble(baseZ + zo);
            chunk.OnGround = chunk.Z.ToDouble() <= FloorOf(chunk.SectorIndex);
            chunk.RememberPosition();
            _actors.Add(chunk);
            Thinkers.Add(chunk, ThinkerStat.Default);
        }
        ActorUnblockActions.NoBlocking(corpse);
        corpse.States.Enter(corpse, corpse.States.HasState(corpse.NullState) ? corpse.NullState : -1);
    }

    // Independent managed stream: lighting must not change weapon damage/spread rolls.
    internal int NextJumpRandom()
    {
        _hasJumpRandomState = true;
        _jumpRandomState = unchecked(1664525u * _jumpRandomState + 1013904223u);
        return (int)(_jumpRandomState >> 24);
    }

    internal uint NextStateRandom()
    {
        _hasStateRandomState = true;
        return NativeStateRandom.Next(ref _stateRandomState);
    }

    internal uint NextClientStateRandom() => NativeStateRandom.Next(ref _clientStateRandomState);

    internal int NextIceTics()
    {
        _hasIceTicsRandomState = true;
        return 70 + (int)(NativeStateRandom.Next(ref _iceTicsRandomState) % 64);
    }

    internal uint NextFreezeChunkByte()
    {
        _hasFreezeChunksRandomState = true;
        return NativeStateRandom.Next(ref _freezeChunksRandomState) & 255;
    }

    internal int NextFreezeDeathTics()
    {
        _hasFreezeDeathRandomState = true;
        return 75 + (int)(NativeStateRandom.Next(ref _freezeDeathRandomState) & 255)
            + (int)(NativeStateRandom.Next(ref _freezeDeathRandomState) & 255);
    }

    internal uint NextDropItemByte()
    {
        _hasDropItemRandomState = true;
        return NativeStateRandom.Next(ref _dropItemRandomState) & 255;
    }

    internal uint NextFreezeChunkRange(uint bound)
    {
        if (bound == 1) return 0;
        if (bound == 0) throw new ArgumentOutOfRangeException(nameof(bound));
        _hasFreezeChunksRandomState = true;
        return NativeStateRandom.NextBounded(ref _freezeChunksRandomState, bound);
    }

    internal uint NextMapSpawnRandom()
    {
        _hasMapSpawnRandomState = true;
        return NativeStateRandom.Next(ref _mapSpawnRandomState) & 255;
    }

    internal int NextFlickerRandom()
    {
        _flickerRandomState = unchecked(1664525u * _flickerRandomState + 1013904223u);
        return (int)(_flickerRandomState >> 24);
    }

    internal int NextLightFlashRandom()
    {
        _lightFlashRandomState = unchecked(1664525u * _lightFlashRandomState + 1013904223u);
        return (int)(_lightFlashRandomState >> 24);
    }

    internal int NextFireFlickerRandom()
    {
        _fireFlickerRandomState = unchecked(1664525u * _fireFlickerRandomState + 1013904223u);
        return (int)(_fireFlickerRandomState >> 24);
    }

    internal int NextStrobeDelay()
    {
        _strobeRandomState = unchecked(1664525u * _strobeRandomState + 1013904223u);
        return (int)((_strobeRandomState >> 24) & 7) + 1;
    }

    internal PuffActor SpawnHitscanPuff(double x, double y, double z, int thingId = 0)
    {
        var puff = new PuffActor()
        {
            Id = _nextActorId,
            Level = Level,
            Simulation = this,
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Z = Fixed.FromDouble(z),
            ThingId = thingId,
        };
        _nextActorId = checked(_nextActorId + 1);
        ActorPhysics.PlaceOnFloor(this, puff);
        puff.Z = Fixed.FromDouble(z);
        puff.RememberPosition();
        _actors.Add(puff);
        Thinkers.Add(puff, ThinkerStat.Default);
        return puff;
    }

    public ProjectileActor SpawnProjectile(Actor owner, ProjectileKind kind, Actor? target = null)
    {
        var projectile = new ProjectileActor(owner, kind)
        {
            Id = _nextActorId, Level = Level, Simulation = this,
            X = owner.X, Y = owner.Y,
            Z = Fixed.FromDouble(owner.Z.ToDouble() + (owner is PlayerPawn firingPlayer
                ? owner.Height.ToDouble() / 2 + (firingPlayer.AttackZOffset.ToDouble() - 4) * firingPlayer.CrouchFactor
                : kind == ProjectileKind.RevenantTracer ? 48 : 32)),
            Angle = owner.Angle,
        };
        if (owner is PlayerPawn && projectile.Z.ToDouble() < FloorOf(owner.SectorIndex))
            projectile.Z = Fixed.FromDouble(FloorOf(owner.SectorIndex));
        var patchIndex = kind switch
        {
            ProjectileKind.RevenantTracer => 7, ProjectileKind.MancubusBall => 10,
            ProjectileKind.BaronBall => 17, ProjectileKind.ImpBall => 32,
            ProjectileKind.CacodemonBall => 33, ProjectileKind.Rocket or ProjectileKind.CyberRocket => 34,
            ProjectileKind.Plasma => 35, ProjectileKind.Bfg => 36, ProjectileKind.ArachnotronPlasma => 37,
            _ => 0,
        };
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.Index == patchIndex);
        if (defaults is { InfightingGroupPatched: true }) projectile.InfightingGroup = defaults.InfightingGroup;
        if (defaults is { ProjectileGroupPatched: true }) projectile.ProjectileGroup = defaults.ProjectileGroup;
        if (defaults is { SplashGroupPatched: true }) projectile.SplashGroup = defaults.SplashGroup;
        if (defaults is { MissileDamagePatched: true }) projectile.Damage = defaults.MissileDamage;
        if (defaults is { SpeedPatched: true }) projectile.MovementSpeed = Fixed.FromDouble(defaults.Speed);
        projectile.SpawnTuning = new(projectile.MovementSpeed.Raw, projectile.FloatSpeed, projectile.PainThreshold);
        if (defaults is { WidthPatched: true }) projectile.Radius = Fixed.FromDouble(defaults.Radius);
        if (defaults is { HeightPatched: true }) projectile.Height = Fixed.FromDouble(defaults.Height);
        projectile.ResurrectionRadius = projectile.Radius;
        projectile.ResurrectionHeight = projectile.Height;
        projectile.SpawnDamage = projectile.Damage;
        _nextActorId = checked(_nextActorId + 1);
        projectile.Aim(target);
        _actors.Add(projectile);
        Thinkers.Add(projectile);
        return projectile;
    }

    internal Actor? SpawnLostSoul(Actor parent, Actor? target, double angle, int limit = -1)
    {
        if (string.Equals(parent.DeathDamageType, "Massacre", StringComparison.OrdinalIgnoreCase)) return null;
        if (parent.SectorIndex >= 0 && parent.Z.ToDouble() + parent.Height.ToDouble() + 8 > CeilingOf(parent.SectorIndex))
        {
            if (parent.Floating)
            {
                parent.VelocityZ = Fixed.FromDouble(parent.VelocityZ.ToDouble() - 2);
                parent.InFloat = true;
                parent.VerticalFriction = true;
            }
            return null;
        }
        if (limit < 0 && Compat.HasFlag(CompatSurface.LimitPain)) limit = 21;
        if (limit > 0 && _actors.Count(actor => !actor.Destroyed && actor.ClassDoomEdNum == 3006) >= limit)
            return null;
        var definition = DoomActorCatalog.Find(3006)!;
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.OriginalDoomEdNum == 3006 && actor.Patched);
        var health = defaults?.Health ?? definition.Health;
        var soul = new Actor
        {
            Id = _nextActorId, DoomEdNum = defaults?.DoomEdNum ?? 3006, DefinitionDoomEdNum = 3006, Level = Level, Simulation = this,
            X = parent.X, Y = parent.Y, Z = Fixed.FromDouble(parent.Z.ToDouble() + 8),
            Radius = Fixed.FromInt(definition.Radius), Height = Fixed.FromInt(definition.Height),
            Health = health, ResurrectionHealth = health,
            GibHealth = health > 0 ? -health : -1,
            PainChance = definition.PainChance, ChaseSpeed = definition.Speed / 4.0,
            NoGravity = true, OnGround = false, SectorIndex = parent.SectorIndex,
            Floating = true, IsMonster = true, Damage = definition.Damage,
            Mass = DoomActorCatalog.MassOf(3006),
            Brain = MonsterBrain.ForType(3006), Angle = BamAngle.FromDegrees(angle),
        };
        if (defaults != null)
        {
            soul.Radius = ActorSpawner.RadiusOf(defaults, player: false);
            soul.Height = ActorSpawner.HeightOf(defaults);
            soul.ChaseSpeed = Math.Clamp(defaults.Speed / 4.0, 0, ActorPhysics.MaxMove);
            soul.PainChance = defaults.PainChance;
        }
        if (defaults is { ReactionTimePatched: true }) soul.ReactionTime = defaults.ReactionTime;
        if (defaults is { InfightingGroupPatched: true }) soul.InfightingGroup = defaults.InfightingGroup;
        if (defaults is { ProjectileGroupPatched: true }) soul.ProjectileGroup = defaults.ProjectileGroup;
        if (defaults is { SplashGroupPatched: true }) soul.SplashGroup = defaults.SplashGroup;
        if (defaults is { MassPatched: true }) soul.Mass = defaults.Mass;
        if (defaults is { MissileDamagePatched: true }) soul.Damage = defaults.MissileDamage;
        ActorSpawner.ApplyPrimaryFlags(soul, defaults, playerStart: false);
        soul.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { BitsPatched: true }) soul.IsMonster = (defaults.Bits & 0x00400000) != 0;
        soul.SpawnCanPickupItems = soul.CanPickupItems;
        soul.SpawnSpecialPickup = soul.SpecialPickup;
        ActorSpawner.ApplyExtendedDefaults(soul, defaults);
        if (soul.IsMonster && Skill == 4) soul.ReactionTime = 0;
        ThingActivation.InitializeSpawn(soul, false, mapSpawn: false);
        _nextActorId = checked(_nextActorId + 1);
        var distance = 4 + (parent.Radius.ToDouble() + soul.Radius.ToDouble()) * 1.5;
        var radians = angle * Math.PI / 180;
        var dx = Math.Cos(radians) * distance; var dy = Math.Sin(radians) * distance;
        var steps = Math.Max(1, (int)(1 + Math.Max(Math.Abs(dx), Math.Abs(dy)) / (soul.Radius.ToDouble() > 1 ? soul.Radius.ToDouble() - 1 : 16)));
        var solid = parent.Solid;
        var noTeleport = soul.NoTeleport;
        try
        {
            parent.Solid = false;
            soul.NoTeleport = true;
            for (var i = 0; i < steps; i++)
                if (!ActorPhysics.TryMove(this, soul, soul.X.ToDouble() + dx / steps, soul.Y.ToDouble() + dy / steps, out _))
                {
                    // Native keeps the failed spawn and applies ordinary telefrag damage.
                    // Do not register this failed child in the managed invasion wave.
                    ActorDamage.Apply(soul, ActorDamage.TelefragDamage, parent, inflictor: parent);
                    soul.RememberPosition();
                    _actors.Add(soul);
                    Thinkers.Add(soul);
                    return null;
                }
        }
        finally { parent.Solid = solid; soul.NoTeleport = noTeleport; }
        soul.Friendly = parent.Friendly;
        soul.FriendPlayer = parent.FriendPlayer;
        soul.TidToHate = parent.TidToHate;
        soul.NoHatePlayers = parent.NoHatePlayers;
        if (target is { Destroyed: false, NoTarget: false, NeverTarget: false })
        {
            soul.LastHeardTargetId = target.Id;
            soul.Brain!.StartCharge(soul, target);
        }
        soul.RememberPosition();
        soul.SpawnReactionTime ??= soul.ReactionTime;
        soul.SpawnDamage ??= soul.Damage;
        _actors.Add(soul);
        Thinkers.Add(soul);
        Invasion.RegisterChild(parent, soul);
        return soul;
    }

    public void MarkExited(bool secret)
    {
        Exited = true;
        SecretExit = secret;
    }

    public bool DamageExitAllowed { get; }

    public string Describe() =>
        $"OK map={Level.MapName} tic={Thinkers.Clock.Tic} players={Players.Count()} bots={_actors.OfType<BotPawn>().Count()} checksum={Checksum:x8} invasion={Invasion.Phase} exited={(Exited ? 1 : 0)} secret={(SecretExit ? 1 : 0)}";

    public SimSaveState CaptureState()
    {
        var state = new SimSaveState
        {
            LightAnimation = HasLightAnimationOverride || LightEffects.Count != 0
                ? new SimLightAnimation(_strobeRandomState, _flickerRandomState, _lightFlashRandomState, _fireFlickerRandomState,
                    LightEffects.Select(e => new SimLightEffect(e.Sector, (int)e.Kind, e.Start, e.End, e.Duration, e.DarkTime, e.Tics)).ToList()) : null,
            Lights = HasLightOverride || HasLightAnimationOverride || LightEffects.Count != 0 || Lights.Where((light, index) => light != Level.Sectors[index].LightLevel).Any() ? Lights.ToList() : null,
            JumpRandomState = _hasJumpRandomState ? _jumpRandomState : null,
            StateRandomState = _hasStateRandomState ? _stateRandomState : null,
            MapSpawnRandomState = _hasMapSpawnRandomState ? _mapSpawnRandomState : null,
            IceTicsRandomState = _hasIceTicsRandomState ? _iceTicsRandomState : null,
            IncludesIceChunkLifecycle = _hasIceChunkLifecycle,
            FreezeChunksRandomState = _hasFreezeChunksRandomState ? _freezeChunksRandomState : null,
            AirControlOverride = _airControlOverride,
            LevelGravityOverride = _levelGravityOverride,
            FreezeDeathRandomState = _hasFreezeDeathRandomState ? _freezeDeathRandomState : null,
            DropItemRandomState = _hasDropItemRandomState ? _dropItemRandomState : null,
            Tic = Thinkers.Clock.Tic,
            Exited = Exited,
            SecretExit = SecretExit,
            CombatRandomState = CombatRandomState,
            DamageTypes = DamageTypes.Capture(),
            IncludesDamageTypeDefinitions = Level.DamageTypes.Count != 0,
            Walls = Level.Sides.Select(SimWallTransform.Capture).ToList(),
            Planes = Level.Sectors.Select(SimPlaneTransform.Capture).ToList(),
            IncludesCarryScrolls = true,
            IncludesCeilingControls = true,
            IncludesFloorControls = true,
            IncludesWallControls = true,
            IncludesWallParts = true,
            GeometryHealth = CaptureGeometryHealth(),
            TextureScrolls = _textureScrolls.Select(s => new SimTextureScroll(s.SectorIndex, (int)s.Plane, s.Dx, s.Dy))
                .Concat(_sideTextureScrolls.Select(s => new SimTextureScroll(s.SideIndex, (int)s.Parts + 1, s.Dx, s.Dy)))
                .Concat(_sectorCarryScrolls.Select(s => new SimTextureScroll(s.SectorIndex, 9, s.Dx, s.Dy, Affect: s.Affect)))
                .Concat(_acceleratingCarryScrolls.Select(s => new SimTextureScroll(s.SectorIndex, 10, s.BaseDx, s.BaseDy,
                    Affect: s.Affect, Vdx: s.Vdx, Vdy: s.Vdy)))
                .Concat(_controlCarryScrolls.Select(s => new SimTextureScroll(s.TargetSector, s.Accelerative ? 12 : 11,
                    s.ScaleX, s.ScaleY, s.ControlSector, LastHeight: s.LastCenter, Vdx: s.Vdx, Vdy: s.Vdy)))
                .Concat(_controlPlaneScrolls.Select(s => new SimTextureScroll(s.Target, s.ArchiveKind,
                    s.Dx, s.Dy, s.Control, LastHeight: s.LastHeight, Vdx: s.Vdx, Vdy: s.Vdy)))
                .Concat(_controlWallScrolls.Select(s => new SimTextureScroll(s.Side, s.ArchiveKind,
                    s.Dx, s.Dy, s.Control, LastHeight: s.LastHeight, Vdx: s.Vdx, Vdy: s.Vdy))).ToList(),
        };
        var includesPlayerMaxHealth = Players.Any(p => p.MaxHealth != 0);
        foreach (var actor in _actors.OrderBy(actor => actor.Id))
        {
            state.Actors.Add(new SimActorPose
            {
                ActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds),
                DeathType = actor.DeathType,
                SpecialFireDamage = actor.SpecialFireDamage,
                FoilInvul = actor.FoilInvul,
                PierceArmor = actor.PierceArmor,
                NoInfightSpecies = actor.NoInfightSpecies,
                NoVerticalMeleeRange = actor.NoVerticalMeleeRange,
                MeleeRangeRaw = actor.MeleeRange != Fixed.FromInt(44) ? actor.MeleeRange.Raw : null,
                ConstantDamage = actor.Damage != actor.SpawnDamage.GetValueOrDefault() ? actor.Damage : null,
                JustHitFlag = actor.JustHit ? 1 : null,
                HandleNoDelayFlag = actor.HandleNoDelay ? 1 : null,
                SynchronizedFlag = actor.Synchronized ? 1 : null,
                IceChunkLifecycle = actor is IceChunkActor { Destroyed: false } ? 1 | (actor.JustSpawned ? 2 : 0) : null,
                FastModeFlags = actor.AlwaysFast || actor.NeverFast ? (actor.AlwaysFast ? 1 : 0) | (actor.NeverFast ? 2 : 0) : null,
                FullBrightFlag = actor.FullBright != actor.States.CurrentFullBright ? actor.FullBright ? 1 : 0 : null,
                GoalPointer = actor.GoalId.HasValue ? new SimGoalPointer(actor.GoalId) : null,
                Killed = actor.Killed,
                SlideFlag = actor.CanSlide != ((actor.ResurrectionMovementFlags.GetValueOrDefault() & 8) != 0) ? (actor.CanSlide ? 1 : 0) : null,
                MovementActionFlags = ActorMovementSave.Capture(actor),
                SkullChargeFlag = actor.Brain?.Charging == true ? 1 : null,
                ReactionTime = actor.ReactionTime != actor.SpawnReactionTime.GetValueOrDefault()
                    ? actor.ReactionTime : null,
                AllowDropOffFlag = actor.ResurrectionMovementFlags is { } dropOffDefaults
                    && actor.AllowDropOff != ((dropOffDefaults & 16) != 0)
                    ? actor.AllowDropOff ? 1 : 0 : null,
                UseSpecialFlag = actor.HasUseSpecialOverride ? actor.UseSpecial ? 1 : 0 : null,
                MasterPointer = actor.HasMasterOverride ? new SimMasterPointer(actor.MasterId) : null,
                FloorClip = actor.HasFloorClipOverride ? actor.FloorClip : null,
                InvisibleFlag = actor.HasVisibilityOverride ? actor.Invisible ? 1 : 0 : null,
                DontFallFlag = actor.DontFall != (actor.ClassDoomEdNum == 3006) ? actor.DontFall ? 1 : 0 : null,
                FallingFlag = actor.Falling ? 1 : null,
                DontCorpseFlag = actor.DontCorpse ? 1 : null,
                CorpseFlag = actor.Corpse != actor.IsDead ? actor.Corpse ? 1 : 0 : null,
                NoFrictionFlag = actor.NoFriction ? 1 : null,
                FlyFlag = actor.Fly ? 1 : null,
                ClassicFlightFlag = actor is PlayerPawn flightPlayer && flightPlayer.ClassicFlight ? 1 : null,
                JumpTics = actor is PlayerPawn jumpingPlayer && jumpingPlayer.JumpTics != 0 ? jumpingPlayer.JumpTics : null,
                Crouch = actor is PlayerPawn crouchPlayer ? SimCrouchArchive.Capture(crouchPlayer) : null,
                MonsterBlockingFlag = actor.NoBlockMonsters != ((actor.ResurrectionCollisionFlags.GetValueOrDefault() & 8) != 0)
                    ? actor.NoBlockMonsters ? 1 : 0 : null,
                BlockmapFlag = actor.HasBlockmapOverride || actor.ResurrectionCollisionFlags is { } blockmapDefaults
                    && actor.NoBlockmap != ((blockmapDefaults & 4) != 0) ? actor.NoBlockmap ? 1 : 0 : null,
                Friendship = actor.HasFriendshipOverride || actor.NoHatePlayers
                    || actor.Friendly != (actor.SpawnFriendly || (actor.ResurrectionDefenseFlags.GetValueOrDefault() & 8) != 0)
                    ? new SimFriendship(actor.FriendPlayer, actor.TidToHate, actor.Friendly, actor.NoHatePlayers) : null,
                GravityRaw = actor.HasGravityOverride || actor.Gravity.Raw != 65536 ? actor.Gravity.Raw : null,
                FrictionRaw = actor.Friction.Raw != 65536 ? actor.Friction.Raw : null,
                DefenseProperties = ActorDefenseSave.Capture(actor),
                SpriteOrientation = actor.HasSpriteOrientationOverride || actor.SpriteAngle != 0 || actor.SpriteRotation != 0
                    ? new SimSpriteOrientation(actor.SpriteAngle, actor.SpriteRotation) : null,
                FloatBobPhase = actor.HasFloatBobPhaseOverride || actor.FloatBobPhase != 0 ? actor.FloatBobPhase : null,
                Scale = actor.HasScaleOverride || actor.ScaleX != 1 || actor.ScaleY != 1 ? new SimActorScale(actor.ScaleX, actor.ScaleY) : null,
                Tuning = actor.HasTuningOverride
                    || new SimActorTuning(actor.MovementSpeed.Raw, actor.FloatSpeed, actor.PainThreshold)
                        != (actor.SpawnTuning ?? new SimActorTuning(Fixed.FromInt(4).Raw, 4, 0))
                    ? new SimActorTuning(actor.MovementSpeed.Raw, actor.FloatSpeed, actor.PainThreshold) : null,
                SpeciesOverride = actor.HasSpeciesOverride || actor.Species is not null ? new SimSpecies(actor.Species) : null,
                Size = actor.HasSizeOverride
                    || actor.ResurrectionRadius is { } spawnRadius && actor.Radius != spawnRadius
                    || actor.ResurrectionHeight is { } spawnHeight && actor.Height != spawnHeight
                    ? new SimActorSize(actor.Radius.Raw,
                    actor is PlayerPawn sizedPlayer ? sizedPlayer.FullHeight : 0, actor.Height.Raw) : null,
                TeleFog = actor.HasTeleFogOverride || actor.TeleFogSource is not null || actor.TeleFogDest is not null
                    ? new SimTeleFog(actor.TeleFogSource, actor.TeleFogDest) : null,
                ChaseThreshold = actor.Brain is { } brain && (brain.HasChaseThresholdOverride
                    || brain.Threshold != 0 || brain.DefThreshold != 100)
                    ? new SimChaseThreshold(brain.Threshold, brain.DefThreshold) : null,
                TargetMemory = actor.HasTargetMemoryOverride || actor.Brain?.TargetId is not null
                    || actor.Brain?.LastEnemyId is not null || actor.LastHeardTargetId is not null
                    ? new SimTargetMemory(actor.Brain?.TargetId, actor.Brain?.LastEnemyId, actor.LastHeardTargetId) : null,
                WornArmorType = actor is PlayerPawn armorPlayer
                    && (armorPlayer.Inventory.ArmorType != "None" || armorPlayer.Inventory.Armor != 0)
                    ? armorPlayer.Inventory.ArmorType : null,
                WornArmorAmount = actor is PlayerPawn amountPlayer ? amountPlayer.Inventory.Armor : 0,
                WornArmor = actor is PlayerPawn metadataPlayer ? SimWornArmor.Capture(metadataPlayer.Inventory) : null,
                ActorArmor = SimActorArmor.Capture(actor),
                SpareArmor = actor is PlayerPawn sparePlayer && sparePlayer.Inventory.SpareArmor.Count != 0
                    ? sparePlayer.Inventory.SpareArmor.ToArray() : null,
                DamageFactor = actor.DamageFactor.Raw,
                DamageMultiplier = actor.DamageMultiplier.Raw,
                DamageFactors = actor.CaptureDamageFactors(),
                StrifeDamage = actor.StrifeDamage,
                Rip = actor.Rip,
                DontRip = actor.DontRip,
                NoBossRip = actor.NoBossRip,
                Pushable = actor.Pushable,
                CannotPush = actor.CannotPush,
                PushFactor = actor.PushFactor,
                RipperLevel = actor.RipperLevel,
                RipLevelMin = actor.RipLevelMin,
                RipLevelMax = actor.RipLevelMax,
                ProjectilePassHeight = actor.ProjectilePassHeight.Raw,
                InfightingGroup = actor.InfightingGroup,
                ProjectileGroup = actor.ProjectileGroup,
                SplashGroup = actor.SplashGroup,
                PowerDamageTics = actor is PlayerPawn damagePlayer ? damagePlayer.PowerDamageTics : 0,
                PowerProtectionTics = actor is PlayerPawn protectionPlayer ? protectionPlayer.PowerProtectionTics : 0,
                PowerBuddhaTics = actor is PlayerPawn buddhaPlayer ? buddhaPlayer.PowerBuddhaTics : 0,
                DamageType = actor.DamageType,
                Id = actor.Id,
                X = actor.X.Raw,
                ActorSpecial = actor.SpecialChanged || actor.Special != 0 || actor.ActivationType != 0 || actor.SpecialArgs.Any(arg => arg != 0)
                    || actor is PlayerPawn { Stamina: not 0 } || actor is PlayerPawn { BonusHealth: not 0 } || actor is PlayerPawn { MaxPickupHealth: not 0 }
                    ? new SimActorSpecial(actor.Special, actor.SpecialArgs[0], actor.SpecialArgs[1], actor.SpecialArgs[2], actor.SpecialArgs[3], actor.SpecialArgs[4], actor.ActivationType,
                        actor is PlayerPawn staminaPlayer ? staminaPlayer.Stamina : 0, actor is PlayerPawn bonusPlayer ? bonusPlayer.BonusHealth : 0,
                        actor is PlayerPawn pickupPlayer ? pickupPlayer.MaxPickupHealth : 0) : null,
                Y = actor.Y.Raw,
                Angle = actor.Angle.Raw,
                Roll = actor.Roll.Raw,
                ContactFlags = (actor.CanPickupItems ? 1 : 0) | (actor.SpecialPickup ? 2 : 0),
                FloatFlags = (actor.InFloat ? 1 : 0) | (actor.VerticalFriction ? 2 : 0),
                IceCorpseFlag = _hasFreezeDeathRandomState ? actor.IceCorpse : null,
                DeathFlags = actor.DeathDamageType == "Massacre" ? 1 : 0,
                PainDeath = actor.Brain?.CapturePainDeath(),
                ProjectileFlags = actor.NoExplodeFloor ? 1 : 0,
                CeilingFlags = actor.CeilingHugger ? 1 : 0,
                FloorHuggerFlags = (actor.FloorHugger ? 1 : 0) | (actor.NoDropOff ? 2 : 0),
                BlastedFlags = actor.Blasted ? 1 : 0,
                BlastEligibilityFlags = (actor.Boss ? 1 : 0) | (actor.DontBlast ? 2 : 0),
                ThruActorsFlags = actor.ThruActors ? 1 : 0,
                MissileThruSpeciesFlags = actor.MThruSpecies ? 1 : 0,
                ThruSpeciesFlags = actor.ThruSpecies ? 1 : 0,
                ThruBits = new SimThruBits(actor.ThruBits, actor.AllowThruBits),
                GhostFlags = (actor.Ghost ? 1 : 0) | (actor.ThruGhost ? 2 : 0),
                NonShootableFlags = actor.NonShootable ? 1 : 0,
                HitOwnerFlags = actor.HitOwner ? 1 : 0,
                SpectralFlags = actor.Spectral ? 1 : 0,
                PlayerMaxHealth = includesPlayerMaxHealth
                    ? actor is PlayerPawn healthPlayer ? healthPlayer.MaxHealth : 0 : null,
                ProjectileLifetime = actor is ProjectileActor { Destroyed: false } projectile
                    ? new SimProjectileLifetime(projectile.RemainingTics, projectile.Kind) : null,
                ProjectilePointers = actor is ProjectileActor { Destroyed: false } pointerProjectile
                    ? new SimProjectilePointers(pointerProjectile.Owner.Id, pointerProjectile.TracerTargetId) : null,
                Pickup = PickupCatalog.IsPickup(actor.DoomEdNum)
                    ? new SimPickupProperties(actor.PickupAmount, actor.IgnoreAmmoSkill, actor.Depleted) : null,
                PickupDelay = actor.PickupDelay,
                Dropped = actor.Dropped,
                SuppressWeaponPickupAmmo = actor.SuppressWeaponPickupAmmo,
                AlwaysPickupOverride = actor.AlwaysPickupOverride,
                Health = actor.Health,
                Z = actor.Z.Raw,
                VelocityX = actor.VelocityX.Raw,
                VelocityY = actor.VelocityY.Raw,
                VelocityZ = actor.VelocityZ.Raw,
                State = actor.States.Current,
                StateTics = actor.States.RemainingTics,
                OnGround = actor.OnGround,
                WeaponCooldown = actor is PlayerPawn pawn ? pawn.WeaponCooldown : 0,
                Pitch = Fixed.FromDouble(actor.PitchDegrees).Raw,
                UseHeld = actor is PlayerPawn usingPawn && usingPawn.UseHeld,
                UseRange = actor is PlayerPawn rangePawn && rangePawn.HasUseRangeOverride ? rangePawn.UseRange : null,
            });
        }

        for (var i = 0; i < Floors.Length; i++)
            state.Sectors.Add((Floors[i], Ceilings[i]));
        return state;
    }

    public void RestoreState(SimSaveState state)
    {
        SimActorSoundArchive.Validate(state);
        if (state.LevelGravityOverride is { } gravity && !double.IsFinite(gravity))
            throw new InvalidOperationException("Invalid saved level gravity.");
        if (state.AirControlOverride is { } control && !double.IsFinite(control))
            throw new InvalidOperationException("Invalid saved air control.");
        SimSavegame.ValidateSectors(state);
        SimLightArchive.Validate(state);
        SimLightAnimationArchive.Validate(state);
        SimUseRangeArchive.Validate(state);
        SimUseSpecialArchive.Validate(state);
        if (state.Lights is { } savedLights && savedLights.Count != Lights.Length)
            throw new InvalidOperationException("Saved sector lights do not match the current level.");
        SimSavegame.ValidateWalls(state);
        SimSavegame.ValidatePlanes(state);
        SimSavegame.ValidateTextureScrolls(state);
        SimSavegame.ValidatePickups(state);
        SimSavegame.ValidateContactFlags(state);
        SimSavegame.ValidateFloatFlags(state);
        SimSlideFlagArchive.Validate(state);
        SimSavegame.ValidateDeathFlags(state);
        SimMovementActionArchive.Validate(state);
        SimDefensePropertiesArchive.Validate(state);
        SimTuningArchive.Validate(state);
        SimSizeArchive.Validate(state);
        SimScaleArchive.Validate(state);
        SimFloatBobPhaseArchive.Validate(state);
        SimFloorClipArchive.Validate(state);
        SimVisibilityArchive.Validate(state);
        SimDontFallArchive.Validate(state);
        SimAllowDropOffArchive.Validate(state);
        SimSkullChargeArchive.Validate(state);
        SimFallingFlagArchive.Validate(state);
        SimDontCorpseArchive.Validate(state);
        SimCorpseFlagArchive.Validate(state);
        SimNoFrictionArchive.Validate(state);
        SimFlyFlagArchive.Validate(state);
        SimClassicFlightArchive.Validate(state);
        SimCrouchArchive.Validate(state);
        SimMonsterBlockingArchive.Validate(state);
        SimSpriteOrientationArchive.Validate(state);
        SimProjectileFlagArchive.Validate(state);
        SimProjectileLifetimeArchive.Validate(state);
        SimProjectilePointerArchive.Validate(state);
        SimCeilingHuggerArchive.Validate(state);
        SimFloorHuggerArchive.Validate(state);
        SimBlastedArchive.Validate(state);
        SimBlastEligibilityArchive.Validate(state);
        SimThruActorsArchive.Validate(state);
        SimMissileThruSpeciesArchive.Validate(state);
        SimThruSpeciesArchive.Validate(state);
        SimThruBitsArchive.Validate(state);
        SimGhostArchive.Validate(state);
        SimNonShootableArchive.Validate(state);
        SimHitOwnerArchive.Validate(state);
        SimSpectralArchive.Validate(state);
        SimPlayerHealthArchive.Validate(state);
        SimPickupDelayArchive.Validate(state);
        SimPainDeathArchive.Validate(state);
        if (state.GeometryHealth is { } savedHealth && (savedHealth.Lines.Count != Level.Lines.Count
            || savedHealth.Sectors.Count != Level.Sectors.Count || !savedHealth.Groups.Keys.Order().SequenceEqual(HealthGroups.Keys.Order())))
            throw new InvalidOperationException("Saved geometry health does not match the current map.");
        if (state.TextureScrolls is { } savedScrolls && savedScrolls.Any(s =>
            s.Target >= (s.Kind is >= 2 and <= 8 or >= 17 ? Level.Sides.Count : Level.Sectors.Count)
                || s.Kind >= 11 && s.Control >= Level.Sectors.Count))
            throw new InvalidOperationException("Saved texture scroller target is outside the current map.");
        if (state.Planes is { } savedPlanes && savedPlanes.Count != Level.Sectors.Count)
            throw new InvalidOperationException("Saved plane count does not match the current map.");
        if (state.Walls is { } savedWalls && savedWalls.Count != Level.Sides.Count)
            throw new InvalidOperationException("Saved wall count does not match the current map.");
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (pose.IceChunkLifecycle is { } lifecycle && (lifecycle is not (1 or 3)
                || pose.Id == 0 || pose.Id == uint.MaxValue || pose.State is < 0 or > 3 || !pose.HasPhysics
                || actor is not null && actor is not IceChunkActor || state.Actors.Count(item => item.Id == pose.Id) != 1))
                throw new InvalidOperationException("Saved ice chunk does not match a valid chunk identity or state.");
            if (pose.ProjectilePointers is { } pointers &&
                (actor is not ProjectileActor pointerMissile || pointerMissile.Destroyed || pointerMissile.Owner.Id != pointers.OwnerId))
                throw new InvalidOperationException("Saved projectile pointers do not match the current actor.");
            if (pose.ProjectileLifetime is { } lifetime &&
                (actor is not ProjectileActor missile || missile.Destroyed || missile.Kind != lifetime.Kind))
                throw new InvalidOperationException("Saved projectile lifetime does not match the current actor.");
            if (pose.PainDeath.HasValue && (actor == null || actor.Destroyed || actor.Brain?.CapturePainDeath() == null))
                throw new InvalidOperationException("Saved Pain Elemental death state does not match the current actor.");
            if (pose.SkullChargeFlag == 1 && (actor == null || actor.Destroyed || actor.Brain == null))
                throw new InvalidOperationException("Saved skull charge does not match the current actor.");
            if (actor != null && (pose.WeaponCooldown < 0 || actor is PlayerPawn && Math.Abs((long)pose.Pitch) > 89L * 65536
                || pose.HasPhysics && !actor.States.HasState(pose.State)))
                throw new InvalidOperationException("Saved actor state is not in the current table.");
        }
        var definitions = state.DamageTypes;
        if (definitions is null)
        {
            var defaults = new DamageTypeCatalog();
            foreach (var definition in Level.DamageTypes)
                defaults.Define(definition.Name, definition.Factor, definition.ReplaceFactor, definition.NoArmor);
            definitions = defaults.Capture();
        }
        DamageTypes.Restore(definitions);
        if (state.Lights is { } lights)
        {
            lights.CopyTo(Lights); HasLightOverride = true;
        }
        if (state.LightAnimation is { } animation)
        {
            LightEffects.Clear();
            LightEffects.AddRange(animation.Effects.Select(e => new LightEffect
            { Sector = e.Sector, Kind = (LightEffectKind)e.Kind, Start = e.Start, End = e.End,
                Duration = e.Duration, DarkTime = e.DarkTime, Tics = e.Tics }));
            _strobeRandomState = animation.StrobeRandom; _flickerRandomState = animation.FlickerRandom;
            _lightFlashRandomState = animation.FlashRandom; _fireFlickerRandomState = animation.FireRandom;
            HasLightAnimationOverride = true;
        }
        if (state.JumpRandomState is { } jumpRandom) { _jumpRandomState = jumpRandom; _hasJumpRandomState = true; }
        _stateRandomState = state.StateRandomState ?? _initialStateRandomState;
        _hasStateRandomState = state.StateRandomState.HasValue;
        _clientStateRandomState = _initialClientStateRandomState;
        _mapSpawnRandomState = state.MapSpawnRandomState ?? _initialMapSpawnRandomState;
        _hasMapSpawnRandomState = state.MapSpawnRandomState.HasValue;
        _iceTicsRandomState = state.IceTicsRandomState ?? _initialIceTicsRandomState;
        _hasIceTicsRandomState = state.IceTicsRandomState.HasValue;
        _hasIceChunkLifecycle = state.IncludesIceChunkLifecycle;
        _freezeChunksRandomState = state.FreezeChunksRandomState ?? _initialFreezeChunksRandomState;
        _hasFreezeChunksRandomState = state.FreezeChunksRandomState.HasValue;
        _freezeDeathRandomState = state.FreezeDeathRandomState ?? _initialFreezeDeathRandomState;
        _hasFreezeDeathRandomState = state.FreezeDeathRandomState.HasValue;
        _dropItemRandomState = state.DropItemRandomState ?? _initialDropItemRandomState;
        _hasDropItemRandomState = state.DropItemRandomState.HasValue;
        _airControlOverride = state.AirControlOverride;
        _levelGravityOverride = state.LevelGravityOverride;
        Thinkers.Clock.Restore(state.Tic);
        if (state.GeometryHealth is { } health)
        {
            for (var i = 0; i < Level.Lines.Count; i++) Level.Lines[i].Health = health.Lines[i];
            for (var i = 0; i < Level.Sectors.Count; i++)
            {
                Level.Sectors[i].HealthFloor = health.Sectors[i].Floor;
                Level.Sectors[i].HealthCeiling = health.Sectors[i].Ceiling;
                Level.Sectors[i].Health3D = health.Sectors[i].ThreeD;
            }
            foreach (var group in health.Groups) HealthGroups[group.Key] = group.Value;
        }
        if (state.TextureScrolls is { } scrolls)
        {
            _textureScrolls.Clear(); _sideTextureScrolls.Clear();
            if (state.IncludesCeilingControls) _controlPlaneScrolls.RemoveAll(s => s.Plane == SectorTextureScrollPlane.Ceiling);
            if (state.IncludesFloorControls) _controlPlaneScrolls.RemoveAll(s => s.Plane == SectorTextureScrollPlane.Floor);
            if (state.IncludesWallParts) _controlWallScrolls.Clear();
            else if (state.IncludesWallControls) _controlWallScrolls.RemoveAll(s => s.Parts == WallScrollParts.All);
            if (state.IncludesCarryScrolls)
            {
                _sectorCarryScrolls.Clear(); _acceleratingCarryScrolls.Clear();
                _controlCarryScrolls.Clear(); _displacementCarryScrolls.Clear();
                Array.Clear(SectorScrollX); Array.Clear(SectorScrollY);
            }
            foreach (var scroll in scrolls)
                if (scroll.Kind < 2)
                    _textureScrolls.Add(new SectorTextureScroll(scroll.Target, scroll.Dx, scroll.Dy, (SectorTextureScrollPlane)scroll.Kind));
                else if (scroll.Kind <= 8)
                    _sideTextureScrolls.Add(new SideTextureScroll(scroll.Target, scroll.Dx, scroll.Dy, (WallScrollParts)(scroll.Kind - 1)));
                else if (scroll.Kind == 9)
                    _sectorCarryScrolls.Add(new SectorCarryScroll(scroll.Target, scroll.Dx, scroll.Dy, scroll.Affect));
                else if (scroll.Kind == 10)
                    _acceleratingCarryScrolls.Add(new AcceleratingSectorCarryScroll(scroll.Target, scroll.Dx, scroll.Dy, scroll.Affect)
                        { Vdx = scroll.Vdx, Vdy = scroll.Vdy });
                else if (scroll.Kind <= 12)
                    _controlCarryScrolls.Add(new ControlSectorCarryScroll(scroll.Target, scroll.Control, scroll.Dx, scroll.Dy,
                        scroll.LastHeight, scroll.Kind == 12) { Vdx = scroll.Vdx, Vdy = scroll.Vdy });
                else if (scroll.Kind <= 16)
                    _controlPlaneScrolls.Add(new ControlPlaneScroll(scroll.Target, scroll.Control, scroll.Dx, scroll.Dy,
                        scroll.LastHeight, scroll.Kind is 14 or 16,
                        scroll.Kind <= 14 ? SectorTextureScrollPlane.Ceiling : SectorTextureScrollPlane.Floor) { Vdx = scroll.Vdx, Vdy = scroll.Vdy });
                else
                    _controlWallScrolls.Add(new ControlWallScroll(scroll.Target, scroll.Control, scroll.Dx, scroll.Dy,
                        scroll.LastHeight, ControlWallScroll.AccelerationFromKind(scroll.Kind),
                        ControlWallScroll.PartsFromKind(scroll.Kind)) { Vdx = scroll.Vdx, Vdy = scroll.Vdy });
        }
        if (state.Planes is { } planes)
            for (var i = 0; i < planes.Count; i++) planes[i].Apply(Level.Sectors[i]);
        if (state.Walls is { } walls)
            for (var i = 0; i < walls.Count; i++) walls[i].Apply(Level.Sides[i]);
        Exited = state.Exited;
        SecretExit = state.SecretExit;
        if (state.CombatRandomState is { } randomState) CombatRandomState = randomState;
        if (state.IncludesIceChunkLifecycle)
        {
            var ids = state.Actors.Where(pose => pose.IceChunkLifecycle.HasValue).Select(pose => pose.Id).ToHashSet();
            foreach (var chunk in _actors.OfType<IceChunkActor>().Where(chunk => !ids.Contains(chunk.Id)).ToArray())
            { chunk.Destroy(); Thinkers.Remove(chunk); _actors.Remove(chunk); }
        }
        foreach (var pose in state.Actors.Where(pose => pose.IceChunkLifecycle.HasValue))
        {
            var existing = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var chunk = new IceChunkActor(10)
            {
                Id = pose.Id, Simulation = this, Level = Level,
                JustSpawned = (pose.IceChunkLifecycle.GetValueOrDefault() & 2) != 0,
            };
            _actors.Add(chunk); Thinkers.Add(chunk);
        }
        if (_actors.Count != 0) _nextActorId = Math.Max(_nextActorId, checked(_actors.Max(actor => actor.Id) + 1));
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor == null)
                continue;
            actor.X = new Fixed(pose.X);
            if (pose.IceChunkLifecycle is { } chunkLifecycle) actor.JustSpawned = (chunkLifecycle & 2) != 0;
            var actorSpecial = pose.ActorSpecial ?? default;
            actor.Special = actorSpecial.Special;
            actor.ActivationType = actorSpecial.ActivationType;
            actor.SpecialArgs[0] = actorSpecial.Arg0; actor.SpecialArgs[1] = actorSpecial.Arg1;
            actor.SpecialArgs[2] = actorSpecial.Arg2; actor.SpecialArgs[3] = actorSpecial.Arg3;
            actor.SpecialArgs[4] = actorSpecial.Arg4;
            actor.SpecialChanged = pose.ActorSpecial.HasValue;
            actor.Y = new Fixed(pose.Y);
            if (actor is IceChunkActor)
                actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            actor.Angle = new BamAngle(pose.Angle);
            actor.PitchDegrees = new Fixed(pose.Pitch).ToDouble();
            if (pose.PainDeath is { } painDeath && actor.Brain?.CapturePainDeath() != null) actor.Brain.RestorePainDeath(painDeath);
            actor.RestoreHealth(pose.Health);
            actor.DeathType = pose.DeathType;
            actor.SpecialFireDamage = pose.SpecialFireDamage;
            actor.FoilInvul = pose.FoilInvul;
            actor.PierceArmor = pose.PierceArmor;
            actor.NoInfightSpecies = pose.NoInfightSpecies;
            actor.DamageFactor = new Fixed(pose.DamageFactor);
            actor.DamageMultiplier = new Fixed(pose.DamageMultiplier);
            actor.RestoreDamageFactors(pose.DamageFactors);
            actor.StrifeDamage = pose.StrifeDamage;
            actor.Rip = pose.Rip;
            actor.DontRip = pose.DontRip;
            actor.NoBossRip = pose.NoBossRip;
            actor.Pushable = pose.Pushable;
            if (pose.IceCorpseFlag is { } iceCorpse) actor.IceCorpse = iceCorpse;
            actor.CannotPush = pose.CannotPush;
            actor.NoVerticalMeleeRange = pose.NoVerticalMeleeRange;
            actor.MeleeRange = pose.MeleeRangeRaw is { } meleeRange ? new Fixed(meleeRange) : Fixed.FromInt(44);
            actor.Damage = pose.ConstantDamage ?? actor.SpawnDamage.GetValueOrDefault();
            actor.JustHit = pose.JustHitFlag == 1;
            actor.HandleNoDelay = pose.HandleNoDelayFlag == 1;
            actor.Synchronized = pose.SynchronizedFlag == 1;
            actor.AlwaysFast = (pose.FastModeFlags.GetValueOrDefault() & 1) != 0;
            actor.NeverFast = (pose.FastModeFlags.GetValueOrDefault() & 2) != 0;
            actor.GoalId = pose.GoalPointer?.ActorId;
            actor.Killed = pose.Killed;
            actor.HasTargetMemoryOverride = pose.TargetMemory.HasValue;
            var memory = pose.TargetMemory ?? default;
            actor.Brain?.RestoreTargetMemory(memory);
            actor.LastHeardTargetId = memory.LastHeard;
            actor.CanSlide = pose.SlideFlag is { } slide ? slide != 0 : (actor.ResurrectionMovementFlags.GetValueOrDefault() & 8) != 0;
            actor.HasMovementActionOverride = pose.MovementActionFlags.HasValue;
            actor.Brain?.RestoreCharge(pose.SkullChargeFlag == 1);
            if (pose.ReactionTime is { } reactionTime)
                actor.ReactionTime = reactionTime;
            else if (actor.SpawnReactionTime is { } spawnReactionTime && actor.ReactionTime != spawnReactionTime)
                actor.ReactionTime = spawnReactionTime;
            if (pose.AllowDropOffFlag is { } allowDropOff)
                actor.AllowDropOff = allowDropOff != 0;
            else if (actor.ResurrectionMovementFlags is { } dropOffDefaults)
                actor.AllowDropOff = (dropOffDefaults & 16) != 0;
            actor.UseSpecial = pose.UseSpecialFlag == 1;
            actor.HasUseSpecialOverride = pose.UseSpecialFlag.HasValue;
            actor.MasterId = pose.MasterPointer?.ActorId;
            actor.HasMasterOverride = pose.MasterPointer.HasValue;
            actor.FloorClip = pose.FloorClip ?? 0;
            actor.HasFloorClipOverride = pose.FloorClip.HasValue;
            actor.Invisible = pose.InvisibleFlag == 1;
            actor.HasVisibilityOverride = pose.InvisibleFlag.HasValue;
            actor.NoBlockMonsters = pose.MonsterBlockingFlag is { } monsterBlocking
                ? monsterBlocking != 0 : (actor.ResurrectionCollisionFlags.GetValueOrDefault() & 8) != 0;
            if (pose.BlockmapFlag is { } blockmap)
            {
                actor.NoBlockmap = blockmap != 0;
                actor.HasBlockmapOverride = true;
            }
            else
            {
                if (actor.ResurrectionCollisionFlags is { } blockmapDefaults)
                    actor.NoBlockmap = (blockmapDefaults & 4) != 0;
                actor.HasBlockmapOverride = false;
            }
            if (pose.Friendship is { } friendship)
            {
                actor.FriendPlayer = friendship.FriendPlayer;
                actor.TidToHate = friendship.TidToHate;
                actor.Friendly = friendship.Friendly;
                actor.NoHatePlayers = friendship.NoHatePlayers;
            }
            else
            {
                actor.FriendPlayer = 0;
                actor.TidToHate = 0;
                actor.Friendly = actor.SpawnFriendly || (actor.ResurrectionDefenseFlags.GetValueOrDefault() & 8) != 0;
                actor.NoHatePlayers = false;
            }
            actor.HasFriendshipOverride = pose.Friendship.HasValue;
            actor.HasGravityOverride = pose.GravityRaw.HasValue;
            actor.HasDefensePropertyOverride = pose.DefenseProperties.HasValue;
            actor.HasSpriteOrientationOverride = pose.SpriteOrientation.HasValue;
            actor.SpriteAngle = pose.SpriteOrientation?.Angle ?? 0;
            actor.SpriteRotation = pose.SpriteOrientation?.Rotation ?? 0;
            actor.HasFloatBobPhaseOverride = pose.FloatBobPhase.HasValue;
            actor.FloatBobPhase = pose.FloatBobPhase ?? 0;
            actor.HasScaleOverride = pose.Scale.HasValue;
            actor.ScaleX = pose.Scale?.X ?? 1;
            actor.ScaleY = pose.Scale?.Y ?? 1;
            actor.HasTuningOverride = pose.Tuning.HasValue;
            actor.HasSizeOverride = pose.Size.HasValue;
            actor.HasSpeciesOverride = pose.SpeciesOverride.HasValue;
            actor.Species = pose.SpeciesOverride?.Name;
            if (pose.Size is { } size)
            {
                actor.Radius = new Fixed(size.RadiusRaw);
                actor.Height = new Fixed(size.HeightRaw);
                if (actor is PlayerPawn sizedPlayer) sizedPlayer.FullHeight = size.FullHeight;
            }
            else
            {
                if (actor.ResurrectionRadius is { } spawnRadius) actor.Radius = spawnRadius;
                if (actor.ResurrectionHeight is { } spawnHeight) actor.Height = spawnHeight;
            }
            if (actor is PlayerPawn crouchPlayer)
            {
                crouchPlayer.RestoreCrouch(pose.Crouch, pose.Size?.FullHeight);
                crouchPlayer.JumpTics = pose.JumpTics ?? 0;
                crouchPlayer.ClassicFlight = pose.ClassicFlightFlag == 1;
            }
            actor.Fly = pose.FlyFlag == 1;
            actor.NoFriction = pose.NoFrictionFlag == 1;
            actor.DontFall = pose.DontFallFlag is { } dontFallFlag ? dontFallFlag == 1 : actor.ClassDoomEdNum == 3006;
            actor.Falling = pose.FallingFlag == 1;
            actor.DontCorpse = pose.DontCorpseFlag == 1;
            actor.Corpse = pose.CorpseFlag is { } corpseFlag ? corpseFlag == 1 : actor.IsDead;
            actor.HasTeleFogOverride = pose.TeleFog.HasValue;
            actor.ActorSounds = new Dictionary<ActorSoundType, string>(pose.ActorSounds);
            actor.TeleFogSource = pose.TeleFog?.Source;
            actor.TeleFogDest = pose.TeleFog?.Destination;
            var tuning = pose.Tuning ?? actor.SpawnTuning ?? new SimActorTuning(Fixed.FromInt(4).Raw, 4, 0);
            actor.MovementSpeed = new Fixed(tuning.SpeedRaw);
            actor.FloatSpeed = tuning.FloatSpeed;
            actor.PainThreshold = tuning.PainThreshold;
            if (pose.DefenseProperties is { } defense)
            {
                actor.Mass = defense.Mass;
                actor.Shootable = (defense.Flags & 1) != 0;
                actor.Invulnerable = (defense.Flags & 2) != 0;
                actor.NonShootable = (defense.Flags & 4) != 0;
            }
            else
            {
                actor.Mass = actor.SpawnMass;
                if (actor.ResurrectionCollisionFlags is { } collisionDefaults)
                    actor.Shootable = (collisionDefaults & 2) != 0;
                if (actor.ResurrectionDefenseFlags is { } defenseDefaults)
                    actor.Invulnerable = (defenseDefaults & 1) != 0;
            }
            actor.Brain?.RestoreChaseThreshold(pose.ChaseThreshold);
            actor.Gravity = new Fixed(pose.GravityRaw ?? 65536);
            actor.Friction = new Fixed(pose.FrictionRaw ?? 65536);
            if (pose.MovementActionFlags is { } movementActionFlags)
            {
                actor.Solid = (movementActionFlags & 1) != 0;
                actor.Floating = (movementActionFlags & 2) != 0;
                actor.NoGravity = (movementActionFlags & 4) != 0;
            }
            else
            {
                if (actor.ResurrectionCollisionFlags is { } collisionDefaults)
                    actor.Solid = (collisionDefaults & 1) != 0;
                if (actor.ResurrectionMovementFlags is { } movementDefaults)
                {
                    actor.NoGravity = (movementDefaults & 1) != 0;
                    actor.Floating = (movementDefaults & 2) != 0;
                }
            }
            if (actor is not PlayerPawn) (pose.ActorArmor ?? default).Restore(actor);
            actor.PushFactor = pose.PushFactor;
            actor.RipperLevel = pose.RipperLevel;
            actor.RipLevelMin = pose.RipLevelMin;
            actor.RipLevelMax = pose.RipLevelMax;
            actor.ProjectilePassHeight = new Fixed(pose.ProjectilePassHeight);
            actor.InfightingGroup = pose.InfightingGroup;
            actor.ProjectileGroup = pose.ProjectileGroup;
            actor.SplashGroup = pose.SplashGroup;
            if (actor is PlayerPawn powerPlayer)
            {
                powerPlayer.PowerDamageTics = pose.PowerDamageTics;
                powerPlayer.PowerProtectionTics = pose.PowerProtectionTics;
                powerPlayer.PowerBuddhaTics = pose.PowerBuddhaTics;
            }
            actor.DamageType = pose.DamageType ?? (pose.DeathFlags == 1 ? "Massacre" : null);
            if (pose.DeathFlags is { } deathFlags) actor.DeathDamageType = deathFlags == 1 ? "Massacre" : null;
            if (pose.ProjectileFlags is { } projectileFlags) actor.NoExplodeFloor = (projectileFlags & 1) != 0;
            if (pose.CeilingFlags is { } ceilingFlags) actor.CeilingHugger = ceilingFlags == 1;
            if (pose.SpectralFlags is { } spectralFlags) actor.Spectral = spectralFlags == 1;
            if (pose.HitOwnerFlags is { } hitOwnerFlags) actor.HitOwner = hitOwnerFlags == 1;
            if (pose.NonShootableFlags is { } nonShootableFlags) actor.NonShootable = nonShootableFlags == 1;
            if (pose.GhostFlags is { } ghostFlags) { actor.Ghost = (ghostFlags & 1) != 0; actor.ThruGhost = (ghostFlags & 2) != 0; }
            if (pose.ThruBits is { } thruBits) { actor.ThruBits = thruBits.Mask; actor.AllowThruBits = thruBits.Enabled; }
            if (pose.ThruSpeciesFlags is { } thruSpeciesFlags) actor.ThruSpecies = thruSpeciesFlags == 1;
            if (pose.MissileThruSpeciesFlags is { } missileThruSpeciesFlags) actor.MThruSpecies = missileThruSpeciesFlags == 1;
            if (pose.ThruActorsFlags is { } thruActorsFlags) actor.ThruActors = thruActorsFlags == 1;
            if (pose.BlastEligibilityFlags is { } blastEligibilityFlags)
            {
                actor.Boss = (blastEligibilityFlags & 1) != 0;
                actor.DontBlast = (blastEligibilityFlags & 2) != 0;
            }
            if (pose.BlastedFlags is { } blastedFlags) actor.Blasted = blastedFlags == 1;
            if (pose.FloorHuggerFlags is { } floorHuggerFlags)
            {
                actor.FloorHugger = (floorHuggerFlags & 1) != 0;
                actor.NoDropOff = (floorHuggerFlags & 2) != 0;
            }
            if (pose.ProjectileLifetime is { } savedLifetime && actor is ProjectileActor savedProjectile)
                savedProjectile.RestoreRemainingTics(savedLifetime.Tics);
            if (pose.ProjectilePointers is { } savedPointers && actor is ProjectileActor pointerProjectile)
                pointerProjectile.RestoreTracerTarget(_actors.Any(a => a.Id == savedPointers.TracerTargetId && !a.Destroyed)
                    ? savedPointers.TracerTargetId : null);
            actor.Roll = new BamAngle(pose.Roll ?? 0);
            if (pose.ContactFlags is { } contactFlags)
            {
                actor.CanPickupItems = (contactFlags & 1) != 0;
                actor.SpecialPickup = (contactFlags & 2) != 0;
            }
            if (pose.FloatFlags is { } floatFlags)
            {
                actor.InFloat = (floatFlags & 1) != 0;
                actor.VerticalFriction = (floatFlags & 2) != 0;
            }
            if (pose.Pickup is { } pickup)
            {
                actor.PickupAmount = pickup.Amount;
                actor.IgnoreAmmoSkill = pickup.IgnoreSkill;
                actor.Depleted = pickup.Depleted;
            }
            actor.PickupDelay = pose.PickupDelay;
            actor.Dropped = pose.Dropped;
            actor.SuppressWeaponPickupAmmo = pose.SuppressWeaponPickupAmmo;
            actor.AlwaysPickupOverride = pose.AlwaysPickupOverride;
            actor.Z = new Fixed(pose.Z);
            actor.VelocityX = new Fixed(pose.VelocityX);
            actor.VelocityY = new Fixed(pose.VelocityY);
            actor.VelocityZ = new Fixed(pose.VelocityZ);
            actor.OnGround = pose.OnGround;
            actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            if (pose.HasPhysics) actor.States.Restore(actor, pose.State, pose.StateTics);
            else actor.States.Restore(actor, actor.IsDead ? actor.DeathState : actor.SpawnState, -1);
            actor.FullBright = pose.FullBrightFlag is { } brightness ? brightness == 1 : actor.States.CurrentFullBright;
            if (actor is PlayerPawn player)
            {
                player.ClearCommands(); player.AttackPressed = false; player.UsePressed = false;
                player.Inventory.ArmorType = pose.WornArmorType ?? "None";
                if (pose.WornArmorType is not null || pose.WornArmor.HasValue) player.Inventory.Armor = pose.WornArmorAmount;
                (pose.WornArmor ?? SimWornArmor.Default).Restore(player.Inventory);
                player.Inventory.RestoreSpareArmor(pose.SpareArmor);
                player.MaxHealth = pose.PlayerMaxHealth ?? 0;
                player.Stamina = pose.ActorSpecial?.Stamina ?? 0;
                player.BonusHealth = pose.ActorSpecial?.BonusHealth ?? 0;
                player.MaxPickupHealth = pose.ActorSpecial?.MaxPickupHealth ?? 0;
                player.WeaponCooldown = pose.WeaponCooldown;
                player.UseHeld = pose.UseHeld;
                if (pose.UseRange is { } useRange) player.UseRange = useRange;
                player.BobTimer = state.Tic;
                player.ViewBobOffset = 0;
                player.MovementBob = 0;
                player.WeaponBobX = 0;
                player.WeaponBobY = 0;
                if (!player.IsDead)
                    player.UpdateViewBob();
            }
            actor.RememberPosition();
        }

        var count = Math.Min(Floors.Length, state.Sectors.Count);
        for (var i = 0; i < count; i++)
        {
            Floors[i] = Fixed.FromDouble(state.Sectors[i].Floor).ToDouble();
            Ceilings[i] = Fixed.FromDouble(state.Sectors[i].Ceiling).ToDouble();
        }
        foreach (var pose in state.Actors.Where(pose => !pose.HasPhysics))
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor != null) ActorPhysics.PlaceOnFloor(this, actor);
        }

        // Support depends on other actors and restored sector planes, so derive it after both are loaded.
        foreach (var actor in _actors) ActorPhysics.RefreshOnMobj(this, actor);

        RecomputeChecksum();
        PublishStatus();
    }

    public bool QueueCommand(byte playerNum, PlayerCommand command)
    {
        foreach (var player in Players)
        {
            if (player.PlayerNum == playerNum)
            {
                return player.TryQueueCommand(command);
            }
        }
        return false;
    }

    /// <summary>Native <c>Level-&gt;PlayerInGame</c> for ACS. Server slot remapping is absent.</summary>
    internal bool IsPlayerInGame(int playerNum) =>
        Players.Any(player => !player.Destroyed && player.PlayerNum == playerNum);

    /// <summary>Native <c>Level-&gt;Players[n]-&gt;Bot</c> for ACS. Player-slot bots are absent.</summary>
    internal bool IsPlayerBot(int playerNum) => false;

    internal void StartPlayerScripts(bool death, PlayerPawn player)
    {
        foreach (var number in death ? _deathScripts : _respawnScripts)
        {
            if (!Acs.ExecuteAlways(number, [0], player))
                throw new InvalidDataException(death ? "acs-death-script-start-failed" : "acs-respawn-script-start-failed");
        }
    }

    /// <summary>
    /// player.zs DeathThink, then g_game.cpp G_DoReborn for the supported modes.
    /// The wait is TICRATE (35). A button held across the killing tic does not arm.
    /// <see cref="NoRespawn"/> keeps the corpse and the buffered press.
    /// Single-player requests a reload and leaves the corpse. Coop and deathmatch revive
    /// at that player's start, or player 1's start if theirs is missing. Deathmatch
    /// inventory returns to the pistol start. Cooperative and single-player respawn
    /// run the coop inventory filter, which keeps the pack while every lose flag is off.
    /// A deathmatch revive uses a thing-11 start when the map has one. The weapon offset
    /// and the view are snapped ready. The revive telefrags a monster standing on that
    /// spot, and in deathmatch it telefrags another player there too. <see cref="WeaponDrop"/>
    /// spawns the selected weapon's map thing. Level load still spawns players on player
    /// starts and does not stomp.
    /// </summary>
    internal void TryPlayerRespawn(PlayerPawn player)
    {
        if (player.Destroyed || !player.IsDead || ReloadRequested) return;
        var forced = ForceRespawn && GameMode == SpawnGameMode.Deathmatch;
        if (!player.RespawnArmed && !forced) return;
        if (Thinkers.Clock.Tic < player.RespawnEarliestTic) return;
        if (NoRespawn) return;
        player.DisarmRespawn();
        if (GameMode == SpawnGameMode.Single && !AllowSinglePlayerRespawn)
        {
            ReloadRequested = true;
            return;
        }

        var start = GameMode == SpawnGameMode.Deathmatch
            ? PickDeathmatchRespawn(player) ?? PlayerStart(player)
            : PlayerStart(player);
        if (start != null)
        {
            player.X = Fixed.FromDouble(start.X);
            player.Y = Fixed.FromDouble(start.Y);
            player.Angle = BamAngle.FromDegrees(start.Angle);
        }
        player.VelocityX = default;
        player.VelocityY = default;
        player.VelocityZ = default;
        player.CanPickupItems = player.SpawnCanPickupItems;
        player.SpecialPickup = player.SpawnSpecialPickup;
        player.MaxHealth = 0;
        player.Stamina = 0;
        player.BonusHealth = 0;
        player.MaxPickupHealth = 0;
        player.Health = player.ResurrectionHealth > 0 ? player.ResurrectionHealth : 100;
        ActorPhysics.PlaceOnFloor(this, player);
        player.ClearCommands();
        player.AttackPressed = false;
        player.UsePressed = false;
        if (GameMode == SpawnGameMode.Deathmatch)
        {
            player.Inventory.ResetToPistolStart();
            player.PowerBuddhaTics = 0;
        }
        else
        {
            player.Inventory.FilterCoopRespawn(CoopLoseInventory, CoopLoseKeys && !CoopShareKeys, CoopLoseWeapons, CoopLoseArmor, CoopLoseAmmo, CoopHalveAmmo);
            if (CoopLoseInventory)
                player.PowerBuddhaTics = 0;
        }
        player.Inventory.Pending = null;
        player.TurnTicks = 0;
        player.JumpTics = 0;
        player.ClearTurnHeld();
        player.WeaponOffsetY = PlayerPawn.WeaponTop;
        player.WeaponLowering = false;
        player.ViewHeight = player.DefaultViewHeight.ToDouble();
        player.PitchDegrees = 0;
        StompSpawn(player);
        StartPlayerScripts(death: false, player);
    }

    /// <summary>
    /// <c>P_PlayerStartStomp</c> after a revive. Monsters on the spot are telefragged.
    /// Other players are telefragged only in deathmatch. The overlap is a square of the
    /// two radii, and the bodies must meet in Z. Monsters use the native
    /// <see cref="Actor.IsMonster"/> classification. Level load does not stomp.
    /// </summary>
    private void StompSpawn(PlayerPawn player)
    {
        var monstersOnly = GameMode != SpawnGameMode.Deathmatch;
        var px = player.X.ToDouble();
        var py = player.Y.ToDouble();
        var pz = player.Z.ToDouble();
        var pTop = pz + player.Height.ToDouble();
        foreach (var other in _actors)
        {
            if (other == player || !other.IsBlockmapActor || !other.Shootable || other.IsDead || other.Destroyed)
                continue;
            if (other is not PlayerPawn && !other.IsMonster)
                continue;
            if (other is PlayerPawn && monstersOnly)
                continue;
            if (other.NoTelefrag && !other.AlwaysTelefrag)
                continue;
            var block = other.Radius.ToDouble() + player.Radius.ToDouble();
            if (Math.Abs(other.X.ToDouble() - px) >= block || Math.Abs(other.Y.ToDouble() - py) >= block)
                continue;
            var otherTop = other.Z.ToDouble() + other.Height.ToDouble();
            if (pz > otherTop || pTop < other.Z.ToDouble())
                continue;
            ActorDamage.Apply(other, ActorDamage.TelefragDamage, player, inflictor: player);
        }
    }

    private LevelThing? PlayerStart(PlayerPawn player) =>
        Level.Things.FirstOrDefault(thing => thing.Type == player.PlayerNum + 1)
        ?? Level.Things.FirstOrDefault(thing => Actor.IsPlayerStart(thing.Type));

    private LevelThing? PickDeathmatchRespawn(PlayerPawn player)
    {
        var starts = Level.Things.Where(thing => thing.Type == DeathmatchStarts.ThingType).ToList();
        if (starts.Count == 0)
            return null;
        var living = _actors.OfType<PlayerPawn>()
            .Where(other => other != player && !other.IsDead && !other.Destroyed)
            .Select(other => (other.X.ToDouble(), other.Y.ToDouble()))
            .ToList();
        if (SpawnFarthest && living.Count > 0)
        {
            var farthest = DeathmatchStarts.PickFarthest(starts, living);
            if (farthest != null)
                return farthest;
        }
        return DeathmatchStarts.PickRandom(starts, count => (int)(NextDmSpawnRandom() % (uint)count), spot => SpotOpen(player, spot));
    }

    private bool SpotOpen(PlayerPawn player, LevelThing spot)
    {
        var reachBase = player.Radius.ToDouble();
        foreach (var other in _actors)
        {
            if (other == player || other.IsDead || other.Destroyed || !other.Solid)
                continue;
            var dx = spot.X - other.X.ToDouble();
            var dy = spot.Y - other.Y.ToDouble();
            var reach = reachBase + other.Radius.ToDouble();
            if (dx * dx + dy * dy <= reach * reach)
                return false;
        }
        return true;
    }

    public void Tick()
    {
        // Native ACS observes Level->time before the end-of-tic increment.
        var levelTic = Thinkers.Clock.Tic;
        if (LightEffects.Count != 0) HasLightAnimationOverride = true;
        LightEffects.RemoveAll(effect => effect.Tick(this));
        Thinkers.Run();
        RebuildSectorCarryScrolls();
        ApplySectorTextureScrolls();
        ApplySideTextureScrolls();
        MarkInScrollSectorActors();
        foreach (var actor in _actors)
            ActorPhysics.ApplySectorScroll(this, actor);
        _actors.RemoveAll(actor => actor.Destroyed);
        CollectPickups();
        ResolveAttacks();
        LineSpecials.ActivateCrossings(this);
        LineSpecials.ActivateUses(this);
        LineSpecials.TickMotions(this);
        // Carriers before riders so <see cref="ActorPhysics.SupportFloor"/> sees updated mobj tops.
        foreach (var actor in _actors.Where(actor => actor is not ProjectileActor)
            .OrderBy(actor => actor.OnMobj ? 1 : 0).ThenBy(actor => actor.Id))
            ActorPhysics.FitToSector(this, actor, sectorPinchCrush: true);
        Acs.Tick(this, levelTic);
        Invasion.Tick(this);
        if (RewindEnabled)
            Rewind.Capture(this);
        RecomputeChecksum();
        PublishStatus();
    }

    private void ResolveAttacks()
    {
        foreach (var player in Players.ToArray())
        {
            if (!player.AttackPressed)
                continue;
            player.AttackPressed = false;
            HitscanCombat.Fire(this, player);
        }
    }

    private void CollectPickups()
    {
        var taken = new List<Actor>();
        foreach (var player in Players)
        {
            if (player.IsDead || !player.CanPickupItems)
                continue;
            foreach (var actor in _actors)
            {
                if (taken.Contains(actor) || !actor.SpecialPickup || !PickupCatalog.IsPickup(actor.DoomEdNum))
                    continue;
                if (!PickupCatalog.IsWithinHorizontalReach(player, actor) || !PickupCatalog.IsWithinVerticalReach(player, actor))
                    continue;
                var key = PickupCatalog.IsKey(actor.DoomEdNum);
                var weapon = PickupCatalog.PickupWeapon(actor.DoomEdNum);
                var shouldStay = key && GameMode != SpawnGameMode.Single
                    || weapon != 0 && NonDroppedWeaponsStay && !actor.Dropped;
                if (shouldStay && weapon != 0 && player.Inventory.Owns(weapon)) continue;
                if (!PickupCatalog.TryGive(player, actor.DoomEdNum, actor.IgnoreAmmoSkill, actor.Depleted, actor.PickupAmount,
                    actor.SuppressWeaponPickupAmmo)
                    && !(key && GameMode == SpawnGameMode.Single)
                    && (shouldStay || !(actor.AlwaysPickupOverride ?? PickupCatalog.AlwaysPickup(actor.DoomEdNum))))
                    continue;
                if (!shouldStay) taken.Add(actor);
                if (GameMode == SpawnGameMode.Cooperative && CoopShareKeys && PickupCatalog.IsKey(actor.DoomEdNum))
                {
                    foreach (var other in Players)
                    {
                        if (other != player)
                            PickupCatalog.TryGive(other, actor.DoomEdNum);
                    }
                }
            }
        }

        foreach (var actor in taken)
        {
            actor.Destroy();
            _actors.Remove(actor);
        }
    }

    private static bool CirclesOverlap(Actor left, Actor right)
        => HorizontalCirclesOverlap(left, right)
            && left.Z.ToDouble() < right.Z.ToDouble() + right.Height.ToDouble()
            && right.Z.ToDouble() < left.Z.ToDouble() + left.Height.ToDouble();

    private static bool HorizontalCirclesOverlap(Actor left, Actor right)
    {
        var dx = left.X.ToDouble() - right.X.ToDouble();
        var dy = left.Y.ToDouble() - right.Y.ToDouble();
        var reach = left.Radius.ToDouble() + right.Radius.ToDouble();
        return dx * dx + dy * dy <= reach * reach;
    }

    private void PublishStatus() => StatusLine = Describe();

    private SimGeometryHealth CaptureGeometryHealth()
    {
        var health = new SimGeometryHealth();
        health.Lines.AddRange(Level.Lines.Select(line => line.Health));
        health.Sectors.AddRange(Level.Sectors.Select(sector => new SimSectorHealth(sector.HealthFloor, sector.HealthCeiling, sector.Health3D)));
        foreach (var group in HealthGroups) health.Groups.Add(group.Key, group.Value);
        return health;
    }

    private void RecomputeChecksum()
    {
        var hash = 2166136261u;
        hash = DamageTypes.MixChecksum(hash);
        if (_levelGravityOverride.HasValue || LevelGravity != 800)
        {
            hash = Mix(hash, 0x4c475241u); hash = MixDouble(hash, LevelGravity);
        }
        if (AirControl is { } airControl)
        {
            hash = Mix(hash, 0x41495243u);
            hash = MixDouble(hash, airControl);
        }
        if (Level.TerrainSplashes.Count != 0)
        {
            hash = Mix(hash, 0x5453504cu);
            foreach (var splash in Level.TerrainSplashes)
            {
                hash = Mix(hash, (uint)splash.Length);
                foreach (var character in splash) hash = Mix(hash, char.ToUpperInvariant(character));
            }
        }
        if (Level.FloorTerrainMappings.Count != 0 || Level.DefaultTerrain.Length != 0)
        {
            hash = Mix(hash, 0x5445524du);
            hash = Mix(hash, (uint)Level.DefaultTerrain.Length);
            foreach (var character in Level.DefaultTerrain) hash = Mix(hash, char.ToUpperInvariant(character));
            foreach (var floor in Level.FloorTerrainMappings)
            {
                hash = Mix(hash, (uint)floor.Texture.Length);
                foreach (var character in floor.Texture) hash = Mix(hash, char.ToUpperInvariant(character));
                hash = Mix(hash, (uint)floor.Terrain.Length);
                foreach (var character in floor.Terrain) hash = Mix(hash, char.ToUpperInvariant(character));
            }
        }
        if (Level.TerrainDefinitions.Count != 0)
        {
            hash = Mix(hash, 0x54455244u);
            foreach (var terrain in Level.TerrainDefinitions)
            {
                hash = Mix(hash, (uint)terrain.Name.Length);
                foreach (var character in terrain.Name) hash = Mix(hash, char.ToUpperInvariant(character));
                hash = Mix(hash, (uint)terrain.DamageType.Length);
                foreach (var character in terrain.DamageType) hash = Mix(hash, char.ToUpperInvariant(character));
                hash = Mix(hash, unchecked((uint)terrain.DamageAmount));
                hash = Mix(hash, unchecked((uint)terrain.DamageTimeMask));
                hash = Mix(hash, terrain.DamageOnLand ? 1u : 0u);
                hash = MixDouble(hash, terrain.Friction);
                hash = MixDouble(hash, terrain.MoveFactor);
                hash = Mix(hash, (uint)terrain.Splash.Length);
                foreach (var character in terrain.Splash) hash = Mix(hash, char.ToUpperInvariant(character));
            }
        }
        if (Level.HexenHack) hash = Mix(hash, 0x48455848u);
        if (Level.ActivateOwnDeathSpecials) hash = Mix(hash, 0x414F4453u);
        hash = Mix(hash, unchecked((uint)Compat));
        if (DehackedNoAutofreeze) hash = Mix(hash, 0x44454E46u);
        if (DehackedInitialBullets != 50)
        {
            hash = Mix(hash, 0x44454942u);
            hash = Mix(hash, unchecked((uint)DehackedInitialBullets));
        }
        if (DehackedGreenArmorClass != 1 || DehackedBlueArmorClass != 2)
        {
            hash = Mix(hash, 0x44454143u);
            hash = Mix(hash, unchecked((uint)DehackedGreenArmorClass));
            hash = Mix(hash, unchecked((uint)DehackedBlueArmorClass));
        }
        if (DehackedMaxArmor != 200)
        {
            hash = Mix(hash, 0x44454841u);
            hash = Mix(hash, unchecked((uint)DehackedMaxArmor));
        }
        if (DehackedHealthBonusCapPatched) hash = Mix(hash, 0x44454842u);
        if (DehackedMaxSoulsphere != 200 || DehackedSoulsphereHealth != 100 || DehackedMegasphereHealth != 200)
        {
            hash = Mix(hash, 0x44454853u);
            hash = Mix(hash, unchecked((uint)DehackedMaxSoulsphere));
            hash = Mix(hash, unchecked((uint)DehackedSoulsphereHealth));
            hash = Mix(hash, unchecked((uint)DehackedMegasphereHealth));
        }
        if (DehackedMaxHealth != 100)
        {
            hash = Mix(hash, 0x44454848u);
            hash = Mix(hash, unchecked((uint)DehackedMaxHealth));
        }
        hash = Mix(hash, DamageExitAllowed ? 1u : 0u);
        hash = Mix(hash, (uint)GameMode);
        hash = Mix(hash, ReloadRequested ? 1u : 0u);
        hash = Mix(hash, AllowSinglePlayerRespawn ? 1u : 0u);
        hash = Mix(hash, ForceRespawn ? 1u : 0u);
        hash = Mix(hash, NoRespawn ? 1u : 0u);
        hash = Mix(hash, WeaponDrop ? 1u : 0u);
        if (WeaponStay) hash = Mix(hash, 0x57535459u);
        if (NoExtraAmmo) hash = Mix(hash, 0x4e455841u);
        if (DropAmmoFactor != -1)
        {
            var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(DropAmmoFactor));
            hash = Mix(hash, 0x44414d46u);
            hash = Mix(hash, (uint)bits);
            hash = Mix(hash, (uint)(bits >> 32));
        }
        if (AlwaysApplyDmFlags) hash = Mix(hash, 0x41444d46u);
        hash = Mix(hash, CoopLoseInventory ? 1u : 0u);
        hash = Mix(hash, CoopLoseKeys ? 1u : 0u);
        hash = Mix(hash, CoopShareKeys ? 1u : 0u);
        hash = Mix(hash, CoopLoseWeapons ? 1u : 0u);
        hash = Mix(hash, CoopLoseArmor ? 1u : 0u);
        hash = Mix(hash, CoopLoseAmmo ? 1u : 0u);
        hash = Mix(hash, CoopHalveAmmo ? 1u : 0u);
        hash = Mix(hash, SpawnFarthest ? 1u : 0u);
        hash = Mix(hash, DmSpawnRandomState);
        hash = Mix(hash, (uint)Skill);
        if (FastMonsters != (Skill == 4) || SlowMonsters)
        { hash = Mix(hash, 0x534b544du); hash = Mix(hash, (FastMonsters ? 1u : 0u) | (SlowMonsters ? 2u : 0u)); }
        if (HealthFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(HealthFactor));
            hash = Mix(hash, 0x48464143u);
            hash = Mix(hash, (uint)factorBits);
            hash = Mix(hash, (uint)(factorBits >> 32));
        }
        if (ArmorFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(ArmorFactor));
            hash = Mix(hash, 0x41524643u);
            hash = Mix(hash, (uint)factorBits);
            hash = Mix(hash, (uint)(factorBits >> 32));
        }
        hash = Mix(hash, unchecked((uint)Fixed.FromDouble(AmmoFactor).Raw));
        hash = Mix(hash, DoubleAmmo ? 1u : 0u);
        if (DropStyle != 0) hash = Mix(hash, unchecked((uint)DropStyle));
        if (DefaultDropStyle != 1) { hash = Mix(hash, 0x47445354u); hash = Mix(hash, unchecked((uint)DefaultDropStyle)); }
        hash = Mix(hash, unchecked((uint)Infighting));
        hash = Mix(hash, unchecked((uint)Thinkers.Clock.Tic));
        hash = Mix(hash, unchecked((uint)RngSeed));
        hash = Mix(hash, CombatRandomState);
        hash = Mix(hash, SwitchTargetRandomState);
        if (_hasJumpRandomState) { hash = Mix(hash, 0x43414a55u); hash = Mix(hash, _jumpRandomState); }
        if (_hasStateRandomState)
        { hash = Mix(hash, 0x53544943u); hash = Mix(hash, (uint)_stateRandomState); hash = Mix(hash, (uint)(_stateRandomState >> 32)); }
        if (_hasMapSpawnRandomState)
        { hash = Mix(hash, 0x4d415052u); hash = Mix(hash, (uint)_mapSpawnRandomState); hash = Mix(hash, (uint)(_mapSpawnRandomState >> 32)); }
        if (_hasIceTicsRandomState)
        { hash = Mix(hash, 0x49434554u); hash = Mix(hash, (uint)_iceTicsRandomState); hash = Mix(hash, (uint)(_iceTicsRandomState >> 32)); }
        if (_hasFreezeDeathRandomState)
        { hash = Mix(hash, 0x46524454u); hash = Mix(hash, (uint)_freezeDeathRandomState); hash = Mix(hash, (uint)(_freezeDeathRandomState >> 32)); }
        if (_hasFreezeChunksRandomState)
        { hash = Mix(hash, 0x46525a43u); hash = Mix(hash, (uint)_freezeChunksRandomState); hash = Mix(hash, (uint)(_freezeChunksRandomState >> 32)); }
        if (_hasDropItemRandomState)
        { hash = Mix(hash, 0x44524f50u); hash = Mix(hash, (uint)_dropItemRandomState); hash = Mix(hash, (uint)(_dropItemRandomState >> 32)); }
        hash = Mix(hash, _strobeRandomState);
        hash = Mix(hash, _flickerRandomState);
        hash = Mix(hash, _lightFlashRandomState);
        hash = Mix(hash, _fireFlickerRandomState);
        hash = Mix(hash, Acs.Checksum);
        if (HealthGroups.Count != 0 || Level.Lines.Any(line => line.Health != 0)
            || Level.Sectors.Any(sector => sector.HealthFloor != 0 || sector.HealthCeiling != 0 || sector.Health3D != 0))
        {
            hash = Mix(hash, 0x4845414cu);
            foreach (var line in Level.Lines) hash = Mix(hash, unchecked((uint)line.Health));
            foreach (var sector in Level.Sectors)
            {
                hash = Mix(hash, unchecked((uint)sector.HealthFloor));
                hash = Mix(hash, unchecked((uint)sector.HealthCeiling));
                hash = Mix(hash, unchecked((uint)sector.Health3D));
            }
            foreach (var group in HealthGroups.OrderBy(group => group.Key))
            {
                hash = Mix(hash, unchecked((uint)group.Key));
                hash = Mix(hash, unchecked((uint)group.Value));
            }
        }
        foreach (var actor in _actors.OrderBy(actor => actor.Id))
        {
            hash = Mix(hash, actor.Id);
            if (actor.ActivationType != 0) { hash = Mix(hash, 0x41435454u); hash = Mix(hash, unchecked((uint)actor.ActivationType)); }
            if (actor.Special != 0 || actor.SpecialArgs.Any(arg => arg != 0))
            {
                hash = Mix(hash, 0x41535043u);
                hash = Mix(hash, unchecked((uint)actor.Special));
                foreach (var arg in actor.SpecialArgs) hash = Mix(hash, unchecked((uint)arg));
            }
            hash = Mix(hash, unchecked((uint)actor.ThingId));
            hash = Mix(hash, unchecked((uint)actor.X.Raw));
            hash = Mix(hash, unchecked((uint)actor.Y.Raw));
            hash = Mix(hash, unchecked((uint)actor.Z.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityX.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityY.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityZ.Raw));
            hash = Mix(hash, actor.Angle.Raw);
            if (actor.FloatBobPhase != 0)
            {
                hash = Mix(hash, 0x424f4250u);
                hash = Mix(hash, unchecked((uint)actor.FloatBobPhase));
            }
            if (actor.SpriteAngle != 0 || actor.SpriteRotation != 0)
            {
                hash = Mix(hash, 0x53505254u);
                hash = MixDouble(hash, actor.SpriteAngle); hash = MixDouble(hash, actor.SpriteRotation);
            }
            if (actor.ScaleX != 1 || actor.ScaleY != 1)
            {
                hash = Mix(hash, 0x5343414cu);
                hash = MixDouble(hash, actor.ScaleX); hash = MixDouble(hash, actor.ScaleY);
            }
            if (actor.Roll.Raw != 0)
            {
                hash = Mix(hash, 0x524f4c4cu);
                hash = Mix(hash, actor.Roll.Raw);
            }
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.PitchDegrees).Raw));
            hash = Mix(hash, unchecked((uint)actor.States.Current));
            hash = Mix(hash, unchecked((uint)actor.States.RemainingTics));
            hash = Mix(hash, unchecked((uint)actor.Health));
            if (actor.ClassDoomEdNum != actor.DoomEdNum)
            {
                hash = Mix(hash, 0x434C4153u);
                hash = Mix(hash, unchecked((uint)actor.ClassDoomEdNum));
            }
            hash = Mix(hash, unchecked((uint)actor.PainChance));
            hash = Mix(hash, unchecked((uint)actor.PainThreshold));
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.ChaseSpeed).Raw));
            hash = Mix(hash, (uint)actor.Radius.Raw);
            hash = Mix(hash, (uint)actor.Height.Raw);
            if (actor.ResurrectionRadius is { } resurrectionRadius && resurrectionRadius != actor.Radius)
            {
                hash = Mix(hash, 0x52535244u);
                hash = Mix(hash, unchecked((uint)resurrectionRadius.Raw));
            }
            if (actor.ResurrectionHeight is { } resurrectionHeight && resurrectionHeight != actor.Height)
            {
                hash = Mix(hash, 0x52534854u);
                hash = Mix(hash, unchecked((uint)resurrectionHeight.Raw));
            }
            if (actor.DontFall != (actor.ClassDoomEdNum == 3006)) hash = Mix(hash, actor.DontFall ? 0x4446414Cu : 0x4446414Du);
            if (actor.Falling) hash = Mix(hash, 0x46414C4Cu);
            if (actor.DontCorpse) hash = Mix(hash, 0x44434F52u);
            if (actor.Corpse != actor.IsDead) hash = Mix(hash, actor.Corpse ? 0x434F5251u : 0x434F5250u);
            if (actor.NoFriction) hash = Mix(hash, 0x4E4F4652u);
            if (actor.Fly) hash = Mix(hash, 0x464C595Fu);
            hash = Mix(hash, actor.NoGravity ? 1u : 0u);
            if (actor.ResurrectionMovementFlags is { } movementFlags
                && movementFlags != ActorSpawner.MovementFlagsOf(actor))
            {
                hash = Mix(hash, 0x52534D46u);
                hash = Mix(hash, unchecked((uint)movementFlags));
            }
            if (actor.ResurrectionDefenseFlags is { } defenseFlags
                && defenseFlags != ActorSpawner.DefenseFlagsOf(actor))
            {
                hash = Mix(hash, 0x52534446u);
                hash = Mix(hash, unchecked((uint)defenseFlags));
            }
            if (actor.ResurrectionCollisionFlags is { } collisionFlags
                && collisionFlags != ActorSpawner.CollisionFlagsOf(actor))
            {
                hash = Mix(hash, 0x52534346u);
                hash = Mix(hash, unchecked((uint)collisionFlags));
            }
            hash = Mix(hash, actor.LastHeardTargetId ?? 0);
            hash = Mix(hash, actor.Floating ? 1u : 0u);
            hash = Mix(hash, actor.AllowDropOff ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.MaxDropOffHeight.Raw));
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.FloatSpeed).Raw));
            hash = Mix(hash, (uint)actor.ResurrectionHealth);
            hash = Mix(hash, unchecked((uint)actor.GibHealth));
            hash = Mix(hash, unchecked((uint)actor.SeeState));
            if (actor.RaiseState >= 0) { hash = Mix(hash, 0x52414953u); hash = Mix(hash, (uint)actor.RaiseState); }
            hash = Mix(hash, unchecked((uint)actor.ExtremeDeathState));
            hash = Mix(hash, unchecked((uint)actor.GenericFreezeDeath));
            hash = Mix(hash, actor.NoIceDeath ? 1u : 0u);
            if (actor.SpecialFireDamage) hash = Mix(hash, 0x53464952u);
            if (actor.FoilInvul) hash = Mix(hash, 0x46494E56u);
            if (actor.PierceArmor) hash = Mix(hash, 0x5041524Du);
            if (actor.StrifeDamage) hash = Mix(hash, 0x5354444Du);
            if (actor.Rip) hash = Mix(hash, 0x52495050u);
            if (actor.DontRip) hash = Mix(hash, 0x444E5250u);
            if (actor.NoBossRip) hash = Mix(hash, 0x4E425250u);
            if (actor.ProjectilePassHeight.Raw != 0)
            { hash = Mix(hash, 0x50504854u); hash = Mix(hash, unchecked((uint)actor.ProjectilePassHeight.Raw)); }
            if (actor.Pushable) hash = Mix(hash, 0x50555348u);
            if (actor.CannotPush) hash = Mix(hash, 0x4E505553u);
            if (actor.PushFactor != 0.25) hash = DamageRuleChecksum.Entry(hash, "PushFactor", actor.PushFactor);
            if (actor.RipperLevel != 0 || actor.RipLevelMin != 0 || actor.RipLevelMax != 0)
            {
                hash = Mix(hash, 0x524C564Cu);
                hash = Mix(hash, unchecked((uint)actor.RipperLevel));
                hash = Mix(hash, unchecked((uint)actor.RipLevelMin));
                hash = Mix(hash, unchecked((uint)actor.RipLevelMax));
            }
            if (!string.IsNullOrEmpty(actor.Species) && !string.Equals(actor.Species, "None", StringComparison.OrdinalIgnoreCase))
            {
                hash = Mix(hash, 0x53504543u);
                hash = Mix(hash, (uint)actor.Species.Length);
                foreach (var character in actor.Species) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            if (!string.IsNullOrEmpty(actor.DamageType) && !string.Equals(actor.DamageType, "None", StringComparison.OrdinalIgnoreCase))
            {
                hash = Mix(hash, 0x44545941u);
                hash = Mix(hash, (uint)actor.DamageType.Length);
                foreach (var character in actor.DamageType) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            if (!string.IsNullOrEmpty(actor.DeathType) && !string.Equals(actor.DeathType, "None", StringComparison.OrdinalIgnoreCase))
            {
                hash = Mix(hash, 0x44545950u);
                hash = Mix(hash, (uint)actor.DeathType.Length);
                foreach (var character in actor.DeathType) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            foreach (var typed in actor.TypedDeaths())
            {
                hash = Mix(hash, typed.Extreme ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)typed.State));
                foreach (var character in typed.Type)
                    hash = Mix(hash, character);
                hash = Mix(hash, 0);
            }
            foreach (var pain in actor.TypedPains())
            {
                hash = Mix(hash, unchecked((uint)pain.State));
                hash = Mix(hash, unchecked((uint)(pain.Chance ?? -1)));
                foreach (var character in pain.Type)
                    hash = Mix(hash, character);
                hash = Mix(hash, 0);
            }
            hash = Mix(hash, unchecked((uint)actor.WoundHealth));
            foreach (var wound in actor.TypedWounds())
            {
                hash = Mix(hash, unchecked((uint)wound.State));
                foreach (var character in wound.Type)
                    hash = Mix(hash, character);
                hash = Mix(hash, 0);
            }
            hash = Mix(hash, unchecked((uint)actor.Armor));
            hash = Mix(hash, unchecked((uint)actor.ArmorSavePercent));
            hash = Mix(hash, unchecked((uint)actor.MaxAbsorb));
            hash = Mix(hash, unchecked((uint)actor.MaxFullAbsorb));
            hash = Mix(hash, unchecked((uint)actor.AbsorbCount));
            hash = Mix(hash, actor.Buddha ? 1u : 0u);
            hash = Mix(hash, actor.FoilBuddha ? 1u : 0u);
            hash = Mix(hash, actor.ForcePain ? 1u : 0u);
            hash = Mix(hash, actor.NoPain ? 1u : 0u);
            hash = Mix(hash, actor.Painless ? 1u : 0u);
            hash = Mix(hash, actor.ExtremeDeath ? 1u : 0u);
            hash = Mix(hash, actor.NoExtremeDeath ? 1u : 0u);
            hash = Mix(hash, actor.FullBright ? 1u : 0u);
            if (actor.HandleNoDelay) hash = Mix(hash, 0x4e444c59u);
            if (actor.Synchronized) hash = Mix(hash, 0x53594e43u);
            if (actor.AlwaysFast || actor.NeverFast)
            { hash = Mix(hash, 0x46415354u); hash = Mix(hash, (actor.AlwaysFast ? 1u : 0u) | (actor.NeverFast ? 2u : 0u)); }
            hash = Mix(hash, actor.JustHit ? 1u : 0u);
            hash = Mix(hash, actor.ActsLikeBridge ? 1u : 0u);
            hash = Mix(hash, actor.IceCorpse ? 1u : 0u);
            hash = Mix(hash, actor.Shattering ? 1u : 0u);
            hash = Mix(hash, actor.IceShatter ? 1u : 0u);
            hash = Mix(hash, actor.NeverTarget ? 1u : 0u);
            if (actor.NoAutoOffSkullFly) hash = Mix(hash, 0x534B554Cu);
            if (actor.NoExplodeFloor) hash = Mix(hash, 0x4E45464Cu);
            if (actor.CeilingHugger) hash = Mix(hash, 0x43485547u);
            if (actor.FloorHugger) hash = Mix(hash, 0x46485547u);
            if (actor.NoDropOff) hash = Mix(hash, 0x4E44524Fu);
            if (actor.Blasted) hash = Mix(hash, 0x424C5354u);
            if (actor.Boss) hash = Mix(hash, 0x424F5353u);
            if (actor.DontBlast) hash = Mix(hash, 0x44424C53u);
            if (actor.ThruActors) hash = Mix(hash, 0x54485255u);
            if (actor.MThruSpecies) hash = Mix(hash, 0x4D545350u);
            if (actor.ThruSpecies) hash = Mix(hash, 0x54535043u);
            if (actor.Ghost) hash = Mix(hash, 0x47484F53u);
            if (actor.ThruGhost) hash = Mix(hash, 0x54474853u);
            if (actor.NonShootable) hash = Mix(hash, 0x4E534854u);
            if (actor.HitOwner) hash = Mix(hash, 0x484F574Eu);
            if (actor.Spectral) hash = Mix(hash, 0x53504354u);
            if (actor.AllowThruBits) hash = Mix(hash, 0x41544254u);
            if (actor.ThruBits != 0) { hash = Mix(hash, 0x54424954u); hash = Mix(hash, actor.ThruBits); }
            if (actor.SpawnCeiling) hash = Mix(hash, 0x4345494Cu);
            if (actor.NoBlockMonsters) hash = Mix(hash, 0x4E424D4Fu);
            if (actor.NoBlockmap) hash = Mix(hash, 0x4E424D50u);
            if (actor.Invisible) hash = Mix(hash, 0x494E5649u);
            if (actor.FloorClip != 0) { hash = Mix(hash, 0x46434C50u); hash = MixDouble(hash, actor.FloorClip); }
            if (actor.NoTeleport) hash = Mix(hash, 0x4E54454Cu);
            if (actor.Dormant) hash = Mix(hash, 0x444F524Du);
            if (actor.ActiveState != -1 || actor.InactiveState != -1)
            {
                hash = Mix(hash, 0x41435456u);
                hash = Mix(hash, unchecked((uint)actor.ActiveState));
                hash = Mix(hash, unchecked((uint)actor.InactiveState));
            }
            if (actor.CanSlide != (actor is PlayerPawn)) hash = Mix(hash, 0x534C4944u);
            hash = Mix(hash, actor.NoTarget ? 1u : 0u);
            hash = Mix(hash, actor.OnMobj ? 1u : 0u);
            hash = Mix(hash, actor.IsMonster ? 1u : 0u);
            if (actor.UseSpecial) hash = Mix(hash, 0x55535043u);
            if (actor.MasterId is { } masterId) { hash = Mix(hash, 0x4d415354u); hash = Mix(hash, masterId); }
            if (actor.GoalId is { } goalId) { hash = Mix(hash, 0x474f414cu); hash = Mix(hash, goalId); }
            hash = Mix(hash, actor.HarmFriends ? 1u : 0u);
            hash = Mix(hash, actor.NoTargetSwitch ? 1u : 0u);
            hash = Mix(hash, actor.NoHatePlayers ? 1u : 0u);
            if (actor.CanPickupItems != (actor is PlayerPawn))
            {
                hash = Mix(hash, 0x5049434bu);
                hash = Mix(hash, actor.CanPickupItems ? 1u : 0u);
            }
            if (actor.SpecialPickup != PickupCatalog.IsPickup(actor.DoomEdNum))
            {
                hash = Mix(hash, 0x53504543u);
                hash = Mix(hash, actor.SpecialPickup ? 1u : 0u);
            }
            if (actor is PlayerPawn && (!actor.SpawnCanPickupItems || actor.SpawnSpecialPickup))
            {
                hash = Mix(hash, 0x53504346u);
                hash = Mix(hash, (actor.SpawnCanPickupItems ? 1u : 0u) | (actor.SpawnSpecialPickup ? 2u : 0u));
            }
            if (actor is not PlayerPawn && (actor.SpawnCanPickupItems != actor.CanPickupItems
                || actor.SpawnSpecialPickup != actor.SpecialPickup))
            {
                hash = Mix(hash, 0x52535046u);
                hash = Mix(hash, (actor.SpawnCanPickupItems ? 1u : 0u) | (actor.SpawnSpecialPickup ? 2u : 0u));
            }
            hash = Mix(hash, unchecked((uint)actor.TidToHate));
            hash = Mix(hash, actor.QuickToRetaliate ? 1u : 0u);
            hash = Mix(hash, actor.Friendly ? 1u : 0u);
            if (actor.SpawnFriendly) hash = Mix(hash, 0x53504652u);
            hash = Mix(hash, (uint)actor.FriendPlayer);
            hash = Mix(hash, actor.NoInfighting ? 1u : 0u);
            if (actor.NoInfightSpecies) hash = Mix(hash, 0x4E494653u);
            if (actor.InfightingGroup != 0)
            { hash = Mix(hash, 0x49464752u); hash = Mix(hash, unchecked((uint)actor.InfightingGroup)); }
            if (actor.ProjectileGroup != 0)
            { hash = Mix(hash, 0x50524752u); hash = Mix(hash, unchecked((uint)actor.ProjectileGroup)); }
            if (actor.SplashGroup != 0)
            { hash = Mix(hash, 0x53504752u); hash = Mix(hash, unchecked((uint)actor.SplashGroup)); }
            hash = Mix(hash, actor.ForceInfighting ? 1u : 0u);
            hash = Mix(hash, actor.DoHarmSpecies ? 1u : 0u);
            hash = Mix(hash, actor.NoTelefrag ? 1u : 0u);
            hash = Mix(hash, actor.AlwaysTelefrag ? 1u : 0u);
            hash = Mix(hash, actor.DontDrain ? 1u : 0u);
            hash = Mix(hash, actor.IgnoreAmmoSkill ? 1u : 0u);
            hash = Mix(hash, actor.Depleted ? 1u : 0u);
            hash = Mix(hash, (uint)actor.PickupAmount);
            if (actor.SuppressWeaponPickupAmmo) hash = Mix(hash, 0x575a414du);
            if (actor.AlwaysPickupOverride is { } alwaysPickup)
            {
                hash = Mix(hash, 0x41504f56u);
                hash = Mix(hash, alwaysPickup ? 1u : 0u);
            }
            if (actor.PickupDelay != 0)
            {
                hash = Mix(hash, 0x5044454cu);
                hash = Mix(hash, (uint)actor.PickupDelay);
            }
            hash = Mix(hash, (uint)actor.RaiseDuration);
            hash = Mix(hash, (uint)actor.Mass);
            hash = Mix(hash, (uint)actor.Gravity.Raw);
            hash = Mix(hash, (uint)actor.DamageFactor.Raw);
            hash = actor.MixDamageFactorChecksum(hash);
            if (actor.ActorSounds.Count != 0)
            {
                hash = Mix(hash, 0x41534E44u);
                hash = Mix(hash, (uint)actor.ActorSounds.Count);
                foreach (var (type, name) in actor.ActorSounds.OrderBy(pair => pair.Key))
                {
                    hash = Mix(hash, (uint)type);
                    hash = Mix(hash, (uint)name.Length);
                    foreach (var character in name) hash = Mix(hash, character);
                }
            }
            hash = Mix(hash, (uint)actor.DamageMultiplier.Raw);
            hash = Mix(hash, (uint)actor.MeleeRange.Raw);
            if (actor.NoVerticalMeleeRange) hash = Mix(hash, 0x4E564D52u);
            if (actor.Killed) hash = Mix(hash, 0x4B494C4Cu);
            hash = Mix(hash, (uint)actor.Friction.Raw);
            hash = Mix(hash, actor.NoTrigger ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.Score));
            hash = Mix(hash, unchecked((uint)actor.MovementSpeed.Raw));
            hash = Mix(hash, unchecked((uint)actor.Damage));
            hash = Mix(hash, actor.Dropped ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.ReactionTime));
            hash = Mix(hash, actor.ReactionTimeInitialized ? 1u : 0u);
            if (actor.InFloat || actor.VerticalFriction)
            {
                hash = Mix(hash, 0x56465249u);
                hash = Mix(hash, actor.InFloat ? 1u : 0u);
                hash = Mix(hash, actor.VerticalFriction ? 1u : 0u);
            }
            hash = Mix(hash, actor.NoRadiusDamage ? 1u : 0u);
            hash = Mix(hash, actor.NoSectorDamage ? 1u : 0u);
            hash = Mix(hash, actor.ForceSectorDamage ? 1u : 0u);
            hash = Mix(hash, actor.Ambush ? 1u : 0u);
            if (!string.IsNullOrEmpty(actor.DeathDamageType))
            {
                hash = Mix(hash, 0x444D4F44u);
                hash = Mix(hash, (uint)actor.DeathDamageType.Length);
                foreach (var character in actor.DeathDamageType) hash = Mix(hash, character);
            }
            hash = Mix(hash, actor.LastDamageSourceId ?? 0);
            if (actor is PlayerPawn player)
            {
                hash = Mix(hash, (uint)player.WeaponCooldown);
                if (player.MaxHealth != 0)
                {
                    hash = Mix(hash, 0x504d4850u);
                    hash = Mix(hash, unchecked((uint)player.MaxHealth));
                }
                if (player.MaxPickupHealth != 0)
                {
                    hash = Mix(hash, 0x4D504855u);
                    hash = Mix(hash, unchecked((uint)player.MaxPickupHealth));
                }
                if (player.Stamina != 0 || player.BonusHealth != 0)
                {
                    hash = Mix(hash, 0x48555047u);
                    hash = Mix(hash, unchecked((uint)player.Stamina));
                    hash = Mix(hash, unchecked((uint)player.BonusHealth));
                }
                hash = Mix(hash, player.PlayerNum);
                hash = Mix(hash, player.GodMode ? 1u : 0u);
                hash = Mix(hash, player.UseHeld ? 1u : 0u);
                hash = Mix(hash, player.AttackHeld ? 1u : 0u);
                hash = Mix(hash, player.RespawnArmed ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)player.RespawnEarliestTic));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.UseRange).Raw));
                if (Fixed.FromDouble(player.UseRange).ToDouble() != player.UseRange)
                {
                    hash = Mix(hash, 0x55535247u);
                    hash = MixDouble(hash, player.UseRange);
                }
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.CrouchFactor).Raw));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.FullHeight).Raw));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.ViewHeight).Raw));
                hash = Mix(hash, unchecked((uint)player.DefaultViewHeight.Raw));
                hash = Mix(hash, unchecked((uint)player.AttackZOffset.Raw));
                hash = Mix(hash, unchecked((uint)player.BobTimer));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.ViewBobOffset).Raw));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.MovementBob).Raw));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.WeaponBobX).Raw));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.WeaponBobY).Raw));
                if (player.ClassicFlight) hash = Mix(hash, 0x464C5943u);
                if (player.JumpTics != 0)
                {
                    hash = Mix(hash, 0x4A554D50u);
                    hash = Mix(hash, unchecked((uint)player.JumpTics));
                }
                hash = Mix(hash, player.UncrouchLocked ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)player.WeaponOffsetY));
                hash = Mix(hash, player.WeaponLowering ? 1u : 0u);
                hash = Mix(hash, player.InstantWeaponSwitch ? 1u : 0u);
                hash = Mix(hash, (uint)player.TurnTicks);
                hash = Mix(hash, player.TurnHeld ? 1u : 0u);
                hash = Mix(hash, player.Inventory.Pending is { } pending ? (uint)pending : 0u);
                if (player.Inventory.ArmorMaximum != 1)
                {
                    hash = Mix(hash, 0x414d4158u);
                    hash = Mix(hash, unchecked((uint)player.Inventory.ArmorMaximum));
                }
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.DrainStrength).Raw));
                hash = Mix(hash, player.Buddha2 ? 1u : 0u);
                hash = Mix(hash, player.ExtremelyDead ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)player.PowerBuddhaTics));
                if (player.PowerDamageTics != 0)
                { hash = Mix(hash, 0x50444D47u); hash = Mix(hash, unchecked((uint)player.PowerDamageTics)); }
                if (player.PowerProtectionTics != 0)
                { hash = Mix(hash, 0x50505254u); hash = Mix(hash, unchecked((uint)player.PowerProtectionTics)); }
                hash = Mix(hash, unchecked((uint)player.FragCount));

                hash = Mix(hash, player.Inventory.BlueKey ? 1u : 0u);
                hash = Mix(hash, player.Inventory.YellowKey ? 1u : 0u);
                hash = Mix(hash, player.Inventory.RedKey ? 1u : 0u);
                hash = Mix(hash, (uint)player.Inventory.Bullets);
                hash = Mix(hash, (uint)player.Inventory.Shells);
                hash = Mix(hash, (uint)player.Inventory.Rockets);
                hash = Mix(hash, (uint)player.Inventory.Cells);
                hash = Mix(hash, (uint)player.Inventory.MaxBullets);
                hash = Mix(hash, (uint)player.Inventory.MaxShells);
                hash = Mix(hash, (uint)player.Inventory.MaxRockets);
                hash = Mix(hash, (uint)player.Inventory.MaxCells);
                hash = Mix(hash, player.Inventory.HasBackpack ? 1u : 0u);
                hash = Mix(hash, (uint)player.JumpZ.Raw);
                hash = Mix(hash, (uint)player.Inventory.Armor);
                hash = Mix(hash, (uint)player.Inventory.ArmorSavePercent);
                hash = Mix(hash, (uint)player.Inventory.MaxAbsorb);
                hash = Mix(hash, (uint)player.Inventory.MaxFullAbsorb);
                hash = Mix(hash, (uint)player.Inventory.AbsorbCount);
                if (player.Inventory.ArmorType != "None")
                {
                    hash = Mix(hash, 0x41545950u);
                    foreach (var character in player.Inventory.ArmorType)
                        hash = Mix(hash, character);
                }
                if (player.Inventory.ArmorActualSaveAmount != 0)
                {
                    hash = Mix(hash, 0x41415341u);
                    hash = Mix(hash, unchecked((uint)player.Inventory.ArmorActualSaveAmount));
                }
                hash = Mix(hash, (uint)player.Inventory.SpareArmor.Count);
                foreach (var spare in player.Inventory.SpareArmor)
                {
                    hash = Mix(hash, (uint)spare.SaveAmount);
                    hash = Mix(hash, (uint)spare.SavePercent);
                    hash = Mix(hash, (uint)spare.MaxAbsorb);
                    hash = Mix(hash, (uint)spare.MaxFullAbsorb);
                    if (spare.IgnoreSkill) hash = Mix(hash, 0x53494753u);
                    if (spare.ArmorType != "BasicArmorPickup")
                    {
                        hash = Mix(hash, 0x53545950u);
                        foreach (var character in spare.ArmorType)
                            hash = Mix(hash, character);
                    }
                }
                hash = Mix(hash, (uint)player.Inventory.Selected);
                if (player.Inventory.NeverAutoSwitch) hash = Mix(hash, 0x4e535743u);
                hash = Mix(hash, (uint)player.Inventory.Weapons);
            }
            if (actor.Brain is { } brain)
            {
                hash = Mix(hash, brain.StateChecksum);
            }
            if (actor is ProjectileActor projectile)
            {
                hash = Mix(hash, projectile.Owner.Id);
                hash = Mix(hash, (uint)projectile.Kind);
                hash = Mix(hash, (uint)projectile.RemainingTics);
                hash = Mix(hash, projectile.TracerTargetId ?? 0);
            }
            if (actor is IceChunkActor chunk)
                hash = Mix(hash, (uint)chunk.RemainingTics);
            if (actor is PuffActor puff)
                hash = Mix(hash, (uint)puff.RemainingTics);
        }

        foreach (var line in Level.Lines)
        {
            hash = Mix(hash, unchecked((uint)line.Special));
            hash = Mix(hash, unchecked((uint)line.Arg3));
            hash = Mix(hash, line.PlayerUse ? 1u : 0u);
            hash = Mix(hash, line.UseThrough ? 1u : 0u);
            hash = Mix(hash, line.PlayerUseBack ? 1u : 0u);
        }
        foreach (var side in Level.Sides)
        {
            hash = MixDouble(hash, side.MidTextureOffsetY);
            hash = MixDouble(hash, side.MidTextureOffsetX);
            hash = MixDouble(hash, side.TopTextureOffsetX);
            hash = MixDouble(hash, side.TopTextureOffsetY);
            hash = MixDouble(hash, side.BottomTextureOffsetX);
            hash = MixDouble(hash, side.BottomTextureOffsetY);
            hash = MixDouble(hash, side.TopTextureScaleX);
            hash = MixDouble(hash, side.TopTextureScaleY);
            hash = MixDouble(hash, side.MidTextureScaleX);
            hash = MixDouble(hash, side.MidTextureScaleY);
            hash = MixDouble(hash, side.BottomTextureScaleX);
            hash = MixDouble(hash, side.BottomTextureScaleY);
        }
        foreach (var sector in Level.Sectors)
        {
            hash = MixDouble(hash, sector.FloorTextureOffsetX);
            hash = MixDouble(hash, sector.FloorTextureOffsetY);
            hash = MixDouble(hash, sector.CeilingTextureOffsetX);
            hash = MixDouble(hash, sector.CeilingTextureOffsetY);
            hash = MixDouble(hash, sector.FloorTextureScaleX);
            hash = MixDouble(hash, sector.FloorTextureScaleY);
            hash = MixDouble(hash, sector.CeilingTextureScaleX);
            hash = MixDouble(hash, sector.CeilingTextureScaleY);
            hash = MixDouble(hash, sector.FloorTextureBaseOffsetY);
            hash = MixDouble(hash, sector.CeilingTextureBaseOffsetY);
            if (sector.Gravity != 1)
            {
                hash = Mix(hash, 0x53475256u);
                var gravityBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(sector.Gravity));
                hash = Mix(hash, (uint)gravityBits);
                hash = Mix(hash, (uint)(gravityBits >> 32));
            }
            hash = Mix(hash, sector.FloorTextureAngle);
            hash = Mix(hash, sector.CeilingTextureAngle);
            hash = Mix(hash, sector.FloorTextureBaseAngle);
            hash = Mix(hash, sector.CeilingTextureBaseAngle);
            hash = Mix(hash, unchecked((uint)sector.Special));
            hash = Mix(hash, unchecked((uint)sector.DamageAmount));
            hash = Mix(hash, unchecked((uint)sector.DamageInterval));
            hash = Mix(hash, unchecked((uint)sector.Leakiness));
            hash = Mix(hash, sector.DamageEndsGodMode ? 1u : 0u);
            hash = Mix(hash, sector.DamageEndsLevel ? 1u : 0u);
            hash = Mix(hash, sector.HurtMonsters ? 1u : 0u);
            hash = Mix(hash, sector.HarmInAir ? 1u : 0u);
            if (sector.NoAttack) hash = Mix(hash, 0x4E4F4154u);
            hash = Mix(hash, (uint)sector.DamageType.Length);
            foreach (var character in sector.DamageType) hash = Mix(hash, character);
            hash = Mix(hash, (uint)sector.FloorPic.Length);
            foreach (var character in sector.FloorPic) hash = Mix(hash, character);
        }
        foreach (var scroll in _textureScrolls)
            hash = MixScroll(hash, scroll.SectorIndex, (int)scroll.Plane, scroll.Dx, scroll.Dy);
        foreach (var scroll in _sideTextureScrolls)
            hash = MixScroll(hash, scroll.SideIndex, (int)scroll.Parts + 1, scroll.Dx, scroll.Dy);
        foreach (var scroll in _sectorCarryScrolls)
            hash = MixScroll(hash, scroll.SectorIndex, 9, scroll.Dx, scroll.Dy, affect: scroll.Affect);
        foreach (var scroll in _acceleratingCarryScrolls)
            hash = MixScroll(hash, scroll.SectorIndex, 10, scroll.BaseDx, scroll.BaseDy,
                affect: scroll.Affect, vdx: scroll.Vdx, vdy: scroll.Vdy);
        foreach (var scroll in _controlCarryScrolls)
            hash = MixScroll(hash, scroll.TargetSector, scroll.Accelerative ? 12 : 11, scroll.ScaleX, scroll.ScaleY,
                scroll.ControlSector, lastHeight: scroll.LastCenter, vdx: scroll.Vdx, vdy: scroll.Vdy);

        foreach (var scroll in _controlPlaneScrolls)
            hash = MixScroll(hash, scroll.Target, scroll.ArchiveKind, scroll.Dx, scroll.Dy,
                scroll.Control, lastHeight: scroll.LastHeight, vdx: scroll.Vdx, vdy: scroll.Vdy);

        foreach (var scroll in _controlWallScrolls)
            hash = MixScroll(hash, scroll.Side, scroll.ArchiveKind, scroll.Dx, scroll.Dy,
                scroll.Control, lastHeight: scroll.LastHeight, vdx: scroll.Vdx, vdy: scroll.Vdy);

        foreach (var floor in Floors)
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(floor).Raw));
        foreach (var ceiling in Ceilings)
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(ceiling).Raw));
        foreach (var light in Lights)
            hash = Mix(hash, unchecked((uint)light));
        foreach (var scroll in SectorScrollX)
            hash = Mix(hash, unchecked((uint)BitConverter.DoubleToInt64Bits(scroll)));
        foreach (var scroll in SectorScrollY)
            hash = Mix(hash, unchecked((uint)BitConverter.DoubleToInt64Bits(scroll)));
        foreach (var effect in LightEffects.OrderBy(effect => effect.Sector))
            hash = Mix(hash, effect.Checksum);
        foreach (var motion in _motions.OrderBy(motion => motion.SectorIndex).ThenBy(motion => motion.Kind))
        {
            foreach (var value in new[] { motion.SectorIndex, (int)motion.Kind, motion.ClosedFloor,
                motion.TargetFloor, motion.ClosedCeiling, motion.TargetCeiling, motion.Speed, motion.Delay,
                motion.Wait, motion.CrushDamage, motion.Closing ? 1 : 0, motion.CloseAfterOpen ? 1 : 0,
                motion.Tag, motion.Paused ? 1 : 0, motion.Loop ? 1 : 0, motion.ReturnSpeed,
                motion.SlowOnCrush ? 1 : 0, motion.StopOnCrush ? 1 : 0, motion.Slowed ? 1 : 0,
                motion.StairGroup, motion.StairSeedIndex, motion.Completed ? 1 : 0, motion.StairStopped ? 1 : 0, motion.ResetCountdown, motion.WaitingForReset ? 1 : 0,
                motion.StepPeriod, motion.StepCountdown, motion.StairPause, motion.CloseOnly ? 1 : 0,
                motion.OpenAfterClose ? 1 : 0, motion.CeilingDirection, motion.FloorDirection, motion.CrushWithoutDamage ? 1 : 0, motion.FloorCrushStopEligible ? 1 : 0 })
            {
                hash = Mix(hash, unchecked((uint)BitConverter.DoubleToInt64Bits(value)));
                hash = Mix(hash, unchecked((uint)(BitConverter.DoubleToInt64Bits(value) >> 32)));
            }
        }
        hash = Mix(hash, Exited ? 1u : 0u);

        Checksum = hash;
    }

    private static uint MixScroll(uint hash, int target, int kind, double dx, double dy,
        int control = -1, ScrollCarryAffect affect = ScrollCarryAffect.All,
        double lastHeight = 0, double vdx = 0, double vdy = 0)
    {
        hash = Mix(hash, 0x5343524cu);
        hash = Mix(hash, unchecked((uint)target));
        hash = Mix(hash, unchecked((uint)kind));
        hash = MixDouble(hash, dx);
        hash = MixDouble(hash, dy);
        hash = Mix(hash, unchecked((uint)control));
        hash = Mix(hash, (uint)affect);
        hash = MixDouble(hash, lastHeight);
        hash = MixDouble(hash, vdx);
        return MixDouble(hash, vdy);
    }

    private static uint MixDouble(uint hash, double value)
    {
        var bits = BitConverter.DoubleToInt64Bits(value);
        return Mix(Mix(hash, unchecked((uint)bits)), unchecked((uint)(bits >> 32)));
    }

    private static uint Mix(uint hash, uint value) => (hash ^ value) * 16777619u;
}

public static class HeadlessMapBoot
{
    public static bool TryBoot(
        ReadOnlySpan<byte> wad,
        string mapName,
        out AuthoritySimulation? simulation,
        out string? error,
        int rngSeed = 0, BinaryThingFlagFormat thingFlagFormat = BinaryThingFlagFormat.Doom)
    {
        simulation = null;
        if (!LevelBuilder.TryFromWad(wad, mapName, out var level, out error, thingFlagFormat))
            return false;

        try
        {
            simulation = AuthoritySimulation.Start(level, rngSeed);
        }
        catch (InvalidDataException ex)
        {
            simulation = null;
            error = ex.Message;
            return false;
        }

        simulation.Tick();
        return true;
    }
}
