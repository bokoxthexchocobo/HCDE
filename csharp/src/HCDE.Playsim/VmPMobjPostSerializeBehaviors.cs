namespace HCDE.Playsim;

/// <summary>Post-serialize and behavior attach/tick from <c>p_mobj.cpp</c> (431–631).</summary>
public static class VmPMobjPostSerializeBehaviors
{
    public const int Mf4Noskin = 1;

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

    public static bool RemoveBehaviorFound(object? behavior, Action? destroy)
    {
        if (behavior == null)
            return false;
        destroy?.Invoke();
        return true;
    }

    public static bool AddBehaviorAbstract(bool abstractType, bool descendant) =>
        !abstractType && descendant;

    public static bool AddBehaviorCreatesNew(object? existing, Func<object?>? create, Action<object?>? register)
    {
        if (existing != null)
            return false;
        var created = create?.Invoke();
        if (created == null)
            return false;
        register?.Invoke(created);
        return true;
    }

    public static bool TickBehaviorsQueuesNull(object? behavior, ICollection<string> toRemove, string key)
    {
        if (behavior != null)
            return false;
        toRemove.Add(key);
        return true;
    }
}
