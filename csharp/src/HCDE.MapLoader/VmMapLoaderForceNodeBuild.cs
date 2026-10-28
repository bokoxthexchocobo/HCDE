namespace HCDE.MapLoader;

/// <summary>Force node build / NanoBSP fallback policy from <c>LoadLevel</c> in <c>maploader.cpp</c> (3128–3183).</summary>
public static class VmMapLoaderForceNodeBuild
{
    public static bool ShouldEnterForceNodeBuild(bool forceNodeBuild) => forceNodeBuild;

    public static bool ShouldUseNanoBsp(bool nanoBspEnabled) => nanoBspEnabled;

    public static bool ShouldFallbackToZdbsp(bool nanoBspSucceeded) => !nanoBspSucceeded;

    public static double ElapsedSecondsFromMs(long endMs, long startMs) => (endMs - startMs) * 0.001;

    public static string FormatBspGenerationMessage(double seconds, int segCount) =>
        $"BSP generation took {seconds:F3} sec ({segCount} segs)\n";

    public static bool ShouldAdjustLinesAfterCompat(bool forceNodeBuild) => forceNodeBuild;

    public static bool ShouldMarkBuildGlNodes(bool forceNodeBuild) => forceNodeBuild;
}
