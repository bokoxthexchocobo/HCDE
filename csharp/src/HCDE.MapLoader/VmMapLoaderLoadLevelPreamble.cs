namespace HCDE.MapLoader;

/// <summary>Opening of <c>MapLoader::LoadLevel</c> from <c>maploader.cpp</c> (2944–3018).</summary>
public static class VmMapLoaderLoadLevelPreamble
{
    public const int MapTypeHexen = 1;
    public const int MapTypeDoom = 2;
    public const int MapTypeUdmf = 3;

    public static bool ShouldLoadBehaviorPath(bool hasBehavior) => hasBehavior;

    public static int ResolveMapTypeAfterBehavior(bool hasBehavior) =>
        hasBehavior ? MapTypeHexen : MapTypeDoom;

    public static int ResolveMapTypeForTextMap(int current, bool isTextMap) =>
        isTextMap ? MapTypeUdmf : current;

    public static string? ResolveTranslator(
        string mapTranslator,
        string? xlatArg,
        string mapName,
        string gameDefaultTranslator,
        Func<string, bool> isEternityMap)
    {
        if (mapTranslator.Length > 0)
            return mapTranslator;
        if (xlatArg != null)
            return xlatArg;
        if (isEternityMap(mapName))
            return "xlat/eternity.txt";
        return gameDefaultTranslator;
    }

    public static bool ShouldForceNodeBuildFromCompat(uint compatFlags, uint rebuildFlag) =>
        (compatFlags & rebuildFlag) != 0;

    public static bool ShouldClearLaxMonsterActivation(uint levelFlags2, uint laxMapInfoFlag, uint laxMonsterFlag) =>
        (levelFlags2 & laxMapInfoFlag) == 0;

    public static uint ApplyLaxMonsterActivationPolicy(uint levelFlags2, uint laxMapInfoFlag, uint laxMonsterFlag) =>
        ShouldClearLaxMonsterActivation(levelFlags2, laxMapInfoFlag, laxMonsterFlag)
            ? levelFlags2 & ~laxMonsterFlag
            : levelFlags2;

    public static uint ApplyDummySwitchesFlag(uint levelFlags2, uint dummySwitchesFlag, bool hasBehavior, bool isTextMap) =>
        !hasBehavior && !isTextMap ? levelFlags2 | dummySwitchesFlag : levelFlags2;

    public static bool ShouldLoadDoomFormatGeometry(bool isTextMap) => !isTextMap;

    public static bool ShouldUseLineDefs1(bool hasBehavior) => !hasBehavior;

    public static bool ShouldUseThings1(bool hasBehavior) => !hasBehavior;

    public static bool ShouldLoadDefaultBehaviorModules(bool hasBehavior) => true;

    public static bool ShouldLoadMapinfoAcsLump() => true;

    public static bool ShouldLoadStrifeConversations(string lumpName) => lumpName.Length > 0;

    public static bool ShouldLoadVertexesForBinaryMap(bool isTextMap) => !isTextMap;
}
