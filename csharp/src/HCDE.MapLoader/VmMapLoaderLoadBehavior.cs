namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadBehavior</c> from <c>maploader.cpp</c> (2858–2869).</summary>
public static class VmMapLoaderLoadBehavior
{
    public static bool ShouldLoadBehaviorModule(int behaviorLumpSize) => behaviorLumpSize > 0;

    public static bool ShouldUnloadAcsOnCheckFailure(bool allGood) => !allGood;

    public static bool ShouldPrintAcsUnloadMessage(bool allGood) => !allGood;
}
