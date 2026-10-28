namespace HCDE.Client;

/// <summary>
/// <c>V_InitCustomFonts</c> FONTDEFS lump parser policy from <c>v_font.cpp</c> (**151–300**).
/// Chunk **1058**; <c>fileSystem</c>, <c>TexMan</c>, and <c>FFont</c> construction remain host-injected.
/// </summary>
public static class VmVFontFontDefs1058Slice
{
    public const int DefaultStart = 33;
    public const int DefaultFirst = 33;
    public const int DefaultCount = 223;
    public const int DefaultSpaceWidth = -1;
    public const char DefaultCursor = '_';

    public enum FontDefFormat
    {
        None = 0,
        Template = 1,
        LumpMap = 2
    }

    public sealed class FontDefParseState
    {
        public FontDefFormat Format { get; set; }
        public int Start { get; set; } = DefaultStart;
        public int First { get; set; } = DefaultFirst;
        public int Count { get; set; } = DefaultCount;
        public int SpaceWidth { get; set; } = DefaultSpaceWidth;
        public int Kerning { get; set; }
        public char Cursor { get; set; } = DefaultCursor;
        public bool DontTranslate { get; set; }
        public bool IgnoreOffsets { get; set; }
        public int MinLuminosity { get; set; } = -1;
        public int MaxLuminosity { get; set; } = -1;
        public bool[] NoTranslate { get; } = new bool[256];
    }

    public static FontDefParseState CreateDefaults() => new();

    public static bool FormatConflict(FontDefFormat current, FontDefFormat required) =>
        current != FontDefFormat.None && current != required;

    public static bool TryApplyTemplateKeyword(FontDefParseState state, FontDefFormat requiredFormat)
    {
        if (FormatConflict(state.Format, requiredFormat))
            return false;
        state.Format = FontDefFormat.Template;
        return true;
    }

    public static bool TryApplyLumpMapKeyword(FontDefParseState state) =>
        !FormatConflict(state.Format, FontDefFormat.LumpMap) && (state.Format = FontDefFormat.LumpMap) == FontDefFormat.LumpMap;

    public static bool TryMarkNoTranslation(FontDefParseState state, int codePoint)
    {
        if (codePoint < 0 || codePoint >= state.NoTranslate.Length)
            return false;
        state.NoTranslate[codePoint] = true;
        return true;
    }

    public static int ClampLuminosity(long value) => (int)Math.Clamp(value, 0, 255);

    /// <summary>Native brace escape in lump-map glyph keys (**273–274**).</summary>
    public static string NormalizeFontGlyphKey(string token)
    {
        if (token == "-{")
            return "{";
        if (token == "-}")
            return "}";
        return token;
    }

    public static bool ShouldEmitMissingTextureMessage(int lumpContainer, int iwadNum) =>
        lumpContainer >= iwadNum;
}
