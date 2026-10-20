namespace HCDE.Playsim;

/// <summary><c>P_BounceActor</c> and <c>P_ReflectOffActor</c> from <c>p_map.cpp</c> (3709–3887).</summary>
public static class VmPMapBounceActor
{
    public const int MfMissile = 1;
    public const int MfShootable = 4;
    public const int Mf2Rip = 1;
    public const int Mf2Reflective = 1;
    public const int Mf2Boss = 1;
    public const int Mf5Dontrip = 1;
    public const int Mf6Nobossrip = 1;
    public const int Mf7Thrureflect = 1;
    public const int Mf7Hittarget = 1;
    public const int Mf7Hitmaster = 1;
    public const int Mf7Hittracer = 1;
    public const int Mf7Mirrorreflect = 1;
    public const int Mf7Aimreflect = 1;
    public const int Mf2Seekermissile = 1;
    public const int MfNoblood = 1;
    public const int BounceAllactors = 1;
    public const int BounceNotonshootables = 1;
    public const int BounceBounceonunrips = 1;
    public const int BounceKeepangle = 1;
    public const int BounceHeretictype = 1;
    public const int BounceMbf = 1;
    public const int BounceModifypitch = 1;
    public const int BounceAutooff = 1;
    public const int BounceUsebouncestate = 1;

    public static bool BounceWallDelegates(bool slideResult) => slideResult;

    public static bool RequiresMissileAndBlocker(bool hasMo, bool missile, bool hasBlocker) =>
        hasMo && missile && hasBlocker;

    public static int MissileHitContinue(int hit) => hit;

    public static bool ThruReflectPasses(bool reflective, bool thruReflect) =>
        reflective && thruReflect;

    public static bool ActorBounceEligible(int bounceFlags, bool missile, bool rip, bool donTrip, bool nobossRip, bool boss, bool reflective, bool isMonster, bool hasPlayer) =>
        (bounceFlags & BounceAllactors) != 0
        || (missile && (!rip || donTrip || (nobossRip && boss)) && reflective)
        || (!hasPlayer && !isMonster);

    public static bool RipSkipsShootable(bool missile, bool rip, int bounceFlags, bool shootable, bool donTrip) =>
        missile && rip && shootable && (bounceFlags & BounceBounceonunrips) == 0 && !donTrip;

    public static bool NotOnShootablesExplode(int bounceFlags, bool shootable) =>
        shootable && (bounceFlags & BounceNotonshootables) != 0;

    public static bool BounceCountExplodes(int count) => count > 0;

    public static bool AssignHitTarget(int flags7) => (flags7 & Mf7Hittarget) != 0;

    public static bool AssignHitMaster(int flags7) => (flags7 & Mf7Hitmaster) != 0;

    public static bool AssignHitTracer(int flags7) => (flags7 & Mf7Hittracer) != 0;

    public static bool SideBounceApplies(bool ontop) => !ontop;

    public static bool HereticBounceDeath(int bounceFlags) =>
        (bounceFlags & BounceHeretictype) != 0 && (bounceFlags & BounceMbf) == 0;

    public static bool MbfVerticalBounce(int bounceFlags) => (bounceFlags & BounceMbf) != 0;

    public static bool ModifyPitchOnBounce(int bounceFlags) => (bounceFlags & BounceModifypitch) != 0;

    public static bool AutoOffBounce(int bounceFlags, double velZ, bool nogravity) =>
        (bounceFlags & BounceAutooff) != 0 && (!nogravity && velZ < 3);

    public static bool UseBounceState(int bounceFlags) => (bounceFlags & BounceUsebouncestate) != 0;

    public static bool ShootableBloodForState(bool shootable, bool noBlood) =>
        shootable && !noBlood;

    public static bool ReflectRequiresReflective(int flags2) => (flags2 & Mf2Reflective) != 0;

    public static bool ReflectAdjustBlocks(bool adjusted) => adjusted;

    public static bool AimReflectRetarget(bool aimReflect, bool hasTarget) => aimReflect && hasTarget;

    public static bool MirrorReflectFlip(int flags7) => (flags7 & Mf7Mirrorreflect) != 0;

    public static bool SeekerSetsTracer(int flags2) => (flags2 & Mf2Seekermissile) != 0;

    public readonly record struct BounceActorContext(
        bool Missile,
        bool HasBlocker,
        int SpecialMissileHit,
        int SpecialBounceHit,
        bool ReflectiveThru,
        int BounceFlags,
        bool Rip,
        bool DonTrip,
        bool Boss,
        bool Shootable,
        int BounceCount);

    public static bool EvaluateBounceActorEntry(BounceActorContext ctx)
    {
        if (!RequiresMissileAndBlocker(true, ctx.Missile, ctx.HasBlocker)) return false;
        if (ctx.SpecialMissileHit == 1 || ctx.SpecialMissileHit == 0) return ctx.SpecialMissileHit == 1;
        if (ctx.SpecialBounceHit == 1 || ctx.SpecialBounceHit == 0) return ctx.SpecialBounceHit == 1;
        if (ThruReflectPasses(true, ctx.ReflectiveThru)) return true;
        return ActorBounceEligible(ctx.BounceFlags, ctx.Missile, ctx.Rip, ctx.DonTrip, false, ctx.Boss, true, false, false);
    }
}
