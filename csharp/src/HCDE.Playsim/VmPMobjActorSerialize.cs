namespace HCDE.Playsim;

/// <summary><c>AActor::Serialize</c> field and rollback policy from <c>p_mobj.cpp</c> (210–421).</summary>
public static class VmPMobjActorSerialize
{
    public sealed class SerializeActorState
    {
        public bool IsRollback { get; init; }
        public bool Writing { get; init; }
        public int Special { get; set; }
        public int[] Args { get; set; } = Array.Empty<int>();
        public int[] DefaultArgs { get; set; } = Array.Empty<int>();
        public int FloorTerrain { get; set; }
        public int DefaultFloorTerrain { get; set; }
        public List<string> SerializedKeys { get; } = new();
    }

    public static bool ShouldSerializeUserLights(SerializeActorState s) => !s.IsRollback;

    public static bool PersistsReferenceField(string key) =>
        key is "target" or "lastenemy" or "LastHeard" or "tracer" or "goal" or "master"
            or "Poisoner" or "alternative" or "Inventory" or "BlockingMobj" or "modelData"
            or "ViewPos" or "behaviors";

    public static bool PersistsScalarField(string key) => !PersistsReferenceField(key);

    public static bool SerializeTerrainRoundTrip(int current, int def) => current != def;

    public static bool SerializeArgsWhenSpecial(int special) => special != 0;

    public static int ResolveArgsCount(int special, int argsLength, int defaultLength) =>
        special != 0 ? Math.Min(argsLength, defaultLength) : 0;

    public static void RecordField(SerializeActorState s, string key)
    {
        if (s.Writing)
            s.SerializedKeys.Add(key);
    }

    public static bool RollbackSkipsUserLights(SerializeActorState s) =>
        s.IsRollback && !ShouldSerializeUserLights(s);

    public readonly record struct FlagPersistGate(int Flags, int Flags2, int Flags3, int Flags6, int Flags8);

    public static bool ShouldPersistGhostBits(FlagPersistGate g) =>
        (g.Flags3 & Mf3Ghost) != 0 || (g.Flags2 & Mf2Thrughost) != 0;

    public static bool ShouldPersistThruSpecies(FlagPersistGate g) =>
        (g.Flags2 & Mf2Thruspecies) != 0 || (g.Flags6 & Mf6Thruspecies) != 0;

    public static bool ShouldPersistMthruSpecies(FlagPersistGate g) =>
        (g.Flags6 & Mf6Mthruspecies) != 0;

    public static bool ShouldPersistNonshootable(FlagPersistGate g) =>
        (g.Flags2 & Mf2Nonshootable) != 0;

    public static bool ShouldPersistHitOwner(FlagPersistGate g) =>
        (g.Flags8 & Mf8Hitowner) != 0;

    public const int Mf3Ghost = 1 << 0;
    public const int Mf2Thrughost = 1 << 3;
    public const int Mf2Thruspecies = 1 << 14;
    public const int Mf2Nonshootable = 1 << 2;
    public const int Mf6Thruspecies = 1 << 1;
    public const int Mf6Mthruspecies = 1 << 2;
    public const int Mf8Hitowner = 1 << 1;

    public static bool NonRollbackInventoryTics(SerializeActorState s) => !s.IsRollback;

    public static bool MorphBlockSkippedOnRollback(SerializeActorState s, string key) =>
        s.IsRollback && key is "UnmorphTime" or "MorphFlags" or "PremorphProperties" or "MorphExitFlash";

    public static bool DamageSourcePersisted(SerializeActorState s) =>
        !MorphBlockSkippedOnRollback(s, "damagesource");

    public static void ApplySerializeTail(
        SerializeActorState s,
        Action serializeTerrain,
        Action serializeArgs,
        Action? userLights)
    {
        serializeTerrain();
        if (SerializeArgsWhenSpecial(s.Special))
            serializeArgs();
        if (ShouldSerializeUserLights(s))
            userLights?.Invoke();
    }

    public static bool IdleStateUsesSpawn(object? idle, object? spawn) => idle == null;

    public static bool HealthFieldRoundTrip(int health, int startHealth) => health != startHealth;

    public static bool PoisonFieldsPersist(int duration, int period) =>
        duration != 0 || period != 0;

    public static bool ConversationFieldsPersist(int root, int conv) => root != 0 || conv != 0;

