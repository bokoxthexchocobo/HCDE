namespace HCDE.Client;

/// <summary>Automap CUSTOM_CVAR callbacks and default overlay colors from <c>am_map.cpp</c> (126–318, 357–418).</summary>
public static class VmAutomapCustomCvarCallbacks
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

    public static uint DefaultOverlayWallColor => 0x00ff00;

    public static uint DefaultOverlayUnseenColor => 0x00226e;

    public static float DefaultOverlaySectorFillAlpha => 0.2f;

    public static int StandardCvarTableLength => 25;

    public static int OverlayCvarTableLength => 25;

    public static int ColorNameSlotCount => 26;

    public static bool IsValidColorNameIndex(int index, int slotCount) =>
        index >= 0 && index < slotCount;

    public static bool ShouldInitColorFromCvar(int index, int almostBackgroundIndex) =>
        index < almostBackgroundIndex;

    public static int AlmostBackgroundColorIndex => 25;

    public static (byte r, byte g, byte b) WhiteTriplet => (255, 255, 255);

    public static (byte r, byte g, byte b) BlackTriplet => (0, 0, 0);
}
