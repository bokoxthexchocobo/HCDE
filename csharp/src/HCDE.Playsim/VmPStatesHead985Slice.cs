namespace HCDE.Playsim;

/// <summary>
/// `GetSpriteTexture`, state-owner lookup, state naming, and label find from <c>p_states.cpp</c> (**1–150**).
/// </summary>
public static class VmPStatesHead985Slice
{
    public const int UseSelfSprite = -1;
    public const int UseSelfFrame = -1;
    public const int MaxSpriteTextureReturns = 3;

    public static int SelectEffectiveSprite(int spriteArgument, int selfSprite) =>
        spriteArgument == UseSelfSprite ? selfSprite : spriteArgument;

    public static int SelectEffectiveFrame(int frameArgument, int selfFrame) =>
        frameArgument == UseSelfFrame ? selfFrame : frameArgument;

    public static bool UsesAlternateSkinSprite(int skinIndex) => skinIndex != 0;

    public static int CapSpriteTextureReturnCount(int numReturnValues) =>
        numReturnValues < MaxSpriteTextureReturns ? numReturnValues : MaxSpriteTextureReturns;

    public static bool ShouldWriteTextureReturn(int numReturnValues) => numReturnValues > 0;

    public static bool ShouldWriteFlipReturn(int numReturnValues) => numReturnValues > 1;

    public static bool ShouldWriteScaleReturn(int numReturnValues) => numReturnValues > 2;

    public static bool SpriteFlipForRotation(int flipField, int rotation) =>
        (flipField & (1 << rotation)) != 0;

    public static object? ScanAllActorClassesForStateOwner(
        object? state,
        Func<int, object?>? getActorClassAt,
        Func<object?, object?, bool>? ownsState,
        int classCount)
    {
        if (state is null || getActorClassAt is null || ownsState is null || classCount <= 0)
            return null;

        for (var i = 0; i < classCount; i++)
        {
            var info = getActorClassAt(i);
            if (info is null)
                continue;
            if (ownsState(info, state))
                return info;
        }

        return null;
    }

    public static object? FindStateOwnerInHierarchy(
        object? state,
        object? startClass,
        Func<object?, object?>? getParentClass,
        Func<object?, object?, bool>? ownsState)
    {
        if (state is null || startClass is null || getParentClass is null || ownsState is null)
            return null;

        var info = startClass;
        while (info is not null)
        {
            if (ownsState(info, state))
                return info;
            info = getParentClass(info);
        }

        return null;
    }

    public static bool ShouldFormatDehExtraStateName(int dehIndex) => dehIndex > 0;

    public static string FormatDehExtraStateName(int dehIndex) => $"DehExtraState.{dehIndex}";

    public static string FormatTypedStateName(string typeName, int stateIndex) =>
        $"{typeName}.{stateIndex}";

    public static string ResolveStateDisplayName(
        object? state,
        object? globalOwner,
        object? hierarchyOwner,
        int dehIndex,
        Func<object?, int>? stateIndexFromOwner)
    {
        var owner = globalOwner ?? hierarchyOwner;
        if (owner is null)
        {
            return ShouldFormatDehExtraStateName(dehIndex)
                ? FormatDehExtraStateName(dehIndex)
                : "<unknown>";
        }

        var index = stateIndexFromOwner?.Invoke(state) ?? 0;
        var typeName = owner.ToString() ?? "Actor";
        return FormatTypedStateName(typeName, index);
    }

    public static bool StateLabelLookupEnabled(int labelCount) => labelCount > 0;
}
