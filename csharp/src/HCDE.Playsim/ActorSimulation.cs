using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim;

public readonly struct PlayerCommand
{
    public short ForwardMove { get; init; }
    public short SideMove { get; init; }
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
    /// <summary>Native <c>GetNetworkID</c>. Assigned by netplay; ACS may query it offline.</summary>
    public uint NetworkId { get; set; }
    public int DoomEdNum { get; init; }
    internal int DefinitionDoomEdNum { get; set; }
    internal int ClassDoomEdNum => DefinitionDoomEdNum > 0 ? DefinitionDoomEdNum : DoomEdNum;
    public int ThingId { get; internal set; }
    /// <summary>Native <c>AActor::IsMapActor</c>. Owned inventory items are excluded from ACS thing counts.</summary>
    internal virtual bool IsMapActor => true;
    public bool NoBlockmap { get; set; }
    internal virtual bool IsBlockmapActor => !NoBlockmap;
    /// <summary>Native actor gravity multiplier; ACS reads/writes signed 16.16 values.</summary>
    public Fixed Gravity { get; set; } = Fixed.FromInt(1);
    /// <summary>Native DamageFactor, applied to incoming ordinary damage before armor.</summary>
    public Fixed DamageFactor { get; set; } = Fixed.FromInt(1);
    /// <summary>Native DamageMultiply, applied to this source's outgoing ordinary damage.</summary>
    public Fixed DamageMultiplier { get; set; } = Fixed.FromInt(1);
    /// <summary>Native MeleeRange default: 64 minus MELEEDELTA (20).</summary>
    public Fixed MeleeRange { get; set; } = Fixed.FromInt(44);
    /// <summary>Native actor friction multiplier, applied to surface friction.</summary>
    public Fixed Friction { get; set; } = Fixed.FromInt(1);
    /// <summary>Native MF6_NOTRIGGER: suppress automatic movement line triggers.</summary>
    public bool NoTrigger { get; set; }
    /// <summary>Native actor Score property; independent of player frag statistics.</summary>
    public int Score { get; set; }
    /// <summary>Native DamageVal; scripted damage functions are not supported.</summary>
    public int Damage { get; set; }
    /// <summary>Native MF_DROPPED; independent of ammo skill handling and pickup amount.</summary>
    public bool Dropped { get; set; }
    private int _reactionTime;
    private bool _reactionTimeInitialized;
    internal bool ReactionTimeInitialized => _reactionTimeInitialized;
    /// <summary>Native actor reaction counter; attached managed brains use this same state.</summary>
    public int ReactionTime
    {
        get => _reactionTime;
        set { _reactionTime = value; _reactionTimeInitialized = true; }
    }
    /// <summary>Native <c>TIDtoHate</c>. Teammates share this value; a shooter may hurt or wake actors whose <see cref="ThingId"/> matches.</summary>
    public int TidToHate { get; set; }
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
    /// <summary>Native MF_PICKUP. Allows movement contact to collect inventory.</summary>
    public bool CanPickupItems { get; set; }
    /// <summary>Native MF_SPECIAL. Enables this item's movement-contact pickup.</summary>
    public bool SpecialPickup { get; set; }
    internal bool SpawnCanPickupItems { get; set; }
    internal bool SpawnSpecialPickup { get; set; }
    public bool Floating { get; set; }
    public double FloatSpeed { get; set; } = 4;
    public bool NoRadiusDamage { get; set; }
    public bool NoSectorDamage { get; set; }
    public bool ForceSectorDamage { get; set; }
    public bool Ambush { get; set; }
    public int PainChance { get; set; } = 256;
    /// <summary>Native <c>PainThreshold</c>. A surviving hit below this post-armor amount does not flinch. 0 flinches on any loss.</summary>
    public int PainThreshold { get; set; }
    public int ResurrectionHealth { get; internal set; }
    public int RaiseDuration { get; internal set; }
    public int Mass { get; set; } = 100;
    /// <summary>Native actor Speed in ACS signed 16.16 units.</summary>
    public Fixed MovementSpeed { get; set; } = Fixed.FromInt(4);
    public double ChaseSpeed { get => MovementSpeed.ToDouble() / 4; set => MovementSpeed = Fixed.FromDouble(value * 4); }
    public bool Solid { get; set; } = true;
    public bool Shootable { get; set; } = true;
    public bool Invulnerable { get; set; }
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
    /// <summary>Native <c>MF4_NOTARGETSWITCH</c>. Wake-up will not pick a new chase target while one is alive.</summary>
    public bool NoTargetSwitch { get; set; }
    /// <summary>Native <c>MF4_NOHATEPLAYERS</c>. <see cref="OkayToSwitchTarget"/> ignores player sources.</summary>
    public bool NoHatePlayers { get; set; }
    /// <summary>Native <c>MF4_QUICKTORETALIATE</c>. Wake-up may switch targets while chase threshold is still positive.</summary>
    public bool QuickToRetaliate { get; set; }
    /// <summary>Native <c>MF_FRIENDLY</c>. Used with <see cref="FriendPlayer"/> for <see cref="IsFriend"/>.</summary>
    public bool Friendly { get; set; }
    /// <summary>Native <c>FriendPlayer</c>. 0 means any friendly. 1 is the first player.</summary>
    public int FriendPlayer { get; set; }
    /// <summary>Native <c>MF5_NOINFIGHTING</c>. Wake-up treats infighting as off for this actor.</summary>
    public bool NoInfighting { get; set; }
    /// <summary>Native <c>MF7_FORCEINFIGHTING</c>. Standard infighting applies when the level is set to none.</summary>
    public bool ForceInfighting { get; set; }
    /// <summary>Native <c>MF6_DOHARMSPECIES</c>. Same-species projectile immunity does not apply.</summary>
    public bool DoHarmSpecies { get; set; }
    /// <summary>Native <c>IsFriend</c> subset. Deathmatch teamplay and designated teams are absent.</summary>
    public bool IsFriend(Actor other)
    {
        if (!Friendly || !other.Friendly) return false;
        if (FriendPlayer == 0 || other.FriendPlayer == 0) return true;
        return FriendPlayer == other.FriendPlayer;
    }
    /// <summary>Native <c>IsHostile</c> subset. Two plain monsters are never hostile to each other.</summary>
    public bool IsHostile(Actor other)
    {
        if (!Friendly && !other.Friendly) return false;
        if ((Friendly && other.Friendly))
        {
            return FriendPlayer != 0 && other.FriendPlayer != 0 && FriendPlayer != other.FriendPlayer;
        }
        return true;
    }
    /// <summary>Native species subset for default <c>P_ProjectileImmune</c>. Uses resolved class identity and vanilla monster ancestry.</summary>
    public bool IsSameSpecies(Actor other) =>
        DefaultSpecies == other.DefaultSpecies && this is not PlayerPawn && other is not PlayerPawn;
    // GetSpecies climbs monster parents: Spectre : Demon and HellKnight : BaronOfHell.
    private int DefaultSpecies => ClassDoomEdNum switch { 58 => 3002, 69 => 3003, _ => ClassDoomEdNum };
    /// <summary>Native <c>P_ProjectileImmune</c> default-group subset. Projectile groups are absent.</summary>
    public bool ProjectileImmune(Actor source) =>
        IsSameSpecies(source) && !DoHarmSpecies;
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

    /// <summary>Native <c>OkayToSwitchTarget</c> subset. Master/minion and infighting groups are absent.</summary>
    internal bool OkayToSwitchTarget(Actor other, MonsterBrain brain)
    {
        if (!other.CanTakeDamage || other.Id == Id || other.NeverTarget)
            return false;
        if (NoTargetSwitch && brain.TargetId != null)
            return false;
        if (other.NoTarget && (other.ThingId != TidToHate || TidToHate == 0) && !IsHostile(other))
            return false;
        if (brain.Threshold > 0 && !QuickToRetaliate)
            return false;
        if (IsFriend(other))
            return false;
        var sim = Simulation;
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
            if (current != null && current.CanTakeDamage && current.ThingId == TidToHate
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
        set
        {
            var dead = _health <= 0;
            _health = value;
            if (!dead && IsDead)
            {
                DeathCount++;
                var death = ChooseDeathState(out var extremeDeath);
                if (extremeDeath && this is not PlayerPawn && _health >= GibHealth)
                    _health = GibHealth - 1;
                if (States.HasState(death)) States.Enter(this, death);
                if (this is PlayerPawn player)
                {
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
    }
    public ActorStateMachine States { get; } = new();
    public int SpawnState { get; set; } = ActorStateMachine.Spawn;
    public int ActiveState { get; set; } = -1;
    public int InactiveState { get; set; } = -1;
    /// <summary>Native See state. A waking monster in <see cref="SpawnState"/> can enter this frame. -1 means absent.</summary>
    public int SeeState { get; set; } = -1;
    public int PainState { get; set; } = ActorStateMachine.Pain;
    public int DeathState { get; set; } = ActorStateMachine.Death;
    /// <summary>Optional Death.Extreme state. Absent until a table actually contains it.</summary>
    public int ExtremeDeathState { get; set; } = -1;
    /// <summary>Native GenericFreezeDeath. An Ice kill uses this when no Ice death is registered. -1 means absent.</summary>
    public int GenericFreezeDeath { get; set; } = -1;
    /// <summary>Native <c>MF4_NOICEDEATH</c>. An Ice kill does not fall back to <see cref="GenericFreezeDeath"/>.</summary>
    public bool NoIceDeath { get; set; }
    /// <summary>Damage type of the hit currently being applied. The setter consumes it.</summary>
    internal string? DamageTypeReceived { get; set; }
    /// <summary>Inflictor for the hit currently being applied. The health setter consumes it.</summary>
    internal Actor? DeathInflictor { get; set; }
    private readonly Dictionary<string, int> _typedDeaths = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _typedExtremeDeaths = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _typedPain = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _typedPainChance = new(StringComparer.Ordinal);
    private readonly Dictionary<string, int> _typedWounds = new(StringComparer.Ordinal);
    /// <summary>Native <c>WoundHealth</c>. A surviving hit at or below this health can enter a typed wound frame.</summary>
    public int WoundHealth { get; set; }
    /// <summary>Native GetGibHealth. A killing blow below this value can enter <see cref="ExtremeDeathState"/>.</summary>
    public int GibHealth { get; set; } = -100;
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
    /// <summary>Dropped weapons skip the skill ammo factor, matching <c>bIgnoreSkill</c>.</summary>
    public bool IgnoreAmmoSkill { get; set; }
    /// <summary>Native <c>BackpackItem.bDepleted</c>. A tossed backpack raises caps and gives no ammo.</summary>
    public bool Depleted { get; set; }
    /// <summary>ACS <c>DropItem</c> amount override. Zero uses the catalog default.</summary>
    public int PickupAmount { get; set; }
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

    public bool BlocksActors =>
        Solid && !IsDead && !Destroyed && DoomEdNum != LineSpecials.TeleportDestType
        && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public bool CanTakeDamage => Shootable && !IsDead && !Destroyed
        && DoomEdNum != LineSpecials.TeleportDestType && DoomEdNum != InvasionDirector.SpawnSpotType
        && !PickupCatalog.IsPickup(DoomEdNum);

    public static bool IsPlayerStart(int type) => type is >= PlayerStartMin and <= PlayerStartMax;

    /// <summary>
    /// <c>Death.Fire</c> or <c>Death.Extreme.Fire</c>. The name match is ordinal.
    /// A missing frame falls through. An Ice kill can still use <see cref="GenericFreezeDeath"/>.
    /// </summary>
    public void SetTypedDeath(string damageType, int state, bool extreme = false)
    {
        if (string.IsNullOrEmpty(damageType) || damageType is "None" or "Extreme")
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

    /// <summary>
    /// Native death-state order. An extreme typed frame wins, then the plain typed
    /// frame, then generic ice freeze, then <see cref="ExtremeDeathState"/>, then <see cref="DeathState"/>.
    /// A plain typed frame is used even when health is past the gib line.
    /// Damage type <c>Extreme</c> forces the gib state and then clears the type.
    /// Ice freeze is not extreme. It applies to a player or a monster, and <see cref="NoIceDeath"/> blocks it.
    /// Inflictor <see cref="ExtremeDeath"/> and <see cref="NoExtremeDeath"/> follow native <c>MF4_*</c> on the inflictor only.
    /// </summary>
    private int ChooseDeathState(out bool usedExtremeDeath)
    {
        usedExtremeDeath = false;
        var type = DamageTypeReceived;
        var inflictor = DeathInflictor;
        var extreme = (_health < GibHealth || inflictor is { ExtremeDeath: true })
            && inflictor is not { NoExtremeDeath: true };
        if (string.Equals(type, "Extreme", StringComparison.Ordinal))
        {
            extreme = true;
            type = null;
        }
        if (!string.IsNullOrEmpty(type) && !string.Equals(type, "None", StringComparison.Ordinal))
        {
            if (extreme && _typedExtremeDeaths.TryGetValue(type, out var extremeState) && States.HasState(extremeState))
            {
                usedExtremeDeath = true;
                return extremeState;
            }
            if (_typedDeaths.TryGetValue(type, out var typed) && States.HasState(typed))
                return typed;
            if (string.Equals(type, "Ice", StringComparison.Ordinal) && !NoIceDeath
                && (this is PlayerPawn || Brain != null) && States.HasState(GenericFreezeDeath))
                return GenericFreezeDeath;
        }
        if (extreme && States.HasState(ExtremeDeathState))
        {
            usedExtremeDeath = true;
            return ExtremeDeathState;
        }
        return DeathState;
    }

    /// <summary>
    /// <c>Pain.Fire</c>. The name match is ordinal. A missing frame uses <see cref="PainState"/>.
    /// <paramref name="chance"/> replaces <see cref="PainChance"/> for that type only.
    /// Electric flicker uses <see cref="AuthoritySimulation.NextFlickerRandom"/>. Poison howling is absent.
    /// </summary>
    /// <summary><c>Wound.Fire</c>. The name match is ordinal. A missing frame is ignored.</summary>
    public void SetTypedWound(string damageType, int state)
    {
        if (string.IsNullOrEmpty(damageType) || damageType == "None")
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
            && !string.Equals(damageType, "None", StringComparison.Ordinal)
            && _typedWounds.TryGetValue(damageType, out var state)
            && States.HasState(state))
            return state;
        return -1;
    }

    public void SetTypedPain(string damageType, int state, int? chance = null)
    {
        if (string.IsNullOrEmpty(damageType) || damageType == "None")
            throw new ArgumentException("A typed pain needs a damage type other than None.", nameof(damageType));
        _typedPain[damageType] = state;
        if (chance is { } value)
            _typedPainChance[damageType] = value;
    }

    internal int PainStateFor(string? damageType)
    {
        if (!string.IsNullOrEmpty(damageType)
            && !string.Equals(damageType, "None", StringComparison.Ordinal)
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

    public override void Tick()
    {
        RememberPosition();
        States.Tick(this);
        if (!Destroyed && Simulation != null)
        {
            if (!Dormant) Brain?.Tick(Simulation, this);
            TickMovement(Simulation);
            SectorDamage.Tick(Simulation, this);
        }
        base.Tick();
    }

    protected virtual void TickMovement(AuthoritySimulation sim) => ActorPhysics.Step(sim, this);
}

public sealed class PlayerPawn : Actor
{
    public PlayerPawn()
    {
        CanSlide = true;
        MovementSpeed = Fixed.FromInt(1);
        CanPickupItems = true;
        SpawnCanPickupItems = true;
        AllowDropOff = true;
    }

    public const double CrouchSpeed = 1.0 / 12;
    public const double MinimumCrouchFactor = 0.5;
    public const double StandingViewHeight = 41;
    public Fixed JumpZ { get; set; } = Fixed.FromInt(8);
    /// <summary>Native DeathThink leaves the view here.</summary>
    public const double DeathViewHeight = 6;
    public const double DeathPitchStep = 3;
    public const double DeathTurnStep = 5;
    /// <summary>Native <c>P_GiveBody</c> cap for drain. Health already above this is left alone.</summary>
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
    public bool WeaponReady => WeaponOffsetY == WeaponTop && !WeaponLowering;

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
    /// <summary>Native <c>player_t::fragcount</c> for ACS <c>PlayerFrags</c>.</summary>
    public int FragCount { get; set; }

    /// <summary>
    /// Grants <c>PowerBuddha</c>. Above <see cref="PowerBuddhaBlinkThreshold"/> the new
    /// item is taken and the timer stays. At or below that line it resets to
    /// <see cref="PowerBuddhaDuration"/>.
    /// </summary>
    public void GivePowerBuddha()
    {
        if (PowerBuddhaTics > PowerBuddhaBlinkThreshold)
            return;
        if (PowerBuddhaDuration > PowerBuddhaTics)
            PowerBuddhaTics = PowerBuddhaDuration;
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
    public PlayerInventory Inventory { get; } = new();
    public bool GodMode { get; set; }
    public bool AttackPressed { get; set; }
    public bool UsePressed { get; set; }
    public bool UseHeld { get; internal set; }
    /// <summary>Native Player.UseRange. The use trace follows yaw for this distance.</summary>
    public double UseRange { get; set; } = 64;
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
        if (direction > 0 && CrouchFactor < 1)
            CrouchMove(1);
        else if (direction < 0 && CrouchFactor > MinimumCrouchFactor)
            CrouchMove(-1);
        // Leave an externally chosen height alone while the player is not crouching.
        if (Math.Abs(CrouchFactor - before) > 1e-12)
            Height = Fixed.FromDouble(FullHeight * CrouchFactor);
        ViewHeight = DefaultViewHeight.ToDouble() * CrouchFactor;
    }

    private void CrouchMove(int direction)
    {
        var next = Math.Clamp(CrouchFactor + direction * CrouchSpeed, MinimumCrouchFactor, 1);
        if (next >= CrouchFactor && Simulation != null
            && !ActorPhysics.FitsAtHeight(Simulation, this, FullHeight * next))
            return;
        CrouchFactor = next;
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
        if (PowerBuddhaTics > 0) PowerBuddhaTics--;
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
                VelocityX = Fixed.FromDouble(VelocityX.ToDouble() + dx * MovementSpeed.ToDouble());
                VelocityY = Fixed.FromDouble(VelocityY.ToDouble() + dy * MovementSpeed.ToDouble());
            }
            // CheckJump runs before CheckCrouch. A jump while crouched only stands the player up.
            var crouched = CrouchFactor < 1;
            if (command.Jump && crouched)
                UncrouchLocked = true;
            else if (command.Jump && OnGround && Level != null)
            {
                VelocityZ = JumpZ;
                OnGround = false;
            }
        }
        ApplyCrouch(command);

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
    private void AdvanceWeapon()
    {
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
        DehackedPatchResult? dehacked = null, SpawnOptions? spawnOptions = null)
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
                    Health = defaults?.Health ?? 100,
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

            if (actor is PlayerPawn player)
                player.FullHeight = player.Height.ToDouble();
            if (!playerStart)
            {
                actor.PitchDegrees = thing.Pitch;
                actor.Roll = BamAngle.FromDegrees(thing.Roll);
            }
            actor.DefinitionDoomEdNum = definitionType;
            nextId++;
            actor.SpecialPickup = PickupCatalog.IsPickup(actor.DoomEdNum);
            ApplyPrimaryFlags(actor, defaults, playerStart);
            actor.SpawnCanPickupItems = actor.CanPickupItems;
            actor.SpawnSpecialPickup = actor.SpecialPickup;
            actor.ResurrectionHealth = actor.Health;
            if (actor.ResurrectionHealth > 0) actor.GibHealth = -actor.ResurrectionHealth;
            actor.Mass = defaults is { MassPatched: true } ? defaults.Mass : DoomActorCatalog.MassOf(definitionType);
            actor.RaiseDuration = ArchvileActions.RaiseDuration(definitionType);
            ApplyExtendedDefaults(actor, defaults);
            if (thing.Gravity < 0) actor.Gravity = Fixed.FromDouble(-thing.Gravity);
            else if (thing.Gravity > 0) actor.Gravity = Fixed.FromDouble(actor.Gravity.ToDouble() * thing.Gravity);
            else { actor.Gravity = default; actor.NoGravity = true; }
            actor.Ambush = thing.Ambush || defaults is { BitsPatched: true } && (defaults.Bits & 0x00000020) != 0;
            if (defaults is { ReactionTimePatched: true }) actor.ReactionTime = defaults.ReactionTime;
            actor.Brain = MonsterBrain.ForType(definitionType);
            if (!playerStart) actor.Damage = definition?.Damage ?? 0;
            if (defaults is { MissileDamagePatched: true }) actor.Damage = defaults.MissileDamage;
            actor.IsMonster = !playerStart && (defaults is { BitsPatched: true }
                ? (defaults.Bits & 0x00400000) != 0
                : actor.Brain != null);
            if (actor.IsMonster && Math.Clamp(spawnOptions?.Skill ?? 2, 0, 4) == 4) actor.ReactionTime = 0;
            ThingActivation.InitializeSpawn(actor, thing.Dormant);
            actor.RememberPosition();
            thinkers.Add(actor, playerStart ? ThinkerStat.Player : ThinkerStat.Default);
            actors.Add(actor);
        }

        return actors;
    }

    internal static void ApplyPrimaryFlags(Actor actor, DehackedActor? defaults, bool playerStart)
    {
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
            actor.NoBlockMonsters = defaults.NoBlockMonsters;
        }
    }

    internal static void ApplyExtendedDefaults(Actor actor, DehackedActor? defaults)
    {
        if (defaults is { Bits2Patched: true })
        {
            actor.NoTeleport = (defaults.Bits2 & 0x80) != 0;
            actor.CanSlide = (defaults.Bits2 & 0x400) != 0;
            actor.Invulnerable = (defaults.Bits2 & 0x08000000) != 0;
            actor.Dormant = (defaults.Bits2 & 0x10000000) != 0;
        }
        if (defaults is { GravityPatched: true }) actor.Gravity = Fixed.FromDouble(defaults.Gravity);
    }

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
    private readonly List<Actor> _actors;
    private uint _nextActorId;
    public uint CombatRandomState { get; private set; }
    /// <summary>Native <c>pr_switcher</c> for <see cref="Actor.OkayToSwitchTarget"/> hate stickiness.</summary>
    public uint SwitchTargetRandomState { get; private set; }
    /// <summary>Rolls for a deathmatch respawn. Not the native <c>DMSpawn</c> table, and not in the save pose.</summary>
    public uint DmSpawnRandomState { get; set; }
    private readonly DehackedPatchResult? _dehacked;
    private uint _uniqueTidRandomState;
    private uint _strobeRandomState;
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
        Thinkers = thinkers;
        _actors = actors;
        _nextActorId = actors.Count == 0 ? 1 : checked(actors.Max(actor => actor.Id) + 1);
        RngSeed = rngSeed;
        CombatRandomState = unchecked((uint)rngSeed) ^ 0x9e3779b9u;
        SwitchTargetRandomState = unchecked((uint)rngSeed) ^ 0x73776974u;
        DmSpawnRandomState = unchecked((uint)rngSeed) ^ 0x646d7370u;
        _uniqueTidRandomState = unchecked((uint)rngSeed) ^ 0x756e6974u;
        _strobeRandomState = unchecked((uint)rngSeed) ^ 0x7374726fu;
        _flickerRandomState = unchecked((uint)rngSeed) ^ 0x666c6963u;
        _lightFlashRandomState = unchecked((uint)rngSeed) ^ 0x666c6173u;
        _fireFlickerRandomState = unchecked((uint)rngSeed) ^ 0x66697265u;
        Compat = compat;
        DamageExitAllowed = damageExitAllowed;
        GameMode = spawnOptions.Mode;
        Skill = Math.Clamp(spawnOptions.Skill, 0, 4);
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
    /// <summary>Native <c>sv_weapondrop</c>. Off until set. Fist and pistol still drop nothing.</summary>
    public bool WeaponDrop { get; set; }
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
    /// <summary>Native <c>sv_ammofactor</c>. Multiplies the skill ammo factor. 1 leaves the skill value alone.</summary>
    public double AmmoFactor { get; set; } = 1;
    /// <summary>Native <c>sv_doubleammo</c>. Off until set. Replaces the skill factor with 2.</summary>
    public bool DoubleAmmo { get; set; }
    /// <summary>Native <c>sv_dropstyle</c>; 0 uses the Doom default, 2 selects the Strife toss.</summary>
    public int DropStyle { get; set; }
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
        var actors = ActorSpawner.Spawn(level, thinkers, dehacked, spawnOptions).ToList();
        return new AuthoritySimulation(level, thinkers, actors, rngSeed, compat,
            !noExit || spawnOptions.Mode != SpawnGameMode.Deathmatch, spawnOptions, dehacked);
    }

    /// <summary>
    /// <c>sv_weapondrop</c> copy of the ready weapon. The corpse keeps its inventory.
    /// The pickup uses the catalog amount, not the ammo the player was holding,
    /// and it ignores the skill ammo factor.
    /// </summary>
    internal void DropSelectedWeapon(PlayerPawn player)
    {
        if (!WeaponDrop || !PickupCatalog.TryWeaponEdNum(player.Inventory.Selected, out var type))
            return;
        SpawnDroppedPickup(player, type, ignoreAmmoSkill: true);
    }

    internal bool SpawnDroppedPickup(Actor dropper, int doomEdNum, bool ignoreAmmoSkill = false, int pickupAmount = 0)
    {
        if (dropper.Destroyed || !PickupCatalog.IsPickup(doomEdNum))
            return false;
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
            IgnoreAmmoSkill = ignoreAmmoSkill,
            Dropped = true,
            PickupAmount = ignoreAmmoSkill ? pickupAmount : PickupCatalog.DropPickupAmount(doomEdNum, pickupAmount),
            SpecialPickup = true,
            Height = PickupCatalog.HeightOf(doomEdNum),
        };
        drop.Simulation = this;
        ActorPhysics.PlaceOnFloor(this, drop);
        var toss = !Compat.HasFlag(CompatSurface.NoTossDrops);
        var strifeStyle = DropStyle == 2;
        drop.Z = Fixed.FromDouble(dropper.Z.ToDouble() + (toss ? strifeStyle ? 24 : dropper.Height.ToDouble() / 2 : 0));
        if (toss)
        {
            var mask = strifeStyle ? 7u : 255u;
            var divisor = strifeStyle ? 1.0 : 256.0;
            drop.VelocityX = Fixed.FromDouble(((int)(NextCombatRandom() & mask) - (int)(NextCombatRandom() & mask)) / divisor);
            drop.VelocityY = Fixed.FromDouble(((int)(NextCombatRandom() & mask) - (int)(NextCombatRandom() & mask)) / divisor);
            if (!strifeStyle) drop.VelocityZ = Fixed.FromDouble(5 + (NextCombatRandom() & 255) / 64.0);
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
    /// depleted, so picking it up restores the caps and gives no ammo. There is no
    /// drop-inventory command; this is the toss. A player who already has a pack
    /// still receives ammo from a depleted one, matching <c>HandlePickup</c>.
    /// </summary>
    public Actor? DropBackpack(PlayerPawn player)
    {
        if (!player.Inventory.RemoveBackpack())
            return null;
        var id = _nextActorId;
        _nextActorId = checked(_nextActorId + 1);
        var drop = new Actor
        {
            Id = id,
            DoomEdNum = PickupCatalog.Backpack,
            X = player.X,
            Y = player.Y,
            Level = Level,
            Solid = false,
            Shootable = false,
            Depleted = true,
            SpecialPickup = true,
            Height = PickupCatalog.HeightOf(PickupCatalog.Backpack),
        };
        drop.RememberPosition();
        drop.Simulation = this;
        ActorPhysics.PlaceOnFloor(this, drop);
        _actors.Add(drop);
        Thinkers.Add(drop, ThinkerStat.Default);
        return drop;
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
        bot.Mass = defaults is { MassPatched: true } ? defaults.Mass : DoomActorCatalog.MassOf(definitionType);
        if (defaults is { MissileDamagePatched: true }) bot.Damage = defaults.MissileDamage;
        ActorSpawner.ApplyPrimaryFlags(bot, defaults, playerStart: false);
        bot.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { BitsPatched: true }) bot.IsMonster = (defaults.Bits & 0x00400000) != 0;
        bot.SpawnCanPickupItems = bot.CanPickupItems;
        bot.SpawnSpecialPickup = bot.SpecialPickup;
        ActorSpawner.ApplyExtendedDefaults(bot, defaults);
        if (bot.IsMonster && Skill == 4) bot.ReactionTime = 0;
        ThingActivation.InitializeSpawn(bot, false);
        bot.RememberPosition();
        bot.Simulation = this;
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
    private double NextIceChunkVelocity() => ((int)(NextCombatRandom() >> 24) - (int)(NextCombatRandom() >> 24)) / 128.0;

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
        var jitter = (int)(NextCombatRandom() % jitterSpan);
        var spawnCount = Math.Max(24, numChunks + jitter);
        var baseX = corpse.X.ToDouble();
        var baseY = corpse.Y.ToDouble();
        var baseZ = corpse.Z.ToDouble();
        for (var i = spawnCount; i >= 0; i--)
        {
            var xo = ((int)(NextCombatRandom() % 256) - 128) * radius / 128;
            var yo = ((int)(NextCombatRandom() % 256) - 128) * radius / 128;
            var zo = (NextCombatRandom() % 256) * height / 255;
            var remainingTics = 70 + (int)(NextCombatRandom() % 64);
            var chunk = new IceChunkActor(remainingTics)
            {
                Id = _nextActorId,
                Level = Level,
                Simulation = this,
                X = Fixed.FromDouble(baseX + xo),
                Y = Fixed.FromDouble(baseY + yo),
                Z = Fixed.FromDouble(baseZ + zo),
            };
            _nextActorId = checked(_nextActorId + 1);
            chunk.States.Enter(chunk, (int)(NextCombatRandom() % 3));
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
        corpse.Solid = corpse.Shootable = false;
        ActorDropItem.DropVanillaDeathItem(this, corpse);
        corpse.States.Enter(corpse, -1);
    }

    // Independent managed stream: lighting must not change weapon damage/spread rolls.
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
        _nextActorId = checked(_nextActorId + 1);
        projectile.Aim(target);
        _actors.Add(projectile);
        Thinkers.Add(projectile);
        return projectile;
    }

    internal Actor? SpawnLostSoul(Actor parent, Actor? target, double angle, int limit = -1)
    {
        if (parent.SectorIndex >= 0 && parent.Z.ToDouble() + parent.Height.ToDouble() + 8 > CeilingOf(parent.SectorIndex))
            return null;
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
        if (defaults is { MassPatched: true }) soul.Mass = defaults.Mass;
        if (defaults is { MissileDamagePatched: true }) soul.Damage = defaults.MissileDamage;
        ActorSpawner.ApplyPrimaryFlags(soul, defaults, playerStart: false);
        soul.Ambush = defaults is { BitsPatched: true } && (defaults.Bits & 0x20) != 0;
        if (defaults is { BitsPatched: true }) soul.IsMonster = (defaults.Bits & 0x00400000) != 0;
        soul.SpawnCanPickupItems = soul.CanPickupItems;
        soul.SpawnSpecialPickup = soul.SpecialPickup;
        ActorSpawner.ApplyExtendedDefaults(soul, defaults);
        if (soul.IsMonster && Skill == 4) soul.ReactionTime = 0;
        ThingActivation.InitializeSpawn(soul, false);
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
            Tic = Thinkers.Clock.Tic,
            Exited = Exited,
            SecretExit = SecretExit,
            CombatRandomState = CombatRandomState,
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
        foreach (var actor in _actors.OrderBy(actor => actor.Id))
        {
            state.Actors.Add(new SimActorPose
            {
                Id = actor.Id,
                X = actor.X.Raw,
                Y = actor.Y.Raw,
                Angle = actor.Angle.Raw,
                Roll = actor.Roll.Raw,
                ContactFlags = (actor.CanPickupItems ? 1 : 0) | (actor.SpecialPickup ? 2 : 0),
                Pickup = PickupCatalog.IsPickup(actor.DoomEdNum)
                    ? new SimPickupProperties(actor.PickupAmount, actor.IgnoreAmmoSkill, actor.Depleted) : null,
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
            });
        }

        for (var i = 0; i < Floors.Length; i++)
            state.Sectors.Add((Floors[i], Ceilings[i]));
        return state;
    }

    public void RestoreState(SimSaveState state)
    {
        SimSavegame.ValidateSectors(state);
        SimSavegame.ValidateWalls(state);
        SimSavegame.ValidatePlanes(state);
        SimSavegame.ValidateTextureScrolls(state);
        SimSavegame.ValidatePickups(state);
        SimSavegame.ValidateContactFlags(state);
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
            if (actor != null && (pose.WeaponCooldown < 0 || actor is PlayerPawn && Math.Abs((long)pose.Pitch) > 89L * 65536
                || pose.HasPhysics && (!actor.States.HasState(pose.State) || pose.StateTics < -1)))
                throw new InvalidOperationException("Saved actor state is not in the current table.");
        }
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
        foreach (var pose in state.Actors)
        {
            var actor = _actors.FirstOrDefault(candidate => candidate.Id == pose.Id);
            if (actor == null)
                continue;
            actor.X = new Fixed(pose.X);
            actor.Y = new Fixed(pose.Y);
            actor.Angle = new BamAngle(pose.Angle);
            actor.PitchDegrees = new Fixed(pose.Pitch).ToDouble();
            actor.RestoreHealth(pose.Health);
            if (pose.Roll is { } roll) actor.Roll = new BamAngle(roll);
            if (pose.ContactFlags is { } contactFlags)
            {
                actor.CanPickupItems = (contactFlags & 1) != 0;
                actor.SpecialPickup = (contactFlags & 2) != 0;
            }
            if (pose.Pickup is { } pickup)
            {
                actor.PickupAmount = pickup.Amount;
                actor.IgnoreAmmoSkill = pickup.IgnoreSkill;
                actor.Depleted = pickup.Depleted;
            }
            actor.Z = new Fixed(pose.Z);
            actor.VelocityX = new Fixed(pose.VelocityX);
            actor.VelocityY = new Fixed(pose.VelocityY);
            actor.VelocityZ = new Fixed(pose.VelocityZ);
            actor.OnGround = pose.OnGround;
            actor.SectorIndex = ActorPhysics.SectorAt(Level, actor.X.ToDouble(), actor.Y.ToDouble());
            if (pose.HasPhysics) actor.States.Restore(pose.State, pose.StateTics);
            else actor.States.Restore(actor.IsDead ? actor.DeathState : actor.SpawnState, -1);
            if (actor is PlayerPawn player)
            {
                player.ClearCommands(); player.AttackPressed = false; player.UsePressed = false;
                player.WeaponCooldown = pose.WeaponCooldown;
                player.UseHeld = pose.UseHeld;
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
                if (!PickupCatalog.TryGive(player, actor.DoomEdNum, actor.IgnoreAmmoSkill, actor.Depleted, actor.PickupAmount)
                    && !PickupCatalog.AlwaysPickup(actor.DoomEdNum))
                    continue;
                taken.Add(actor);
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
        hash = Mix(hash, unchecked((uint)Compat));
        hash = Mix(hash, DamageExitAllowed ? 1u : 0u);
        hash = Mix(hash, (uint)GameMode);
        hash = Mix(hash, ReloadRequested ? 1u : 0u);
        hash = Mix(hash, AllowSinglePlayerRespawn ? 1u : 0u);
        hash = Mix(hash, ForceRespawn ? 1u : 0u);
        hash = Mix(hash, NoRespawn ? 1u : 0u);
        hash = Mix(hash, WeaponDrop ? 1u : 0u);
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
        hash = Mix(hash, unchecked((uint)Fixed.FromDouble(AmmoFactor).Raw));
        hash = Mix(hash, DoubleAmmo ? 1u : 0u);
        if (DropStyle != 0) hash = Mix(hash, unchecked((uint)DropStyle));
        hash = Mix(hash, unchecked((uint)Infighting));
        hash = Mix(hash, unchecked((uint)Thinkers.Clock.Tic));
        hash = Mix(hash, unchecked((uint)RngSeed));
        hash = Mix(hash, CombatRandomState);
        hash = Mix(hash, SwitchTargetRandomState);
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
            hash = Mix(hash, unchecked((uint)actor.ThingId));
            hash = Mix(hash, unchecked((uint)actor.X.Raw));
            hash = Mix(hash, unchecked((uint)actor.Y.Raw));
            hash = Mix(hash, unchecked((uint)actor.Z.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityX.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityY.Raw));
            hash = Mix(hash, unchecked((uint)actor.VelocityZ.Raw));
            hash = Mix(hash, actor.Angle.Raw);
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
            hash = Mix(hash, actor.NoGravity ? 1u : 0u);
            hash = Mix(hash, actor.LastHeardTargetId ?? 0);
            hash = Mix(hash, actor.Floating ? 1u : 0u);
            hash = Mix(hash, actor.AllowDropOff ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.MaxDropOffHeight.Raw));
            hash = Mix(hash, unchecked((uint)Fixed.FromDouble(actor.FloatSpeed).Raw));
            hash = Mix(hash, (uint)actor.ResurrectionHealth);
            hash = Mix(hash, unchecked((uint)actor.GibHealth));
            hash = Mix(hash, unchecked((uint)actor.SeeState));
            hash = Mix(hash, unchecked((uint)actor.ExtremeDeathState));
            hash = Mix(hash, unchecked((uint)actor.GenericFreezeDeath));
            hash = Mix(hash, actor.NoIceDeath ? 1u : 0u);
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
            hash = Mix(hash, actor.JustHit ? 1u : 0u);
            hash = Mix(hash, actor.ActsLikeBridge ? 1u : 0u);
            hash = Mix(hash, actor.IceCorpse ? 1u : 0u);
            hash = Mix(hash, actor.Shattering ? 1u : 0u);
            hash = Mix(hash, actor.IceShatter ? 1u : 0u);
            hash = Mix(hash, actor.NeverTarget ? 1u : 0u);
            if (actor.NoAutoOffSkullFly) hash = Mix(hash, 0x534B554Cu);
            if (actor.SpawnCeiling) hash = Mix(hash, 0x4345494Cu);
            if (actor.NoBlockMonsters) hash = Mix(hash, 0x4E424D4Fu);
            if (actor.NoBlockmap) hash = Mix(hash, 0x4E424D50u);
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
            hash = Mix(hash, unchecked((uint)actor.TidToHate));
            hash = Mix(hash, actor.QuickToRetaliate ? 1u : 0u);
            hash = Mix(hash, actor.Friendly ? 1u : 0u);
            hash = Mix(hash, (uint)actor.FriendPlayer);
            hash = Mix(hash, actor.NoInfighting ? 1u : 0u);
            hash = Mix(hash, actor.ForceInfighting ? 1u : 0u);
            hash = Mix(hash, actor.DoHarmSpecies ? 1u : 0u);
            hash = Mix(hash, actor.NoTelefrag ? 1u : 0u);
            hash = Mix(hash, actor.AlwaysTelefrag ? 1u : 0u);
            hash = Mix(hash, actor.DontDrain ? 1u : 0u);
            hash = Mix(hash, actor.IgnoreAmmoSkill ? 1u : 0u);
            hash = Mix(hash, actor.Depleted ? 1u : 0u);
            hash = Mix(hash, (uint)actor.PickupAmount);
            hash = Mix(hash, (uint)actor.RaiseDuration);
            hash = Mix(hash, (uint)actor.Mass);
            hash = Mix(hash, (uint)actor.Gravity.Raw);
            hash = Mix(hash, (uint)actor.DamageFactor.Raw);
            hash = Mix(hash, (uint)actor.DamageMultiplier.Raw);
            hash = Mix(hash, (uint)actor.MeleeRange.Raw);
            hash = Mix(hash, (uint)actor.Friction.Raw);
            hash = Mix(hash, actor.NoTrigger ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.Score));
            hash = Mix(hash, unchecked((uint)actor.MovementSpeed.Raw));
            hash = Mix(hash, unchecked((uint)actor.Damage));
            hash = Mix(hash, actor.Dropped ? 1u : 0u);
            hash = Mix(hash, unchecked((uint)actor.ReactionTime));
            hash = Mix(hash, actor.ReactionTimeInitialized ? 1u : 0u);
            hash = Mix(hash, actor.NoRadiusDamage ? 1u : 0u);
            hash = Mix(hash, actor.NoSectorDamage ? 1u : 0u);
            hash = Mix(hash, actor.ForceSectorDamage ? 1u : 0u);
            hash = Mix(hash, actor.Ambush ? 1u : 0u);
            hash = Mix(hash, actor.LastDamageSourceId ?? 0);
            if (actor is PlayerPawn player)
            {
                hash = Mix(hash, (uint)player.WeaponCooldown);
                hash = Mix(hash, player.PlayerNum);
                hash = Mix(hash, player.GodMode ? 1u : 0u);
                hash = Mix(hash, player.UseHeld ? 1u : 0u);
                hash = Mix(hash, player.AttackHeld ? 1u : 0u);
                hash = Mix(hash, player.RespawnArmed ? 1u : 0u);
                hash = Mix(hash, unchecked((uint)player.RespawnEarliestTic));
                hash = Mix(hash, unchecked((uint)Fixed.FromDouble(player.UseRange).Raw));
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
                hash = Mix(hash, (uint)player.Inventory.SpareArmor.Count);
                foreach (var spare in player.Inventory.SpareArmor)
                {
                    hash = Mix(hash, (uint)spare.SaveAmount);
                    hash = Mix(hash, (uint)spare.SavePercent);
                    hash = Mix(hash, (uint)spare.MaxAbsorb);
                    hash = Mix(hash, (uint)spare.MaxFullAbsorb);
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
        int rngSeed = 0)
    {
        simulation = null;
        if (!LevelBuilder.TryFromWad(wad, mapName, out var level, out error))
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
