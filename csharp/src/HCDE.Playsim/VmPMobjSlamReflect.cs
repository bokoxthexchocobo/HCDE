namespace HCDE.Playsim;

/// <summary><c>AActor::Slam</c>, missile hit virtuals, reflection, and <c>IsOkayToAttack</c> from <c>p_mobj.cpp</c> (3769–4019).</summary>
public static class VmPMobjSlamReflect
{
    public const int Mf8Onlyslamsolid = 1;
    public const int MfSolid = 1;
    public const int MfShootable = 4;
    public const int MfSkullfly = 1;
    public const int Mf2Dormant = 1;
    public const int Mf2Dontreflect = 1;
    public const int Mf4Shieldreflect = 1;
    public const int Mf4Deflect = 1;
    public const int Mf7Noshieldreflect = 1;
    public const int Mf7Thrureflect = 1;
    public const int Mf7Aimreflect = 1;
    public const int Mf7Nevertarget = 1;
    public const int Mf3Ismonster = 1;
    public const int Mf5Summonedmonster = 1;
    public const int MfMissile = 1;
    public const int MfFriendly = 1;
    public const int Mf3Screenseeker = 1;

    public static bool OnlySlamSolidBlocks(int flags8, int thingFlags) =>
        (flags8 & Mf8Onlyslamsolid) != 0
        && (thingFlags & (MfSolid | MfShootable)) == 0;

    public static bool SlamClearsSkullfly(int flags) => (flags & MfSkullfly) != 0;

    public static bool SlamDamageWhenAlive(int health, bool dormant) =>
        health > 0 && !dormant;

    public static bool RetargetAfterSlam(int flags8) => (flags8 & 1) == 0;

    public static int SpecialMissileHitDefault(bool hasVirtual) => hasVirtual ? 0 : -1;

    public static int SpecialBounceHitDefault(bool hasVirtual) => hasVirtual ? 0 : -1;

    public static bool DontReflect(int flags2) => (flags2 & Mf2Dontreflect) != 0;

    public static bool ThruReflectBlocks(int flags7) => (flags7 & Mf7Thrureflect) != 0;

    public static bool ShieldReflectAngleGate(int flags7, double angleDiffDeg) =>
        (flags7 & Mf7Noshieldreflect) == 0 && angleDiffDeg <= 45;

    public static bool GenericReflectJitter(int rollMod16) => rollMod16 != 0;

    public static bool AimReflectFacesTarget(bool aimReflect, bool hasTarget) =>
        aimReflect && hasTarget;

    public static bool AimReflectFacesThingTarget(bool aimReflect, bool selfTargetNull, bool thingHasTarget) =>
        aimReflect && selfTargetNull && thingHasTarget;

    public static bool AttackRequiresPlayerOrMonster(bool player, bool monster) => player || monster;

    public static bool AttackRejectsNevertarget(int flags7) => (flags7 & Mf7Nevertarget) != 0;

    public static bool FriendlyPlayerRef(bool friendly, bool friendPlayer) => friendly && friendPlayer;

    public static bool MinionSameSide(bool summoned, bool tracerMatch) => summoned && tracerMatch;

    public static bool CoopNoDmBlocks(bool mp, bool dm, bool bothPlayers) => mp && !dm && bothPlayers;

    public static bool FriendlyMinionInfight(bool bothFriendly, bool isFriend) => bothFriendly && isFriend;

    public static bool ScreenSeekerFov(bool screenseeker, double angleDeg) =>
        screenseeker && angleDeg < 30 * (256.0 / 360.0);

    public static bool NonScreenSeekerPasses(bool screenseeker) => !screenseeker;

    public readonly record struct SlamContext(
        int Flags8,
        int ThingFlags,
        int Flags,
        int Health,
        bool Dormant,
        bool HasSlamState,
        bool HasSeeState,
        int Flags8RetargetBit);

    public static bool EvaluateSlamPass(SlamContext ctx)
    {
        if (OnlySlamSolidBlocks(ctx.Flags8, ctx.ThingFlags))
            return true;
        return false;
    }

    public static bool EvaluateSlamDamagePath(SlamContext ctx)
    {
        if (!SlamDamageWhenAlive(ctx.Health, ctx.Dormant))
            return ctx.Dormant;
        if (ctx.HasSlamState) return true;
        if (ctx.HasSeeState && RetargetAfterSlam(ctx.Flags8RetargetBit)) return true;
        return true;
    }

    public readonly record struct ReflectContext(
        int Flags2,
        int ThingFlags4,
        int ThingFlags7,
        int Flags7Self,
        double AngleDiffDeg,
        bool RollUnder128,
        bool AimReflect,
        bool HasSelfTarget,
        bool ThingHasTarget);

    public static bool EvaluateReflectEarly(ReflectContext ctx)
    {
        if (DontReflect(ctx.Flags2)) return true;
        if (ThruReflectBlocks(ctx.ThingFlags7)) return false;
        return true;
    }

    public static bool EvaluateShieldReflect(ReflectContext ctx)
    {
        if ((ctx.ThingFlags4 & Mf4Shieldreflect) == 0) return false;
        if (!ShieldReflectAngleGate(ctx.Flags7Self, ctx.AngleDiffDeg)) return true;
        return ctx.RollUnder128;
    }

    public static bool EvaluateDeflect(ReflectContext ctx) =>
        (ctx.ThingFlags4 & Mf4Deflect) != 0 && ctx.RollUnder128;

    public readonly record struct AttackEligibilityContext(
        bool SameActor,
        bool PlayerOrMonster,
        bool Shootable,
        bool Dormant,
        int LinkFlags7,
        bool LinkIsFriend,
        bool FriendNull,
        bool FriendIsFriend,
        bool SummonedMinionSameTracer,
        bool CoopBothPlayers,
        bool FriendlyMinion,
        bool Sight,
        bool ScreenSeeker,
        double FovAngleDeg);

    public static bool IsOkayToAttackGate(AttackEligibilityContext ctx)
    {
        if (ctx.SameActor) return false;
        if (!AttackRequiresPlayerOrMonster(ctx.PlayerOrMonster, true)) return false;
        if (!ctx.Shootable) return false;
        if (ctx.Dormant) return false;
        if (AttackRejectsNevertarget(ctx.LinkFlags7)) return false;
        if (ctx.LinkIsFriend) return false;
        if (ctx.FriendNull) return false;
        if (ctx.FriendIsFriend) return false;
        if (MinionSameSide(true, ctx.SummonedMinionSameTracer)) return false;
        if (CoopNoDmBlocks(true, false, ctx.CoopBothPlayers)) return false;
        if (FriendlyMinionInfight(true, ctx.FriendlyMinion)) return false;
        if (!ctx.Sight) return false;
        if (ScreenSeekerFov(ctx.ScreenSeeker, ctx.FovAngleDeg)) return true;
        return NonScreenSeekerPasses(ctx.ScreenSeeker);
    }
}
