namespace HCDE.Playsim;

/// <summary>Blood spawn/splatter/ripper from <c>p_mobj.cpp</c> (7223–7467).</summary>
public static class VmPMobjSpawnBlood
{
    public const int Mf4Allowparticles = 1;
    public const int Mf5Puffgetsowner = 1;
    public const int Mf2Donttranslate = 1;
    public const int GameDoomchex = 1;
    public const int GameDoomstrifechex = 1 << 1;
    public const int GameStrife = 1 << 2;
    public const int GameHeretic = 1 << 3;
    public const int MfNogravity = 1;
    public const int RfInvisible = 1;

    public static bool BloodClassValid(bool bloodNull) => !bloodNull;

    public static bool ForceBloodTypeOff(int flags4) => (flags4 & Mf4Allowparticles) == 0;

    public static bool AssignOwnerTarget(int flags5) => (flags5 & Mf5Puffgetsowner) != 0;

    public static bool DoomChexRandomizeTics(int gameType) => (gameType & GameDoomchex) != 0;

    public static int ClampBloodTics(int tics) => tics < 1 ? 1 : tics;

    public static bool ApplyBloodTranslation(int flags2) => (flags2 & Mf2Donttranslate) == 0;

    public static bool StrifeSprayState(int gameType, int damage, bool hasSprayState) =>
        (gameType & GameStrife) != 0 && damage > 13 && hasSprayState;

    public static int StrifeLowDamageBump(int gameType, int damage) =>
        (gameType & GameStrife) != 0 && damage <= 13 ? damage + 2 : damage;

    public static int DamageStateAdvance(int damage)
    {
        if (damage <= 12 && damage >= 9) return 1;
        if (damage < 9) return 2;
        return 0;
    }

    public static bool UseDoomStrifeChexStatePick(int gameType) => (gameType & GameDoomstrifechex) != 0;

    public static bool HideActorForBloodType(int bloodTypeCvar) => bloodTypeCvar > 1;

    public static int InvisibleRenderFlags(int renderFlags) => renderFlags | RfInvisible;

    public static bool DrawSplashParticles(int bloodTypeCvar) => bloodTypeCvar >= 1;

    public static bool SplatterClassValid(bool bloodNull) => !bloodNull;

    public static double SplatterVelocityComponent(int random2) => random2 / 64.0;

    public static double Splatter2Offset(int splatRandom) => (splatRandom - 128) / 32.0;

    public static double OppositeHitAngle(double hitAngleDeg) => hitAngleDeg - 180;

    public static bool RipperClassValid(bool bloodNull) => !bloodNull;

    public static bool RipperHereticNoGravity(int gameType) => (gameType & GameHeretic) != 0;

    public static double RipperOffsetComponent(int random2) => random2 / 16.0;

    public readonly record struct BloodStateWalkContext(
        int Advance,
        bool OwnsSpawnState,
        bool OwnsAdvancedState);

    public static bool TryAdvanceOwnedState(BloodStateWalkContext ctx) =>
        ctx.Advance > 0 && ctx.OwnsSpawnState && ctx.OwnsAdvancedState;

    public static bool WalkParentForOwnedState(bool atActorBase) => !atActorBase;

    public static int RemainingAdvanceSteps(int requested, int ownedSteps) =>
        requested < ownedSteps ? requested : ownedSteps;

    public static bool RipperSplashUsesAngleTo(int bloodTypeCvar) => bloodTypeCvar >= 1;

    public readonly record struct RipperSplashContext(
        int BloodTypeCvar,
        double AngleToMoDeg);

    public static double RipperSplashAngle(RipperSplashContext ctx) => ctx.AngleToMoDeg + 180;

    public static bool ClampLowDamageStrife(int gameType, int damage) =>
        (gameType & GameStrife) != 0 && damage <= 13;

    public static bool ShouldPickStateAfterSpray(bool foundSprayState) => !foundSprayState;

    public static int SelectBloodStateAdvance(int damage, int gameType, bool strifeSprayFound)
    {
        if (StrifeSprayState(gameType, damage, strifeSprayFound)) return -1;
        return DamageStateAdvance(StrifeLowDamageBump(gameType, damage));
    }

    public readonly record struct ParentStateWalkStep(
        bool AtActorBase,
        int RemainingAdvance,
        bool OwnsSpawn,
        bool OwnsAdvanced);

    public static bool ContinueParentWalk(ParentStateWalkStep step) =>
        WalkParentForOwnedState(step.AtActorBase) && step.RemainingAdvance > 0;

    public static bool ApplyAdvanceOnStep(ParentStateWalkStep step) =>
        TryAdvanceOwnedState(new BloodStateWalkContext(
            step.RemainingAdvance, step.OwnsSpawn, step.OwnsAdvanced));

    public static bool SplatterVelocityFromRandom(int random2) => random2 != 0;

    public static bool CoopBloodTranslation(bool applyTranslation, bool hasBloodClass) =>
        applyTranslation && hasBloodClass;

    public static bool RipperSplashWhenEnabled(RipperSplashContext ctx) =>
        RipperSplashUsesAngleTo(ctx.BloodTypeCvar);
}
