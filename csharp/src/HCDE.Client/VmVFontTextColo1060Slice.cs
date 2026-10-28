namespace HCDE.Client;

/// <summary>
/// <c>V_InitFontColors</c> TEXTCOLO tail, translation table build, and <c>V_FindFontColor</c> from <c>v_font.cpp</c> (**451–600**).
/// Chunk **1060**; follows **1059** (**301–450**). Scanner, <c>qsort</c>, and luminosity arena remain host-injected.
/// </summary>
public static class VmVFontTextColo1060Slice
{
    public const int FullPaletteRangeEnd = VmVFontFontDefsTextColo1059Slice.MaxColorRangePosition;
    public const int UntranslatedColorRange = -1;

    public enum TextColoBlockValidationError
    {
        None,
        MissingNormalRange,
        UntranslatedMustBeUndefined
    }

    public readonly record struct TranslationParmRgb(
        int RangeStart,
        int RangeEnd,
        byte StartR,
        byte StartG,
        byte StartB,
        byte EndR,
        byte EndG,
        byte EndB);

    public readonly record struct TranslationMapEntry(int NameIndex, int Number);

    public static TextColoBlockValidationError ValidateColorBlockParmLengths(
        int normalParmLen,
        string primaryColorName)
    {
        var untranslated = primaryColorName.Equals(
            VmVFontFontDefsTextColo1059Slice.UntranslatedColorName,
            StringComparison.OrdinalIgnoreCase);
        if (normalParmLen == 0)
        {
            if (!untranslated)
                return TextColoBlockValidationError.MissingNormalRange;
        }
        else if (untranslated)
        {
            return TextColoBlockValidationError.UntranslatedMustBeUndefined;
        }

        return TextColoBlockValidationError.None;
    }

    public static bool ShouldSynthesizeDefaultConsoleTranslation(int consoleParmLen, string primaryColorName) =>
        consoleParmLen == 0
        && !primaryColorName.Equals(
            VmVFontFontDefsTextColo1059Slice.UntranslatedColorName,
            StringComparison.OrdinalIgnoreCase);

    public static TranslationParmRgb CreateDefaultConsoleTranslation() => new(
        RangeStart: 0,
        RangeEnd: FullPaletteRangeEnd,
        StartR: 0,
        StartG: 0,
        StartB: 0,
        EndR: 255,
        EndG: 255,
        EndB: 255);

    public static bool ShouldRegisterNewColorName(int duplicateScanIndex, int existingCount) =>
        duplicateScanIndex == existingCount;

    public static int AllocateNextTranslationIndex(int currentAssignedIndex, ref int nextIndex)
    {
        if (currentAssignedIndex >= 0)
            return currentAssignedIndex;
        var assigned = nextIndex;
        nextIndex++;
        return assigned;
    }

    public static int CompareTranslationNameIndices(int leftNameIndex, int rightNameIndex) =>
        leftNameIndex.CompareTo(rightNameIndex);

    public static int BinarySearchTranslationNumber(
        IReadOnlyList<TranslationMapEntry> sortedLookup,
        int targetNameIndex)
    {
        var min = 0;
        var max = sortedLookup.Count - 1;
        while (min <= max)
        {
            var mid = (min + max) / 2;
            var probe = sortedLookup[mid];
            var cmp = CompareTranslationNameIndices(probe.NameIndex, targetNameIndex);
            if (cmp == 0)
                return probe.Number;
            if (cmp < 0)
                min = mid + 1;
            else
                max = mid - 1;
        }

        return UntranslatedColorRange;
    }
}
