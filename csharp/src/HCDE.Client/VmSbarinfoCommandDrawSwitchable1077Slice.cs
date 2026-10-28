namespace HCDE.Client;

/// <summary>
/// Switchable inventory <c>Tick</c> tail, off-image path, and <c>CommandDrawString</c> parse head from <c>sbarinfo_commands.cpp</c> (**601–750**).
/// Chunk **1077**; follows **1076** armor <c>drawAlt</c> seam (**601–605** excluded). Inventory compare uses **1075** evaluators.
/// </summary>
public static class VmSbarinfoCommandDrawSwitchable1077Slice
{
    public const double SwitchableOffImageAlpha = 1.0;

    public static int ResolveInventoryDrawAltWhenFirstFails(bool firstConditionMet) => firstConditionMet ? 0 : 1;

    public static bool EvaluateInventoryAmountCondition(
        bool itemPresent,
        VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator op,
        int threshold,
        int amount) =>
        itemPresent && VmSbarinfoCommandDrawSwitchable1075Slice.EvaluateSwitchableCompare(op, threshold, amount);

    public static int MergeInventoryDualConditionDrawAlt(int drawAlt, bool conditionAnd, bool secondConditionMet)
    {
        if (!conditionAnd)
            return drawAlt;
        if (secondConditionMet && drawAlt == 0)
            return 0;
        if (secondConditionMet && drawAlt == 1)
            return 3;
        if (!secondConditionMet && drawAlt == 0)
            return 2;
        return drawAlt;
    }

    public static int ResolveSwitchableOffImageIndex(int drawAlt) => drawAlt - 1;
}

/// <summary>
/// <c>CommandDrawString</c> parse and line-break layout policy from the same native band (**665–750**).
/// </summary>
public static class VmSbarinfoCommandDrawString1077Slice
{
    public const int DefaultBreakWidth = 320;
    public const int DefaultShadowOffset = 2;
    public const int LineBreakExtraLeading = 4;

    public enum DrawStringAlignment
    {
        Left,
        Center,
        Right
    }

    public enum DrawStringFontSource
    {
        Requested,
        SmallFallback
    }

    public static DrawStringFontSource ResolveDrawStringFontSource(bool fontLookupSucceeded) =>
        fontLookupSucceeded ? DrawStringFontSource.Requested : DrawStringFontSource.SmallFallback;

    public static (bool Recognized, DrawStringAlignment Alignment, bool InvalidKeyword) TryParseDrawStringAlignment(
        string identifier)
    {
        if (identifier.Equals("right", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringAlignment.Right, false);
        if (identifier.Equals("left", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringAlignment.Left, false);
        if (identifier.Equals("center", StringComparison.OrdinalIgnoreCase))
            return (true, DrawStringAlignment.Center, false);
        return (false, DrawStringAlignment.Right, true);
    }

    public static (bool EnableShadow, int ShadowX, int ShadowY) ApplyDrawShadowFlag(
        bool sawDrawShadowKeyword,
        bool hasParenthesizedOffsets,
        int parsedShadowX,
        int parsedShadowY,
        int priorShadowX,
        int priorShadowY)
    {
        if (!sawDrawShadowKeyword)
            return (false, priorShadowX, priorShadowY);
        if (hasParenthesizedOffsets)
            return (true, parsedShadowX, parsedShadowY);
        return (true, priorShadowX, priorShadowY);
    }

    public static (bool LineBreaksEnabled, int BreakWidth) ApplyLineBreaksFlag(
        bool sawLineBreaksKeyword,
        int parsedBreakWidth) =>
        sawLineBreaksKeyword ? (true, parsedBreakWidth) : (false, DefaultBreakWidth);

    public static bool ShouldContinueDrawStringFlagList(bool sawPipeSeparator, bool sawCommaSeparator) =>
        sawPipeSeparator || sawCommaSeparator;

    public static (uint Flags, bool UnknownKeyword) ClassifyDrawStringParseFlag(string identifier)
    {
        if (identifier.Equals("alignment", StringComparison.OrdinalIgnoreCase)
            || identifier.Equals("drawshadow", StringComparison.OrdinalIgnoreCase)
            || identifier.Equals("linebreaks", StringComparison.OrdinalIgnoreCase))
            return (0, false);
        return (0, true);
    }

    public static double ComputeLineBreakDrawY(double baseY, int lineIndex, double fontHeight) =>
        baseY + lineIndex * (fontHeight + LineBreakExtraLeading);
}
