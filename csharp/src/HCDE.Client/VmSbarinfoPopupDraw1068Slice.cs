namespace HCDE.Client;

/// <summary>
/// <c>Popup</c> tick tail, accessors/open/close, <c>adjustRelCenter</c>, and <c>DSBarInfo</c> setup head from <c>sbarinfo.cpp</c> (**901–1050**).
/// Chunk **1068**; follows **1067** popup tick alpha step (**873–900**). Fade alpha advance delegates to <see cref="VmSbarinfoMugShotPopup1067Slice.AdvanceFadePopupAlpha"/>.
/// </summary>
public static class VmSbarinfoPopupDraw1068Slice
{
    public const int InventoryBarLumpCount = 9;
    public const int NormalHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.Normal;
    public const int AutomapHudBarIndex = (int)VmSbarinfoMainBlockLoad1064Slice.StatusBarTypeId.Automap;

    public readonly record struct DsBarInfoPatchLayout(
        int ScriptImageCount,
        int InvBarOffset,
        int TotalPatchCount);

    public static readonly string[] InventoryBarLumpNames =
    [
        "ARTIBOX", "SELECTBO", "INVCURS", "INVGEML1", "INVGEMR1",
        "USEARTIA", "USEARTIB", "USEARTIC", "USEARTID"
    ];

    public static DsBarInfoPatchLayout BuildPatchNameTableLayout(int scriptImageCount) =>
        new(scriptImageCount, scriptImageCount, scriptImageCount + InventoryBarLumpCount);

    public static bool ShouldApplyFadeAlphaStep(
        bool movingThisFrame,
        VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind transition) =>
        movingThisFrame && transition == VmSbarinfoParseTopLevel1066Slice.PopupTransitionKind.Fade;

    public static bool ResolveSlidePopupMovingAfterStep(int y, int height, bool slideStepApplied) =>
        slideStepApplied
        && VmSbarinfoMugShotPopup1067Slice.ShouldKeepPopupMovingOnSlideAxis(y, height);

    public static bool ResolveFadePopupMovingAfterAlphaStep(double alpha, bool alphaStepApplied)
    {
        if (!alphaStepApplied)
            return false;
        return alpha != 0.0 && alpha != 1.0;
    }

    public static (int X, int Y, bool Moving) ResolveDefaultTransitionPopupPose(bool opened, int width, int height) =>
        opened ? (0, 0, false) : (width, height, false);

    public static double ScalePopupAlphaForDraw(double alpha, double maxAlpha) => alpha * maxAlpha;

    public static (bool Opened, bool Moving) ApplyPopupOpenCommand() => (true, true);

    public static (bool Opened, bool Moving) ApplyPopupCloseCommand() => (false, true);

    public static double AdjustRelativeCenterX(bool relativeX, double x, double viewWidth, double scaleX) =>
        relativeX ? x + viewWidth / (scaleX * 2.0) : x;

    public static double AdjustRelativeCenterY(bool relativeY, double y, double viewHeight, double scaleY) =>
        relativeY ? y + viewHeight / (scaleY * 2.0) : y;

    public static int ComputeTotalPatchNameCount(int scriptImageCount) =>
        scriptImageCount + InventoryBarLumpCount;

    public static (object? PrimaryAmmo, object? SecondaryAmmo) NormalizeReadyWeaponAmmo(
        object? ammo1,
        object? ammo2)
    {
        if (ammo1 == null && ammo2 != null)
            return (ammo2, null);
        return (ammo1, ammo2);
    }

    public static int ResolveInventoryAmount(bool actorPresent, int amount) =>
        actorPresent ? amount : 0;

    public static int SelectHudIndexForStatusBarDraw(bool automapBarEnabled, bool automapActive) =>
        automapBarEnabled && automapActive ? AutomapHudBarIndex : NormalHudBarIndex;

    public static double TickFadePopupAlpha(
        bool opened,
        double alpha,
        double speed,
        double speed2,
        bool movingThisFrame) =>
        movingThisFrame
            ? VmSbarinfoMugShotPopup1067Slice.AdvanceFadePopupAlpha(opened, alpha, speed, speed2)
            : alpha;
}
