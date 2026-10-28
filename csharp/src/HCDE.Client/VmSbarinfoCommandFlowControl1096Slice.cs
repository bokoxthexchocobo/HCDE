namespace HCDE.Client;

/// <summary>
/// <c>CommandIfWaterLevel</c> parse/tick from <c>sbarinfo_commands.cpp</c> (**3456–3468**); hull **3449–3450** is **1095**.
/// </summary>
public static class VmSbarinfoCommandIfWaterLevel1096Slice
{
    public static (bool Parsed, int Threshold) ResolveIfWaterLevelParseNegatableThreshold(int intTokenValue) =>
        (Parsed: true, Threshold: intTokenValue);

    public static bool ResolveIfWaterLevelFlowTruth(int playerWaterLevel, int threshold) =>
        playerWaterLevel >= threshold;
}

/// <summary>
/// <c>CommandIfCVarInt</c> from the same native band (**3476–3541**).
/// </summary>
public static class VmSbarinfoCommandIfCVarInt1096Slice
{
    public enum IfCVarIntValueType
    {
        Unsupported,
        Bool,
        Int
    }

    public static (bool EqualComparison, int CompareValue) CreateIfCVarIntCtorDefaults() =>
        (EqualComparison: false, CompareValue: 0);

    public static IfCVarIntValueType ClassifyIfCVarIntSupportedType(bool isBoolCVar, bool isIntCVar) =>
        isBoolCVar ? IfCVarIntValueType.Bool
        : isIntCVar ? IfCVarIntValueType.Int
        : IfCVarIntValueType.Unsupported;

    public static (bool EqualComparison, int CompareValue) ResolveIfCVarIntParseThresholdFields(
        int parsedInt,
        bool optionalSecondComma,
        bool equalIdentifier) =>
        optionalSecondComma && equalIdentifier
            ? (EqualComparison: true, CompareValue: parsedInt)
            : (EqualComparison: false, CompareValue: parsedInt);

    public static bool ResolveIfCVarIntFlowTruthFromLiveValue(
        bool cvarPresent,
        int liveValue,
        int compareValue,
        bool equalComparison)
    {
        if (!cvarPresent)
            return false;
        return equalComparison ? liveValue == compareValue : liveValue >= compareValue;
    }
}

/// <summary>
/// <c>SBarInfoCommandNames</c>, <c>SBarInfoCommands</c>, and <c>NextCommand</c> dispatch head (**3545–3600**).
/// </summary>
public static class VmSbarinfoCommandFlowControlNextCommand1096Slice
{
    public enum SBarInfoFlowCommandKind
    {
        DrawImage = 0,
        DrawNumber,
        DrawSwitchableImage,
        DrawMugShot,
        DrawSelectedInventory,
        DrawInventoryBar,
        DrawBar,
        DrawGem,
        DrawShader,
        DrawString,
        DrawKeyBar,
        GameMode,
        PlayerClass,
        PlayerType,
        AspectRatio,
        IsSelected,
        UsesAmmo,
        UsesSecondaryAmmo,
        HasWeaponPiece,
        InventoryBarNotVisible,
        WeaponAmmo,
        InInventory,
        Alpha,
        IfHealth,
        IfInvulnerable,
        IfWaterLevel,
        IfCVarInt,
        Unknown = -1
    }

    private static readonly string[] CommandNames =
    [
        "drawimage", "drawnumber", "drawswitchableimage",
        "drawmugshot", "drawselectedinventory",
        "drawinventorybar", "drawbar", "drawgem",
        "drawshader", "drawstring", "drawkeybar",
        "gamemode", "playerclass", "playertype", "aspectratio",
        "isselected", "usesammo", "usessecondaryammo",
        "hasweaponpiece", "inventorybarnotvisible",
        "weaponammo", "ininventory", "alpha", "ifhealth",
        "ifinvulnerable", "ifwaterlevel", "ifcvarint"
    ];

    public static (bool Recognized, SBarInfoFlowCommandKind Kind) TryResolveSBarInfoFlowCommandKind(string identifier)
    {
        for (var index = 0; index < CommandNames.Length; index++)
        {
            if (identifier.Equals(CommandNames[index], StringComparison.OrdinalIgnoreCase))
                return (true, (SBarInfoFlowCommandKind)index);
        }

        return (false, SBarInfoFlowCommandKind.Unknown);
    }

    public static bool IsNextCommandDispatchCaseWithinBand3600(SBarInfoFlowCommandKind kind) =>
        kind >= SBarInfoFlowCommandKind.DrawImage && kind <= SBarInfoFlowCommandKind.InInventory;

    public static (bool CloseFlowBlock, bool AllocateCommand) ResolveNextCommandWhenIdentifierMissing(bool identifierPresent) =>
        identifierPresent
            ? (CloseFlowBlock: false, AllocateCommand: true)
            : (CloseFlowBlock: true, AllocateCommand: false);
}
