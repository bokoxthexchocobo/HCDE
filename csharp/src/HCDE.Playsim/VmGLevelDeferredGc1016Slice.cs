namespace HCDE.Playsim;

/// <summary>
/// Visited read tail, ACS deferred save/load, and <c>DSectorMarker</c> GC stepping from <c>g_level.cpp</c> (**2259–2408**).
/// </summary>
public static class VmGLevelDeferredGc1016Slice
{
    public const int MaxPlayers = 8;
    public const int SectorStepSize = 32;
    public const int PolyStepSize = 120;
    public const int SideDefStepSize = 240;
    public const int SectorInterpolationSlots = 4;
    public const int SideTextureInterpolationSlots = 3;
    public const string DeferredObjectKey = "deferred";

    public readonly struct SteppedPropagateBatch
    {
        public int ItemsProcessed { get; init; }
        public int NextIndex { get; init; }
        public bool MoreRemaining { get; init; }
    }

    public static bool ShouldIteratePlayerClassSlot(int playerIndex) =>
        playerIndex >= 0 && playerIndex < MaxPlayers;

    public enum PlayerClassRestoreAction
    {
        UpdateDefault,
        DeserializeSaved,
    }

    public static PlayerClassRestoreAction ResolvePlayerClassRestoreAction(bool hasSavedKey) =>
        hasSavedKey ? PlayerClassRestoreAction.DeserializeSaved : PlayerClassRestoreAction.UpdateDefault;

    public static bool AnyWadLevelHasDeferred(IReadOnlyList<int> deferredSizesPerLevel)
    {
        for (var i = 0; i < deferredSizesPerLevel.Count; i++)
        {
            if (deferredSizesPerLevel[i] > 0)
                return true;
        }

        return false;
    }

    public static bool ShouldWriteMapDeferredEntry(int deferredSize) => deferredSize > 0;

    public static bool IsUnknownDeferredMapKey(bool levelInfoFound, string? mapKey) =>
        !levelInfoFound && !string.IsNullOrEmpty(mapKey);

    public static SteppedPropagateBatch StepSectorPropagateMark(int startIndex, int totalSectors)
    {
        var remaining = totalSectors - startIndex;
        if (remaining <= 0)
            return new SteppedPropagateBatch { ItemsProcessed = 0, NextIndex = startIndex, MoreRemaining = false };

        var batch = remaining < SectorStepSize ? remaining : SectorStepSize;
        var next = startIndex + batch;
        return new SteppedPropagateBatch
        {
            ItemsProcessed = batch,
            NextIndex = next,
            MoreRemaining = next < totalSectors,
        };
    }

    public static bool ShouldBeginPolyobjectPropagatePass(bool sectorsFinished, int polyobjectCount) =>
        sectorsFinished && polyobjectCount > 0;

    public static SteppedPropagateBatch StepPolyobjectPropagateMark(int startIndex, int totalPolyobjects)
    {
        var remaining = totalPolyobjects - startIndex;
        if (remaining <= 0)
            return new SteppedPropagateBatch { ItemsProcessed = 0, NextIndex = startIndex, MoreRemaining = false };

        var batch = remaining < PolyStepSize ? remaining : PolyStepSize;
        var next = startIndex + batch;
        return new SteppedPropagateBatch
        {
            ItemsProcessed = batch,
            NextIndex = next,
            MoreRemaining = next < totalPolyobjects,
        };
    }

    public static bool ShouldBeginSideDefPropagatePass(
        bool sectorsFinished,
        bool polyobjectsFinished,
        int sideCount) =>
        sectorsFinished && polyobjectsFinished && sideCount > 0;

    public static SteppedPropagateBatch StepSideDefPropagateMark(int startIndex, int totalSides)
    {
        var remaining = totalSides - startIndex;
        if (remaining <= 0)
            return new SteppedPropagateBatch { ItemsProcessed = 0, NextIndex = startIndex, MoreRemaining = false };

        var batch = remaining < SideDefStepSize ? remaining : SideDefStepSize;
        var next = startIndex + batch;
        return new SteppedPropagateBatch
        {
            ItemsProcessed = batch,
            NextIndex = next,
            MoreRemaining = next < totalSides,
        };
    }

    public static int CountSectorGcMarkTargets() =>
        5 + SectorInterpolationSlots;

    public static int CountSideTextureGcMarkTargets() => SideTextureInterpolationSlots;
}
