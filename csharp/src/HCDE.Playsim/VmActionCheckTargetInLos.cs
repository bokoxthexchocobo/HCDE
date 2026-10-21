namespace HCDE.Playsim;

/// <summary>
/// <c>AActor::CheckIfTargetInLOS</c> and <c>CheckIfInTargetLOS</c> from
/// <c>p_actionfunctions.cpp</c> (2551–2678, 2688–2760).
/// </summary>
public static class VmActionCheckTargetInLos
{
    public const int JlosfProjectile = 1 << 0;
    public const int JlosfNosight = 1 << 1;
    public const int JlosfClosenofov = 1 << 2;
    public const int JlosfClosenosight = 1 << 3;
    public const int JlosfClosenojump = 1 << 4;
    public const int JlosfDeadnojump = 1 << 5;
    public const int JlosfCheckmaster = 1 << 6;
    public const int JlosfTargetlos = 1 << 7;
    public const int JlosfFlipfov = 1 << 8;
    public const int JlosfAllynojump = 1 << 9;
    public const int JlosfCombatantonly = 1 << 10;
    public const int JlosfNoautoaim = 1 << 11;
    public const int JlosfChecktracer = 1 << 12;

    public const int MfMissile = 0x00000020;
    public const int Mf2Seekermissile = 0x00000080;
    public const int Mf3Ismonster = 0x00080000;

    public enum TargetLosPlan
    {
        FailNoTarget,
        FailDeadTarget,
        FailNotCombatant,
        FailAlly,
        FailMaxDistance,
        FailCloseNoJump,
        FailSight,
        FailFov,
        Success,
    }

    public readonly record struct TargetInLosContext(
        bool IsPlayer,
        bool HasAimTarget,
        bool HasMaster,
        bool HasTracer,
        bool HasMonsterTarget,
        bool IsMissile,
        bool IsSeekerMissile,
        bool TargetIsPlayer,
        bool TargetIsMonster,
        bool IsFriend,
        int TargetHealth);

    public static bool UseMaster(int flags) => (flags & JlosfCheckmaster) != 0;

    public static bool UseTracerOrProjectile(int flags, bool isMissile) =>
        (isMissile && (flags & JlosfProjectile) != 0) || (flags & JlosfChecktracer) != 0;

    public static bool SeekerOrCheckTracer(int flags, bool isSeeker) =>
        isSeeker || (flags & JlosfChecktracer) != 0;

    public static bool FailNullTarget(bool hasTarget) => !hasTarget;

    public static bool FailDeadTarget(int flags, int health) =>
        (flags & JlosfDeadnojump) != 0 && health <= 0;

    public static bool InitialDoCheckSight(int flags) => (flags & JlosfNosight) == 0;

    public static (bool HasAimTarget, double Fov, bool DoCheckSight) PlayerAimBranch(
        bool hasAimTarget, int flags, double fov, bool initialSight)
    {
        if (!hasAimTarget) return (false, fov, initialSight);
        var viewFlags = flags & (JlosfTargetlos | JlosfFlipfov);
        return viewFlags switch
        {
            JlosfTargetlos | JlosfFlipfov => (true, 0, initialSight),
            JlosfTargetlos => (true, fov, initialSight),
            _ => (true, viewFlags == JlosfFlipfov ? fov : 0, viewFlags == JlosfFlipfov),
        };
    }

    public static bool FailCombatantOnly(int flags, bool targetIsPlayer, bool targetIsMonster) =>
        (flags & JlosfCombatantonly) != 0 && !targetIsPlayer && !targetIsMonster;

    public static bool FailAlly(int flags, bool isFriend) => (flags & JlosfAllynojump) != 0 && isFriend;

    public static bool FailMaxDistance(double distance, double distMax) =>
        distMax > 0 && distance > distMax;

    public static bool FailCloseNoJump(int flags, double distance, double distClose) =>
        distClose > 0 && distance < distClose && (flags & JlosfClosenojump) != 0;

    public static (double Fov, bool DoCheckSight) ApplyCloseDistanceAdjustments(
        int flags, double distance, double distClose, double fov, bool doCheckSight)
    {
        if (distClose <= 0 || distance >= distClose) return (fov, doCheckSight);
        var nextFov = (flags & JlosfClosenofov) != 0 ? 0 : fov;
        var nextSight = (flags & JlosfClosenosight) != 0 ? false : doCheckSight;
        return (nextFov, nextSight);
    }

    public static (bool ViewportIsSelf, bool FlipActors) TargetLosViewport(int flags) =>
        ((flags & JlosfTargetlos) == 0, (flags & JlosfFlipfov) != 0);

    public static bool SightRequired(bool doCheckSight, bool sightResult) =>
        !doCheckSight || sightResult;

    public static double ClampFov(double fov) => fov > 360 ? 360 : fov;

