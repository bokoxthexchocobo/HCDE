namespace HCDE.Playsim;

/// <summary><c>P_BounceWall</c>, <c>P_BounceActor</c>, <c>P_ReflectOffActor</c> from <c>p_map.cpp</c> (3709–3887).</summary>
public static class VmPMapBounceReflect
{
    public const int MfMissile = 1;
    public const int Mf2Rip = 1;
    public const int Mf2Reflective = 1;
    public const int Mf5Dontrip = 1;
    public const int Mf6Nobossrip = 1;
    public const int Mf2Boss = 1;
    public const int MfShootable = 1;
    public const int MfNoblood = 1;
    public const int Mf7Thrureflect = 1;
    public const int Mf7HitTarget = 1;
    public const int Mf7HitMaster = 1;
    public const int Mf7HitTracer = 1;
    public const int Mf7Aimreflect = 1;
    public const int Mf7Mirrorreflect = 1;
    public const int BounceAllActors = 1;
    public const int BounceNotOnShootables = 1;
    public const int BounceBounceOnUnrips = 1;
    public const int BounceKeepAngle = 1;
    public const int BounceHereticType = 1;
    public const int BounceMbf = 1;
    public const int BounceModifyPitch = 1;
    public const int BounceUseBounceState = 1;
    public const int Mf2Seekermissile = 1;

    public static bool BounceWallDelegates(bool slideResult) => slideResult;

    public static bool MissileSpecialHitPass(int hit) => hit == 1;

    public static bool MissileSpecialHitBlock(int hit) => hit == 0;

    public static bool ThrureflectPass(bool reflective, bool thrureflect) => reflective && thrureflect;

    public static bool BounceAllOrReflective(bool allActors, bool missileReflective) => allActors || missileReflective;

    public static bool RipperSkipsShootable(bool missile, bool rip, bool shootable, bool bounceOnUnrips, bool dontrip) =>
        missile && rip && shootable && !(bounceOnUnrips && dontrip);

    public static bool NotOnShootablesExplode(int shootable, int bounceFlags) =>
        shootable != 0 && (bounceFlags & BounceNotOnShootables) != 0;

    public static bool BounceCountExplode(int bouncecount) => bouncecount > 0;

    public static bool AssignHitTarget(bool hitTarget) => hitTarget;

    public static bool WallBounceAngle(bool ontop) => !ontop;

    public static bool HereticDeathOnTop(bool ontop, int bounceFlags) =>
        ontop && (bounceFlags & (BounceHereticType | BounceMbf)) == BounceHereticType;

    public static bool MbfZBounce(int bounceFlags) => (bounceFlags & BounceMbf) != 0;

    public static bool ReflectRequiresReflective(int flags2) => (flags2 & Mf2Reflective) != 0;

    public static bool SkipAngleChange(bool thrureflect) => thrureflect;

    public static bool AimReflect(bool aim, bool hasTarget) => aim && hasTarget;

    public static bool MirrorReflect(bool mirror) => mirror;

    public static bool SeekerSwapTracer(bool seeker) => seeker;

    public static bool ZeroSpeedBounce(double speed) => Math.Abs(speed) < 1e-6;

    public static bool RipperNoBounceOnShootable(bool rip, bool shootable, int bounceFlags, bool dontrip) =>
        rip && shootable && (bounceFlags & BounceBounceOnUnrips) == 0 || dontrip;

    public static bool MonsterBounceEligible(bool isPlayer, bool isMonster) => isPlayer == false || isMonster;

    public static bool StateBounceCreature(bool shootable, bool noblood) =>
        shootable && noblood == false;

    public readonly record struct BounceActorContext(
        bool Missile,
        int SpecialHit,
        int SpecialBounce,
        bool Reflective,
        bool Thrureflect,
        bool AllActors,
        bool Rip,
        bool DontRip,
        bool Boss,
        bool NoBossRip,
        bool Shootable,
        int BounceFlags,
        int BounceCount,
        bool Ontop);

    public static bool EvaluateBounceActorEarly(BounceActorContext ctx)
    {
        if (ctx.Missile && MissileSpecialHitPass(ctx.SpecialHit)) return true;
        if (ctx.Missile && MissileSpecialHitBlock(ctx.SpecialHit)) return false;
        if (ThrureflectPass(ctx.Reflective, ctx.Thrureflect)) return true;
        return false;
    }

    public static bool EvaluateBounceActorBody(BounceActorContext ctx) =>
        RipperSkipsShootable(ctx.Missile, ctx.Rip, ctx.Shootable, (ctx.BounceFlags & BounceBounceOnUnrips) != 0, ctx.DontRip)
        || BounceCountExplode(ctx.BounceCount);

    public static bool ReflectOffActor(bool reflective, bool thrureflect, bool aim, bool mirror, bool seeker) =>
        ReflectRequiresReflective(reflective ? 1 : 0) && !SkipAngleChange(thrureflect) || AimReflect(aim, true) || MirrorReflect(mirror) || SeekerSwapTracer(seeker);
}