    public static bool RenderHiddenRequired(int hidden, int required) => hidden != 0 || required != 0;

    public static bool StealthAlphaPersist(double alpha) => alpha > 0 && alpha < 1;

    public static bool ViewAnglePersist(double pitch, double yaw) =>
        Math.Abs(pitch) > 1e-6 || Math.Abs(yaw) > 1e-6;

    public static bool SpriteOffsetPersist(double x, double y) => Math.Abs(x) > 1e-6 || Math.Abs(y) > 1e-6;

    public static bool WorldOffsetPersist(double x, double y, double z) =>
        Math.Abs(x) > 1e-6 || Math.Abs(y) > 1e-6 || Math.Abs(z) > 1e-6;

    public static bool LandingSpeedPersist(double speed) => speed > 1e-6;

    public static bool UnmorphTimePersist(SerializeActorState s, int tics) =>
        !MorphBlockSkippedOnRollback(s, "UnmorphTime") && tics > 0;

    public static bool BehaviorsArrayPersist(SerializeActorState s) =>
        !s.IsRollback && PersistsReferenceField("behaviors");

    public static bool ModelDataPersist(SerializeActorState s) =>
        PersistsReferenceField("modelData") && !s.IsRollback;

    public static bool DecalGeneratorPersist(int generator) => generator != 0;

    public static bool RespawnDicePersist(int dice) => dice != 0;

    public static bool MinRespawnTicsPersist(int tics) => tics > 0;

    public static bool FriendPlayerPersist(int fp) => fp >= 0;

    public static bool TagPersist(int tag) => tag != 0;

    public static bool TidPersist(int tid) => tid != 0;

    public static bool TranslationPersist(int tr) => tr != 0;

    public static bool BloodColorPersist(int color) => color != 0;

    public static bool FloatBobPersist(double phase, double strength, double factor) =>
        Math.Abs(phase) > 1e-6 || Math.Abs(strength) > 1e-6 || Math.Abs(factor - 1) > 1e-6;

    public static bool WeaveIndexPersist(int xy, int z) => xy != 0 || z != 0;

    public static bool FreezeTicsPersist(int tics) => tics > 0;

    public static bool DamageFactorPersist(double factor) => Math.Abs(factor - 1) > 1e-6;

    public static bool DamageMultiplyPersist(double mult) => Math.Abs(mult - 1) > 1e-6;

    public static bool PushFactorPersist(double push) => Math.Abs(push) > 1e-6;

    public static bool ScorePersist(int score) => score != 0;

    public static bool DesignatedTeamPersist(int team) => team != 0;

    public static bool VisibleToTeamPersist(int mask) => mask != 0;

    public static bool ActivationTypePersist(int type) => type != 0;

    public static bool LastBumpPersist(int bump) => bump != 0;

    public static bool PainThresholdPersist(int threshold) => threshold != 0;

    public static bool RipLevelPersist(int level, int min, int max) =>
        level != 0 || min != 0 || max != 0;

    public static bool TeleFogTypesPersist(int src, int dst) => src != 0 || dst != 0;

    public static bool CameraPersist(double height, double fov) =>
        Math.Abs(height) > 1e-6 || Math.Abs(fov) > 1e-6;

    public static bool VisibleConePersist(double startA, double endA, double startP, double endP) =>
        Math.Abs(startA) > 1e-6 || Math.Abs(endA) > 1e-6 || Math.Abs(startP) > 1e-6 || Math.Abs(endP) > 1e-6;

    public static bool WoundHealthPersist(int wh) => wh != 0;

    public static bool RadiusDamageFactorPersist(double f) => Math.Abs(f - 1) > 1e-6;

    public static bool SelfDamageFactorPersist(double f) => Math.Abs(f - 1) > 1e-6;

    public static bool FriendlySeeBlocksPersist(int blocks) => blocks != 0;

    public static bool SpawnOrderPersist(int order) => order != 0;

    public static bool SpawnTimePersist(int time) => time != 0;

    public static bool FrictionPersist(double friction) => Math.Abs(friction - 1) > 1e-6;

    public static bool LightLevelPersist(int level) => level != 0;

    public static bool SerializeSuperCalled(SerializeActorState s)
    {
        RecordField(s, "Super");
        return true;
    }
}