    public static bool OutsideFov(double fov, double angleDelta) =>
        fov > 0 && angleDelta > fov * 0.5;

    public static bool InTargetLosFovCheck(double fov, double angleDelta) =>
        fov <= 0 || fov >= 360 || angleDelta <= fov * 0.5;

    public static TargetLosPlan PlanCheckIfTargetInLos(
        TargetInLosContext ctx,
        int flags,
        double fov,
        double distance,
        double distMax,
        double distClose,
        bool sightOk,
        double angleDelta)
    {
        if (!ctx.IsPlayer)
        {
            var hasTarget = ResolveNonPlayerHasTarget(ctx, flags);
            if (!hasTarget) return TargetLosPlan.FailNoTarget;
            if (FailDeadTarget(flags, ctx.TargetHealth)) return TargetLosPlan.FailDeadTarget;
        }
        else
        {
            var aim = PlayerAimBranch(ctx.HasAimTarget, flags, fov, InitialDoCheckSight(flags));
            if (!aim.HasAimTarget) return TargetLosPlan.FailNoTarget;
            fov = aim.Fov;
            var doSight = aim.DoCheckSight;
            if (FailCombatantOnly(flags, ctx.TargetIsPlayer, ctx.TargetIsMonster)) return TargetLosPlan.FailNotCombatant;
            if (FailAlly(flags, ctx.IsFriend)) return TargetLosPlan.FailAlly;
            if (FailMaxDistance(distance, distMax)) return TargetLosPlan.FailMaxDistance;
            (fov, doSight) = ApplyCloseDistanceAdjustments(flags, distance, distClose, fov, doSight);
            if (!SightRequired(doSight, sightOk)) return TargetLosPlan.FailSight;
            fov = ClampFov(fov);
            if (OutsideFov(fov, angleDelta)) return TargetLosPlan.FailFov;
            return TargetLosPlan.Success;
        }

        if (FailCombatantOnly(flags, ctx.TargetIsPlayer, ctx.TargetIsMonster)) return TargetLosPlan.FailNotCombatant;
        if (FailAlly(flags, ctx.IsFriend)) return TargetLosPlan.FailAlly;
        if (FailMaxDistance(distance, distMax)) return TargetLosPlan.FailMaxDistance;
        var doCheckSight = InitialDoCheckSight(flags);
        (fov, doCheckSight) = ApplyCloseDistanceAdjustments(flags, distance, distClose, fov, doCheckSight);
        if (FailCloseNoJump(flags, distance, distClose)) return TargetLosPlan.FailCloseNoJump;
        if (!SightRequired(doCheckSight, sightOk)) return TargetLosPlan.FailSight;
        fov = ClampFov(fov);
        if (OutsideFov(fov, angleDelta)) return TargetLosPlan.FailFov;
        return TargetLosPlan.Success;
    }

    public static bool ResolveNonPlayerHasTarget(TargetInLosContext ctx, int flags)
    {
        if (UseMaster(flags)) return ctx.HasMaster;
        if (UseTracerOrProjectile(flags, ctx.IsMissile))
        {
            if (SeekerOrCheckTracer(flags, ctx.IsSeekerMissile)) return ctx.HasTracer;
            return false;
        }
        return ctx.HasMonsterTarget;
    }

    public static TargetLosPlan PlanCheckIfInTargetLos(
        TargetInLosContext ctx,
        int flags,
        double fov,
        double distance,
        double distMax,
        double distClose,
        bool sightOk,
        double angleDelta)
    {
        var hasTarget = ResolveInTargetLosHasTarget(ctx, flags);
        if (!hasTarget) return TargetLosPlan.FailNoTarget;
        if (FailDeadTarget(flags, ctx.TargetHealth)) return TargetLosPlan.FailDeadTarget;
        if (FailMaxDistance(distance, distMax)) return TargetLosPlan.FailMaxDistance;
        var doCheckSight = InitialDoCheckSight(flags);
        (fov, doCheckSight) = ApplyCloseDistanceAdjustments(flags, distance, distClose, fov, doCheckSight);
        if (FailCloseNoJump(flags, distance, distClose)) return TargetLosPlan.FailCloseNoJump;
        if (!InTargetLosFovCheck(fov, angleDelta)) return TargetLosPlan.FailFov;
        if (!SightRequired(doCheckSight, sightOk)) return TargetLosPlan.FailSight;
        return TargetLosPlan.Success;
    }

    public static bool ResolveInTargetLosHasTarget(TargetInLosContext ctx, int flags)
    {
        if (UseMaster(flags)) return ctx.HasMaster;
        if (ctx.IsMissile && (flags & JlosfProjectile) != 0)
        {
            if (ctx.IsSeekerMissile) return ctx.HasTracer;
            return false;
        }
        return ctx.HasMonsterTarget;
    }

    public static bool PlayerUsesNoAutoaim(int flags) => (flags & JlosfNoautoaim) != 0;
}
