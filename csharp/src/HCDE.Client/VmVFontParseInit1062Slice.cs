namespace HCDE.Client;

/// <summary>
/// <c>CalcDefaultTranslation</c> tail, log/parse color helpers, and font init/load head from <c>v_font.cpp</c> (**751–900**).
/// Chunk **1062**; follows **1061** (**601–750**). <c>V_FindFontColor</c>, lump I/O, and <c>FFont</c> hosts remain injected.
/// </summary>
public static class VmVFontParseInit1062Slice
{
    public const int ColorUndefined = -1;
    public const int TranslationIndexMask = 0x3fff << 16;

    public enum FontColorEscapeKind
    {
        Incomplete,
        NormalAlias,
        BoldAlias,
        TeamChatAlias,
        ChatAlias,
        NamedBracket,
        StandardLetter
    }

    public static (int LowIndex, int HighIndex) AdvanceGapFillCursor(int lowIndex, int highIndex) =>
        (highIndex, highIndex + 1);

    public static bool ShouldUseDefaultLogColor(int range, int translationColorCount) =>
        (uint)range >= (uint)translationColorCount;

    public static int ResolveStandardColorLetter(int ch, int numTextColors)
    {
        if (ch >= 'A' && ch < numTextColors + 'A')
            return ch - 'A';
        if (ch >= 'a' && ch < numTextColors + 'a')
            return ch - 'a';
        return ColorUndefined;
    }

    public static FontColorEscapeKind ClassifyFontColorEscape(int leadByte)
    {
        return leadByte switch
        {
            '-' => FontColorEscapeKind.NormalAlias,
            '+' => FontColorEscapeKind.BoldAlias,
            '!' => FontColorEscapeKind.TeamChatAlias,
            '*' => FontColorEscapeKind.ChatAlias,
            '[' => FontColorEscapeKind.NamedBracket,
            >= 'A' and <= 'Z' => FontColorEscapeKind.StandardLetter,
            >= 'a' and <= 'z' => FontColorEscapeKind.StandardLetter,
            _ => FontColorEscapeKind.Incomplete
        };
    }

    public static bool TryParseNamedColorToken(ReadOnlySpan<byte> input, out ReadOnlySpan<byte> remainder, out string name)
    {
        name = string.Empty;
        if (input.Length == 0 || input[0] != '[')
        {
            remainder = input;
            return false;
        }

        var start = 1;
        var end = start;
        while (end < input.Length && input[end] != ']' && input[end] != 0)
            end++;
        name = System.Text.Encoding.Latin1.GetString(input.Slice(start, end - start));
        remainder = end < input.Length && input[end] == ']' ? input[(end + 1)..] : input[end..];
        return true;
    }

    public static int RewindIncompleteColorParse(int consumedLead, int bytesAdvanced) =>
        bytesAdvanced - (consumedLead == 0 ? 0 : 1);

    public static bool RequiresConsoleHexFont(int lumpNum) => lumpNum < 0;

    public static int BigFontDefaultTranslationTableIndex() =>
        VmVFontLuminosity1061Slice.UntranslatedTableIndex * 2 + 1;

    public static int SmallFontDefaultTranslationTableIndex() =>
        VmVFontLuminosity1061Slice.UntranslatedTableIndex * 2;

    public static int PatchLuminosityTranslationIndex(int existingIndex, int tableIndex) =>
        (existingIndex & ~TranslationIndexMask) | (tableIndex << 16);

    public static bool ShouldLoadFontTranslations(bool noTranslate) => !noTranslate;
}
