namespace HCDE.Playsim;

/// <summary>Monster look / TID / enemy search from <c>p_enemy.cpp</c> (1489–1777).</summary>
public static class VmPEnemyLookTargets
{
    public const int MonsLookRange = 20 * 64;
    public const int MonsLookLimit = 64;
    public const int PrLookForMonstersThreshold = 16;
    public const int Mf3Ismonster = 1;
    public const int MfShootable = 1;
    public const int Mf2Dormant = 1 << 1;
    public const int Mf3Nosightcheck = 1 << 2;
    public const int LofDontchasegoal = 1;

    public sealed class LookActorState
    {
        public required string ActorId { get; init; }
        public int Species { get; set; }
        public int TidToHate { get; set; }
        public int Flags3 { get; set; }
        public object? Target { get; set; }
        public object? Goal { get; set; }
        public object? LastLookActor { get; set; }
        public int LastLookActorTid { get; set; }
        public object? LastEnemy { get; set; }
        public int LastEnemyHealth { get; set; }
        public bool LastEnemyIsFriend { get; set; }
        public int ReactionTime { get; set; }
        public int FriendlySeeBlocks { get; set; }
        public bool HasParams { get; set; }
        public int ParamFlags { get; set; }
        public Func<bool>? PlayerCanSeeActor { get; init; }
        public Func<IEnumerable<MonsterCandidate>>? EnumerateMonsters { get; init; }
        public Func<string, string, bool>? CheckSight { get; init; }
        public Func<int>? PrLookForMonsters { get; init; }
        public Func<int>? PrLook3 { get; init; }
        public Func<object, bool, bool>? IsVisible { get; init; }
        public Func<object, int, object?>? BlockmapSearch { get; init; }
        public Func<int, object?, IEnumerable<object?>>? ActorIterator { get; init; }
        public Func<object, object, bool>? ValidEnemyInBlock { get; init; }
        public Func<object, bool>? IsFriend { get; init; }
    }

    public sealed class MonsterCandidate
    {
        public required string Id { get; init; }
        public int Flags3 { get; init; }
        public int Health { get; init; }
        public int Species { get; init; }
        public double Distance2d { get; init; }
    }

    public sealed class BlockLink
    {
        public required string Id { get; init; }
        public int Flags { get; init; }
        public int Flags2 { get; init; }
        public int Health { get; init; }
        public int Tid { get; init; }
        public object? Target { get; init; }
        public int TargetFlags { get; init; }
        public int TargetHealth { get; init; }
        public int TargetFlags2 { get; init; }
        public int TargetTid { get; init; }
    }

    static void ResetReactionIfLeavingGoal(LookActorState actor)
    {
        if (actor.Goal != null && ReferenceEquals(actor.Target, actor.Goal))
            actor.ReactionTime = 0;
    }

    static bool TryAssignGoalOrLastEnemy(LookActorState actor, bool chasegoal)
    {
        if (actor.Goal != null && chasegoal)
        {
            actor.Target = actor.Goal;
            return true;
        }

        if (actor.LastEnemy != null && actor.LastEnemyHealth > 0)
        {
            if (actor.IsFriend?.Invoke(actor.LastEnemy) == true || actor.LastEnemyIsFriend)
            {
                actor.LastEnemy = null;
                return false;
            }

            actor.Target = actor.LastEnemy;
            actor.LastEnemy = null;
            return true;
        }

        actor.LastEnemy = null;
        return false;
    }

    static void ClearLastLookActorIfTidMismatch(LookActorState actor)
    {
        if (actor.LastLookActor != null && actor.LastLookActorTid != actor.TidToHate)
            actor.LastLookActor = null;
    }

    static bool IsShootableActive(BlockLink link, string selfId) =>
        (link.Flags & MfShootable) != 0 && link.Id != selfId && link.Health > 0 && (link.Flags2 & Mf2Dormant) == 0;

    public static bool WithinMonsterLookRange(double distance2d) => distance2d <= MonsLookRange;

    public static bool MonsterLookRollPasses(LookActorState actor) =>
        (actor.PrLookForMonsters?.Invoke() ?? 255) >= PrLookForMonstersThreshold;

    public static bool SameMonsterSpecies(LookActorState actor, MonsterCandidate mo) =>
        mo.Species == actor.Species;

