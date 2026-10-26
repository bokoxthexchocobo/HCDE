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

    public static double SplatterVelocityComponent(int random2) => random2 / 64.0;

    public static double Splatter2Offset(int splatRandom) => (splatRandom - 128) / 32.0;

    public static double OppositeHitAngle(double hitAngleDeg) => hitAngleDeg - 180;

    public static bool RipperHereticNoGravity(int gameType) => (gameType & GameHeretic) != 0;

    public static double RipperOffsetComponent(int random2) => random2 / 16.0;

    public readonly record struct BloodStateWalkContext(
        int Advance,
        bool OwnsSpawnState,
        bool OwnsAdvancedState);

    public static bool TryAdvanceOwnedState(BloodStateWalkContext ctx) =>
        ctx.Advance > 0 && ctx.OwnsSpawnState && ctx.OwnsAdvancedState;

    public static int RemainingAdvanceSteps(int requested, int ownedSteps) =>
        requested < ownedSteps ? requested : ownedSteps;

    public static bool RipperSplashUsesAngleTo(int bloodTypeCvar) => bloodTypeCvar >= 1;

    public readonly record struct RipperSplashContext(
        int BloodTypeCvar,
        double AngleToMoDeg);

    public static double RipperSplashAngle(RipperSplashContext ctx) => ctx.AngleToMoDeg + 180;

    public static bool ClampLowDamageStrife(int gameType, int damage) =>
        (gameType & GameStrife) != 0 && damage <= 13;

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
        !step.AtActorBase && step.RemainingAdvance > 0;

    public static bool ApplyAdvanceOnStep(ParentStateWalkStep step) =>
        TryAdvanceOwnedState(new BloodStateWalkContext(
            step.RemainingAdvance, step.OwnsSpawn, step.OwnsAdvanced));

    public static bool SplatterVelocityFromRandom(int random2) => random2 != 0;

    public static bool CoopBloodTranslation(bool applyTranslation, bool hasBloodClass) =>
        applyTranslation && hasBloodClass;

    public static bool RipperSplashWhenEnabled(RipperSplashContext ctx) =>
        RipperSplashUsesAngleTo(ctx.BloodTypeCvar);

    public static double ApplyBloodPositionZ(double z, int random2) => z + random2 / 64.0;

    public static int ResolveBloodTypeCvar(int bloodTypeCvar, int bloodClassFlags4) =>
        ForceBloodTypeOff(bloodClassFlags4) ? 0 : bloodTypeCvar;

    public readonly record struct BloodSpawnInitial(
        double VelZ,
        double YawDeg,
        bool AssignOriginatorTarget,
        bool ApplyTranslation,
        int TicsAfterRandomize);

    public static BloodSpawnInitial PlanBloodSpawnInitial(
        int flags5,
        int flags2,
        int gameType,
        int baseTics,
        int randomRoll,
        double dirDeg)
    {
        var tics = baseTics;
        if (DoomChexRandomizeTics(gameType))
            tics = ClampBloodTics(tics - (randomRoll & 3));
        return new BloodSpawnInitial(2, dirDeg, AssignOwnerTarget(flags5), ApplyBloodTranslation(flags2), tics);
    }

    public sealed class BloodClassStateNode
    {
        public required bool OwnsSpawnState { get; init; }
        public required Func<int, bool> OwnsSpawnOffset { get; init; }
    }

    public static int? WalkParentChainForBloodAdvance(IReadOnlyList<BloodClassStateNode> chain, int advance)
    {
        if (advance <= 0)
            return null;
        foreach (var node in chain)
        {
            if (!node.OwnsSpawnState)
                continue;
            for (var checkedAdvance = advance; checkedAdvance > 0; checkedAdvance--)
            {
                if (node.OwnsSpawnOffset(checkedAdvance))
                    return checkedAdvance;
            }
        }

        return null;
    }

    public readonly record struct BloodStatePickResult(bool UseSprayState, int StateAdvanceOffset);

    public static BloodStatePickResult PlanBloodDamageState(int damage, int gameType, bool sprayStateFound)
    {
        var advance = SelectBloodStateAdvance(damage, gameType, sprayStateFound);
        if (advance < 0)
            return new BloodStatePickResult(true, 0);
        return new BloodStatePickResult(false, advance);
    }

    public readonly record struct BloodSpawnFinishOutcome(
        bool HideActor,
        bool DrawSplash,
        int SplashColor);

    public static BloodSpawnFinishOutcome PlanBloodSpawnFinish(int bloodTypeCvar, int bloodColor) =>
        new(HideActorForBloodType(bloodTypeCvar), DrawSplashParticles(bloodTypeCvar), bloodColor);

    public static BloodSpawnFinishOutcome RunSpawnBloodFinish(
        int bloodTypeCvar,
        int bloodClassFlags4,
        int bloodColor) =>
        PlanBloodSpawnFinish(ResolveBloodTypeCvar(bloodTypeCvar, bloodClassFlags4), bloodColor);

    public readonly record struct SplatterSpawnOutcome(
        double VelX,
        double VelY,
        double VelZ,
        bool ApplyTranslation,
        bool HideActor,
        bool DrawSplash,
        double SplashAngleDeg);

    public static SplatterSpawnOutcome PlanBloodSplatter(
        int bloodTypeCvar,
        int bloodClassFlags4,
        int flags2,
        int random2X,
        int random2Y,
        double hitAngleDeg,
        int bloodColor)
    {
        var effective = ResolveBloodTypeCvar(bloodTypeCvar, bloodClassFlags4);
        return new SplatterSpawnOutcome(
            SplatterVelocityComponent(random2X),
            SplatterVelocityComponent(random2Y),
            3,
            ApplyBloodTranslation(flags2),
            HideActorForBloodType(effective),
            DrawSplashParticles(effective),
            OppositeHitAngle(hitAngleDeg));
    }

    public readonly record struct Splatter2SpawnOutcome(
        double OffsetX,
        double OffsetY,
        bool ApplyTranslation,
        bool HideActor,
        bool DrawSplash,
        double SplashAngleDeg);

    public static Splatter2SpawnOutcome PlanBloodSplatter2(
        int bloodTypeCvar,
        int bloodClassFlags4,
        int flags2,
        int splatRandomX,
        int splatRandomY,
        double hitAngleDeg)
    {
        var effective = ResolveBloodTypeCvar(bloodTypeCvar, bloodClassFlags4);
        return new Splatter2SpawnOutcome(
            Splatter2Offset(splatRandomX),
            Splatter2Offset(splatRandomY),
            ApplyBloodTranslation(flags2),
            HideActorForBloodType(effective),
            DrawSplashParticles(effective),
            OppositeHitAngle(hitAngleDeg));
    }

    public readonly record struct RipperBloodOutcome(
        double OffsetX,
        double OffsetY,
        double OffsetZ,
        bool AssignBleederTarget,
        bool HereticNoGravity,
        double VelXHalf,
        double VelYHalf,
        int TicsBump,
        bool ApplyTranslation,
        bool HideActor,
        bool DrawSplash,
        double SplashAngleDeg);

    public static RipperBloodOutcome PlanRipperBlood(
        int bloodTypeCvar,
        int bloodClassFlags4,
        int flags5,
        int flags2,
        int gameType,
        int randomRipper,
        double moVelX,
        double moVelY,
        double angleToMoDeg,
        int baseTics)
    {
        var effective = ResolveBloodTypeCvar(bloodTypeCvar, bloodClassFlags4);
        return new RipperBloodOutcome(
            RipperOffsetComponent(randomRipper),
            RipperOffsetComponent(randomRipper),
            RipperOffsetComponent(randomRipper),
            AssignOwnerTarget(flags5),
            RipperHereticNoGravity(gameType),
            moVelX / 2,
            moVelY / 2,
            baseTics + (randomRipper & 3),
            ApplyBloodTranslation(flags2),
            HideActorForBloodType(effective),
            RipperSplashWhenEnabled(new RipperSplashContext(effective, angleToMoDeg)),
            RipperSplashAngle(new RipperSplashContext(effective, angleToMoDeg)));
    }
}
