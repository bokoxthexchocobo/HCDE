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
    internal bool SpawnedDroppedPickup { get; set; }
    internal int? MapPickupThingIndex { get; set; }
    internal int? MapDecorationThingIndex { get; set; }
    internal int? MapMonsterThingIndex { get; set; }
    protected override void OnDestroy() => Simulation?.NotifyActorDestroyed(this);
    public const int PlayerStartMin = 1;
    public const int PlayerStartMax = 4;

    public uint Id { get; init; }
    public IReadOnlyDictionary<ActorSoundType, string> ActorSounds { get; set; }
        = new Dictionary<ActorSoundType, string>();
    /// <summary>Native <c>SN_StartSequence</c> name from ACS CallFunc.</summary>
    internal string? AcsSoundSequenceName { get; set; }
    internal IReadOnlyDictionary<ActorSoundType, string>? SpawnActorSounds { get; set; }
    /// <summary>Native <c>GetNetworkID</c>. Assigned by netplay; ACS may query it offline.</summary>
    public uint NetworkId { get; set; }
    public int DoomEdNum { get; init; }
    internal int DefinitionDoomEdNum { get; set; }
    internal bool SpawnedLostSoul { get; set; }
    internal bool FailedLostSoulSpawn { get; set; }
    internal int ClassDoomEdNum => DefinitionDoomEdNum > 0 ? DefinitionDoomEdNum : DoomEdNum;
    public int ThingId { get; internal set; }
    /// <summary>Native morph state subset for ACS <c>PCD_UNMORPHACTOR</c>.</summary>
    public bool IsMorphForm { get; set; }
    /// <summary>Native <c>AActor::IsMapActor</c> for represented non-inventory actors.</summary>
    public virtual bool IsMapActor() => true;
    public Func<Actor, (double X, double Y, double Z), double, int, bool>? PreTeleportFunction { get; set; }
    public Action<Actor, (double X, double Y, double Z), double, int>? PostTeleportFunction { get; set; }
    public virtual bool PreTeleport((double X, double Y, double Z) destination, double angle, int flags)
        => PreTeleportFunction?.Invoke(this, destination, angle, flags) ?? true;
    public virtual void PostTeleport((double X, double Y, double Z) destination, double angle, int flags)
        => PostTeleportFunction?.Invoke(this, destination, angle, flags);
    public bool NoBlockmap { get; set; }
    public bool NoSector { get; set; }
    internal bool HasSectorOverride { get; set; }
    public bool IsSectorLinked => !Destroyed && !NoSector;
    public bool CanPass { get; set; }
    public bool DontOverlap { get; set; }
    public bool ExplodeCount { get; set; }
    public bool SkyExplode { get; set; }
    public bool HitTarget { get; set; }
    public bool HitMaster { get; set; }
    public bool HitTracer { get; set; }
    internal bool DefaultCanPass => this is PlayerPawn || (ResurrectionCollisionFlags.GetValueOrDefault() & 256) != 0;
    public bool MoveWithSector { get; set; }
    internal bool DefaultMoveWithSector => this is IceChunkActor || DoomDecorationDefaults.MovesWithSector(ClassDoomEdNum);
    public bool NoLiftDrop { get; set; }
    public bool RelativeToFloor { get; set; }
    internal bool IsSectorMoverActor => IsSectorLinked && (!NoBlockmap || MoveWithSector);
    public bool CantSeek { get; set; }
    public bool NoSightCheck { get; set; }
    public bool HuntPlayers { get; set; }
    public bool ChaseGoal { get; set; }
    public bool Vulnerable { get; set; }
    public bool MVisBlocked { get; set; }
    public bool SeeInvisible { get; set; }
    public int LastLookPlayerNumber { get; internal set; }
    public bool NoBlood { get; set; }
    public bool PuffGetsOwner { get; set; }
    public bool IsPuff { get; internal set; }
    internal bool PendingSpawnDeath { get; set; }
    internal bool DefaultIsPuff { get; set; }
    internal bool DefaultNoBlood { get; set; }
    public bool SpawnSoundSource { get; set; }
    public bool DontThrust { get; set; }
    public bool DontSeekInvisible { get; set; }
    public bool Shadow { get; set; }
    internal bool DefaultShadow { get; set; }
    public bool MinVisible { get; set; }
    public ActorRenderStyle RenderStyle { get; set; } = ActorRenderStyle.Normal;
    public int GetRenderStyle() => RenderStyle.ToNative();
    internal ActorRenderStyle DefaultRenderStyle { get; set; } = ActorRenderStyle.Normal;
    internal double DefaultAlpha { get; set; } = 1;
    public void RestoreRenderStyle() => ActorPropertyActions.RestoreRenderStyle(this);
    private double _alpha = 1;
    public bool VisibilityPulse { get; set; }
    public bool Stealth { get; set; }
    internal bool DefaultStealth { get; set; }
    internal bool DefaultFloatBob { get; set; }
    internal bool DefaultSeeker { get; set; }
    internal bool DefaultReflective { get; set; }
    internal bool DefaultThruGhost { get; set; }
    internal bool DefaultNonShootable { get; set; }
    internal bool DefaultFloorPic { get; set; }
    internal bool DefaultRip { get; set; }
    internal int DefaultPushFlags { get; set; }
    internal bool DefaultTeleStomp { get; set; }
    internal bool DefaultFloorClip { get; set; }
    internal bool DefaultBlasted { get; set; }
    internal bool DefaultTouchy { get; set; }
    internal bool DefaultNoClip { get; set; }
    internal int DefaultCombatFlags { get; set; }
    internal bool DefaultInFloat { get; set; }
    internal bool DefaultCorpse { get; set; }
    internal bool DefaultFly { get; set; }
    public bool SpawnFloat { get; set; }
    internal bool DefaultSpawnFloat { get; set; }
    private double _stealthAlpha;
    public double StealthAlpha
    {
        get => _stealthAlpha;
        set => _stealthAlpha = double.IsFinite(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    private sbyte _visibilityDirection;
    public int VisibilityDirection
    {
        get => _visibilityDirection;
        set => _visibilityDirection = value is >= sbyte.MinValue and <= sbyte.MaxValue
            ? (sbyte)value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public double Alpha
    {
        get => _alpha;
        set
        {
            if (!double.IsFinite(value)) throw new ArgumentOutOfRangeException(nameof(value));
            _alpha = value;
        }
    }
    /// <summary>Native CheckFov: fov is the maximum angular deviation, not a full cone width.</summary>
    public bool CheckFov(Actor target, double fov)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!double.IsFinite(fov)) throw new ArgumentOutOfRangeException(nameof(fov));
        return Math.Abs(Normalize180(Angle.ToDegrees() - AngleTo(target))) <= fov;
    }

    public bool CheckSight(Actor target, ActorSightFlags flags = 0)
    {
        ArgumentNullException.ThrowIfNull(target);
        var sim = Simulation ?? throw new InvalidOperationException("Sight queries require an attached simulation.");
        return CombatTrace.CheckSight(sim, this, target, flags);
    }

    public double GetCrouchFactor(int pointerSelector = AcsActorPointer.Player1)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? this
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(Simulation
                ?? throw new InvalidOperationException("Crouch pointer queries require an attached simulation."), this, pointerSelector);
        return subject is PlayerPawn player ? player.CrouchFactor : 1;
    }

    /// <summary>Native Actor.GetCVar. Missing, ignored, and unresolved user cvars return 0.</summary>
    public double GetCVar(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var sim = Simulation ?? throw new InvalidOperationException("CVar queries require an attached simulation.");
        return sim.CVars.ReadFloat(CVarPlayerNumber, sim.IsPlayerInGame, sim.ConsolePlayer, name);
    }

    /// <summary>Native Actor.GetCVarString. Missing, ignored, and unresolved user cvars return empty.</summary>
    public string GetCVarString(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var sim = Simulation ?? throw new InvalidOperationException("CVar queries require an attached simulation.");
        return sim.CVars.ReadString(CVarPlayerNumber, sim.IsPlayerInGame, sim.ConsolePlayer, name);
    }

    /// <summary>Native Actor.GetPlayerInput. Missing players and unknown input numbers return 0.</summary>
    public int GetPlayerInput(int input, int pointerSelector = AcsActorPointer.Default)
    {
        var subject = pointerSelector == AcsActorPointer.Default ? this
            : pointerSelector == AcsActorPointer.Null ? null
            : AcsActorPointer.Resolve(Simulation
                ?? throw new InvalidOperationException("Player input pointer queries require an attached simulation."), this, pointerSelector);
        return subject is PlayerPawn player ? player.ReadPlayerInput(input) : 0;
    }

    private int CVarPlayerNumber => this is PlayerPawn player ? player.PlayerNum : -1;

    public bool CanSeek(Actor target)
    {
        ArgumentNullException.ThrowIfNull(target);
        return !target.CantSeek && !(DontSeekInvisible && (target.Invisible || target.Shadow || target.MinVisible || !target.RenderStyle.IsVisible(target.Alpha)));
    }
    /// <summary>Native rough-search attack eligibility for represented actors and projectile ownership.</summary>
    public bool IsOkayToAttack(Actor candidate)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate == this || candidate.Destroyed || candidate is not PlayerPawn && !candidate.IsMonster
            || !candidate.Shootable || candidate.Dormant || candidate.NeverTarget) return false;
        var sim = Simulation ?? throw new InvalidOperationException("Attack eligibility requires an attached simulation.");
        Actor? friend = SummonedMonster ? AcsActorPointer.Resolve(sim, this, AcsActorPointer.Tracer) : this is ProjectileActor
            ? AcsActorPointer.Resolve(sim, this, AcsActorPointer.Target)
            : Friendly && FriendPlayer != 0
                ? sim.Players.FirstOrDefault(player => player.PlayerNum + 1 == FriendPlayer && !player.Destroyed)
                : this;
        if (friend is null || candidate == friend || friend.IsFriend(candidate)) return false;
        if (candidate.SummonedMonster && AcsActorPointer.Resolve(sim, candidate, AcsActorPointer.Tracer) == friend) return false;
        if (sim.GameMode != SpawnGameMode.Deathmatch && candidate is PlayerPawn && friend is PlayerPawn) return false;
        if (Friendly && candidate.Friendly && IsFriend(candidate)) return false;
        if (!CheckSight(candidate)) return false;
        // Native SCREENSEEKER uses this strict historical angle, after sight.
        return !ScreenSeeker || Math.Abs(Normalize180(friend.Angle.ToDegrees() - friend.AngleTo(candidate))) < 30 * (256.0 / 360);
    }
    public bool CanSeek(Actor target, ActorRenderStyle targetStyle, double targetAlpha)
    {
        ArgumentNullException.ThrowIfNull(target);
        return !target.CantSeek && (!DontSeekInvisible ||
            !(target.Invisible || target.Shadow || target.MinVisible) && targetStyle.IsVisible(targetAlpha));
    }
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
    internal Dictionary<string, int[]>? ScriptUserVariables { get; set; }
    internal int GetScriptUserVar(string name, int index = 0)
    {
        if (ScriptUserVariables?.TryGetValue(name, out var slots) == true && (uint)index < (uint)slots.Length)
            return slots[index];
        return 0;
    }
    internal void SetScriptUserVar(string name, int index, int value)
    {
        ScriptUserVariables ??= new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
        if (!ScriptUserVariables.TryGetValue(name, out var slots))
        {
            slots = new int[Math.Max(1, index + 1)];
            ScriptUserVariables[name] = slots;
        }
        else if ((uint)index >= (uint)slots.Length)
        {
            var expanded = new int[index + 1];
            Array.Copy(slots, expanded, slots.Length);
            slots = expanded;
            ScriptUserVariables[name] = slots;
        }
        slots[index] = value;
    }
    internal bool SpecialChanged { get; set; }
    private int _activationType;
    public virtual void Activate(Actor? activator) => ThingActivation.Apply(this, true);
    public virtual void Deactivate(Actor? activator) => ThingActivation.Apply(this, false);
    public Func<Actor, bool>? UseAction { get; set; }
    private uint? _lastLookActorId;
    public uint? LastLookActorId { get => ReadActorPointer(ref _lastLookActorId); internal set => _lastLookActorId = value; }
    private uint? _storedTargetId;
    internal uint? StoredTargetId { get => ReadActorPointer(ref _storedTargetId); set => _storedTargetId = value; }
    private SimChaseThreshold? _chaseThreshold;
    public int ChaseThreshold => Brain?.Threshold ?? _chaseThreshold?.Current ?? 0;
    public int DefaultChaseThreshold => Brain?.DefThreshold ?? _chaseThreshold?.Default ?? 100;
    internal SimChaseThreshold? CaptureChaseThreshold() => Brain is { } brain
        ? brain.HasChaseThresholdOverride || brain.Threshold != 0 || brain.DefThreshold != 100
            ? new SimChaseThreshold(brain.Threshold, brain.DefThreshold) : null
        : _chaseThreshold;
    internal void SetChaseThreshold(int threshold, bool setDefault)
    {
        if (Brain is { } brain) { brain.SetChaseThreshold(threshold, setDefault); return; }
        _chaseThreshold = setDefault ? new(ChaseThreshold, Math.Max(0, threshold))
            : new(Math.Max(0, threshold), DefaultChaseThreshold);
    }
    internal void RestoreChaseThreshold(SimChaseThreshold? saved)
    {
        _chaseThreshold = Brain is null ? saved : null;
        Brain?.RestoreChaseThreshold(saved);
    }
    public uint? TargetId => this is ProjectileActor projectile ? projectile.Owner?.Id : Brain?.TargetId ?? StoredTargetId;
    internal void SetTargetPointer(Actor? target)
    {
        if (this is ProjectileActor projectile) { projectile.ReassignOwner(target); return; }
        if (Brain is { } brain)
        {
            StoredTargetId = null;
            brain.SetSpecialTarget(target);
        }
        else StoredTargetId = target?.Id;
        HasTargetMemoryOverride = true;
    }

    internal bool AssignAcsPointer(int slot, Actor? ptr)
    {
        var assigned = false;
        if ((slot & AcsActorPointer.Target) != 0) { SetTargetPointer(ptr); assigned = ptr != null; }
        if ((slot & AcsActorPointer.Master) != 0) { MasterId = ptr?.Id; assigned = ptr != null; }
        if ((slot & AcsActorPointer.Tracer) != 0) { TracerId = ptr?.Id; assigned = ptr != null; }
        return assigned;
    }

    internal bool ApplyTranslationName(string translation)
    {
        if (string.IsNullOrEmpty(translation)) return false;
        SpecialArgs[1] = translation.GetHashCode(StringComparison.OrdinalIgnoreCase);
        SpecialChanged = true;
        return true;
    }

    private uint? _tracerId;
    public uint? TracerId { get => ReadActorPointer(ref _tracerId); set { _tracerId = value == 0 ? null : value; HasTracerOverride = true; } }
    internal bool HasTracerOverride { get; set; }
    private uint? _masterId;
    public uint? MasterId { get => ReadActorPointer(ref _masterId); set { _masterId = value == 0 ? null : value; HasMasterOverride = true; } }
    internal bool HasMasterOverride { get; set; }
    internal uint? ReadActorPointer(ref uint? pointer)
    {
        if (pointer is { } id && Simulation is { } sim
            && !sim.Actors.Any(actor => actor.Id == id && !actor.Destroyed)) pointer = null;
        return pointer;
    }
    internal void ForgetDestroyedPointers(uint actorId)
    {
        if (_tracerId == actorId) TracerId = null;
        if (_masterId == actorId) MasterId = null;
        if (StoredTargetId == actorId)
        {
            StoredTargetId = null;
            HasTargetMemoryOverride = true;
        }
    }
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
    public double GetGravity()
    {
        if (NoGravity) return 0;
        var sim = Simulation ?? throw new InvalidOperationException("Gravity queries require an attached simulation.");
        var sectorGravity = (uint)SectorIndex < (uint)sim.Level.Sectors.Count
            ? sim.Level.Sectors[SectorIndex].Gravity : 1;
        return sim.LevelGravity * sectorGravity * Gravity.ToDouble() * 0.00125;
    }

    /// <summary>Resolved terrain for the represented sector floor; contact overrides and 3D floors are not modeled.</summary>
    public LevelTerrainDefinition? GetFloorTerrain()
    {
        var sim = Simulation ?? throw new InvalidOperationException("Terrain queries require an attached simulation.");
        return sim.Level.FloorTerrainDefinition(SectorIndex);
    }

    public (double Friction, double MoveFactor) GetFriction()
    {
        var sim = Simulation ?? throw new InvalidOperationException("Friction queries require an attached simulation.");
        return ActorPhysics.TerrainMovementFactors(sim, this);
    }
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
    public Fixed SelfDamageFactor { get; set; } = Fixed.FromInt(1);
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
    /// <summary>Native actor fillcolor for stencil render styles.</summary>
    public uint StencilFillColor { get; set; }
    public int WaterLevel { get; set; }
    public Fixed WaterDepth { get; set; }
    public int FreezeTics { get; set; }
    /// <summary>Native DamageVal; projectiles may also supply a damage expression.</summary>
    public int Damage { get; set; }
    public Func<Actor, int>? DamageFunction { get; set; }
    public Func<Actor, Actor, int>? SpecialMissileHitFunction { get; set; }
    /// <summary>Native callback: 0 blocks, 1 passes, other values use ordinary missile handling.</summary>
    public virtual int SpecialMissileHit(Actor victim) => SpecialMissileHitFunction?.Invoke(this, victim) ?? -1;
    public ReflectionAngleHandler? AdjustReflectionAngleFunction { get; set; }
    /// <summary>Native hook: true rejects reflection; false accepts the updated angle.</summary>
    public virtual bool AdjustReflectionAngle(Actor reflector, ref double angleDegrees) =>
        AdjustReflectionAngleFunction is { } action ? action(this, reflector, ref angleDegrees)
            : ActorReflectionActions.AdjustReflectionAngle(this, reflector, ref angleDegrees);
    internal int? SpawnDamage { get; set; }
    /// <summary>Native SetDamage; projectile overrides also clear their damage expression.</summary>
    public virtual void SetDamage(int damage)
    { Damage = damage; DamageFunction = null; }
    /// <summary>Native IsZeroDamage; projectile overrides also check for a damage expression.</summary>
    public virtual bool IsZeroDamage() => Damage == 0 && DamageFunction == null;
    public virtual bool Slam(Actor victim) => ActorSkullActions.Slam(this, victim);
    /// <summary>Native P_DoMissileDamage action, using this actor's RIP flag and target pointer.</summary>
    public DamageResult DoMissileDamage(Actor victim) => ActorMissileDamage.Apply(this, victim, Rip);
    public DamageResult DoMissileDamage(Actor victim, bool ripper) => ActorMissileDamage.Apply(this, victim, ripper);
    public virtual int GetMissileDamage(int mask, int add)
    {
        if (DamageFunction is { } function) return function(this);
        if (Damage < 0) throw new NotSupportedException("Negative actor damage requires a native damage function.");
        if (mask == 0) return unchecked(add * Damage);
        var sim = Simulation ?? throw new InvalidOperationException("Random missile damage requires a simulation.");
        return unchecked(((int)sim.NextMissileDamageByte() & mask) + add) * Damage;
    }
    /// <summary>Native MBF sentience query: alive and has a See state, independent of brain or monster flags.</summary>
    public bool IsSentient() => Health > 0 && States.HasState(SeeState);
    /// <summary>Native CheckMeleeRange against this actor's current target.</summary>
    public bool CheckMeleeRange(double range = -1) => ActorJumpActions.CheckMeleeRange(this, range);
    private uint? _goalId;
    /// <summary>Native goal pointer identity used by the melee arrival check.</summary>
    public uint? GoalId
    {
        get => ReadActorPointer(ref _goalId);
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
    internal bool DefaultNoTeleport => ResurrectionMovementFlags is { } flags ? (flags & 4) != 0
        : this is IceChunkActor or ProjectileActor;
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
    public bool FlyCheat { get; set; }
    public bool NoClip { get; set; }
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
    public bool FloatBob { get; set; }
    private double _floatBobStrength = 1, _floatBobFactor = 1;
    public double FloatBobStrength
    {
        get => _floatBobStrength;
        set => _floatBobStrength = double.IsFinite(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public double FloatBobFactor
    {
        get => _floatBobFactor;
        set => _floatBobFactor = double.IsFinite(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public double GetBobOffset(double ticFraction = 0)
    {
        if (!double.IsFinite(ticFraction)) throw new ArgumentOutOfRangeException(nameof(ticFraction));
        if (!FloatBob) return 0;
        var tic = Simulation?.Thinkers.Clock.Tic
            ?? throw new InvalidOperationException("Float bob requires an attached simulation.");
        return Math.Sin((FloatBobPhase + tic * FloatBobFactor + ticFraction) * Math.PI / 32) * 8 * FloatBobStrength;
    }
    internal bool HasFloatBobPhaseOverride { get; set; }
    public double SpriteAngle { get; set; }
    public double SpriteRotation { get; set; }
    internal bool HasSpriteOrientationOverride { get; set; }
    internal bool HasSizeOverride { get; set; }
    public bool NoRadiusDamage { get; set; }
    /// <summary>Native <c>RadiusDamageFactor</c> applied in <see cref="MapRadiusAttack"/>.</summary>
    public double RadiusDamageFactor { get; set; } = 1;
    /// <summary>Native <c>MF3_OLDRADIUSDMG</c> barrel-style XY falloff.</summary>
    public bool OldRadiusDamage { get; set; }
    public bool NoSectorDamage { get; set; }
    public bool ForceSectorDamage { get; set; }
    public bool Ambush { get; set; }
    internal bool DefaultAmbush => SpawnAmbush || (ResurrectionDefenseFlags.GetValueOrDefault() & 16) != 0;
    public int PainChance { get; set; } = 256;
    internal int? SpawnPainChance { get; set; }
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
    public bool BlockedBySolidActors { get; set; }
    public bool BumpSpecial { get; set; }
    public bool Reflective { get; set; }
    public bool ThruReflect { get; set; }
    public bool DontReflect { get; set; }
    public bool MirrorReflect { get; set; }
    public bool Deflect { get; set; }
    public bool ShieldReflect { get; set; }
    public bool NoShieldReflect { get; set; }
    public bool AimReflect { get; set; }
    public bool SeekerMissile { get; set; }
    public bool SummonedMonster { get; set; }
    public bool ScreenSeeker { get; set; }
    internal int LastBumpTic { get; set; }
    internal HashSet<Actor>? MovementPushContacts { get; set; }
    internal bool AiPushContext { get; set; }
    internal bool LastPushFromMovement { get; set; }
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
    public bool CausePain { get; set; }
    public bool AllowPain { get; set; }
    public bool NoDamage { get; set; }
    public bool LaxTelefragDamage { get; set; }
    /// <summary>Native <c>MF5_NOPAIN</c>. This actor does not flinch, even when the inflictor forces pain.</summary>
    public bool NoPain { get; set; }
    /// <summary>Native <c>MF5_PAINLESS</c>. As an inflictor, this hit does not flinch, even when forced pain is also set.</summary>
    public bool Painless { get; set; }
    /// <summary>Native <c>MF4_EXTREMEDEATH</c>. As an inflictor, this killing blow can use the extreme death frame without passing the gib line.</summary>
    public bool ExtremeDeath { get; set; }
    /// <summary>Native <c>MF4_NOEXTREMEDEATH</c>. As an inflictor, this hit never uses an extreme death frame, even past the gib line.</summary>
    public bool NoExtremeDeath { get; set; }
    /// <summary>Native <c>RF_FULLBRIGHT</c> from an electric hit that passed the pain roll but missed the pain flicker.</summary>
    public const double DefaultCameraHeight = int.MinValue;
    private double _cameraHeight = DefaultCameraHeight;
    public double CameraHeight
    {
        get => _cameraHeight;
        set => _cameraHeight = double.IsFinite(value) ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public double GetCameraHeight() => CameraHeight == DefaultCameraHeight ? Height.ToDouble() / 2 : CameraHeight;
    public bool FullBright { get; set; }
    /// <summary>Native <c>MF_JUSTHIT</c>. Set when a pain flinch lands from the current or no chase target.</summary>
    public bool JustHit { get; set; }
    public bool JustAttacked { get; set; }
    public bool InCombat { get; set; }
    public bool NoSplashAlert { get; set; }
    public bool DontSplash { get; set; }
    public bool Touchy { get; set; }
    public bool Armed { get; set; }
    public bool DontHarmSpecies { get; set; }
    public bool DontHarmClass { get; set; }
    public const double DefaultLandingSpeed = -8;
    public double LandingSpeed { get; set; } = DefaultLandingSpeed;
    public bool FallDamage { get; set; }
    public bool Smashable { get; set; }
    public bool Crashed { get; set; }
    public bool DontGib { get; set; }
    public bool TeleStomp { get; set; }
    public bool NoTeleStomp { get; set; }
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
    public bool SeeFriendlyMonsters { get; set; }
    public bool LookAllAround { get; set; }
    public bool StandStill { get; set; }
    public bool InConversation { get; set; }
    public bool DontFollowPlayers { get; set; }
    public bool CanJump { get; set; }
    public bool BlockAsPlayer { get; set; }
    public bool FloorClipEnabled { get; set; }
    public bool SpecialFloorClip { get; set; }
    public bool AvoidHazards { get; set; }
    public bool StayOnLift { get; set; }
    /// <summary>Native <c>MF4_CANUSEWALLS</c>: blocked <c>P_Move</c> may activate use lines.</summary>
    public bool CanUseWalls { get; set; }
    /// <summary>Native <c>MF2_PUSHWALL</c>: blocked <c>P_Move</c> may activate push lines.</summary>
    public bool PushWall { get; set; }
    /// <summary>Native <c>MF2_MCROSS</c>: movement may activate lines with monster cross.</summary>
    public bool MonsterCross { get; set; }
    /// <summary>Native <c>MF6_JUMPDOWN</c>: short-range chase drop-off assist.</summary>
    public bool JumpDown { get; set; }
    public bool AvoidMelee { get; set; }
    public int StrafeCount { get; set; }
    public int FastChaseStrafeCount { get; set; }
    /// <summary>Optional native byte-valued missile probability cap; null uses the built-in class default.</summary>
    public byte? MinMissileChance { get; set; }
    private double? _missileChanceMult;
    private double? _maxTargetRange;
    public double? MaxTargetRange
    {
        get => _maxTargetRange;
        set => _maxTargetRange = value is { } range && !double.IsFinite(range)
            ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
    }
    private double? _meleeThreshold;
    public double? MeleeThreshold
    {
        get => _meleeThreshold;
        set => _meleeThreshold = value is { } threshold && !double.IsFinite(threshold)
            ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
    }
    public double? MissileChanceMult
    {
        get => _missileChanceMult;
        set => _missileChanceMult = value is { } multiplier && !double.IsFinite(multiplier)
            ? throw new ArgumentOutOfRangeException(nameof(value)) : value;
    }
    public bool CantLeaveFloorPic { get; set; }
    public bool NoFear { get; set; }
    public bool Frightened { get; set; }
    public bool Frightening { get; set; }
    public byte MoveDirection { get; set; }
    public int MoveCount { get; set; }
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
    internal bool BossCoopHealthScaled { get; set; }
    public bool FullVolumeDeath { get; set; }
    internal bool DefaultFullVolumeDeath { get; set; }
    internal bool DefaultFullVolumeSee { get; set; }
    public bool FullVolumeSee { get; set; }
    public bool OnlySlamSolid { get; set; }
    public bool RetargetAfterSlam { get; set; }
    public bool FullVolumeActive { get; set; }
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
    internal bool DefaultFriendly => SpawnFriendly || (ResurrectionDefenseFlags is { } flags
        ? (flags & 8) != 0 : this is PlayerPawn);
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
    internal string SpeciesName
    {
        get
        {
            if (!string.IsNullOrEmpty(Species) && !Species.Equals("None", StringComparison.OrdinalIgnoreCase)) return Species;
            if (DoomActorCatalog.TrySpawnClassName(DefaultSpecies, out var monster)) return monster;
            var concrete = AcsCallFunctions.ClassNameOf(this);
            return concrete != "None" ? concrete : "Class:" + DefaultSpecies.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }
    }
    // GetSpecies climbs monster parents: Spectre : Demon and HellKnight : BaronOfHell.
    private int DefaultSpecies => ClassDoomEdNum switch { 58 => 3002, 69 => 3003, _ => ClassDoomEdNum };
    // All supported Doom player starts share DoomPlayer's collision species.
    // Damage species immunity deliberately excludes players; contact filtering does not.
    internal bool SharesContactSpecies(Actor other) => SpeciesName.Equals(other.SpeciesName, StringComparison.OrdinalIgnoreCase);
    internal bool RadiusSpeciesImmune(Actor? source) => this is not PlayerPawn
        && source is { DontHarmSpecies: true } && SharesContactSpecies(source);
    internal bool RadiusSourceImmune(Actor? source) => this is not PlayerPawn && source is not null
        && (source.DontHarmClass && ClassDoomEdNum == source.ClassDoomEdNum || RadiusSpeciesImmune(source));
    /// <summary>Native <c>P_ProjectileImmune</c> group and default species rules.</summary>
    public bool ProjectileImmune(Actor source) =>
        (ProjectileGroup != -1 || ReferenceEquals(this, source))
        && (ProjectileGroup == 0 ? SharesContactSpecies(source) && !DoHarmSpecies : ProjectileGroup == source.ProjectileGroup);
    /// <summary>Native <c>CanAttackHurt</c> subset for monster-monster damage.</summary>
    internal bool CanAttackHurtFrom(Actor? shooter)
    {
        if (shooter is null || this is PlayerPawn || shooter is PlayerPawn) return true;
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
    public bool OkayToSwitchTarget(Actor? other) => OkayToSwitchTargetCore(other, TargetId, ChaseThreshold);

    internal bool OkayToSwitchTarget(Actor? other, MonsterBrain brain) =>
        other is not PlayerPawn { NoTarget: true, NoTargetCheatOverride: null }
        && OkayToSwitchTargetCore(other, brain.TargetId, brain.Threshold);

    private bool OkayToSwitchTargetCore(Actor? other, uint? targetId, int threshold)
    {
        if (other is null || !other.Shootable || other.Destroyed || ReferenceEquals(other, this)
            || other.NeverTarget)
            return false;
        if (NoTargetSwitch && targetId != null)
            return false;
        var sim = Simulation;
        var master = sim?.Actors.FirstOrDefault(candidate => candidate.Id == MasterId && !candidate.Destroyed);
        var otherMaster = sim?.Actors.FirstOrDefault(candidate => candidate.Id == other.MasterId && !candidate.Destroyed);
        var masterClass = master != null && MatchesMasterClass(other, master);
        var minionClass = otherMaster != null && MatchesMasterClass(this, otherMaster);
        if ((masterClass || minionClass) && !IsHostile(other)
            && (other.ThingId != TidToHate || TidToHate == 0) && other.TidToHate == TidToHate)
            return false;
        if (NoInfightSpecies && SharesContactSpecies(other))
            return false;
        if (InfightingGroup != 0 && InfightingGroup == other.InfightingGroup)
            return false;
        if (other.NoTarget && (other.ThingId != TidToHate || TidToHate == 0) && !IsHostile(other))
            return false;
        if (threshold != 0 && !QuickToRetaliate)
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
        if (sim != null && targetId is { } currentId && TidToHate != 0)
        {
            var current = sim.Actors.FirstOrDefault(candidate => candidate.Id == currentId);
            if (current != null && !current.IsDead && !current.Destroyed && current.ThingId == TidToHate
                && sim.NextSwitchTargetByte() < 128
                && CombatTrace.CheckSight(sim, this, current))
                return false;
        }
        return true;

        static bool MatchesMasterClass(Actor candidate, Actor master) => master.ClassDoomEdNum > 0
            && (candidate.ClassDoomEdNum == master.ClassDoomEdNum
                || DoomActorCatalog.TrySpawnClassName(master.ClassDoomEdNum, out var name)
                    && DoomActorCatalog.IsKindOfClassName(candidate.ClassDoomEdNum, name));
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

    internal void SetDamageHealth(int value, Actor? source, Func<int> afterDamage, bool useInflictorDeathType = true,
        bool creditSummonedSource = false)
        => SetHealth(value, source, true, afterDamage, useInflictorDeathType: useInflictorDeathType,
            creditSummonedSource: creditSummonedSource);

    private void SetHealth(int value, Actor? source, bool executeDeathSpecial, Func<int>? afterDamage = null, bool forceDeath = false,
        bool useInflictorDeathType = true, bool creditSummonedSource = false)
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
        if ((!dead || forceDeath) && IsDead)
        {
            if (executeDeathSpecial && creditSummonedSource && source is { SummonedMonster: true, Simulation: { } sourceSim }
                && AcsActorPointer.Resolve(sourceSim, source, AcsActorPointer.Tracer) is PlayerPawn summoner
                && sourceSim.Players.Contains(summoner))
                source = summoner;
            // Native Die first queries gib health while living dimensions and flags are intact.
            if (executeDeathSpecial) _ = GetGibHealth();
            if (this is PlayerPawn powerOwner) powerOwner.ClearDamagePowers();
            if (executeDeathSpecial && this is ProjectileActor dyingProjectile)
            {
                dyingProjectile.ExplodeForAction();
                return;
            }
            if (executeDeathSpecial && source is not null && this is not ProjectileActor)
                SetTargetPointer(source);
            if (executeDeathSpecial)
            {
                Shootable = false;
                Floating = false;
                Brain?.ClearDeathCharge();
                AllowDropOff = true;
                if (!DontFall) NoGravity = false;
            }
            if (!DontCorpse && (IsMonster || this is PlayerPawn || States.HasState(RaiseState) || RaiseDuration > 0))
                Corpse = true;
            if (executeDeathSpecial) Killed = true;
            if (executeDeathSpecial)
            {
                var incomingType = useInflictorDeathType ? ResolveDeathType(DamageTypeReceived, DeathInflictor) : DamageTypeReceived;
                if (!useInflictorDeathType || !(this is PlayerPawn && string.Equals(incomingType, "Fire", StringComparison.OrdinalIgnoreCase)
                    && DeathInflictor is { SpecialFireDamage: true } && (_health <= -50 || DeathDamageAmount <= 25)))
                    DamageType = incomingType;
            }
            if (executeDeathSpecial) Height = GetDeathHeight();
            DeathDamageType = string.Equals(DamageType, "Massacre", StringComparison.OrdinalIgnoreCase) ? "Massacre" : null;
            DeathCount++;
            if (executeDeathSpecial && Simulation is { } deathSimulation)
                ActorSpecialActions.OnDeath(deathSimulation, this, source);
            if (executeDeathSpecial && this is PlayerPawn) Solid = false;
            if (this is PlayerPawn dyingPlayer)
            {
                dyingPlayer.ClearCommands();
                dyingPlayer.AttackPressed = false;
                dyingPlayer.NoteDeath(Simulation);
            }
            var gibHealth = GetGibHealth();
            var death = ChooseDeathState(gibHealth, out var extremeDeath);
            if (this is PlayerPawn deadPlayer) deadPlayer.ExtremelyDead = extremeDeath;
            if (extremeDeath && this is not PlayerPawn && _health >= gibHealth)
                _health = gibHealth - 1;
            if (States.HasState(death))
            {
                States.Enter(this, death);
                if (executeDeathSpecial && States.RemainingTics > 1 && Simulation is { } timingSimulation)
                    States.ForceRemainingTics(Math.Max(1, States.RemainingTics - timingSimulation.NextActorDieRandom()));
            }
            else if (this is not PlayerPawn) Destroy();
            if (executeDeathSpecial && this is PlayerPawn deadRumblePlayer)
            {
                PlayerDiedMakeRumble(DeathInflictor);
                deadRumblePlayer.BonusCount = 0;
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
    internal int DieFromSpawnState { get; set; } = -1;
    internal void DieFromSpawn() => SetHealth(0, null, executeDeathSpecial: true, forceDeath: true);
    internal int GenericCrushState { get; set; } = -1;
    public int CrushState { get; set; } = -1;
    public int CrashState { get; set; } = -1;
    public int ExtremeCrashState { get; set; } = -1;
    internal int NullState { get; set; } = -1;
    public int ActiveState { get; set; } = -1;
    public int InactiveState { get; set; } = -1;
    /// <summary>Native See state. A waking monster in <see cref="SpawnState"/> can enter this frame. -1 means absent.</summary>
    public int SeeState { get; set; } = -1;
    public int SlamState { get; set; } = -1;
    public int IdleState { get; set; } = -1;
    public int HealState { get; set; } = -1;
    public void SetIdle(bool noFunction = false)
        => States.Enter(this, States.HasState(IdleState) ? IdleState : SpawnState, noFunction);
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
    /// <summary>Native inflictor pain-type override; None preserves the hit's type.</summary>
    public string? PainType { get; set; }
    /// <summary>Native MF5_SPECIALFIREDAMAGE. Player flame death requires shallow overkill and damage above 25.</summary>
    public bool SpecialFireDamage { get; set; }
    /// <summary>Native persistent actor DamageType, also used by death-state selection.</summary>
    public string? DamageType { get; set; }
    internal int DeathDamageAmount { get; set; }
    /// <summary>Damage type of the hit currently being applied. The setter consumes it.</summary>
    internal string? DamageTypeReceived { get; set; }
    internal string? PainDamageTypeContext { get; set; }
    public string? DeathDamageType { get; internal set; }
    /// <summary>Inflictor for the hit currently being applied. The health setter consumes it.</summary>
    internal Actor? DeathInflictor { get; set; }
    private readonly Dictionary<string, int> _typedDeaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedExtremeDeaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedCrashes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedExtremeCrashes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedPain = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedPainChance = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _typedWounds = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Native <c>WoundHealth</c>. A surviving hit at or below this health can enter a typed wound frame.</summary>
    public int WoundHealth { get; set; }
    /// <summary>Native GetGibHealth. A killing blow below this value can enter <see cref="ExtremeDeathState"/>.</summary>
    public int GibHealth { get; set; } = -100;
    internal int DefaultGibHealth { get; set; } = -100;
    public virtual int GetGibHealth() => GibHealth == int.MinValue
        ? -(int)Math.Clamp(SpawnHealth() * (Simulation?.GibFactor ?? 1), 0, int.MaxValue)
        : -Math.Abs(GibHealth);
    /// <summary>Native class metadata; negative values select the default quarter-height fallback.</summary>
    public virtual double DeathHeight => -1;
    /// <summary>Native class metadata for Fire deaths; negative values fall back to DeathHeight.</summary>
    public virtual double BurnHeight => -1;
    public virtual Fixed GetDeathHeight()
    {
        var height = string.Equals(DamageType, "Fire", StringComparison.OrdinalIgnoreCase) ? BurnHeight : -1;
        if (height < 0) height = DeathHeight;
        return Fixed.FromDouble(height < 0 ? Height.ToDouble() * 0.25 : Math.Max(height, 0));
    }
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
    public int Accuracy { get; set; }
    public int FriendlySeeBlocks { get; set; } = 10;
    public int Stamina { get; set; }
    /// <summary>HealthPickup healing copied to a tossed pickup; null uses the class default.</summary>
    public int? PickupHealthOverride { get; set; }
    public int? PickupAutoUseOverride { get; set; }
    public bool PickupQuiet { get; set; }
    public bool NoScreenFlash { get; set; }
    public bool NoAttenPickupSound { get; set; }
    public int PickupDelay { get; internal set; }
    public bool SuppressWeaponPickupAmmo { get; internal set; }
    public bool? AlwaysPickupOverride { get; internal set; }
    public int DeathCount { get; private set; }
    internal void RestoreDamageHistory(SimActorDamageHistory? saved)
    {
        DeathCount = saved?.DeathCount ?? 0;
        LastDamageSourceId = saved is { SourceId: not 0 } history ? history.SourceId : null;
    }
    public uint? LastDamageSourceId { get; internal set; }
    public uint? LastHeardTargetId { get; internal set; }
    /// <summary>Mover crush on this tic. Suppresses duplicate sector pinch crush the same tic.</summary>
    internal int MoverCrushTic { get; private set; }
    internal void MarkMoverCrush(int tic) => MoverCrushTic = tic;
    internal void RestoreHealth(int health) => _health = health;
    public Fixed Radius { get; set; } = Fixed.FromInt(20);
    public Fixed Height { get; set; } = Fixed.FromInt(56);
    public PlayLevel? Level { get; init; }

    public virtual void PlayerPushedSomethingMakeRumble(Actor thing)
    {
        if (Simulation is { WorldHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "misc/push", LocalPlayerOrCameraOnly: true));
    }

    public virtual void PlayerWasPushedMakeRumble(Actor source)
    {
        if (Simulation is { WorldHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "misc/pushed", LocalPlayerOrCameraOnly: true, SuppressIfLocalActorId: source.Id));
    }

    public virtual void PlayerUsedSomethingMakeRumble(int activationType, int levelNum, int lineNum, int lineSpecial)
    {
        if (Simulation is { ActionHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "*usesuccess", LocalPlayerOrCameraOnly: true));
    }

    public virtual void PlayerTeleportedMakeRumble()
    {
        if (this is PlayerPawn && Simulation is { WorldHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "misc/teleport", LocalPlayerOrCameraOnly: true));
    }

    public virtual void PlayerHurtMakeRumble(Actor? source = null)
    {
        if (this is PlayerPawn && Simulation is { DamageHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "*pain"));
    }

    public virtual void PlayerDiedMakeRumble(Actor? source = null)
    {
        if (this is PlayerPawn && Simulation is { DamageHapticsEnabled: true } sim)
            sim.RequestRumble(new(Id, "*death"));
    }

    public virtual void PlayerLandedMakeRumble(Actor? onMobj = null)
    {
        if (this is not PlayerPawn player) return;
        var sim = Simulation ?? throw new InvalidOperationException("Landing rumble requires an attached simulation.");
        if (!sim.WorldHapticsEnabled) return;
        var liquid = Z.ToDouble() <= sim.FloorOf(SectorIndex)
            && sim.Level.FloorTerrainDefinition(SectorIndex) is { IsLiquid: true };
        if (onMobj is not null || !liquid) sim.RequestRumble(new(Id, "*land"));
        else if (VelocityZ.ToDouble() < -player.GruntSpeed) sim.RequestRumble(new(Id, "*grunt"));
    }

    /// <summary>Native landing sound selection for the represented unmorphed player body.</summary>
    public virtual void PlayerLandedMakeGruntSound(Actor? onMobj = null)
    {
        if (Destroyed || Health <= 0 || this is not PlayerPawn player) return;
        var grunted = VelocityZ.ToDouble() < -player.GruntSpeed;
        if (grunted) ActorSoundActions.StartSound(this, "*grunt", 2, SimulationSoundFlags.NoRumble);
        var liquid = Simulation is { } sim && Z.ToDouble() <= sim.FloorOf(SectorIndex)
            && sim.Level.FloorTerrainDefinition(SectorIndex) is { IsLiquid: true };
        if (onMobj is not null || !liquid)
        {
            if (grunted) ActorSoundActions.StartSoundIfNotSame(this, "*land", "*grunt", 0, SimulationSoundFlags.NoRumble);
            else ActorSoundActions.StartSound(this, "*land", 0, SimulationSoundFlags.NoRumble);
        }
    }

    /// <summary>Native landing view impulse for the represented player's active body.</summary>
    public virtual void PlayerSquatView(Actor? onMobj = null)
    {
        if (this is PlayerPawn player) player.DeltaViewHeight = VelocityZ.ToDouble() / 8;
    }

    /// <summary>Native corpse landing dispatch for represented ordinary, extreme and damage-typed Crash labels.</summary>
    public virtual void Crash()
    {
        if (Destroyed || DontCorpse || !(Corpse || Killed) || Crashed || IceCorpse) return;
        var extreme = Health < GetGibHealth();
        var state = -1;
        if (!string.IsNullOrEmpty(DamageType) && !DamageType.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            if (extreme && _typedExtremeCrashes.TryGetValue(DamageType, out var typedExtreme)
                && States.HasState(typedExtreme)) state = typedExtreme;
            if (state < 0 && _typedCrashes.TryGetValue(DamageType, out var typed)
                && States.HasState(typed)) state = typed;
        }
        if (state < 0) state = extreme && States.HasState(ExtremeCrashState) ? ExtremeCrashState : CrashState;
        if (States.HasState(state)) States.Enter(this, state);
        Crashed = true;
    }

    public bool IsDead => Health <= 0;
    /// <summary>Native AActor.Massacre: attempt repeated Massacre damage and report whether the actor died.</summary>
    public bool Massacre() => ThingDamage.Massacre(this);
    /// <summary>Native GiveBody: negative amounts raise health to a percentage of the resolved maximum.</summary>
    public bool GiveBody(int amount, int maximum = 0) => this is PlayerPawn player
        ? PickupCatalog.GiveHealth(player, amount, maximum)
        : PickupCatalog.GiveHealth(this, amount);

    public bool BlocksActors =>
        Solid && !IsDead && !Destroyed && DoomEdNum != LineSpecials.TeleportDestType
        && DoomEdNum != InvasionDirector.SpawnSpotType;

    public bool CanTakeDamage => (Shootable || Vulnerable) && !IsDead && !Destroyed
        && DoomEdNum != LineSpecials.TeleportDestType && DoomEdNum != InvasionDirector.SpawnSpotType;

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

    /// <summary>Configure a Crash.Type or Crash.Extreme.Type label on the managed actor class.</summary>
    public void SetTypedCrash(string damageType, int state, bool extreme = false)
    {
        if (string.IsNullOrEmpty(damageType) || damageType.Equals("None", StringComparison.OrdinalIgnoreCase)
            || damageType.Equals("Extreme", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("A typed crash needs a damage type other than None or Extreme.", nameof(damageType));
        (extreme ? _typedExtremeCrashes : _typedCrashes)[damageType] = state;
    }

    internal IEnumerable<(string Type, int State, bool Extreme)> TypedCrashes()
    {
        foreach (var entry in _typedCrashes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            yield return (entry.Key, entry.Value, false);
        foreach (var entry in _typedExtremeCrashes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
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
    /// Electric flicker uses <see cref="AuthoritySimulation.NextFlickerRandom"/>. Poison cloud howling uses the independent PoisonDamage stream.
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
    public double AttackOffset(double offset = 0) => this is PlayerPawn player
        ? (player.AttackZOffset.ToDouble() + offset) * player.CrouchFactor : 8 + offset;
    public double AttackOriginZ(double offset = 0) => Center() - FloorClip + AttackOffset(offset);
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
        if (ActorFreezeTickGate.TryConsumeFreezeTick(this))
            return;
        if (!CheckNoDelay()) return;
        if (PendingSpawnDeath)
        {
            if (States.RemainingTics != -1)
            {
                States.ForceRemainingTics(unchecked(States.RemainingTics - 1));
                if (States.RemainingTics <= 0)
                {
                    PendingSpawnDeath = false;
                    SetHealth(0, null, executeDeathSpecial: true, forceDeath: true);
                }
            }
        }
        else States.Tick(this);
        if (!Destroyed && PickupDelay > 0 && --PickupDelay == 0)
        {
            SpecialPickup = PickupCatalog.IsPickup(DoomEdNum, Simulation?.RavenHealthPickups ?? false);
            Solid = false;
        }
        if (!Destroyed && Simulation != null)
        {
            if (!PendingSpawnDeath)
            {
                if (!Dormant) Brain?.Tick(Simulation, this, nativeMovement: Simulation.NativeMonsterMovement);
                else if (!IsDead) Brain?.AdvanceRaiseFrame();
            }
            TickMovement(Simulation);
            if (VelocityX.Raw == 0 && VelocityY.Raw == 0) Blasted = false;
            SectorDamage.Tick(Simulation, this);
            TerrainDamage.Tick(Simulation, this);
        }
        if (!Destroyed && VisibilityPulse)
        {
            if (VisibilityDirection > 0)
            {
                Alpha += 1.0 / 32;
                if (Alpha >= 1) { Alpha = 1; VisibilityDirection = -1; }
            }
            else
            {
                Alpha -= 1.0 / 32;
                if (Alpha <= 0.25) { Alpha = 0.25; VisibilityDirection = 1; }
            }
        }
        else if (!Destroyed && Stealth)
        {
            RenderStyle = RenderStyle with { Flags = (byte)(RenderStyle.Flags & ~2) };
            if (VisibilityDirection > 0)
            {
                Alpha += 2.0 / GameTicClock.TicRate;
                if (Alpha > 1) { Alpha = 1; VisibilityDirection = 0; }
            }
            else if (VisibilityDirection < 0)
            {
                Alpha -= 1.5 / GameTicClock.TicRate;
                if (Alpha < StealthAlpha) { Alpha = StealthAlpha; VisibilityDirection = 0; }
            }
        }
        base.Tick();
    }

    protected virtual void TickMovement(AuthoritySimulation sim) => ActorPhysics.Step(sim, this);
}

public class PlayerPawn : Actor
{
    public bool NoClip2 { get; set; }
    public bool Frozen { get; set; }
    public bool TotallyFrozen { get; set; }
    /// <summary>Native CF_NOTARGET; unset overrides retain the legacy shared NoTarget policy.</summary>
    public bool NoTargetCheat { get => NoTargetCheatOverride ?? NoTarget; set => NoTargetCheatOverride = value; }
    internal bool? NoTargetCheatOverride { get; set; }
    public int ExtraLight { get; set; }
    public int BonusCount { get; internal set; }
    public int PowerStrengthTics { get; internal set; }
    public bool HasPowerStrength => PowerStrengthTics != 0;
    public void GivePowerStrength() => PowerStrengthTics = 1;
    public uint GetStrengthBlend()
    {
        if (!HasPowerStrength) return 0;
        var count = 128 - (PowerStrengthTics >> 3);
        return count > 0 ? ((uint)(127 * count / 256) << 24) | 0x00ff0000u : 0;
    }
    public string? LastDamageType { get; set; }
    public override int GetMaxHealth(bool withUpgrades) => withUpgrades
        ? checked(EffectiveMaxHealth + Stamina + BonusHealth) : EffectiveMaxHealth;
    public string? SoundClass { get; set; }
    /// <summary>Native status bar mugshot state from <c>PCD_SETMUGSHOTSTATE</c>.</summary>
    public string? MugshotState { get; set; }
    public int BonusHealth { get; set; }
    public int MaxPickupHealth { get; set; }
    public override void BeginPlay()
    {
        base.BeginPlay();
        FullHeight = Height.ToDouble();
    }

    public PlayerPawn()
    {
        Friendly = true;
        Inventory = new PlayerInventory(this);
        CanSlide = true;
        CanPass = true;
        DefaultTeleStomp = TeleStomp = true;
        MovementSpeed = Fixed.FromInt(1);
        CanPickupItems = true;
        SpawnCanPickupItems = true;
        AllowDropOff = true;
        NoBlockMonsters = true;
    }

    public int TeleportFreezeTime { get; set; } = 18;
    public Func<PlayerPawn, int>? TeleportFreezeTimeFunction { get; set; }
    public virtual int GetTeleportFreezeTime()
        => TeleportFreezeTimeFunction?.Invoke(this) ?? Math.Max(0, TeleportFreezeTime);

    public const double CrouchSpeed = 1.0 / 12;
    public const double MinimumCrouchFactor = 0.5;
    public const double StandingViewHeight = 41;
    public const double DefaultGruntSpeed = 12;
    public double GruntSpeed { get; set; } = DefaultGruntSpeed;
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
    internal double EffectiveDrainStrength => PowerDrainTics > 0 ? PowerDrainStrength : DrainStrength;

    /// <summary>Height before crouch. <c>BeginPlay</c> copies the spawned height.</summary>
    public double FullHeight { get; internal set; }
    /// <summary>Native player.crouchfactor. 1 is standing. 0.5 is fully crouched.</summary>
    public double CrouchFactor { get; private set; } = 1;
    /// <summary>Native player.viewheight. Standing view is 41, scaled by <see cref="CrouchFactor"/>.</summary>
    public double ViewHeight { get; internal set; } = StandingViewHeight;
    public double DeltaViewHeight { get; internal set; }
    public double GetDeltaViewHeight() => (DefaultViewHeight.ToDouble() * CrouchFactor - ViewHeight) / 8;
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
    /// <summary>Represented CalcHeight eye position, without advancing view height or bob.</summary>
    public double GetViewZ()
    {
        var sim = Simulation ?? throw new InvalidOperationException("Player eye height requires an attached simulation.");
        var floor = sim.FloorOf(SectorIndex);
        var viewZ = Z.ToDouble() + ViewHeight + ViewBobOffset;
        if (!IsDead && Z.ToDouble() <= floor) viewZ -= FloorClip;
        viewZ = Math.Min(viewZ, sim.CeilingOf(SectorIndex) - 4);
        return Math.Max(viewZ, floor + 4);
    }
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
    internal void RestoreTurn(int ticks, bool held) { TurnTicks = ticks; _turnHeld = held; }
    public bool WeaponReady => Inventory.Selected != 0 && WeaponOffsetY == WeaponTop && !WeaponLowering;

    public const int CommandQueueCapacity = 128;
    private readonly Queue<PlayerCommand> _commands = new();
    public byte PlayerNum { get; init; }
    /// <summary>Native <c>air_finished</c> map tic; ACS supply is <see cref="AirFinishedTic"/> minus current tic.</summary>
    internal int AirFinishedTic { get; set; }
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
    public int PowerMaskTics { get; set; }
    public bool HasIronFeetProtection => PowerIronFeetTics > 0 || PowerMaskTics > 0;
    internal IronFeetMode SectorProtectionMode => PowerIronFeetTics > 0 && PowerIronFeetMode == IronFeetMode.Full
        ? IronFeetMode.Full : PowerMaskTics > 0 ? IronFeetMode.Normal : PowerIronFeetMode;
    public void GivePowerMask(int durationTics = 80 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerMaskTics = PowerupDuration.Merge(PowerMaskTics, durationTics, additiveTime, alwaysPickup);

    public int PowerIronFeetTics { get; set; }
    public IronFeetMode PowerIronFeetMode { get; set; }
    public void GivePowerIronFeet(int durationTics = 60 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerIronFeetTics = PowerupDuration.Merge(PowerIronFeetTics, durationTics, additiveTime, alwaysPickup);

    public int PowerInfiniteAmmoTics { get; set; }
    public int PowerDrainTics { get; set; }
    public double PowerDrainStrength { get; internal set; } = 0.5;
    public int PowerProsperityTics { get; set; }
    public int PowerSpreadTics { get; set; }
    public int PowerRegenerationTics { get; set; }
    public double PowerRegenerationStrength { get; internal set; } = 5;
    public int PowerDoubleFiringSpeedTics { get; set; }
    public int PowerHighJumpTics { get; set; }
    public bool PowerHighJumpPendingRemoval { get; internal set; }
    public double PowerHighJumpStrength { get; internal set; } = 2;
    public bool HasPowerHighJump => PowerHighJumpTics > 0 || PowerHighJumpPendingRemoval;
    public bool HasInfiniteAmmo => Simulation?.InfiniteAmmo == true || PowerInfiniteAmmoTics > 0;

    public void GivePowerInfiniteAmmo(int durationTics = 30 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerInfiniteAmmoTics = PowerupDuration.Merge(PowerInfiniteAmmoTics, durationTics, additiveTime, alwaysPickup);

    public void GivePowerHighJump(int durationTics = 0, bool additiveTime = false, bool alwaysPickup = false, double strength = 2)
    {
        if (!double.IsFinite(strength)) throw new ArgumentOutOfRangeException(nameof(strength));
        var merged = PowerupDuration.Merge(PowerHighJumpTics, durationTics, additiveTime, alwaysPickup);
        if (!HasPowerHighJump) PowerHighJumpStrength = strength;
        if (durationTics == 0 && !HasPowerHighJump) PowerHighJumpPendingRemoval = true;
        PowerHighJumpTics = merged;
        if (merged > 0) PowerHighJumpPendingRemoval = false;
    }

    public void GivePowerDoubleFiringSpeed(int durationTics = 40 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerDoubleFiringSpeedTics = PowerupDuration.Merge(PowerDoubleFiringSpeedTics, durationTics, additiveTime, alwaysPickup);

    public void GivePowerProsperity(int durationTics = 120 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerProsperityTics = PowerupDuration.Merge(PowerProsperityTics, durationTics, additiveTime, alwaysPickup);
    public void GivePowerSpread(int durationTics = 30 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false) =>
        PowerSpreadTics = PowerupDuration.Merge(PowerSpreadTics, durationTics, additiveTime, alwaysPickup);

    public void GivePowerRegeneration(int durationTics = 120 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false, double strength = 5)
    {
        if (!double.IsFinite(strength) || strength < int.MinValue || strength > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(strength));
        var merged = PowerupDuration.Merge(PowerRegenerationTics, durationTics, additiveTime, alwaysPickup);
        if (PowerRegenerationTics == 0 && merged > 0) PowerRegenerationStrength = strength;
        PowerRegenerationTics = merged;
    }

    public void GivePowerDrain(int durationTics = 60 * GameTicClock.TicRate,
        bool additiveTime = false, bool alwaysPickup = false, double strength = 0.5)
    {
        if (!double.IsFinite(strength)) throw new ArgumentOutOfRangeException(nameof(strength));
        var merged = PowerupDuration.Merge(PowerDrainTics, durationTics, additiveTime, alwaysPickup);
        if (PowerDrainTics == 0 && merged > 0) PowerDrainStrength = strength;
        PowerDrainTics = merged;
    }

    internal void ClearDamagePowers()
    {
        PowerBuddhaTics = 0;
        PowerDamageTics = 0;
        PowerProtectionTics = 0;
        PowerMaskTics = 0;
        PowerIronFeetTics = 0;
        PowerInfiniteAmmoTics = 0;
        PowerDrainTics = 0;
        PowerDrainStrength = 0.5;
        PowerRegenerationTics = 0;
        PowerProsperityTics = 0;
        PowerSpreadTics = 0;
        PowerStrengthTics = 0;
        PowerRegenerationStrength = 5;
        PowerDoubleFiringSpeedTics = 0;
        PowerHighJumpTics = 0;
        PowerHighJumpPendingRemoval = false;
        PowerHighJumpStrength = 2;
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
            case ManagedPowerupKind.Mask: GivePowerMask(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.IronFeet: GivePowerIronFeet(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Damage: GivePowerDamage(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Protection: GivePowerProtection(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Buddha: GivePowerBuddha(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.InfiniteAmmo: GivePowerInfiniteAmmo(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Drain: GivePowerDrain(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Regeneration: GivePowerRegeneration(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.DoubleFiringSpeed: GivePowerDoubleFiringSpeed(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.HighJump: GivePowerHighJump(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Prosperity: GivePowerProsperity(duration, additiveTime, alwaysPickup); break;
            case ManagedPowerupKind.Spread: GivePowerSpread(duration, additiveTime, alwaysPickup); break;
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
    private int _originalButtons, _modifiedButtons, _originalOldButtons, _modifiedOldButtons;
    private short _originalPitch, _originalYaw, _originalForward, _originalSide, _originalUp;
    private short _modifiedPitch, _modifiedYaw, _modifiedForward, _modifiedSide, _modifiedUp;

    internal int ReadPlayerInput(int input) => input switch
    {
        0 => _originalOldButtons,
        1 => _originalButtons,
        2 => _originalPitch,
        3 => _originalYaw,
        4 => 0,
        5 => _originalForward,
        6 => _originalSide,
        7 => _originalUp,
        8 => _modifiedOldButtons,
        9 => _modifiedButtons,
        10 => _modifiedPitch,
        11 => _modifiedYaw,
        12 => 0,
        13 => _modifiedForward,
        14 => _modifiedSide,
        15 => _modifiedUp,
        _ => 0,
    };

    internal SimPlayerInput CapturePlayerInput() => new(
        _originalButtons, _modifiedButtons, _originalOldButtons, _modifiedOldButtons,
        _originalPitch, _originalYaw, _originalForward, _originalSide, _originalUp,
        _modifiedPitch, _modifiedYaw, _modifiedForward, _modifiedSide, _modifiedUp);

    internal void RestorePlayerInput(SimPlayerInput saved)
    {
        _originalButtons = saved.OriginalButtons;
        _modifiedButtons = saved.ModifiedButtons;
        _originalOldButtons = saved.OriginalOldButtons;
        _modifiedOldButtons = saved.ModifiedOldButtons;
        _originalPitch = saved.OriginalPitch;
        _originalYaw = saved.OriginalYaw;
        _originalForward = saved.OriginalForward;
        _originalSide = saved.OriginalSide;
        _originalUp = saved.OriginalUp;
        _modifiedPitch = saved.ModifiedPitch;
        _modifiedYaw = saved.ModifiedYaw;
        _modifiedForward = saved.ModifiedForward;
        _modifiedSide = saved.ModifiedSide;
        _modifiedUp = saved.ModifiedUp;
    }

    private void RememberPlayerInput(PlayerCommand incoming, PlayerCommand modified)
    {
        _originalOldButtons = _originalButtons;
        _modifiedOldButtons = _modifiedButtons;
        _originalButtons = EncodeButtons(incoming);
        _modifiedButtons = EncodeButtons(modified);
        if (modified.Jump) _modifiedButtons &= ~(1 << 3);
        _originalPitch = incoming.PitchDelta;
        _originalYaw = incoming.YawDelta;
        _originalForward = incoming.ForwardMove;
        _originalSide = incoming.SideMove;
        _originalUp = incoming.UpMove;
        _modifiedPitch = modified.PitchDelta;
        _modifiedYaw = modified.YawDelta;
        _modifiedForward = modified.ForwardMove;
        _modifiedSide = modified.SideMove;
        _modifiedUp = modified.UpMove;
    }

    private static int EncodeButtons(PlayerCommand command)
    {
        var buttons = 0;
        if (command.Attack) buttons |= 1 << 0;
        if (command.Use) buttons |= 1 << 1;
        if (command.Jump) buttons |= 1 << 2;
        if (command.Crouch) buttons |= 1 << 3;
        if (command.Turn180) buttons |= 1 << 4;
        return buttons;
    }
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
    internal PlayerCommand[] CaptureBufferedCommands() => _commands.ToArray();
    internal void RestoreBufferedCommands(PlayerCommand[] commands)
    {
        _commands.Clear();
        foreach (var command in commands) _commands.Enqueue(command);
    }
    public PlayerInventory Inventory { get; }
    public bool GodMode { get; set; }
    /// <summary>Native PlayerPawn.GetPainFlash color hook, encoded as ARGB.</summary>
    public virtual uint DamageFade => 0xffff0000u;
    protected virtual IReadOnlyList<PlayerPainFlash> PainFlashes => Array.Empty<PlayerPainFlash>();
    public virtual uint GetPainFlash()
    {
        var flashes = PainFlashes;
        for (var index = flashes.Count - 1; index >= 0; index--)
        {
            var flash = flashes[index];
            if (string.Equals(flash.DamageType, LastDamageType ?? "None", StringComparison.OrdinalIgnoreCase)
                && flash.PlayerClass.IsAssignableFrom(GetType()))
                return flash.Color == 0 ? DamageFade : flash.Color;
        }
        return DamageFade;
    }
    private int _damageCount;
    private int _poisonCount;
    public uint? PoisonSourceId { get; internal set; }
    public string? PoisonDamageType { get; internal set; }
    public string? PoisonPainType { get; internal set; }
    public bool ApplyPoison(int amount, Actor? poisoner = null, Actor? source = null)
    {
        if (GodMode || GodMode2 || Invulnerable || NoDamage) return false;
        if (amount > 0)
        {
            PoisonCount = (int)Math.Min(100L, (long)PoisonCount + amount);
            PoisonSourceId = source?.Id;
            PoisonDamageType = poisoner?.DamageType;
            PoisonPainType = string.IsNullOrEmpty(poisoner?.PainType)
                || string.Equals(poisoner.PainType, "None", StringComparison.OrdinalIgnoreCase)
                ? PoisonDamageType : poisoner.PainType;
        }
        return true;
    }
    public int PoisonCount
    {
        get => _poisonCount;
        set => _poisonCount = value is >= 0 and <= 100 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    public int DamageCount
    {
        get => _damageCount;
        internal set => _damageCount = value is >= 0 and <= 100 ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }
    /// <summary>Native CF_GODMODE2. Protects against forced hits and telefrags.</summary>
    public bool GodMode2 { get; set; }
    public bool AttackPressed { get; set; }
    public bool UsePressed { get; set; }
    public bool UseHeld { get; internal set; }
    /// <summary>Native Player.UseRange. The use trace follows yaw for this distance.</summary>
    private double _useRange = 64;
    public double UseRange { get => _useRange; set { _useRange = value; HasUseRangeOverride = true; } }
    internal bool HasUseRangeOverride { get; set; }
    public int WeaponCooldown { get; internal set; }
    public bool WeaponRefiring { get; internal set; }
    /// <summary>First tic on which a press may respawn. <see cref="int.MaxValue"/> until the player dies.</summary>
    public int RespawnEarliestTic { get; internal set; } = int.MaxValue;
    /// <summary>Set by a rising use or attack edge while dead. Cleared when the respawn is taken.</summary>
    public bool RespawnArmed { get; internal set; }
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
        sim.StartPlayerScripts(death: true, this);
        var tic = sim.Thinkers.Clock.Tic;
        RespawnEarliestTic = tic > int.MaxValue - GameTicClock.TicRate ? int.MaxValue : tic + GameTicClock.TicRate;
        sim.DropSelectedWeapon(this);
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
        if (DeltaViewHeight == 0) ViewHeight = DefaultViewHeight.ToDouble() * CrouchFactor;
        else ViewHeight += DefaultViewHeight.ToDouble() * (CrouchFactor - before);
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
        if (!IsDead) Height = Fixed.FromDouble(FullHeight);
    }

    public override void Tick()
    {
        HasMovementInput = false;
        Inventory.AbsorbCount = 0;
        if (PowerBuddhaTics > 0) PowerBuddhaTics--;
        if (PowerMaskTics > 0)
        {
            if (--PowerMaskTics > 0 && !IsDead && Simulation is { } maskSim
                && (maskSim.Thinkers.Clock.Tic & 63) == 0)
                ActorSoundActions.StartSound(this, "misc/mask", channel: 0);
        }
        if (PowerIronFeetTics > 0) PowerIronFeetTics--;
        if (PowerInfiniteAmmoTics > 0) PowerInfiniteAmmoTics--;
        if (PowerDrainTics > 0 && --PowerDrainTics == 0) PowerDrainStrength = 0.5;
        if (PowerRegenerationTics > 0)
        {
            if (Simulation is { } regenerationSim && (regenerationSim.Thinkers.Clock.Tic & 31) == 0
                && PickupCatalog.GiveHealth(this, (int)PowerRegenerationStrength))
                ActorSoundActions.StartSound(this, "*regenerate", channel: 3);
            if (--PowerRegenerationTics == 0) PowerRegenerationStrength = 5;
        }
        if (PowerDamageTics > 0) PowerDamageTics--;
        if (PowerProtectionTics > 0) PowerProtectionTics--;
        if (PowerProsperityTics > 0) PowerProsperityTics--;
        if (PowerSpreadTics > 0) PowerSpreadTics--;
        BobTimer++;
        RememberPosition();
        if (WeaponCooldown > 0)
        {
            WeaponCooldown--;
            if (WeaponCooldown > 0 && PowerDoubleFiringSpeedTics > 0) WeaponCooldown--;
        }
        if (PowerDoubleFiringSpeedTics > 0) PowerDoubleFiringSpeedTics--;
        if (HasPowerStrength) PowerStrengthTics = unchecked(PowerStrengthTics + 1);
        if (IsDead)
        {
            JustAttacked = false;
            WeaponRefiring = false;
            AttackPressed = false;
            ViewBobOffset = 0;
            MovementBob = 0;
            WeaponBobX = 0;
            WeaponBobY = 0;
            Uncrouch();
            WatchKiller();
            Simulation?.TryPlayerRespawn(this);
            PowerHighJumpPendingRemoval = false;
            if (PowerHighJumpTics > 0) PowerHighJumpTics--;
            if (!HasPowerHighJump) PowerHighJumpStrength = 2;
            base.Tick();
            return;
        }
        var incoming = _commands.TryDequeue(out var queued) ? queued : default;
        var command = incoming;
        if (NoClip2)
        {
            if (!NoClip) { NoClip2 = false; if (!Fly) NoGravity = false; }
            else { NoGravity = true; OnGround = true; }
        }
        if (JustAttacked)
        {
            command = command with { YawDelta = 0, ForwardMove = 0x6400, SideMove = 0 };
            JustAttacked = false;
        }
        if (TotallyFrozen)
        {
            command = new PlayerCommand { Use = command.Use };
            TurnTicks = 0;
        }
        else if (Frozen)
            command = command with { ForwardMove = 0, SideMove = 0, UpMove = 0 };
        RememberPlayerInput(incoming, command);
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
                VelocityZ = Fixed.FromDouble(VelocityZ.ToDouble() + JumpZ.ToDouble() * (HasPowerHighJump && PowerHighJumpStrength > 0 ? PowerHighJumpStrength : 1));
                OnMobj = false;
                JumpTics = -1;
                OnGround = false;
                ActorSoundActions.StartSound(this, "*jump", channel: 4);
            }
            if (command.UpMove == short.MinValue)
            {
                if (NoGravity) NoGravity = false;
            }
            else if (command.UpMove != 0 && (Fly || NoClip2))
            {
                var upMove = Math.Clamp((int)command.UpMove, -768, 768);
                VelocityZ = Fixed.FromDouble(MovementSpeed.ToDouble() * upMove / 128.0);
                NoGravity = true;
            }
        }

        PowerHighJumpPendingRemoval = false;
        if (PowerHighJumpTics > 0) PowerHighJumpTics--;
        if (!HasPowerHighJump) PowerHighJumpStrength = 2;
        base.Tick();
        AdvanceViewHeight();
        UpdateViewBob();
        if (!IsDead && DamageCount > 0) DamageCount--;
        if (!IsDead && BonusCount > 0) BonusCount--;
        if (!IsDead) CheckPoison();
    }

    public virtual void CheckPoison()
    {
        if (IsDead || PoisonCount == 0 || Simulation is not { } simulation || (simulation.Thinkers.Clock.Tic & 15) != 0) return;
        PoisonCount = Math.Max(0, PoisonCount - 5);
        var source = PoisonSourceId is { } id ? simulation.Actors.FirstOrDefault(actor => actor.Id == id && !actor.Destroyed) : null;
        ActorDamage.ApplyPoisonDamage(this, 1, source);
    }

    /// <summary>Advance the represented live-player CalcHeight landing impulse once per tic.</summary>
    internal void AdvanceViewHeight()
    {
        if (!IsDead && DeltaViewHeight != 0)
        {
            var target = DefaultViewHeight.ToDouble() * CrouchFactor;
            ViewHeight += DeltaViewHeight;
            if (ViewHeight > target) { ViewHeight = target; DeltaViewHeight = 0; }
            else if (ViewHeight < target / 2)
            {
                ViewHeight = target / 2;
                if (DeltaViewHeight <= 0) DeltaViewHeight = 1.0 / 65536;
            }
            if (DeltaViewHeight != 0)
            {
                DeltaViewHeight += 0.25;
                if (DeltaViewHeight == 0) DeltaViewHeight = 1.0 / 65536;
            }
        }
    }

    /// <summary>Recalculate movement and weapon bob without advancing the landing impulse.</summary>
    public void UpdateViewBob()
    {
        var speed = VelocityX.ToDouble() * VelocityX.ToDouble() + VelocityY.ToDouble() * VelocityY.ToDouble();
        MovementBob = NoClip2 || speed <= 0 ? 0 : Math.Min(MaxBob, speed * MoveBob);
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
        var attacker = LastDamageSourceId is uint sourceId && Simulation != null
            ? Simulation.Actors.FirstOrDefault(actor => actor.Id == sourceId) : null;
        if (attacker == null || ReferenceEquals(attacker, this))
        {
            if (DamageCount > 0) DamageCount--;
            if (PoisonCount > 0) PoisonCount--;
            return;
        }
        var aim = Math.Atan2(attacker.Y.ToDouble() - Y.ToDouble(), attacker.X.ToDouble() - X.ToDouble()) * 180 / Math.PI;
        var diff = AimDelta(Angle.ToDegrees(), aim);
        if (Math.Abs(diff) < 10 && DamageCount > 0) DamageCount--;
        if (Math.Abs(diff) < 10 && PoisonCount > 0) PoisonCount--;
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
        WeaponRefiring = false;
        Inventory.Selected = weapon;
        Inventory.Pending = null;
        WeaponLowering = false;
        WeaponOffsetY = InstantWeaponSwitch ? WeaponTop : WeaponBottom - WeaponMoveSpeed;
    }

    private void AdvanceWeapon()
    {
        if (WeaponLowering) WeaponRefiring = false;
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
    internal static void ApplySoundDefaults(Actor actor, DehackedActor? defaults)
    {
        if (defaults is not null)
        {
            ApplyFullVolumeSoundDefaults(actor, defaults);
            if (defaults.SoundNameOverrides.Count != 0)
                actor.ActorSounds = defaults.SoundNameOverrides.ToDictionary(pair => (ActorSoundType)pair.Key, pair => pair.Value);
        }
        if (PickupCatalog.IsPickup(actor.ClassDoomEdNum) && !actor.ActorSounds.ContainsKey(ActorSoundType.Rip))
            actor.ActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds) { [ActorSoundType.Rip] = "misc/ripslop" };
        if (PickupCatalog.IsPickup(actor.ClassDoomEdNum))
            actor.SpawnActorSounds = new Dictionary<ActorSoundType, string>(actor.ActorSounds);
    }

    internal static void ApplyFullVolumeSoundDefaults(Actor actor, DehackedActor? defaults)
    {
        if (defaults is { FullVolumeSoundsPatched: true })
        {
            actor.FullVolumeDeath = defaults.FullVolumeSounds;
            actor.FullVolumeSee = defaults.FullVolumeSounds;
            actor.DefaultFullVolumeDeath = defaults.FullVolumeSounds;
            actor.DefaultFullVolumeSee = defaults.FullVolumeSounds;
        }
    }
    public static IReadOnlyList<Actor> Spawn(
        PlayLevel level,
        ThinkerCollection thinkers,
        DehackedPatchResult? dehacked = null, SpawnOptions? spawnOptions = null, Func<uint>? mapSpawnRandom = null, Func<uint>? actorSpawnRandom = null)
    {
        var actors = new List<Actor>();
        uint nextId = 1;
        for (var thingIndex = 0; thingIndex < level.Things.Count; thingIndex++)
        {
            var thing = level.Things[thingIndex];
            if (thing.Type == 0 || thing.Type == DeathmatchStarts.ThingType
                || !(spawnOptions ?? new SpawnOptions()).Includes(thing))
                continue;

            var defaults = dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == thing.Type && actor.Patched);
            if (spawnOptions?.Mode == SpawnGameMode.Deathmatch
                && (defaults is not null ? (defaults.Bits & 0x02000000) != 0 : PickupCatalog.IsKey(thing.Type)))
                continue;

            var actor = CreateMapActor(level, thing, nextId++, dehacked, spawnOptions, mapSpawnRandom, actorSpawnRandom);
            if (actor.Brain is null && actor.SpecialPickup && PickupCatalog.IsPickup(thing.Type, spawnOptions?.RavenHealthPickups ?? false))
                actor.MapPickupThingIndex = thingIndex;
            else if (actor.Brain is null && !actor.SpecialPickup && DoomDecorationDefaults.IsDecoration(actor.ClassDoomEdNum))
                actor.MapDecorationThingIndex = thingIndex;
            if (actor is not PlayerPawn && actor.Brain is not null) actor.MapMonsterThingIndex = thingIndex;
            thinkers.Add(actor, actor is PlayerPawn ? ThinkerStat.Player : ThinkerStat.Default);
            actors.Add(actor);
        }

        return actors;
    }

    internal static void InitializePlayerLookCursor(Actor actor, Func<uint> random)
    {
        if (actor.IsMonster)
        {
            actor.LastLookPlayerNumber = (int)(random() & 63);
            actor.TidToHate = 0;
        }
        // SpawnActor stores a full byte; script setters intentionally accept only phases 0..63.
        actor.FloatBobPhase = (int)(random() & 255);
    }

    internal static Actor CreateMapActor(PlayLevel level, LevelThing thing, uint id,
        DehackedPatchResult? dehacked, SpawnOptions? spawnOptions, Func<uint>? mapSpawnRandom, Func<uint>? actorSpawnRandom = null)
    {
        var defaults = Actor.IsPlayerStart(thing.Type)
            ? dehacked?.Actors.FirstOrDefault(actor => actor.Index == 1 && actor.Patched)
            : dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == thing.Type && actor.Patched);
        var definitionType = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : thing.Type;
        var definition = DoomActorCatalog.Find(definitionType);
        var playerStart = Actor.IsPlayerStart(thing.Type);
        Actor actor = playerStart
            ? new PlayerPawn
            {
                PlayerNum = (byte)(thing.Type - 1),
                Id = id,
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
                Id = id,
                DoomEdNum = thing.Type,
                X = Fixed.FromDouble(thing.X),
                Y = Fixed.FromDouble(thing.Y),
                ThingId = thing.Id, SpawnZOffset = thing.Z,
                Angle = BamAngle.FromDegrees(thing.Angle),
                Health = defaults?.Health ?? definition?.Health
                    ?? (PickupCatalog.IsPickup(thing.Type, spawnOptions?.RavenHealthPickups ?? false) ? 1000 : 30),
                Radius = defaults == null && definition != null ? Fixed.FromInt(definition.Radius) : RadiusOf(defaults, player: false),
                Height = defaults == null && PickupCatalog.IsPickup(thing.Type, spawnOptions?.RavenHealthPickups ?? false) ? PickupCatalog.HeightOf(thing.Type)
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
        actor.SpecialPickup = PickupCatalog.IsPickup(actor.DoomEdNum, spawnOptions?.RavenHealthPickups ?? false);
        if (actor.SpecialPickup) actor.Solid = actor.Shootable = false;
        ApplyPrimaryFlags(actor, defaults, playerStart);
        DoomDecorationDefaults.Apply(actor, definitionType, defaults);
        DoomPickupStates.Apply(actor, definitionType);
        PatrolMarkers.Apply(actor, definitionType, defaults);
        actor.SpawnCanPickupItems = actor.CanPickupItems;
        actor.SpawnSpecialPickup = actor.SpecialPickup;
        actor.ResurrectionHealth = actor.Health;
        if (actor.ResurrectionHealth > 0) actor.GibHealth = int.MinValue;
        actor.DefaultGibHealth = actor.GibHealth;
        actor.Mass = defaults is { MassPatched: true } ? defaults.Mass : PatrolMarkers.IsMarker(definitionType) ? 10 : DoomActorCatalog.MassOf(definitionType);
        actor.RaiseDuration = ArchvileActions.RaiseDuration(definitionType);
        actor.Boss = definitionType is 7 or 16;
        ApplyExtendedDefaults(actor, defaults, spawnOptions?.FastMonsters ?? spawnOptions?.Skill == 4);
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
        ActorSpawner.ApplySoundDefaults(actor, defaults);
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
        if (actorSpawnRandom is not null) InitializePlayerLookCursor(actor, actorSpawnRandom);
        ThingActivation.InitializeSpawn(actor, thing.Dormant, mapSpawnRandom: mapSpawnRandom);
        if (thing.FloatBobPhase is >= 0 and < 64) actor.FloatBobPhase = thing.FloatBobPhase;
        if (thing.FriendlySeeBlocks > 0) actor.FriendlySeeBlocks = thing.FriendlySeeBlocks;
        if (thing.Score != 0) actor.Score = thing.Score;
        if (thing.Alpha >= 0) actor.Alpha = thing.Alpha;
        if (thing.RenderStyle is { } mapRenderStyle) actor.RenderStyle = ActorRenderStyle.FromNative(mapRenderStyle);
        if (thing.ScaleX != 0) actor.ScaleX *= thing.ScaleX;
        if (thing.ScaleY != 0) actor.ScaleY *= thing.ScaleY;
        if (thing.Health == 0)
        {
            if (playerStart) throw new NotSupportedException("Zero-health player starts are not supported.");
            actor.RestoreHealth(0);
            actor.PendingSpawnDeath = true;
            actor.States.ForceRemainingTics(1);
            actor.FullBright = false;
        }
        else if (thing.Health != 1)
        {
            var health = thing.Health > 0 ? actor.Health * thing.Health : -thing.Health;
            if (!double.IsFinite(health) || health < 1 || health > int.MaxValue)
                throw new InvalidDataException("Actor spawn health is outside the supported range.");
            actor.Health = (int)health;
            actor.ResurrectionHealth = actor.Health;
        }
        actor.RememberPosition();
        return actor;
    }

    internal static void ApplyPrimaryFlags(Actor actor, DehackedActor? defaults, bool playerStart)
    {
        actor.Stealth = defaults is { BitsPatched: true, BitsUseStealth: true } && (defaults.Bits & 0x40000000) != 0;
        actor.DefaultStealth = actor.Stealth;
        actor.DontFall = actor.ClassDoomEdNum == 3006;
        actor.CanPass = playerStart || DoomActorCatalog.Find(actor.ClassDoomEdNum) is not null;
        actor.Shadow = defaults is { BitsPatched: true } ? (defaults.Bits & 0x00040000) != 0 : actor.ClassDoomEdNum == 58;
        actor.DefaultShadow = actor.Shadow;
        actor.NoBlood = actor.DefaultNoBlood = defaults is { BitsPatched: true }
            ? (defaults.Bits & 0x00080000) != 0 : actor.ClassDoomEdNum == 2035;
        var nativeShadow = actor.ClassDoomEdNum == 58;
        actor.RenderStyle = nativeShadow ? ActorRenderStyle.OptFuzzy : ActorRenderStyle.Normal;
        actor.Alpha = nativeShadow ? 0.5 : 1;
        if (defaults?.RenderStyle is { } renderStyle) actor.RenderStyle = ActorRenderStyle.FromDehacked(renderStyle);
        if (defaults?.Alpha is { } alpha) actor.Alpha = alpha;
        if (defaults?.Invisible is { } invisible) actor.Invisible = invisible;
        actor.DefaultRenderStyle = actor.RenderStyle;
        actor.DefaultAlpha = actor.Alpha;
        if (defaults is { BitsPatched: true })
        {
            ApplyContactPrimaryFlags(actor, defaults, playerStart);
            ApplyPhysicalPrimaryFlags(actor, defaults.Bits);
        }
        else if (defaults is not null && PickupCatalog.IsPickup(defaults.OriginalDoomEdNum))
            ApplyPhysicalPrimaryFlags(actor, defaults.Bits);
    }

    internal static void ApplyPhysicalPrimaryFlags(Actor actor, uint bits)
    {
        actor.SpawnCeiling = (bits & 0x00000100) != 0;
        actor.NoClip = (bits & 0x00001000) != 0;
        actor.JustHit = (bits & 0x40) != 0;
        actor.JustAttacked = (bits & 0x80) != 0;
        actor.InFloat = (bits & 0x00200000) != 0;
        actor.Corpse = (bits & 0x00100000) != 0;
        actor.Solid = (bits & 0x00000002) != 0;
        actor.Shootable = (bits & 0x00000004) != 0;
        actor.NoBlockmap = (bits & 0x00000010) != 0;
        actor.NoSector = (bits & 0x00000008) != 0;
        actor.NoGravity = (bits & 0x00000200) != 0;
        actor.AllowDropOff = (bits & 0x00000400) != 0;
        actor.Floating = (bits & 0x00004000) != 0;
    }

    internal static void ApplyContactPrimaryFlags(Actor actor, DehackedActor defaults, bool playerStart)
    {
        actor.SpecialPickup = (defaults.Bits & 0x00000001) != 0;
        actor.CanPickupItems = (defaults.Bits & 0x00000800) != 0;
        actor.Dropped = (defaults.Bits & 0x00020000) != 0;
        actor.Friendly = playerStart || !defaults.BitsUseStealth && (defaults.Bits & 0x40000000) != 0;
        actor.NoBlockMonsters = playerStart || defaults.NoBlockMonsters;
    }

    internal static void ApplyExtendedDefaults(Actor actor, DehackedActor? defaults, bool fastMonsters = false)
    {
        if (PickupCatalog.IsPickup(actor.ClassDoomEdNum))
        {
            actor.MovementSpeed = Fixed.FromDouble(defaults?.Speed ?? 0);
            actor.PainChance = defaults?.PainChance ?? 0;
            actor.ReactionTime = defaults?.ReactionTime ?? 8;
            actor.WoundHealth = 6;
        }
        if (actor is PlayerPawn && defaults is { SpeedPatched: true })
            actor.MovementSpeed = Fixed.FromDouble(defaults.Speed);
        if (fastMonsters && defaults?.FastSpeed is { } fastSpeed && fastSpeed >= 0)
            actor.MovementSpeed = Fixed.FromDouble(fastSpeed);
        if (defaults is { PainChancePatched: true }) actor.PainChance = defaults.PainChance;
        if (defaults?.MeleeRangeRaw is { } meleeRange) actor.MeleeRange = new Fixed(meleeRange);
        if (defaults?.MeleeThreshold is { } meleeThreshold) actor.MeleeThreshold = meleeThreshold;
        if (defaults?.MaxTargetRange is { } maxTargetRange) actor.MaxTargetRange = maxTargetRange;
        if (defaults?.MissileChanceMultiplier is { } missileChanceMultiplier) actor.MissileChanceMult = missileChanceMultiplier;
        if (defaults?.MinMissileChance is { } minMissileChance) actor.MinMissileChance = minMissileChance;
        if (defaults?.ProjectilePassHeightRaw is { } projectilePassHeight) actor.ProjectilePassHeight = new Fixed(projectilePassHeight);
        actor.DefaultTouchy = actor.Touchy = defaults?.Touchy ?? false;
        actor.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { Bits2Patched: true })
        {
            actor.CanPass = (defaults.Bits2 & 0x1000) != 0;
            actor.NoTeleport = (defaults.Bits2 & 0x80) != 0;
            actor.FloatBob = (defaults.Bits2 & 0x00080000) != 0;
            actor.SeekerMissile = (defaults.Bits2 & 0x40000000) != 0;
            actor.Reflective = (defaults.Bits2 & 0x80000000u) != 0;
            actor.Boss = (defaults.Bits2 & 0x00008000) != 0;
            actor.ThruGhost = (defaults.Bits2 & 0x00004000) != 0;
            actor.NonShootable = (defaults.Bits2 & 0x04000000) != 0;
            actor.CantLeaveFloorPic = (defaults.Bits2 & 0x02000000) != 0;
            actor.Rip = (defaults.Bits2 & 0x00000100) != 0;
            actor.Pushable = (defaults.Bits2 & 0x00000200) != 0;
            actor.CannotPush = (defaults.Bits2 & 0x00002000) != 0;
            actor.TeleStomp = (defaults.Bits2 & 0x00040000) != 0;
            actor.FloorClipEnabled = (defaults.Bits2 & 0x00000020) != 0;
            actor.Fly = (defaults.Bits2 & 0x00000010) != 0;
            actor.SpawnFloat = (defaults.Bits2 & 0x00000040) != 0;
            actor.Blasted = (defaults.Bits2 & 0x00000008) != 0;
            actor.CanSlide = (defaults.Bits2 & 0x400) != 0;
            actor.Invulnerable = (defaults.Bits2 & 0x08000000) != 0;
            actor.Dormant = (defaults.Bits2 & 0x10000000) != 0;
        }
        if (defaults is { GravityPatched: true }) actor.Gravity = Fixed.FromDouble(defaults.Gravity);
        if (actor is not PlayerPawn && defaults is { BitsPatched: true })
        {
            actor.IsMonster = (defaults.Bits & 0x00400000) != 0;
            // Native applies COUNTKILL's standard monster flags after secondary flags.
            if (actor.IsMonster) actor.CanPass = true;
        }
        actor.DefaultFloatBob = actor.FloatBob;
        actor.DefaultSeeker = actor.SeekerMissile;
        actor.DefaultReflective = actor.Reflective;
        actor.DefaultThruGhost = actor.ThruGhost;
        actor.DefaultNonShootable = actor.NonShootable;
        actor.DefaultFloorPic = actor.CantLeaveFloorPic;
        actor.DefaultRip = actor.Rip;
        actor.DefaultPushFlags = (actor.Pushable ? 1 : 0) | (actor.CannotPush ? 2 : 0);
        actor.DefaultTeleStomp = actor.TeleStomp;
        actor.DefaultFloorClip = actor.FloorClipEnabled;
        actor.DefaultNoClip = actor.NoClip;
        actor.DefaultCombatFlags = (actor.JustHit ? 1 : 0) | (actor.JustAttacked ? 2 : 0);
        actor.DefaultInFloat = actor.InFloat;
        actor.DefaultCorpse = actor.Corpse;
        actor.DefaultFly = actor.Fly;
        actor.SpawnPainChance = actor.PainChance;
        actor.DefaultSpawnFloat = actor.SpawnFloat;
        actor.DefaultBlasted = actor.Blasted;
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
        | (actor.IsMonster ? 32 : 0) | (actor.SpawnCeiling ? 64 : 0) | (actor.NoSector ? 128 : 0) | (actor.CanPass ? 256 : 0);

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
    private readonly Dictionary<int, uint> _sectorSoundTargets = new();
    public uint? SectorSoundTargetId(int sector)
        => _sectorSoundTargets.TryGetValue(sector, out var id) && Actors.Any(a => a.Id == id && !a.Destroyed) ? id : null;
    internal void SetSectorSoundTarget(int sector, Actor? target)
    {
        if (target is null || target.Destroyed) _sectorSoundTargets.Remove(sector);
        else _sectorSoundTargets[sector] = target.Id;
    }

    private readonly Dictionary<int, string> _sectorAcsSoundSequences = new();
    private readonly Dictionary<int, string> _polyobjAcsSoundSequences = new();
    public string? SectorAcsSoundSequence(int sector) =>
        _sectorAcsSoundSequences.GetValueOrDefault(sector);
    internal void SetSectorAcsSoundSequence(int sector, string name) => _sectorAcsSoundSequences[sector] = name;
    public string? PolyobjAcsSoundSequence(int polyId) =>
        _polyobjAcsSoundSequences.GetValueOrDefault(polyId);
    internal void SetPolyobjAcsSoundSequence(int polyId, string name) => _polyobjAcsSoundSequences[polyId] = name;

    private double? _airControlOverride;
    private double? _levelGravityOverride;
    private double _gibFactor = 1;
    public double GibFactor
    {
        get => _gibFactor;
        set => _gibFactor = double.IsFinite(value) && value >= 0
            ? value : throw new ArgumentOutOfRangeException(nameof(value));
    }

    /// <summary>Native <c>splashfactor</c> cvar reciprocal for bomb-source radius damage.</summary>
    public double SplashFactor { get; set; } = 1;
    /// <summary>Native <c>selfthrustscale</c> for radius self-thrust scaling.</summary>
    public double SelfThrustScale { get; set; } = 1;

    public bool EnvironmentLightningForced { get; set; }
    public List<(int Intensity, int Duration, int Radius, int Tid)> EnvironmentQuakes { get; } = [];
    public int CameraThingId { get; set; }
    public int CameraRevertTics { get; set; }
    /// <summary>Native process cvar map used by Actor.GetCVar. Not part of the save pose.</summary>
    internal ConsoleVariables CVars { get; } = new();
    /// <summary>Native <c>consoleplayer</c> fallback for userinfo queries without a player.</summary>
    internal int ConsolePlayer { get; set; }

    public double LevelGravity => _levelGravityOverride ?? Level.LevelGravity;
    public void SetLevelGravity(int fixedValue) => _levelGravityOverride = fixedValue / 65536.0;
    public double? AirControl => _airControlOverride ?? Level.AirControl;
    public void SetAirControl(int fixedValue) => _airControlOverride = fixedValue / 65536.0;
    public event Action<Actor>? WorldThingRevived;
    public event Action<Actor>? ActorDestroyed;
    public event Action? StateRestored;
    public event Action? LevelExited;
    private bool _suppressRestoreNotifications;
    internal void NotifyActorDestroyed(Actor actor)
    {
        foreach (var observer in _actors)
        {
            observer.Brain?.ForgetDestroyedActor(actor.Id);
            observer.ForgetDestroyedPointers(actor.Id);
            if (observer.LastHeardTargetId == actor.Id) observer.LastHeardTargetId = null;
        }
        if (!_suppressRestoreNotifications) ActorDestroyed?.Invoke(actor);
    }

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
    private readonly SpawnOptions _spawnOptions;
    private readonly Dictionary<int, int> _mapPickupTypes;
    private readonly Dictionary<int, int> _mapDecorationTypes;
    private readonly Dictionary<int, int> _mapMonsterTypes;
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
    private ulong? _uniqueTidRandomState;
    internal ulong? UniqueTidRandomState => _uniqueTidRandomState;
    private ulong? _acsRandomState;
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
    private ulong _bfgSelfDamageRandomState;
    private readonly ulong _initialBfgSelfDamageRandomState;
    private bool _hasBfgSelfDamageRandomState;
    private ulong _bfgSprayRandomState;
    private readonly ulong _initialBfgSprayRandomState;
    private bool _hasBfgSprayRandomState;
    private ulong _fireSg2RandomState;
    private readonly ulong _initialFireSg2RandomState;
    private bool _hasFireSg2RandomState;
    internal int NextFireSg2Damage()
    {
        _hasFireSg2RandomState = true;
        return 5 * (1 + (int)NativeStateRandom.NextBounded(ref _fireSg2RandomState, 3));
    }
    internal double NextFireSg2Spread(double degrees)
    {
        _hasFireSg2RandomState = true;
        var first = (int)(NativeStateRandom.Next(ref _fireSg2RandomState) & 255);
        var second = (int)(NativeStateRandom.Next(ref _fireSg2RandomState) & 255);
        return (first - second) * (degrees / 256);
    }
    private ulong _gunShotRandomState;
    private readonly ulong _initialGunShotRandomState;
    private bool _hasGunShotRandomState;
    internal int NextGunShotDamage()
    {
        _hasGunShotRandomState = true;
        return 5 * (1 + (int)NativeStateRandom.NextBounded(ref _gunShotRandomState, 3));
    }
    internal double NextGunShotSpread(double degrees)
    {
        _hasGunShotRandomState = true;
        var first = (int)(NativeStateRandom.Next(ref _gunShotRandomState) & 255);
        var second = (int)(NativeStateRandom.Next(ref _gunShotRandomState) & 255);
        return (first - second) * (degrees / 256);
    }
    internal int NextBfgSprayDamageDie()
    {
        _hasBfgSprayRandomState = true;
        return 1 + (int)NativeStateRandom.NextBounded(ref _bfgSprayRandomState, 8);
    }
    internal uint NextBfgSelfDamageRandom()
    {
        _hasBfgSelfDamageRandomState = true;
        return NativeStateRandom.Next(ref _bfgSelfDamageRandomState);
    }
    private ulong _monsterRefireRandomState;
    private readonly ulong _initialMonsterRefireRandomState;
    internal int NextMonsterRefireByte()
    {
        return (int)(NativeStateRandom.Next(ref _monsterRefireRandomState) & 255);
    }
    private ulong _freezeDeathRandomState;
    private readonly ulong _initialFreezeDeathRandomState;
    private bool _hasFreezeDeathRandomState;
    internal int NextActorDieRandom()
    {
        _hasActorDieRandomState = true;
        return (int)(NativeStateRandom.Next(ref _actorDieRandomState) & 3);
    }

    public bool CompatibilityInvisibility { get; set; }
    public bool VerticalWeaponSpread { get; set; }
    internal int NextPlayerSearchStart()
    {
        _hasPlayerSearchRandomState = true;
        return (int)(NativeStateRandom.Next(ref _playerSearchRandomState) & 63);
    }

    /// <summary>Native <c>pr_look2</c> / LookyLooky stream used by <c>A_Look2</c>.</summary>
    internal int NextLook2Byte()
    {
        _hasPlayerSearchRandomState = true;
        return (int)(NativeStateRandom.Next(ref _playerSearchRandomState) & 255);
    }
    private ulong _playerSearchRandomState;
    private readonly ulong _initialPlayerSearchRandomState;
    private bool _hasPlayerSearchRandomState;

    internal int NextPlayerLookByte()
    {
        _hasPlayerLookRandomState = true;
        return (int)(NativeStateRandom.Next(ref _playerLookRandomState) & 255);
    }
    private ulong _playerLookRandomState;
    private readonly ulong _initialPlayerLookRandomState;
    private bool _hasPlayerLookRandomState;
    private ulong _floatSpawnRandomState;
    private readonly ulong _initialFloatSpawnRandomState;
    private bool _hasFloatSpawnRandomState;

    internal double RandomFloatSpawnZ(Actor actor)
    {
        var floor = FloorOf(actor.SectorIndex);
        var space = CeilingOf(actor.SectorIndex) - actor.Height.ToDouble() - floor;
        if (space <= 48) return floor;
        _hasFloatSpawnRandomState = true;
        var random = NativeStateRandom.Next(ref _floatSpawnRandomState) & 255;
        return floor + 40 + (space - 40) * random / 256.0;
    }

    internal int NextHateSearchBudget()
    {
        _hasHateSearchRandomState = true;
        return 7 + (int)(NativeStateRandom.Next(ref _hateSearchRandomState) & 31);
    }
    private ulong _hateSearchRandomState;
    private readonly ulong _initialHateSearchRandomState;
    private bool _hasHateSearchRandomState;
    private ulong _actorDieRandomState;
    private readonly ulong _initialActorDieRandomState;
    private bool _hasActorDieRandomState;
    private ulong _puffRandomState;
    private readonly ulong _initialPuffRandomState;
    private bool _hasPuffRandomState;
    private ulong _dropItemRandomState;
    private readonly ulong _initialDropItemRandomState;
    private bool _hasDropItemRandomState;
    private ulong _chaseRandomState;
    private readonly ulong _initialChaseRandomState;
    private bool _hasChaseRandomState;
    private ulong _missileDamageRandomState;
    private readonly ulong _initialMissileDamageRandomState;
    private bool _hasMissileDamageRandomState;
    private ulong _poisonDamageRandomState;
    private readonly ulong _initialPoisonDamageRandomState;
    private bool _hasPoisonDamageRandomState;
    private ulong _reflectRandomState;
    private readonly ulong _initialReflectRandomState;
    private bool _hasReflectRandomState;
    private ulong _seekerMissileRandomState;
    private readonly ulong _initialSeekerMissileRandomState;
    private bool _hasSeekerMissileRandomState;
    private ulong _painChanceRandomState;
    private readonly ulong _initialPainChanceRandomState;
    private bool _hasPainChanceRandomState;
    private ulong _skelFistRandomState;
    private readonly ulong _initialSkelFistRandomState;
    private bool _hasSkelFistRandomState;
    private ulong _sawRandomState;
    private readonly ulong _initialSawRandomState;
    private bool _hasSawRandomState;
    private ulong _punchRandomState;
    private readonly ulong _initialPunchRandomState;
    private bool _hasPunchRandomState;
    private ulong? _newChaseDirRandomState;
    private ulong? _tryWalkRandomState;
    private ulong? _stayOnLiftRandomState;
    private ulong? _scaredyCatRandomState;
    private ulong? _checkMissileRandomState;
    private ulong? _defectRandomState;
    private ulong? _sectorDamageRandomState;
    internal ulong? SectorDamageRandomState => _sectorDamageRandomState;
    internal int NextSectorDamageByte() => NextWanderByte(ref _sectorDamageRandomState, 0x953d8868u);
    private ulong? _nativeSwitchTargetRandomState;
    internal ulong? NativeSwitchTargetRandomState => _nativeSwitchTargetRandomState;
    internal int NextSwitchTargetByte() => NextWanderByte(ref _nativeSwitchTargetRandomState, 0x71a89a33u);
    private ulong? _checkSightRandomState;
    internal int NextCheckSightByte() => NextWanderByte(ref _checkSightRandomState, 0xbf9374eau);
    internal int NextDefectByte() => NextWanderByte(ref _defectRandomState, 0x3d303db1u);
    internal int NextCheckMissileByte() => NextWanderByte(ref _checkMissileRandomState, 0xfbb4b7f2u);
    internal int NextScaredyCatByte() => NextWanderByte(ref _scaredyCatRandomState, 0x39450eceu);
    internal int NextStayOnLiftByte() => NextWanderByte(ref _stayOnLiftRandomState, 0x5350ed82u);
    private ulong? _avoidCrushRandomState;
    internal int NextAvoidCrushByte() => NextWanderByte(ref _avoidCrushRandomState, 0x5eef6bb0u);
    private ulong? _enemyStrafeRandomState;
    internal int NextEnemyStrafeByte() => NextWanderByte(ref _enemyStrafeRandomState, 0x2748785bu);
    internal int NextNewChaseDirByte() => NextWanderByte(ref _newChaseDirRandomState, 0x5f7b8192u);
    internal int NextTryWalkByte() => NextWanderByte(ref _tryWalkRandomState, 0xf43936d9u);
    private ulong? _monsterOpenDoorRandomState;
    internal int NextMonsterOpenDoorByte() => NextWanderByte(ref _monsterOpenDoorRandomState, 0x6f70656eu);
    private ulong? _dropoffRandomState;
    internal int NextDropoffByte() => NextWanderByte(ref _dropoffRandomState, 0x64726f70u);
    private int NextWanderByte(ref ulong? saved, uint name)
    {
        var state = saved ?? NativeStateRandom.Seed(unchecked((uint)RngSeed), name);
        var result = (int)(NativeStateRandom.Next(ref state) & 255); saved = state; return result;
    }
    private ulong _skipTargetRandomState;
    private readonly ulong _initialSkipTargetRandomState;
    private bool _hasSkipTargetRandomState;
    internal int NextSkipTargetByte()
    {
        _hasSkipTargetRandomState = true;
        return (int)(NativeStateRandom.Next(ref _skipTargetRandomState) & 255);
    }
    private ulong _cPosRefireRandomState;
    private readonly ulong _initialCPosRefireRandomState;
    private bool _hasCPosRefireRandomState;
    private ulong _cPosAttackRandomState;
    private readonly ulong _initialCPosAttackRandomState;
    private bool _hasCPosAttackRandomState;
    private ulong _sPosAttackRandomState;
    private readonly ulong _initialSPosAttackRandomState;
    private bool _hasSPosAttackRandomState;
    private ulong _posAttackRandomState;
    private readonly ulong _initialPosAttackRandomState;
    private bool _hasPosAttackRandomState;
    private ulong _bruisAttackRandomState;
    private readonly ulong _initialBruisAttackRandomState;
    private bool _hasBruisAttackRandomState;
    private ulong _headAttackRandomState;
    private readonly ulong _initialHeadAttackRandomState;
    private bool _hasHeadAttackRandomState;
    private ulong _troopAttackRandomState;
    private readonly ulong _initialTroopAttackRandomState;
    private bool _hasTroopAttackRandomState;
    private ulong _sargAttackRandomState;
    private readonly ulong _initialSargAttackRandomState;
    private bool _hasSargAttackRandomState;
    private uint _flickerRandomState;
    private uint _lightFlashRandomState;
    private uint _fireFlickerRandomState;
    private ulong? _switchAnimRandomState;
    private ulong? _teleportRandomState;
    private readonly List<SectorMotion> _motions = new();
    private readonly SimLineAction[] _initialLineActions;
    private readonly SimSectorTerrain[] _initialSectorTerrains;

    private AuthoritySimulation(
        PlayLevel level,
        ThinkerCollection thinkers,
        List<Actor> actors,
        int rngSeed,
        CompatSurface compat, bool damageExitAllowed, SpawnOptions spawnOptions, DehackedPatchResult? dehacked)
    {
        _dehacked = dehacked;
        _spawnOptions = spawnOptions;
        _mapPickupTypes = actors.Where(actor => actor.MapPickupThingIndex.HasValue)
            .ToDictionary(actor => actor.MapPickupThingIndex!.Value, actor => actor.DoomEdNum);
        _mapDecorationTypes = actors.Where(actor => actor.MapDecorationThingIndex.HasValue)
            .ToDictionary(actor => actor.MapDecorationThingIndex!.Value, actor => actor.DoomEdNum);
        _mapMonsterTypes = actors.Where(actor => actor.MapMonsterThingIndex.HasValue)
            .ToDictionary(actor => actor.MapMonsterThingIndex!.Value, actor => actor.DoomEdNum);
        Level = level;
        _initialSectorTerrains = level.Sectors.Select(s => new SimSectorTerrain(s.FloorTerrain, s.CeilingTerrain)).ToArray();
        LevelLiftMarkers.Apply(level);
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
        _gunShotRandomState = _initialGunShotRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x4b5c394eu);
        _fireSg2RandomState = _initialFireSg2RandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x03b36d9au);
        _bfgSprayRandomState = _initialBfgSprayRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x2dda5aa9u);
        _bfgSelfDamageRandomState = _initialBfgSelfDamageRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x7edbb96cu);
        _monsterRefireRandomState = _initialMonsterRefireRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x6d8f9a11u);
        SwitchTargetRandomState = unchecked((uint)rngSeed) ^ 0x73776974u;
        DmSpawnRandomState = unchecked((uint)rngSeed) ^ 0x646d7370u;
        _uniqueTidRandomState = null;
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
        _playerSearchRandomState = _initialPlayerSearchRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x5a9c92d8u);
        _playerLookRandomState = _initialPlayerLookRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x30d3f84eu);
        _floatSpawnRandomState = _initialFloatSpawnRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x05ec38a2u);
        _hateSearchRandomState = _initialHateSearchRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xbdb01ae9u);
        _actorDieRandomState = _initialActorDieRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x8ed8ec1bu);
        _puffRandomState = _initialPuffRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x1223e6dcu);
        _dropItemRandomState = _initialDropItemRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x2d1fda00u);
        _chaseRandomState = _initialChaseRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x573c149du);
        _missileDamageRandomState = _initialMissileDamageRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x333cfa17u);
        _poisonDamageRandomState = _initialPoisonDamageRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x15ba5b61u);
        _reflectRandomState = _initialReflectRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x4139f7d5u);
        _seekerMissileRandomState = _initialSeekerMissileRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x85890f4bu);
        _painChanceRandomState = _initialPainChanceRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xdae49b5au);
        _skelFistRandomState = _initialSkelFistRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x99c25fa8u);
        _sawRandomState = _initialSawRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x233cf782u);
        _punchRandomState = _initialPunchRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xed205f5cu);
        _skipTargetRandomState = _initialSkipTargetRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xbc52aa96u);
        _cPosRefireRandomState = _initialCPosRefireRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x59106a10u);
        _cPosAttackRandomState = _initialCPosAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xc1b064acu);
        _sPosAttackRandomState = _initialSPosAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xe61ee544u);
        _posAttackRandomState = _initialPosAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xb1bea4d7u);
        _bruisAttackRandomState = _initialBruisAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x0b85c748u);
        _headAttackRandomState = _initialHeadAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0xd7c9c8abu);
        _troopAttackRandomState = _initialTroopAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x83af1235u);
        _sargAttackRandomState = _initialSargAttackRandomState = NativeStateRandom.Seed(unchecked((uint)rngSeed), 0x96b7cfedu);
        Compat = compat;
        DamageExitAllowed = damageExitAllowed;
        GameMode = spawnOptions.Mode;
        AutoUseHealth = spawnOptions.AutoUseHealth;
        DisableAutoHealth = spawnOptions.DisableAutoHealth;
        StrifeHealthPickups = spawnOptions.StrifeHealthPickups;
        if (StrifeHealthPickups && spawnOptions.RavenHealthPickups) throw new ArgumentException("Select one health pickup mapping.", nameof(spawnOptions));
        RavenHealthPickups = spawnOptions.RavenHealthPickups;
        UnlimitedHealthPickups = spawnOptions.UnlimitedHealthPickups;
        InfiniteStoredInventoryUse = spawnOptions.InfiniteStoredInventoryUse;
        InfiniteAmmo = spawnOptions.InfiniteAmmo;
        InfiniteHealthItemUse = spawnOptions.InfiniteHealthItemUse || InfiniteStoredInventoryUse;
        if (spawnOptions.DefaultInventoryMaximum <= 0) throw new ArgumentOutOfRangeException(nameof(spawnOptions));
        DefaultInventoryMaximum = spawnOptions.DefaultInventoryMaximum;
        Skill = Math.Clamp(spawnOptions.Skill, 0, 4);
        FastMonsters = spawnOptions.FastMonsters ?? Skill == 4;
        SlowMonsters = spawnOptions.SlowMonsters;
        NoMonsterPain = spawnOptions.NoMonsterPain;
        NativeMonsterMovement = spawnOptions.NativeMonsterMovement;
        if (!double.IsFinite(spawnOptions.SkillAggressiveness))
            throw new ArgumentOutOfRangeException(nameof(spawnOptions.SkillAggressiveness));
        SkillAggressiveness = Math.Clamp(spawnOptions.SkillAggressiveness, 0, 1);
        if (!double.IsFinite(spawnOptions.PlayerDamageFactor) || spawnOptions.PlayerDamageFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(spawnOptions.PlayerDamageFactor));
        PlayerDamageFactor = spawnOptions.PlayerDamageFactor;
        if (!double.IsFinite(spawnOptions.SkillDamageFactor) || spawnOptions.SkillDamageFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(spawnOptions.SkillDamageFactor));
        SkillDamageFactor = spawnOptions.SkillDamageFactor;
        if (!double.IsFinite(spawnOptions.MonsterDamageFactor) || spawnOptions.MonsterDamageFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(spawnOptions.MonsterDamageFactor));
        if (!double.IsFinite(spawnOptions.FriendlyDamageFactor) || spawnOptions.FriendlyDamageFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(spawnOptions.FriendlyDamageFactor));
        MonsterDamageFactor = spawnOptions.MonsterDamageFactor;
        FriendlyDamageFactor = spawnOptions.FriendlyDamageFactor;
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
        SwitchActions.Initialize(this);
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
                : actor.SpawnFloat ? RandomFloatSpawnZ(actor) : actor.Z.ToDouble() + actor.SpawnZOffset;
            actor.Z = Fixed.FromDouble(Math.Clamp(spawnZ, short.MinValue, short.MaxValue));
            if (actor.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(this, actor);
            actor.OnGround = actor.SpawnCeiling || actor.SpawnFloat
                ? actor.Z.ToDouble() <= FloorOf(actor.SectorIndex) : actor.SpawnZOffset <= 0;
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
        _initialLineActions = level.Lines.Select(SimLineAction.Capture).ToArray();
        RecomputeChecksum();
        PublishStatus();
    }

    public PlayLevel Level { get; }
    /// <summary>Local haptics_do_world preference; presentation events are not saved or checksummed.</summary>
    public bool WorldHapticsEnabled { get; set; } = true;
    public bool DamageHapticsEnabled { get; set; } = true;
    public bool ActionHapticsEnabled { get; set; } = true;
    public event Action<SimulationRumbleRequest>? RumbleRequested;
    internal void RequestRumble(SimulationRumbleRequest request) => RumbleRequested?.Invoke(request);
    private readonly List<SimulationSoundRequest> _soundRequests = new();
    public IReadOnlyList<SimulationSoundRequest> PendingSoundRequests => _soundRequests.AsReadOnly();
    public string AcsPrintBuffer { get; private set; } = "";
    public string? AcsPrintBoldBuffer { get; private set; }
    internal void AppendAcsPrint(string fragment) => AcsPrintBuffer += fragment;
    internal void AppendAcsPrintBold(string fragment) => AcsPrintBoldBuffer = fragment;
    private readonly List<AcsHudMessage> _acsHudMessages = new();
    public IReadOnlyList<AcsHudMessage> AcsHudMessages => _acsHudMessages;
    internal void RecordAcsHudMessage(AcsHudMessage message) => _acsHudMessages.Add(message);
    public AcsHudMessage[] DrainAcsHudMessages()
    {
        var messages = _acsHudMessages.ToArray();
        _acsHudMessages.Clear();
        return messages;
    }
    internal LevelLine? AcsTriggerLine { get; set; }
    internal bool AcsTriggerBackSide { get; set; }
    internal int AcsHudClipLeft { get; set; }
    internal int AcsHudClipTop { get; set; }
    internal int AcsHudClipWidth { get; set; }
    internal int AcsHudClipHeight { get; set; }
    internal int AcsHudWrapWidth { get; set; }
    internal string? AcsSlideshowName { get; set; }
    internal int AcsSubtitleNumber { get; set; }
    internal void QueueAnnouncerSound(string sound) =>
        QueueSoundRequest(new(0, sound, 7, (int)SimulationSoundFlags.Ui, 1, false, 0, true));
    internal void QueueAmbientAcsSound(string sound, float volume) =>
        QueueSoundRequest(new(0, sound, 0, 0, volume, false, 0, false));
    internal void QueueSectorAcsSound(int sectorIndex, string sound, float volume) =>
        QueueSoundRequest(new(0, sound, 0, 0, volume, false, 1, false));
    private readonly Dictionary<int, int> _teamScores = new();
    public int GetTeamScore(int team) => _teamScores.GetValueOrDefault(team);
    public void SetTeamScore(int team, int value) => _teamScores[team] = value;
    public event Action? SoundRequestsReady;
    internal void QueueSoundRequest(SimulationSoundRequest request, bool immediate = false)
    {
        if (!request.Stop && !request.ChangeVolume && !request.ChangePitch
            && string.Equals(request.Sound, "None", StringComparison.OrdinalIgnoreCase)) return;
        _soundRequests.Add(request);
        if (immediate) SoundRequestsReady?.Invoke();
    }
    public SimulationSoundRequest[] DrainSoundRequests()
    {
        var requests = _soundRequests.ToArray();
        _soundRequests.Clear();
        return requests;
    }

    internal sealed record TransientState(Dictionary<uint, PlayerCommand[]> Commands, SimulationSoundRequest[] Sounds);
    internal TransientState CaptureTransientState() => new(
        Players.ToDictionary(player => player.Id, player => player.CaptureBufferedCommands()), _soundRequests.ToArray());
    internal void RestoreTransientState(TransientState state)
    {
        foreach (var entry in state.Commands)
            Players.Single(player => player.Id == entry.Key).RestoreBufferedCommands(entry.Value);
        _soundRequests.Clear();
        _soundRequests.AddRange(state.Sounds);
    }
    internal Dictionary<int, int> HealthGroups { get; }
    /// <summary>Native command-line str args for ACS <c>StrArg</c>.</summary>
    public IReadOnlyList<string> StrArgs { get; }
    public ThinkerCollection Thinkers { get; }
    public IReadOnlyList<Actor> Actors => _actors;
    public IEnumerable<Actor> SectorActors(int sectorIndex)
    {
        if ((uint)sectorIndex >= (uint)Level.Sectors.Count) throw new ArgumentOutOfRangeException(nameof(sectorIndex));
        return _actors.Where(actor => actor.IsSectorLinked && actor.SectorIndex == sectorIndex);
    }
    public IEnumerable<PlayerPawn> Players => _actors.OfType<PlayerPawn>();
    public int RngSeed { get; }
    public uint Checksum { get; private set; }
    public string StatusLine { get; private set; } = "";
    public CompatSurface Compat { get; }
    public SpawnGameMode GameMode { get; }
    public bool AutoUseHealth { get; }
    public bool DisableAutoHealth { get; }
    public bool StrifeHealthPickups { get; }
    public bool RavenHealthPickups { get; }
    public bool UnlimitedHealthPickups { get; }
    public bool InfiniteHealthItemUse { get; }
    public bool InfiniteStoredInventoryUse { get; }
    public bool InfiniteAmmo { get; }
    public int DefaultInventoryMaximum { get; }
    /// <summary>
    /// Single-player death sets this instead of moving the corpse. Native <c>G_DoReborn</c> reloads the map.
    /// There is no <c>G_InitNew</c> here.
    /// </summary>
    public bool ReloadRequested { get; private set; }
    internal bool ConsumeReloadRequest()
    {
        if (!ReloadRequested) return false;
        ReloadRequested = false;
        return true;
    }
    /// <summary>Native <c>sv_singleplayerrespawn</c>. When set, single-player death revives at the player start.</summary>
    public bool AllowSinglePlayerRespawn { get; set; }
    /// <summary>Native <c>sv_forcerespawn</c>. Deathmatch only: revive after the wait without a button press.</summary>
    public bool ForceRespawn { get; set; }
    /// <summary>Combined native level/dmflags ZDoom or Hexen falling-damage gate for death screams.</summary>
    public bool FallingDeathSplatEnabled { get; set; }
    public HCDE.Gamedata.FallingDamageStyle ForcedFallingDamage { get; set; }
    public HCDE.Gamedata.FallingDamageStyle EffectiveFallingDamage =>
        (HCDE.Gamedata.FallingDamageStyle)((int)Level.FallingDamage | (int)ForcedFallingDamage);
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
    /// <summary>Native skill NoPain. Suppresses non-player damage pain, preserving wakeup.</summary>
    public bool NoMonsterPain { get; }
    /// <summary>Enables converted chase movement on automatic ticks. Supply the same policy when loading saves.</summary>
    public bool NativeMonsterMovement { get; }
    /// <summary>MAPINFO-style aggressiveness, clamped to 0..1. Supply the same policy when loading saves.</summary>
    public double SkillAggressiveness { get; }
    /// <summary>Native server player damage factor.</summary>
    public double PlayerDamageFactor { get; }
    /// <summary>Native skill DamageFactor, applied to player damage before server scaling.</summary>
    public double SkillDamageFactor { get; }
    /// <summary>Native sv_damagefactormobj for non-player, non-friendly targets.</summary>
    public double MonsterDamageFactor { get; }
    /// <summary>Native sv_damagefactorfriendly for non-player friendly targets.</summary>
    public double FriendlyDamageFactor { get; }
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
    private bool _rewindEnabled;
    public bool RewindEnabled
    {
        get => _rewindEnabled;
        set
        {
            _rewindEnabled = value;
            if (!value) Rewind.Reset();
        }
    }
    /// <summary>Native ACS <c>SetMusicVolume</c> scale. Audio playback is absent.</summary>
    public double MusicVolume { get; set; } = 1;
    public AcsVm Acs { get; }
    public AcsScriptCallRegistry ScriptCalls { get; } = new();
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
    internal List<ActiveButton> ActiveButtons { get; } = [];
    internal List<PlaneWaggle> PlaneWaggles { get; } = [];
    internal List<SectorPusher> SectorPushers { get; } = [];
    internal List<LightTransfer> LightTransfers { get; } = [];
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
            if (actor.Destroyed || actor.NoSector || actor.Floating || !actor.OnGround || actor is ProjectileActor or PlayerPawn)
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
        if (dehacked is not null) SoundResourceResolver.ApplyDehackedSoundNames(level, dehacked);
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
        bool inventoryToss = false, bool depleted = false, bool suppressWeaponAmmo = false, bool ravenHealthArtifact = false,
        int? healthOverride = null, int? autoUseOverride = null)
    {
        // Only named artifact routes may use editor numbers that overlap Doom weapons.
        if (RavenHealthPickups && doomEdNum is 82 or 32 && !ravenHealthArtifact) return false;
        if (dropper.Destroyed || !PickupCatalog.IsPickup(doomEdNum, RavenHealthPickups))
            return false;
        var adjustedAmount = ignoreAmmoSkill || ravenHealthArtifact ? pickupAmount
            : PickupCatalog.DropPickupAmount(doomEdNum, pickupAmount, DropAmmoFactor);
        var customDropFactor = !ravenHealthArtifact && !inventoryToss && !ignoreAmmoSkill && DropAmmoFactor != -1
            && PickupCatalog.UsesDropAmmoFactor(doomEdNum);
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var drop = CreateDroppedPickupActor(id, doomEdNum, dropper.X, dropper.Y);
        if (!inventoryToss) drop.NoGravity = false;
        drop.IgnoreAmmoSkill = ignoreAmmoSkill || customDropFactor;
        drop.Depleted = depleted;
        drop.SuppressWeaponPickupAmmo = suppressWeaponAmmo || customDropFactor && adjustedAmount == 0;
        drop.PickupAmount = adjustedAmount;
        drop.PickupHealthOverride = healthOverride;
        drop.PickupAutoUseOverride = autoUseOverride;
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
            drop.Solid = false;
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

    private Actor CreateDroppedPickupActor(uint id, int doomEdNum, Fixed x, Fixed y)
    {
        var defaults = _dehacked?.Actors.FirstOrDefault(candidate => candidate.DoomEdNum == doomEdNum && candidate.Patched);
        var actor = new Actor
        {
            Id = id, DoomEdNum = doomEdNum, X = x, Y = y, Level = Level, Simulation = this,
            SectorIndex = ActorPhysics.SectorAt(Level, x.ToDouble(), y.ToDouble()),
            Solid = false, Shootable = false, Dropped = true, SpecialPickup = true,
            Radius = ActorSpawner.RadiusOf(defaults, player: false),
            Height = defaults is null ? PickupCatalog.HeightOf(doomEdNum) : ActorSpawner.HeightOf(defaults),
            Health = defaults?.Health ?? 1000,
            Mass = defaults is { MassPatched: true } ? defaults.Mass : DoomActorCatalog.MassOf(doomEdNum),
            SpawnedDroppedPickup = true
        };
        actor.ResurrectionHealth = actor.Health;
        if (actor.ResurrectionHealth > 0) actor.GibHealth = int.MinValue;
        actor.DefaultGibHealth = actor.GibHealth;
        actor.SpawnMass = actor.Mass;
        actor.ResurrectionRadius = actor.Radius;
        actor.ResurrectionHeight = actor.Height;
        ActorSpawner.ApplyPrimaryFlags(actor, defaults, playerStart: false);
        // Inventory.BeginPlay marks dynamically spawned inventory as dropped.
        actor.Dropped = actor.SpawnDropped = true;
        actor.SpawnCanPickupItems = actor.CanPickupItems;
        actor.SpawnSpecialPickup = actor.SpecialPickup;
        ActorSpawner.ApplyExtendedDefaults(actor, defaults, FastMonsters);
        ActorSpawner.ApplySoundDefaults(actor, defaults);
        if (defaults is { ReactionTimePatched: true }) actor.ReactionTime = defaults.ReactionTime;
        if (defaults is { MissileDamagePatched: true }) actor.Damage = defaults.MissileDamage;
        if (defaults is { InfightingGroupPatched: true }) actor.InfightingGroup = defaults.InfightingGroup;
        if (defaults is { ProjectileGroupPatched: true }) actor.ProjectileGroup = defaults.ProjectileGroup;
        if (defaults is { SplashGroupPatched: true }) actor.SplashGroup = defaults.SplashGroup;
        if (actor.IsMonster && Skill == 4) actor.ReactionTime = 0;
        actor.SpawnReactionTime = actor.ReactionTime;
        actor.SpawnDamage = actor.Damage;
        actor.ResurrectionCollisionFlags = (ActorSpawner.CollisionFlagsOf(actor) & ~16)
            | (defaults is { BitsPatched: true } && (defaults.Bits & 0x00020000) != 0 ? 16 : 0);
        DoomPickupStates.Apply(actor, doomEdNum);
        actor.BeginPlay();
        return actor;
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
        => AddBotCore(x, y, doomEdNum, thingId);

    private BotPawn AddBotCore(double x, double y, int doomEdNum, int thingId, uint? restoredId = null, bool justSpawned = true)
    {
        var id = restoredId ?? _nextActorId;
        _nextActorId = Math.Max(_nextActorId, checked(id + 1));
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == doomEdNum && actor.Patched);
        var definitionType = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : doomEdNum;
        var definition = DoomActorCatalog.Find(definitionType);
        var catalogHealth = definition?.Health;
        var health = defaults?.Health ?? catalogHealth ?? 30;
        var bot = new BotPawn
        {
            Id = id,
            DoomEdNum = doomEdNum,
            JustSpawned = justSpawned,
            DefinitionDoomEdNum = definitionType,
            ThingId = thingId,
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Angle = new BamAngle(0),
            Health = health,
            ResurrectionHealth = health,
            GibHealth = health > 0 ? int.MinValue : -1,
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
        ActorSpawner.ApplySoundDefaults(bot, defaults);
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
        bot.DefaultGibHealth = bot.GibHealth;
        ActorSpawner.ApplyExtendedDefaults(bot, defaults, FastMonsters);
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
        else if (bot.SpawnFloat && restoredId is null)
        {
            bot.Z = Fixed.FromDouble(RandomFloatSpawnZ(bot));
            bot.OnGround = bot.Z.ToDouble() <= FloorOf(bot.SectorIndex);
        }
        if (bot.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(this, bot);
        _actors.Add(bot);
        Thinkers.Add(bot, ThinkerStat.Default);
        return bot;
    }

    /// <summary>Native ACS <c>DoSpawn</c> (non-forced placement; simplified collision).</summary>
    internal int SpawnAcsActor(string className, double x, double y, double z, int tid, int angleByte, bool force,
        BamAngle? facing = null)
    {
        if (!DoomActorCatalog.TryEditorNumberForClassName(className, out var doomEdNum)) return 0;
        var definition = DoomActorCatalog.Find(doomEdNum);
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.DoomEdNum == doomEdNum && actor.Patched);
        var actor = new Actor
        {
            Id = id,
            DoomEdNum = doomEdNum,
            DefinitionDoomEdNum = defaults?.OriginalDoomEdNum is > 0 ? defaults.OriginalDoomEdNum : doomEdNum,
            ThingId = tid,
            X = Fixed.FromDouble(x),
            Y = Fixed.FromDouble(y),
            Z = Fixed.FromDouble(z),
            Angle = facing ?? new BamAngle((uint)(angleByte * (4294967296.0 / 256.0))),
            Health = defaults?.Health ?? definition?.Health ?? 30,
            Radius = definition != null ? Fixed.FromInt(definition.Radius) : Fixed.FromInt(20),
            Height = definition != null ? Fixed.FromInt(definition.Height) : Fixed.FromInt(56),
            ChaseSpeed = Math.Clamp((definition?.Speed ?? 4) / 4.0, 0, ActorPhysics.MaxMove),
            PainChance = definition?.PainChance ?? 256,
            NoGravity = definition?.Floating ?? false,
            Floating = definition?.Floating ?? false,
            Level = Level,
            Brain = MonsterBrain.ForType(doomEdNum),
        };
        actor.IsMonster = actor.Brain is not null;
        if (defaults != null)
        {
            actor.Radius = ActorSpawner.RadiusOf(defaults, player: false);
            actor.Height = ActorSpawner.HeightOf(defaults);
            ActorSpawner.ApplyPrimaryFlags(actor, defaults, playerStart: false);
            ActorSpawner.ApplySoundDefaults(actor, defaults);
        }
        actor.Simulation = this;
        actor.SectorIndex = ActorPhysics.SectorAt(Level, x, y);
        if (actor.SectorIndex < 0)
        {
            actor.Destroy();
            return 0;
        }
        ThingActivation.InitializeSpawn(actor, false, mapSpawn: false);
        actor.OnGround = actor.Z.ToDouble() <= FloorOf(actor.SectorIndex);
        _actors.Add(actor);
        Thinkers.Add(actor, ThinkerStat.Default);
        return 1;
    }

    public uint NextCombatRandom()
    {
        CombatRandomState = unchecked(1664525u * CombatRandomState + 1013904223u);
        return CombatRandomState;
    }

    /// <summary>Native <c>pr_acs</c> stream used by ACS <c>PCD_RANDOM</c> opcodes.</summary>
    internal int NextAcsRandom(int min, int max)
    {
        if (max < min) (min, max) = (max, min);
        var state = _acsRandomState ?? NativeStateRandom.Seed(unchecked((uint)RngSeed), 0xa72f226du);
        var span = (uint)(max - min + 1L);
        var draw = (int)NativeStateRandom.NextBounded(ref state, span) + min;
        _acsRandomState = state;
        return draw;
    }

    internal uint NextSwitchTargetRandom()
    {
        SwitchTargetRandomState = unchecked(1664525u * SwitchTargetRandomState + 1013904223u);
        return SwitchTargetRandomState;
    }

    internal uint NextUniqueTidRandom()
    {
        var state = _uniqueTidRandomState ?? NativeStateRandom.Seed(unchecked((uint)RngSeed), 0xc388ea6bu);
        var value = NativeStateRandom.Next(ref state);
        _uniqueTidRandomState = state;
        return value;
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
        QueueSoundRequest(new SimulationSoundRequest(corpse.Id, "misc/icebreak", 4, 0, 1, false, 1, false), immediate: true);
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
            chunk.SectorIndex = ActorPhysics.SectorAt(Level, chunk.X.ToDouble(), chunk.Y.ToDouble());
            chunk.OnGround = chunk.Z.ToDouble() <= FloorOf(chunk.SectorIndex);
            if (chunk.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(this, chunk);
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

    internal uint NextChaseByte()
    {
        _hasChaseRandomState = true;
        return NativeStateRandom.Next(ref _chaseRandomState) & 255;
    }

    internal byte NextMissileDamageByte()
    {
        _hasMissileDamageRandomState = true;
        return (byte)(NativeStateRandom.Next(ref _missileDamageRandomState) & 255);
    }
    internal byte NextPoisonDamageByte()
    {
        _hasPoisonDamageRandomState = true;
        return (byte)(NativeStateRandom.Next(ref _poisonDamageRandomState) & 255);
    }
    internal byte NextReflectByte()
    {
        _hasReflectRandomState = true;
        return (byte)(NativeStateRandom.Next(ref _reflectRandomState) & 255);
    }
    internal byte NextSeekerMissileByte()
    {
        _hasSeekerMissileRandomState = true;
        return (byte)(NativeStateRandom.Next(ref _seekerMissileRandomState) & 255);
    }
    internal byte NextPainChanceByte()
    {
        _hasPainChanceRandomState = true;
        return (byte)(NativeStateRandom.Next(ref _painChanceRandomState) & 255);
    }

    internal int NextPunchDamage()
    {
        _hasPunchRandomState = true;
        return 2 * (1 + (int)NativeStateRandom.NextBounded(ref _punchRandomState, 10));
    }

    internal double NextPunchSpread()
    {
        _hasPunchRandomState = true;
        var first = (int)(NativeStateRandom.Next(ref _punchRandomState) & 255);
        var second = (int)(NativeStateRandom.Next(ref _punchRandomState) & 255);
        return (first - second) * (5.625 / 256);
    }

    internal int NextSawByte()
    {
        _hasSawRandomState = true;
        return (int)(NativeStateRandom.Next(ref _sawRandomState) & 255);
    }

    internal double NextSawSpread() => (NextSawByte() - NextSawByte()) / 255.0;

    internal int NextSawDamageMultiplier()
    {
        _hasSawRandomState = true;
        return 1 + (int)NativeStateRandom.NextBounded(ref _sawRandomState, 10);
    }

    internal int NextSkelFistDamage()
    {
        _hasSkelFistRandomState = true;
        return (1 + (int)NativeStateRandom.NextBounded(ref _skelFistRandomState, 10)) * 6;
    }

    internal bool CheckCPosRefire(bool visible, byte continueThreshold = 40)
    {
        _hasCPosRefireRandomState = true;
        return (NativeStateRandom.Next(ref _cPosRefireRandomState) & 255) < continueThreshold || visible;
    }

    internal (double Spread, int Damage) NextCPosAttackShot()
    {
        _hasCPosAttackRandomState = true;
        return NativeStateRandom.NextHitscanShot(ref _cPosAttackRandomState);
    }
    internal (double Spread, int Damage) NextSPosAttackPellet()
    {
        _hasSPosAttackRandomState = true;
        return NativeStateRandom.NextHitscanShot(ref _sPosAttackRandomState);
    }

    internal (double Spread, int Damage) NextPosAttackShot()
    {
        _hasPosAttackRandomState = true;
        return NativeStateRandom.NextHitscanShot(ref _posAttackRandomState);
    }

    internal int NextBruisAttackDamage()
    {
        _hasBruisAttackRandomState = true;
        return (1 + (int)NativeStateRandom.NextBounded(ref _bruisAttackRandomState, 8)) * 10;
    }

    internal int NextHeadAttackDamage()
    {
        _hasHeadAttackRandomState = true;
        return (1 + (int)NativeStateRandom.NextBounded(ref _headAttackRandomState, 6)) * 10;
    }

    internal int NextTroopAttackDamage()
    {
        _hasTroopAttackRandomState = true;
        return (1 + (int)NativeStateRandom.NextBounded(ref _troopAttackRandomState, 8)) * 3;
    }

    internal int NextSargAttackDamage()
    {
        _hasSargAttackRandomState = true;
        return (1 + (int)NativeStateRandom.NextBounded(ref _sargAttackRandomState, 10)) * 4;
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

    internal int NextSwitchAnimByte() => NextWanderByte(ref _switchAnimRandomState, 0x53776974u);

    internal int NextTeleportRandom() => NextWanderByte(ref _teleportRandomState, 0x54656c70u);

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

    internal PuffActor SpawnHitscanPuff(double x, double y, double z, int thingId = 0, bool noRandomZ = false)
    {
        _hasPuffRandomState = true;
        if (!noRandomZ)
        {
            var first = (int)(NativeStateRandom.Next(ref _puffRandomState) & 255);
            var second = (int)(NativeStateRandom.Next(ref _puffRandomState) & 255);
            z += (first - second) / 64.0;
        }
        var firstFrameTics = 4 - (int)(NativeStateRandom.Next(ref _puffRandomState) & 3);
        var puff = new PuffActor(firstFrameTics)
        {
            IsPuff = true, DefaultIsPuff = true,
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

    internal void DiscardTemporaryPuff(PuffActor puff)
    {
        puff.Destroy();
        Thinkers.Remove(puff);
        _actors.Remove(puff);
    }

    public ProjectileActor SpawnProjectile(Actor owner, ProjectileKind kind, Actor? target = null, bool spawnSoundSource = false)
        => SpawnProjectileCore(owner, kind, target, null, spawnSoundSource);

    private ProjectileActor SpawnProjectileCore(Actor? owner, ProjectileKind kind, Actor? target, uint? savedId, bool spawnSoundSource = false)
    {
        var projectile = new ProjectileActor(owner, kind)
        {
            Id = savedId ?? _nextActorId, Level = Level, Simulation = this,
            SpawnSoundSource = spawnSoundSource,
            X = owner?.X ?? default, Y = owner?.Y ?? default,
            Z = Fixed.FromDouble(owner is PlayerPawn ? owner.AttackOriginZ(-4)
                : (owner?.Z.ToDouble() ?? 0) + (kind == ProjectileKind.RevenantTracer ? 48 : 32)
                    + (owner?.GetBobOffset() ?? 0) - (owner?.FloorClip ?? 0)),
            Angle = owner?.Angle ?? default,
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
        ActorSpawner.ApplyFullVolumeSoundDefaults(projectile, defaults);
        if (defaults is not null && defaults.SoundNameOverrides.Count != 0)
        {
            var sounds = new Dictionary<ActorSoundType, string>(projectile.ActorSounds);
            foreach (var (type, name) in defaults.SoundNameOverrides) sounds[(ActorSoundType)type] = name;
            projectile.ActorSounds = sounds;
        }
        if (defaults?.RenderStyle is { } renderStyle) projectile.RenderStyle = ActorRenderStyle.FromDehacked(renderStyle);
        projectile.DefaultActorSounds = new Dictionary<ActorSoundType, string>(projectile.ActorSounds);
        if (defaults?.Alpha is { } alpha) projectile.Alpha = alpha;
        if (defaults?.Invisible is { } invisible) projectile.Invisible = invisible;
        if (defaults is { BitsPatched: true }) projectile.Shadow = (defaults.Bits & 0x40000) != 0;
        projectile.Stealth = defaults is { BitsPatched: true, BitsUseStealth: true } && (defaults.Bits & 0x40000000) != 0;
        projectile.DefaultStealth = projectile.Stealth;
        projectile.DefaultShadow = projectile.Shadow;
        projectile.DefaultRenderStyle = projectile.RenderStyle;
        projectile.DefaultAlpha = projectile.Alpha;
        if (defaults is { InfightingGroupPatched: true }) projectile.InfightingGroup = defaults.InfightingGroup;
        if (defaults is { ProjectileGroupPatched: true }) projectile.ProjectileGroup = defaults.ProjectileGroup;
        if (defaults is { SplashGroupPatched: true }) projectile.SplashGroup = defaults.SplashGroup;
        if (defaults is { MissileDamagePatched: true }) projectile.Damage = defaults.MissileDamage;
        if (defaults is { SpeedPatched: true }) projectile.MovementSpeed = Fixed.FromDouble(defaults.Speed);
        if (defaults is { MassPatched: true }) projectile.Mass = defaults.Mass;
        if (defaults is { ReactionTimePatched: true }) projectile.ReactionTime = defaults.ReactionTime;
        projectile.SpawnReactionTime = projectile.ReactionTime;
        projectile.SpawnTuning = new(projectile.MovementSpeed.Raw, projectile.FloatSpeed, projectile.PainThreshold);
        if (defaults is { WidthPatched: true }) projectile.Radius = Fixed.FromDouble(defaults.Radius);
        if (defaults is { HeightPatched: true }) projectile.Height = Fixed.FromDouble(defaults.Height);
        projectile.ResurrectionRadius = projectile.Radius;
        projectile.ResurrectionHeight = projectile.Height;
        projectile.SpawnDamage = projectile.Damage;
        if (defaults is { BitsPatched: true })
        {
            ActorSpawner.ApplyPhysicalPrimaryFlags(projectile, defaults.Bits);
            ActorSpawner.ApplyContactPrimaryFlags(projectile, defaults, playerStart: false);
            projectile.NoBlood = projectile.DefaultNoBlood = (defaults.Bits & 0x00080000) != 0;
        }
        ActorSpawner.ApplyExtendedDefaults(projectile, defaults, FastMonsters);
        if (defaults is { HealthPatched: true })
        {
            var health = projectile.IsMonster && defaults.Health != 0 ? Math.Max(1, defaults.Health) : defaults.Health;
            projectile.RestoreHealth(health);
            projectile.ResurrectionHealth = health;
            if (health > 0) projectile.GibHealth = int.MinValue;
            projectile.DefaultGibHealth = projectile.GibHealth;
        }
        projectile.ResurrectionCollisionFlags = ActorSpawner.CollisionFlagsOf(projectile);
        _nextActorId = checked(_nextActorId + 1);
        projectile.Aim(target);
        _actors.Add(projectile);
        Thinkers.Add(projectile);
        if (savedId is null)
        {
            ActorSoundActions.PlaySpawnSound(projectile, owner);
            MissileSpawnCheck.Check(this, projectile, owner, owner?.Radius.ToDouble() ?? 0);
        }
        return projectile;
    }

    private Actor CreateLostSoulActor(uint id, Fixed x, Fixed y, Fixed z, int sectorIndex, double angle)
    {
        var definition = DoomActorCatalog.Find(3006)!;
        var defaults = _dehacked?.Actors.FirstOrDefault(actor => actor.OriginalDoomEdNum == 3006 && actor.Patched);
        var health = defaults?.Health ?? definition.Health;
        var soul = new Actor
        {
            Id = id, DoomEdNum = defaults?.DoomEdNum ?? 3006, DefinitionDoomEdNum = 3006, Level = Level, Simulation = this,
            SpawnedLostSoul = true,
            X = x, Y = y, Z = z,
            Radius = Fixed.FromInt(definition.Radius), Height = Fixed.FromInt(definition.Height),
            Health = health, ResurrectionHealth = health,
            GibHealth = health > 0 ? int.MinValue : -1,
            PainChance = definition.PainChance, ChaseSpeed = definition.Speed / 4.0,
            NoGravity = true, OnGround = false, SectorIndex = sectorIndex,
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
        ActorSpawner.ApplySoundDefaults(soul, defaults);
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
        soul.DefaultGibHealth = soul.GibHealth;
        ActorSpawner.ApplyExtendedDefaults(soul, defaults, FastMonsters);
        if (soul.IsMonster && Skill == 4) soul.ReactionTime = 0;
        ThingActivation.InitializeSpawn(soul, false, mapSpawn: false);
        return soul;
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
        var soul = CreateLostSoulActor(_nextActorId, parent.X, parent.Y,
            Fixed.FromDouble(parent.Z.ToDouble() + 8), parent.SectorIndex, angle);
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
                    soul.FailedLostSoulSpawn = true;
                    ActorDamage.Apply(soul, ActorDamage.TelefragDamage, parent, inflictor: parent);
                    soul.RememberPosition();
                    _actors.Add(soul);
                    Thinkers.Add(soul);
                    return null;
                }
        }
        finally { parent.Solid = solid; soul.NoTeleport = noTeleport; }
        soul.SpawnedLostSoul = true;
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
        var firstExit = !Exited;
        Exited = true;
        SecretExit = secret;
        if (firstExit)
        {
            _soundRequests.Clear();
            LevelExited?.Invoke();
        }
    }

    public bool DamageExitAllowed { get; }

    public string Describe() =>
        $"OK map={Level.MapName} tic={Thinkers.Clock.Tic} players={Players.Count()} bots={_actors.OfType<BotPawn>().Count()} checksum={Checksum:x8} invasion={Invasion.Phase} exited={(Exited ? 1 : 0)} secret={(SecretExit ? 1 : 0)}";

    private static SimAmmoInventory? CaptureAmmo(PlayerInventory inventory)
    {
        var ammo = new SimAmmoInventory(inventory.Bullets, inventory.Shells, inventory.Rockets, inventory.Cells,
            inventory.MaxBullets, inventory.MaxShells, inventory.MaxRockets, inventory.MaxCells, inventory.HasBackpack);
        return ammo == new SimAmmoInventory(50, 0, 0, 0, 200, 50, 50, 300, false) ? null : ammo;
    }

    private IReadOnlyList<SimThinkerPlacement> CaptureThinkerOrder()
    {
        var order = Thinkers.SnapshotOrder().Where(thinker => !thinker.Destroyed).ToArray();
        if (order.Any(thinker => thinker is not Actor actor || !_actors.Contains(actor)))
            throw new InvalidOperationException("Only simulation actors support saved thinker order.");
        return order.Cast<Actor>().Select(actor => new SimThinkerPlacement(actor.Id, actor.StatNum, actor.JustSpawned)).ToArray();
    }

    private SimAcsState? CaptureAcsState()
    {
        var captured = Acs.CaptureState(_acsRandomState);
        return captured.IsDefault ? null : captured;
    }

    public SimSaveState CaptureState(bool includeBotIdentities = false) =>
        CaptureState(includeBotIdentities, includeThinkerOrder: includeBotIdentities);

    public SimSaveState CaptureState(bool includeBotIdentities, bool includeThinkerOrder)
    {
        if (includeThinkerOrder && !includeBotIdentities)
            throw new ArgumentException("Thinker order requires actor identity state.", nameof(includeThinkerOrder));
        var state = new SimSaveState
        {
            IncludesBotIdentities = includeBotIdentities,
            NextActorId = includeBotIdentities ? _nextActorId : null,
            Invasion = includeBotIdentities ? Invasion.CaptureState() : null,
            ThinkerOrder = includeThinkerOrder ? CaptureThinkerOrder() : null,
            LightAnimation = HasLightAnimationOverride || LightEffects.Count != 0
                ? new SimLightAnimation(_strobeRandomState, _flickerRandomState, _lightFlashRandomState, _fireFlickerRandomState,
                    LightEffects.Select(e => new SimLightEffect(e.Sector, (int)e.Kind, e.Start, e.End, e.Duration, e.DarkTime, e.Tics)).ToList()) : null,
            Lights = HasLightOverride || HasLightAnimationOverride || LightEffects.Count != 0 || Lights.Where((light, index) => light != Level.Sectors[index].LightLevel).Any() ? Lights.ToList() : null,
            JumpRandomState = _hasJumpRandomState ? _jumpRandomState : null,
            StateRandomState = _hasStateRandomState ? _stateRandomState : null,
            MapSpawnRandomState = _hasMapSpawnRandomState ? _mapSpawnRandomState : null,
            FloatSpawnRandomState = _hasFloatSpawnRandomState ? _floatSpawnRandomState : null,
            IceTicsRandomState = _hasIceTicsRandomState ? _iceTicsRandomState : null,
            IncludesIceChunkLifecycle = _hasIceChunkLifecycle,
            FreezeChunksRandomState = _hasFreezeChunksRandomState ? _freezeChunksRandomState : null,
            BfgSelfDamageRandomState = _hasBfgSelfDamageRandomState ? _bfgSelfDamageRandomState : null,
            BfgSprayRandomState = _hasBfgSprayRandomState ? _bfgSprayRandomState : null,
            FireSg2RandomState = _hasFireSg2RandomState ? _fireSg2RandomState : null,
            GunShotRandomState = _hasGunShotRandomState ? _gunShotRandomState : null,
            ForcedFallingDamage = ForcedFallingDamage,
            AirControlOverride = _airControlOverride,
            GibFactor = GibFactor != 1 ? GibFactor : null,
            LevelGravityOverride = _levelGravityOverride,
            FreezeDeathRandomState = _hasFreezeDeathRandomState ? _freezeDeathRandomState : null,
            PlayerSearchRandomState = _hasPlayerSearchRandomState ? _playerSearchRandomState : null,
            PlayerLookRandomState = _hasPlayerLookRandomState ? _playerLookRandomState : null,
            CompatibilityInvisibility = CompatibilityInvisibility,
            VerticalWeaponSpread = VerticalWeaponSpread,
            HateSearchRandomState = _hasHateSearchRandomState ? _hateSearchRandomState : null,
            ActorDieRandomState = _hasActorDieRandomState ? _actorDieRandomState : null,
            PuffRandomState = _hasPuffRandomState ? _puffRandomState : null,
            DropItemRandomState = _hasDropItemRandomState ? _dropItemRandomState : null,
            ChaseRandomState = _hasChaseRandomState ? _chaseRandomState : null,
            MissileDamageRandomState = _hasMissileDamageRandomState ? _missileDamageRandomState : null,
            PoisonDamageRandomState = _hasPoisonDamageRandomState ? _poisonDamageRandomState : null,
            ReflectRandomState = _hasReflectRandomState ? _reflectRandomState : null,
            SeekerMissileRandomState = _hasSeekerMissileRandomState ? _seekerMissileRandomState : null,
            PainChanceRandomState = _hasPainChanceRandomState ? _painChanceRandomState : null,
            SkelFistRandomState = _hasSkelFistRandomState ? _skelFistRandomState : null,
            SawRandomState = _hasSawRandomState ? _sawRandomState : null,
            PunchRandomState = _hasPunchRandomState ? _punchRandomState : null,
            NewChaseDirRandomState = _newChaseDirRandomState,
            TryWalkRandomState = _tryWalkRandomState,
            AvoidCrushRandomState = _avoidCrushRandomState,
            StayOnLiftRandomState = _stayOnLiftRandomState,
            ScaredyCatRandomState = _scaredyCatRandomState,
            CheckMissileRandomState = _checkMissileRandomState,
            DefectRandomState = _defectRandomState,
            CheckSightRandomState = _checkSightRandomState,
            UniqueTidRandomState = _uniqueTidRandomState,
            SectorDamageRandomState = _sectorDamageRandomState,
            NativeSwitchTargetRandomState = _nativeSwitchTargetRandomState,
            SwitchTargetRandomState = SwitchTargetRandomState == (unchecked((uint)RngSeed) ^ 0x73776974u)
                ? null : SwitchTargetRandomState,
            EnemyStrafeRandomState = _enemyStrafeRandomState,
            SkipTargetRandomState = _hasSkipTargetRandomState ? _skipTargetRandomState : null,
            CPosRefireRandomState = _hasCPosRefireRandomState ? _cPosRefireRandomState : null,
            CPosAttackRandomState = _hasCPosAttackRandomState ? _cPosAttackRandomState : null,
            SPosAttackRandomState = _hasSPosAttackRandomState ? _sPosAttackRandomState : null,
            PosAttackRandomState = _hasPosAttackRandomState ? _posAttackRandomState : null,
            BruisAttackRandomState = _hasBruisAttackRandomState ? _bruisAttackRandomState : null,
            HeadAttackRandomState = _hasHeadAttackRandomState ? _headAttackRandomState : null,
            TroopAttackRandomState = _hasTroopAttackRandomState ? _troopAttackRandomState : null,
            SargAttackRandomState = _hasSargAttackRandomState ? _sargAttackRandomState : null,
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
            SectorTerrains = Level.Sectors.Select(s => new SimSectorTerrain(s.FloorTerrain, s.CeilingTerrain))
                .SequenceEqual(_initialSectorTerrains) ? null
                : Level.Sectors.Select(s => new SimSectorTerrain(s.FloorTerrain, s.CeilingTerrain)).ToArray(),
            SectorSoundTargets = _sectorSoundTargets.Where(pair => Actors.Any(a => a.Id == pair.Value && !a.Destroyed))
                .ToDictionary(pair => pair.Key, pair => pair.Value),
            Acs = CaptureAcsState(),
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
            if (includeBotIdentities && actor is PuffActor && actor.Brain is not null)
                throw new InvalidOperationException("Cannot save a puff with a custom brain.");
            if (includeBotIdentities && actor.SpawnedDroppedPickup && actor.Brain is not null)
                throw new InvalidOperationException("Dropped pickup brains cannot be saved as pickup identities.");
            if (includeBotIdentities && actor.MapDecorationThingIndex.HasValue && actor.Brain is not null)
                throw new InvalidOperationException("Map decoration brains cannot be saved as decoration identities.");
            if (includeBotIdentities && actor.MapPickupThingIndex.HasValue && actor.Brain is not null)
                throw new InvalidOperationException("Map pickup brains cannot be saved as pickup identities.");
            state.Actors.Add(new SimActorPose
            {
                AmbushFlag = actor.Ambush != actor.DefaultAmbush ? actor.Ambush ? 1 : 0 : null,
                SpawnDeathFlag = actor.PendingSpawnDeath ? 1 : null,
                IsPuffFlag = actor.IsPuff != actor.DefaultIsPuff ? actor.IsPuff ? 1 : 0 : null,
                GibHealthOverride = actor.GibHealth != actor.DefaultGibHealth ? actor.GibHealth : null,
                LastLookActorId = actor.LastLookActorId,
                HateTargetingFlags = actor.NoSightCheck || actor.HuntPlayers ? (actor.NoSightCheck ? 1 : 0) | (actor.HuntPlayers ? 2 : 0) : null,
                VulnerableFlag = actor.Vulnerable ? 1 : null,
                MonsterClassificationFlag = actor.IsMonster != ((actor.ResurrectionCollisionFlags.GetValueOrDefault() & 32) != 0)
                    ? actor.IsMonster ? 1 : 0 : null,
                NoTargetFlag = actor.NoTarget ? 1 : null,
                BuddhaFlag = actor.Buddha ? 1 : 0,
                FlyCheatFlag = actor.FlyCheat ? 1 : 0,
                NoClipFlag = actor.NoClip ? 1 : 0,
                PlayerNoClip2Flag = actor is PlayerPawn noClipPlayer && noClipPlayer.NoClip2 ? 1 : 0,
                PlayerFrozenFlags = actor is PlayerPawn frozenPlayer
                    ? (frozenPlayer.Frozen ? 1 : 0) | (frozenPlayer.TotallyFrozen ? 2 : 0) : 0,
                PlayerNoTargetCheatOverride = actor is PlayerPawn noTargetPlayer
                    ? noTargetPlayer.NoTargetCheatOverride is { } cheat ? cheat ? 1 : 0 : null : null,
                ChaseGoalFlag = actor.ChaseGoal ? 1 : null,
                JustAttackedFlag = actor.JustAttacked ? 1 : null,
                InCombatFlag = actor.InCombat ? 1 : null,
                NoSplashAlertFlag = actor.NoSplashAlert ? 1 : null,
                DontSplashFlag = actor.DontSplash ? 1 : null,
                DontHarmSpeciesFlag = actor.DontHarmSpecies ? 1 : null,
                DontHarmClassFlag = actor.DontHarmClass ? 1 : null,
                GruntSpeed = actor is PlayerPawn gruntPlayer ? gruntPlayer.GruntSpeed : PlayerPawn.DefaultGruntSpeed,
                DeltaViewHeight = actor is PlayerPawn squatPlayer ? squatPlayer.DeltaViewHeight : 0,
                LandingSpeed = actor.LandingSpeed,
                PushHistoryFlag = actor.LastPushFromMovement ? 1 : null,
                BlockedBySolidActorsFlag = actor.BlockedBySolidActors ? 1 : null,
                BumpSpecialFlag = actor.BumpSpecial ? 1 : null,
                SeekerMissileFlag = actor.SeekerMissile != (actor is ProjectileActor { Kind: ProjectileKind.RevenantTracer })
                    ? actor.SeekerMissile ? 1 : 0 : null,
                SummonedMonsterFlag = actor.SummonedMonster ? 1 : null,
                ScreenSeekerFlag = actor.ScreenSeeker ? 1 : null,
                VisibilityPulseFlag = actor.VisibilityPulse ? 1 : null,
                VisibilityDirection = actor.VisibilityDirection,
                StealthFlag = actor.Stealth ? 1 : null,
                StealthDefaultsFlag = actor.DefaultStealth ? 1 : null,
                FloatBobDefaultsFlag = actor.DefaultFloatBob ? 1 : null,
                ReflectiveDefaultsFlag = actor.DefaultReflective ? 1 : null,
                ThruGhostDefaultsFlag = actor.DefaultThruGhost ? 1 : null,
                NonShootableDefaultsFlag = actor.DefaultNonShootable ? 1 : null,
                FloorPicDefaultsFlag = actor.DefaultFloorPic ? 1 : null,
                RipDefaultsFlag = actor.DefaultRip ? 1 : null,
                FloorClipDefaultsFlag = actor.DefaultFloorClip ? 1 : null,
                BlastedDefaultsFlag = actor.DefaultBlasted ? 1 : null,
                TouchyDefaultsFlag = actor.DefaultTouchy ? 1 : null,
                NoClipDefaultsFlag = actor.DefaultNoClip ? 1 : null,
                CombatDefaultsFlags = actor.DefaultCombatFlags != 0 ? actor.DefaultCombatFlags : null,
                InFloatDefaultsFlag = actor.DefaultInFloat ? 1 : null,
                CorpseDefaultsFlag = actor.DefaultCorpse ? 1 : null,
                FlyDefaultsFlag = actor.DefaultFly ? 1 : null,
                PainChanceOverride = actor.PainChance != actor.SpawnPainChance.GetValueOrDefault(256)
                    ? actor.PainChance : null,
                SpawnFloatFlags = actor.SpawnFloat || actor.DefaultSpawnFloat
                    ? (actor.SpawnFloat ? 1 : 0) | (actor.DefaultSpawnFloat ? 2 : 0) : null,
                SpawnCeilingOverrideFlag = actor.SpawnCeiling != ((actor.ResurrectionCollisionFlags.GetValueOrDefault() & 64) != 0)
                    ? actor.SpawnCeiling ? 1 : 0 : null,
                NoTeleportOverrideFlag = actor is not ProjectileActor
                    && actor.NoTeleport != actor.DefaultNoTeleport
                    ? actor.NoTeleport ? 1 : 0 : null,
                PushDefaultsFlags = actor.DefaultPushFlags != 0 ? actor.DefaultPushFlags : null,
                TeleStompDefaultsFlag = actor.DefaultTeleStomp != (actor is PlayerPawn)
                    ? actor.DefaultTeleStomp ? 1 : 0 : null,
                SeekerDefaultsFlag = actor.DefaultSeeker != (actor is ProjectileActor { Kind: ProjectileKind.RevenantTracer })
                    ? actor.DefaultSeeker ? 1 : 0 : null,
                ProjectileSecondaryFlags = actor is ProjectileActor && ((actor.NoTeleport ? 1 : 0) | (actor.Dormant ? 2 : 0)) != 1
                    ? (actor.NoTeleport ? 1 : 0) | (actor.Dormant ? 2 : 0) : null,
                StealthAlpha = actor.StealthAlpha,
                FloatBobSettings = actor.FloatBob || actor.FloatBobStrength != 1 || actor.FloatBobFactor != 1
                    ? new SimFloatBobSettings(actor.FloatBob, actor.FloatBobStrength, actor.FloatBobFactor) : null,
                ReflectionFlags = actor.Reflective || actor.ThruReflect || actor.DontReflect || actor.MirrorReflect || actor.Deflect
                    || actor.ShieldReflect || actor.NoShieldReflect || actor.AimReflect
                    ? (actor.Reflective ? 1 : 0) | (actor.ThruReflect ? 2 : 0) | (actor.DontReflect ? 4 : 0)
                        | (actor.MirrorReflect ? 8 : 0) | (actor.Deflect ? 16 : 0)
                        | (actor.ShieldReflect ? 32 : 0) | (actor.NoShieldReflect ? 64 : 0) | (actor.AimReflect ? 128 : 0) : null,
                BumpCooldownTic = actor.LastBumpTic,
                FallDamageFlag = actor.FallDamage ? 1 : null,
                SmashableFlag = actor.Smashable ? 1 : null,
                CrashedFlag = actor.Crashed ? 1 : null,
                DontGibFlag = actor.DontGib ? 1 : null,
                TeleStompFlags = actor.TeleStomp != (actor is PlayerPawn) || actor.NoTeleStomp
                    ? (actor.TeleStomp ? 1 : 0) | (actor.NoTeleStomp ? 2 : 0) : null,
                CrashStateOverride = actor.CrashState != -1 ? actor.CrashState : null,
                ExtremeCrashStateOverride = actor.ExtremeCrashState != -1 ? actor.ExtremeCrashState : null,
                CrushStateOverride = actor.CrushState != -1 ? actor.CrushState : null,
                TeleportFreezeTime = actor is PlayerPawn teleportPlayer ? teleportPlayer.TeleportFreezeTime : 18,
                TouchyImpactFlags = actor.Touchy || actor.Armed ? (actor.Touchy ? 1 : 0) | (actor.Armed ? 2 : 0) : null,
                RespawnEarliestTic = actor is PlayerPawn respawnPlayer ? respawnPlayer.RespawnEarliestTic : int.MaxValue,
                RespawnArmed = actor is PlayerPawn armedPlayer && armedPlayer.RespawnArmed,
                PlayerExtraLight = actor is PlayerPawn lightPlayer ? lightPlayer.ExtraLight : 0,
                PowerStrengthTics = actor is PlayerPawn strengthPlayer ? strengthPlayer.PowerStrengthTics : 0,
                PlayerBonusCount = actor is PlayerPawn pickupFlashPlayer ? pickupFlashPlayer.BonusCount : 0,
                CombatBehaviorFlags = actor.HarmFriends || actor.DontDrain || actor.NoAutoOffSkullFly
                    ? (actor.HarmFriends ? 1 : 0) | (actor.DontDrain ? 2 : 0) | (actor.NoAutoOffSkullFly ? 4 : 0) : null,
                TargetSwitchFlags = actor.NeverTarget || actor.NoTargetSwitch || actor.QuickToRetaliate
                    ? (actor.NeverTarget ? 1 : 0) | (actor.NoTargetSwitch ? 2 : 0) | (actor.QuickToRetaliate ? 4 : 0) : null,
                InfightingFlags = actor.NoInfighting || actor.ForceInfighting || actor.DoHarmSpecies
                    ? (actor.NoInfighting ? 1 : 0) | (actor.ForceInfighting ? 2 : 0) | (actor.DoHarmSpecies ? 4 : 0) : null,
                Score = actor.Score,
                SeeInvisible = actor.SeeInvisible,
                LastLookPlayerNumber = actor.LastLookPlayerNumber,
                MVisBlockedFlag = actor.MVisBlocked ? 1 : null,
                NoBloodFlag = actor.NoBlood != actor.DefaultNoBlood ? actor.NoBlood ? 1 : 0 : null,
                PuffGetsOwnerFlag = actor.PuffGetsOwner ? 1 : null,
                CantSeekFlag = actor.CantSeek ? 1 : null,
                SpawnSoundSourceFlag = actor.SpawnSoundSource ? 1 : null,
                MoveWithSectorFlag = actor.MoveWithSector ? 1 : null,
                CanPassFlag = actor.CanPass != actor.DefaultCanPass ? actor.CanPass ? 1 : 0 : null,
                DontOverlapFlag = actor.DontOverlap ? 1 : null,
                ExplodeCountFlag = actor.ExplodeCount ? 1 : null,
                SkyExplodeFlag = actor.SkyExplode ? 1 : null,
                MissileHitPointerFlags = actor.HitTarget || actor.HitMaster || actor.HitTracer
                    ? (actor.HitTarget ? 1 : 0) | (actor.HitMaster ? 2 : 0) | (actor.HitTracer ? 4 : 0) : null,
                FloorCarryFlags = actor.NoLiftDrop || actor.RelativeToFloor
                    ? (actor.NoLiftDrop ? 1 : 0) | (actor.RelativeToFloor ? 2 : 0) : null,
                RenderProperties = actor is not ProjectileActor && (actor.RenderStyle != ActorRenderStyle.Normal || actor.Alpha != 1)
                    || actor.RenderStyle != actor.DefaultRenderStyle || actor.Alpha != actor.DefaultAlpha
                    ? new SimActorRenderProperties(actor.RenderStyle, actor.Alpha) : null,
                DontThrustFlag = actor.DontThrust ? 1 : null,
                DontSeekInvisibleFlag = actor.DontSeekInvisible ? 1 : null,
                SeekVisibilityFlags = actor.Shadow != actor.DefaultShadow || actor.MinVisible
                    ? (actor.Shadow ? 1 : 0) | (actor.MinVisible ? 2 : 0) : null,
                ActorSounds = actor is ProjectileActor soundMissile ? soundMissile.CaptureSounds()
                    : new Dictionary<ActorSoundType, string>(actor.ActorSounds),
                FullVolumeDeath = actor.FullVolumeDeath,
                FullVolumeSee = actor.FullVolumeSee,
                OnlySlamSolid = actor.OnlySlamSolid,
                CausePain = actor.CausePain,
                AllowPain = actor.AllowPain,
                NoDamage = actor.NoDamage,
                LaxTelefragDamage = actor.LaxTelefragDamage,
                RetargetAfterSlam = actor.RetargetAfterSlam,
                FullVolumeActive = actor.FullVolumeActive,
                DeathType = actor.DeathType,
                PainType = actor.PainType,
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
                TracerPointer = actor is not ProjectileActor && actor.HasTracerOverride ? new SimTracerPointer(actor.TracerId) : null,
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
                SectorLinkFlag = actor.HasSectorOverride
                    || actor.NoSector != ((actor.ResurrectionCollisionFlags.GetValueOrDefault() & 128) != 0)
                    ? actor.NoSector ? 1 : 0 : null,
                BlockmapFlag = actor.HasBlockmapOverride || actor.ResurrectionCollisionFlags is { } blockmapDefaults
                    && actor.NoBlockmap != ((blockmapDefaults & 4) != 0) ? actor.NoBlockmap ? 1 : 0 : null,
                Friendship = actor.HasFriendshipOverride || actor.NoHatePlayers
                    || actor.Friendly != actor.DefaultFriendly
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
                ChaseThreshold = actor.CaptureChaseThreshold(),
                TargetMemory = actor.HasTargetMemoryOverride || actor.TargetId is not null && actor is not ProjectileActor
                    || actor.Brain?.LastEnemyId is not null || actor.LastHeardTargetId is not null
                    ? new SimTargetMemory(actor is ProjectileActor ? actor.Brain?.TargetId : actor.TargetId, actor.Brain?.LastEnemyId, actor.LastHeardTargetId) : null,
                WornArmorType = actor is PlayerPawn armorPlayer
                    && (armorPlayer.Inventory.ArmorType != "None" || armorPlayer.Inventory.Armor != 0)
                    ? armorPlayer.Inventory.ArmorType : null,
                WornArmorAmount = actor is PlayerPawn amountPlayer ? amountPlayer.Inventory.Armor : 0,
                WornArmor = actor is PlayerPawn metadataPlayer ? SimWornArmor.Capture(metadataPlayer.Inventory) : null,
                ActorArmor = SimActorArmor.Capture(actor),
                SpareArmor = actor is PlayerPawn sparePlayer && sparePlayer.Inventory.SpareArmor.Count != 0
                    ? sparePlayer.Inventory.SpareArmor.ToArray() : null,
                DamageFactor = actor.DamageFactor.Raw,
                SelfDamageFactor = actor.SelfDamageFactor.Raw,
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
                CameraHeight = actor.CameraHeight,
                PowerIronFeetMode = actor is PlayerPawn modePlayer ? (int)modePlayer.PowerIronFeetMode : 0,
                PowerMaskTics = actor is PlayerPawn maskPlayer ? maskPlayer.PowerMaskTics : 0,
                PowerIronFeetTics = actor is PlayerPawn feetPlayer ? feetPlayer.PowerIronFeetTics : 0,
                PowerInfiniteAmmoTics = actor is PlayerPawn infiniteAmmoPlayer ? infiniteAmmoPlayer.PowerInfiniteAmmoTics : 0,
                PowerDrainTics = actor is PlayerPawn timedDrainPlayer ? timedDrainPlayer.PowerDrainTics : 0,
                PowerDrainStrength = actor is PlayerPawn drainStrengthPlayer ? drainStrengthPlayer.PowerDrainStrength : 0.5,
                PowerRegenerationTics = actor is PlayerPawn regenerationPlayer ? regenerationPlayer.PowerRegenerationTics : 0,
                PowerProsperityTics = actor is PlayerPawn prosperityPlayer ? prosperityPlayer.PowerProsperityTics : 0,
                PowerSpreadTics = actor is PlayerPawn spreadPlayer ? spreadPlayer.PowerSpreadTics : 0,
                PowerRegenerationStrength = actor is PlayerPawn regenerationStrengthPlayer ? regenerationStrengthPlayer.PowerRegenerationStrength : 5,
                PowerDoubleFiringSpeedTics = actor is PlayerPawn firingSpeedPlayer ? firingSpeedPlayer.PowerDoubleFiringSpeedTics : 0,
                PowerHighJumpTics = actor is PlayerPawn highJumpPlayer ? highJumpPlayer.PowerHighJumpTics : 0,
                PowerHighJumpPendingRemoval = actor is PlayerPawn transientJumpPlayer && transientJumpPlayer.PowerHighJumpPendingRemoval,
                PowerHighJumpStrength = actor is PlayerPawn strengthJumpPlayer ? strengthJumpPlayer.PowerHighJumpStrength : 2,
                PlayerDrainStrength = actor is PlayerPawn drainPlayer ? drainPlayer.DrainStrength : 0,
                PlayerGodMode2 = actor is PlayerPawn godPlayer && godPlayer.GodMode2,
                PlayerBuddha2 = actor is PlayerPawn buddha2Player && buddha2Player.Buddha2,
                PlayerGodMode = actor is PlayerPawn godModePlayer && godModePlayer.GodMode,
                PlayerDamageCount = actor is PlayerPawn flashPlayer ? flashPlayer.DamageCount : 0,
                PlayerPoisonCount = actor is PlayerPawn poisonPlayer ? poisonPlayer.PoisonCount : 0,
                HealthPickups = actor is PlayerPawn pickupHealthPlayer ? pickupHealthPlayer.Inventory.HealthPickups.ToArray() : null,
                PlayerAmmo = actor is PlayerPawn ammoPlayer ? CaptureAmmo(ammoPlayer.Inventory) : null,
                PlayerWeapons = actor is PlayerPawn weaponPlayer
                    && (weaponPlayer.Inventory.Weapons != (WeaponKind.Fist | WeaponKind.Pistol)
                        || weaponPlayer.Inventory.Selected != WeaponKind.Pistol || weaponPlayer.Inventory.Pending.HasValue
                        || weaponPlayer.WeaponOffsetY != PlayerPawn.WeaponTop || weaponPlayer.WeaponLowering
                        || weaponPlayer.InstantWeaponSwitch || weaponPlayer.AttackPressed
                        || weaponPlayer.VelocityX.Raw != 0 || weaponPlayer.VelocityY.Raw != 0
                        || weaponPlayer.BobTimer != Thinkers.Clock.Tic || weaponPlayer.ViewBobOffset != 0 || weaponPlayer.MovementBob != 0
                        || weaponPlayer.WeaponBobX != 0 || weaponPlayer.WeaponBobY != 0
                        || weaponPlayer.TurnTicks != 0 || weaponPlayer.TurnHeld)
                    ? new SimWeaponInventory(weaponPlayer.Inventory.Weapons, weaponPlayer.Inventory.Selected, weaponPlayer.Inventory.Pending,
                        weaponPlayer.WeaponOffsetY, weaponPlayer.WeaponLowering, weaponPlayer.InstantWeaponSwitch, weaponPlayer.AttackPressed,
                        weaponPlayer.VelocityX.Raw != 0 || weaponPlayer.VelocityY.Raw != 0
                            || weaponPlayer.BobTimer != Thinkers.Clock.Tic || weaponPlayer.ViewBobOffset != 0 || weaponPlayer.MovementBob != 0
                            || weaponPlayer.WeaponBobX != 0 || weaponPlayer.WeaponBobY != 0 ? weaponPlayer.BobTimer : null,
                        weaponPlayer.ViewBobOffset, weaponPlayer.MovementBob, weaponPlayer.WeaponBobX, weaponPlayer.WeaponBobY,
                        weaponPlayer.TurnTicks, weaponPlayer.TurnHeld) : null,
                PlayerKeyBits = actor is PlayerPawn keyPlayer ? (keyPlayer.Inventory.BlueKey ? 1 : 0)
                    | (keyPlayer.Inventory.RedKey ? 2 : 0) | (keyPlayer.Inventory.YellowKey ? 4 : 0) : 0,
                PoisonSourceId = (actor as PlayerPawn)?.PoisonSourceId,
                PoisonDamageType = (actor as PlayerPawn)?.PoisonDamageType,
                PoisonPainType = (actor as PlayerPawn)?.PoisonPainType,
                DamageType = actor.DamageType,
                PlayerLastDamageType = (actor as PlayerPawn)?.LastDamageType,
                DeathDamageAmount = actor.DeathDamageAmount,
                DamageHistory = includeBotIdentities && (actor.DeathCount != 0 || actor.LastDamageSourceId.GetValueOrDefault() != 0)
                    ? new(actor.DeathCount, actor.LastDamageSourceId.GetValueOrDefault()) : null,
                Id = actor.Id,
                PuffIdentity = includeBotIdentities && actor is PuffActor savedPuff
                    ? new(actor.ThingId, actor.JustSpawned, actor.StatNum, savedPuff.DamageSource is { Destroyed: false } source ? source.Id : 0) : null,
                MapMonsterThingIndex = includeBotIdentities ? actor.MapMonsterThingIndex : null,
                MapDecorationIdentity = includeBotIdentities && actor.MapDecorationThingIndex is { } decorationIndex
                    ? new(decorationIndex, actor.DoomEdNum, actor.ThingId, actor.JustSpawned, actor.StatNum,
                        actor.ClassDoomEdNum != actor.DoomEdNum ? actor.ClassDoomEdNum : 0) : null,
                MapPickupIdentity = includeBotIdentities && actor.MapPickupThingIndex is { } thingIndex
                    ? new(thingIndex, actor.DoomEdNum, actor.ThingId, actor.JustSpawned, actor.StatNum) : null,
                DroppedPickupIdentity = includeBotIdentities && actor.SpawnedDroppedPickup
                    ? new(actor.DoomEdNum, actor.ThingId, actor.JustSpawned, actor.StatNum) : null,
                BasicAttackState = includeBotIdentities && (actor is BotPawn || actor.SpawnedLostSoul || actor.MapMonsterThingIndex.HasValue) ? actor.Brain?.CaptureBasicAttackState() : null,
                ProfileChaseHistory = includeBotIdentities && (actor is BotPawn || actor.SpawnedLostSoul || actor.MapMonsterThingIndex.HasValue) ? actor.Brain?.CaptureProfileChaseHistory() : null,
                BotIdentity = includeBotIdentities && (actor is BotPawn || actor.SpawnedLostSoul || actor.MapMonsterThingIndex.HasValue)
                    ? new(actor.DoomEdNum, actor.ThingId, actor.JustSpawned, actor.Brain is not null, actor.StatNum,
                        actor.Brain?.BasicAttack, actor.Dormant,
                        actor.Brain?.ProfileType is { } profile && profile != actor.ClassDoomEdNum ? profile : null,
                        actor.SpawnedLostSoul, actor.FailedLostSoulSpawn) : null,
                X = actor.X.Raw,
                ActorSpecial = actor.SpecialChanged || actor.Special != 0 || actor.ActivationType != 0 || actor.SpecialArgs.Any(arg => arg != 0)
                    || actor.Stamina != 0 || actor is PlayerPawn { BonusHealth: not 0 } || actor is PlayerPawn { MaxPickupHealth: not 0 }
                    ? new SimActorSpecial(actor.Special, actor.SpecialArgs[0], actor.SpecialArgs[1], actor.SpecialArgs[2], actor.SpecialArgs[3], actor.SpecialArgs[4], actor.ActivationType,
                        actor.Stamina, actor is PlayerPawn bonusPlayer ? bonusPlayer.BonusHealth : 0,
                        actor is PlayerPawn pickupPlayer ? pickupPlayer.MaxPickupHealth : 0) : null,
                Y = actor.Y.Raw,
                Angle = actor.Angle.Raw,
                Roll = actor.Roll.Raw,
                ContactFlags = (actor.CanPickupItems ? 1 : 0) | (actor.SpecialPickup ? 2 : 0),
                FloatFlags = (actor.InFloat ? 1 : 0) | (actor.VerticalFriction ? 2 : 0),
                IceCorpseFlag = _hasFreezeDeathRandomState ? actor.IceCorpse : null,
                DeathFlags = actor.DeathDamageType == "Massacre" ? 1 : 0,
                PainDeath = actor.Brain?.CapturePainDeath(),
                RepeatAttack = actor.Brain?.CaptureRepeatAttack(),
                ProfileAttack = actor.Brain?.CaptureProfileAttack(),
                RevenantAttack = actor.Brain?.CaptureRevenantAttack(),
                ArchvileAttack = actor.Brain?.CaptureArchvileAttack(),
                LostSoulAttack = actor.Brain?.CaptureLostSoulAttack(),
                BrainDisabled = actor.Brain is { Enabled: false },
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
                    ? new SimProjectilePointers(pointerProjectile.Owner?.Id ?? 0, pointerProjectile.TracerTargetId) : null,
                Pickup = PickupCatalog.IsPickup(actor.DoomEdNum, RavenHealthPickups)
                    ? new SimPickupProperties(actor.PickupAmount, actor.IgnoreAmmoSkill, actor.Depleted) : null,
                PickupDelay = actor.PickupDelay,
                PickupPresentationFlags = (actor.PickupQuiet ? 1 : 0) | (actor.NoScreenFlash ? 2 : 0),
                PickupNoAttenuationFlag = actor.NoAttenPickupSound ? 1 : 0,
                PickupHealthOverride = actor.PickupHealthOverride,
                Accuracy = actor.Accuracy,
                FriendlySeeBlocks = actor.FriendlySeeBlocks,
                SeeFriendlyMonstersFlag = actor.SeeFriendlyMonsters ? 1 : 0,
                LookBehaviorFlags = (actor.LookAllAround ? 1 : 0) | (actor.StandStill ? 2 : 0),
                InConversationFlag = actor.InConversation ? 1 : 0,
                DontFollowPlayersFlag = actor.DontFollowPlayers ? 1 : 0,
                CanJumpFlag = actor.CanJump ? 1 : 0,
                BlockAsPlayerFlag = actor.BlockAsPlayer ? 1 : 0,
                FloorClipFlags = (actor.FloorClipEnabled ? 1 : 0) | (actor.SpecialFloorClip ? 2 : 0),
                AvoidHazardsFlag = actor.AvoidHazards ? 1 : 0,
                StayOnLiftFlag = actor.StayOnLift ? 1 : 0,
                AvoidMeleeFlag = actor.AvoidMelee ? 1 : 0, StrafeCount = actor.StrafeCount,
                FastChaseStrafeCount = actor.FastChaseStrafeCount,
                MinMissileChanceOverride = actor.MinMissileChance ?? -1,
                MissileChanceMultOverride = actor.MissileChanceMult,
                MaxTargetRangeOverride = actor.MaxTargetRange, MeleeThresholdOverride = actor.MeleeThreshold,
                CantLeaveFloorPicFlag = actor.CantLeaveFloorPic ? 1 : 0,
                FearFlags = (actor.NoFear ? 1 : 0) | (actor.Frightened ? 2 : 0) | (actor.Frightening ? 4 : 0),
                MoveDirection = actor.MoveDirection,
                MoveCount = actor.MoveCount,
                SoundClassOverride = actor is PlayerPawn { SoundClass: not null } soundPlayer ? new SimSoundClass(soundPlayer.SoundClass) : null,
                PickupAutoUseOverride = actor.PickupAutoUseOverride,
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
            WeaponRefiringFlag = actor is PlayerPawn { WeaponRefiring: true } ? 1 : null,
                PlayerInput = actor is PlayerPawn inputPlayer && inputPlayer.CapturePlayerInput() is { IsZero: false } capturedInput
                    ? capturedInput : null,
                Pitch = Fixed.FromDouble(actor.PitchDegrees).Raw,
                UseHeld = actor is PlayerPawn usingPawn && usingPawn.UseHeld,
                UseRange = actor is PlayerPawn rangePawn && rangePawn.HasUseRangeOverride ? rangePawn.UseRange : null,
            });
        }

        state.SectorMotions = _motions.Select(SimSectorMotionArchive.Capture).ToArray();
        var lineActions = Level.Lines.Select(SimLineAction.Capture).ToArray();
        state.LineActions = lineActions.SequenceEqual(_initialLineActions) ? null : lineActions;
        state.DoorMotions = _motions.Where(m => m.Kind == MotionKind.Door).Select(SimDoorMotionArchive.Capture).ToArray();
        for (var i = 0; i < Floors.Length; i++)
            state.Sectors.Add((Floors[i], Ceilings[i]));
        return state;
    }

    internal bool HasSavedActorAllocatorConflict(SimSaveState state) => state.NextActorId is { } next
        && _actors.Any(actor => actor.Id >= next && actor is not BotPawn && !actor.SpawnedLostSoul && !actor.SpawnedDroppedPickup
            && actor is not PuffActor && actor is not ProjectileActor && !(actor is IceChunkActor && state.IncludesIceChunkLifecycle));

    internal bool CanRestoreThinkerMembership(SimSaveState state)
    {
        if (state.ThinkerOrder is not { } order) return true;
        if (Thinkers.SnapshotOrder().Any(thinker => !thinker.Destroyed
            && (thinker is not Actor actor || !_actors.Contains(actor)))) return false;
        foreach (var placement in order)
        {
            var pose = state.Actors.Single(pose => pose.Id == placement.Id);
            var existing = _actors.FirstOrDefault(actor => actor.Id == placement.Id && !actor.Destroyed);
            var survives = existing is not null
                && !(existing is BotPawn && !pose.BotIdentity.HasValue)
                && !(existing is ProjectileActor && !pose.ProjectileLifetime.HasValue)
                && !(existing is IceChunkActor && state.IncludesIceChunkLifecycle && !pose.IceChunkLifecycle.HasValue);
            if (!survives && !pose.BotIdentity.HasValue && !pose.DroppedPickupIdentity.HasValue && !pose.MapPickupIdentity.HasValue && !pose.MapDecorationIdentity.HasValue && !pose.PuffIdentity.HasValue
                && !pose.ProjectileLifetime.HasValue && !pose.IceChunkLifecycle.HasValue)
                return false;
        }
        return true;
    }

    internal void RestoreState(SimSaveState state, bool notify)
    {
        var previous = _suppressRestoreNotifications;
        _suppressRestoreNotifications = previous || !notify;
        try { RestoreState(state); }
        finally { _suppressRestoreNotifications = previous; }
    }

    public void RestoreState(SimSaveState state)
    {
        SimLineActionsArchive.Validate(state);
        if (state.LineActions is { } actions && actions.Count != Level.Lines.Count)
            throw new InvalidOperationException("Saved line action count does not match the level.");
        SimSectorSoundArchive.Validate(state);
        SimPlayerExtraLightArchive.Validate(state);
        SimPowerStrengthArchive.Validate(state);
        SimPlayerBonusArchive.Validate(state);
        SimPickupPresentationArchive.Validate(state);
        SimPickupAttenuationArchive.Validate(state);
        SimPickupHealthArchive.Validate(state);
        SimPickupAutoUseArchive.Validate(state);
        SimDoorMotionArchive.Validate(state);
        SimSectorMotionArchive.Validate(state);
        SimAccuracyArchive.Validate(state);
        SimFriendlySeeBlocksArchive.Validate(state);
        SimSeeFriendlyMonstersArchive.Validate(state);
        SimLookBehaviorArchive.Validate(state);
        SimConversationArchive.Validate(state);
        SimDontFollowPlayersArchive.Validate(state);
        SimCanJumpArchive.Validate(state);
        SimFastChaseArchive.Validate(state);
        SimMinMissileChanceArchive.Validate(state);
        SimMissileChanceMultArchive.Validate(state);
        SimMaxTargetRangeArchive.Validate(state);
        SimMeleeThresholdArchive.Validate(state);
        if (state.CheckSightRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("CheckSight random state requires geometry state.");
        if (state.SwitchTargetRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("SwitchTarget random state requires geometry state.");
        if (state.UniqueTidRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Unique TID random state requires geometry state.");
        if (state.SectorDamageRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Sector damage random state requires geometry state.");
        if (state.NativeSwitchTargetRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Native SwitchTarget random state requires geometry state.");
        if (state.DefectRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("Defect random state requires geometry state.");
        SimBlockAsPlayerArchive.Validate(state);
        if (state.CheckMissileRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("CheckMissile random state requires geometry state.");
        if (state.ScaredyCatRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("ScaredyCat random state requires geometry state.");
        SimFloorClipFlagsArchive.Validate(state);
        SimAvoidHazardsArchive.Validate(state);
        SimStayOnLiftArchive.Validate(state);
        if (state.StayOnLiftRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("StayOnLift random state requires geometry state.");
        if (state.AvoidCrushRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("AvoidCrush random state requires geometry state.");
        SimMeleeAvoidanceArchive.Validate(state);
        SimCantLeaveFloorPicArchive.Validate(state);
        SimFearFlagsArchive.Validate(state);
        SimWanderArchive.Validate(state);
        if (state.SkipTargetRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("SkipTarget random archives require geometry state.");
        SimSoundClassArchive.Validate(state);
        SimGibFactorArchive.Validate(state);
        SimActorDieRandomArchive.Validate(state);
        SimBfgSelfDamageRandomArchive.Validate(state);
        SimBfgSprayRandomArchive.Validate(state);
        SimFireSg2RandomArchive.Validate(state);
        SimGunShotRandomArchive.Validate(state);
        SimVerticalWeaponSpreadArchive.Validate(state);
        SimPuffRandomArchive.Validate(state);
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
        SimSpawnBobPhaseArchive.Validate(state);
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
        SimPlayerDrainArchive.Validate(state);
        SimHealthPickupArchive.Validate(state);
        SimAmmoArchive.Validate(state);
        SimInfiniteAmmoPowerArchive.Validate(state);
        SimAmbushArchive.Validate(state);
        SimCantSeekArchive.Validate(state);
        SimGibHealthArchive.Validate(state);
        SimHateSearchArchive.Validate(state);
        SimHateTargetingArchive.Validate(state);
        SimVulnerableArchive.Validate(state);
        SimMVisBlockedArchive.Validate(state);
        SimPlayerLookArchive.Validate(state);
        SimPlayerSearchArchive.Validate(state);
        SimActorScoreArchive.Validate(state);
        SimInfightingFlagsArchive.Validate(state);
        SimTargetSwitchFlagsArchive.Validate(state);
        SimCombatBehaviorArchive.Validate(state);
        SimNoTargetArchive.Validate(state);
        SimPlayerNoTargetCheatArchive.Validate(state);
        SimBuddhaArchive.Validate(state);
        SimFlyCheatArchive.Validate(state);
        SimPlayerFrozenArchive.Validate(state);
        SimNoClipArchive.Validate(state);
        SimPlayerNoClip2Archive.Validate(state);
        SimIronFeetArchive.Validate(state);
        SimIronFeetModeArchive.Validate(state);
        SimPowerMaskArchive.Validate(state);
        SimCameraHeightArchive.Validate(state);
        SimDontSplashArchive.Validate(state);
        SimPlayerRespawnArchive.Validate(state);
        SimTouchyImpactArchive.Validate(state);
        SimDontHarmSpeciesArchive.Validate(state);
        SimDontHarmClassArchive.Validate(state);
        SimGruntSpeedArchive.Validate(state);
        SimDeltaViewHeightArchive.Validate(state);
        SimLandingSpeedArchive.Validate(state);
        SimForcedFallingArchive.Validate(state);
        SimPushHistoryArchive.Validate(state);
        SimBlockedBySolidActorsArchive.Validate(state);
        SimBumpSpecialArchive.Validate(state);
        SimReflectionArchive.Validate(state);
        SimReflectRandomArchive.Validate(state);
        SimSeekerRandomArchive.Validate(state);
        SimSeekerMissileArchive.Validate(state);
        SimSummonedMonsterArchive.Validate(state);
        SimScreenSeekerArchive.Validate(state);
        SimVisibilityPulseArchive.Validate(state);
        SimStealthArchive.Validate(state);
        SimStealthDefaultsArchive.Validate(state);
        SimFloatBobDefaultsArchive.Validate(state);
        SimSeekerDefaultsArchive.Validate(state);
        SimReflectiveDefaultsArchive.Validate(state);
        SimThruGhostDefaultsArchive.Validate(state);
        SimNonShootableDefaultsArchive.Validate(state);
        SimFloorPicDefaultsArchive.Validate(state);
        SimRipDefaultsArchive.Validate(state);
        SimPushDefaultsArchive.Validate(state);
        SimTeleStompDefaultsArchive.Validate(state);
        SimFloorClipDefaultsArchive.Validate(state);
        SimBlastedDefaultsArchive.Validate(state);
        SimNoTeleportOverrideArchive.Validate(state);
        SimTouchyDefaultsArchive.Validate(state);
        SimNoClipDefaultsArchive.Validate(state);
        SimCombatDefaultsArchive.Validate(state);
        SimInFloatDefaultsArchive.Validate(state);
        SimCorpseDefaultsArchive.Validate(state);
        SimSpawnCeilingOverrideArchive.Validate(state);
        SimFlyDefaultsArchive.Validate(state);
        SimSpawnFloatArchive.Validate(state);
        SimFloatSpawnRandomArchive.Validate(state);
        SimPainChanceOverrideArchive.Validate(state);
        SimProjectileSecondaryArchive.Validate(state);
        SimFloatBobArchive.Validate(state);
        SimFallDamageArchive.Validate(state);
        SimSmashableArchive.Validate(state);
        SimCrashedArchive.Validate(state);
        SimDontGibArchive.Validate(state);
        SimCrashStateArchive.Validate(state);
        SimExtremeCrashStateArchive.Validate(state);
        SimCrushStateArchive.Validate(state);
        SimTeleStompArchive.Validate(state);
        SimTeleportFreezeArchive.Validate(state);
        SimMonsterClassificationArchive.Validate(state);
        SimNoBloodArchive.Validate(state);
        SimPuffGetsOwnerArchive.Validate(state);
        SimWeaponRefiringArchive.Validate(state);
        SimSectorTerrainArchive.Validate(state);
        if (state.SectorTerrains is { } terrainState && terrainState.Count != Level.Sectors.Count)
            throw new InvalidOperationException("Saved sector terrain count does not match the level.");
        SimIsPuffArchive.Validate(state);
        SimSpawnDeathArchive.Validate(state);
        SimSpawnSoundSourceArchive.Validate(state);
        SimMoveWithSectorArchive.Validate(state);
        SimFloorCarryArchive.Validate(state);
        SimCanPassArchive.Validate(state);
        SimDontOverlapArchive.Validate(state);
        SimExplodeCountArchive.Validate(state);
        SimSkyExplodeArchive.Validate(state);
        SimBotIdentityArchive.Validate(state);
        SimBotAllocatorArchive.Validate(state);
        SimBotProfileArchive.Validate(state);
        SimActorDamageHistoryArchive.Validate(state);
        SimInvasionArchive.Validate(state);
        SimDroppedPickupArchive.Validate(state);
        SimMapPickupArchive.Validate(state);
        SimMapDecorationArchive.Validate(state);
        SimMapMonsterArchive.Validate(state);
        SimPuffArchive.Validate(state);
        SimThinkerOrderArchive.Validate(state);
        if (!CanRestoreThinkerMembership(state))
            throw new InvalidOperationException("Saved thinker membership cannot be restored.");
        SimBasicAttackArchive.Validate(state);
        SimProfileChaseArchive.Validate(state);
        if (HasSavedActorAllocatorConflict(state))
            throw new InvalidOperationException("A surviving actor conflicts with the saved actor ID allocator.");
        SimMissileHitPointerFlagsArchive.Validate(state);
        SimActorRenderArchive.Validate(state);
        SimDontThrustArchive.Validate(state);
        SimDontSeekInvisibleArchive.Validate(state);
        SimSeekVisibilityArchive.Validate(state);
        if (state.Actors.Any(pose => pose.PlayerDamageCount is < 0 or > 100))
            throw new InvalidOperationException("Invalid player damage count in snapshot.");
        if (state.Actors.Any(pose => pose.PlayerPoisonCount is < 0 or > 100))
            throw new InvalidOperationException("Invalid player poison count in snapshot.");
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
        // All saved missiles are reconstructed before their ownership links are restored.
        bool SavedOwnerAvailable(uint id) => id == 0 || state.Actors.Any(pose => pose.Id == id)
            && (_actors.Any(candidate => candidate.Id == id && !candidate.Destroyed)
                || state.Actors.Any(pose => pose.Id == id && (pose.BotIdentity.HasValue || pose.DroppedPickupIdentity.HasValue || pose.MapPickupIdentity.HasValue || pose.MapDecorationIdentity.HasValue || pose.PuffIdentity.HasValue))
                || state.Actors.Any(pose => pose.Id == id && pose.ProjectileLifetime.HasValue)
                    && !_actors.Any(candidate => candidate.Id == id && candidate is not ProjectileActor));
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor is PlayerPawn && pose.SpawnDeathFlag.HasValue)
                throw new InvalidOperationException("Pending map-spawn death is not supported for players.");
            if (pose.PuffIdentity.HasValue && actor is not null && actor is not PuffActor)
                throw new InvalidOperationException("Saved puff does not match the current actor.");
            if (pose.PuffIdentity is { DamageSourceId: not 0 } puffIdentity && !SavedOwnerAvailable(puffIdentity.DamageSourceId))
                throw new InvalidOperationException("Saved puff damage source cannot be restored.");
            if (pose.MapMonsterThingIndex is { } monsterOrigin
                && (!_mapMonsterTypes.TryGetValue(monsterOrigin, out var monsterType) || monsterType != pose.BotIdentity!.Value.DoomEdNum
                    || actor is not null && (actor.MapMonsterThingIndex != monsterOrigin || actor.DoomEdNum != monsterType)))
                throw new InvalidOperationException("Saved monster does not match the current map.");
            if (pose.MapDecorationIdentity is { } decoration)
            {
                if (!_mapDecorationTypes.TryGetValue(decoration.ThingIndex, out var decorationType)
                    || decorationType != decoration.DoomEdNum
                    || actor is not null && (actor.MapDecorationThingIndex != decoration.ThingIndex || actor.DoomEdNum != decorationType))
                    throw new InvalidOperationException("Saved decoration does not match the current map.");
                var prototype = ActorSpawner.CreateMapActor(Level, Level.Things[decoration.ThingIndex], pose.Id, _dehacked, _spawnOptions, () => 0);
                var decorationClass = decoration.DefinitionDoomEdNum != 0 ? decoration.DefinitionDoomEdNum : decoration.DoomEdNum;
                if (prototype.ClassDoomEdNum != decorationClass || actor is not null && actor.ClassDoomEdNum != decorationClass)
                    throw new InvalidOperationException("Saved decoration class does not match the current patch.");
                if (!prototype.States.HasState(pose.State))
                    throw new InvalidOperationException("Saved decoration state is not in the current table.");
            }
            if (pose.MapPickupIdentity is { } mapPickup
                && (!_mapPickupTypes.TryGetValue(mapPickup.ThingIndex, out var type) || type != mapPickup.DoomEdNum
                    || actor is not null && (actor.MapPickupThingIndex != mapPickup.ThingIndex || actor.DoomEdNum != type)
                    || !ActorSpawner.CreateMapActor(Level, Level.Things[mapPickup.ThingIndex], pose.Id,
                        _dehacked, _spawnOptions, () => 0).States.HasState(pose.State)))
                throw new InvalidOperationException("Saved map pickup does not match the current map.");
            if (pose.DroppedPickupIdentity is { } dropIdentity
                && (!PickupCatalog.IsPickup(dropIdentity.DoomEdNum, RavenHealthPickups)
                    || actor is not null && (!actor.SpawnedDroppedPickup || actor.DoomEdNum != dropIdentity.DoomEdNum)
                    || !CreateDroppedPickupActor(pose.Id, dropIdentity.DoomEdNum, default, default).States.HasState(pose.State)))
                throw new InvalidOperationException("Saved dropped pickup does not match a supported pickup.");
            var reconstructProjectile = pose.ProjectileLifetime.HasValue && (actor == null || actor.Destroyed);
            var restoreBotBrain = pose.BotIdentity is { BrainPresent: true };
            var savedType = pose.BotIdentity?.DoomEdNum ?? actor?.DoomEdNum ?? 0;
            var savedDefaults = _dehacked?.Actors.FirstOrDefault(candidate => candidate.DoomEdNum == savedType && candidate.Patched);
            var savedDefinitionType = savedDefaults?.OriginalDoomEdNum is > 0 ? savedDefaults.OriginalDoomEdNum : savedType;
            var savedBrainType = pose.BotIdentity?.ProfileType ?? actor?.ClassDoomEdNum ?? savedDefinitionType;
            if (pose.BotIdentity is { } identity && (actor is not null
                    && (pose.MapMonsterThingIndex.HasValue ? actor.MapMonsterThingIndex != pose.MapMonsterThingIndex
                        : identity.SpawnedLostSoul ? !actor.SpawnedLostSoul : actor is not BotPawn)
                || actor is not null && actor.DoomEdNum != identity.DoomEdNum
                || !new BotPawn().States.HasState(pose.State)))
                throw new InvalidOperationException("Saved bot identity or state does not match a supported bot.");
            if (pose.BotIdentity is { SpawnedLostSoul: true } soulIdentity)
            {
                var soulDefaults = _dehacked?.Actors.FirstOrDefault(candidate => candidate.OriginalDoomEdNum == 3006 && candidate.Patched);
                if (soulIdentity.DoomEdNum != (soulDefaults?.DoomEdNum ?? 3006))
                    throw new InvalidOperationException("Saved spawned lost soul does not match its class defaults.");
            }
            if (pose.ProfileChaseHistory.HasValue && pose.BotIdentity.HasValue)
            {
                if (DoomMonsterAttacks.Find(savedBrainType) is null)
                    throw new InvalidOperationException("Saved chase history requires a supported class brain.");
                if (pose.ProfileChaseHistory?.Runtime is { HealTics: > 0 } && savedBrainType != 64)
                    throw new InvalidOperationException("Saved healing wait requires an archvile class brain.");
            }
            var savedAttack = pose.RepeatAttack ?? pose.ProfileAttack ?? pose.RevenantAttack
                ?? pose.ArchvileAttack ?? pose.LostSoulAttack;
            if (pose.BotIdentity is { BrainPresent: true, BasicAttack: null } && savedAttack is { } attack
                && attack.NativeType != savedBrainType)
                throw new InvalidOperationException("Saved attack sequence does not match the bot brain profile.");
            if (pose.BotIdentity is { BrainPresent: true, BasicAttack: null } && pose.PainDeath.HasValue && savedBrainType != 71)
                throw new InvalidOperationException("Saved death action does not match the bot brain profile.");
            if (reconstructProjectile && (pose.ProjectilePointers is not { } savedPointers
                || !SavedOwnerAvailable(savedPointers.OwnerId)
                || pose.Id == 0 || pose.Id == uint.MaxValue || !pose.HasPhysics
                || state.Actors.Count(item => item.Id == pose.Id) != 1
                || actor != null && actor is not ProjectileActor))
                throw new InvalidOperationException("Saved projectile cannot be reconstructed with its owner.");
            if (reconstructProjectile)
            {
                var owner = pose.ProjectilePointers!.Value.OwnerId == 0 ? null
                    : _actors.FirstOrDefault(candidate => candidate.Id == pose.ProjectilePointers!.Value.OwnerId && !candidate.Destroyed);
                var prototype = new ProjectileActor(owner, pose.ProjectileLifetime!.Value.Kind);
                if (pose.WeaponCooldown < 0 || !prototype.States.HasState(pose.State))
                    throw new InvalidOperationException("Saved projectile state is not in the current table.");
            }
            if (pose.IceChunkLifecycle is { } lifecycle && (lifecycle is not (1 or 3)
                || pose.Id == 0 || pose.Id == uint.MaxValue || !new IceChunkActor(10).States.HasState(pose.State) || !pose.HasPhysics
                || actor is not null && actor is not IceChunkActor || state.Actors.Count(item => item.Id == pose.Id) != 1))
                throw new InvalidOperationException("Saved ice chunk does not match a valid chunk identity or state.");
            if (pose.ProjectilePointers is { } pointers &&
                !reconstructProjectile && (actor is not ProjectileActor pointerMissile || pointerMissile.Destroyed || !SavedOwnerAvailable(pointers.OwnerId)))
                throw new InvalidOperationException("Saved projectile pointers do not match the current actor.");
            if (pose.ProjectileLifetime is { } lifetime &&
                !reconstructProjectile && (actor is not ProjectileActor missile || missile.Destroyed || missile.Kind != lifetime.Kind))
                throw new InvalidOperationException("Saved projectile lifetime does not match the current actor.");
            if (pose.PainDeath.HasValue && !(restoreBotBrain && savedBrainType == 71)
                && (actor == null || actor.Destroyed || actor.Brain?.CapturePainDeath() == null))
                throw new InvalidOperationException("Saved Pain Elemental death state does not match the current actor.");
            if (pose.SkullChargeFlag == 1 && !restoreBotBrain && (actor == null || actor.Destroyed || actor.Brain == null))
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
        _bfgSelfDamageRandomState = state.BfgSelfDamageRandomState ?? _initialBfgSelfDamageRandomState;
        _hasBfgSelfDamageRandomState = state.BfgSelfDamageRandomState.HasValue;
        _bfgSprayRandomState = state.BfgSprayRandomState ?? _initialBfgSprayRandomState;
        _hasBfgSprayRandomState = state.BfgSprayRandomState.HasValue;
        _fireSg2RandomState = state.FireSg2RandomState ?? _initialFireSg2RandomState;
        _hasFireSg2RandomState = state.FireSg2RandomState.HasValue;
        _gunShotRandomState = state.GunShotRandomState ?? _initialGunShotRandomState;
        _hasGunShotRandomState = state.GunShotRandomState.HasValue;
        _freezeDeathRandomState = state.FreezeDeathRandomState ?? _initialFreezeDeathRandomState;
        _hasFreezeDeathRandomState = state.FreezeDeathRandomState.HasValue;
        _playerSearchRandomState = state.PlayerSearchRandomState ?? _initialPlayerSearchRandomState;
        _hasPlayerSearchRandomState = state.PlayerSearchRandomState.HasValue;
        _playerLookRandomState = state.PlayerLookRandomState ?? _initialPlayerLookRandomState;
        _hasPlayerLookRandomState = state.PlayerLookRandomState.HasValue;
        _floatSpawnRandomState = state.FloatSpawnRandomState ?? _initialFloatSpawnRandomState;
        _hasFloatSpawnRandomState = state.FloatSpawnRandomState.HasValue;
        CompatibilityInvisibility = state.CompatibilityInvisibility;
        VerticalWeaponSpread = state.VerticalWeaponSpread;
        _hateSearchRandomState = state.HateSearchRandomState ?? _initialHateSearchRandomState;
        _hasHateSearchRandomState = state.HateSearchRandomState.HasValue;
        _actorDieRandomState = state.ActorDieRandomState ?? _initialActorDieRandomState;
        _hasActorDieRandomState = state.ActorDieRandomState.HasValue;
        _puffRandomState = state.PuffRandomState ?? _initialPuffRandomState;
        _hasPuffRandomState = state.PuffRandomState.HasValue;
        _dropItemRandomState = state.DropItemRandomState ?? _initialDropItemRandomState;
        _hasDropItemRandomState = state.DropItemRandomState.HasValue;
        _chaseRandomState = state.ChaseRandomState ?? _initialChaseRandomState;
        _hasChaseRandomState = state.ChaseRandomState.HasValue;
        _missileDamageRandomState = state.MissileDamageRandomState ?? _initialMissileDamageRandomState;
        _hasMissileDamageRandomState = state.MissileDamageRandomState.HasValue;
        _poisonDamageRandomState = state.PoisonDamageRandomState ?? _initialPoisonDamageRandomState;
        _hasPoisonDamageRandomState = state.PoisonDamageRandomState.HasValue;
        _reflectRandomState = state.ReflectRandomState ?? _initialReflectRandomState;
        _hasReflectRandomState = state.ReflectRandomState.HasValue;
        _seekerMissileRandomState = state.SeekerMissileRandomState ?? _initialSeekerMissileRandomState;
        _hasSeekerMissileRandomState = state.SeekerMissileRandomState.HasValue;
        _painChanceRandomState = state.PainChanceRandomState ?? _initialPainChanceRandomState;
        _hasPainChanceRandomState = state.PainChanceRandomState.HasValue;
        _skelFistRandomState = state.SkelFistRandomState ?? _initialSkelFistRandomState;
        _hasSkelFistRandomState = state.SkelFistRandomState.HasValue;
        _sawRandomState = state.SawRandomState ?? _initialSawRandomState;
        _hasSawRandomState = state.SawRandomState.HasValue;
        _punchRandomState = state.PunchRandomState ?? _initialPunchRandomState;
        _hasPunchRandomState = state.PunchRandomState.HasValue;
        _newChaseDirRandomState = state.NewChaseDirRandomState;
        _tryWalkRandomState = state.TryWalkRandomState;
        _avoidCrushRandomState = state.AvoidCrushRandomState;
        _stayOnLiftRandomState = state.StayOnLiftRandomState;
        _scaredyCatRandomState = state.ScaredyCatRandomState;
        _checkMissileRandomState = state.CheckMissileRandomState;
        _defectRandomState = state.DefectRandomState;
        _checkSightRandomState = state.CheckSightRandomState;
        _uniqueTidRandomState = state.UniqueTidRandomState;
        _sectorDamageRandomState = state.SectorDamageRandomState;
        _nativeSwitchTargetRandomState = state.NativeSwitchTargetRandomState;
        SwitchTargetRandomState = state.SwitchTargetRandomState ?? (unchecked((uint)RngSeed) ^ 0x73776974u);
        _enemyStrafeRandomState = state.EnemyStrafeRandomState;
        _skipTargetRandomState = state.SkipTargetRandomState ?? _initialSkipTargetRandomState;
        _hasSkipTargetRandomState = state.SkipTargetRandomState.HasValue;
        _cPosRefireRandomState = state.CPosRefireRandomState ?? _initialCPosRefireRandomState;
        _hasCPosRefireRandomState = state.CPosRefireRandomState.HasValue;
        _cPosAttackRandomState = state.CPosAttackRandomState ?? _initialCPosAttackRandomState;
        _hasCPosAttackRandomState = state.CPosAttackRandomState.HasValue;
        _sPosAttackRandomState = state.SPosAttackRandomState ?? _initialSPosAttackRandomState;
        _hasSPosAttackRandomState = state.SPosAttackRandomState.HasValue;
        _posAttackRandomState = state.PosAttackRandomState ?? _initialPosAttackRandomState;
        _hasPosAttackRandomState = state.PosAttackRandomState.HasValue;
        _bruisAttackRandomState = state.BruisAttackRandomState ?? _initialBruisAttackRandomState;
        _hasBruisAttackRandomState = state.BruisAttackRandomState.HasValue;
        _headAttackRandomState = state.HeadAttackRandomState ?? _initialHeadAttackRandomState;
        _hasHeadAttackRandomState = state.HeadAttackRandomState.HasValue;
        _troopAttackRandomState = state.TroopAttackRandomState ?? _initialTroopAttackRandomState;
        _hasTroopAttackRandomState = state.TroopAttackRandomState.HasValue;
        _sargAttackRandomState = state.SargAttackRandomState ?? _initialSargAttackRandomState;
        _hasSargAttackRandomState = state.SargAttackRandomState.HasValue;
        ForcedFallingDamage = state.ForcedFallingDamage;
        _airControlOverride = state.AirControlOverride;
        GibFactor = state.GibFactor ?? 1;
        _levelGravityOverride = state.LevelGravityOverride;
        Thinkers.Clock.Restore(state.Tic);
        for (var i = 0; i < Level.Lines.Count; i++)
            (state.LineActions?[i] ?? _initialLineActions[i]).Apply(Level.Lines[i]);
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
        if (state.GeometryHealth != null)
        {
            var savedActorIds = state.Actors.Select(pose => pose.Id).ToHashSet();
            var removedProjectileIds = new HashSet<uint>();
            foreach (var projectile in _actors.OfType<ProjectileActor>().Where(actor => !savedActorIds.Contains(actor.Id)).ToArray())
            {
                removedProjectileIds.Add(projectile.Id);
                projectile.Destroy(); Thinkers.Remove(projectile); _actors.Remove(projectile);
            }
            // Reuse only discarded projectile IDs at the allocation tail; other actor identities stay reserved.
            while (removedProjectileIds.Contains(_nextActorId - 1)) _nextActorId--;
        }
        if (state.IncludesIceChunkLifecycle)
        {
            var ids = state.Actors.Where(pose => pose.IceChunkLifecycle.HasValue).Select(pose => pose.Id).ToHashSet();
            foreach (var chunk in _actors.OfType<IceChunkActor>().Where(chunk => !ids.Contains(chunk.Id)).ToArray())
            { chunk.Destroy(); Thinkers.Remove(chunk); _actors.Remove(chunk); }
        }
        if (state.IncludesBotIdentities)
        {
            var savedBots = state.Actors.Where(pose => pose.BotIdentity.HasValue).Select(pose => pose.Id).ToHashSet();
            foreach (var bot in _actors.OfType<BotPawn>().Where(bot => !savedBots.Contains(bot.Id)).ToArray())
            { bot.Destroy(); Thinkers.Remove(bot); _actors.Remove(bot); }
            var savedActorIds = state.Actors.Select(pose => pose.Id).ToHashSet();
            foreach (var puff in _actors.OfType<PuffActor>().Where(actor => !savedActorIds.Contains(actor.Id)).ToArray())
            { puff.Destroy(); Thinkers.Remove(puff); _actors.Remove(puff); }
            foreach (var monster in _actors.Where(actor => actor.MapMonsterThingIndex.HasValue && !savedActorIds.Contains(actor.Id)).ToArray())
            { monster.Destroy(); Thinkers.Remove(monster); _actors.Remove(monster); }
            foreach (var decoration in _actors.Where(actor => actor.MapDecorationThingIndex.HasValue && !savedActorIds.Contains(actor.Id)).ToArray())
            { decoration.Destroy(); Thinkers.Remove(decoration); _actors.Remove(decoration); }
            foreach (var soul in _actors.Where(actor => actor.SpawnedLostSoul && !savedActorIds.Contains(actor.Id)).ToArray())
            { soul.Destroy(); Thinkers.Remove(soul); _actors.Remove(soul); }
            foreach (var drop in _actors.Where(actor => actor.SpawnedDroppedPickup && !savedActorIds.Contains(actor.Id)).ToArray())
            { drop.Destroy(); Thinkers.Remove(drop); _actors.Remove(drop); }
            foreach (var pickup in _actors.Where(actor => actor.MapPickupThingIndex.HasValue && !savedActorIds.Contains(actor.Id)).ToArray())
            { pickup.Destroy(); Thinkers.Remove(pickup); _actors.Remove(pickup); }
            foreach (var pose in state.Actors.Where(pose => pose.BotIdentity.HasValue))
            {
                var existing = _actors.FirstOrDefault(actor => actor.Id == pose.Id);
                if (existing is { Destroyed: false }) continue;
                if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
                var identity = pose.BotIdentity!.Value;
                if (pose.MapMonsterThingIndex is { } monsterOrigin)
                {
                    var monster = ActorSpawner.CreateMapActor(Level, Level.Things[monsterOrigin], pose.Id, _dehacked, _spawnOptions, () => 0);
                    monster.Simulation = this; monster.MapMonsterThingIndex = monsterOrigin;
                    monster.SpawnReactionTime ??= monster.ReactionTime; monster.SpawnDamage ??= monster.Damage;
                    monster.SpawnTuning ??= new(monster.MovementSpeed.Raw, monster.FloatSpeed, monster.PainThreshold);
                    monster.JustSpawned = identity.JustSpawned;
                    _actors.Add(monster); Thinkers.Add(monster, identity.StatNum);
                    _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
                }
                else if (identity.SpawnedLostSoul)
                {
                    var soul = CreateLostSoulActor(pose.Id, new Fixed(pose.X), new Fixed(pose.Y), new Fixed(pose.Z),
                        ActorPhysics.SectorAt(Level, pose.X / 65536.0, pose.Y / 65536.0), 0);
                    soul.SpawnedLostSoul = true; soul.JustSpawned = identity.JustSpawned;
                    soul.FailedLostSoulSpawn = identity.FailedLostSoulSpawn;
                    if (!identity.FailedLostSoulSpawn)
                    { soul.SpawnReactionTime ??= soul.ReactionTime; soul.SpawnDamage ??= soul.Damage; }
                    _actors.Add(soul); Thinkers.Add(soul);
                    _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
                }
                else AddBotCore(pose.X / 65536.0, pose.Y / 65536.0, identity.DoomEdNum, identity.ThingId, pose.Id, identity.JustSpawned);
            }
        }
        foreach (var pose in state.Actors.Where(pose => pose.DroppedPickupIdentity.HasValue))
        {
            var existing = _actors.FirstOrDefault(actor => actor.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var identity = pose.DroppedPickupIdentity!.Value;
            var drop = CreateDroppedPickupActor(pose.Id, identity.DoomEdNum, new Fixed(pose.X), new Fixed(pose.Y));
            drop.JustSpawned = identity.JustSpawned;
            _actors.Add(drop); Thinkers.Add(drop, identity.StatNum);
            _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
        }
        foreach (var pose in state.Actors.Where(pose => pose.MapPickupIdentity.HasValue))
        {
            var existing = _actors.FirstOrDefault(actor => actor.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var identity = pose.MapPickupIdentity!.Value;
            var pickup = ActorSpawner.CreateMapActor(Level, Level.Things[identity.ThingIndex], pose.Id,
                _dehacked, _spawnOptions, () => 0);
            pickup.Simulation = this; pickup.MapPickupThingIndex = identity.ThingIndex;
            pickup.SpawnReactionTime ??= pickup.ReactionTime; pickup.SpawnDamage ??= pickup.Damage;
            pickup.SpawnTuning ??= new(pickup.MovementSpeed.Raw, pickup.FloatSpeed, pickup.PainThreshold);
            pickup.JustSpawned = identity.JustSpawned;
            _actors.Add(pickup); Thinkers.Add(pickup, identity.StatNum);
            _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
        }
        foreach (var pose in state.Actors.Where(pose => pose.MapDecorationIdentity.HasValue))
        {
            var existing = _actors.FirstOrDefault(actor => actor.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var identity = pose.MapDecorationIdentity!.Value;
            var pickup = ActorSpawner.CreateMapActor(Level, Level.Things[identity.ThingIndex], pose.Id,
                _dehacked, _spawnOptions, () => 0);
            pickup.Simulation = this; pickup.MapDecorationThingIndex = identity.ThingIndex;
            pickup.SpawnReactionTime ??= pickup.ReactionTime; pickup.SpawnDamage ??= pickup.Damage;
            pickup.SpawnTuning ??= new(pickup.MovementSpeed.Raw, pickup.FloatSpeed, pickup.PainThreshold);
            pickup.JustSpawned = identity.JustSpawned;
            _actors.Add(pickup); Thinkers.Add(pickup, identity.StatNum);
            _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
        }
        foreach (var pose in state.Actors.Where(pose => pose.PuffIdentity.HasValue))
        {
            var existing = _actors.FirstOrDefault(actor => actor.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing is not null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var identity = pose.PuffIdentity!.Value;
            var puff = new PuffActor { Id = pose.Id, Level = Level, Simulation = this, IsPuff = true, DefaultIsPuff = true, JustSpawned = identity.JustSpawned };
            _actors.Add(puff); Thinkers.Add(puff, identity.StatNum);
            _nextActorId = Math.Max(_nextActorId, checked(pose.Id + 1));
        }
        foreach (var pose in state.Actors.Where(pose => pose.ProjectileLifetime.HasValue))
        {
            var existing = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (existing is { Destroyed: false }) continue;
            if (existing != null) { Thinkers.Remove(existing); _actors.Remove(existing); }
            var owner = pose.ProjectilePointers!.Value.OwnerId == 0 ? null
                    : _actors.FirstOrDefault(candidate => candidate.Id == pose.ProjectilePointers!.Value.OwnerId && !candidate.Destroyed);
            SpawnProjectileCore(owner, pose.ProjectileLifetime!.Value.Kind, null, pose.Id);
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
        if (_actors.Count != 0)
        {
            var nextId = checked(_actors.Max(actor => actor.Id) + 1);
            _nextActorId = Math.Max(_nextActorId, nextId);
        }
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor == null)
                continue;
            if (pose.BotIdentity is { } savedBot)
            {
                actor.ThingId = savedBot.ThingId;
                actor.Dormant = savedBot.Dormant;
                var placementChanged = actor.JustSpawned != savedBot.JustSpawned || actor.StatNum != savedBot.StatNum;
                actor.JustSpawned = savedBot.JustSpawned;
                if (placementChanged) Thinkers.ChangeStatNum(actor, savedBot.StatNum);
                if (!savedBot.BrainPresent) actor.Brain = null;
                else if (savedBot.BasicAttack is { } basicAttack)
                {
                    if (actor.Brain?.BasicAttack != basicAttack) actor.Brain = new MonsterBrain(basicAttack);
                }
                else if (actor.Brain is null || actor.Brain.ProfileType != (savedBot.ProfileType ?? actor.ClassDoomEdNum))
                    actor.Brain = MonsterBrain.ForType(savedBot.ProfileType ?? actor.ClassDoomEdNum) ?? new MonsterBrain(MonsterAttack.Hitscan);
            }
            actor.X = new Fixed(pose.X);
            if (pose.PuffIdentity is { } savedPuff && actor is PuffActor puff)
            {
                actor.ThingId = savedPuff.ThingId;
                var changed = actor.JustSpawned != savedPuff.JustSpawned || actor.StatNum != savedPuff.StatNum;
                actor.JustSpawned = savedPuff.JustSpawned;
                if (changed) Thinkers.ChangeStatNum(actor, savedPuff.StatNum);
                puff.DamageSource = _actors.FirstOrDefault(candidate => candidate.Id == savedPuff.DamageSourceId && !candidate.Destroyed);
                actor.Brain = null;
            }
            if (pose.MapDecorationIdentity is { } savedDecoration)
            {
                actor.ThingId = savedDecoration.ThingId;
                var changed = actor.JustSpawned != savedDecoration.JustSpawned || actor.StatNum != savedDecoration.StatNum;
                actor.JustSpawned = savedDecoration.JustSpawned;
                if (changed) Thinkers.ChangeStatNum(actor, savedDecoration.StatNum);
                actor.Brain = null;
            }
            if (pose.MapPickupIdentity is { } savedMapPickup)
            {
                actor.ThingId = savedMapPickup.ThingId;
                var changed = actor.JustSpawned != savedMapPickup.JustSpawned || actor.StatNum != savedMapPickup.StatNum;
                actor.JustSpawned = savedMapPickup.JustSpawned;
                if (changed) Thinkers.ChangeStatNum(actor, savedMapPickup.StatNum);
                actor.Brain = null;
            }
            if (pose.DroppedPickupIdentity is { } savedDrop)
            {
                actor.ThingId = savedDrop.ThingId;
                var placementChanged = actor.JustSpawned != savedDrop.JustSpawned || actor.StatNum != savedDrop.StatNum;
                actor.JustSpawned = savedDrop.JustSpawned;
                if (placementChanged) Thinkers.ChangeStatNum(actor, savedDrop.StatNum);
                actor.Brain = null;
            }
            if (pose.IceChunkLifecycle is { } chunkLifecycle) actor.JustSpawned = (chunkLifecycle & 2) != 0;
            var actorSpecial = pose.ActorSpecial ?? default;
            actor.Special = actorSpecial.Special;
            actor.ActivationType = actorSpecial.ActivationType;
            actor.SpecialArgs[0] = actorSpecial.Arg0; actor.SpecialArgs[1] = actorSpecial.Arg1;
            actor.SpecialArgs[2] = actorSpecial.Arg2; actor.SpecialArgs[3] = actorSpecial.Arg3;
            actor.SpecialArgs[4] = actorSpecial.Arg4;
            actor.SpecialChanged = pose.ActorSpecial.HasValue;
            actor.Y = new Fixed(pose.Y);
            if (actor is IceChunkActor || pose.MapMonsterThingIndex.HasValue || pose.MapDecorationIdentity.HasValue || pose.MapPickupIdentity.HasValue || pose.DroppedPickupIdentity.HasValue)
                actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            actor.Angle = new BamAngle(pose.Angle);
            actor.PitchDegrees = new Fixed(pose.Pitch).ToDouble();
            if (pose.PainDeath is { } painDeath && actor.Brain?.CapturePainDeath() != null) actor.Brain.RestorePainDeath(painDeath);
            actor.Brain?.RestoreRepeatAttack(pose.RepeatAttack);
            actor.Brain?.RestoreProfileAttack(pose.ProfileAttack);
            actor.Brain?.RestoreRevenantAttack(pose.RevenantAttack);
            actor.Brain?.RestoreArchvileAttack(pose.ArchvileAttack);
            actor.Brain?.RestoreLostSoulAttack(pose.LostSoulAttack);
            if (actor.Brain != null) actor.Brain.Enabled = !pose.BrainDisabled;
            actor.RestoreHealth(pose.Health);
            actor.DeathType = pose.DeathType;
            actor.PainType = pose.PainType;
            actor.SpecialFireDamage = pose.SpecialFireDamage;
            actor.FoilInvul = pose.FoilInvul;
            actor.PierceArmor = pose.PierceArmor;
            actor.NoInfightSpecies = pose.NoInfightSpecies;
            actor.DamageFactor = new Fixed(pose.DamageFactor);
            actor.SelfDamageFactor = new Fixed(pose.SelfDamageFactor);
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
            actor.NoSector = pose.SectorLinkFlag is { } sectorLink ? sectorLink != 0
                : (actor.ResurrectionCollisionFlags.GetValueOrDefault() & 128) != 0;
            actor.HasSectorOverride = pose.SectorLinkFlag.HasValue;
            actor.GoalId = pose.GoalPointer?.ActorId;
            actor.Killed = pose.Killed;
            actor.HasTargetMemoryOverride = pose.TargetMemory.HasValue;
            var memory = pose.TargetMemory ?? default;
            actor.Brain?.RestoreTargetMemory(memory);
            if (actor is not ProjectileActor) actor.StoredTargetId = actor.Brain is null ? memory.Target : null;
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
            if (actor is not ProjectileActor)
            {
                actor.TracerId = pose.TracerPointer?.ActorId;
                actor.HasTracerOverride = pose.TracerPointer.HasValue;
            }
            actor.MasterId = pose.MasterPointer?.ActorId;
            actor.HasMasterOverride = pose.MasterPointer.HasValue;
            actor.FloorClip = pose.FloorClip ?? 0;
            actor.HasFloorClipOverride = pose.FloorClip.HasValue;
            actor.Invisible = pose.InvisibleFlag == 1;
            actor.Ambush = pose.AmbushFlag is { } ambush ? ambush != 0 : actor.DefaultAmbush;
            actor.CantSeek = pose.CantSeekFlag == 1;
            actor.PendingSpawnDeath = pose.SpawnDeathFlag == 1;
            actor.IsPuff = pose.IsPuffFlag is { } isPuff ? isPuff != 0 : actor.DefaultIsPuff;
            actor.GibHealth = pose.GibHealthOverride ?? actor.DefaultGibHealth;
            actor.LastLookActorId = pose.LastLookActorId;
            actor.NoSightCheck = (pose.HateTargetingFlags.GetValueOrDefault() & 1) != 0;
            actor.HuntPlayers = (pose.HateTargetingFlags.GetValueOrDefault() & 2) != 0;
            actor.Vulnerable = pose.VulnerableFlag == 1;
            actor.IsMonster = pose.MonsterClassificationFlag is { } monsterClassification
                ? monsterClassification == 1 : (actor.ResurrectionCollisionFlags.GetValueOrDefault() & 32) != 0;
            actor.NoTarget = pose.NoTargetFlag == 1;
            actor.Buddha = pose.BuddhaFlag != 0;
            actor.FlyCheat = pose.FlyCheatFlag != 0;
            actor.NoClip = pose.NoClipFlag != 0;
            if (actor is PlayerPawn noClipPlayer) noClipPlayer.NoClip2 = pose.PlayerNoClip2Flag != 0;
            if (actor is PlayerPawn frozenPlayer)
            {
                frozenPlayer.Frozen = (pose.PlayerFrozenFlags & 1) != 0;
                frozenPlayer.TotallyFrozen = (pose.PlayerFrozenFlags & 2) != 0;
            }
            if (actor is PlayerPawn noTargetPlayer)
                noTargetPlayer.NoTargetCheatOverride = pose.PlayerNoTargetCheatOverride is { } cheat ? cheat == 1 : null;
            actor.ChaseGoal = pose.ChaseGoalFlag == 1;
            actor.JustAttacked = pose.JustAttackedFlag == 1;
            actor.InCombat = pose.InCombatFlag == 1;
            actor.NoSplashAlert = pose.NoSplashAlertFlag == 1;
            actor.DontSplash = pose.DontSplashFlag == 1;
            actor.DontHarmSpecies = pose.DontHarmSpeciesFlag == 1;
            actor.DontHarmClass = pose.DontHarmClassFlag == 1;
            actor.LandingSpeed = pose.LandingSpeed;
            actor.LastPushFromMovement = pose.PushHistoryFlag == 1;
            actor.BlockedBySolidActors = pose.BlockedBySolidActorsFlag == 1;
            actor.BumpSpecial = pose.BumpSpecialFlag == 1;
            actor.Reflective = (pose.ReflectionFlags.GetValueOrDefault() & 1) != 0;
            actor.ThruReflect = (pose.ReflectionFlags.GetValueOrDefault() & 2) != 0;
            actor.DontReflect = (pose.ReflectionFlags.GetValueOrDefault() & 4) != 0;
            actor.MirrorReflect = (pose.ReflectionFlags.GetValueOrDefault() & 8) != 0;
            actor.Deflect = (pose.ReflectionFlags.GetValueOrDefault() & 16) != 0;
            actor.ShieldReflect = (pose.ReflectionFlags.GetValueOrDefault() & 32) != 0;
            actor.NoShieldReflect = (pose.ReflectionFlags.GetValueOrDefault() & 64) != 0;
            actor.AimReflect = (pose.ReflectionFlags.GetValueOrDefault() & 128) != 0;
            actor.SeekerMissile = pose.SeekerMissileFlag is { } seekerFlag ? seekerFlag == 1
                : actor is ProjectileActor { Kind: ProjectileKind.RevenantTracer };
            actor.LastBumpTic = pose.BumpCooldownTic;
            actor.SummonedMonster = pose.SummonedMonsterFlag == 1;
            actor.ScreenSeeker = pose.ScreenSeekerFlag == 1;
            actor.VisibilityPulse = pose.VisibilityPulseFlag == 1;
            actor.VisibilityDirection = pose.VisibilityDirection;
            actor.Stealth = pose.StealthFlag == 1;
            actor.DefaultStealth = pose.StealthDefaultsFlag == 1;
            actor.DefaultFloatBob = pose.FloatBobDefaultsFlag == 1;
            actor.DefaultReflective = pose.ReflectiveDefaultsFlag == 1;
            actor.DefaultThruGhost = pose.ThruGhostDefaultsFlag == 1;
            actor.DefaultNonShootable = pose.NonShootableDefaultsFlag == 1;
            actor.DefaultFloorPic = pose.FloorPicDefaultsFlag == 1;
            actor.DefaultRip = pose.RipDefaultsFlag == 1;
            actor.DefaultFloorClip = pose.FloorClipDefaultsFlag == 1;
            actor.DefaultBlasted = pose.BlastedDefaultsFlag == 1;
            actor.DefaultTouchy = pose.TouchyDefaultsFlag == 1;
            actor.DefaultNoClip = pose.NoClipDefaultsFlag == 1;
            actor.DefaultCombatFlags = pose.CombatDefaultsFlags ?? 0;
            actor.DefaultInFloat = pose.InFloatDefaultsFlag == 1;
            actor.DefaultCorpse = pose.CorpseDefaultsFlag == 1;
            actor.DefaultFly = pose.FlyDefaultsFlag == 1;
            actor.PainChance = pose.PainChanceOverride ?? actor.SpawnPainChance ?? 256;
            actor.SpawnFloat = (pose.SpawnFloatFlags.GetValueOrDefault() & 1) != 0;
            actor.DefaultSpawnFloat = (pose.SpawnFloatFlags.GetValueOrDefault() & 2) != 0;
            actor.SpawnCeiling = pose.SpawnCeilingOverrideFlag is { } ceilingFlag ? ceilingFlag == 1
                : (actor.ResurrectionCollisionFlags.GetValueOrDefault() & 64) != 0;
            actor.DefaultPushFlags = pose.PushDefaultsFlags ?? 0;
            actor.DefaultTeleStomp = pose.TeleStompDefaultsFlag is { } stompDefault ? stompDefault == 1 : actor is PlayerPawn;
            actor.DefaultSeeker = pose.SeekerDefaultsFlag is { } seekerDefault ? seekerDefault == 1
                : actor is ProjectileActor { Kind: ProjectileKind.RevenantTracer };
            if (actor is ProjectileActor)
            {
                var secondary = pose.ProjectileSecondaryFlags ?? 1;
                actor.NoTeleport = (secondary & 1) != 0;
                actor.Dormant = (secondary & 2) != 0;
            }
            else actor.NoTeleport = pose.NoTeleportOverrideFlag is { } noTeleportFlag ? noTeleportFlag == 1
                : actor.DefaultNoTeleport;
            actor.StealthAlpha = pose.StealthAlpha;
            actor.FloatBob = pose.FloatBobSettings?.Enabled ?? false;
            actor.FloatBobStrength = pose.FloatBobSettings?.Strength ?? 1;
            actor.FloatBobFactor = pose.FloatBobSettings?.Factor ?? 1;
            actor.FallDamage = pose.FallDamageFlag == 1;
            actor.Smashable = pose.SmashableFlag == 1;
            actor.Crashed = pose.CrashedFlag == 1;
            actor.DontGib = pose.DontGibFlag == 1;
            var teleStompFlags = pose.TeleStompFlags ?? (actor is PlayerPawn ? 1 : 0);
            actor.TeleStomp = (teleStompFlags & 1) != 0;
            actor.NoTeleStomp = (teleStompFlags & 2) != 0;
            actor.CrashState = pose.CrashStateOverride ?? -1;
            actor.ExtremeCrashState = pose.ExtremeCrashStateOverride ?? -1;
            actor.CrushState = pose.CrushStateOverride ?? -1;
            actor.Touchy = (pose.TouchyImpactFlags.GetValueOrDefault() & 1) != 0;
            actor.Armed = (pose.TouchyImpactFlags.GetValueOrDefault() & 2) != 0;
            var combatBehavior = pose.CombatBehaviorFlags ?? 0;
            actor.HarmFriends = (combatBehavior & 1) != 0;
            actor.DontDrain = (combatBehavior & 2) != 0;
            actor.NoAutoOffSkullFly = (combatBehavior & 4) != 0;
            var targeting = pose.TargetSwitchFlags ?? 0;
            actor.NeverTarget = (targeting & 1) != 0;
            actor.NoTargetSwitch = (targeting & 2) != 0;
            actor.QuickToRetaliate = (targeting & 4) != 0;
            var infighting = pose.InfightingFlags ?? 0;
            actor.NoInfighting = (infighting & 1) != 0;
            actor.ForceInfighting = (infighting & 2) != 0;
            actor.DoHarmSpecies = (infighting & 4) != 0;
            actor.Score = pose.Score;
            actor.SeeInvisible = pose.SeeInvisible;
            actor.LastLookPlayerNumber = pose.LastLookPlayerNumber;
            actor.MVisBlocked = pose.MVisBlockedFlag == 1;
            actor.NoBlood = pose.NoBloodFlag is { } noBlood ? noBlood != 0 : actor.DefaultNoBlood;
            actor.PuffGetsOwner = pose.PuffGetsOwnerFlag == 1;
            actor.SpawnSoundSource = pose.SpawnSoundSourceFlag == 1;
            actor.MoveWithSector = pose.MoveWithSectorFlag == 1;
            actor.CanPass = pose.CanPassFlag is { } canPass ? canPass != 0 : actor.DefaultCanPass;
            actor.DontOverlap = pose.DontOverlapFlag == 1;
            actor.ExplodeCount = pose.ExplodeCountFlag == 1;
            actor.SkyExplode = pose.SkyExplodeFlag == 1;
            actor.HitTarget = (pose.MissileHitPointerFlags.GetValueOrDefault() & 1) != 0;
            actor.HitMaster = (pose.MissileHitPointerFlags.GetValueOrDefault() & 2) != 0;
            actor.HitTracer = (pose.MissileHitPointerFlags.GetValueOrDefault() & 4) != 0;
            actor.NoLiftDrop = (pose.FloorCarryFlags.GetValueOrDefault() & 1) != 0;
            actor.RelativeToFloor = (pose.FloorCarryFlags.GetValueOrDefault() & 2) != 0;
            actor.RenderStyle = pose.RenderProperties?.Style ?? actor.DefaultRenderStyle;
            actor.Alpha = pose.RenderProperties?.Alpha ?? actor.DefaultAlpha;
            actor.DontThrust = pose.DontThrustFlag == 1;
            actor.DontSeekInvisible = pose.DontSeekInvisibleFlag == 1;
            actor.Shadow = pose.SeekVisibilityFlags is { } visibility ? (visibility & 1) != 0 : actor.DefaultShadow;
            actor.MinVisible = (pose.SeekVisibilityFlags.GetValueOrDefault() & 2) != 0;
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
                actor.Friendly = actor.DefaultFriendly;
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
                crouchPlayer.GruntSpeed = pose.GruntSpeed;
                crouchPlayer.DeltaViewHeight = pose.DeltaViewHeight;
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
            actor.ActorSounds = actor is ProjectileActor soundMissile && pose.ActorSounds.Count == 0
                ? new Dictionary<ActorSoundType, string>(soundMissile.DefaultActorSounds)
                : pose.ActorSounds.Count == 0 && actor.SpawnActorSounds is { } spawnSounds
                ? new Dictionary<ActorSoundType, string>(spawnSounds)
                : new Dictionary<ActorSoundType, string>(pose.ActorSounds);
            actor.FullVolumeDeath = pose.FullVolumeDeath;
            actor.FullVolumeSee = pose.FullVolumeSee;
            actor.OnlySlamSolid = pose.OnlySlamSolid;
            actor.CausePain = pose.CausePain;
            actor.AllowPain = pose.AllowPain;
            actor.NoDamage = pose.NoDamage;
            actor.LaxTelefragDamage = pose.LaxTelefragDamage;
            actor.RetargetAfterSlam = pose.RetargetAfterSlam;
            actor.FullVolumeActive = pose.FullVolumeActive;
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
            actor.RestoreChaseThreshold(pose.ChaseThreshold);
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
                powerPlayer.PowerIronFeetMode = (IronFeetMode)pose.PowerIronFeetMode;
                powerPlayer.PowerMaskTics = pose.PowerMaskTics;
                powerPlayer.PowerIronFeetTics = pose.PowerIronFeetTics;
                powerPlayer.PowerInfiniteAmmoTics = pose.PowerInfiniteAmmoTics;
                powerPlayer.PowerDrainTics = pose.PowerDrainTics;
                powerPlayer.PowerDrainStrength = pose.PowerDrainStrength;
                powerPlayer.PowerRegenerationTics = pose.PowerRegenerationTics;
                powerPlayer.PowerProsperityTics = pose.PowerProsperityTics;
                powerPlayer.PowerSpreadTics = pose.PowerSpreadTics;
                powerPlayer.PowerRegenerationStrength = pose.PowerRegenerationStrength;
                powerPlayer.PowerDoubleFiringSpeedTics = pose.PowerDoubleFiringSpeedTics;
                powerPlayer.PowerHighJumpTics = pose.PowerHighJumpTics;
                powerPlayer.PowerHighJumpPendingRemoval = pose.PowerHighJumpPendingRemoval;
                powerPlayer.PowerHighJumpStrength = pose.PowerHighJumpStrength;
                powerPlayer.DrainStrength = pose.PlayerDrainStrength;
                powerPlayer.GodMode2 = pose.PlayerGodMode2;
                powerPlayer.Buddha2 = pose.PlayerBuddha2;
                powerPlayer.GodMode = pose.PlayerGodMode;
                powerPlayer.DamageCount = pose.PlayerDamageCount;
                powerPlayer.PoisonCount = pose.PlayerPoisonCount;
                powerPlayer.Inventory.RestoreHealthPickups(pose.HealthPickups);
                var weapons = pose.PlayerWeapons ?? new SimWeaponInventory(WeaponKind.Fist | WeaponKind.Pistol, WeaponKind.Pistol, null);
                powerPlayer.Inventory.Weapons = weapons.Owned;
                powerPlayer.Inventory.Selected = weapons.Selected;
                powerPlayer.Inventory.Pending = weapons.Pending;
                powerPlayer.WeaponOffsetY = weapons.OffsetY;
                powerPlayer.WeaponLowering = weapons.Lowering;
                powerPlayer.InstantWeaponSwitch = weapons.InstantSwitch;
                powerPlayer.RestoreTurn(weapons.TurnTicks, weapons.TurnHeld);
                // Capture omits only canonical Doom defaults; older archives cannot recover historical ammo.
                var ammo = pose.PlayerAmmo ?? new SimAmmoInventory(50,
                    0, 0, 0, 200, 50, 50, 300, false);
                {
                    var inventory = powerPlayer.Inventory;
                    inventory.Bullets = ammo.Bullets; inventory.Shells = ammo.Shells;
                    inventory.Rockets = ammo.Rockets; inventory.Cells = ammo.Cells;
                    inventory.MaxBullets = ammo.MaxBullets; inventory.MaxShells = ammo.MaxShells;
                    inventory.MaxRockets = ammo.MaxRockets; inventory.MaxCells = ammo.MaxCells;
                    inventory.HasBackpack = ammo.Backpack;
                }
                powerPlayer.Inventory.BlueKey = (pose.PlayerKeyBits & 1) != 0;
                powerPlayer.Inventory.RedKey = (pose.PlayerKeyBits & 2) != 0;
                powerPlayer.Inventory.YellowKey = (pose.PlayerKeyBits & 4) != 0;
                powerPlayer.PoisonSourceId = pose.PoisonSourceId;
                powerPlayer.PoisonDamageType = pose.PoisonDamageType;
                powerPlayer.PoisonPainType = pose.PoisonPainType;
            }
            actor.DamageType = pose.DamageType ?? (pose.DeathFlags == 1 ? "Massacre" : null);
            if (actor is PlayerPawn damagedPlayer) damagedPlayer.LastDamageType = pose.PlayerLastDamageType;
            actor.DeathDamageAmount = pose.DeathDamageAmount;
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
            {
                pointerProjectile.OwnerFromSave(savedPointers.OwnerId == 0 ? null
                    : _actors.Single(a => a.Id == savedPointers.OwnerId && !a.Destroyed));
                pointerProjectile.RestoreTracerTarget(_actors.Any(a => a.Id == savedPointers.TracerTargetId && !a.Destroyed)
                    ? savedPointers.TracerTargetId : null);
            }
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
            actor.PickupQuiet = (pose.PickupPresentationFlags & 1) != 0;
            actor.NoScreenFlash = (pose.PickupPresentationFlags & 2) != 0;
            actor.NoAttenPickupSound = pose.PickupNoAttenuationFlag == 1;
            actor.PickupHealthOverride = pose.PickupHealthOverride;
            actor.Accuracy = pose.Accuracy;
            actor.FriendlySeeBlocks = pose.FriendlySeeBlocks;
            actor.SeeFriendlyMonsters = pose.SeeFriendlyMonstersFlag != 0;
            actor.LookAllAround = (pose.LookBehaviorFlags & 1) != 0;
            actor.StandStill = (pose.LookBehaviorFlags & 2) != 0;
            actor.InConversation = pose.InConversationFlag != 0;
            actor.DontFollowPlayers = pose.DontFollowPlayersFlag != 0;
            actor.CanJump = pose.CanJumpFlag != 0;
            actor.BlockAsPlayer = pose.BlockAsPlayerFlag != 0;
            actor.FloorClipEnabled = (pose.FloorClipFlags & 1) != 0; actor.SpecialFloorClip = (pose.FloorClipFlags & 2) != 0;
            actor.AvoidHazards = pose.AvoidHazardsFlag != 0;
            actor.StayOnLift = pose.StayOnLiftFlag != 0;
            actor.AvoidMelee = pose.AvoidMeleeFlag != 0; actor.StrafeCount = pose.StrafeCount;
            actor.FastChaseStrafeCount = pose.FastChaseStrafeCount;
            actor.MinMissileChance = pose.MinMissileChanceOverride < 0 ? null : (byte)pose.MinMissileChanceOverride;
            actor.MissileChanceMult = pose.MissileChanceMultOverride;
            actor.MaxTargetRange = pose.MaxTargetRangeOverride; actor.MeleeThreshold = pose.MeleeThresholdOverride;
            actor.CantLeaveFloorPic = pose.CantLeaveFloorPicFlag != 0;
            actor.NoFear = (pose.FearFlags & 1) != 0;
            actor.Frightened = (pose.FearFlags & 2) != 0;
            actor.Frightening = (pose.FearFlags & 4) != 0;
            actor.MoveDirection = (byte)pose.MoveDirection;
            actor.MoveCount = pose.MoveCount;
            actor.Stamina = pose.ActorSpecial?.Stamina ?? 0;
            actor.PickupAutoUseOverride = pose.PickupAutoUseOverride;
            actor.Dropped = pose.Dropped;
            actor.SuppressWeaponPickupAmmo = pose.SuppressWeaponPickupAmmo;
            actor.AlwaysPickupOverride = pose.AlwaysPickupOverride;
            actor.Z = new Fixed(pose.Z);
            actor.VelocityX = new Fixed(pose.VelocityX);
            actor.VelocityY = new Fixed(pose.VelocityY);
            actor.VelocityZ = new Fixed(pose.VelocityZ);
            actor.OnGround = pose.OnGround;
            actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            if (pose.HasPhysics) actor.States.Restore(actor, pose.State, pose.LegacyImmediateSpawnDeath ? 1 : pose.StateTics);
            else actor.States.Restore(actor, actor.IsDead ? actor.DeathState : actor.SpawnState, -1);
            if (pose.BotIdentity?.BasicAttack is not null) actor.Brain?.RestoreBasicAttackState(pose.BasicAttackState);
            else if (pose.BotIdentity is { BrainPresent: true }) actor.Brain?.RestoreProfileChaseHistory(pose.ProfileChaseHistory);
            if (state.IncludesBotIdentities) actor.RestoreDamageHistory(pose.DamageHistory);
            actor.CameraHeight = pose.CameraHeight;
            actor.FullBright = pose.FullBrightFlag is { } brightness ? brightness == 1 : actor.States.CurrentFullBright;
            if (actor is PlayerPawn player)
            {
                player.ClearCommands(); player.AttackPressed = pose.PlayerWeapons?.AttackPressed ?? false; player.UsePressed = false;
                player.Inventory.ArmorType = pose.WornArmorType ?? "None";
                player.SoundClass = pose.SoundClassOverride?.Name;
                player.ExtraLight = pose.PlayerExtraLight;
                player.PowerStrengthTics = pose.PowerStrengthTics;
                player.BonusCount = pose.PlayerBonusCount;
                player.Inventory.Armor = pose.WornArmorAmount;
                (pose.WornArmor ?? SimWornArmor.Default).Restore(player.Inventory);
                player.Inventory.RestoreSpareArmor(pose.SpareArmor);
                player.MaxHealth = pose.PlayerMaxHealth ?? 0;
                player.BonusHealth = pose.ActorSpecial?.BonusHealth ?? 0;
                player.MaxPickupHealth = pose.ActorSpecial?.MaxPickupHealth ?? 0;
                player.WeaponCooldown = pose.WeaponCooldown;
                player.WeaponRefiring = pose.WeaponRefiringFlag == 1;
                if (pose.PlayerInput is { } playerInput) player.RestorePlayerInput(playerInput);
                player.TeleportFreezeTime = pose.TeleportFreezeTime;
                player.RespawnEarliestTic = pose.RespawnEarliestTic;
                player.RespawnArmed = pose.RespawnArmed;
                player.UseHeld = pose.UseHeld;
                if (pose.UseRange is { } useRange) player.UseRange = useRange;
                player.BobTimer = pose.PlayerWeapons?.BobTimer ?? state.Tic;
                player.ViewBobOffset = pose.PlayerWeapons?.ViewBob ?? 0;
                player.MovementBob = pose.PlayerWeapons?.MovementBob ?? 0;
                player.WeaponBobX = pose.PlayerWeapons?.WeaponBobX ?? 0;
                player.WeaponBobY = pose.PlayerWeapons?.WeaponBobY ?? 0;
                if (pose.PlayerWeapons?.BobTimer is null && !player.IsDead)
                    player.UpdateViewBob();
            }
            actor.RememberPosition();
        }

        _motions.Clear();
        if (state.SectorMotions is { } motions) _motions.AddRange(motions.Select(SimSectorMotionArchive.Restore));
        else if (state.DoorMotions is { } doors) _motions.AddRange(doors.Select(SimDoorMotionArchive.Restore));
        _sectorSoundTargets.Clear();
        var restoredTerrains = state.SectorTerrains ?? _initialSectorTerrains;
        for (var i = 0; i < Level.Sectors.Count; i++)
        {
            Level.Sectors[i].FloorTerrain = restoredTerrains[i].Floor;
            Level.Sectors[i].CeilingTerrain = restoredTerrains[i].Ceiling;
        }
        if (state.SectorSoundTargets is { } soundTargets)
            foreach (var pair in soundTargets) _sectorSoundTargets[pair.Key] = pair.Value;
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

        if (state.NextActorId is { } nextActorId) _nextActorId = nextActorId;

        // Support depends on other actors and restored sector planes, so derive it after both are loaded.
        foreach (var actor in _actors) ActorPhysics.RefreshOnMobj(this, actor);

        if (state.IncludesBotIdentities) Invasion.RestoreState(state.Invasion);
        if (state.Acs is { } acs)
        {
            _acsRandomState = acs.RandomState;
            Acs.RestoreState(acs, this);
        }
        if (state.ThinkerOrder is { } order)
        {
            var actorsById = _actors.ToDictionary(actor => actor.Id);
            foreach (var pose in state.Actors)
                if (actorsById.TryGetValue(pose.Id, out var actor)) Thinkers.Remove(actor);
            foreach (var placement in order)
            {
                var actor = actorsById[placement.Id];
                actor.JustSpawned = placement.JustSpawned;
                Thinkers.Add(actor, placement.StatNum);
            }
        }
        RecomputeChecksum();
        _soundRequests.Clear();
        if (!_suppressRestoreNotifications)
        {
            StateRestored?.Invoke();
            PublishStatus();
        }
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
        var spawnTuning = player.SpawnTuning ?? new SimActorTuning(Fixed.FromInt(1).Raw, 4, 0);
        player.MovementSpeed = new Fixed(spawnTuning.SpeedRaw);
        player.FloatSpeed = spawnTuning.FloatSpeed;
        player.PainThreshold = spawnTuning.PainThreshold;
        player.HasTuningOverride = false;
        player.CanPickupItems = player.SpawnCanPickupItems;
        player.SpecialPickup = player.SpawnSpecialPickup;
        player.MaxHealth = 0;
        player.Stamina = 0;
        player.Accuracy = 0;
        player.FriendlySeeBlocks = 10;
        player.MoveDirection = 0; player.MoveCount = 0;
        player.BonusHealth = 0;
        player.MaxPickupHealth = 0;
        player.LastDamageType = null;
        player.DeathDamageAmount = 0;
        player.Health = player.ResurrectionHealth > 0 ? player.ResurrectionHealth : 100;
        player.Solid = (player.ResurrectionCollisionFlags.GetValueOrDefault(3) & 1) != 0;
        player.HasMovementActionOverride = false;
        player.HasDefensePropertyOverride = false;
        player.DamageCount = 0;
        player.ExtraLight = 0;
        player.BonusCount = 0;
        player.PoisonCount = 0;
        player.PoisonSourceId = null;
        player.PoisonDamageType = null;
        player.PoisonPainType = null;
        player.Height = player.ResurrectionHeight ?? player.Height;
        player.Radius = player.ResurrectionRadius ?? player.Radius;
        player.FullHeight = player.Height.ToDouble();
        player.HasSizeOverride = false;
        player.SpawnCeiling = (player.ResurrectionCollisionFlags.GetValueOrDefault() & 64) != 0;
        player.SpawnFloat = player.DefaultSpawnFloat;
        ActorPhysics.PlaceOnFloor(this, player);
        if (player.SpawnCeiling)
            player.Z = Fixed.FromDouble(CeilingOf(player.SectorIndex) - player.Height.ToDouble());
        else if (player.SpawnFloat)
            player.Z = Fixed.FromDouble(RandomFloatSpawnZ(player));
        player.OnGround = player.Z.ToDouble() <= FloorOf(player.SectorIndex);
        if (player.FloorClipEnabled) ActorPropertyActions.AdjustFloorClip(this, player);
        player.ClearCommands();
        player.AttackPressed = false;
        player.WeaponRefiring = false;
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
        player.DeltaViewHeight = 0;
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
        SwitchActions.Tick(this);
        WaggleActions.Tick(this);
        PusherActions.Tick(this);
        LightTransferActions.Tick(this);
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
            ActorPhysics.FitToSector(this, actor, carryFloor: false, sectorPinchCrush: true);
        Acs.Tick(this, levelTic);
        Invasion.Tick(this);
        if (RewindEnabled)
            Rewind.CaptureIfDue(this);
        RecomputeChecksum();
        PublishStatus();
    }

    private void ResolveAttacks()
    {
        foreach (var player in Players.ToArray())
        {
            if (player.IsDead || player.WeaponCooldown == 0 && (!player.AttackHeld || !player.WeaponReady))
                player.WeaponRefiring = false;
            if (!player.AttackPressed)
                continue;
            player.AttackPressed = false;
            if (HitscanCombat.Fire(this, player)) player.WeaponRefiring = true;
            else if (player.WeaponCooldown == 0) player.WeaponRefiring = false;
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
                if (taken.Contains(actor) || !actor.SpecialPickup || !PickupCatalog.IsPickup(actor.DoomEdNum, RavenHealthPickups))
                    continue;
                if (!PickupCatalog.IsWithinHorizontalReach(player, actor) || !PickupCatalog.IsWithinVerticalReach(player, actor))
                    continue;
                var key = PickupCatalog.IsKey(actor.DoomEdNum);
                var weapon = RavenHealthPickups && actor.DoomEdNum == 82 ? 0 : PickupCatalog.PickupWeapon(actor.DoomEdNum);
                var shouldStay = key && GameMode != SpawnGameMode.Single
                    || weapon != 0 && NonDroppedWeaponsStay && !actor.Dropped;
                if (shouldStay && weapon != 0 && player.Inventory.Owns(weapon)) continue;
                if (!PickupCatalog.TryGive(player, actor.DoomEdNum, actor.IgnoreAmmoSkill, actor.Depleted, actor.PickupAmount,
                    actor.SuppressWeaponPickupAmmo, actor.PickupHealthOverride, actor.PickupAutoUseOverride)
                    && !(key && GameMode == SpawnGameMode.Single)
                    && (shouldStay || !(actor.AlwaysPickupOverride ?? PickupCatalog.AlwaysPickup(actor.DoomEdNum, StrifeHealthPickups))))
                    continue;
                if (!actor.PickupQuiet)
                    ActorPickupActions.PlayPickupSound(actor, player);
                if (!actor.PickupQuiet && !actor.NoScreenFlash) player.BonusCount = 6;
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
        for (var i = 0; i < Level.Sectors.Count; i++)
        {
            var terrain = new SimSectorTerrain(Level.Sectors[i].FloorTerrain, Level.Sectors[i].CeilingTerrain);
            if (terrain == _initialSectorTerrains[i]) continue;
            hash = Mix(hash, 0x53544552u); hash = Mix(hash, (uint)i);
            hash = Mix(hash, (uint)terrain.Floor.Length);
            foreach (var character in terrain.Floor) hash = Mix(hash, character);
            hash = Mix(hash, (uint)terrain.Ceiling.Length);
            foreach (var character in terrain.Ceiling) hash = Mix(hash, character);
        }
        hash = DamageTypes.MixChecksum(hash);
        if (GibFactor != 1) { hash = Mix(hash, 0x47494246u); hash = MixDouble(hash, GibFactor); }
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
        foreach (var splash in Level.TerrainSplashDefinitions)
        {
            if (splash.NoAlert)
            {
                hash = Mix(hash, 0x54534e41u); hash = Mix(hash, (uint)splash.Name.Length);
                foreach (var character in splash.Name) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            if (splash.SmallSplashSound.Length != 0)
            {
                hash = Mix(hash, 0x54535353u);
                hash = Mix(hash, (uint)splash.Name.Length);
                foreach (var character in splash.Name) hash = Mix(hash, char.ToUpperInvariant(character));
                hash = Mix(hash, (uint)splash.SmallSplashSound.Length);
                foreach (var character in splash.SmallSplashSound) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            if (splash.NormalSplashSound.Length == 0) continue;
            hash = Mix(hash, 0x54534e53u);
            hash = Mix(hash, (uint)splash.Name.Length);
            foreach (var character in splash.Name) hash = Mix(hash, char.ToUpperInvariant(character));
            hash = Mix(hash, (uint)splash.NormalSplashSound.Length);
            foreach (var character in splash.NormalSplashSound) hash = Mix(hash, char.ToUpperInvariant(character));
        }
        if (Level.TerrainDefinitions.Count != 0)
        {
            hash = Mix(hash, 0x54455244u);
            foreach (var terrain in Level.TerrainDefinitions)
            {
                if (terrain.FootClip != 0) { hash = Mix(hash, 0x5446434cu); hash = MixDouble(hash, terrain.FootClip); }
                if (terrain.AllowProtection) hash = Mix(hash, 0x54415052u);
                if (terrain.IsLiquid) hash = Mix(hash, 0x544c4951u);
                if (terrain.StepVolume != 0) { hash = Mix(hash, 0x5453564fu); hash = MixDouble(hash, terrain.StepVolume); }
                if (terrain.WalkStepTics != 0) { hash = Mix(hash, 0x54575354u); hash = Mix(hash, unchecked((uint)terrain.WalkStepTics)); }
                if (terrain.RunStepTics != 0) { hash = Mix(hash, 0x54525354u); hash = Mix(hash, unchecked((uint)terrain.RunStepTics)); }
                if (terrain.StepDistance != 0) { hash = Mix(hash, 0x54534449u); hash = MixDouble(hash, terrain.StepDistance); }
                if (terrain.StepDistanceMinVel != 0) { hash = Mix(hash, 0x54534d56u); hash = MixDouble(hash, terrain.StepDistanceMinVel); }
                if (terrain.StepSound.Length != 0)
                {
                    hash = Mix(hash, 0x5453534eu); hash = Mix(hash, (uint)terrain.StepSound.Length);
                    foreach (var character in terrain.StepSound) hash = Mix(hash, char.ToUpperInvariant(character));
                }
                if (terrain.LeftStepSound.Length != 0)
                {
                    hash = Mix(hash, 0x544c534eu); hash = Mix(hash, (uint)terrain.LeftStepSound.Length);
                    foreach (var character in terrain.LeftStepSound) hash = Mix(hash, char.ToUpperInvariant(character));
                }
                if (terrain.RightStepSound.Length != 0)
                {
                    hash = Mix(hash, 0x5452534eu); hash = Mix(hash, (uint)terrain.RightStepSound.Length);
                    foreach (var character in terrain.RightStepSound) hash = Mix(hash, char.ToUpperInvariant(character));
                }
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
        if (Level.FallingDamage != HCDE.Gamedata.FallingDamageStyle.None)
        { hash = Mix(hash, 0x46444d47u); hash = Mix(hash, (uint)Level.FallingDamage); }
        if (Level.MonsterFallingDamage) hash = Mix(hash, 0x4d46444du);
        if (Level.ProperMonsterFallingDamage) hash = Mix(hash, 0x504d4644u);
        if (Level.AllowMonsterTelefrags) hash = Mix(hash, 0x4d544652u);
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
        if (ForcedFallingDamage != HCDE.Gamedata.FallingDamageStyle.None)
        { hash = Mix(hash, 0x4646444du); hash = Mix(hash, (uint)ForcedFallingDamage); }
        if (FallingDeathSplatEnabled) hash = Mix(hash, 0x46445350u);
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
        if (NoMonsterPain) hash = Mix(hash, 0x4e4d504eu);
        if (Level.Lines.Any(line => line.Impact))
        {
            hash = Mix(hash, 0x4c564e4du);
            hash = Mix(hash, unchecked((uint)(Level.LevelNumber ?? HCDE.Gamedata.MapInfoParser.DefaultLevelNum(Level.MapName, out _))));
        }
        if (NativeMonsterMovement) hash = Mix(hash, 0x4e4d4d56u);
        if (SkillAggressiveness != 0)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(SkillAggressiveness));
            hash = Mix(hash, 0x534b4147u); hash = Mix(hash, (uint)factorBits); hash = Mix(hash, (uint)(factorBits >> 32));
        }
        if (PlayerDamageFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(PlayerDamageFactor));
            hash = Mix(hash, 0x50444641u); hash = Mix(hash, (uint)factorBits); hash = Mix(hash, (uint)(factorBits >> 32));
        }
        if (AutoUseHealth) hash = Mix(hash, 0x4155484cu);
        if (DisableAutoHealth) hash = Mix(hash, 0x4441484cu);
        if (StrifeHealthPickups) hash = Mix(hash, 0x53544850u);
        if (RavenHealthPickups) hash = Mix(hash, 0x52564850u);
        if (UnlimitedHealthPickups) hash = Mix(hash, 0x5548504bu);
        if (InfiniteHealthItemUse) hash = Mix(hash, 0x49485553u);
        if (InfiniteStoredInventoryUse) hash = Mix(hash, 0x49534955u);
        if (InfiniteAmmo) hash = Mix(hash, 0x49414d4fu);
        if (DefaultInventoryMaximum != 25) { hash = Mix(hash, 0x44494d58u); hash = Mix(hash, (uint)DefaultInventoryMaximum); }
        if (SkillDamageFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(SkillDamageFactor));
            hash = Mix(hash, 0x53444641u); hash = Mix(hash, (uint)factorBits); hash = Mix(hash, (uint)(factorBits >> 32));
        }
        if (MonsterDamageFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(MonsterDamageFactor));
            hash = Mix(hash, 0x4d444641u); hash = Mix(hash, (uint)factorBits); hash = Mix(hash, (uint)(factorBits >> 32));
        }
        if (FriendlyDamageFactor != 1)
        {
            var factorBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(FriendlyDamageFactor));
            hash = Mix(hash, 0x46444641u); hash = Mix(hash, (uint)factorBits); hash = Mix(hash, (uint)(factorBits >> 32));
        }
        foreach (var pair in _sectorSoundTargets.OrderBy(pair => pair.Key))
            if (Actors.Any(a => a.Id == pair.Value && !a.Destroyed))
            { hash = Mix(hash, 0x534e4454u); hash = Mix(hash, (uint)pair.Key); hash = Mix(hash, pair.Value); }
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
        if (_hasGunShotRandomState)
        { hash = Mix(hash, 0x47554e53u); hash = Mix(hash, (uint)_gunShotRandomState); hash = Mix(hash, (uint)(_gunShotRandomState >> 32)); }
        if (_hasFireSg2RandomState)
        { hash = Mix(hash, 0x46534732u); hash = Mix(hash, (uint)_fireSg2RandomState); hash = Mix(hash, (uint)(_fireSg2RandomState >> 32)); }
        if (_hasBfgSprayRandomState)
        { hash = Mix(hash, 0x42464750u); hash = Mix(hash, (uint)_bfgSprayRandomState); hash = Mix(hash, (uint)(_bfgSprayRandomState >> 32)); }
        if (_hasBfgSelfDamageRandomState)
        { hash = Mix(hash, 0x42464753u); hash = Mix(hash, (uint)_bfgSelfDamageRandomState); hash = Mix(hash, (uint)(_bfgSelfDamageRandomState >> 32)); }
        if (_hasFreezeChunksRandomState)
        { hash = Mix(hash, 0x46525a43u); hash = Mix(hash, (uint)_freezeChunksRandomState); hash = Mix(hash, (uint)(_freezeChunksRandomState >> 32)); }
        if (_hasPlayerSearchRandomState)
        { hash = Mix(hash, 0x50535243u); hash = Mix(hash, (uint)_playerSearchRandomState); hash = Mix(hash, (uint)(_playerSearchRandomState >> 32)); }
        if (_hasPlayerLookRandomState)
        { hash = Mix(hash, 0x504c4f4bu); hash = Mix(hash, (uint)_playerLookRandomState); hash = Mix(hash, (uint)(_playerLookRandomState >> 32)); }
        if (CompatibilityInvisibility) hash = Mix(hash, 0x43494e56u);
        if (VerticalWeaponSpread) hash = Mix(hash, 0x56535052u);
        if (_hasFloatSpawnRandomState)
        { hash = Mix(hash, 0x46535052u); hash = Mix(hash, (uint)_floatSpawnRandomState); hash = Mix(hash, (uint)(_floatSpawnRandomState >> 32)); }
        if (_hasHateSearchRandomState)
        { hash = Mix(hash, 0x48415445u); hash = Mix(hash, (uint)_hateSearchRandomState); hash = Mix(hash, (uint)(_hateSearchRandomState >> 32)); }
        if (_hasActorDieRandomState)
        { hash = Mix(hash, 0x41444945u); hash = Mix(hash, (uint)_actorDieRandomState); hash = Mix(hash, (uint)(_actorDieRandomState >> 32)); }
        if (_hasPuffRandomState)
        { hash = Mix(hash, 0x50554646u); hash = Mix(hash, (uint)_puffRandomState); hash = Mix(hash, (uint)(_puffRandomState >> 32)); }
        if (_hasDropItemRandomState)
        { hash = Mix(hash, 0x44524f50u); hash = Mix(hash, (uint)_dropItemRandomState); hash = Mix(hash, (uint)(_dropItemRandomState >> 32)); }
        if (_hasChaseRandomState)
        { hash = Mix(hash, 0x43484153u); hash = Mix(hash, (uint)_chaseRandomState); hash = Mix(hash, (uint)(_chaseRandomState >> 32)); }
        if (_hasMissileDamageRandomState)
        { hash = Mix(hash, 0x4d53444du); hash = Mix(hash, (uint)_missileDamageRandomState); hash = Mix(hash, (uint)(_missileDamageRandomState >> 32)); }
        if (_hasPoisonDamageRandomState)
        { hash = Mix(hash, 0x50534e44u); hash = Mix(hash, (uint)_poisonDamageRandomState); hash = Mix(hash, (uint)(_poisonDamageRandomState >> 32)); }
        if (_hasReflectRandomState)
        { hash = Mix(hash, 0x52454652u); hash = Mix(hash, (uint)_reflectRandomState); hash = Mix(hash, (uint)(_reflectRandomState >> 32)); }
        if (_hasSeekerMissileRandomState)
        { hash = Mix(hash, 0x5345454bu); hash = Mix(hash, (uint)_seekerMissileRandomState); hash = Mix(hash, (uint)(_seekerMissileRandomState >> 32)); }
        if (_hasPainChanceRandomState)
        { hash = Mix(hash, 0x5043484eu); hash = Mix(hash, (uint)_painChanceRandomState); hash = Mix(hash, (uint)(_painChanceRandomState >> 32)); }
        if (_hasSkelFistRandomState)
        { hash = Mix(hash, 0x534b4649u); hash = Mix(hash, (uint)_skelFistRandomState); hash = Mix(hash, (uint)(_skelFistRandomState >> 32)); }
        if (_hasSawRandomState)
        { hash = Mix(hash, 0x53415752u); hash = Mix(hash, (uint)_sawRandomState); hash = Mix(hash, (uint)(_sawRandomState >> 32)); }
        if (_hasPunchRandomState)
        { hash = Mix(hash, 0x50554e52u); hash = Mix(hash, (uint)_punchRandomState); hash = Mix(hash, (uint)(_punchRandomState >> 32)); }
        if (_newChaseDirRandomState is { } directionRandom)
        { hash = Mix(hash, 0x4e434452u); hash = Mix(hash, (uint)directionRandom); hash = Mix(hash, (uint)(directionRandom >> 32)); }
        if (_tryWalkRandomState is { } walkRandom)
        { hash = Mix(hash, 0x54574c4bu); hash = Mix(hash, (uint)walkRandom); hash = Mix(hash, (uint)(walkRandom >> 32)); }
        if (_stayOnLiftRandomState is { } liftRandom)
        { hash = Mix(hash, 0x53544c52u); hash = Mix(hash, (uint)liftRandom); hash = Mix(hash, (uint)(liftRandom >> 32)); }
        if (_scaredyCatRandomState is { } scaredRandom)
        { hash = Mix(hash, 0x53434154u); hash = Mix(hash, (uint)scaredRandom); hash = Mix(hash, (uint)(scaredRandom >> 32)); }
        if (_checkMissileRandomState is { } missileRandom)
        { hash = Mix(hash, 0x434d524eu); hash = Mix(hash, (uint)missileRandom); hash = Mix(hash, (uint)(missileRandom >> 32)); }
        if (_defectRandomState is { } defectRandom)
        { hash = Mix(hash, 0x44454643u); hash = Mix(hash, (uint)defectRandom); hash = Mix(hash, (uint)(defectRandom >> 32)); }
        if (_checkSightRandomState is { } sightRandom)
        { hash = Mix(hash, 0x43534947u); hash = Mix(hash, (uint)sightRandom); hash = Mix(hash, (uint)(sightRandom >> 32)); }
        if (_uniqueTidRandomState is { } uniqueRandom)
        { hash = Mix(hash, 0x55544944u); hash = Mix(hash, (uint)uniqueRandom); hash = Mix(hash, (uint)(uniqueRandom >> 32)); }
        if (_sectorDamageRandomState is { } sectorRandom)
        { hash = Mix(hash, 0x53444352u); hash = Mix(hash, (uint)sectorRandom); hash = Mix(hash, (uint)(sectorRandom >> 32)); }
        if (_nativeSwitchTargetRandomState is { } switchRandom)
        { hash = Mix(hash, 0x53575447u); hash = Mix(hash, (uint)switchRandom); hash = Mix(hash, (uint)(switchRandom >> 32)); }
        if (_avoidCrushRandomState is { } avoidRandom)
        { hash = Mix(hash, 0x41564352u); hash = Mix(hash, (uint)avoidRandom); hash = Mix(hash, (uint)(avoidRandom >> 32)); }
        if (_enemyStrafeRandomState is { } strafeRandom)
        { hash = Mix(hash, 0x45535452u); hash = Mix(hash, (uint)strafeRandom); hash = Mix(hash, (uint)(strafeRandom >> 32)); }
        if (_hasSkipTargetRandomState)
        { hash = Mix(hash, 0x534b5054u); hash = Mix(hash, (uint)_skipTargetRandomState); hash = Mix(hash, (uint)(_skipTargetRandomState >> 32)); }
        if (_hasCPosRefireRandomState)
        { hash = Mix(hash, 0x43505246u); hash = Mix(hash, (uint)_cPosRefireRandomState); hash = Mix(hash, (uint)(_cPosRefireRandomState >> 32)); }
        if (_hasCPosAttackRandomState)
        { hash = Mix(hash, 0x43504f53u); hash = Mix(hash, (uint)_cPosAttackRandomState); hash = Mix(hash, (uint)(_cPosAttackRandomState >> 32)); }
        if (_hasSPosAttackRandomState)
        { hash = Mix(hash, 0x53504f53u); hash = Mix(hash, (uint)_sPosAttackRandomState); hash = Mix(hash, (uint)(_sPosAttackRandomState >> 32)); }
        if (_hasPosAttackRandomState)
        { hash = Mix(hash, 0x504f5341u); hash = Mix(hash, (uint)_posAttackRandomState); hash = Mix(hash, (uint)(_posAttackRandomState >> 32)); }
        if (_hasBruisAttackRandomState)
        { hash = Mix(hash, 0x42525549u); hash = Mix(hash, (uint)_bruisAttackRandomState); hash = Mix(hash, (uint)(_bruisAttackRandomState >> 32)); }
        if (_hasHeadAttackRandomState)
        { hash = Mix(hash, 0x48454144u); hash = Mix(hash, (uint)_headAttackRandomState); hash = Mix(hash, (uint)(_headAttackRandomState >> 32)); }
        if (_hasTroopAttackRandomState)
        { hash = Mix(hash, 0x54524f4fu); hash = Mix(hash, (uint)_troopAttackRandomState); hash = Mix(hash, (uint)(_troopAttackRandomState >> 32)); }
        if (_hasSargAttackRandomState)
        { hash = Mix(hash, 0x53415247u); hash = Mix(hash, (uint)_sargAttackRandomState); hash = Mix(hash, (uint)(_sargAttackRandomState >> 32)); }
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
            if (actor.Brain is null && (actor.ChaseThreshold != 0 || actor.DefaultChaseThreshold != 100))
            {
                hash = Mix(hash, 0x43544852u);
                hash = Mix(hash, unchecked((uint)actor.ChaseThreshold));
                hash = Mix(hash, unchecked((uint)actor.DefaultChaseThreshold));
            }
            if (actor.Brain is null && actor is not ProjectileActor && actor.TargetId is { } targetId)
            { hash = Mix(hash, 0x54415247u); hash = Mix(hash, targetId); }
            hash = Mix(hash, actor.Floating ? 1u : 0u);
            hash = Mix(hash, actor.AllowDropOff ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.MaxDropOffHeight.Raw));
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.FloatSpeed).Raw));
            hash = Mix(hash, (uint)actor.ResurrectionHealth);
            if (actor.LastLookActorId is { } lastLook) { hash = Mix(hash, 0x4c4c4f4fu); hash = Mix(hash, lastLook); }
            if (actor.NoSightCheck) hash = Mix(hash, 0x4e534348u);
            if (actor.HuntPlayers) hash = Mix(hash, 0x48554e54u);
            if (actor.ChaseGoal) hash = Mix(hash, 0x43474f41u);
            if (actor.JustAttacked) hash = Mix(hash, 0x4a415454u);
            if (actor.InCombat) hash = Mix(hash, 0x49434f4du);
            if (actor.NoSplashAlert) hash = Mix(hash, 0x4e53504cu);
            if (actor.DontSplash) hash = Mix(hash, 0x4453504cu);
            if (actor.DontHarmSpecies) hash = Mix(hash, 0x44485350u);
            if (actor.DontHarmClass) hash = Mix(hash, 0x4448434cu);
            if (actor.LandingSpeed != Actor.DefaultLandingSpeed)
            {
                hash = Mix(hash, 0x4c535044u);
                var landingBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(actor.LandingSpeed));
                hash = Mix(hash, (uint)landingBits); hash = Mix(hash, (uint)(landingBits >> 32));
            }
            if (actor.LastPushFromMovement) hash = Mix(hash, 0x50534848u);
            if (actor.BlockedBySolidActors) hash = Mix(hash, 0x42535341u);
            if (actor.BumpSpecial) hash = Mix(hash, 0x42554d50u);
            if (actor.Reflective) hash = Mix(hash, 0x5245464cu);
            if (actor.ThruReflect) hash = Mix(hash, 0x54524546u);
            if (actor.DontReflect) hash = Mix(hash, 0x44524546u);
            if (actor.MirrorReflect) hash = Mix(hash, 0x4d524546u);
            if (actor.Deflect) hash = Mix(hash, 0x44464c43u);
            if (actor.ShieldReflect) hash = Mix(hash, 0x53485246u);
            if (actor.NoShieldReflect) hash = Mix(hash, 0x4e535246u);
            if (actor.AimReflect) hash = Mix(hash, 0x41495246u);
            if (actor.SeekerMissile) hash = Mix(hash, 0x53454b52u);
            if (actor.SummonedMonster) hash = Mix(hash, 0x53554d4du);
            if (actor.ScreenSeeker) hash = Mix(hash, 0x5343524eu);
            if (actor.VisibilityPulse || actor.VisibilityDirection != 0)
            {
                hash = Mix(hash, 0x5650554cu);
                hash = Mix(hash, actor.VisibilityPulse ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)actor.VisibilityDirection));
            }
            if (actor.Stealth || actor.StealthAlpha != 0)
            {
                hash = Mix(hash, 0x53544c54u);
                hash = Mix(hash, actor.Stealth ? 1u : 0u);
                hash = MixDouble(hash, actor.StealthAlpha);
            }
            if (actor.DefaultStealth) hash = Mix(hash, 0x4453544cu);
            if (actor.DefaultFloatBob) hash = Mix(hash, 0x44464242u);
            if (actor.DefaultReflective) hash = Mix(hash, 0x4452464Cu);
            if (actor.DefaultThruGhost) hash = Mix(hash, 0x44544748u);
            if (actor.DefaultNonShootable) hash = Mix(hash, 0x444E5348u);
            if (actor.DefaultFloorPic) hash = Mix(hash, 0x44464C50u);
            if (actor.DefaultRip) hash = Mix(hash, 0x44524950u);
            if (actor.DefaultFloorClip) hash = Mix(hash, 0x4446434Cu);
            if (actor.DefaultBlasted) hash = Mix(hash, 0x44424C53u);
            if (actor.DefaultTouchy) hash = Mix(hash, 0x44544348u);
            if (actor.DefaultNoClip) hash = Mix(hash, 0x444E434Cu);
            if (actor.DefaultInFloat) hash = Mix(hash, 0x4449464Cu);
            if (actor.DefaultCorpse) hash = Mix(hash, 0x44435250u);
            if (actor.DefaultFly) hash = Mix(hash, 0x44464C59u);
            if (actor.SpawnFloat || actor.DefaultSpawnFloat)
            { hash = Mix(hash, 0x5350464Cu); hash = Mix(hash, (actor.SpawnFloat ? 1u : 0u) | (actor.DefaultSpawnFloat ? 2u : 0u)); }
            if (actor.DefaultCombatFlags != 0) { hash = Mix(hash, 0x44434D42u); hash = Mix(hash, (uint)actor.DefaultCombatFlags); }
            if (actor.DefaultPushFlags != 0) { hash = Mix(hash, 0x44505553u); hash = Mix(hash, (uint)actor.DefaultPushFlags); }
            if (actor.DefaultTeleStomp != (actor is PlayerPawn))
                hash = Mix(hash, actor.DefaultTeleStomp ? 0x44545331u : 0x44545330u);
            if (actor.DefaultSeeker != (actor is ProjectileActor { Kind: ProjectileKind.RevenantTracer }))
                hash = Mix(hash, actor.DefaultSeeker ? 0x44534B31u : 0x44534B30u);
            if (actor.FloatBob || actor.FloatBobStrength != 1 || actor.FloatBobFactor != 1)
            {
                hash = Mix(hash, 0x46424f42u);
                hash = Mix(hash, actor.FloatBob ? 1u : 0u);
                var strength = unchecked((ulong)BitConverter.DoubleToInt64Bits(actor.FloatBobStrength));
                var factor = unchecked((ulong)BitConverter.DoubleToInt64Bits(actor.FloatBobFactor));
                hash = Mix(hash, (uint)strength); hash = Mix(hash, (uint)(strength >> 32));
                hash = Mix(hash, (uint)factor); hash = Mix(hash, (uint)(factor >> 32));
            }
            if (actor.LastBumpTic != 0) { hash = Mix(hash, 0x42554d54u); hash = Mix(hash, unchecked((uint)actor.LastBumpTic)); }
            if (actor.FallDamage) hash = Mix(hash, 0x46414c4cu);
            if (actor.Smashable) hash = Mix(hash, 0x534d5348u);
            if (actor.Crashed) hash = Mix(hash, 0x43525344u);
            if (actor.DontGib) hash = Mix(hash, 0x44474942u);
            if (actor.TeleStomp != (actor is PlayerPawn)) hash = Mix(hash, actor.TeleStomp ? 0x54535431u : 0x54535430u);
            if (actor.NoTeleStomp) hash = Mix(hash, 0x4e545354u);
            if (actor.CrashState != -1) { hash = Mix(hash, 0x43525331u); hash = Mix(hash, unchecked((uint)actor.CrashState)); }
            if (actor.ExtremeCrashState != -1) { hash = Mix(hash, 0x43525332u); hash = Mix(hash, unchecked((uint)actor.ExtremeCrashState)); }
            if (actor.CrushState != -1) { hash = Mix(hash, 0x43525348u); hash = Mix(hash, unchecked((uint)actor.CrushState)); }
            if (actor.Touchy) hash = Mix(hash, 0x544f5543u);
            if (actor.Armed) hash = Mix(hash, 0x41524d44u);
            if (actor.Vulnerable) hash = Mix(hash, 0x56554c4eu);
            if (actor.LastLookPlayerNumber != 0) { hash = Mix(hash, 0x4c4c504eu); hash = Mix(hash, (uint)actor.LastLookPlayerNumber); }
            if (actor.SeeInvisible) hash = Mix(hash, 0x5345494eu);
            if (actor.MVisBlocked) hash = Mix(hash, 0x4d564953u);
            hash = Mix(hash, unchecked((uint)actor.GibHealth));
            hash = Mix(hash, unchecked((uint)actor.SeeState));
            if (actor.SlamState != -1)
            { hash = Mix(hash, 0x534c414du); hash = Mix(hash, unchecked((uint)actor.SlamState)); }
            if (actor.IdleState != -1)
            { hash = Mix(hash, 0x49444c45u); hash = Mix(hash, unchecked((uint)actor.IdleState)); }
            if (actor.HealState != -1)
            { hash = Mix(hash, 0x4845414cu); hash = Mix(hash, unchecked((uint)actor.HealState)); }
            if (actor.RaiseState >= 0) { hash = Mix(hash, 0x52414953u); hash = Mix(hash, (uint)actor.RaiseState); }
            hash = Mix(hash, unchecked((uint)actor.ExtremeDeathState));
            hash = Mix(hash, unchecked((uint)actor.GenericFreezeDeath));
            hash = Mix(hash, actor.NoIceDeath ? 1u : 0u);
            if (actor.SpecialFireDamage) hash = Mix(hash, 0x53464952u);
            if (actor.Accuracy != 0) { hash = Mix(hash, 0x41434355u); hash = Mix(hash, unchecked((uint)actor.Accuracy)); }
            if (actor.FriendlySeeBlocks != 10) { hash = Mix(hash, 0x4653424cu); hash = Mix(hash, unchecked((uint)actor.FriendlySeeBlocks)); }
            if (actor.SeeFriendlyMonsters) hash = Mix(hash, 0x53464d4fu);
            if (actor.InConversation) hash = Mix(hash, 0x434f4e56u);
            if (actor.DontFollowPlayers) hash = Mix(hash, 0x4446504cu);
            if (actor.CanJump) hash = Mix(hash, 0x434a4d50u);
            if (actor.BlockAsPlayer) hash = Mix(hash, 0x4241504cu);
            if (actor.FloorClipEnabled || actor.SpecialFloorClip)
            { hash = Mix(hash, 0x46434c46u); hash = Mix(hash, (actor.FloorClipEnabled ? 1u : 0u) | (actor.SpecialFloorClip ? 2u : 0u)); }
            if (actor.AvoidHazards) hash = Mix(hash, 0x4156485au);
            if (actor.StayOnLift) hash = Mix(hash, 0x53544c46u);
            if (actor.AvoidMelee || actor.StrafeCount != 0)
            { hash = Mix(hash, 0x41564d4cu); hash = Mix(hash, actor.AvoidMelee ? 1u : 0u); hash = Mix(hash, unchecked((uint)actor.StrafeCount)); }
            if (actor.FastChaseStrafeCount != 0)
            { hash = Mix(hash, 0x46435343u); hash = Mix(hash, unchecked((uint)actor.FastChaseStrafeCount)); }
            if (actor.MinMissileChance is { } chance)
            { hash = Mix(hash, 0x4d4d4348u); hash = Mix(hash, chance); }
            if (actor.MissileChanceMult is { } multiplier)
            {
                var multiplierBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(multiplier));
                hash = Mix(hash, 0x4d434d55u); hash = Mix(hash, (uint)multiplierBits); hash = Mix(hash, (uint)(multiplierBits >> 32));
            }
            if (actor.MaxTargetRange is { } maximumRange)
            {
                var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(maximumRange));
                hash = Mix(hash, 0x4d545247u); hash = Mix(hash, (uint)bits); hash = Mix(hash, (uint)(bits >> 32));
            }
            if (actor.MeleeThreshold is { } meleeThreshold)
            {
                var bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(meleeThreshold));
                hash = Mix(hash, 0x4d544852u); hash = Mix(hash, (uint)bits); hash = Mix(hash, (uint)(bits >> 32));
            }
            if (actor.CantLeaveFloorPic) hash = Mix(hash, 0x434c4650u);
            if (actor.NoFear || actor.Frightened || actor.Frightening)
            { hash = Mix(hash, 0x46454152u); hash = Mix(hash, (actor.NoFear ? 1u : 0u) | (actor.Frightened ? 2u : 0u) | (actor.Frightening ? 4u : 0u)); }
            if (actor.MoveDirection != 0 || actor.MoveCount != 0)
            { hash = Mix(hash, 0x4d564452u); hash = Mix(hash, actor.MoveDirection); hash = Mix(hash, unchecked((uint)actor.MoveCount)); }
            if (actor.LookAllAround || actor.StandStill)
            { hash = Mix(hash, 0x4c4f4f4bu); hash = Mix(hash, (actor.LookAllAround ? 1u : 0u) | (actor.StandStill ? 2u : 0u)); }
            if (actor is not PlayerPawn && actor.Stamina != 0)
            { hash = Mix(hash, 0x5354414du); hash = Mix(hash, unchecked((uint)actor.Stamina)); }
            if (actor.NoAttenPickupSound) hash = Mix(hash, 0x504b4e41u);
            if (actor.PickupHealthOverride is { } pickupHealth)
            {
                hash = Mix(hash, 0x504b4850u);
                hash = Mix(hash, unchecked((uint)pickupHealth));
            }
            if (actor.PickupAutoUseOverride is { } pickupAutoUse)
            {
                hash = Mix(hash, 0x504b4155u);
                hash = Mix(hash, unchecked((uint)pickupAutoUse));
            }
            if (actor.PickupQuiet || actor.NoScreenFlash)
            {
                hash = Mix(hash, 0x504b5052u);
                hash = Mix(hash, (actor.PickupQuiet ? 1u : 0u) | (actor.NoScreenFlash ? 2u : 0u));
            }
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
            if (!string.IsNullOrEmpty(actor.PainType) && !string.Equals(actor.PainType, "None", StringComparison.OrdinalIgnoreCase))
            {
                hash = Mix(hash, 0x50545950u);
                hash = Mix(hash, (uint)actor.PainType.Length);
                foreach (var character in actor.PainType) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            if (!string.IsNullOrEmpty(actor.DeathType) && !string.Equals(actor.DeathType, "None", StringComparison.OrdinalIgnoreCase))
            {
                hash = Mix(hash, 0x44545950u);
                hash = Mix(hash, (uint)actor.DeathType.Length);
                foreach (var character in actor.DeathType) hash = Mix(hash, char.ToUpperInvariant(character));
            }
            foreach (var typed in actor.TypedCrashes())
            {
                hash = Mix(hash, 0x54435253u);
                hash = Mix(hash, typed.Extreme ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)typed.State));
                foreach (var character in typed.Type) hash = Mix(hash, char.ToUpperInvariant(character));
                hash = Mix(hash, 0);
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
            if (actor.FlyCheat) hash = Mix(hash, 0x464c5943u);
            if (actor.NoClip) hash = Mix(hash, 0x4e434c50u);
            if (actor is PlayerPawn { NoClip2: true }) hash = Mix(hash, 0x4e434c32u);
            if (actor is PlayerPawn frozen && (frozen.Frozen || frozen.TotallyFrozen))
            { hash = Mix(hash, 0x5046525au); hash = Mix(hash, (frozen.Frozen ? 1u : 0u) | (frozen.TotallyFrozen ? 2u : 0u)); }
            hash = Mix(hash, actor.FoilBuddha ? 1u : 0u);
            hash = Mix(hash, actor.ForcePain ? 1u : 0u);
            hash = Mix(hash, actor.NoPain ? 1u : 0u);
            hash = Mix(hash, actor.Painless ? 1u : 0u);
            hash = Mix(hash, actor.ExtremeDeath ? 1u : 0u);
            hash = Mix(hash, actor.NoExtremeDeath ? 1u : 0u);
            if (actor.CameraHeight != Actor.DefaultCameraHeight)
            { hash = Mix(hash, 0x43414d48u); hash = MixDouble(hash, actor.CameraHeight); }
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
            if (actor.FullVolumeDeath) hash = Mix(hash, 0x46564454u);
            if (actor.FullVolumeSee) hash = Mix(hash, 0x46565345u);
            if (actor.OnlySlamSolid) hash = Mix(hash, 0x4f534c4du);
            if (actor.CausePain) hash = Mix(hash, 0x43504149u);
            if (actor.AllowPain) hash = Mix(hash, 0x41504149u);
            if (actor.NoDamage) hash = Mix(hash, 0x4e444d47u);
            if (actor.LaxTelefragDamage) hash = Mix(hash, 0x4c544644u);
            if (actor.RetargetAfterSlam) hash = Mix(hash, 0x52534c4du);
            if (actor.FullVolumeActive) hash = Mix(hash, 0x46564143u);
            if (actor.DeathDamageAmount != 0)
            {
                hash = Mix(hash, 0x4444414Du); hash = Mix(hash, unchecked((uint)actor.DeathDamageAmount));
            }
            if (actor is PlayerPawn { LastDamageType: { } lastDamage })
            {
                hash = Mix(hash, 0x4C445459u); hash = Mix(hash, (uint)lastDamage.Length);
                foreach (var character in lastDamage) hash = Mix(hash, character);
            }
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
            if (actor.NoSector) hash = Mix(hash, 0x4E534543u);
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
            if (actor is PlayerPawn { NoTargetCheatOverride: { } cheat })
            { hash = Mix(hash, 0x504e5443u); hash = Mix(hash, cheat ? 1u : 0u); }
            hash = Mix(hash, actor.OnMobj ? 1u : 0u);
            hash = Mix(hash, actor.IsMonster ? 1u : 0u);
            if (actor.UseSpecial) hash = Mix(hash, 0x55535043u);
            if (actor is not ProjectileActor && actor.TracerId is { } tracerId)
            {
                hash = Mix(hash, 0x54524352u);
                hash = Mix(hash, tracerId);
            }
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
            if (actor.SpecialPickup != PickupCatalog.IsPickup(actor.DoomEdNum, RavenHealthPickups))
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
            if (actor.SelfDamageFactor.Raw != 65536)
            {
                hash = Mix(hash, 0x53444641u);
                hash = Mix(hash, unchecked((uint)actor.SelfDamageFactor.Raw));
            }
            hash = actor.MixDamageFactorChecksum(hash);
            var soundEntries = actor.ActorSounds.Where(pair => actor is not ProjectileActor || pair.Value.Length != 0).ToArray();
            if (soundEntries.Length != 0)
            {
                hash = Mix(hash, 0x41534E44u);
                hash = Mix(hash, (uint)soundEntries.Length);
                foreach (var (type, name) in soundEntries.OrderBy(pair => pair.Key))
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
            if (actor.NoBlood) hash = Mix(hash, 0x4e424c44u);
            if (actor.PuffGetsOwner) hash = Mix(hash, 0x50474f57u);
            if (actor.PendingSpawnDeath) hash = Mix(hash, 0x53504454u);
            if (actor.IsPuff) hash = Mix(hash, 0x49505546u);
            if (actor.CantSeek) hash = Mix(hash, 0x43534545u);
            if (actor.SpawnSoundSource) hash = Mix(hash, 0x5353504Eu);
            if (actor.MoveWithSector) hash = Mix(hash, 0x4D575343u);
            if (actor.DontOverlap) hash = Mix(hash, 0x444F564Cu);
            if (actor.ExplodeCount) hash = Mix(hash, 0x45585043u);
            if (actor is BotPawn && actor.StatNum != ThinkerStat.Default)
            { hash = Mix(hash, 0x42535441u); hash = Mix(hash, (uint)actor.StatNum); }
            if (actor.SkyExplode) hash = Mix(hash, 0x534B5945u);
            if (actor.HitTarget || actor.HitMaster || actor.HitTracer)
            {
                hash = Mix(hash, 0x48495046u);
                hash = Mix(hash, (actor.HitTarget ? 1u : 0) | (actor.HitMaster ? 2u : 0) | (actor.HitTracer ? 4u : 0));
            }
            if (actor.CanPass != actor.DefaultCanPass) { hash = Mix(hash, 0x43415053u); hash = Mix(hash, actor.CanPass ? 1u : 0u); }
            if (actor.NoLiftDrop) hash = Mix(hash, 0x4E4C4450u);
            if (actor.RelativeToFloor) hash = Mix(hash, 0x52464C52u);
            if (actor.DontThrust) hash = Mix(hash, 0x44544852u);
            if (actor.RenderStyle != ActorRenderStyle.Normal || actor.Alpha != 1)
            {
                hash = Mix(hash, 0x52454E44u);
                hash = Mix(hash, SimActorRenderArchive.Pack(actor.RenderStyle));
                var alphaBits = (ulong)BitConverter.DoubleToInt64Bits(actor.Alpha == 0 ? 0 : actor.Alpha);
                hash = Mix(hash, (uint)alphaBits);
                hash = Mix(hash, (uint)(alphaBits >> 32));
            }
            if (actor.DontSeekInvisible) hash = Mix(hash, 0x4453494Eu);
            if (actor.Shadow) hash = Mix(hash, 0x53484144u);
            if (actor.DefaultShadow) hash = Mix(hash, 0x53484446u);
            if (actor.MinVisible) hash = Mix(hash, 0x4D564953u);
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
                if (player.WeaponRefiring) hash = Mix(hash, 0x57524652u);
                var input = player.CapturePlayerInput();
                if (!input.IsZero)
                {
                    hash = Mix(hash, 0x50494E50u);
                    hash = Mix(hash, unchecked((uint)input.OriginalButtons));
                    hash = Mix(hash, unchecked((uint)input.ModifiedButtons));
                    hash = Mix(hash, unchecked((uint)input.OriginalOldButtons));
                    hash = Mix(hash, unchecked((uint)input.ModifiedOldButtons));
                    hash = Mix(hash, unchecked((uint)input.OriginalForward));
                    hash = Mix(hash, unchecked((uint)input.ModifiedForward));
                    hash = Mix(hash, unchecked((uint)input.OriginalSide));
                    hash = Mix(hash, unchecked((uint)input.ModifiedSide));
                    hash = Mix(hash, unchecked((uint)input.OriginalYaw));
                    hash = Mix(hash, unchecked((uint)input.ModifiedYaw));
                }
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
                if (player.DamageCount != 0) { hash = Mix(hash, 0x44434e54u); hash = Mix(hash, (uint)player.DamageCount); }
                foreach (var item in player.Inventory.HealthPickups)
                {
                    hash = Mix(hash, 0x48504943u); hash = Mix(hash, (uint)item.Health); hash = Mix(hash, (uint)item.Amount); hash = Mix(hash, (uint)item.AutoUseMode);
                    if (item.TypeName is { } healthType)
                    {
                        hash = Mix(hash, 0x48545950u); hash = Mix(hash, (uint)healthType.Length);
                        foreach (var character in healthType) hash = Mix(hash, character);
                    }
                }
                if (player.PoisonCount != 0) { hash = Mix(hash, 0x50434e54u); hash = Mix(hash, (uint)player.PoisonCount); }
                if (player.PoisonSourceId is { } poisonSource) { hash = Mix(hash, 0x50535243u); hash = Mix(hash, poisonSource); }
                if (player.PoisonDamageType is { } poisonType) { hash = Mix(hash, 0x50445459u); hash = Mix(hash, (uint)poisonType.Length); foreach (var character in poisonType) hash = Mix(hash, character); }
                if (player.SoundClass is { } soundClass)
                {
                    hash = Mix(hash, 0x53434c53u); hash = Mix(hash, (uint)soundClass.Length);
                    foreach (var character in soundClass) hash = Mix(hash, character);
                }
                if (player.PoisonPainType is { } poisonPain) { hash = Mix(hash, 0x50505459u); hash = Mix(hash, (uint)poisonPain.Length); foreach (var character in poisonPain) hash = Mix(hash, character); }
                if (player.GodMode2) hash = Mix(hash, 0x474f4432u);
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
                if (player.GruntSpeed != PlayerPawn.DefaultGruntSpeed)
                {
                    hash = Mix(hash, 0x47525350u);
                    var gruntBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(player.GruntSpeed));
                    hash = Mix(hash, (uint)gruntBits); hash = Mix(hash, (uint)(gruntBits >> 32));
                }
                if (player.DeltaViewHeight != 0)
                {
                    hash = Mix(hash, 0x44564854u);
                    var deltaBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(player.DeltaViewHeight));
                    hash = Mix(hash, (uint)deltaBits); hash = Mix(hash, (uint)(deltaBits >> 32));
                }
                if (player.JumpTics != 0)
                {
                    hash = Mix(hash, 0x4A554D50u);
                    hash = Mix(hash, unchecked((uint)player.JumpTics));
                }
                hash = Mix(hash, player.UncrouchLocked ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)player.WeaponOffsetY));
                hash = Mix(hash, player.WeaponLowering ? 1u : 0u);
                hash = Mix(hash, player.InstantWeaponSwitch ? 1u : 0u);
                if (player.AttackPressed) hash = Mix(hash, 0x41544b50u);
                if (player.BonusCount != 0)
                { hash = Mix(hash, 0x424f4e55u); hash = Mix(hash, unchecked((uint)player.BonusCount)); }
                if (player.HasPowerStrength)
                { hash = Mix(hash, 0x50535452u); hash = Mix(hash, unchecked((uint)player.PowerStrengthTics)); }
                if (player.ExtraLight != 0)
                {
                    hash = Mix(hash, 0x4558544cu);
                    hash = Mix(hash, unchecked((uint)player.ExtraLight));
                }
                hash = Mix(hash, (uint)player.TurnTicks);
                hash = Mix(hash, player.TurnHeld ? 1u : 0u);
                hash = Mix(hash, player.Inventory.Pending is { } pending ? (uint)pending : 0u);
                if (player.Inventory.Pending == (WeaponKind)0) hash = Mix(hash, 0x57504e30u);
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
                if (player.PowerIronFeetMode != IronFeetMode.Normal)
                { hash = Mix(hash, 0x5049464du); hash = Mix(hash, (uint)player.PowerIronFeetMode); }
                if (player.PowerMaskTics != 0)
                { hash = Mix(hash, 0x504d534bu); hash = Mix(hash, unchecked((uint)player.PowerMaskTics)); }
                if (player.PowerIronFeetTics != 0)
                { hash = Mix(hash, 0x50494654u); hash = Mix(hash, unchecked((uint)player.PowerIronFeetTics)); }
                if (player.TeleportFreezeTime != 18)
                { hash = Mix(hash, 0x5446525au); hash = Mix(hash, unchecked((uint)player.TeleportFreezeTime)); }
                if (player.PowerInfiniteAmmoTics != 0)
                { hash = Mix(hash, 0x5049414du); hash = Mix(hash, unchecked((uint)player.PowerInfiniteAmmoTics)); }
                if (player.PowerDoubleFiringSpeedTics != 0)
                { hash = Mix(hash, 0x50444653u); hash = Mix(hash, unchecked((uint)player.PowerDoubleFiringSpeedTics)); }
                if (player.PowerHighJumpStrength != 2)
                {
                    var strengthBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(player.PowerHighJumpStrength));
                    hash = Mix(hash, 0x484a5354u); hash = Mix(hash, (uint)strengthBits); hash = Mix(hash, (uint)(strengthBits >> 32));
                }
                if (player.PowerHighJumpPendingRemoval) hash = Mix(hash, 0x484a5052u);
                if (player.PowerHighJumpTics != 0)
                { hash = Mix(hash, 0x50484a50u); hash = Mix(hash, unchecked((uint)player.PowerHighJumpTics)); }
                if (player.PowerRegenerationStrength != 5)
                {
                    var regenerationBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(player.PowerRegenerationStrength));
                    hash = Mix(hash, 0x50525354u); hash = Mix(hash, (uint)regenerationBits); hash = Mix(hash, (uint)(regenerationBits >> 32));
                }
                if (player.PowerProsperityTics != 0)
                { hash = Mix(hash, 0x5050524fu); hash = Mix(hash, unchecked((uint)player.PowerProsperityTics)); }
                if (player.PowerSpreadTics != 0)
                { hash = Mix(hash, 0x50535052u); hash = Mix(hash, unchecked((uint)player.PowerSpreadTics)); }
                if (player.PowerRegenerationTics != 0)
                { hash = Mix(hash, 0x50524547u); hash = Mix(hash, unchecked((uint)player.PowerRegenerationTics)); }
                if (player.PowerDrainStrength != 0.5)
                {
                    var drainBits = unchecked((ulong)BitConverter.DoubleToInt64Bits(player.PowerDrainStrength));
                    hash = Mix(hash, 0x50445354u); hash = Mix(hash, (uint)drainBits); hash = Mix(hash, (uint)(drainBits >> 32));
                }
                if (player.PowerDrainTics != 0)
                { hash = Mix(hash, 0x5044524eu); hash = Mix(hash, unchecked((uint)player.PowerDrainTics)); }
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
                hash = Mix(hash, projectile.Owner?.Id ?? 0);
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
            if (line.Impact)
            { hash = Mix(hash, 0x494d5043u); hash = Mix(hash, unchecked((uint)line.Arg1)); }
            if (line.PlayerCross || line.Repeat)
            { hash = Mix(hash, 0x4c414354u); hash = Mix(hash, (line.PlayerCross ? 1u : 0u) | (line.Repeat ? 2u : 0u)); }
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
            if (sector.NoFallingDamage) hash = Mix(hash, 0x4e46444du);
            if (sector.NoAttack) hash = Mix(hash, 0x4E4F4154u);
            if (sector.IsLift) hash = Mix(hash, 0x4c494654u);
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

        if (Invasion.CaptureState() is { } invasion)
        {
            hash = Mix(hash, 0x494e5653u);
            foreach (var value in new[] { invasion.Enabled ? 1 : 0, (int)invasion.Phase, invasion.Wave,
                invasion.Cooldown, invasion.Spawned, invasion.ActiveMonsters, invasion.MaxWaves,
                invasion.CountdownTics, invasion.IntermissionTics, invasion.SpawnSpotCount, invasion.WaveActors.Count })
                hash = Mix(hash, (uint)value);
            foreach (var id in invasion.WaveActors) hash = Mix(hash, id);
        }

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
