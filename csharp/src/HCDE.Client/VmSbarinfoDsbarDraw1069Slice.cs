namespace HCDE.Client;

/// <summary>
/// <c>DSBarInfo::_Draw</c> tail, tick/popup helpers, protrusion lookup, and <c>DrawGraphic</c> prologue from <c>sbarinfo.cpp</c> (**1051–1200**).
/// Chunk **1069**; follows **1068** status-bar automap HUD pick (**1048–1050**). That pick delegates via <see cref="VmSbarinfoPopupDraw1068Slice.SelectHudIndexForStatusBarDraw"/>.
/// </summary>
public static class VmSbarinfoDsbarDraw1069Slice
{
    public const int PopNone = 0;
    public const int PopLog = 1;
    public const int PopKeys = 2;
    public const int PopStatus = 3;
    public const int LastHudResetSentinel = -1;

    public const int FullscreenHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.Fullscreen;
    public const int NoneHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.None;
    public const int InventoryHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.Inventory;
    public const int InventoryFullscreenHudBarIndex =
        (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.InventoryFullscreen;
    public const int PopupLogHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.PopupLog;
    public const int PopupKeysHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.PopupKeys;
    public const int PopupStatusHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.PopupStatus;

    public enum PrimaryDrawHudState
    {
        StatusBar,
        Fullscreen,
        Other
    }

    public static int ResolveHudBarIndexForDrawState(
        PrimaryDrawHudState state,
        bool automapBarEnabled,
        bool automapActive)
    {
        return state switch
        {
            PrimaryDrawHudState.StatusBar => VmSbarinfoPopupDraw1068Slice.SelectHudIndexForStatusBarDraw(
                automapBarEnabled, automapActive),
            PrimaryDrawHudState.Fullscreen => FullscreenHudBarIndex,
            _ => NoneHudBarIndex
        };
    }

    public static bool ShouldDrawMainHudWithPopupDisplacement(int currentPopup, bool fullScreenOffsets) =>
        currentPopup != PopNone && !fullScreenOffsets;

    public static (int X, int Y) ResolveMainHudDrawOffsets(
        bool usePopupDisplacement,
        int displacementX,
        int displacementY) =>
        usePopupDisplacement ? (displacementX, displacementY) : (0, 0);

    public static bool ShouldDrawInventoryBar(
        int inventoryTics,
        bool levelAllowsInventoryBar,
        PrimaryDrawHudState state) =>
        inventoryTics > 0
        && levelAllowsInventoryBar
        && (state == PrimaryDrawHudState.StatusBar || state == PrimaryDrawHudState.Fullscreen);

    public static int SelectInventoryHudBarIndex(PrimaryDrawHudState state) =>
        state == PrimaryDrawHudState.StatusBar ? InventoryHudBarIndex : InventoryFullscreenHudBarIndex;

    public static bool ShouldClearInventoryTics(bool inventoryBarHasCommands) => !inventoryBarHasCommands;

    public static int MapActivePopupToHudBarIndex(int currentPopup) => currentPopup switch
    {
        PopLog => PopupLogHudBarIndex,
        PopKeys => PopupKeysHudBarIndex,
        PopStatus => PopupStatusHudBarIndex,
        _ => NoneHudBarIndex
    };

    public static bool ShouldAdvancePendingPopupAfterClose(bool popupOpened, bool doneMoving) =>
        !popupOpened && doneMoving;

    public static (int PendingPopup, bool CloseCurrent) ResolveShowPopupIntent(int requestedPop, int currentPopup)
    {
        var pending = requestedPop != currentPopup ? requestedPop : PopNone;
        if (currentPopup != PopNone)
            return (pending, CloseCurrent: true);
        return (pending, CloseCurrent: false);
    }

    public static (int ActivePopup, int PendingPopup) CommitImmediatePopupWhenIdle(
        int pendingPopup,
        int currentPopup)
    {
        if (currentPopup != PopNone)
            return (currentPopup, pendingPopup);
        return (pendingPopup, PopNone);
    }

    public static int ResolveProtrusionOffset(ReadOnlySpan<(double Factor, int Offset)> protrusions, double scaleFactor)
    {
        var returnVal = 0;
        foreach (var prot in protrusions)
        {
            if (prot.Factor > scaleFactor)
                break;
            returnVal = prot.Offset;
        }

        return returnVal;
    }

    public static bool ShouldSkipDrawGraphic(object? texture) => texture == null;

    public static bool ShouldUseNativeLogPopup(bool popupLogBarHasCommands) => !popupLogBarHasCommands;
}
