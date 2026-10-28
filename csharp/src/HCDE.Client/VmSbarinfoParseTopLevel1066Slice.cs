namespace HCDE.Client;

/// <summary>
/// <c>ParseSBarInfo</c> top-level commands from <c>sbarinfo.cpp</c> (**601–750**): health cap through protrusion loop head.
/// Chunk **1066**; follows **1065** (**451–600**). Status-bar name matching delegates to <see cref="VmSbarinfoMainBlockLoad1064Slice"/>.
/// </summary>
public static class VmSbarinfoParseTopLevel1066Slice
{
    public const int TicRate = 35;
    public const int AutomapBarTypeIndex = 3;
    public const double ProtrusionInitialLastFactor = -2.220446049250313e-16;

    public enum PopupKind
    {
        Log = 0,
        Keys = 1,
        Status = 2
    }

    public enum PopupTransitionKind
    {
        None,
        SlideInBottom,
        PushUp,
        Fade
    }

    public readonly record struct MugShotFlagState(
        bool UsesLevels,
        bool Health2,
        bool HealthSpecial,
        bool Directional);

    public static bool ResolveLowerHealthCap(bool checkFalseHit, bool validatedTrueToken)
    {
        if (checkFalseHit)
            return false;
        if (validatedTrueToken)
            return true;
        throw new ArgumentException("lowerhealthcap requires false or validated true token.");
    }

    public static (int ResW, int ResH) MergeResolution(
        int priorW,
        int priorH,
        int parsedW,
        int parsedH,
        bool resolutionCommandActive) =>
        resolutionCommandActive ? (parsedW, parsedH) : (priorW, priorH);

    public static int? ResolveImplicitGameTypeWhenBaseUnset(bool baseWasSet) =>
        baseWasSet ? null : VmSbarinfoParseTopLevel1065Slice.GameTypeAny;

    public static int ResolveStatusBarIndex(bool hasExplicitBarName, int matchedBarIndex) =>
        hasExplicitBarName ? matchedBarIndex : 0;

    public static bool IsAppendStatusBarCommand(int topLevelCommandIndex) =>
        topLevelCommandIndex == (int)VmSbarinfoMainBlockLoad1064Slice.TopLevelKeywordId.AppendStatusBar;

    public enum StatusBarHudAction
    {
        CreateNewBlock,
        ParseExistingBlock,
        RejectAppendMissing
    }

    public static StatusBarHudAction ResolveStatusBarHudAction(bool isAppendCommand, bool hudIsNull)
    {
        if (isAppendCommand && hudIsNull)
            return StatusBarHudAction.RejectAppendMissing;
        if (isAppendCommand)
            return StatusBarHudAction.ParseExistingBlock;
        return StatusBarHudAction.CreateNewBlock;
    }

    public static bool ShouldReplaceExistingHud(bool isAppendCommand, bool hudSlotOccupied) =>
        !isAppendCommand && hudSlotOccupied;

    public static bool ShouldRejectAppendToMissingHud(bool isAppendCommand, bool hudIsNull) =>
        ResolveStatusBarHudAction(isAppendCommand, hudIsNull) == StatusBarHudAction.RejectAppendMissing;

    public static bool ShouldMarkAutomapBar(int barIndex) => barIndex == AutomapBarTypeIndex;

    public static MugShotFlagState ApplyMugShotStateFlag(string token, MugShotFlagState current)
    {
        if (token.Equals("health", StringComparison.OrdinalIgnoreCase))
            return current with { UsesLevels = true };
        if (token.Equals("health2", StringComparison.OrdinalIgnoreCase))
            return current with { UsesLevels = true, Health2 = true };
        if (token.Equals("healthspecial", StringComparison.OrdinalIgnoreCase))
            return current with { UsesLevels = true, HealthSpecial = true };
        if (token.Equals("directional", StringComparison.OrdinalIgnoreCase))
            return current with { Directional = true };
        throw new ArgumentException($"Unknown MugShot state flag '{token}'.", nameof(token));
    }

    public static bool IsKnownMugShotStateFlag(string token) =>
        token.Equals("health", StringComparison.OrdinalIgnoreCase)
        || token.Equals("health2", StringComparison.OrdinalIgnoreCase)
        || token.Equals("healthspecial", StringComparison.OrdinalIgnoreCase)
        || token.Equals("directional", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldRegisterMugShotReplacement(int existingStateIndex, bool stateAlreadyStored) =>
        stateAlreadyStored && existingStateIndex != -1;

    public static PopupKind? ClassifyPopupKind(string name)
    {
        if (name.Equals("log", StringComparison.OrdinalIgnoreCase))
            return PopupKind.Log;
        if (name.Equals("keys", StringComparison.OrdinalIgnoreCase))
            return PopupKind.Keys;
        if (name.Equals("status", StringComparison.OrdinalIgnoreCase))
            return PopupKind.Status;
        return null;
    }

    public static PopupTransitionKind? ClassifyPopupTransition(string name)
    {
        if (name.Equals("slideinbottom", StringComparison.OrdinalIgnoreCase))
            return PopupTransitionKind.SlideInBottom;
        if (name.Equals("pushup", StringComparison.OrdinalIgnoreCase))
            return PopupTransitionKind.PushUp;
        if (name.Equals("fade", StringComparison.OrdinalIgnoreCase))
            return PopupTransitionKind.Fade;
        return null;
    }

    public static double ComputeFadePopupSpeed(double floatConst, int ticRate) =>
        1.0 / (ticRate * floatConst);

    public static (double Speed, double Speed2) BuildFadePopupSpeeds(double firstFloat, double secondFloat, int ticRate) =>
        (ComputeFadePopupSpeed(firstFloat, ticRate), ComputeFadePopupSpeed(secondFloat, ticRate));

    public static bool IsProtrusionFactorAscending(double candidate, double lastFactor) =>
        candidate > lastFactor;

    public static (double Factor, int PixelOffset) BuildProtrusionPair(double factor, int pixelOffset, bool factorAscending) =>
        factorAscending ? (factor, pixelOffset) : throw new ArgumentOutOfRangeException(nameof(factor));

    public static bool ShouldParseAnotherProtrusionPair(bool hasTrailingComma, bool lastFactorAscending) =>
        hasTrailingComma && lastFactorAscending;
}
