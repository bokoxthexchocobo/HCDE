namespace HCDE.MapLoader;

/// <summary><c>LoadLevel</c> post-spawn finalize from <c>maploader.cpp</c> (3225–3335).</summary>
public static class VmMapLoaderLoadLevelFinalize
{
    public static int ResolveHeadGameNodeIndex(int gameNodeCount, int renderNodeCount) =>
        gameNodeCount > 0 ? gameNodeCount - 1 : renderNodeCount > 0 ? renderNodeCount - 1 : -1;

    public static void ClearBodyQueue<T>(Span<T?> bodyque) where T : class
    {
        for (var i = 0; i < bodyque.Length; i++)
            bodyque[i] = null;
    }

    public static bool ShouldLoadLightmapAfterSpawn(bool forceNodeBuild) => !forceNodeBuild;

    public static bool ShouldTranslateTeleportThings(bool hasBehavior, bool isTextMap) =>
        !hasBehavior && !isTextMap;

    public static void ClearSlopeReflection(byte[] reflectFloorCeiling, bool floorIsSlope, bool ceilingIsSlope)
    {
        if (floorIsSlope)
            reflectFloorCeiling[0] = 0;
        if (ceilingIsSlope)
            reflectFloorCeiling[1] = 0;
    }

    public static double ComputeNodeLength(double dx, double dy) => Math.Sqrt(dx * dx + dy * dy);

    public static bool ShouldCreateRenderResources(bool screenPresent, bool vertexDataPresent) =>
        screenPresent && vertexDataPresent;

    public static bool ShouldReloopSidedefs(bool reloop) => reloop;

    public static bool ShouldFinalizePortals(bool isReentering) => !isReentering;

    public static void ExpandSubsectorBounds(
        int subsectorIndex,
        int segCount,
        Func<int, (double x, double y)> segVertex,
        Action<double, double> addToBox)
    {
        for (var s = 0; s < segCount; s++)
        {
            var (x1, y1) = segVertex(s * 2);
            var (x2, y2) = segVertex(s * 2 + 1);
            addToBox(x1, y1);
            addToBox(x2, y2);
        }
    }

    public static bool SegHasVertices(bool v1Present, bool v2Present) => v1Present && v2Present;

    public static void SyncPlayerHealthFromMobj(int playerIndex, int mobjHealth, Action<int, int> setHealth)
    {
        if (playerIndex < 0)
            return;
        setHealth(playerIndex, mobjHealth);
    }

    public static bool ShouldRunSpawnSpecialsAfterThings(bool hasBehavior) => true;

    public static bool ShouldInitPortalGroupsAfterPoly(bool reloop) => true;

    public static bool ShouldBuildAabbWhenResourcesReady(bool renderResourcesAvailable) => renderResourcesAvailable;

    public static int CountSubsectorSegs(int numlines) => numlines;

    public static void IterateSubsectorSegs(int segCount, Action<int> perSeg)
    {
        for (var i = 0; i < segCount; i++)
            perSeg(i);
    }

    public static void ApplyExColorFlagCheck(bool floorSlope, bool ceilingSlope, Action checkExColor)
    {
        if (floorSlope || ceilingSlope)
            checkExColor();
    }

    public static bool ShouldRecalculate3dFloors(int sectorCount) => sectorCount > 0;

    public static bool ShouldSetSoftwareColormap(bool swRendererPresent) => swRendererPresent;
}
