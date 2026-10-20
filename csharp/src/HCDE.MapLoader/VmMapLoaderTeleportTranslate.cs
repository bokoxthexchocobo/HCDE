namespace HCDE.MapLoader;

/// <summary><c>MapLoader::TranslateTeleportThings</c> from <c>maploader.cpp</c> (92–134).</summary>
public static class VmMapLoaderTeleportTranslate
{
    public const int TeleportSpecial = 39;
    public const int TeleportNoFogSpecial = 154;
    public const int TeleportZombieChangerSpecial = 125;

    public static bool ShouldAssignTid(bool sectorHasTags) => !sectorHasTags;

    public static void ApplyTeleportLineArgs(int special, int[] args, int defaultTid = 1)
    {
        if (special == TeleportSpecial && args[1] == 0)
            args[0] = defaultTid;
        else if (special == TeleportNoFogSpecial && args[2] == 0)
            args[0] = defaultTid;
        else if (special == TeleportZombieChangerSpecial && args[1] == 0)
            args[0] = defaultTid;
    }
}
