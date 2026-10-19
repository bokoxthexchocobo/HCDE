using System.Buffers.Binary;

namespace HCDE.Playsim;

public readonly record struct SimPickupProperties(int Amount, bool IgnoreSkill, bool Depleted);

public readonly record struct SimThruBits(uint Mask, bool Enabled);
public readonly record struct SimActorSpecial(int Special, int Arg0, int Arg1, int Arg2, int Arg3, int Arg4, int ActivationType = 0, int Stamina = 0, int BonusHealth = 0, int MaxPickupHealth = 0);

public sealed class SimActorPose
{
    public int PlayerExtraLight { get; internal set; }
    public int Accuracy { get; internal set; }
    public int FriendlySeeBlocks { get; internal set; } = 10;
    public int SeeFriendlyMonstersFlag { get; internal set; }
    public int LookBehaviorFlags { get; internal set; }
    public int InConversationFlag { get; internal set; }
    public int DontFollowPlayersFlag { get; internal set; }
    public int CanJumpFlag { get; internal set; }
    public int BlockAsPlayerFlag { get; internal set; }
    public int FloorClipFlags { get; internal set; }
    public int AvoidHazardsFlag { get; internal set; }
    public int StayOnLiftFlag { get; internal set; }
    public int AvoidMeleeFlag { get; internal set; }
    public int StrafeCount { get; internal set; }
    public int FastChaseStrafeCount { get; internal set; }
    public int MinMissileChanceOverride { get; internal set; } = -1;
    public double? MissileChanceMultOverride { get; internal set; }
    public double? MaxTargetRangeOverride { get; internal set; }
    public double? MeleeThresholdOverride { get; internal set; }
    public int CantLeaveFloorPicFlag { get; internal set; }
    public int FearFlags { get; internal set; }
    public int MoveDirection { get; internal set; }
    public int MoveCount { get; internal set; }
    public SimSoundClass? SoundClassOverride { get; internal set; }
    public int PowerStrengthTics { get; internal set; }
    public int PlayerBonusCount { get; internal set; }
    public int PickupPresentationFlags { get; internal set; }
    public int PickupNoAttenuationFlag { get; internal set; }
    public int? PickupHealthOverride { get; internal set; }
    public int? PickupAutoUseOverride { get; internal set; }
    public SimDroppedPickupIdentity? DroppedPickupIdentity { get; internal set; }
    public SimPuffIdentity? PuffIdentity { get; internal set; }
    public int? MapMonsterThingIndex { get; internal set; }
    public SimMapDecorationIdentity? MapDecorationIdentity { get; internal set; }
    public SimMapPickupIdentity? MapPickupIdentity { get; internal set; }
    public SimBotIdentity? BotIdentity { get; internal set; }
    public SimActorDamageHistory? DamageHistory { get; internal set; }
    public int? AmbushFlag { get; internal set; }
    public int? CantSeekFlag { get; internal set; }
    public int? GibHealthOverride { get; internal set; }
    public uint? LastLookActorId { get; internal set; }
    public int? HateTargetingFlags { get; internal set; }
    public int? VulnerableFlag { get; internal set; }
    public int? MonsterClassificationFlag { get; internal set; }
    public int? NoTargetFlag { get; internal set; }
    public int? PlayerNoTargetCheatOverride { get; internal set; }
    public int BuddhaFlag { get; internal set; }
    public int FlyCheatFlag { get; internal set; }
    public int PlayerFrozenFlags { get; internal set; }
    public int NoClipFlag { get; internal set; }
    public int PlayerNoClip2Flag { get; internal set; }
    public int? ChaseGoalFlag { get; internal set; }
    public int? JustAttackedFlag { get; internal set; }
    public int? InCombatFlag { get; internal set; }
    public int? NoSplashAlertFlag { get; internal set; }
    public int? DontSplashFlag { get; internal set; }
    public int? DontHarmSpeciesFlag { get; internal set; }
    public int? DontHarmClassFlag { get; internal set; }
    public double DeltaViewHeight { get; internal set; }
    public double GruntSpeed { get; internal set; } = PlayerPawn.DefaultGruntSpeed;
    public double LandingSpeed { get; internal set; } = Actor.DefaultLandingSpeed;
    public int? PushHistoryFlag { get; internal set; }
    public int? BlockedBySolidActorsFlag { get; internal set; }
    public int? BumpSpecialFlag { get; internal set; }
    public int? ReflectionFlags { get; internal set; }
    public int? SeekerMissileFlag { get; internal set; }
    public int? SummonedMonsterFlag { get; internal set; }
    public int? ScreenSeekerFlag { get; internal set; }
    public int? VisibilityPulseFlag { get; internal set; }
    public int VisibilityDirection { get; internal set; }
    public int? StealthFlag { get; internal set; }
    public int? StealthDefaultsFlag { get; internal set; }
    public int? FloatBobDefaultsFlag { get; internal set; }
    public int? SeekerDefaultsFlag { get; internal set; }
    public int? ReflectiveDefaultsFlag { get; internal set; }
    public int? ThruGhostDefaultsFlag { get; internal set; }
    public int? NonShootableDefaultsFlag { get; internal set; }
    public int? FloorPicDefaultsFlag { get; internal set; }
    public int? RipDefaultsFlag { get; internal set; }
    public int? PushDefaultsFlags { get; internal set; }
    public int? TeleStompDefaultsFlag { get; internal set; }
    public int? FloorClipDefaultsFlag { get; internal set; }
    public int? BlastedDefaultsFlag { get; internal set; }
    public int? NoTeleportOverrideFlag { get; internal set; }
    public int? TouchyDefaultsFlag { get; internal set; }
    public int? NoClipDefaultsFlag { get; internal set; }
    public int? CombatDefaultsFlags { get; internal set; }
    public int? InFloatDefaultsFlag { get; internal set; }
    public int? CorpseDefaultsFlag { get; internal set; }
    public int? SpawnCeilingOverrideFlag { get; internal set; }
    public int? FlyDefaultsFlag { get; internal set; }
    public int? SpawnFloatFlags { get; internal set; }
    public int? PainChanceOverride { get; internal set; }
    public int? ProjectileSecondaryFlags { get; internal set; }
    public double StealthAlpha { get; internal set; }
    public SimFloatBobSettings? FloatBobSettings { get; internal set; }
    public int BumpCooldownTic { get; internal set; }
    public int? FallDamageFlag { get; internal set; }
    public int? SmashableFlag { get; internal set; }
    public int? CrashedFlag { get; internal set; }
    public int? DontGibFlag { get; internal set; }
    public int? CrashStateOverride { get; internal set; }
    public int? ExtremeCrashStateOverride { get; internal set; }
    public int? CrushStateOverride { get; internal set; }
    public int? TeleStompFlags { get; internal set; }
    public int TeleportFreezeTime { get; internal set; } = 18;
    public int? TouchyImpactFlags { get; internal set; }
    public int RespawnEarliestTic { get; internal set; } = int.MaxValue;
    public bool RespawnArmed { get; internal set; }
    public int? CombatBehaviorFlags { get; internal set; }
    public int? TargetSwitchFlags { get; internal set; }
    public int? InfightingFlags { get; internal set; }
    public int Score { get; internal set; }
    public bool SeeInvisible { get; internal set; }
    public int LastLookPlayerNumber { get; internal set; }
    public int? MVisBlockedFlag { get; internal set; }
    public int? NoBloodFlag { get; internal set; }
    public int? PuffGetsOwnerFlag { get; internal set; }
    public int? WeaponRefiringFlag { get; internal set; }
    internal SimPlayerInput? PlayerInput { get; set; }
    public int? IsPuffFlag { get; internal set; }
    public int? SpawnDeathFlag { get; internal set; }
    internal bool LegacyImmediateSpawnDeath { get; set; }
    public int? SpawnSoundSourceFlag { get; internal set; }
    public SimActorRenderProperties? RenderProperties { get; internal set; }
    public int? DontThrustFlag { get; internal set; }
    public int? DontSeekInvisibleFlag { get; internal set; }
    public int? SeekVisibilityFlags { get; internal set; }
    public bool BrainDisabled { get; internal set; }
    public SimRepeatAttack? LostSoulAttack { get; internal set; }
    public SimRepeatAttack? ArchvileAttack { get; internal set; }
    public SimRepeatAttack? RevenantAttack { get; internal set; }
    public SimRepeatAttack? ProfileAttack { get; internal set; }
    public SimRepeatAttack? RepeatAttack { get; internal set; }
    public IReadOnlyDictionary<ActorSoundType, string> ActorSounds { get; internal set; } = new Dictionary<ActorSoundType, string>();
    public bool FullVolumeDeath { get; internal set; }
    public bool FullVolumeSee { get; internal set; }
    public bool OnlySlamSolid { get; internal set; }
    public bool CausePain { get; internal set; }
    public bool AllowPain { get; internal set; }
    public bool NoDamage { get; internal set; }
    public bool LaxTelefragDamage { get; internal set; }
    public double PlayerDrainStrength { get; internal set; }
    public bool PlayerGodMode2 { get; internal set; }
    public bool PlayerBuddha2 { get; internal set; }
    public bool PlayerGodMode { get; internal set; }
    public int PlayerDamageCount { get; internal set; }
    public int PlayerPoisonCount { get; internal set; }
    public IReadOnlyList<HealthPickupStack>? HealthPickups { get; internal set; }
    public int PlayerKeyBits { get; internal set; }
    public SimAmmoInventory? PlayerAmmo { get; internal set; }
    public SimWeaponInventory? PlayerWeapons { get; internal set; }
    public uint? PoisonSourceId { get; internal set; }
    public string? PoisonDamageType { get; internal set; }
    public string? PoisonPainType { get; internal set; }
    public int SelfDamageFactor { get; internal set; } = 65536;
    public bool RetargetAfterSlam { get; internal set; }
    public bool FullVolumeActive { get; internal set; }
    public string? PlayerLastDamageType { get; internal set; }
    public string? PainType { get; internal set; }
    public int DeathDamageAmount { get; internal set; }
    public double? UseRange { get; internal set; }
    public int? UseSpecialFlag { get; internal set; }
    public int? SectorLinkFlag { get; internal set; }
    public int? MoveWithSectorFlag { get; internal set; }
    public int? CanPassFlag { get; internal set; }
    public int? DontOverlapFlag { get; internal set; }
    public int? ExplodeCountFlag { get; internal set; }
    public int? SkyExplodeFlag { get; internal set; }
    public int? MissileHitPointerFlags { get; internal set; }
    public int? FloorCarryFlags { get; internal set; }
    public SimTracerPointer? TracerPointer { get; internal set; }
    public SimMasterPointer? MasterPointer { get; internal set; }
    public SimFriendship? Friendship { get; internal set; }
    public int? BlockmapFlag { get; internal set; }
    public double? FloorClip { get; internal set; }
    public int? InvisibleFlag { get; internal set; }
    public int? AllowDropOffFlag { get; internal set; }
    public int? SkullChargeFlag { get; internal set; }
    public int? ReactionTime { get; internal set; }
    public int? MeleeRangeRaw { get; internal set; }
    public int? ConstantDamage { get; internal set; }
    public int? JustHitFlag { get; internal set; }
    public int? FullBrightFlag { get; internal set; }
    public int? FastModeFlags { get; internal set; }
    public int? HandleNoDelayFlag { get; internal set; }
    public int? SynchronizedFlag { get; internal set; }
    public int? IceChunkLifecycle { get; internal set; }
    public bool? IceCorpseFlag { get; internal set; }
    public SimGoalPointer? GoalPointer { get; internal set; }
    public int? DontFallFlag { get; internal set; }
    public int? SlideFlag { get; internal set; }
    public int? FallingFlag { get; internal set; }
    public int? DontCorpseFlag { get; internal set; }
    public int? CorpseFlag { get; internal set; }
    public int? NoFrictionFlag { get; internal set; }
    public int? FlyFlag { get; internal set; }
    public int? ClassicFlightFlag { get; internal set; }
    public int? JumpTics { get; internal set; }
    public SimCrouchState? Crouch { get; internal set; }
    public int? MonsterBlockingFlag { get; internal set; }
    public string? DeathType { get; internal set; }
    public string? DamageType { get; internal set; }
    public bool SpecialFireDamage { get; internal set; }
    public bool FoilInvul { get; internal set; }
    public bool PierceArmor { get; internal set; }
    public bool NoInfightSpecies { get; internal set; }
    public bool NoVerticalMeleeRange { get; internal set; }
    public bool Killed { get; internal set; }
    public int? MovementActionFlags { get; internal set; }
    public int? FrictionRaw { get; internal set; }
    public int? GravityRaw { get; internal set; }
    public SimChaseThreshold? ChaseThreshold { get; internal set; }
    public SimDefenseProperties? DefenseProperties { get; internal set; }
    public SimActorTuning? Tuning { get; internal set; }
    public SimTeleFog? TeleFog { get; internal set; }
    public SimActorSize? Size { get; internal set; }
    public SimSpecies? SpeciesOverride { get; internal set; }
    public SimActorScale? Scale { get; internal set; }
    public int? FloatBobPhase { get; internal set; }
    public SimSpriteOrientation? SpriteOrientation { get; internal set; }
    public SimTargetMemory? TargetMemory { get; internal set; }
    public SimBasicAttackState? BasicAttackState { get; internal set; }
    public SimProfileChaseHistory? ProfileChaseHistory { get; internal set; }
    public string? WornArmorType { get; internal set; }
    public int WornArmorAmount { get; internal set; }
    public SimWornArmor? WornArmor { get; internal set; }
    public IReadOnlyList<SpareArmor>? SpareArmor { get; internal set; }
    public SimActorArmor? ActorArmor { get; internal set; }
    public bool StrifeDamage { get; internal set; }
    public bool Rip { get; internal set; }
    public bool DontRip { get; internal set; }
    public bool NoBossRip { get; internal set; }
    public bool Pushable { get; internal set; }
    public bool CannotPush { get; internal set; }
    public double PushFactor { get; internal set; } = 0.25;
    public int RipperLevel { get; internal set; }
    public int RipLevelMin { get; internal set; }
    public int RipLevelMax { get; internal set; }
    public int ProjectilePassHeight { get; internal set; }
    public int DamageFactor { get; internal set; } = 65536;
    public int DamageMultiplier { get; internal set; } = 65536;
    public IReadOnlyDictionary<string, double>? DamageFactors { get; internal set; }
    public int InfightingGroup { get; internal set; }
    public int ProjectileGroup { get; internal set; }
    public int SplashGroup { get; internal set; }
    public int PowerDamageTics { get; internal set; }
    public int PowerProtectionTics { get; internal set; }
    public int PowerBuddhaTics { get; internal set; }
    public double CameraHeight { get; internal set; } = Actor.DefaultCameraHeight;
    public int PowerIronFeetMode { get; internal set; }
    public int PowerMaskTics { get; internal set; }
    public int PowerIronFeetTics { get; internal set; }
    public int PowerInfiniteAmmoTics { get; internal set; }
    public int PowerDrainTics { get; internal set; }
    public double PowerDrainStrength { get; internal set; } = 0.5;
    public int PowerProsperityTics { get; internal set; }
    public int PowerSpreadTics { get; internal set; }
    public int PowerRegenerationTics { get; internal set; }
    public double PowerRegenerationStrength { get; internal set; } = 5;
    public int PowerDoubleFiringSpeedTics { get; internal set; }
    public int PowerHighJumpTics { get; internal set; }
    public bool PowerHighJumpPendingRemoval { get; internal set; }
    public double PowerHighJumpStrength { get; internal set; } = 2;
    public SimActorSpecial? ActorSpecial { get; internal set; }
    public uint Id { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public uint Angle { get; init; }
    public int Health { get; init; }
    public int Z { get; init; }
    public int VelocityX { get; init; }
    public int VelocityY { get; init; }
    public int VelocityZ { get; init; }
    public int State { get; init; }
    public int StateTics { get; init; } = -1;
    public bool OnGround { get; init; } = true;
    public bool HasPhysics { get; init; } = true;
    public int WeaponCooldown { get; init; }
    public int Pitch { get; init; }
    public bool UseHeld { get; init; }
    public uint? Roll { get; internal set; }
    public SimPickupProperties? Pickup { get; internal set; }
    public int PickupDelay { get; internal set; }
    public bool Dropped { get; internal set; }
    public bool SuppressWeaponPickupAmmo { get; internal set; }
    public bool? AlwaysPickupOverride { get; internal set; }
    public int? ContactFlags { get; internal set; }
    public int? FloatFlags { get; internal set; }
    public int? DeathFlags { get; internal set; }
    public SimPainDeath? PainDeath { get; internal set; }
    public int? ProjectileFlags { get; internal set; }
    public SimProjectileLifetime? ProjectileLifetime { get; internal set; }
    public SimProjectilePointers? ProjectilePointers { get; internal set; }
    public int? CeilingFlags { get; internal set; }
    public int? FloorHuggerFlags { get; internal set; }
    public int? BlastedFlags { get; internal set; }
    public int? BlastEligibilityFlags { get; internal set; }
    public int? ThruActorsFlags { get; internal set; }
    public int? MissileThruSpeciesFlags { get; internal set; }
    public int? ThruSpeciesFlags { get; internal set; }
    public SimThruBits? ThruBits { get; internal set; }
    public int? GhostFlags { get; internal set; }
    public int? NonShootableFlags { get; internal set; }
    public int? HitOwnerFlags { get; internal set; }
    public int? SpectralFlags { get; internal set; }
    public int? PlayerMaxHealth { get; internal set; }
}

public sealed class SimSaveState
{
    public IReadOnlyList<SimDoorMotion>? DoorMotions { get; internal set; }
    public IReadOnlyList<SimSectorMotion>? SectorMotions { get; internal set; }
    public IReadOnlyDictionary<int, uint>? SectorSoundTargets { get; internal set; }
    public IReadOnlyList<SimThinkerPlacement>? ThinkerOrder { get; internal set; }
    public bool IncludesBotIdentities { get; internal set; }
    public uint? NextActorId { get; internal set; }
    public SimInvasionState? Invasion { get; internal set; }
    public HCDE.Gamedata.FallingDamageStyle ForcedFallingDamage { get; internal set; }
    public double? AirControlOverride { get; internal set; }
    public double? GibFactor { get; internal set; }
    public double? LevelGravityOverride { get; internal set; }
    public uint? JumpRandomState { get; internal set; }
    public ulong? StateRandomState { get; internal set; }
    public ulong? MapSpawnRandomState { get; internal set; }
    public ulong? FloatSpawnRandomState { get; internal set; }
    public ulong? IceTicsRandomState { get; internal set; }
    public bool IncludesIceChunkLifecycle { get; internal set; }
    public ulong? FreezeChunksRandomState { get; internal set; }
    public ulong? BfgSelfDamageRandomState { get; internal set; }
    public ulong? BfgSprayRandomState { get; internal set; }
    public ulong? FireSg2RandomState { get; internal set; }
    public ulong? GunShotRandomState { get; internal set; }
    public ulong? FreezeDeathRandomState { get; internal set; }
    public ulong? DropItemRandomState { get; internal set; }
    public ulong? PlayerSearchRandomState { get; internal set; }
    public ulong? PlayerLookRandomState { get; internal set; }
    public bool CompatibilityInvisibility { get; internal set; }
    public bool VerticalWeaponSpread { get; internal set; }
    public ulong? HateSearchRandomState { get; internal set; }
    public ulong? ActorDieRandomState { get; internal set; }
    public ulong? PuffRandomState { get; internal set; }
    public ulong? ChaseRandomState { get; internal set; }
    public ulong? MissileDamageRandomState { get; internal set; }
    public ulong? PoisonDamageRandomState { get; internal set; }
    public ulong? ReflectRandomState { get; internal set; }
    public ulong? SeekerMissileRandomState { get; internal set; }
    public ulong? PainChanceRandomState { get; internal set; }
    public ulong? SkelFistRandomState { get; internal set; }
    public ulong? SawRandomState { get; internal set; }
    public ulong? PunchRandomState { get; internal set; }
    public ulong? NewChaseDirRandomState { get; internal set; }
    public ulong? TryWalkRandomState { get; internal set; }
    public ulong? AvoidCrushRandomState { get; internal set; }
    public ulong? StayOnLiftRandomState { get; internal set; }
    public ulong? ScaredyCatRandomState { get; internal set; }
    public ulong? CheckMissileRandomState { get; internal set; }
    public ulong? DefectRandomState { get; internal set; }
    public ulong? CheckSightRandomState { get; internal set; }
    public uint? SwitchTargetRandomState { get; internal set; }
    public ulong? UniqueTidRandomState { get; internal set; }
    public ulong? SectorDamageRandomState { get; internal set; }
    public ulong? NativeSwitchTargetRandomState { get; internal set; }
    public ulong? EnemyStrafeRandomState { get; internal set; }
    public ulong? SkipTargetRandomState { get; internal set; }
    public ulong? CPosRefireRandomState { get; internal set; }
    public ulong? CPosAttackRandomState { get; internal set; }
    public ulong? SPosAttackRandomState { get; internal set; }
    public ulong? PosAttackRandomState { get; internal set; }
    public ulong? BruisAttackRandomState { get; internal set; }
    public ulong? HeadAttackRandomState { get; internal set; }
    public ulong? TroopAttackRandomState { get; internal set; }
    public ulong? SargAttackRandomState { get; internal set; }
    public List<short>? Lights { get; internal set; }
    public SimLightAnimation? LightAnimation { get; internal set; }
    public int Tic { get; init; }
    public bool Exited { get; init; }
    public bool SecretExit { get; init; }
    public uint? CombatRandomState { get; init; }
    public IReadOnlyDictionary<string, DamageTypeFactor>? DamageTypes { get; internal set; }
    public bool IncludesDamageTypeDefinitions { get; internal set; }
    public List<SimActorPose> Actors { get; } = new();
    public List<(double Floor, double Ceiling)> Sectors { get; } = new();
    public List<SimWallTransform>? Walls { get; init; }
    public List<SimPlaneTransform>? Planes { get; init; }
    public List<SimTextureScroll>? TextureScrolls { get; init; }
    public bool IncludesCarryScrolls { get; init; }
    public bool IncludesCeilingControls { get; init; }
    public bool IncludesFloorControls { get; init; }
    public bool IncludesWallControls { get; init; }
    public bool IncludesWallParts { get; init; }
    public SimGeometryHealth? GeometryHealth { get; internal set; }
    public IReadOnlyList<SimSectorTerrain>? SectorTerrains { get; internal set; }
    public IReadOnlyList<SimLineAction>? LineActions { get; internal set; }
}

/// <summary>
/// Authority pose archive. Actors match by id. Sector floor and ceiling heights round-trip.
/// This is not <c>p_saveg.cpp</c>.
/// </summary>
public static class SimSavegame
{
    public static ReadOnlySpan<byte> Magic => "HCSV"u8;

