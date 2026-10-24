namespace HCDE.Playsim;

/// <summary><c>CLOF_flags</c> and <c>CheckLOFTraceFunc</c> from <c>p_actionfunctions.cpp</c> (2239–2356).</summary>
public static class VmActionCheckLofTrace
{
    public const int CloffNoaimVert = 1 << 0;
    public const int CloffJumpEnemy = 1 << 2;
    public const int CloffJumpFriend = 1 << 3;
    public const int CloffJumpObject = 1 << 4;
    public const int CloffJumpNonhostile = 1 << 5;
    public const int CloffSkipEnemy = 1 << 6;
    public const int CloffSkipFriend = 1 << 7;
    public const int CloffSkipObject = 1 << 8;
    public const int CloffSkipNonhostile = 1 << 9;
    public const int CloffMustBeShootable = 1 << 10;
    public const int CloffSkipTarget = 1 << 11;
    public const int CloffBeyondTarget = 1 << 17;
    public const int CloffMustBeSolid = 1 << 16;
    public const int CloffMustBeGhost = 1 << 14;
    public const int CloffIgnoreGhost = 1 << 15;
    public const int MfShootable = 1 << 9;
    public const int Mf2Nonshootable = 1 << 0;
    public const int MfSolid = 1 << 1;
    public const int Mf3Ghost = 1 << 5;
    public const int Mf3IsMonster = 1 << 14;

    public enum TraceDecision
    {
        Stop,
        Skip,
        Abort,
    }

    public static TraceDecision EvaluateHitActor(
        bool hitActor,
        object? traceActor,
        object? targetActor,
        int flags,
        bool selfHostileToActor,
        bool selfFriendToActor,
        bool actorIsMonster,
        int actorFlags,
        int actorFlags2,
        int actorFlags3,
        ref bool badActor)
    {
        if (!hitActor)
            return TraceDecision.Stop;

        if (ReferenceEquals(traceActor, targetActor))
        {
            if ((flags & CloffSkipTarget) != 0)
            {
                if ((flags & CloffBeyondTarget) != 0)
                    return TraceDecision.Skip;
                return TraceDecision.Abort;
            }

            return TraceDecision.Stop;
        }

        if ((flags & CloffMustBeShootable) != 0)
        {
            if ((actorFlags & MfShootable) == 0 || (actorFlags2 & Mf2Nonshootable) != 0)
                return TraceDecision.Skip;
        }

        if ((flags & CloffMustBeSolid) != 0 && (actorFlags & MfSolid) == 0)
            return TraceDecision.Skip;

        if ((flags & CloffMustBeGhost) != 0)
        {
            if ((actorFlags3 & Mf3Ghost) == 0)
                return TraceDecision.Skip;
        }
        else if ((flags & CloffIgnoreGhost) != 0 && (actorFlags3 & Mf3Ghost) != 0)
            return TraceDecision.Skip;

        if (((flags & CloffJumpEnemy) != 0 && selfHostileToActor) ||
            ((flags & CloffJumpFriend) != 0 && selfFriendToActor) ||
            ((flags & CloffJumpObject) != 0 && (actorFlags3 & Mf3IsMonster) == 0) ||
            ((flags & CloffJumpNonhostile) != 0 && (actorFlags3 & Mf3IsMonster) != 0 && !selfHostileToActor))
            return TraceDecision.Stop;

        if (((flags & CloffSkipEnemy) != 0 && selfHostileToActor) ||
            ((flags & CloffSkipFriend) != 0 && selfFriendToActor) ||
            ((flags & CloffSkipObject) != 0 && (actorFlags3 & Mf3IsMonster) == 0) ||
            ((flags & CloffSkipNonhostile) != 0 && (actorFlags3 & Mf3IsMonster) != 0 && !selfHostileToActor))
            return TraceDecision.Skip;

        badActor = true;
        return TraceDecision.Abort;
    }
}
