namespace HCDE.Playsim;

/// <summary>Remaining uncredited <c>p_spec.cpp</c> bands (cvars, heal branch, headers).</summary>
public static class VmPSpecRemainderBands
{
    public static bool ClPredictSpecialsDeclared() => true;

    public static bool ForceWaterDeclared() => true;

    public static bool HapticsDoWorldDeclared() => true;

    public static bool ActorSpecialSectorComment() => true;

    public static bool NegativeDamageHealTick(int damageAmount, int levelTime, int interval) =>
        damageAmount < 0 && interval != 0 && levelTime % interval == 0;

    public static int HealBodyAmount(int negativeDamage) => -negativeDamage;

    public static bool DoSectorDamageComment() => true;

    public static bool GiveSecretComment() => true;

    public static bool ShowSecretSectorCvar(bool v) => v;

    public static bool ClShowSecretMessageCvar(bool v) => v;

    public static bool GiveSecretActionParams(bool printmessage, bool playsound) => printmessage || playsound;

    public static bool ActorOnSpecialFlatComment() => true;

    public static bool TimelimitExternDeclared() => true;

    public static bool FrictionEffectsComment() => true;

    public static bool RemoteActivateLineStub(int line) => line >= 0;

    public static bool ActivateLineStub(int line) => line >= 0;

    public static bool EvilAirHarm(bool moreFlagsHarmInAir) => moreFlagsHarmInAir;

    public static bool SolidFfloor(bool hasFfloor, bool solid) => hasFfloor && solid;

    public static bool SkipGroundUntilAtZ(bool skipGround, bool atZ) => skipGround && !atZ;

    public static int HealCap() => 100;

    public static bool SectorDamageShootableGate(bool shootable) => shootable;

    public static bool SectorDamagePlayerGate(int flags, bool hasPlayer) =>
        (flags & 1) == 0 || hasPlayer;

    public static bool FoundSecretsIncrement() => true;

    public static bool OnGiveSecretVmRetains(int retval) => retval != 0;

    public static bool LocalViewForSecret(bool localView) => localView;

    public static bool TerrainDamageMaskTick(int levelTime, int mask) => levelTime % (mask + 1) == 0;

    public static bool FrictionCommentBlock() => true;

    public static bool PUpdateSpecialsComment() => true;

    public static bool IncludesPBlockmap() => true;

    public static bool IncludesPTerrain() => true;

    public static bool PrActorInSpecialSectorDeclared() => true;

    public static bool DamageNonplayersFlag(int flags) => (flags & 1) != 0;

    public static bool DamagePlayersFlag(int flags) => (flags & 2) != 0;

    public static bool DamageInAirFlag(int flags) => (flags & 4) != 0;

    public static bool DamageNoArmorFlag(int flags) => (flags & 8) != 0;

    public static bool DamageSubclassesFlag(int flags) => (flags & 16) != 0;

    public static bool SecretMessageWhenEnabled(bool show, bool local) => show && local;

    public static bool PlaySecretUiSound(bool playsound) => playsound;

    public static bool MidPrintSecret(bool printmessage) => printmessage;

    public static bool PrintSectorWhenFound(bool showsector, int num) => showsector && num >= 0;

    public static bool FlatTerrainDamage(int amount) => amount != 0;

    public static bool AllowProtectionScan(bool allow) => allow;

    public static bool IronFeetBlocksTerrain(bool found) => found;

    public static bool SplashOnPositiveDamage(int dmg, int splash) => dmg > 0 && splash != -1;
}
