namespace HCDE.MapLoader;

/// <summary>Classic node load and GL fallback from <c>LoadLevel</c> in <c>maploader.cpp</c> (3091–3114).</summary>
public static class VmMapLoaderClassicNodeFallback
{
    public static bool ShouldAttemptClassicNodeLoad(int segsSize, int subsectorsSize, int nodesSize) =>
        segsSize != 0 || subsectorsSize != 0 || nodesSize != 0;

    public static bool CombineClassicNodeLoadResults(bool subsectorsOk, bool nodesOk, bool segsOk) =>
        subsectorsOk && nodesOk && segsOk;

    public static bool ShouldTryGlNodesAfterFailure(bool nodesLoaded, bool forceNodeBuild) =>
        !nodesLoaded && !forceNodeBuild;

    public static bool ShouldForceNodeBuildWhenGlFails(bool glLoaded) => !glLoaded;

    public static void AssignSideNumbers(Span<int> sideNums)
    {
        for (var i = 0; i < sideNums.Length; i++)
            sideNums[i] = i;
    }

    public static bool ShouldLoadV4NodePath(bool isV4Header) => isV4Header;

    public static bool ShouldLoadClassicTemplates(bool isV4) => !isV4;

    public static bool ShouldSetReloopAfterGl(bool glLoaded) => glLoaded;

    public static bool ShouldClearForceWhenGlSucceeds(bool glLoaded) => glLoaded;
}
