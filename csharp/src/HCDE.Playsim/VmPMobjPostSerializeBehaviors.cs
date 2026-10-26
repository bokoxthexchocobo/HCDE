namespace HCDE.Playsim;

/// <summary>Post-serialize and behavior attach/tick from <c>p_mobj.cpp</c> (431–631).</summary>
public static class VmPMobjPostSerializeBehaviors
{
    public const int Mf4Noskin = 1;
    public const int OfTransferrableFlags = 1;

    public sealed class PostSerializeContext
    {
        public bool HasPlayer { get; init; }
        public bool PlayerInGame { get; init; }
        public bool ClsValid { get; init; }
        public int Flags4 { get; init; }
        public bool SpawnSpriteMatch { get; init; }
        public double Speed { get; init; }
        public Action? ClearSectorList { get; init; }
        public Action? ClearRenderSectors { get; init; }
        public Action? LinkToWorld { get; init; }
        public Action? AddToHash { get; init; }
        public Action? RestoreSkin { get; init; }
        public Action? RestoreDefaultSpeed { get; init; }
        public Action? ClearInterpolation { get; init; }
        public Action? ClearFovInterpolation { get; init; }
        public Action<bool>? UpdateWaterLevel { get; init; }
    }

    public sealed class AddBehaviorContext
    {
        public bool AbstractType { get; init; }
        public bool DescendantOfBehavior { get; init; }
        public object? Existing { get; init; }
        public Func<object?>? CreateBehavior { get; init; }
        public Action<object?>? RegisterBehavior { get; init; }
        public Action<object?>? AddToLevel { get; init; }
        public Func<object, bool>? IsValidAfterInitialize { get; init; }
        public Action? RunInitializeVm { get; init; }
        public Func<object, bool>? IsValidAfterReinitialize { get; init; }
        public Action? RunReinitializeVm { get; init; }
        public Action<string>? RemoveBehavior { get; init; }
        public string TypeName { get; init; } = "";
        public int OwnerObjectFlags { get; init; }
    }

    public readonly record struct BehaviorMapEntry(string Key, object? Behavior);

    public static void RunPostSerialize(PostSerializeContext ctx)
    {
        PostSerializeClearTouchLists(ctx.ClearSectorList, ctx.ClearRenderSectors, ctx.LinkToWorld, ctx.AddToHash);
        if (ctx.HasPlayer)
        {
            if (ShouldRestorePlayerSkin(ctx.PlayerInGame, ctx.ClsValid, ctx.Flags4, ctx.SpawnSpriteMatch))
                ctx.RestoreSkin?.Invoke();
            if (ShouldRestoreDefaultSpeed(ctx.Speed))
                ctx.RestoreDefaultSpeed?.Invoke();
        }

        ctx.ClearInterpolation?.Invoke();
        ctx.ClearFovInterpolation?.Invoke();
        ctx.UpdateWaterLevel?.Invoke(false);
    }

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

    public static bool RunRemoveBehavior(string typeName, object? behavior, Action? destroy, Action<string>? removeKey)
    {
        var had = behavior != null;
        if (had)
            destroy?.Invoke();
        removeKey?.Invoke(typeName);
        return had;
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

    public static bool AddBehaviorTypeRejected(bool abstractType, bool descendant) =>
        abstractType || !descendant;

    public static bool AddBehaviorAbstract(bool abstractType, bool descendant) =>
        !abstractType && descendant;

    public static object? RunAddBehavior(AddBehaviorContext ctx)
    {
        if (AddBehaviorTypeRejected(ctx.AbstractType, ctx.DescendantOfBehavior))
            return null;

        if (ctx.Existing == null)
        {
            var created = ctx.CreateBehavior?.Invoke();
            if (created == null)
                return null;
            ctx.RegisterBehavior?.Invoke(created);
            ApplyTransferrableFlags(ctx.OwnerObjectFlags, created);
            ctx.AddToLevel?.Invoke(created);
            ctx.RunInitializeVm?.Invoke();
            if (ctx.IsValidAfterInitialize?.Invoke(created) == false)
            {
                ctx.RemoveBehavior?.Invoke(ctx.TypeName);
                return null;
            }

            return created;
        }

        ctx.RunReinitializeVm?.Invoke();
        if (ctx.IsValidAfterReinitialize?.Invoke(ctx.Existing) == false)
        {
            ctx.RemoveBehavior?.Invoke(ctx.TypeName);
            return null;
        }

        return ctx.Existing;
    }

    public static void ApplyTransferrableFlags(int ownerFlags, object behavior) =>
        _ = ownerFlags & OfTransferrableFlags;

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

    public static void RunTickBehaviors(
        IEnumerable<BehaviorMapEntry> behaviors,
        ICollection<string> toRemove,
        ICollection<object> toTick,
        Func<object, bool>? isValid,
        Action<object>? tickVm,
        Func<object, string>? typeNameFromBehavior,
        Action<string>? removeBehavior)
    {
        foreach (var entry in behaviors)
        {
            if (TickBehaviorsQueuesNull(entry.Behavior, toRemove, entry.Key))
                continue;
            toTick.Add(entry.Behavior!);
        }

        foreach (var behavior in toTick)
        {
            if (isValid?.Invoke(behavior) == false)
            {
                var name = typeNameFromBehavior?.Invoke(behavior) ?? "";
                if (name.Length > 0)
                    toRemove.Add(name);
                continue;
            }

            tickVm?.Invoke(behavior);
            if (isValid?.Invoke(behavior) == false)
            {
                var name = typeNameFromBehavior?.Invoke(behavior) ?? "";
                if (name.Length > 0)
                    toRemove.Add(name);
            }
        }

        FlushBehaviorRemovals(toRemove, removeBehavior);
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
