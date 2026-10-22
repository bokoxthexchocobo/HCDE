namespace HCDE.Playsim;

/// <summary>Post-serialize and behavior attach/tick from <c>p_mobj.cpp</c> (431–631).</summary>
public static class VmPMobjPostSerializeBehaviors
{
    public const int Mf4Noskin = 1;
    public const int OfTransferrableFlags = 1;

    public static void PostSerializeClearTouchLists(Action? clearSectorList, Action? clearRenderSectors, Action? linkToWorld, Action? addToHash)
    {
        clearSectorList?.Invoke();
        clearRenderSectors?.Invoke();
        linkToWorld?.Invoke();
        addToHash?.Invoke();
    }

    public static bool ShouldRestorePlayerSkin(bool playerInGame, bool clsValid, int flags4, bool spawnSpriteMatch) =>
        playerInGame && clsValid && (flags4 & Mf4Noskin) == 0 && spawnSpriteMatch;

    public static bool ShouldRestoreDefaultSpeed(double speed) => speed == 0;

    public static void SerializeBehaviorOwnerFields(bool writing, object? owner, object? level)
    {
        if (!writing)
            return;
        _ = owner;
        _ = level;
    }

    public static bool RemoveBehaviorOnLevelDestroy(object? level, Action? removeFromLevel)
    {
        if (level == null)
            return false;
        removeFromLevel?.Invoke();
        return true;
    }

    public static bool RemoveBehaviorFound(object? behavior, Action? destroy)
    {
        if (behavior == null)
            return false;
        destroy?.Invoke();
        return true;
    }

    public static bool RemoveBehaviorClearsMap(bool hadBehavior, Action? removeKey)
    {
        if (!hadBehavior)
            return false;
        removeKey?.Invoke();
        return true;
    }

    public static bool AddBehaviorAbstract(bool abstractType, bool descendant) =>
        !abstractType && descendant;

    public static bool AddBehaviorCreatesNew(object? existing, Func<object?>? create, Action<object?>? register, Action<object?>? addToLevel)
    {
        if (existing != null)
            return false;
        var created = create?.Invoke();
        if (created == null)
            return false;
        register?.Invoke(created);
        addToLevel?.Invoke(created);
        return true;
    }

    public static bool AddBehaviorReinitialize(object? existing, Func<bool>? reinit, Action? removeOnInvalid)
    {
        if (existing == null)
            return false;
        if (reinit?.Invoke() == false)
        {
            removeOnInvalid?.Invoke();
            return false;
        }
        return true;
    }

    public static bool TickBehaviorsQueuesNull(object? behavior, ICollection<string> toRemove, string key)
    {
        if (behavior != null)
            return false;
        toRemove.Add(key);
        return true;
    }

    public static bool TickBehaviorsInvalidAfterVm(bool valid, ICollection<string> toRemove, string key)
    {
        if (valid)
            return false;
        toRemove.Add(key);
        return true;
    }

    public static void FlushBehaviorRemovals(IEnumerable<string> toRemove, Action<string>? removeBehavior)
    {
        foreach (var key in toRemove)
            removeBehavior?.Invoke(key);
    }

    public static object? FindBehaviorInMap(object? behavior) => behavior;
}
