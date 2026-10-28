namespace HCDE.Client;

/// <summary>
/// <c>V_GetFont</c> lookup policy and font TU prologue constants from <c>v_font.cpp</c> (**1–150**).
/// Chunk **1072**; precedes **1058** FONTDEFS band (**151–300**). Lump/texture/folder I/O and <c>FFont</c> ctors remain host-injected.
/// </summary>
public static class VmVFontGetFont1072Slice
{
    public const uint FontLumpMagicFon0 = 0x004E4F46;
    public const uint FontLumpMagicCompressed = 0x1AD5E6E1;
    public const int DefaultLogColorRgb = 223;

    public enum LegacyFontAliasKind
    {
        None,
        BigFont,
        ConsoleFont,
        IndexFont
    }

    public readonly record struct FontCreateFollowUp(bool CreatedFont, bool TranslationsLoaded);

    public static LegacyFontAliasKind ClassifyLegacyFontAlias(string name)
    {
        if (name.Equals("DBIGFONT", StringComparison.OrdinalIgnoreCase))
            return LegacyFontAliasKind.BigFont;
        if (name.Equals("CONFONT", StringComparison.OrdinalIgnoreCase))
            return LegacyFontAliasKind.ConsoleFont;
        if (name.Equals("INDEXFON", StringComparison.OrdinalIgnoreCase))
            return LegacyFontAliasKind.IndexFont;
        return LegacyFontAliasKind.None;
    }

    public static string CanonicalNameForLegacyAlias(LegacyFontAliasKind aliasKind, string originalName) =>
        aliasKind switch
        {
            LegacyFontAliasKind.BigFont => "BigFont",
            LegacyFontAliasKind.ConsoleFont => "ConsoleFont",
            LegacyFontAliasKind.IndexFont => "IndexFont",
            _ => originalName
        };

    public static bool ShouldAbortFontLookup(string? name) => name == null;

    public static string BuildFolderFontPath(string fontName) => $"fonts/{fontName}/";

    public static string ResolveFontLumpLookupName(string? fontLumpName, string canonicalName) =>
        string.IsNullOrEmpty(fontLumpName) ? canonicalName : fontLumpName;

    public static bool ShouldReturnExistingFont(object? foundFont) => foundFont != null;

    public static string ResolveCanonicalFontLookupName(string name)
    {
        var alias = ClassifyLegacyFontAlias(name);
        return CanonicalNameForLegacyAlias(alias, name);
    }

    public static bool ShouldTryBigUpperFallback(string name) =>
        name.Equals("BIGUPPER", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldPreferSingleLumpOverFolder(int lumpNum, int lumpContainer, int folderContainer) =>
        lumpNum != -1 && lumpContainer >= folderContainer;

    public static bool IsRecognizedSingleLumpFontHeader(uint lumpHead) =>
        (lumpHead & 0x00FFFFFFu) == FontLumpMagicFon0 || lumpHead == FontLumpMagicCompressed;

    public static bool ShouldLoadTranslationsAfterFontCreate(FontCreateFollowUp followUp) =>
        followUp.CreatedFont && followUp.TranslationsLoaded;

    public static bool ShouldCreatePicFontFromTexture(
        bool textureIsValid,
        int textureSourceLump,
        int folderContainer) =>
        textureIsValid && textureSourceLump >= folderContainer;

    public static bool ShouldInstantiateFolderFont(int folderEntryCount) => folderEntryCount > 0;

    public static int ResolveFolderContainerWhenPresent(bool folderScanSucceeded, int firstEntryContainer) =>
        folderScanSucceeded ? firstEntryContainer : -1;

    public static (byte R, byte G, byte B) DefaultLogPaletteEntry() =>
        ((byte)DefaultLogColorRgb, (byte)DefaultLogColorRgb, (byte)DefaultLogColorRgb);
}