    public static bool RequiresVisibilityCheck(LookActorState actor) =>
        (actor.Flags3 & Mf3Nosightcheck) == 0;

    public static bool TidHateMatchesLink(LookActorState lookee, BlockLink link) =>
        link.Tid == lookee.TidToHate;

    public static bool TidHateMatchesTarget(BlockLink link, int tidToHate) =>
        link.Target != null && link.TargetTid == tidToHate;

    public static bool LookForMonsters(LookActorState actor)
    {
        if (actor.PlayerCanSeeActor?.Invoke() != true)
            return false;

        var count = 0;
        foreach (var mo in actor.EnumerateMonsters?.Invoke() ?? Array.Empty<MonsterCandidate>())
        {
            if ((mo.Flags3 & Mf3Ismonster) == 0 || mo.Id == actor.ActorId || mo.Health <= 0)
                continue;
            if (!WithinMonsterLookRange(mo.Distance2d))
                continue;
            if (!MonsterLookRollPasses(actor))
                continue;
            if (++count >= MonsLookLimit)
                return false;
            if (SameMonsterSpecies(actor, mo))
                continue;
            if (actor.CheckSight?.Invoke(actor.ActorId, mo.Id) != true)
                continue;
            actor.Target = mo.Id;
            return true;
        }

        return false;
    }

    public static object? LookForTidInBlock(LookActorState lookee, IEnumerable<BlockLink> links)
    {
        foreach (var link in links)
        {
            if (!IsShootableActive(link, lookee.ActorId))
                continue;

            object? other;
            if (TidHateMatchesLink(lookee, link))
                other = link.Id;
            else if (TidHateMatchesTarget(link, lookee.TidToHate))
            {
                other = link.Target;
                if ((link.TargetFlags & MfShootable) == 0 || link.TargetHealth <= 0 || (link.TargetFlags2 & Mf2Dormant) != 0)
                    continue;
            }
            else
                continue;

            if (RequiresVisibilityCheck(lookee) && lookee.IsVisible?.Invoke(other, true) != true)
                continue;
            return other;
        }

        return null;
    }

    public static bool LookForTid(LookActorState actor, bool allaround)
    {
        var chasegoal = !actor.HasParams || (actor.ParamFlags & LofDontchasegoal) == 0;
        var other = actor.BlockmapSearch?.Invoke(actor, 0);
        if (other != null)
        {
            ResetReactionIfLeavingGoal(actor);
            actor.Target = other;
            actor.LastLookActor = other;
            return true;
        }

        ClearLastLookActorIfTidMismatch(actor);

        var c = ((actor.PrLook3?.Invoke() ?? 0) & 31) + 7;
        var reachedend = false;
        object? loopOther = null;
        foreach (var candidate in actor.ActorIterator?.Invoke(actor.TidToHate, actor.LastLookActor) ?? Array.Empty<object?>())
        {
            loopOther = candidate;
            if (candidate == null)
            {
                if (reachedend)
                    break;
                reachedend = true;
                continue;
            }

            if (candidate is BlockLink bl)
            {
                if (!IsShootableActive(bl, actor.ActorId))
                    continue;
                if (--c == 0)
                    break;
                if (RequiresVisibilityCheck(actor) && actor.IsVisible?.Invoke(candidate, allaround) != true)
                    continue;
                ResetReactionIfLeavingGoal(actor);
                actor.Target = candidate;
                actor.LastLookActor = candidate;
                return true;
            }
        }

        actor.LastLookActor = loopOther;
        if (actor.Target == null)
            return TryAssignGoalOrLastEnemy(actor, chasegoal);
        return false;
    }

    public static object? LookForEnemiesInBlock(LookActorState lookee, IEnumerable<BlockLink> links)
    {
        foreach (var link in links)
        {
            if (lookee.ValidEnemyInBlock?.Invoke(lookee, link) == true)
                return link.Id;
        }

        return null;
    }

    public static bool LookForEnemies(LookActorState actor)
    {
        var other = actor.BlockmapSearch?.Invoke(actor, actor.FriendlySeeBlocks);
        if (other != null)
        {
            ResetReactionIfLeavingGoal(actor);
            actor.Target = other;
            return true;
        }

        if (actor.Target == null)
            return TryAssignGoalOrLastEnemy(actor, chasegoal: true);
        return false;
    }
}
