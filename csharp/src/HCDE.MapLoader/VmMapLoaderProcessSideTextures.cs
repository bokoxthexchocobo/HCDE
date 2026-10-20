namespace HCDE.MapLoader;

/// <summary><c>MapLoader::ProcessSideTextures</c> from <c>maploader.cpp</c> (2053–2158).</summary>
public static class VmMapLoaderProcessSideTextures
{
    public const int TransferHeights = 209;
    public const int StaticInit = 190;
    public const int SectorSet3dFloor = 160;
    public const int TranslucentLine = 208;

    public static bool ShouldApplyHeightTransfer(int special) => special == TransferHeights;

    public static bool ShouldApplyStaticInit(int special) => special == StaticInit;

    public static bool ShouldApply3dFloor(int special) => special == SectorSet3dFloor;

    public static bool ShouldApplyTranslucentLine(int special, bool checkTranMap) =>
        special == TranslucentLine && checkTranMap;

    public static bool UsesHashAlphaTopTexture(string topTexture) =>
        topTexture.Length > 0 && topTexture[0] == '#';

    public static bool IsTranMapLumpName(string midTexture) =>
        midTexture.Length >= 8 && midTexture.StartsWith("TRANMAP", StringComparison.OrdinalIgnoreCase);

    public static bool IsTranslucencyLumpSize(long fileLength) => fileLength == 65536;

    public static bool ShouldClearMidTextureForTranMap() => true;

    public static bool ShouldFallThroughToDefaultTextures(bool checkTranMap, int special) =>
        special == TranslucentLine && !checkTranMap;

    public static bool ShouldTagSectorForStaticInit(bool colorGood, bool fogGood) => colorGood || fogGood;

    public static bool ShouldApplySectorColor(bool colorGood) => colorGood;

    public static bool ShouldApplySectorFog(bool fogGood) => fogGood;

    public static int ResolveSpecialDispatch(int special, bool checkTranMap)
    {
        if (ShouldApplyHeightTransfer(special))
            return 1;
        if (ShouldApplyStaticInit(special))
            return 2;
        if (ShouldApply3dFloor(special))
            return 3;
        if (ShouldApplyTranslucentLine(special, checkTranMap))
            return 4;
        if (ShouldFallThroughToDefaultTextures(checkTranMap, special))
            return 0;
        return 5;
    }
}
