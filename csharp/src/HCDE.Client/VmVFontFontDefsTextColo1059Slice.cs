namespace HCDE.Client;

/// <summary>
/// Tail of <c>V_InitCustomFonts</c> lump-map build and <c>V_InitFontColors</c> TEXTCOLO head from <c>v_font.cpp</c> (**301–450**).
/// Chunk **1059**; follows **1058** (**151–300**). <c>CreateSpecialFont</c>, <c>V_GetColor</c>, and scanner I/O remain host-injected.
/// </summary>
public static class VmVFontFontDefsTextColo1059Slice
{
    public const int GlyphSlotCount = 256;
    public const int MaxColorRangePosition = 256;
    public const string UntranslatedColorName = "Untranslated";

    public readonly record struct LumpMapGlyphSpan(int First, int Count);

    public readonly record struct ColorRangeSpan(int Start, int End);

    public enum ColorRangeValidationError
    {
        None,
        FirstRangeMustStartAtZero,
        OutOfBounds,
        OverlapsPrevious,
        EndNotAfterStart,
        TooManyRanges
    }

    public static LumpMapGlyphSpan ComputeLumpMapGlyphSpan(ReadOnlySpan<bool> hasGlyph)
    {
        if (hasGlyph.Length != GlyphSlotCount)
            throw new ArgumentException($"Expected {GlyphSlotCount} glyph slots.", nameof(hasGlyph));

        var first = -1;
        for (var i = 0; i < GlyphSlotCount; i++)
        {
            if (!hasGlyph[i])
                continue;
            first = i;
            break;
        }

        if (first < 0)
            return new LumpMapGlyphSpan(0, 0);

        var last = first;
        for (var i = GlyphSlotCount - 1; i >= 0; i--)
        {
            if (!hasGlyph[i])
                continue;
            last = i;
            break;
        }

        return new LumpMapGlyphSpan(first, last - first + 1);
    }

    public static bool ShouldInstantiateLumpMapFont(int count) => count > 0;

    public static bool ShouldApplyExplicitSpaceWidth(int spaceWidth) => spaceWidth >= 0;

    public static bool RejectsAliasForUntranslated(string primaryName, IReadOnlyList<string> additionalNames) =>
        primaryName.Equals(UntranslatedColorName, StringComparison.OrdinalIgnoreCase)
        && additionalNames.Count > 0;

    public static bool TrySwitchToConsoleRange(int currentParmChoice, out int newParmChoice, out bool duplicateConsoleSet)
    {
        duplicateConsoleSet = currentParmChoice == 1;
        if (duplicateConsoleSet)
        {
            newParmChoice = currentParmChoice;
            return false;
        }

        newParmChoice = 1;
        return true;
    }

    public static ColorRangeValidationError ValidateExplicitColorRange(int previousRangeEnd, int rangeStart, int rangeEnd)
    {
        if (previousRangeEnd == -1 && rangeStart != 0)
            return ColorRangeValidationError.FirstRangeMustStartAtZero;
        if (rangeStart < 0 || rangeStart > MaxColorRangePosition)
            return ColorRangeValidationError.OutOfBounds;
        if (rangeEnd < 0 || rangeEnd > MaxColorRangePosition)
            return ColorRangeValidationError.OutOfBounds;
        if (rangeStart <= previousRangeEnd)
            return ColorRangeValidationError.OverlapsPrevious;
        if (rangeEnd <= rangeStart)
            return ColorRangeValidationError.EndNotAfterStart;
        return ColorRangeValidationError.None;
    }

    public static bool TryBuildImplicitColorRange(int previousRangeEnd, out ColorRangeSpan span, out ColorRangeValidationError error)
    {
        var start = previousRangeEnd + 1;
        var end = MaxColorRangePosition;
        if (start >= end)
        {
            span = default;
            error = ColorRangeValidationError.TooManyRanges;
            return false;
        }

        span = new ColorRangeSpan(start, end);
        error = ColorRangeValidationError.None;
        return true;
    }

    public static (byte R, byte G, byte B) SplitRgb(int color) =>
        ((byte)(color & 0xff), (byte)((color >> 8) & 0xff), (byte)((color >> 16) & 0xff));
}
