namespace HCDE.Playsim;

/// <summary>Monster look / TID / enemy search from <c>p_enemy.cpp</c> (1489–1777).</summary>
public static class VmPEnemyLookTargets
{
    public const int MonsLookRange = 20 * 64;
    public const int MonsLookLimit = 64;
    public const int Mf3Ismonster = 1;
    public const int MfShootable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf3Nosightcheck = 1 << 2;
    public const int LofDontchasegoal = 1;

    public static bool PlayerCannotSeeMonster(bool sightOk) => !sightOk;

    public static bool SkipNonMonster(bool isMonster, bool sameActor, int health) =>
        !isMonster || sameActor || health <= 0;

    public static bool OutOfLookRange(double distance2d) => distance2d > MonsLookRange;

    public static bool RandomLookSkip(int random0to255) => random0to255 < 16;

    public static bool LookLimitReached(int count) => count >= MonsLookLimit;

    public static bool SameSpecies(int selfSpecies, int otherSpecies) => selfSpecies == otherSpecies;

    public static bool MonsterOutOfSight(bool sightOk) => !sightOk;

    public readonly record struct LookMonsterContext(
        bool PlayerSight,
        double Distance2d,
        int RandomRoll,
        int Count,
        int SelfSpecies,
        int OtherSpecies,
        bool ActorSight);

    public static bool TryPickLookTarget(LookMonsterContext ctx, out bool found)
    {
        if (PlayerCannotSeeMonster(ctx.PlayerSight))
        {
            found = false;
            return true;
        }

        if (OutOfLookRange(ctx.Distance2d) || RandomLookSkip(ctx.RandomRoll) ||
            LookLimitReached(ctx.Count) || SameSpecies(ctx.SelfSpecies, ctx.OtherSpecies) ||
            MonsterOutOfSight(ctx.ActorSight))
        {
            found = false;
            return false;
        }

        found = true;
        return true;
    }

    public static bool BlockNodeNotShootable(int flags) => (flags & MfShootable) == 0;

    public static bool BlockNodeDormant(int flags2) => (flags2 & Mf2Dormant) != 0;

    public static bool TidMatches(int linkTid, int hateTid) => linkTid == hateTid;

    public static bool TargetTidMatches(bool hasTarget, int targetTid, int hateTid) =>
        hasTarget && targetTid == hateTid;

    public static bool RequiresVisibility(bool noSightCheck) => !noSightCheck;

    public readonly record struct TidBlockCandidate(
        int Flags,
        int Flags2,
        int Health,
        bool IsSelf,
        int LinkTid,
        int HateTid,
        bool HasTarget,
        int TargetTid,
        int TargetFlags,
        int TargetHealth,
        int TargetFlags2,
        bool Visible,
        bool NoSightCheck);

    public static bool TryPickTidInBlock(TidBlockCandidate c, out bool found)
    {
        found = false;
        if (BlockNodeNotShootable(c.Flags) || c.IsSelf || c.Health <= 0 || BlockNodeDormant(c.Flags2))
            return false;

        var other = false;
        if (TidMatches(c.LinkTid, c.HateTid))
            other = true;
        else if (TargetTidMatches(c.HasTarget, c.TargetTid, c.HateTid))
        {
            if (BlockNodeNotShootable(c.TargetFlags) || c.TargetHealth <= 0 ||
                BlockNodeDormant(c.TargetFlags2))
                return false;
            other = true;
        }

        if (!other)
            return false;
        if (RequiresVisibility(c.NoSightCheck) && !c.Visible)
            return false;
        found = true;
        return true;
    }

    public static bool ChaseGoal(bool hasParams, int paramFlags) =>
        !hasParams || (paramFlags & LofDontchasegoal) == 0;

    public static bool ClearReactionWhenLeavingGoal(bool hasGoal, bool targetIsGoal) =>
        hasGoal && targetIsGoal;

    public static bool LastLookTidStale(bool hasLast, int lastTid, int hateTid) =>
        hasLast && lastTid != hateTid;

    public static int LookIteratorBudget(int random0to31) => (random0to31 & 31) + 7;

    public static bool AbortLookCycle(bool reachedEnd, AActorRef? other) =>
        other is null && reachedEnd;

    public readonly record struct LookTidContext(
        bool BlockSearchHit,
        bool HasGoal,
        bool TargetIsGoal,
        bool HasParams,
        int ParamFlags,
        bool HasLastLook,
        int LastTid,
        int HateTid,
        int Budget,
        bool NoSightCheck,
        bool Visible,
        int OtherFlags,
        int OtherHealth,
        int OtherFlags2,
        bool IsSelf);

    public static bool EvaluateLookTidStep(LookTidContext ctx, out bool found)
    {
        if (ctx.BlockSearchHit)
        {
            found = true;
            return true;
        }

        if (BlockNodeNotShootable(ctx.OtherFlags) || ctx.IsSelf || ctx.OtherHealth <= 0 ||
            BlockNodeDormant(ctx.OtherFlags2))
        {
            found = false;
            return false;
        }

        if (ctx.Budget <= 0)
        {
            found = false;
            return false;
        }

        if (RequiresVisibility(ctx.NoSightCheck) && !ctx.Visible)
        {
            found = false;
            return false;
        }

        found = true;
        return true;
    }

    public static bool FallbackToGoal(bool noTarget, bool hasGoal, bool chaseGoal) =>
        noTarget && hasGoal && chaseGoal;

    public static bool FallbackToLastEnemy(bool noTarget, bool hasLast, int lastHealth, bool isFriend) =>
        noTarget && hasLast && lastHealth > 0 && !isFriend;

    public static bool ClearFriendlyLastEnemy(bool isFriend) => isFriend;

    public readonly record struct LookEnemiesContext(
        bool BlockSearchHit,
        bool HasGoal,
        bool TargetIsGoal,
        bool NoTarget,
        bool HasGoalFallback,
        bool HasLastEnemy,
        int LastHealth,
        bool LastIsFriend);

    public static bool EvaluateLookEnemies(LookEnemiesContext ctx, out bool found)
    {
        if (ctx.BlockSearchHit)
        {
            found = true;
            return true;
        }

        if (FallbackToGoal(ctx.NoTarget, ctx.HasGoalFallback, true))
        {
            found = true;
            return true;
        }

        if (FallbackToLastEnemy(ctx.NoTarget, ctx.HasLastEnemy, ctx.LastHealth, ctx.LastIsFriend))
        {
            found = true;
            return true;
        }

        found = false;
        return false;
    }
}

/// <summary>Minimal stand-in for native actor pointer checks in look VMs.</summary>
public readonly record struct AActorRef(int Tid);
