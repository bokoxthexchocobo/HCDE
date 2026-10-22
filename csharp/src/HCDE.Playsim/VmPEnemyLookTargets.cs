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

    public sealed class LookActorState
    {
        public required string ActorId { get; init; }
        public int Species { get; set; }
        public int TidToHate { get; set; }
        public int Flags3 { get; set; }
        public object? Target { get; set; }
        public object? Goal { get; set; }
        public object? LastLookActor { get; set; }
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
        public Func<object?, bool>? IsVisible { get; init; }
        public Func<object, int, object?>? BlockmapSearch { get; init; }
        public Func<int, object?, IEnumerable<object?>>? ActorIterator { get; init; }
        public Func<object, object, bool>? ValidEnemyInBlock { get; init; }
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

    public static bool LookForMonsters(LookActorState actor)
    {
        if (actor.PlayerCanSeeActor?.Invoke() != true) return false;
        var count = 0;
        foreach (var mo in actor.EnumerateMonsters?.Invoke() ?? Array.Empty<MonsterCandidate>())
        {
            if ((mo.Flags3 & Mf3Ismonster) == 0 || mo.Id == actor.ActorId || mo.Health <= 0) continue;
            if (mo.Distance2d > MonsLookRange) continue;
            if ((actor.PrLookForMonsters?.Invoke() ?? 255) < 16) continue;
            if (++count >= MonsLookLimit) return false;
            if (mo.Species == actor.Species) continue;
            if (actor.CheckSight?.Invoke(actor.ActorId, mo.Id) != true) continue;
            actor.Target = mo.Id;
            return true;
        }

        return false;
    }

    public static object? LookForTidInBlock(LookActorState lookee, IEnumerable<BlockLink> links)
    {
        foreach (var link in links)
        {
            if ((link.Flags & MfShootable) == 0 || link.Id == lookee.ActorId || link.Health <= 0) continue;
            if ((link.Flags2 & Mf2Dormant) != 0) continue;
            object? other;
            if (link.Tid == lookee.TidToHate) other = link.Id;
            else if (link.Target != null && link.TargetTid == lookee.TidToHate)
            {
                other = link.Target;
                if ((link.TargetFlags & MfShootable) == 0 || link.TargetHealth <= 0 || (link.TargetFlags2 & Mf2Dormant) != 0)
                    continue;
            }
            else continue;

            if ((lookee.Flags3 & Mf3Nosightcheck) == 0 && lookee.IsVisible?.Invoke(other) != true) continue;
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
            if (actor.Goal != null && ReferenceEquals(actor.Target, actor.Goal)) actor.ReactionTime = 0;
            actor.Target = other;
            actor.LastLookActor = other;
            return true;
        }

        var c = ((actor.PrLook3?.Invoke() ?? 0) & 31) + 7;
        var reachedend = false;
        foreach (var candidate in actor.ActorIterator?.Invoke(actor.TidToHate, actor.LastLookActor) ?? Array.Empty<object?>())
        {
            if (candidate == null)
            {
                if (reachedend) break;
                reachedend = true;
                continue;
            }

            if (candidate is BlockLink bl)
            {
                if ((bl.Flags & MfShootable) == 0 || bl.Id == actor.ActorId || bl.Health <= 0) continue;
                if ((bl.Flags2 & Mf2Dormant) != 0) continue;
                if (--c == 0) break;
                if ((actor.Flags3 & Mf3Nosightcheck) == 0 && actor.IsVisible?.Invoke(candidate) != true) continue;
                if (actor.Goal != null && ReferenceEquals(actor.Target, actor.Goal)) actor.ReactionTime = 0;
                actor.Target = candidate;
                actor.LastLookActor = candidate;
                return true;
            }
        }

        if (actor.Target == null)
        {
            if (actor.Goal != null && chasegoal) { actor.Target = actor.Goal; return true; }
            if (actor.LastEnemy != null && actor.LastEnemyHealth > 0 && !actor.LastEnemyIsFriend)
            {
                actor.Target = actor.LastEnemy;
                actor.LastEnemy = null;
                return true;
            }

            actor.LastEnemy = null;
        }

        return false;
    }

    public static object? LookForEnemiesInBlock(LookActorState lookee, IEnumerable<BlockLink> links)
    {
        foreach (var link in links)
            if (lookee.ValidEnemyInBlock?.Invoke(lookee, link) == true)
                return link.Id;
        return null;
    }

    public static bool LookForEnemies(LookActorState actor)
    {
        var other = actor.BlockmapSearch?.Invoke(actor, actor.FriendlySeeBlocks);
        if (other != null)
        {
            if (actor.Goal != null && ReferenceEquals(actor.Target, actor.Goal)) actor.ReactionTime = 0;
            actor.Target = other;
            return true;
        }

        if (actor.Target == null)
        {
            if (actor.Goal != null) { actor.Target = actor.Goal; return true; }
            if (actor.LastEnemy != null && actor.LastEnemyHealth > 0 && !actor.LastEnemyIsFriend)
            {
                actor.Target = actor.LastEnemy;
                actor.LastEnemy = null;
                return true;
            }

            actor.LastEnemy = null;
        }

        return false;
    }
}
