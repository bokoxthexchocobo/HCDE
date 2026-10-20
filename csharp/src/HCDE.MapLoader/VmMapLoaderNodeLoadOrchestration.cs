namespace HCDE.MapLoader;

/// <summary>Node-load orchestration from <c>maploader.cpp</c> (2999–3245).</summary>
public static class VmMapLoaderNodeLoadOrchestration
{
    public const uint Level2LaxActivationMapinfo = 1u << 0;
    public const uint Level2LaxMonsterActivation = 1u << 1;
    public const uint Level2DummySwitches = 1u << 2;

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

    public static bool ShouldClearLaxMonsterActivation(bool hasBehavior, bool isText, uint levelFlags2) =>
        (!hasBehavior || isText) && (levelFlags2 & Level2LaxActivationMapinfo) == 0;

    public static uint ApplyLaxMonsterActivationPolicy(uint levelFlags2, bool shouldClear) =>
        shouldClear ? levelFlags2 & ~Level2LaxMonsterActivation : levelFlags2;

    public static bool ShouldEnableDummySwitches(bool hasBehavior, bool isText) =>
        !hasBehavior && !isText;

    public static uint ApplyDummySwitchPolicy(uint levelFlags2, bool enable) =>
        enable ? levelFlags2 | Level2DummySwitches : levelFlags2;

    public static bool ShouldLoadBinaryMapGeometry(bool isText) => !isText;

    public static bool ShouldLoadDoomLineDefs(bool hasBehavior) => !hasBehavior;

    public static bool ShouldLoadHexenLineDefs(bool hasBehavior) => hasBehavior;

    public static bool ShouldAttemptClassicNodeLoad(bool forceNodeBuild) => !forceNodeBuild;

    public static bool ShouldReportMissingBspData(int segsSize, int subsectorsSize, int nodesSize) =>
        segsSize != 0 || subsectorsSize != 0 || nodesSize != 0;

    public static bool ShouldUseV4NodePath(bool checkV4Nodes) => checkV4Nodes;

    public static bool CombineNodeLoadResults(bool subsectors, bool nodes, bool segs) =>
        subsectors && nodes && segs;

    public static bool ShouldTryGlNodeFallback(bool nodesLoaded, bool forceNodeBuild) =>
        !nodesLoaded && !forceNodeBuild;

    public static bool ShouldForceNodeBuildAfterGlFail(bool glLoaded) => !glLoaded;

    public static bool ShouldReloopAfterGlNodes(bool glLoaded) => glLoaded;

    public static bool ShouldAssignSideNumsAfterBuild(bool forceNodeBuild) => forceNodeBuild;

    public static bool ShouldFixSameSectorSegSides(bool sameSector, bool hasLinedef) =>
        sameSector && hasLinedef;

    public static bool PickBackSideByDistance(double distToV1FromSegV1, double distToV1FromSegV2) =>
        distToV1FromSegV2 < distToV1FromSegV1;

    public static int ResolveSegSideIndex(bool backSide) => backSide ? 1 : 0;

    public static int ResolveHeadGameNodeIndex(int gameNodeCount, int renderNodeCount) =>
        gameNodeCount > 0 ? gameNodeCount - 1 : renderNodeCount > 0 ? renderNodeCount - 1 : -1;

    public static bool ShouldLoadLightmapsAfterSpawn(bool forceNodeBuild) => !forceNodeBuild;

    public static IEnumerable<int> EnumerateSideIndices(int sideCount)
    {
        for (var i = 0; i < sideCount; i++)
            yield return i;
    }

    public static IEnumerable<uint> EnumerateZnodedSignatures() =>
        new[] { MakeId('Z', 'N', 'O', 'D'), MakeId('X', 'N', 'O', 'D') };
}
