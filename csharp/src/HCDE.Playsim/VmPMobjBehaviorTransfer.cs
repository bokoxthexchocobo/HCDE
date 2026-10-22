namespace HCDE.Playsim;

/// <summary>Behavior transfer/clear/link and state sequence from <c>p_mobj.cpp</c> (633–822).</summary>
public static class VmPMobjBehaviorTransfer
{
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

    public static void ClearBehaviorsMatching(bool typeNull, bool behaviorNull, bool isKindOf, ICollection<string> toRemove, string key)
    {
        if (typeNull || behaviorNull || isKindOf)
            toRemove.Add(key);
    }

    public static bool ClearAllBehaviorsWhenTypeNull(bool typeNull, Action? clearMap) =>
        typeNull && (clearMap?.Invoke() != null || true);

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
