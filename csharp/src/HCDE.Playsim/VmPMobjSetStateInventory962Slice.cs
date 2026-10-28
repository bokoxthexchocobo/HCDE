namespace HCDE.Playsim;

/// <summary>
/// <c>AActor::SetState</c> tail, <c>DestroyAllInventory</c>, and <c>UseInventory</c> header from <c>p_mobj.cpp</c> (**872–1021**).
/// Exclusive to chunk **962**; does not tally retired chunk **737** / duplicate <c>VmPMobjSetStateUse</c> credit rows.
/// </summary>
public static class VmPMobjSetStateInventory962Slice
{
    public const int StateLoopLimit = 300_000;
    public const int SufActor = 1;
    public const int SprFixed = -1;
    public const int SprNochange = -2;
    public const int Mf4Noskin = 1 << 2;
    public const int Mf8RecreateLights = 1 << 0;
    public const int Level3LightCreated = 1;
    public const int OfEuthanizeMe = 1 << 1;

    public static bool StateLoopExhausted(int remainingIterations) => remainingIterations <= 0;

    public static int PreviousStateSprite(int? currentStateSprite) => currentStateSprite ?? -1;

    public static bool StateUsableOnActor(int useFlags) => (useFlags & SufActor) != 0;

    public static int MergeRenderFullbright(int renderFlags, int fullbrightMask, int fullbrightBits) =>
        (renderFlags & ~fullbrightMask) | fullbrightBits;

    public static bool SpriteChangeAllowed(int newSprite) => newSprite != SprFixed;

    public static bool SpriteValueAllowsChange(int newSprite) => newSprite != SprNochange;

    public static bool SkinSpriteEligible(int flags4, int newSprite, int spawnStateSprite) =>
        (flags4 & Mf4Noskin) == 0 && newSprite == spawnStateSprite;

    public static bool ShouldApplyPlayerSkin(bool hasPlayer, int skinCount) => hasPlayer && skinCount > 0;

    public static bool ShouldKeepBodySpriteAfterRespawn(int newSprite, int previousSprite) => newSprite != previousSprite;

    public static int ResolveSpriteAfterStateChange(
        int flags4,
        int newSprite,
        int spawnStateSprite,
        int previousSprite,
        bool hasPlayer,
        int skinCount,
        int currentSprite,
        Func<int>? pickSkinSprite)
    {
        if (!SpriteChangeAllowed(newSprite))
            return currentSprite;
        if (!SpriteValueAllowsChange(newSprite))
            return currentSprite;
        if (SkinSpriteEligible(flags4, newSprite, spawnStateSprite))
        {
            if (ShouldApplyPlayerSkin(hasPlayer, skinCount))
                return pickSkinSprite?.Invoke() ?? currentSprite;
            if (ShouldKeepBodySpriteAfterRespawn(newSprite, previousSprite))
                return newSprite;
            return currentSprite;
        }

        return newSprite;
    }

    public static bool ShouldAbortSetStateAfterAction(int objectFlags) => (objectFlags & OfEuthanizeMe) != 0;

    public static bool ShouldMarkRecreateLights(
        bool hasLightAssociations,
        bool currentStateLight,
        bool previousStateLight) =>
        hasLightAssociations || currentStateLight || previousStateLight;

    public static int ApplyRecreateLightsFlags(int flags8) => flags8 | Mf8RecreateLights;

    public static void CollectInventoryForDestruction(
        object? head,
        Func<object, object?> getNextLink,
        Action<object> clearOwner,
        Action<object> clearNextLink,
        List<object> pendingDelete,
        ref object? chainHead)
    {
        var inv = chainHead;
        while (inv is not null)
        {
            pendingDelete.Add(inv);
            var next = getNextLink(inv);
            clearNextLink(inv);
            clearOwner(inv);
            inv = next;
        }

        chainHead = null;
    }

    public static void DestroyPendingInventory(
        IEnumerable<object> pendingDelete,
        Func<object, int> getObjectFlags,
        Action<object> destroyItem)
    {
        foreach (var item in pendingDelete)
        {
            if ((getObjectFlags(item) & OfEuthanizeMe) != 0)
                continue;
            destroyItem(item);
        }
    }
}
