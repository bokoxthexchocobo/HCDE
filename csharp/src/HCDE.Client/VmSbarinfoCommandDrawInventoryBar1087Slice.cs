namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawInventoryBar::Draw</c>, <c>Parse</c>, and <c>GetCounterSpacing</c> head from <c>sbarinfo_commands.cpp</c> (**2101–2250**).
/// Chunk **1087**; follows **1086** ctor defaults and <c>Draw</c> `{` seam (**2100**).
/// </summary>
public static class VmSbarinfoCommandDrawInventoryBar1087Slice
{
    public const double HexenShadowAlphaFactor = 0x9800 / 65536.0;
    public const int DefaultCounterXOffsetFromBar = 26;
    public const int DefaultCounterYOffsetFromBar = 22;
    public const int MissingArtiboxSpacingFallback = 32;
    public const int StrifeHorizontalSpacingAdjust = -1;
    public const int NonStrifeHorizontalSpacingAdjust = 1;

    public enum InventoryBarStyle
    {
        Doom = 0,
        Heretic = 1,
        Hexen = 2,
        HexenStrict = 3,
        Strife = 4
    }

    public enum InventoryBarParseKeyword
    {
        AlwaysShow,
        DrawShadow,
        NoArtibox,
        NoArrows,
        AlwaysShowCounter,
        Translucent,
        Vertical,
        Unknown
    }

    public enum InventoryBarFlagListSeparator
    {
        PipeContinues,
        CommaRequired
    }

    public enum InventorySelectionGraphic
    {
        None,
        SelectBox,
        Cursor
    }

    public static double ApplyInventoryBarTranslucentBackgroundAlpha(double blockAlpha, bool translucent) =>
        translucent ? blockAlpha * HexenShadowAlphaFactor : blockAlpha;

    public static bool ShouldDrawInventoryBarWhenEmpty(bool hasFirstItem, bool alwaysShow) =>
        hasFirstItem || alwaysShow;

    public static (int Rx, int Ry) ComputeInventoryBarSlotOrigin(
        int baseX,
        int baseY,
        int slotIndex,
        int spacing,
        bool vertical) =>
        vertical
            ? (baseX, baseY + slotIndex * spacing)
            : (baseX + slotIndex * spacing, baseY);

    public static bool ShouldDrawInventoryIconBeforeSelectionCursor(InventoryBarStyle style) =>
        style != InventoryBarStyle.Strife;

    public static (int IconOffsetX, int IconOffsetY) ResolveInventoryIconDrawOffset(InventoryBarStyle style) =>
        style == InventoryBarStyle.HexenStrict ? (-2, -1) : (0, 0);

    public static (InventorySelectionGraphic Graphic, int DeltaX, int DeltaY) ResolveInventorySelectionOverlay(
        InventoryBarStyle style)
    {
        return style switch
        {
            InventoryBarStyle.Heretic => (InventorySelectionGraphic.SelectBox, 0, 29),
            InventoryBarStyle.Hexen => (InventorySelectionGraphic.SelectBox, 0, -1),
            InventoryBarStyle.HexenStrict => (InventorySelectionGraphic.SelectBox, -1, -1),
            InventoryBarStyle.Strife => (InventorySelectionGraphic.Cursor, -6, -2),
            _ => (InventorySelectionGraphic.SelectBox, 0, 0)
        };
    }

    public static bool ShouldDrawInventoryIconAfterSelectionForStrife(InventoryBarStyle style) =>
        style == InventoryBarStyle.Strife;

    public static bool ShouldDrawPerItemInventoryCounter(
        bool countersAllocated,
        bool alwaysShowCounter,
        int itemAmount) =>
        countersAllocated && (alwaysShowCounter || itemAmount != 1);

    public static bool ShouldFillRemainingArtiboxSlots(int drawnSlots, int barSize, bool noArtibox) =>
        drawnSlots < barSize && !noArtibox;

    public static (int Offset, int CrossAxisOffset) ResolveInventoryBarLeftArrowPlacement(
        InventoryBarStyle style,
        bool vertical)
    {
        var offset = style != InventoryBarStyle.Strife
            ? style != InventoryBarStyle.HexenStrict ? -12 : -10
            : 14;
        var cross = style != InventoryBarStyle.HexenStrict ? 0 : -1;
        return vertical ? (cross, offset) : (offset, cross);
    }

    public static (int Offset, int CrossAxisOffset) ResolveInventoryBarRightArrowPlacement(
        InventoryBarStyle style,
        int barSize,
        bool vertical)
    {
        var offset = style != InventoryBarStyle.Strife
            ? style != InventoryBarStyle.HexenStrict ? barSize * 31 + 2 : barSize * 31
            : barSize * 35 - 4;
        var cross = style != InventoryBarStyle.HexenStrict ? 0 : -1;
        return vertical ? (cross, offset) : (offset, cross);
    }

    public static bool ShouldDrawInventoryBarLeftArrow(bool noArrows, bool hasFirstItem, bool hasPreviousItem) =>
        !noArrows && hasFirstItem && hasPreviousItem;

    public static bool ShouldDrawInventoryBarRightArrow(bool noArrows, bool loopEndedWithItem) =>
        !noArrows && loopEndedWithItem;

    public static (bool Recognized, InventoryBarStyle Style) TryParseInventoryBarStyleKeyword(string identifier)
    {
        if (identifier.Equals("Doom", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarStyle.Doom);
        if (identifier.Equals("Heretic", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarStyle.Heretic);
        if (identifier.Equals("HexenStrict", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarStyle.HexenStrict);
        if (identifier.Equals("Hexen", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarStyle.Hexen);
        if (identifier.Equals("Strife", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarStyle.Strife);
        return (false, InventoryBarStyle.Doom);
    }

    public static (bool Recognized, InventoryBarParseKeyword Keyword) ClassifyInventoryBarParseKeyword(string identifier)
    {
        if (identifier.Equals("alwaysshow", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.AlwaysShow);
        if (identifier.Equals("drawshadow", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.DrawShadow);
        if (identifier.Equals("noartibox", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.NoArtibox);
        if (identifier.Equals("noarrows", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.NoArrows);
        if (identifier.Equals("alwaysshowcounter", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.AlwaysShowCounter);
        if (identifier.Equals("translucent", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.Translucent);
        if (identifier.Equals("vertical", StringComparison.OrdinalIgnoreCase))
            return (true, InventoryBarParseKeyword.Vertical);
        return (false, InventoryBarParseKeyword.Unknown);
    }

    public static InventoryBarFlagListSeparator ResolveInventoryBarFlagListSeparator(bool sawPipeSeparator) =>
        sawPipeSeparator
            ? InventoryBarFlagListSeparator.PipeContinues
            : InventoryBarFlagListSeparator.CommaRequired;

    public static (int CounterX, int CounterY) ResolveInventoryBarDefaultCounterPosition(int barX, int barY) =>
        (barX + DefaultCounterXOffsetFromBar, barY + DefaultCounterYOffsetFromBar);

    public static int SelectInventoryBarBoxDimensionForSpacing(
        int displayWidth,
        int displayHeight,
        bool vertical) =>
        vertical ? displayHeight : displayWidth;

    public static int ResolveInventoryBarCounterSpacingFromBoxDimension(
        int boxDimension,
        InventoryBarStyle style) =>
        boxDimension + (style != InventoryBarStyle.Strife
            ? NonStrifeHorizontalSpacingAdjust
            : StrifeHorizontalSpacingAdjust);
}
