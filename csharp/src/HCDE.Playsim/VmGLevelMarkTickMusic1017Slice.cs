namespace HCDE.Playsim;

/// <summary>
/// <c>DSectorMarker::PropagateMark</c> tail, <c>FLevelLocals::Tick</c>, <c>Mark</c>, scrollers, and intermission music from <c>g_level.cpp</c> (**2409–2558**).
/// </summary>
public static class VmGLevelMarkTickMusic1017Slice
{
    public const int SideTextureMarkCountPerSide = VmGLevelDeferredGc1016Slice.SideTextureInterpolationSlots;

    public enum InterMusicSelection
    {
        MapInterMusic,
        LevelInterMusic,
        GameInterMusic,
    }

    public readonly struct InterMusicChoice
    {
        public InterMusicSelection Source { get; init; }
        public string Music { get; init; }
        public int Order { get; init; }
    }

    public readonly struct GrayListReinsertPlan
    {
        public bool ReinsertOnGrayList { get; init; }
        public bool ChainAsGrayHead { get; init; }
    }

    public static long AccumulateSidePropagateMarkedBytes(int sidesProcessed, int sideStructureSize) =>
        (long)sidesProcessed * sideStructureSize;

    public static int SideTextureMarksInBatch(int sidesProcessed) =>
        sidesProcessed * SideTextureMarkCountPerSide;

    public static GrayListReinsertPlan PlanSectorMarkerGrayReinsert(bool moreTodo, long markedBytesThisPass) =>
        new()
        {
            ReinsertOnGrayList = moreTodo,
            ChainAsGrayHead = moreTodo && markedBytesThisPass >= 0,
        };

    public static bool ShouldClearScrollCarryBuffer(int scrollEntryCount) => scrollEntryCount > 0;

    public static bool ShouldCreateSectorMarker(
        bool markerIsNull,
        int sectorCount,
        int polyobjectCount,
        int sideCount) =>
        markerIsNull && (sectorCount > 0 || polyobjectCount > 0 || sideCount > 0);

    public static bool ShouldNullSectorMarkerWhenEmpty(
        int sectorCount,
        int polyobjectCount,
        int sideCount) =>
        sectorCount == 0 && polyobjectCount == 0 && sideCount == 0;

    public static bool ShouldResetSectorMarkerProgress(
        bool markerExists,
        int sectorCount,
        int polyobjectCount,
        int sideCount) =>
        markerExists && !ShouldNullSectorMarkerWhenEmpty(sectorCount, polyobjectCount, sideCount);

    public static int CountLocalEventHandlerMarkTargets(bool hasLocalEventManager) =>
        hasLocalEventManager ? 2 : 0;

    public static bool IsInvalidScrollerSector(int sectorNum) => sectorNum < 0;

    public static bool ShouldResizeScrollCarryArray(int currentScrollCount) => currentScrollCount == 0;

    public static int ResolveScrollCarryCapacity(int sectorCount, int currentScrollCount) =>
        ShouldResizeScrollCarryArray(currentScrollCount) ? sectorCount : currentScrollCount;

    public static InterMusicChoice ResolveInterMusic(
        bool hasMapInterMusicEntry,
        string? mapInterMusic,
        int mapInterOrder,
        bool levelInterMusicNonEmpty,
        string levelInterMusic,
        int levelInterOrder,
        string gameInterMusic,
        int gameInterOrder)
    {
        if (hasMapInterMusicEntry && !string.IsNullOrEmpty(mapInterMusic))
        {
            return new InterMusicChoice
            {
                Source = InterMusicSelection.MapInterMusic,
                Music = mapInterMusic,
                Order = mapInterOrder,
            };
        }

        if (levelInterMusicNonEmpty)
        {
            return new InterMusicChoice
            {
                Source = InterMusicSelection.LevelInterMusic,
                Music = levelInterMusic,
                Order = levelInterOrder,
            };
        }

        return new InterMusicChoice
        {
            Source = InterMusicSelection.GameInterMusic,
            Music = gameInterMusic,
            Order = gameInterOrder,
        };
    }

    public static bool HasSetInterMusicActionTarget(int paramCountAfterSelf) => paramCountAfterSelf >= 1;

    public static (float LevelVolume, bool InvokeHostVolume) AssignMusicVolume(float requestedVolume) =>
        (requestedVolume, true);
}
