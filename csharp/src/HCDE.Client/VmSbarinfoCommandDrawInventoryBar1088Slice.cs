namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawInventoryBar::GetCounterSpacing</c> tail, <c>Tick</c> counter layout, and <c>CommandDrawKeyBar</c> draw head from
/// <c>sbarinfo_commands.cpp</c> (**2251–2400**). Chunk **1088**; follows **1087** null-box fallback (**2250**).
/// </summary>
public static class VmSbarinfoCommandDrawInventoryBar1088Slice
{
    public const int InventoryBarCounterLength = int.MaxValue;

    public static int ComputeLiveInventoryBarCounterSpacing(
        int boxDisplayWidth,
        int boxDisplayHeight,
        bool vertical,
        VmSbarinfoCommandDrawInventoryBar1087Slice.InventoryBarStyle style)
    {
        var boxDimension = VmSbarinfoCommandDrawInventoryBar1087Slice.SelectInventoryBarBoxDimensionForSpacing(
            boxDisplayWidth,
            boxDisplayHeight,
            vertical);
        return VmSbarinfoCommandDrawInventoryBar1087Slice.ResolveInventoryBarCounterSpacingFromBoxDimension(
            boxDimension,
            style);
    }

    public static (int StartX, int Y) ResolveInventoryBarCounterSlotCoordinates(
        int counterX,
        int counterY,
        int slotIndex,
        int spacing,
        bool vertical) =>
        (counterX + (vertical ? 0 : spacing * slotIndex), counterY + (vertical ? spacing * slotIndex : 0));

    public static (
        bool WhenNotZero,
        VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind ValueKind,
        int Length) ResolveInventoryBarCounterDrawNumberInit(bool alwaysShowCounter) =>
        (
            !alwaysShowCounter,
            VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Constant,
            InventoryBarCounterLength);

    public static (int ShadowX, int ShadowY, bool ShadowEnabled) CopyInventoryBarShadowToCounter(
        bool shadowEnabled,
        int shadowX,
        int shadowY) => (shadowX, shadowY, shadowEnabled);
}

/// <summary>
/// <c>CommandDrawKeyBar</c> ctor defaults and draw layout from the same native band (**2332–2400**).
/// </summary>
public static class VmSbarinfoCommandDrawKeyBar1088Slice
{
    public const int DefaultKeySlotCount = 3;
    public const int AutoKeyIconPadding = 2;
    public const int UnspecifiedIconSize = -1;

    public readonly record struct DrawKeyBarCtorDefaults(
        int Number,
        bool Vertical,
        bool Reverse,
        bool ReverseRows,
        int IconSize,
        int RowIconSize,
        int KeyOffset,
        int RowSize);

    public static DrawKeyBarCtorDefaults CreateDrawKeyBarCtorDefaults() =>
        new(
            DefaultKeySlotCount,
            Vertical: false,
            Reverse: false,
            ReverseRows: false,
            IconSize: UnspecifiedIconSize,
            RowIconSize: UnspecifiedIconSize,
            KeyOffset: 0,
            RowSize: 0);

    public static bool IsKeyBarInventoryItemDrawable(bool iconValid, bool isKeyKind) =>
        iconValid && isKeyKind;

    public static bool ShouldDrawKeyBarSlot(int slotIndex, int keyOffset) => slotIndex >= keyOffset;

    public static int ResolveKeyBarAutoRowWidth(int textureSpanPlusPadding, int rowIconSize) =>
        rowIconSize == UnspecifiedIconSize ? textureSpanPlusPadding : rowIconSize;

    public static int ResolveKeyBarAutoSlotStride(int textureWidth, int textureHeight, bool vertical) =>
        (vertical ? textureHeight : textureWidth) + AutoKeyIconPadding;

    public static int AdvanceKeyBarSlotOffset(int priorOffset, bool reverse, int stride) =>
        priorOffset + (reverse ? -stride : stride);

    public static int AdvanceKeyBarSlotOffsetWithFixedIconSize(int priorOffset, bool reverse, int iconSize) =>
        priorOffset + (reverse ? -iconSize : iconSize);

    public static bool ShouldWrapKeyBarRow(int slotIndex, int rowSize) =>
        rowSize > 0 && slotIndex % rowSize == rowSize - 1;

    public static int AdvanceKeyBarRowOffset(int priorRowOffset, int rowWidth, bool reverseRows) =>
        reverseRows ? priorRowOffset - rowWidth : priorRowOffset + rowWidth;

    public static (bool Recognized, bool Vertical, bool UnknownDirection) ClassifyKeyBarOrientationKeyword(
        string identifier)
    {
        if (identifier.Equals("vertical", StringComparison.OrdinalIgnoreCase))
            return (true, true, false);
        if (identifier.Equals("horizontal", StringComparison.OrdinalIgnoreCase))
            return (true, false, false);
        return (false, false, true);
    }
}
