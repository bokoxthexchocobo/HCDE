namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawSelectedInventory</c> and <c>CommandGameMode</c> ctor head from <c>sbarinfo_commands.cpp</c> (**1651–1800**).
/// Chunk **1084**; follows **1083** mugshot parse through <c>;</c> (**1650**).
/// </summary>
public static class VmSbarinfoCommandDrawSelectedInventory1084Slice
{
    public const int SelectedInventoryCounterLength = int.MaxValue;
    public const int ArtiflashImageSlotOffsetFromInvBar = 6;
    public const int ArtiflashTickBase = 4;
    public const int DefaultCounterXOffsetFromImage = 30;
    public const int DefaultCounterYOffsetFromImage = 24;

    [Flags]
    public enum DrawImageOffsetFlags
    {
        Top = 0x1,
        VMiddle = 0x2,
        Bottom = 0x4,
        Left = 0x10,
        Right = 0x20,
        HMiddle = 0x40,
        Center = VMiddle | HMiddle,
        CenterBottom = Bottom | HMiddle
    }

    public enum SelectedInventoryParseKeyword
    {
        AlternateOnEmpty,
        Artiflash,
        AlwaysShowCounter,
        Itemflash,
        Center,
        CenterBottom,
        DrawShadow,
        Unknown
    }

    public enum SelectedInventoryFlagListSeparator
    {
        PipeContinues,
        CommaRequired
    }

    public readonly record struct SelectedInventoryCtorDefaults(
        bool AlternateOnEmpty,
        bool Artiflash,
        bool AlwaysShowCounter,
        bool Itemflash,
        int CounterLength);

    public static SelectedInventoryCtorDefaults CreateSelectedInventoryCtorDefaults() =>
        new(false, false, false, false, SelectedInventoryCounterLength);

    public static bool ShouldDrawSelectedInventoryBar(bool hasSelectedItem, bool levelDisablesInventoryBar) =>
        hasSelectedItem && !levelDisablesInventoryBar;

    public static bool ShouldTakeArtiflashDrawBranch(bool artiflashEnabled, int artiflashTick) =>
        artiflashEnabled && artiflashTick != 0;

    public static int ComputeArtiflashImageIndex(int invBarOffset, int artiflashTick) =>
        invBarOffset + ArtiflashImageSlotOffsetFromInvBar + (ArtiflashTickBase - artiflashTick);

    public static bool ShouldApplyItemFlashCursor(bool itemflashEnabled, int itemflashFade) =>
        itemflashEnabled && itemflashFade != 0;

    public static double ComputeItemFlashDrawAlpha(double blockAlpha, int itemflashFade) =>
        blockAlpha * itemflashFade;

    public static bool ShouldDrawSelectedInventoryAmountCounter(bool alwaysShowCounter, int selectedAmount) =>
        alwaysShowCounter || selectedAmount != 1;

    public static (bool Recognized, SelectedInventoryParseKeyword Keyword) ClassifySelectedInventoryParseKeyword(
        string identifier)
    {
        if (identifier.Equals("alternateonempty", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.AlternateOnEmpty);
        if (identifier.Equals("artiflash", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.Artiflash);
        if (identifier.Equals("alwaysshowcounter", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.AlwaysShowCounter);
        if (identifier.Equals("itemflash", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.Itemflash);
        if (identifier.Equals("center", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.Center);
        if (identifier.Equals("centerbottom", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.CenterBottom);
        if (identifier.Equals("drawshadow", StringComparison.OrdinalIgnoreCase))
            return (true, SelectedInventoryParseKeyword.DrawShadow);
        return (false, SelectedInventoryParseKeyword.Unknown);
    }

    public static DrawImageOffsetFlags ResolveSelectedInventoryImageOffset(
        SelectedInventoryParseKeyword keyword) =>
        keyword switch
        {
            SelectedInventoryParseKeyword.Center => DrawImageOffsetFlags.Center,
            SelectedInventoryParseKeyword.CenterBottom => DrawImageOffsetFlags.CenterBottom,
            _ => DrawImageOffsetFlags.Top
        };

    public static bool ShouldTreatSelectedInventoryTokenAsFont(bool tokenIsIdentifier, bool recognizedKeyword) =>
        !tokenIsIdentifier || !recognizedKeyword;

    public static SelectedInventoryFlagListSeparator ResolveSelectedInventoryFlagListSeparator(bool sawPipeSeparator) =>
        sawPipeSeparator
            ? SelectedInventoryFlagListSeparator.PipeContinues
            : SelectedInventoryFlagListSeparator.CommaRequired;

    public static (int StartX, int StartY) ResolveSelectedInventoryDefaultCounterPosition(int imageX, int imageY) =>
        (imageX + DefaultCounterXOffsetFromImage, imageY + DefaultCounterYOffsetFromImage);

    public static (bool RunFlowControlParse, bool RequireStatementSemicolon) ResolveSelectedInventoryParseTail(
        bool alternateOnEmpty) =>
        alternateOnEmpty ? (true, false) : (false, true);

    public static bool ResolveSelectedInventoryFlowControlTruth(
        bool hasSelectedItem,
        bool levelDisablesInventoryBar) =>
        !hasSelectedItem || levelDisablesInventoryBar;
}

/// <summary>
/// <c>CommandGameMode</c> mode bitmask helpers at the native band tail (**1796–1800**).
/// </summary>
public static class VmSbarinfoCommandGameMode1084Slice
{
    public static int ApplyCommandGameModeFlagBit(int modes, int flagBit) => modes | flagBit;
}
