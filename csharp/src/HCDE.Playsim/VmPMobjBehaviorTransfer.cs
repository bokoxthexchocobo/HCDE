namespace HCDE.Playsim;

/// <summary>Behavior transfer/clear/link and state sequence from <c>p_mobj.cpp</c> (633–822).</summary>
public static class VmPMobjBehaviorTransfer
{
    public sealed class MoveBehaviorsContext
    {
        public bool SameActor { get; init; }
        public bool ClientSideMismatch { get; init; }
        public Action? ClearTargetBehaviors { get; init; }
        public Action? TransferBehaviorMap { get; init; }
        public IEnumerable<VmPMobjPostSerializeBehaviors.BehaviorMapEntry> Entries { get; init; } =
            Array.Empty<VmPMobjPostSerializeBehaviors.BehaviorMapEntry>();
        public object? NewLevel { get; init; }
        public Func<object?, object?>? BehaviorLevel { get; init; }
        public Action<object?>? RemoveFromOldLevel { get; init; }
        public Action<object?>? AddToNewLevel { get; init; }
        public Func<object, bool>? IsValid { get; init; }
        public Action<object, object?>? RunTransferredOwnerVm { get; init; }
        public object? FromActor { get; init; }
        public Action<object?, object?>? SetOwner { get; init; }
        public Func<object, string>? TypeNameFromBehavior { get; init; }
        public Action<string>? RemoveBehavior { get; init; }
    }

    public static bool RunMoveBehaviors(MoveBehaviorsContext ctx)
    {
        if (MoveBehaviorsRejected(ctx.SameActor, ctx.ClientSideMismatch))
            return false;

        ctx.ClearTargetBehaviors?.Invoke();
        ctx.TransferBehaviorMap?.Invoke();

        var toRemove = new List<string>();
        var toTransfer = new List<object>();
        foreach (var entry in ctx.Entries)
        {
            if (entry.Behavior == null)
            {
                toRemove.Add(entry.Key);
                continue;
            }

            ctx.SetOwner?.Invoke(entry.Behavior, ctx.FromActor);
            var level = ctx.BehaviorLevel?.Invoke(entry.Behavior);
            if (!ReferenceEquals(level, ctx.NewLevel))
            {
                ctx.RemoveFromOldLevel?.Invoke(entry.Behavior);
                ctx.AddToNewLevel?.Invoke(entry.Behavior);
            }

            toTransfer.Add(entry.Behavior);
        }

        foreach (var behavior in toTransfer)
        {
            if (ctx.IsValid?.Invoke(behavior) == false)
            {
                var name = ctx.TypeNameFromBehavior?.Invoke(behavior) ?? "";
                if (name.Length > 0)
                    toRemove.Add(name);
                continue;
            }

            ctx.RunTransferredOwnerVm?.Invoke(behavior, ctx.FromActor);
            if (ctx.IsValid?.Invoke(behavior) == false)
            {
                var name = ctx.TypeNameFromBehavior?.Invoke(behavior) ?? "";
                if (name.Length > 0)
                    toRemove.Add(name);
            }
        }

        VmPMobjPostSerializeBehaviors.FlushBehaviorRemovals(toRemove, ctx.RemoveBehavior);
        return true;
    }

    public static bool MoveBehaviorsRejected(bool sameActor, bool clientSideMismatch) =>
        sameActor || clientSideMismatch;

    public static void MoveBehaviorsClearTarget(Action? clearBehaviors) => clearBehaviors?.Invoke();

    public static bool TransferBehaviorOwner(object? behavior, object? newOwner, Action<object?, object?>? setOwner)
    {
        if (behavior == null)
            return false;
        setOwner?.Invoke(behavior, newOwner);
        return true;
    }

    public static bool RelinkBehaviorLevel(object? behavior, object? oldLevel, object? newLevel, Action<object?>? remove, Action<object?>? add)
    {
        if (behavior == null || oldLevel == newLevel)
            return false;
        remove?.Invoke(behavior);
        add?.Invoke(behavior);
        return true;
    }

    public static bool MoveBehaviorTransferredOwnerVm(bool valid, ICollection<string> toRemove, string key)
    {
        if (valid)
            return false;
        toRemove.Add(key);
        return true;
    }

    public static void RunClearBehaviors(
        IEnumerable<VmPMobjPostSerializeBehaviors.BehaviorMapEntry> entries,
        bool typeNull,
        object? filterType,
        Func<object, object?, bool>? isKindOf,
        ICollection<string> toRemove,
        Action<string>? removeBehavior,
        Action? clearEntireMap)
    {
        foreach (var entry in entries)
            ClearBehaviorsMatching(typeNull, entry.Behavior == null, entry.Behavior != null && isKindOf?.Invoke(entry.Behavior, filterType) == true, toRemove, entry.Key);

        VmPMobjPostSerializeBehaviors.FlushBehaviorRemovals(toRemove, removeBehavior);
        if (ClearAllBehaviorsWhenTypeNull(typeNull, clearEntireMap))
            return;
    }

    public static void ClearBehaviorsMatching(bool typeNull, bool behaviorNull, bool isKindOf, ICollection<string> toRemove, string key)
    {
        if (typeNull || behaviorNull || isKindOf)
            toRemove.Add(key);
    }

    public static bool ClearAllBehaviorsWhenTypeNull(bool typeNull, Action? clearMap)
    {
        if (!typeNull)
            return false;
        clearMap?.Invoke();
        return true;
    }

    public static void RunUnlinkBehaviorsFromLevel(
        IEnumerable<VmPMobjPostSerializeBehaviors.BehaviorMapEntry> entries,
        ICollection<string> toRemove,
        Action<object>? unlinkFromLevel,
        Action<string>? removeBehavior)
    {
        foreach (var entry in entries)
        {
            if (UnlinkBehaviorNull(entry.Behavior, toRemove, entry.Key, unlinkFromLevel))
                continue;
        }

        VmPMobjPostSerializeBehaviors.FlushBehaviorRemovals(toRemove, removeBehavior);
    }

    public static void RunLinkBehaviorsToLevel(
        IEnumerable<VmPMobjPostSerializeBehaviors.BehaviorMapEntry> entries,
        ICollection<string> toRemove,
        Action<object>? linkToLevel,
        Action<string>? removeBehavior)
    {
        foreach (var entry in entries)
        {
            if (entry.Behavior == null)
            {
                toRemove.Add(entry.Key);
                continue;
            }

            linkToLevel?.Invoke(entry.Behavior);
        }

        VmPMobjPostSerializeBehaviors.FlushBehaviorRemovals(toRemove, removeBehavior);
    }

    public static bool UnlinkBehaviorNull(object? behavior, ICollection<string> toRemove, string key, Action<object>? unlink)
    {
        if (behavior == null)
        {
            toRemove.Add(key);
            return true;
        }

        unlink?.Invoke(behavior);
        return false;
    }

    public static bool InStateSequence(object? newState, object? baseState, Func<object?, object?>? nextState, Func<object?, object?, bool>? adjacent)
    {
        if (baseState == null)
            return false;
        var cursor = baseState;
        do
        {
            if (ReferenceEquals(newState, cursor))
                return true;
            var next = nextState?.Invoke(cursor);
            if (next == null || adjacent?.Invoke(cursor, next) != true)
                break;
            cursor = next;
        }
        while (true);
        return false;
    }

    public static bool IsMapActorInventory(bool isInventory, object? owner) =>
        !isInventory || owner == null;
}
