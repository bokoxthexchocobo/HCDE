namespace HCDE.Client;

/// <summary>
/// <c>SBarInfoMainBlock</c> draw/parse tail, script tables, and <c>SBarInfo::Load</c> head from <c>sbarinfo.cpp</c> (**301–450**).
/// Chunk **1064**; follows **1063** (**151–300**). Flag/alpha list (**312–337**) delegates to <see cref="VmSbarinfoParser1063Slice"/> — not re-credited.
/// </summary>
public static class VmSbarinfoMainBlockLoad1064Slice
{
    public const int ScriptSlotCount = 2;
    public const int CustomScriptSlot = 0;
    public const int DefaultScriptSlot = 1;

    public enum DefaultScriptAction
    {
        CreateNew,
        ParseExisting
    }

    public enum TopLevelKeywordId
    {
        Base = 0,
        Height,
        InterpolateHealth,
        InterpolateArmor,
        CompleteBorder,
        MonospaceFonts,
        LowerHealthCap,
        Resolution,
        StatusBar,
        Mugshot,
        CreatePopup,
        Protrusion,
        AppendStatusBar
    }

    public enum StatusBarTypeId
    {
        None = 0,
        Fullscreen,
        Normal,
        Automap,
        Inventory,
        InventoryFullscreen,
        PopupLog,
        PopupKeys,
        PopupStatus
    }

    public static readonly string[] TopLevelKeywords =
    [
        "base", "height", "interpolatehealth", "interpolatearmor", "completeborder",
        "monospacefonts", "lowerhealthcap", "resolution", "statusbar", "mugshot",
        "createpopup", "protrusion", "appendstatusbar"
    ];

    public static readonly string[] StatusBarTypeNames =
    [
        "none", "fullscreen", "normal", "automap", "inventory", "inventoryfullscreen",
        "popuplog", "popupkeys", "popupstatus"
    ];

    public static (int XOffset, int YOffset) StoreDrawOffsets(int xOffset, int yOffset) =>
        (xOffset, yOffset);

    public static double ApplyDrawAlpha(double blockAlpha, double incomingAlpha) =>
        VmSbarinfoParser1063Slice.CombineDrawAlpha(blockAlpha, incomingAlpha);

    public static bool ShouldParseFlowAfterFlagList(bool hasPipeSeparator, bool hasCommaSeparator) =>
        !hasPipeSeparator && !hasCommaSeparator;

    public static bool ShouldContinueMainBlockFlagTokens(bool hasPipeSeparator, bool hasCommaSeparator) =>
        hasPipeSeparator || hasCommaSeparator;

    /// <summary>After the optional comma-flag list, native assigns <c>alpha = sc.Float</c> when the float token was read (**336–337**).</summary>
    public static double ResolveMainBlockAlphaAfterFlagList(double currentAlpha, double floatConst, bool readFloatConst) =>
        readFloatConst ? floatConst : currentAlpha;

    public static bool ShouldFreeScriptSlot(object? slot) => slot != null;

    public static bool ShouldLoadDefaultFromGameInfo(string statusBarName) =>
        !string.IsNullOrEmpty(statusBarName);

    public static bool IsMissingLump(int lumpNum) => lumpNum < 0;

    public static DefaultScriptAction ResolveScriptSlotAction(bool slotIsNull) =>
        slotIsNull ? DefaultScriptAction.CreateNew : DefaultScriptAction.ParseExisting;

    public static bool ShouldIterateCustomSbarinfoLumps(int firstLumpCheckResult) =>
        firstLumpCheckResult != -1;

    public static bool ShouldEmitDefaultLoadBanner(bool batchRun, bool statusBarNamePresent, bool lumpValid) =>
        !batchRun && statusBarNamePresent && lumpValid;

    public static bool ShouldEmitCustomLoadBanner(bool batchRun, bool customLumpExists) =>
        !batchRun && customLumpExists;

    public static int MatchTopLevelKeyword(string token)
    {
        for (var i = 0; i < TopLevelKeywords.Length; i++)
        {
            if (TopLevelKeywords[i].Equals(token, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static int MatchStatusBarTypeName(string token)
    {
        for (var i = 0; i < StatusBarTypeNames.Length; i++)
        {
            if (StatusBarTypeNames[i].Equals(token, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static bool TopLevelKeywordIsAppendStatusBar(int keywordIndex) =>
        keywordIndex == TopLevelKeywords.Length - 1;
}
