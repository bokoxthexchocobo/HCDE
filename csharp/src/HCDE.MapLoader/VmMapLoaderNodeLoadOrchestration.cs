namespace HCDE.MapLoader;

/// <summary>Node-load orchestration from <c>maploader.cpp</c> (3056–3215).</summary>
public static class VmMapLoaderNodeLoadOrchestration
{
    public static uint MakeId(char a, char b, char c, char d) =>
        (uint)(a | (b << 8) | (c << 16) | (d << 24));

    public static bool MatchesAnyNodeSignature(uint id, uint[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (id == candidate)
                return true;
        }

        return false;
    }

    public static bool ShouldAttemptClassicNodeLoad(bool forceNodeBuild) => !forceNodeBuild;

    public static bool ShouldReportMissingBspData(int segsSize, int subsectorsSize, int nodesSize) =>
        segsSize != 0 || subsectorsSize != 0 || nodesSize != 0;

    public static bool ShouldUseV4NodePath(bool checkV4Nodes) => checkV4Nodes;

    public static bool CombineNodeLoadResults(bool subsectors, bool nodes, bool segs) =>
        subsectors && nodes && segs;

    public static bool ShouldTryGlNodeFallback(bool nodesLoaded, bool forceNodeBuild) =>
        !nodesLoaded && !forceNodeBuild;

    public static bool ShouldFixSameSectorSegSides(bool sameSector, bool hasLinedef) =>
        sameSector && hasLinedef;

    public static bool PickBackSideByDistance(double distToV1FromSegV1, double distToV1FromSegV2) =>
        distToV1FromSegV2 < distToV1FromSegV1;

    public static int ResolveHeadGameNodeIndex(int gameNodeCount, int renderNodeCount) =>
        gameNodeCount > 0 ? gameNodeCount - 1 : renderNodeCount > 0 ? renderNodeCount - 1 : -1;

    public static bool ShouldAssignSideNumsAfterBuild(bool forceNodeBuild) => forceNodeBuild;

    public static IEnumerable<int> EnumerateSideIndices(int sideCount)
    {
        for (var i = 0; i < sideCount; i++)
            yield return i;
    }
}
