namespace HCDE.Client;

/// <summary><c>AM_initColors</c> policy from <c>am_map.cpp</c> (705–759).</summary>
public static class VmAutomapInitColors
{
    public const int ColorsetCustom = -1;
    public const int ColorsetDoom = 1;
    public const int ColorsetStrife = 2;
    public const int ColorsetRaven = 3;

    public const int GameDoomChex = 1 << 0;
    public const int GameStrife = 1 << 1;
    public const int GameRaven = 1 << 2;

    public static bool ShouldUseOverlayMod(bool overlayed, bool customColors, bool modOverlayDefined) =>
        overlayed && customColors && modOverlayDefined;

    public static bool ShouldUseStandardMod(bool overlayed, bool customColors, bool modDefined) =>
        !overlayed && customColors && modDefined;

    public static int ResolveAutoColorset(int requestedSet, int gameType)
    {
        if (requestedSet != ColorsetCustom)
            return requestedSet;
        if ((gameType & GameDoomChex) != 0)
            return ColorsetDoom;
        if ((gameType & GameStrife) != 0)
            return ColorsetStrife;
        if ((gameType & GameRaven) != 0)
            return ColorsetRaven;
        return ColorsetCustom;
    }

    public static bool ShouldInitFromCvars(int resolvedSet) => resolvedSet is ColorsetCustom or < 1 or > 3;

    public static bool UsesRavenPaletteLayout(int resolvedSet) => resolvedSet == ColorsetRaven;

    public static double ResolveSectorFillAlpha(bool overlayed, double standardAlpha, double overlayAlpha) =>
        overlayed ? overlayAlpha : standardAlpha;
}
