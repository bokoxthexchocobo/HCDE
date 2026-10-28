namespace HCDE.Client;

/// <summary>Automap CUSTOM_CVAR callbacks from <c>am_map.cpp</c>.</summary>
public static partial class VmAutomapCustomCvarCallbacks
{
    public static bool ShouldUpdateShowAllLines(bool hasAutomap) => hasAutomap;

    public static int ClampAutomapCheat(bool netGame, bool svCheats, int requested)
    {
        if (netGame && !svCheats && requested != 0)
            return 0;
        return requested;
    }

    public static bool ShouldDeactivateOverlay(bool automapActive, bool viewActive, int overlayMode) =>
        automapActive && viewActive && overlayMode == 0;

    public static int ClampEmptySpaceMargin(int value)
    {
        if (value < 0)
            return 0;
        return value > 90 ? 90 : value;
    }

    public static bool ShouldRefreshResolution(bool hasStatusBar, bool hasAutomap) =>
        hasStatusBar && hasAutomap;

    public static bool DefaultShowSecrets => true;

    public static bool DefaultShowMonsters => true;

    public static bool DefaultShowItems => false;

    public static float DefaultOverlaySectorFillAlpha => 0.2f;
}
