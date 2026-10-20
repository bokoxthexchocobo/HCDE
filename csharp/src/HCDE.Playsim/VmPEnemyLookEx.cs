namespace HCDE.Playsim;

/// <summary><c>A_LookEx</c> parameterized look gates from <c>p_enemy.cpp</c> (2063–2245, excluding <c>P_LookForPlayers</c> calls).</summary>
public static class VmPEnemyLookEx
{
    public const int Mf5Inconversation = 1;
    public const int LofNosoundcheck = 1;
    public const int LofNosightcheck = 1;
    public const int LofNoseesound = 1;
    public const int LofFullvolseesound = 1;
    public const int LofNojump = 1;
    public const int MfStealth = 1;
    public const int MfShootable = 4;
    public const int Mf4Standstill = 1;
    public const int MfAmbush = 1;
    public const int Mf7Inchase = 1;

    public static bool ConversationExit(int flags5) => (flags5 & Mf5Inconversation) != 0;

    public static bool PatrolGoalSpecial(int special, int arg0) => special == 14 && arg0 == 0;

    public static bool ChaseGoalFromArg(int arg3) => arg3 != 0;

    public static bool NullFovUsesDefault(bool nullFov) => nullFov;

    public static bool TidHateUsesTarget(int tidHate) => tidHate != 0;

    public static bool SoundTargetDead(int health) => health <= 0;

    public static bool HearDistanceBlocks(double dist, double max) => max != 0 && dist > max;

    public static bool NotargetCheat(bool notarget) => notarget;

    public static bool StealthVisdir(bool stealth) => stealth;

    public static bool ShootableTarget(int flags) => (flags & MfShootable) != 0;

    public static bool FriendBranch(bool friend) => friend;

    public static bool StandstillBlocksWander(int flags4) => (flags4 & Mf4Standstill) != 0;

    public static bool AmbushSightRequired(bool ambush, bool sight) => ambush && sight;

    public static bool SelfTargetNull(bool same) => same;

    public static bool GoalReactionBlocks(bool targetIsGoal, int maptime, int reaction) =>
        targetIsGoal && reaction > maptime;

    public static bool FullVolumeSee(int flags) => (flags & LofFullvolseesound) != 0;

    public static bool InchaseSkipsSee(int flags7) => (flags7 & Mf7Inchase) != 0;

    public static bool NoJumpBlocks(int flags) => (flags & LofNojump) != 0;

    public readonly record struct LookExGoalContext(int Special, int Arg0, int Arg1, int Arg2, int Arg3, int Maptime);

    public static bool EvaluatePatrolGoal(LookExGoalContext ctx) =>
        PatrolGoalSpecial(ctx.Special, ctx.Arg0);

    public readonly record struct LookExSoundContext(
        bool TidHate,
        int Health,
        double Dist,
        double MaxHear,
        bool Notarget,
        int Flags);

    public static bool EvaluateSoundTarget(LookExSoundContext ctx)
    {
        if (ctx.TidHate) return true;
        if ((ctx.Flags & LofNosoundcheck) != 0) return false;
        if (SoundTargetDead(ctx.Health)) return false;
        if (HearDistanceBlocks(ctx.Dist, ctx.MaxHear)) return false;
        if (NotargetCheat(ctx.Notarget)) return false;
        return true;
    }

    public readonly record struct LookExSeeYouContext(
        bool TargetIsGoal,
        int Maptime,
        int Reactiontime,
        bool SeeSoundValid,
        int Flags,
        int Flags7,
        bool HasTarget,
        bool NoJump);

    public static bool EvaluateSeeYouTail(LookExSeeYouContext ctx)
    {
        if (GoalReactionBlocks(ctx.TargetIsGoal, ctx.Maptime, ctx.Reactiontime)) return false;
        if (ctx.SeeSoundValid && (ctx.Flags & LofNoseesound) == 0)
        {
            if (FullVolumeSee(ctx.Flags)) { }
        }
        if (!ctx.HasTarget || InchaseSkipsSee(ctx.Flags7)) return false;
        return !NoJumpBlocks(ctx.Flags);
    }
}