    public static byte[] Write(AuthoritySimulation sim) => Write(sim.CaptureState());
    public static byte[] Write(AuthoritySimulation sim, bool includeBotIdentities) => Write(sim.CaptureState(includeBotIdentities));
    public static byte[] Write(AuthoritySimulation sim, bool includeBotIdentities, bool includeThinkerOrder) =>
        Write(sim.CaptureState(includeBotIdentities, includeThinkerOrder));

    public static byte[] Write(SimSaveState state)
    {
        SimLineActionsArchive.Validate(state);
        if (state.SkipTargetRandomState.HasValue && state.GeometryHealth is null)
            throw new InvalidOperationException("SkipTarget random archives require geometry state.");
        SimActorSoundArchive.Validate(state);
        if (state.Actors.Any(actor => actor.DeathDamageAmount != 0) && state.GeometryHealth is null)
            throw new InvalidOperationException("Saved death-hit amounts require geometry state.");
        if (state.Actors.Any(actor => actor.PainType is not null || actor.PlayerLastDamageType is not null) && state.GeometryHealth is null)
            throw new InvalidOperationException("Saved player damage types require geometry state.");
        if (state.Actors.Any(actor => actor.PlayerDamageCount != 0 || actor.PlayerPoisonCount != 0 || actor.HealthPickups is { Count: > 0 } || actor.PoisonSourceId.HasValue || actor.PoisonDamageType is not null || actor.PoisonPainType is not null || actor.PlayerGodMode || actor.PlayerBuddha2 || actor.PlayerGodMode2 || actor.PlayerDrainStrength != 0 || actor.LaxTelefragDamage || actor.SelfDamageFactor != 65536 || actor.NoDamage || actor.AllowPain || actor.CausePain || actor.OnlySlamSolid || actor.RetargetAfterSlam || actor.FullVolumeDeath || actor.FullVolumeSee || actor.FullVolumeActive) && state.GeometryHealth is null)
            throw new InvalidOperationException("Saved full-volume sound flags require geometry state.");
        if (state.Actors.Any(actor => actor.ActorSounds.Count != 0) && state.GeometryHealth is null)
            throw new InvalidOperationException("Saved actor sounds require geometry state.");
        ValidateSectors(state);
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
        SimChaseGoalArchive.Validate(state);
        SimJustAttackedArchive.Validate(state);
        SimInCombatArchive.Validate(state);
        SimNoSplashAlertArchive.Validate(state);
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
        SimSoundClassArchive.Validate(state);
        SimNoBloodArchive.Validate(state);
        SimPuffGetsOwnerArchive.Validate(state);
        SimWeaponRefiringArchive.Validate(state);
        SimSectorTerrainArchive.Validate(state);
        SimIsPuffArchive.Validate(state);
        SimSpawnDeathArchive.Validate(state);
        SimSpawnSoundSourceArchive.Validate(state);
        SimActorRenderArchive.Validate(state);
        SimDontThrustArchive.Validate(state);
        SimDontSeekInvisibleArchive.Validate(state);
        SimSeekVisibilityArchive.Validate(state);
        ValidateWalls(state);
        ValidatePlanes(state);
        ValidateTextureScrolls(state);
        ValidatePickups(state);
        ValidateContactFlags(state);
        if (state.LevelGravityOverride.HasValue && state.GeometryHealth is null) throw new InvalidOperationException("Saved level gravity requires geometry state.");
        if (state.AirControlOverride.HasValue && state.GeometryHealth is null) throw new InvalidOperationException("Saved air control requires geometry state.");
        if (SimDamageTypeArchive.HasCustomDefinitions(state) && state.GeometryHealth is null)
            throw new InvalidOperationException("Damage definitions require a current archive.");
        SimGibFactorArchive.Validate(state);
        SimActorDieRandomArchive.Validate(state);
        SimBfgSelfDamageRandomArchive.Validate(state);
        SimBfgSprayRandomArchive.Validate(state);
        SimFireSg2RandomArchive.Validate(state);
        SimGunShotRandomArchive.Validate(state);
        SimVerticalWeaponSpreadArchive.Validate(state);
        if ((state.Actors.Any(a => a.LostSoulAttack.HasValue || a.ArchvileAttack.HasValue || a.RevenantAttack.HasValue || a.ProfileAttack.HasValue || a.BrainDisabled || a.RepeatAttack.HasValue) || state.CPosRefireRandomState.HasValue || state.CPosAttackRandomState.HasValue || state.SPosAttackRandomState.HasValue || state.PosAttackRandomState.HasValue || state.BruisAttackRandomState.HasValue || state.HeadAttackRandomState.HasValue || state.TroopAttackRandomState.HasValue || state.SargAttackRandomState.HasValue || state.PainChanceRandomState.HasValue || state.PoisonDamageRandomState.HasValue || state.MissileDamageRandomState.HasValue || state.PunchRandomState.HasValue || state.SawRandomState.HasValue || state.SkelFistRandomState.HasValue || state.ChaseRandomState.HasValue || state.FreezeDeathRandomState.HasValue || state.DropItemRandomState.HasValue || state.PuffRandomState.HasValue || state.FreezeChunksRandomState.HasValue || state.IceTicsRandomState.HasValue || state.MapSpawnRandomState.HasValue || state.StateRandomState.HasValue || state.JumpRandomState.HasValue || state.Lights is not null || state.LightAnimation is not null) && state.GeometryHealth is null) throw new InvalidOperationException("Sector light archives require geometry state.");
        if (state.Actors.Any(actor => actor.FrictionRaw.HasValue || actor.SlideFlag.HasValue || actor.IceChunkLifecycle.HasValue || actor.SynchronizedFlag.HasValue || actor.HandleNoDelayFlag.HasValue || actor.FastModeFlags.HasValue || actor.FullBrightFlag.HasValue || actor.GoalPointer.HasValue || actor.JustHitFlag.HasValue || actor.ConstantDamage.HasValue || actor.MeleeRangeRaw.HasValue || actor.ReactionTime.HasValue || actor.SkullChargeFlag.HasValue || actor.AllowDropOffFlag.HasValue || actor.DontFallFlag.HasValue || actor.FallingFlag.HasValue || actor.DontCorpseFlag.HasValue || actor.CorpseFlag.HasValue || actor.NoFrictionFlag.HasValue || actor.FlyFlag.HasValue || actor.ClassicFlightFlag.HasValue || actor.JumpTics.HasValue || actor.Crouch.HasValue || actor.MonsterBlockingFlag.HasValue || actor.InvisibleFlag.HasValue || actor.FloorClip.HasValue || actor.BlockmapFlag.HasValue || actor.Friendship.HasValue || actor.TracerPointer.HasValue || actor.MasterPointer.HasValue || actor.UseSpecialFlag.HasValue || actor.UseRange.HasValue || actor.FloatBobPhase.HasValue || actor.SpriteOrientation.HasValue || actor.Scale.HasValue || actor.SpeciesOverride.HasValue || actor.Size.HasValue || actor.TeleFog.HasValue || actor.Tuning.HasValue || actor.DefenseProperties.HasValue || actor.ChaseThreshold.HasValue || actor.GravityRaw.HasValue || actor.TargetMemory.HasValue || actor.MovementActionFlags.HasValue || actor.Killed || actor.ActorArmor.HasValue || actor.SpareArmor is { Count: > 0 } || actor.WornArmor.HasValue || actor.WornArmorType is not null || actor.NoVerticalMeleeRange || actor.ActorSpecial.HasValue || !string.IsNullOrEmpty(actor.DeathType) || !string.IsNullOrEmpty(actor.DamageType) || actor.SpecialFireDamage || actor.FoilInvul || actor.PierceArmor || actor.NoInfightSpecies || actor.StrifeDamage || actor.Rip || actor.DontRip || actor.NoBossRip || actor.Pushable || actor.CannotPush || actor.PushFactor != 0.25 || actor.RipperLevel != 0 || actor.RipLevelMin != 0 || actor.RipLevelMax != 0 || actor.ProjectilePassHeight != 0 || actor.DamageFactor != 65536 || actor.DamageMultiplier != 65536 || actor.DamageFactors is { Count: > 0 } || actor.InfightingGroup != 0 || actor.ProjectileGroup != 0 || actor.SplashGroup != 0 || actor.PowerDamageTics != 0 || actor.PowerProtectionTics != 0 || actor.PowerBuddhaTics != 0) && state.GeometryHealth is null)
            throw new InvalidOperationException("Actor specials require a current archive.");
        ValidateFloatFlags(state);
        ValidateDeathFlags(state);
        SimProjectileFlagArchive.Validate(state);
        SimProjectileLifetimeArchive.Validate(state);
        SimProjectilePointerArchive.Validate(state);
        SimSectorLinkArchive.Validate(state);
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
        SimBasicAttackArchive.Validate(state);
        SimProfileChaseArchive.Validate(state);
        SimMissileHitPointerFlagsArchive.Validate(state);
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
        if (state.Actors.Any(actor => actor.Pickup.HasValue) &&
            (state.GeometryHealth is null || state.Actors.Any(actor => !actor.Roll.HasValue)))
            throw new InvalidOperationException("Saved pickup properties require a complete current archive.");
        if (state.Actors.Any(actor => actor.Roll.HasValue) && (state.GeometryHealth is null
            || state.Actors.Any(actor => !actor.Roll.HasValue)))
            throw new InvalidOperationException("Saved actor rolls require a complete current archive.");
        if (state.GeometryHealth is { } geometry && (!state.IncludesWallParts || geometry.Groups.Keys.Any(id => id <= 0)))
            throw new InvalidOperationException("Saved geometry health is invalid.");
        var hasScrolls = state.TextureScrolls is not null;
        var scrollSize = state.IncludesCarryScrolls ? SimTextureScroll.CarrySize : SimTextureScroll.Size;
        var hasPlanes = state.Planes is not null || hasScrolls;
        var hasWalls = state.Walls is { Count: > 0 };
        var size = checked((hasPlanes ? 36 : hasWalls ? 32 : 28) + state.Actors.Count * 60 + state.Sectors.Count * 8
            + (state.Walls?.Count ?? 0) * SimWallTransform.Size
            + (state.Planes?.Count ?? 0) * SimPlaneTransform.Size
            + (hasScrolls ? 4 + state.TextureScrolls!.Count * scrollSize : 0));
        var buffer = new byte[size];
        var span = buffer.AsSpan();
        Magic.CopyTo(span);
        var cursor = 4;
        BinaryPrimitives.WriteUInt16LittleEndian(span[cursor..], hasScrolls ? (state.IncludesWallParts ? (ushort)14 : state.IncludesWallControls ? (ushort)13 : state.IncludesFloorControls ? (ushort)12 : state.IncludesCeilingControls ? (ushort)11 : state.IncludesCarryScrolls ? (ushort)10 : (ushort)9) : hasPlanes ? (ushort)8 : hasWalls ? (ushort)7 : (ushort)6);
        cursor += 2;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Tic);
        cursor += 4;
        span[cursor++] = state.Exited ? (byte)1 : (byte)0;
        span[cursor++] = state.SecretExit ? (byte)1 : (byte)0;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Actors.Count);
        cursor += 4;
        foreach (var actor in state.Actors)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(span[cursor..], actor.Id);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], actor.X);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 8)..], actor.Y);
            BinaryPrimitives.WriteUInt32LittleEndian(span[(cursor + 12)..], actor.Angle);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 16)..], actor.Health);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 20)..], actor.Z);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 24)..], actor.VelocityX);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 28)..], actor.VelocityY);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 32)..], actor.VelocityZ);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 36)..], actor.State);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 40)..], actor.LegacyImmediateSpawnDeath ? 1 : actor.StateTics);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 44)..], (actor.OnGround ? 1 : 0) | (actor.HasPhysics ? 2 : 0));
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 48)..], actor.WeaponCooldown);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 52)..], actor.Pitch);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 56)..], actor.UseHeld ? 1 : 0);
            cursor += 60;
        }

        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Sectors.Count);
        cursor += 4;
        foreach (var sector in state.Sectors)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], Fixed.FromDouble(sector.Floor).Raw);
            BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], Fixed.FromDouble(sector.Ceiling).Raw);
            cursor += 8;
        }

        BinaryPrimitives.WriteUInt32LittleEndian(span[cursor..], state.CombatRandomState ?? 0);
        BinaryPrimitives.WriteInt32LittleEndian(span[(cursor + 4)..], state.CombatRandomState.HasValue ? 1 : 0);
        if (!hasWalls && !hasPlanes) return buffer;
        cursor += 8;
        BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Walls?.Count ?? -1);
        cursor += 4;
        if (state.Walls is { } walls)
            foreach (var wall in walls)
            {
                wall.Write(span[cursor..]);
                cursor += SimWallTransform.Size;
            }
        if (hasPlanes)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], state.Planes?.Count ?? -1);
            cursor += 4;
            if (state.Planes is { } planes) foreach (var plane in planes)
            {
                plane.Write(span[cursor..]);
                cursor += SimPlaneTransform.Size;
            }
        }
        if (state.TextureScrolls is { } scrolls)
        {
            BinaryPrimitives.WriteInt32LittleEndian(span[cursor..], scrolls.Count);
            cursor += 4;
            foreach (var scroll in scrolls)
            {
                scroll.Write(span[cursor..], state.IncludesCarryScrolls);
                cursor += scrollSize;
            }
        }
        if (state.GeometryHealth is { } health)
        {
            var trailer = health.Write();
            var archive = new byte[checked(buffer.Length + trailer.Length)];
            buffer.CopyTo(archive, 0);
            trailer.CopyTo(archive, buffer.Length);
            BinaryPrimitives.WriteUInt16LittleEndian(archive.AsSpan(4), 15);
            archive = SimProjectileFlagArchive.Write(state, SimPainDeathArchive.Write(state, WriteDeathFlags(state, WriteFloatFlags(state, WriteContactFlags(state, WritePickups(state, WriteRolls(state, archive)))))));
            archive = SimFloorHuggerArchive.Write(state, SimCeilingHuggerArchive.Write(state, SimProjectilePointerArchive.Write(state, SimProjectileLifetimeArchive.Write(state, archive))));
            archive = SimPickupDelayArchive.Write(state, SimPlayerHealthArchive.Write(state, SimSpectralArchive.Write(state, SimHitOwnerArchive.Write(state, SimNonShootableArchive.Write(state, SimGhostArchive.Write(state, SimThruBitsArchive.Write(state, SimThruSpeciesArchive.Write(state, SimMissileThruSpeciesArchive.Write(state, SimThruActorsArchive.Write(state, SimBlastEligibilityArchive.Write(state, SimBlastedArchive.Write(state, archive))))))))))));
            archive = SimArmorTypeArchive.Write(state, SimMeleeFlagArchive.Write(state, SimProjectilePassHeightArchive.Write(state, SimRipArchive.Write(state, SimDamageTypeArchive.Write(state, SimActorDamageFactorArchive.Write(state, SimActorGroupArchive.Write(state, SimDamagePowerArchive.Write(state, SimDeathTypeArchive.Write(state, SimActorSpecialArchive.Write(state, archive))))))))));
            archive = SimMasterPointerArchive.Write(state, SimUseSpecialArchive.Write(state, SimUseRangeArchive.Write(state, SimJumpRandomArchive.Write(state, SimLightAnimationArchive.Write(state, SimLightArchive.Write(state, SimFloatBobPhaseArchive.Write(state, SimSpriteOrientationArchive.Write(state, SimScaleArchive.Write(state, SimSpeciesArchive.Write(state, SimSizeArchive.Write(state, SimTeleFogArchive.Write(state, SimTuningArchive.Write(state, SimDefensePropertiesArchive.Write(state, SimChaseThresholdArchive.Write(state, SimGravityArchive.Write(state, SimTargetMemoryArchive.Write(state, SimMovementActionArchive.Write(state, SimKilledFlagArchive.Write(state, archive)))))))))))))))))));
            archive = SimDontFallArchive.Write(state, SimFallingFlagArchive.Write(state, SimDontCorpseArchive.Write(state, SimCorpseFlagArchive.Write(state, SimNoFrictionArchive.Write(state, SimFlyFlagArchive.Write(state, SimClassicFlightArchive.Write(state, SimJumpCooldownArchive.Write(state, SimCrouchArchive.Write(state, SimMonsterBlockingArchive.Write(state, SimVisibilityArchive.Write(state, SimFloorClipArchive.Write(state, SimBlockmapFlagArchive.Write(state, SimFriendshipArchive.Write(state, archive))))))))))))));
            archive = SimReactionTimeArchive.Write(state, SimSkullChargeArchive.Write(state, SimAllowDropOffArchive.Write(state, archive)));
            archive = SimSlideFlagArchive.Write(state, SimFreezeDeathRandomArchive.Write(state, SimDropItemRandomArchive.Write(state, SimFreezeChunksArchive.Write(state, SimIceChunkArchive.Write(state, SimIceTicsArchive.Write(state, SimMapSpawnArchive.Write(state, SimStateRandomArchive.Write(state, SimNoDelayArchive.Write(state, SimFastModeArchive.Write(state, SimFullBrightArchive.Write(state, SimGoalPointerArchive.Write(state, SimJustHitArchive.Write(state, SimActorDamageArchive.Write(state, SimMeleeRangeArchive.Write(state, archive)))))))))))))));
            archive = SimSpawnSoundSourceArchive.Write(state, SimActorRenderArchive.Write(state, SimDontThrustArchive.Write(state, SimSeekVisibilityArchive.Write(state, SimDontSeekInvisibleArchive.Write(state, SimCantSeekArchive.Write(state, SimAmbushArchive.Write(state, SimInfiniteAmmoPowerArchive.Write(state, SimAmmoArchive.Write(state, SimHealthPickupArchive.Write(state, SimPlayerPoisonArchive.Write(state, SimPlayerPoisonCountArchive.Write(state, SimPlayerDamageCountArchive.Write(state, SimPainChanceRandomArchive.Write(state, SimPoisonDamageRandomArchive.Write(state, SimPlayerGodModeArchive.Write(state, SimPlayerBuddha2Archive.Write(state, SimPlayerGodMode2Archive.Write(state, SimPlayerDrainArchive.Write(state, SimLaxTelefragDamageArchive.Write(state, SimSelfDamageFactorArchive.Write(state, SimNoDamageArchive.Write(state, SimAllowPainArchive.Write(state, SimPainTypeArchive.Write(state, SimCausePainArchive.Write(state, SimOnlySlamSolidArchive.Write(state, SimRetargetAfterSlamArchive.Write(state, SimMissileDamageRandomArchive.Write(state, SimLostSoulAttackArchive.Write(state, SimArchvileAttackArchive.Write(state, SimRevenantAttackArchive.Write(state, SimProfileAttackArchive.Write(state, SimBrainDisabledArchive.Write(state, SimRepeatAttackArchive.Write(state, SimCPosRefireRandomArchive.Write(state, SimCPosAttackRandomArchive.Write(state, SimSPosAttackRandomArchive.Write(state, SimPosAttackRandomArchive.Write(state, SimBruisAttackRandomArchive.Write(state, SimHeadAttackRandomArchive.Write(state, SimTroopAttackRandomArchive.Write(state, SimSargAttackRandomArchive.Write(state, SimSkelFistRandomArchive.Write(state, SimChaseRandomArchive.Write(state, SimFullVolumeActiveArchive.Write(state, SimDeathDamageArchive.Write(state, SimPlayerDamageTypeArchive.Write(state, SimFullVolumeSeeArchive.Write(state, SimFullVolumeDeathArchive.Write(state, SimActorSoundArchive.Write(state, SimLevelGravityArchive.Write(state, SimAirControlArchive.Write(state, SimFrictionArchive.Write(state, archive)))))))))))))))))))))))))))))))))))))))))))))))))))));
            archive = SimSpawnDeathArchive.Write(state, SimIsPuffArchive.Write(state, SimNoBloodArchive.Write(state, SimPuffRandomArchive.Write(state, SimMapMonsterArchive.Write(state, SimMapDecorationArchive.Write(state, SimPuffArchive.Write(state, SimMapPickupArchive.Write(state, SimDroppedPickupArchive.Write(state, SimThinkerOrderArchive.Write(state, SimInvasionArchive.Write(state, SimActorDamageHistoryArchive.Write(state, SimBotProfileArchive.Write(state, SimProfileChaseArchive.Write(state, SimBasicAttackArchive.Write(state, SimBotAllocatorArchive.Write(state, SimBotIdentityArchive.Write(state, SimSkyExplodeArchive.Write(state, SimMissileHitPointerFlagsArchive.Write(state, SimExplodeCountArchive.Write(state, SimDontOverlapArchive.Write(state, SimCanPassArchive.Write(state, SimFloorCarryArchive.Write(state, SimMoveWithSectorArchive.Write(state, SimSectorLinkArchive.Write(state, SimNullableProjectilePointerArchive.Write(state, SimTracerPointerArchive.Write(state, archive)))))))))))))))))))))))))));
            archive = SimGibHealthArchive.Write(state, SimGibFactorArchive.Write(state, SimActorDieRandomArchive.Write(state, archive)));
            archive = SimSectorSoundArchive.Write(state, SimNoSplashAlertArchive.Write(state, SimInCombatArchive.Write(state, SimJustAttackedArchive.Write(state, SimChaseGoalArchive.Write(state, SimMonsterClassificationArchive.Write(state, SimNoTargetArchive.Write(state, SimCombatBehaviorArchive.Write(state, SimTargetSwitchFlagsArchive.Write(state, SimInfightingFlagsArchive.Write(state, SimActorScoreArchive.Write(state, SimSpawnBobPhaseArchive.Write(state, SimPlayerSearchArchive.Write(state, SimPlayerLookArchive.Write(state, SimMVisBlockedArchive.Write(state, SimHateSearchArchive.Write(state, SimHateTargetingArchive.Write(state, SimVulnerableArchive.Write(state, archive))))))))))))))))));
            archive = SimPlayerBonusArchive.Write(state, SimPowerStrengthArchive.Write(state, SimPunchRandomArchive.Write(state, SimSawRandomArchive.Write(state, SimPlayerExtraLightArchive.Write(state, archive)))));
            archive = SimPickupPresentationArchive.Write(state, archive);
            archive = SimPickupAttenuationArchive.Write(state, archive);
            archive = SimPickupHealthArchive.Write(state, archive);
            archive = SimPickupAutoUseArchive.Write(state, archive);
            archive = SimDoorMotionArchive.Write(state, archive);
            archive = SimSectorMotionArchive.Write(state, archive);
            archive = SimAccuracyArchive.Write(state, archive);
            archive = SimSoundClassArchive.Write(state, archive);
            archive = SimFriendlySeeBlocksArchive.Write(state, archive);
            archive = SimSkipTargetRandomArchive.Write(state, archive);
            archive = SimSeeFriendlyMonstersArchive.Write(state, archive);
            archive = SimLookBehaviorArchive.Write(state, archive);
            archive = SimConversationArchive.Write(state, archive);
            archive = SimWanderArchive.Write(state, archive);
            archive = SimDontFollowPlayersArchive.Write(state, archive);
            archive = SimCanJumpArchive.Write(state, archive);
            archive = SimFearFlagsArchive.Write(state, archive);
            archive = SimCantLeaveFloorPicArchive.Write(state, archive);
            archive = SimMeleeAvoidanceArchive.Write(state, archive);
            archive = SimAvoidHazardsArchive.Write(state, archive);
            archive = SimAvoidCrushRandomArchive.Write(state, archive);
            archive = SimStayOnLiftArchive.Write(state, archive);
            archive = SimStayOnLiftRandomArchive.Write(state, archive);
            archive = SimLineActionsArchive.Write(state, archive);
            archive = SimFloorClipFlagsArchive.Write(state, archive);
            archive = SimBlockAsPlayerArchive.Write(state, archive);
            archive = SimScaredyCatRandomArchive.Write(state, archive);
            archive = SimCheckMissileRandomArchive.Write(state, archive);
            archive = SimFastChaseArchive.Write(state, archive);
            archive = SimMinMissileChanceArchive.Write(state, archive);
            archive = SimMissileChanceMultArchive.Write(state, archive);
            archive = SimMaxTargetRangeArchive.Write(state, archive);
            archive = SimMeleeThresholdArchive.Write(state, archive);
            archive = SimDefectRandomArchive.Write(state, archive);
            archive = SimCheckSightRandomArchive.Write(state, archive);
            archive = SimSwitchTargetRandomArchive.Write(state, archive);
            archive = SimNativeSwitchTargetRandomArchive.Write(state, archive);
            archive = SimPlayerNoTargetCheatArchive.Write(state, archive);
            archive = SimBuddhaArchive.Write(state, archive);
            archive = SimFlyCheatArchive.Write(state, archive);
            archive = SimPlayerFrozenArchive.Write(state, archive);
            archive = SimNoClipArchive.Write(state, archive);
            archive = SimPlayerNoClip2Archive.Write(state, archive);
            archive = SimIronFeetArchive.Write(state, archive);
            archive = SimSectorDamageRandomArchive.Write(state, archive);
            archive = SimIronFeetModeArchive.Write(state, archive);
            archive = SimPowerMaskArchive.Write(state, archive);
            archive = SimUniqueTidRandomArchive.Write(state, archive);
            archive = SimCameraHeightArchive.Write(state, archive);
            archive = SimDontSplashArchive.Write(state, archive);
            archive = SimPlayerRespawnArchive.Write(state, archive);
            archive = SimTouchyImpactArchive.Write(state, archive);
            archive = SimDontHarmSpeciesArchive.Write(state, archive);
            archive = SimDontHarmClassArchive.Write(state, archive);
            archive = SimDontGibArchive.Write(state, archive);
            archive = SimCrushStateArchive.Write(state, archive);
            archive = SimTeleStompArchive.Write(state, archive);
            archive = SimTeleportFreezeArchive.Write(state, archive);
            archive = SimCrashedArchive.Write(state, archive);
            archive = SimCrashStateArchive.Write(state, archive);
            archive = SimExtremeCrashStateArchive.Write(state, archive);
            archive = SimLandingSpeedArchive.Write(state, archive);
            archive = SimSmashableArchive.Write(state, archive);
            archive = SimDeltaViewHeightArchive.Write(state, archive);
            archive = SimGruntSpeedArchive.Write(state, archive);
            archive = SimFallDamageArchive.Write(state, archive);
            archive = SimForcedFallingArchive.Write(state, archive);
            archive = SimPushHistoryArchive.Write(state, archive);
            archive = SimBlockedBySolidActorsArchive.Write(state, archive);
            archive = SimBumpSpecialArchive.Write(state, archive);
            archive = SimReflectionArchive.Write(state, archive);
            archive = SimReflectRandomArchive.Write(state, archive);
            archive = SimSeekerMissileArchive.Write(state, archive);
            archive = SimFloatBobArchive.Write(state, archive);
            archive = SimSeekerRandomArchive.Write(state, archive);
            archive = SimSummonedMonsterArchive.Write(state, archive);
            archive = SimScreenSeekerArchive.Write(state, archive);
            archive = SimVisibilityPulseArchive.Write(state, archive);
            archive = SimStealthArchive.Write(state, archive);
            archive = SimStealthDefaultsArchive.Write(state, archive);
            archive = SimProjectileSecondaryArchive.Write(state, archive);
            archive = SimFloatBobDefaultsArchive.Write(state, archive);
            archive = SimSeekerDefaultsArchive.Write(state, archive);
            archive = SimReflectiveDefaultsArchive.Write(state, archive);
            archive = SimThruGhostDefaultsArchive.Write(state, archive);
            archive = SimNonShootableDefaultsArchive.Write(state, archive);
            archive = SimFloorPicDefaultsArchive.Write(state, archive);
            archive = SimRipDefaultsArchive.Write(state, archive);
            archive = SimPushDefaultsArchive.Write(state, archive);
            archive = SimTeleStompDefaultsArchive.Write(state, archive);
            archive = SimFloorClipDefaultsArchive.Write(state, archive);
            archive = SimBlastedDefaultsArchive.Write(state, archive);
            archive = SimNoTeleportOverrideArchive.Write(state, archive);
            archive = SimTouchyDefaultsArchive.Write(state, archive);
            archive = SimNoClipDefaultsArchive.Write(state, archive);
            archive = SimCombatDefaultsArchive.Write(state, archive);
            archive = SimInFloatDefaultsArchive.Write(state, archive);
            archive = SimCorpseDefaultsArchive.Write(state, archive);
            archive = SimSpawnCeilingOverrideArchive.Write(state, archive);
            archive = SimFlyDefaultsArchive.Write(state, archive);
            archive = SimSpawnFloatArchive.Write(state, archive);
            archive = SimFloatSpawnRandomArchive.Write(state, archive);
            archive = SimWeaponRefiringArchive.Write(state, SimVerticalWeaponSpreadArchive.Write(state, SimGunShotRandomArchive.Write(state, SimFireSg2RandomArchive.Write(state, SimPuffGetsOwnerArchive.Write(state, SimBfgSprayRandomArchive.Write(state, SimBfgSelfDamageRandomArchive.Write(state, SimRemappedDecorationArchive.Write(state, SimPainChanceOverrideArchive.Write(state, archive)))))))));
            return SimPlayerInputArchive.Write(state, SimSectorTerrainArchive.Write(state, archive));
        }
        return buffer;
    }

    private static byte[] WriteRolls(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.Roll.HasValue)) return archive;
        var size = checked(8 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var offset = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset + 4 + i * 4), state.Actors[i].Roll!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 16);
        return bytes;
    }

    internal static void ValidatePickups(SimSaveState state)
    {
        if (state.Actors.Any(actor => actor.Pickup is { Amount: < 0 }))
            throw new InvalidOperationException("Saved pickup amount cannot be negative.");
    }

    internal static void ValidateContactFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.ContactFlags.HasValue)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.ContactFlags.HasValue || actor.ContactFlags.Value is < 0 or > 3))
            throw new InvalidOperationException("Saved contact flags require a complete current archive.");
    }

    private static byte[] WriteContactFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.ContactFlags.HasValue)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].ContactFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 18);
        return bytes;
    }

    internal static void ValidateFloatFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.FloatFlags.HasValue)) return;
        if (state.Actors.All(actor => actor.FloatFlags == 0)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.ContactFlags.HasValue) || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.FloatFlags.HasValue || actor.FloatFlags.Value is < 0 or > 3))
            throw new InvalidOperationException("Saved float flags require a complete current archive.");
    }

    private static byte[] WriteFloatFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.FloatFlags is > 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].FloatFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 19);
        return bytes;
    }

    internal static void ValidateDeathFlags(SimSaveState state)
    {
        if (!state.Actors.Any(actor => actor.DeathFlags.HasValue)) return;
        if (state.Actors.All(actor => actor.DeathFlags == 0)) return;
        if (state.GeometryHealth is null || state.Actors.Any(actor => !actor.ContactFlags.HasValue) || state.Actors.Any(actor => !actor.Roll.HasValue
            || !actor.DeathFlags.HasValue || actor.DeathFlags.Value is < 0 or > 1))
            throw new InvalidOperationException("Saved death flags require a complete current archive.");
    }

    private static byte[] WriteDeathFlags(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.DeathFlags is > 0)) return archive;
        var size = checked(12 + state.Actors.Count * 4);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), BinaryPrimitives.ReadUInt16LittleEndian(archive.AsSpan(4)));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 4), state.Actors[i].DeathFlags!.Value);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 20);
        return bytes;
    }

    private static byte[] WritePickups(SimSaveState state, byte[] archive)
    {
        if (!state.Actors.Any(actor => actor.Pickup.HasValue)) return archive;
        var size = checked(8 + state.Actors.Count * 8);
        var bytes = new byte[checked(archive.Length + size)];
        archive.CopyTo(bytes, 0);
        var start = archive.Length;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start), state.Actors.Count);
        for (var i = 0; i < state.Actors.Count; i++)
        {
            if (state.Actors[i].Pickup is not { } pickup) continue;
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 4 + i * 8), pickup.Amount);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 8 + i * 8),
                4 | (pickup.IgnoreSkill ? 1 : 0) | (pickup.Depleted ? 2 : 0));
        }
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 17);
        return bytes;
    }

    public static bool TryRead(ReadOnlySpan<byte> bytes, out SimSaveState state, out string? error)
    {
        state = new SimSaveState();
        error = null;
        if (bytes.Length < 16 || !bytes.StartsWith(Magic))
        {
            error = "save-magic";
            return false;
        }

        var version = BinaryPrimitives.ReadUInt16LittleEndian(bytes[4..]);
        if (version is 46 or 47 or 48 or 49 or 50 or 53 or 54) return SimDeathTypeArchive.TryRead(bytes, out state, out error);
        if (version is 51 or 52) return SimDamagePowerArchive.TryRead(bytes, out state, out error);
        if (version == 55) return SimActorGroupArchive.TryRead(bytes, out state, out error);
        if (version == 56) return SimActorDamageFactorArchive.TryRead(bytes, out state, out error);
        if (version == 57) return SimDamageTypeArchive.TryRead(bytes, out state, out error);
        if (version is 58 or 59 or 60) return SimRipArchive.TryRead(bytes, out state, out error);
        if (version is 63 or 64 or 65 or 66) return SimArmorTypeArchive.TryRead(bytes, out state, out error);
        if (version == 67) return SimKilledFlagArchive.TryRead(bytes, out state, out error);
        if (version == 68) return SimMovementActionArchive.TryRead(bytes, out state, out error);
        if (version == 69) return SimTargetMemoryArchive.TryRead(bytes, out state, out error);
        if (version == 70) return SimGravityArchive.TryRead(bytes, out state, out error);
        if (version == 71) return SimChaseThresholdArchive.TryRead(bytes, out state, out error);
        if (version == 72) return SimDefensePropertiesArchive.TryRead(bytes, out state, out error);
        if (version == 73) return SimTuningArchive.TryRead(bytes, out state, out error);
        if (version == 74) return SimTeleFogArchive.TryRead(bytes, out state, out error);
        if (version == 75) return SimSizeArchive.TryRead(bytes, out state, out error);
        if (version == 76) return SimSpeciesArchive.TryRead(bytes, out state, out error);
        if (version == 77) return SimScaleArchive.TryRead(bytes, out state, out error);
        if (version == 78) return SimSpriteOrientationArchive.TryRead(bytes, out state, out error);
        if (version == 79) return SimFloatBobPhaseArchive.TryRead(bytes, out state, out error);
        if (version == 80) return SimLightArchive.TryRead(bytes, out state, out error);
        if (version == 81) return SimLightAnimationArchive.TryRead(bytes, out state, out error);
        if (version == 82) return SimJumpRandomArchive.TryRead(bytes, out state, out error);
        if (version == 83) return SimUseRangeArchive.TryRead(bytes, out state, out error);
        if (version == 84) return SimUseSpecialArchive.TryRead(bytes, out state, out error);
        if (version == 85) return SimMasterPointerArchive.TryRead(bytes, out state, out error);
        if (version == 86) return SimFriendshipArchive.TryRead(bytes, out state, out error);
        if (version == 87) return SimBlockmapFlagArchive.TryRead(bytes, out state, out error);
        if (version == 88) return SimFloorClipArchive.TryRead(bytes, out state, out error);
        if (version == 89) return SimVisibilityArchive.TryRead(bytes, out state, out error);
        if (version == 100) return SimAllowDropOffArchive.TryRead(bytes, out state, out error);
        if (version == 101) return SimSkullChargeArchive.TryRead(bytes, out state, out error);
        if (version == 102) return SimReactionTimeArchive.TryRead(bytes, out state, out error);
        if (version == 103) return SimMeleeRangeArchive.TryRead(bytes, out state, out error);
        if (version == 104) return SimActorDamageArchive.TryRead(bytes, out state, out error);
        if (version == 105) return SimJustHitArchive.TryRead(bytes, out state, out error);
        if (version == 106) return SimGoalPointerArchive.TryRead(bytes, out state, out error);
        if (version == 107) return SimFullBrightArchive.TryRead(bytes, out state, out error);
        if (version == 108) return SimFastModeArchive.TryRead(bytes, out state, out error);
        if (version == 109) return SimNoDelayArchive.TryRead(bytes, out state, out error);
        if (version == 110) return SimStateRandomArchive.TryRead(bytes, out state, out error);
        if (version == 111) return SimStateRandomArchive.TryRead(bytes, out state, out error);
        if (version == 112) return SimMapSpawnArchive.TryRead(bytes, out state, out error);
        if (version == 113) return SimIceTicsArchive.TryRead(bytes, out state, out error);
        if (version == 114) return SimIceChunkArchive.TryRead(bytes, out state, out error);
        if (version == 115) return SimFreezeChunksArchive.TryRead(bytes, out state, out error);
        if (version == 119) return SimFrictionArchive.TryRead(bytes, out state, out error);
        if (version == 120) return SimAirControlArchive.TryRead(bytes, out state, out error);
        if (version == 122) return SimActorSoundArchive.TryRead(bytes, out state, out error);
        if (version == 124) return SimFullVolumeSeeArchive.TryRead(bytes, out state, out error);
        if (version == 125) return SimPlayerDamageTypeArchive.TryRead(bytes, out state, out error);
        if (version == 126) return SimDeathDamageArchive.TryRead(bytes, out state, out error);
        if (version == 127) return SimFullVolumeActiveArchive.TryRead(bytes, out state, out error);
        if (version == 128) return SimChaseRandomArchive.TryRead(bytes, out state, out error);
        if (version == 129) return SimSkelFistRandomArchive.TryRead(bytes, out state, out error);
        if (version == 146) return SimOnlySlamSolidArchive.TryRead(bytes, out state, out error);
        if (version == 147) return SimCausePainArchive.TryRead(bytes, out state, out error);
        if (version == 148) return SimPainTypeArchive.TryRead(bytes, out state, out error);
        if (version == 149) return SimAllowPainArchive.TryRead(bytes, out state, out error);
        if (version == 150) return SimNoDamageArchive.TryRead(bytes, out state, out error);
        if (version == 151) return SimSelfDamageFactorArchive.TryRead(bytes, out state, out error);
        if (version == 152) return SimLaxTelefragDamageArchive.TryRead(bytes, out state, out error);
        if (version == 153) return SimPlayerDrainArchive.TryRead(bytes, out state, out error);
        if (version == 182) return SimAmbushArchive.TryRead(bytes, out state, out error);
        if (version == 183) return SimCantSeekArchive.TryRead(bytes, out state, out error);
        if (version == 194) return SimCanPassArchive.TryRead(bytes, out state, out error);
        if (version == 195) return SimDontOverlapArchive.TryRead(bytes, out state, out error);
        if (version == 196) return SimExplodeCountArchive.TryRead(bytes, out state, out error);
        if (version == 197) return SimMissileHitPointerFlagsArchive.TryRead(bytes, out state, out error);
        if (version == 198) return SimSkyExplodeArchive.TryRead(bytes, out state, out error);
        if (version == 199) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 200) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 201) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 202) return SimBotAllocatorArchive.TryRead(bytes, out state, out error);
        if (version == 203) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 204) return SimBasicAttackArchive.TryRead(bytes, out state, out error);
        if (version == 205) return SimBasicAttackArchive.TryRead(bytes, out state, out error);
        if (version == 206) return SimProfileChaseArchive.TryRead(bytes, out state, out error);
        if (version == 207) return SimProfileChaseArchive.TryRead(bytes, out state, out error);
        if (version == 208) return SimProfileChaseArchive.TryRead(bytes, out state, out error);
        if (version == 209) return SimBasicAttackArchive.TryRead(bytes, out state, out error);
        if (version == 210) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 211) return SimBotProfileArchive.TryRead(bytes, out state, out error);
        if (version == 212) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 213) return SimBotIdentityArchive.TryRead(bytes, out state, out error);
        if (version == 214) return SimActorDamageHistoryArchive.TryRead(bytes, out state, out error);
        if (version == 215) return SimInvasionArchive.TryRead(bytes, out state, out error);
        if (version == 216) return SimThinkerOrderArchive.TryRead(bytes, out state, out error);
        if (version == 217) return SimDroppedPickupArchive.TryRead(bytes, out state, out error);
        if (version == 260) return SimSoundClassArchive.TryRead(bytes, out state, out error);
        if (version == 259) return SimAccuracyArchive.TryRead(bytes, out state, out error);
        if (version == 261) return SimFriendlySeeBlocksArchive.TryRead(bytes, out state, out error);
        if (version == 262) return SimSkipTargetRandomArchive.TryRead(bytes, out state, out error);
        if (version == 263) return SimSeeFriendlyMonstersArchive.TryRead(bytes, out state, out error);
        if (version == 264) return SimLookBehaviorArchive.TryRead(bytes, out state, out error);
        if (version == 265) return SimConversationArchive.TryRead(bytes, out state, out error);
        if (version == 266) return SimWanderArchive.TryRead(bytes, out state, out error);
        if (version == 267) return SimDontFollowPlayersArchive.TryRead(bytes, out state, out error);
        if (version == 268) return SimCanJumpArchive.TryRead(bytes, out state, out error);
        if (version == 269) return SimFearFlagsArchive.TryRead(bytes, out state, out error);
        if (version == 270) return SimCantLeaveFloorPicArchive.TryRead(bytes, out state, out error);
        if (version == 271) return SimMeleeAvoidanceArchive.TryRead(bytes, out state, out error);
        if (version == 272) return SimAvoidHazardsArchive.TryRead(bytes, out state, out error);
        if (version == 273) return SimAvoidCrushRandomArchive.TryRead(bytes, out state, out error);
        if (version == 274) return SimStayOnLiftArchive.TryRead(bytes, out state, out error);
        if (version == 275) return SimStayOnLiftRandomArchive.TryRead(bytes, out state, out error);
        if (version == 276) return SimLineActionsArchive.TryRead(bytes, out state, out error);
        if (version == 277) return SimFloorClipFlagsArchive.TryRead(bytes, out state, out error);
        if (version == 278) return SimBlockAsPlayerArchive.TryRead(bytes, out state, out error);
        if (version == 279) return SimScaredyCatRandomArchive.TryRead(bytes, out state, out error);
        if (version == 280) return SimCheckMissileRandomArchive.TryRead(bytes, out state, out error);
        if (version == 281) return SimFastChaseArchive.TryRead(bytes, out state, out error);
        if (version == 282) return SimMinMissileChanceArchive.TryRead(bytes, out state, out error);
        if (version == 283) return SimMissileChanceMultArchive.TryRead(bytes, out state, out error);
        if (version == 284) return SimMaxTargetRangeArchive.TryRead(bytes, out state, out error);
        if (version == 285) return SimMeleeThresholdArchive.TryRead(bytes, out state, out error);
        if (version == 286) return SimDefectRandomArchive.TryRead(bytes, out state, out error);
        if (version == 287) return SimCheckSightRandomArchive.TryRead(bytes, out state, out error);
        if (version == 288) return SimSwitchTargetRandomArchive.TryRead(bytes, out state, out error);
        if (version == 289) return SimNativeSwitchTargetRandomArchive.TryRead(bytes, out state, out error);
        if (version == 290) return SimPlayerNoTargetCheatArchive.TryRead(bytes, out state, out error);
        if (version == 291) return SimBuddhaArchive.TryRead(bytes, out state, out error);
        if (version == 292) return SimFlyCheatArchive.TryRead(bytes, out state, out error);
        if (version == 293) return SimPlayerFrozenArchive.TryRead(bytes, out state, out error);
        if (version == 294) return SimNoClipArchive.TryRead(bytes, out state, out error);
        if (version == 301) return SimCameraHeightArchive.TryRead(bytes, out state, out error);
        if (version == 302) return SimDontSplashArchive.TryRead(bytes, out state, out error);
        if (version == 303) return SimPlayerRespawnArchive.TryRead(bytes, out state, out error);
        if (version == 304) return SimTouchyImpactArchive.TryRead(bytes, out state, out error);
        if (version == 305) return SimDontHarmSpeciesArchive.TryRead(bytes, out state, out error);
        if (version == 306) return SimDontHarmClassArchive.TryRead(bytes, out state, out error);
        if (version == 307) return SimDontGibArchive.TryRead(bytes, out state, out error);
        if (version == 308) return SimCrushStateArchive.TryRead(bytes, out state, out error);
        if (version == 309) return SimTeleStompArchive.TryRead(bytes, out state, out error);
        if (version == 312) return SimCrashStateArchive.TryRead(bytes, out state, out error);
        if (version == 320) return SimPushHistoryArchive.TryRead(bytes, out state, out error);
        if (version == 321) return SimBlockedBySolidActorsArchive.TryRead(bytes, out state, out error);
        if (version == 322) return SimBumpSpecialArchive.TryRead(bytes, out state, out error);
        if (version == 323) return SimReflectionArchive.TryRead(bytes, out state, out error);
        if (version == 324) return SimReflectRandomArchive.TryRead(bytes, out state, out error);
        if (version == 325) return SimSeekerMissileArchive.TryRead(bytes, out state, out error);
        if (version == 326) return SimFloatBobArchive.TryRead(bytes, out state, out error);
        if (version == 327) return SimSeekerRandomArchive.TryRead(bytes, out state, out error);
        if (version == 328) return SimSummonedMonsterArchive.TryRead(bytes, out state, out error);
        if (version == 329) return SimScreenSeekerArchive.TryRead(bytes, out state, out error);
        if (version == 330) return SimVisibilityPulseArchive.TryRead(bytes, out state, out error);
        if (version == 331) return SimStealthArchive.TryRead(bytes, out state, out error);
        if (version == 332) return SimStealthDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 333) return SimProjectileSecondaryArchive.TryRead(bytes, out state, out error);
        if (version == 334) return SimFloatBobDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 335) return SimSeekerDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 336) return SimReflectiveDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 337) return SimThruGhostDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 338) return SimNonShootableDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 339) return SimFloorPicDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 340) return SimRipDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 341) return SimPushDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 342) return SimTeleStompDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 343) return SimFloorClipDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 344) return SimBlastedDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 345) return SimNoTeleportOverrideArchive.TryRead(bytes, out state, out error);
        if (version == 346) return SimTouchyDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 347) return SimNoClipDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 348) return SimCombatDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 349) return SimInFloatDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 350) return SimCorpseDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 351) return SimSpawnCeilingOverrideArchive.TryRead(bytes, out state, out error);
        if (version == 352) return SimFlyDefaultsArchive.TryRead(bytes, out state, out error);
        if (version == 353) return SimSpawnFloatArchive.TryRead(bytes, out state, out error);
        if (version == 354) return SimFloatSpawnRandomArchive.TryRead(bytes, out state, out error);
        if (version == 356) return SimRemappedDecorationArchive.TryRead(bytes, out state, out error);
        if (version == 355) return SimPainChanceOverrideArchive.TryRead(bytes, out state, out error);
        if (version == 319) return SimForcedFallingArchive.TryRead(bytes, out state, out error);
        if (version == 318) return SimFallDamageArchive.TryRead(bytes, out state, out error);
        if (version == 317) return SimGruntSpeedArchive.TryRead(bytes, out state, out error);
        if (version == 316) return SimDeltaViewHeightArchive.TryRead(bytes, out state, out error);
        if (version == 315) return SimSmashableArchive.TryRead(bytes, out state, out error);
        if (version == 314) return SimLandingSpeedArchive.TryRead(bytes, out state, out error);
        if (version == 313) return SimExtremeCrashStateArchive.TryRead(bytes, out state, out error);
        if (version == 311) return SimCrashedArchive.TryRead(bytes, out state, out error);
        if (version == 310) return SimTeleportFreezeArchive.TryRead(bytes, out state, out error);
        if (version == 300) return SimUniqueTidRandomArchive.TryRead(bytes, out state, out error);
        if (version == 299) return SimPowerMaskArchive.TryRead(bytes, out state, out error);
        if (version == 298) return SimIronFeetModeArchive.TryRead(bytes, out state, out error);
        if (version == 297) return SimSectorDamageRandomArchive.TryRead(bytes, out state, out error);
        if (version == 296) return SimIronFeetArchive.TryRead(bytes, out state, out error);
        if (version == 295) return SimPlayerNoClip2Archive.TryRead(bytes, out state, out error);
        if (version == 258) return SimSectorMotionArchive.TryRead(bytes, out state, out error);
        if (version == 257) return SimDoorMotionArchive.TryRead(bytes, out state, out error);
        if (version == 256) return SimPickupAutoUseArchive.TryRead(bytes, out state, out error);
        if (version == 255) return SimPickupHealthArchive.TryRead(bytes, out state, out error);
        if (version == 254) return SimPickupAttenuationArchive.TryRead(bytes, out state, out error);
        if (version == 253) return SimPickupPresentationArchive.TryRead(bytes, out state, out error);
        if (version == 252) return SimPlayerBonusArchive.TryRead(bytes, out state, out error);
        if (version == 251) return SimPowerStrengthArchive.TryRead(bytes, out state, out error);
        if (version == 250) return SimPunchRandomArchive.TryRead(bytes, out state, out error);
        if (version == 249) return SimSawRandomArchive.TryRead(bytes, out state, out error);
        if (version == 248) return SimPlayerExtraLightArchive.TryRead(bytes, out state, out error);
        if (version == 247) return SimSectorSoundArchive.TryRead(bytes, out state, out error);
        if (version == 246) return SimNoSplashAlertArchive.TryRead(bytes, out state, out error);
        if (version == 245) return SimInCombatArchive.TryRead(bytes, out state, out error);
        if (version == 244) return SimJustAttackedArchive.TryRead(bytes, out state, out error);
        if (version == 243) return SimChaseGoalArchive.TryRead(bytes, out state, out error);
        if (version == 242) return SimMonsterClassificationArchive.TryRead(bytes, out state, out error);
        if (version == 241) return SimNoTargetArchive.TryRead(bytes, out state, out error);
        if (version == 240) return SimCombatBehaviorArchive.TryRead(bytes, out state, out error);
        if (version == 239) return SimTargetSwitchFlagsArchive.TryRead(bytes, out state, out error);
        if (version == 238) return SimInfightingFlagsArchive.TryRead(bytes, out state, out error);
        if (version == 237) return SimActorScoreArchive.TryRead(bytes, out state, out error);
        if (version == 236) return SimSpawnBobPhaseArchive.TryRead(bytes, out state, out error);
        if (version == 235) return SimPlayerSearchArchive.TryRead(bytes, out state, out error);
        if (version == 234) return SimPlayerLookArchive.TryRead(bytes, out state, out error);
        if (version == 233) return SimMVisBlockedArchive.TryRead(bytes, out state, out error);
        if (version == 232) return SimHateSearchArchive.TryRead(bytes, out state, out error);
        if (version == 231) return SimHateTargetingArchive.TryRead(bytes, out state, out error);
        if (version == 230) return SimVulnerableArchive.TryRead(bytes, out state, out error);
        if (version == 229) return SimGibHealthArchive.TryRead(bytes, out state, out error);
        if (version == 228) return SimGibFactorArchive.TryRead(bytes, out state, out error);
        if (version == 363) return SimWeaponRefiringArchive.TryRead(bytes, out state, out error);
        if (version == 365) return SimPlayerInputArchive.TryRead(bytes, out state, out error);
        if (version == 364) return SimSectorTerrainArchive.TryRead(bytes, out state, out error);
        if (version == 362) return SimVerticalWeaponSpreadArchive.TryRead(bytes, out state, out error);
        if (version == 361) return SimGunShotRandomArchive.TryRead(bytes, out state, out error);
        if (version == 360) return SimFireSg2RandomArchive.TryRead(bytes, out state, out error);
        if (version == 359) return SimPuffGetsOwnerArchive.TryRead(bytes, out state, out error);
        if (version == 358) return SimBfgSprayRandomArchive.TryRead(bytes, out state, out error);
        if (version == 357) return SimBfgSelfDamageRandomArchive.TryRead(bytes, out state, out error);
        if (version == 227) return SimActorDieRandomArchive.TryRead(bytes, out state, out error);
        if (version == 226) return SimSpawnDeathArchive.TryRead(bytes, out state, out error);
        if (version == 225) return SimSpawnDeathArchive.TryRead(bytes, out state, out error, legacyImmediate: true);
        if (version == 224) return SimIsPuffArchive.TryRead(bytes, out state, out error);
        if (version == 223) return SimNoBloodArchive.TryRead(bytes, out state, out error);
        if (version == 222) return SimPuffRandomArchive.TryRead(bytes, out state, out error);
        if (version == 221) return SimMapMonsterArchive.TryRead(bytes, out state, out error);
        if (version == 220) return SimMapDecorationArchive.TryRead(bytes, out state, out error);
        if (version == 219) return SimPuffArchive.TryRead(bytes, out state, out error);
        if (version == 218) return SimMapPickupArchive.TryRead(bytes, out state, out error);
        if (version == 193) return SimFloorCarryArchive.TryRead(bytes, out state, out error);
        if (version == 192) return SimMoveWithSectorArchive.TryRead(bytes, out state, out error);
        if (version == 191) return SimSectorLinkArchive.TryRead(bytes, out state, out error);
        if (version == 190) return SimNullableProjectilePointerArchive.TryRead(bytes, out state, out error);
        if (version == 189) return SimTracerPointerArchive.TryRead(bytes, out state, out error);
        if (version == 188) return SimSpawnSoundSourceArchive.TryRead(bytes, out state, out error);
        if (version == 184) return SimDontSeekInvisibleArchive.TryRead(bytes, out state, out error);
        if (version == 185) return SimSeekVisibilityArchive.TryRead(bytes, out state, out error);
        if (version == 187) return SimActorRenderArchive.TryRead(bytes, out state, out error);
        if (version == 186) return SimDontThrustArchive.TryRead(bytes, out state, out error);
        if (version is 171 or 172 or 173 or 174 or 175 or 176 or 177 or 178 or 179 or 180 or 181) return SimInfiniteAmmoPowerArchive.TryRead(bytes, out state, out error);
        if (version == 154) return SimPlayerGodMode2Archive.TryRead(bytes, out state, out error);
        if (version == 155) return SimPlayerBuddha2Archive.TryRead(bytes, out state, out error);
        if (version == 156) return SimPlayerGodModeArchive.TryRead(bytes, out state, out error);
        if (version == 157) return SimPoisonDamageRandomArchive.TryRead(bytes, out state, out error);
        if (version == 158) return SimPainChanceRandomArchive.TryRead(bytes, out state, out error);
        if (version == 159) return SimPlayerDamageCountArchive.TryRead(bytes, out state, out error);
        if (version == 160) return SimPlayerPoisonCountArchive.TryRead(bytes, out state, out error);
        if (version == 161) return SimPlayerPoisonArchive.TryRead(bytes, out state, out error);
        if (version is 162 or 163 or 164) return SimHealthPickupArchive.TryRead(bytes, out state, out error);
        if (version is 165 or 166 or 167 or 168 or 169 or 170) return SimAmmoArchive.TryRead(bytes, out state, out error);
        if (version == 145) return SimRetargetAfterSlamArchive.TryRead(bytes, out state, out error);
        if (version == 144) return SimMissileDamageRandomArchive.TryRead(bytes, out state, out error);
        if (version == 143) return SimLostSoulAttackArchive.TryRead(bytes, out state, out error);
        if (version == 142) return SimArchvileAttackArchive.TryRead(bytes, out state, out error);
        if (version == 141) return SimRevenantAttackArchive.TryRead(bytes, out state, out error);
        if (version == 140) return SimProfileAttackArchive.TryRead(bytes, out state, out error);
        if (version == 139) return SimBrainDisabledArchive.TryRead(bytes, out state, out error);
        if (version == 138) return SimRepeatAttackArchive.TryRead(bytes, out state, out error);
        if (version == 137) return SimCPosRefireRandomArchive.TryRead(bytes, out state, out error);
        if (version == 136) return SimCPosAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 135) return SimSPosAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 134) return SimPosAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 133) return SimBruisAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 132) return SimHeadAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 131) return SimTroopAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 130) return SimSargAttackRandomArchive.TryRead(bytes, out state, out error);
        if (version == 123) return SimFullVolumeDeathArchive.TryRead(bytes, out state, out error);
        if (version == 121) return SimLevelGravityArchive.TryRead(bytes, out state, out error);
        if (version == 118) return SimSlideFlagArchive.TryRead(bytes, out state, out error);
        if (version == 117) return SimFreezeDeathRandomArchive.TryRead(bytes, out state, out error);
        if (version == 116) return SimDropItemRandomArchive.TryRead(bytes, out state, out error);
        if (version == 99) return SimDontFallArchive.TryRead(bytes, out state, out error);
        if (version == 98) return SimFallingFlagArchive.TryRead(bytes, out state, out error);
        if (version == 97) return SimDontCorpseArchive.TryRead(bytes, out state, out error);
        if (version == 96) return SimCorpseFlagArchive.TryRead(bytes, out state, out error);
        if (version == 95) return SimNoFrictionArchive.TryRead(bytes, out state, out error);
        if (version == 94) return SimFlyFlagArchive.TryRead(bytes, out state, out error);
        if (version == 93) return SimClassicFlightArchive.TryRead(bytes, out state, out error);
        if (version == 92) return SimJumpCooldownArchive.TryRead(bytes, out state, out error);
        if (version == 91) return SimCrouchArchive.TryRead(bytes, out state, out error);
        if (version == 90) return SimMonsterBlockingArchive.TryRead(bytes, out state, out error);
        if (version == 62) return SimMeleeFlagArchive.TryRead(bytes, out state, out error);
        if (version == 61) return SimProjectilePassHeightArchive.TryRead(bytes, out state, out error);
        if (version is 42 or 43 or 44 or 45) return SimActorSpecialArchive.TryRead(bytes, out state, out error);
        if (version is 38 or 39 or 40 or 41) return SimPickupDelayArchive.TryRead(bytes, out state, out error);
        if (version == 37) return SimPlayerHealthArchive.TryRead(bytes, out state, out error);
        if (version == 36) return SimSpectralArchive.TryRead(bytes, out state, out error);
        if (version == 35) return SimHitOwnerArchive.TryRead(bytes, out state, out error);
        if (version == 34) return SimNonShootableArchive.TryRead(bytes, out state, out error);
        if (version == 33) return SimGhostArchive.TryRead(bytes, out state, out error);
        if (version == 32) return SimThruBitsArchive.TryRead(bytes, out state, out error);
        if (version == 31) return SimThruSpeciesArchive.TryRead(bytes, out state, out error);
        if (version == 30) return SimMissileThruSpeciesArchive.TryRead(bytes, out state, out error);
        if (version == 29) return SimThruActorsArchive.TryRead(bytes, out state, out error);
        if (version == 28) return SimBlastEligibilityArchive.TryRead(bytes, out state, out error);
        if (version == 27) return SimBlastedArchive.TryRead(bytes, out state, out error);
        if (version == 26) return SimFloorHuggerArchive.TryRead(bytes, out state, out error);
        if (version == 25) return SimCeilingHuggerArchive.TryRead(bytes, out state, out error);
        if (version == 24) return SimProjectilePointerArchive.TryRead(bytes, out state, out error);
        if (version == 23) return SimProjectileLifetimeArchive.TryRead(bytes, out state, out error);
        if (version == 22) return SimProjectileFlagArchive.TryRead(bytes, out state, out error);
        if (version == 21) return SimPainDeathArchive.TryRead(bytes, out state, out error);
        if (version == 20)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-death-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior is not (18 or 19) || count < 0 || count != (size - 12) / 4)
            { error = "save-death-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-death-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 1)
                { state = new(); error = "save-death-flags"; return false; }
                state.Actors[i].DeathFlags = flags;
            }
            return true;
        }
        if (version == 19)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-float-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior != 18 || count < 0 || count != (size - 12) / 4)
            { error = "save-float-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-float-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 3)
                { state = new(); error = "save-float-flags"; return false; }
                state.Actors[i].FloatFlags = flags;
            }
            return true;
        }
        if (version == 18)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 12 || size > bytes.Length - 16 || (size - 12) % 4 != 0)
            { error = "save-contact-size"; return false; }
            var start = bytes.Length - size;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4)..]);
            if (prior is not (16 or 17) || count < 0 || count != (size - 12) / 4)
            { error = "save-contact-header"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), (ushort)prior);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-contact-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 4)..]);
                if (flags is < 0 or > 3)
                { state = new(); error = "save-contact-flags"; return false; }
                state.Actors[i].ContactFlags = flags;
                state.Actors[i].FloatFlags = 0;
                state.Actors[i].ProjectileFlags = 0;
                state.Actors[i].CeilingFlags = 0;
                state.Actors[i].FloorHuggerFlags = 0;
                state.Actors[i].BlastedFlags = 0;
                state.Actors[i].DeathFlags = 0;
            }
            return true;
        }
        if (version == 17)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 8 || size > bytes.Length - 16 || (size - 8) % 8 != 0)
            { error = "save-pickup-size"; return false; }
            var start = bytes.Length - size;
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            if (count < 0 || count != (size - 8) / 8)
            { error = "save-pickup-count"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 16);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-pickup-count"; return false; }
            for (var i = 0; i < count; i++)
            {
                var amount = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 4 + i * 8)..]);
                var flags = BinaryPrimitives.ReadInt32LittleEndian(bytes[(start + 8 + i * 8)..]);
                if (amount < 0 || (flags & ~7) != 0 || (flags & 4) == 0 && (flags != 0 || amount != 0))
                { state = new(); error = "save-pickup-properties"; return false; }
                if ((flags & 4) != 0)
                    state.Actors[i].Pickup = new(amount, (flags & 1) != 0, (flags & 2) != 0);
            }
            return true;
        }
        if (version == 16)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 8 || size > bytes.Length - 16 || (size - 8) % 4 != 0)
            { error = "save-actor-roll-size"; return false; }
            var start = bytes.Length - size;
            var count = BinaryPrimitives.ReadInt32LittleEndian(bytes[start..]);
            if (count < 0 || count != (size - 8) / 4)
            { error = "save-actor-roll-count"; return false; }
            var legacy = bytes[..start].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 15);
            if (!TryRead(legacy, out state, out error)) return false;
            if (count != state.Actors.Count)
            { state = new(); error = "save-actor-roll-count"; return false; }
            for (var i = 0; i < count; i++)
                state.Actors[i].Roll = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(start + 4 + i * 4)..]);
            return true;
        }
        if (version == 15)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes[^4..]);
            if (size < 16 || size > bytes.Length - 16 || !SimGeometryHealth.TryRead(bytes[^size..], out var health))
            { error = "save-geometry-health"; return false; }
            var legacy = bytes[..^size].ToArray();
            BinaryPrimitives.WriteUInt16LittleEndian(legacy.AsSpan(4), 14);
            if (!TryRead(legacy, out state, out error)) return false;
            state.GeometryHealth = health;
            return true;
        }
        if (version is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 12 or 13 or 14))
        {
            error = "save-version";
            return false;
        }

        var tic = BinaryPrimitives.ReadInt32LittleEndian(bytes[6..]);
        var exited = bytes[10] != 0;
        var secret = bytes[11] != 0;
        var actorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]);
        var actorSize = version == 1 ? 20 : version == 2 ? 48 : version == 3 ? 52 : 60;
        if (actorCount < 0 || actorCount > (bytes.Length - 16) / actorSize)
        {
            error = "save-actor-count";
            return false;
        }

        var cursor = 16;
        var actors = new List<SimActorPose>(actorCount);
        for (var i = 0; i < actorCount; i++)
        {
            if (cursor + actorSize > bytes.Length)
            {
                error = "save-truncated-actor";
                return false;
            }

            actors.Add(new SimActorPose
            {
                Id = BinaryPrimitives.ReadUInt32LittleEndian(bytes[cursor..]),
                X = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..]),
                Y = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 8)..]),
                Angle = BinaryPrimitives.ReadUInt32LittleEndian(bytes[(cursor + 12)..]),
                Health = BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 16)..]),
                Z = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 20)..]) : 0,
                VelocityX = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 24)..]) : 0,
                VelocityY = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 28)..]) : 0,
                VelocityZ = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 32)..]) : 0,
                State = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 36)..]) : 0,
                StateTics = version >= 2 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 40)..]) : -1,
                OnGround = version == 1 || (BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 44)..]) & 1) != 0,
                HasPhysics = version >= 2 && (BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 44)..]) & 2) != 0,
                WeaponCooldown = version >= 3 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 48)..]) : 0,
                Pitch = version >= 4 ? BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 52)..]) : 0,
                UseHeld = version >= 4 && BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 56)..]) != 0,
            });
            cursor += actorSize;
        }

        if (cursor + 4 > bytes.Length)
        {
            error = "save-truncated-sectors";
            return false;
        }

        var sectorCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..]);
        cursor += 4;
        var remaining = bytes.Length - cursor - (version >= 3 ? 8 : 0);
        var sectorSize = version >= 5 ? 8 : 4;
        if (remaining < 0 || sectorCount < 0 || sectorCount > remaining / sectorSize
            || version < 7 && (sectorCount != remaining / sectorSize || remaining % sectorSize != 0))
        {
            error = "save-sector-size";
            return false;
        }

        var footer = version >= 7 ? cursor + sectorCount * sectorSize : bytes.Length - 8;
        List<SimWallTransform>? walls = null;
        List<SimPlaneTransform>? planes = null;
        List<SimTextureScroll>? scrolls = null;
        if (version >= 7)
        {
            if (bytes.Length - footer < 12) { error = "save-truncated-walls"; return false; }
            var wallCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[(footer + 8)..]);
            var wallBytes = bytes.Length - footer - 12;
            if (version >= 8)
            {
                if (wallCount < -1 || wallCount > wallBytes / SimWallTransform.Size)
                { error = "save-wall-size"; return false; }
                var planeStart = footer + 12 + Math.Max(0, wallCount) * SimWallTransform.Size;
                if (bytes.Length - planeStart < 4) { error = "save-truncated-planes"; return false; }
                var planeCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[planeStart..]);
                var planeBytes = bytes.Length - planeStart - 4;
                if (version >= 9)
                {
                    if (planeCount < -1 || planeCount > planeBytes / SimPlaneTransform.Size)
                    { error = "save-plane-size"; return false; }
                    var scrollStart = planeStart + 4 + Math.Max(0, planeCount) * SimPlaneTransform.Size;
                    if (bytes.Length - scrollStart < 4) { error = "save-truncated-scrolls"; return false; }
                    var scrollCount = BinaryPrimitives.ReadInt32LittleEndian(bytes[scrollStart..]);
                    var scrollBytes = bytes.Length - scrollStart - 4;
                    var scrollSize = version >= 10 ? SimTextureScroll.CarrySize : SimTextureScroll.Size;
                    if (scrollCount < 0 || scrollCount != scrollBytes / scrollSize || scrollBytes % scrollSize != 0)
                    { error = "save-scroll-size"; return false; }
                    scrolls = new List<SimTextureScroll>(scrollCount);
                    for (var i = 0; i < scrollCount; i++)
                    {
                        var scroll = SimTextureScroll.Read(bytes[(scrollStart + 4 + i * scrollSize)..], version >= 10);
                        if (!scroll.IsValid || version < 10 && scroll.Kind > 8 || version < 11 && scroll.Kind > 12 || version < 12 && scroll.Kind > 14 || version < 13 && scroll.Kind > 16 || version < 14 && scroll.Kind > 18) { error = "save-scroll-invalid"; return false; }
                        scrolls.Add(scroll);
                    }
                    planeBytes = Math.Max(0, planeCount) * SimPlaneTransform.Size;
                }
                if (planeCount < (version >= 9 ? -1 : 0) || planeCount >= 0
                    && (planeCount != planeBytes / SimPlaneTransform.Size || planeBytes % SimPlaneTransform.Size != 0))
                { error = "save-plane-size"; return false; }
                planes = planeCount < 0 ? null : new List<SimPlaneTransform>(planeCount);
                for (var i = 0; i < planeCount; i++)
                {
                    var plane = SimPlaneTransform.Read(bytes[(planeStart + 4 + i * SimPlaneTransform.Size)..]);
                    if (!plane.IsValid) { error = "save-plane-nonfinite"; return false; }
                    planes!.Add(plane);
                }
                wallBytes = Math.Max(0, wallCount) * SimWallTransform.Size;
            }
            if (wallCount < -1 || wallCount == -1 && wallBytes != 0
                || wallCount >= 0 && (wallCount != wallBytes / SimWallTransform.Size || wallBytes % SimWallTransform.Size != 0))
            { error = "save-wall-size"; return false; }
            if (wallCount >= 0)
            {
                walls = new List<SimWallTransform>(wallCount);
                for (var i = 0; i < wallCount; i++)
                {
                    var wall = SimWallTransform.Read(bytes[(footer + 12 + i * SimWallTransform.Size)..]);
                    if (!wall.IsValid) { error = "save-wall-nonfinite"; return false; }
                    walls.Add(wall);
                }
            }
        }
        state = new SimSaveState { Tic = tic, Exited = exited, SecretExit = secret, Walls = walls, Planes = planes, TextureScrolls = scrolls, IncludesCarryScrolls = version >= 10, IncludesCeilingControls = version >= 11, IncludesFloorControls = version >= 12, IncludesWallControls = version >= 13, IncludesWallParts = version >= 14,
            CombatRandomState = version >= 3 && BinaryPrimitives.ReadInt32LittleEndian(bytes[(footer + 4)..]) != 0
                ? BinaryPrimitives.ReadUInt32LittleEndian(bytes[footer..]) : null };
        state.Actors.AddRange(actors);
        for (var i = 0; i < sectorCount; i++)
        {
            var floor = version >= 5 ? new Fixed(BinaryPrimitives.ReadInt32LittleEndian(bytes[cursor..])).ToDouble()
                : BinaryPrimitives.ReadInt16LittleEndian(bytes[cursor..]);
            var ceiling = version >= 5 ? new Fixed(BinaryPrimitives.ReadInt32LittleEndian(bytes[(cursor + 4)..])).ToDouble()
                : BinaryPrimitives.ReadInt16LittleEndian(bytes[(cursor + 2)..]);
            if (floor > ceiling) { error = "save-sector-inverted"; state = new(); return false; }
            state.Sectors.Add((floor, ceiling));
            cursor += sectorSize;
        }

        return true;
    }

    public static void Apply(AuthoritySimulation sim, ReadOnlySpan<byte> bytes)
    {
        if (!TryRead(bytes, out var state, out var error))
            throw new InvalidOperationException(error);
        sim.RestoreState(state);
    }

    internal static void ValidateTextureScrolls(SimSaveState state)
    {
        if (state.IncludesWallParts && !state.IncludesWallControls
            || state.IncludesWallControls && !state.IncludesFloorControls
            || state.IncludesFloorControls && !state.IncludesCeilingControls
            || state.IncludesCeilingControls && !state.IncludesCarryScrolls
            || state.IncludesCarryScrolls && state.TextureScrolls is null
            || state.TextureScrolls is { } scrolls && scrolls.Any(scroll => scroll is null || !scroll.IsValid
                || !state.IncludesCarryScrolls && scroll.Kind > 8 || !state.IncludesCeilingControls && scroll.Kind > 12
                || !state.IncludesFloorControls && scroll.Kind > 14 || !state.IncludesWallControls && scroll.Kind > 16 || !state.IncludesWallParts && scroll.Kind > 18))
            throw new InvalidOperationException("Saved texture scrollers are invalid.");
    }

    internal static void ValidatePlanes(SimSaveState state)
    {
        if (state.Planes is { } planes && planes.Any(plane => plane is null || !plane.IsValid))
            throw new InvalidOperationException("Saved plane transforms are invalid.");
    }

    internal static void ValidateWalls(SimSaveState state)
    {
        if (state.Walls is { } walls && walls.Any(wall => wall is null || !wall.IsValid))
            throw new InvalidOperationException("Saved wall transforms are invalid.");
    }

    internal static void ValidateSectors(SimSaveState state)
    {
        foreach (var (floor, ceiling) in state.Sectors)
            if (!double.IsFinite(floor) || !double.IsFinite(ceiling) || floor < -32768
                || ceiling > int.MaxValue / 65536.0 || floor > ceiling)
                throw new InvalidOperationException("Saved sector planes are invalid.");
    }
}
