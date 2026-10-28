namespace HCDE.Client;

/// <summary>
/// <c>DSBarInfo::DrawString</c>, translation lookup, and <c>AmmoType</c> from <c>sbarinfo.cpp</c> (**1351–1500**).
/// Chunk **1071**; follows **1070** <c>DrawGraphic</c>. Relative center and fullscreen anchors reuse **1068**/**1070** seams.
/// </summary>
public static class VmSbarinfoDrawString1071Slice
{
    public const int TextColorEscape = 0x1c;
    public const int GameRavenMask = 2 | 4;
    public const int UndefinedColorRange = int.MinValue;
    public const double HereticShadowAlphaFactor = 0x6800 / 65536.0;

    public static double ApplyCoordinateSpacing(double coordinateValue, int spacing) => coordinateValue + spacing;

    public static int ResolveBoldTranslationRange(int translation, int numTextColors) =>
        translation != 0 ? translation - 1 : numTextColors - 1;

    public static (double ScaleX, double ScaleY) ResolveDrawStringHudScale(
        bool fullScreenOffsets,
        double hudScaleX,
        double hudScaleY) =>
        fullScreenOffsets ? (hudScaleX, hudScaleY) : (1.0, 1.0);

    public static bool IsTextColorEscape(int characterCode) => characterCode == TextColorEscape;

    public static bool ShouldUpdateFontColorFromEscape(int parsedColorRange) =>
        parsedColorRange != UndefinedColorRange;

    public static double AdvanceDrawStringForSpace(
        double ax,
        char spacingCharacter,
        double fontSpaceWidth,
        double spacingCharacterWidth) =>
        spacingCharacter == '\0' ? ax + fontSpaceWidth : ax + spacingCharacterWidth;

    public static int ResolveGlyphAdvanceWidth(bool monospaceMode, int charWidth, int spacingCharacterWidth) =>
        monospaceMode ? spacingCharacterWidth : charWidth;

    public static double ApplyProportionalGlyphLeftInset(double ax, double displayLeftOffset) =>
        ax + displayLeftOffset + 1;

    public static double ApplyMonospaceAlignmentX(
        double rx,
        double glyphWidth,
        double spacingSize,
        VmSbarinfoParseTopLevel1065Slice.SpacingAlignment alignment) =>
        alignment switch
        {
            VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Center =>
                rx + (spacingSize / 2.0) - (glyphWidth / 2.0),
            VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Right => rx + spacingSize - glyphWidth,
            _ => rx
        };

    public static (double Rx, double Ry, double Rw, double Rh) BuildGlyphDrawRect(
        double ax,
        double ay,
        int xOffset,
        int yOffset,
        double displayWidth,
        double displayHeight) =>
        (ax + xOffset, ay + yOffset, displayWidth, displayHeight);

    public static (double Rx, double Ry, double Rw, double Rh) ScaleFullscreenGlyphRect(
        double rx,
        double ry,
        double rw,
        double rh,
        double scaleX,
        double scaleY) =>
        (rx * scaleX, ry * scaleY, rw * scaleX, rh * scaleY);

    public static (double Rx, double Ry) AnchorNegativeFullscreenGlyph(
        double rx,
        double ry,
        bool anchorFromRight,
        bool anchorFromBottom,
        double viewWidth,
        double viewHeight) =>
        (anchorFromRight ? viewWidth + rx : rx, anchorFromBottom ? viewHeight + ry : ry);

    public static double ComputeDrawStringShadowAlpha(double alpha) => alpha * HereticShadowAlphaFactor;

    public static (double ShadowX, double ShadowY) OffsetDrawStringShadow(
        double rx,
        double ry,
        int shadowX,
        int shadowY,
        double scaleX,
        double scaleY) =>
        (rx + shadowX * scaleX, ry + shadowY * scaleY);

    public static double AdvanceDrawStringCursorAfterGlyph(
        double ax,
        int glyphWidth,
        int spacing,
        double displayLeftOffset,
        bool proportionalMode,
        int spacingCharacterWidth) =>
        proportionalMode
            ? ax + glyphWidth + spacing - (displayLeftOffset + 1)
            : ax + spacingCharacterWidth + spacing;

    public static bool ShouldUseRavenExtraPlayerTranslation(int gameType) => (gameType & GameRavenMask) != 0;

    public static int ResolveTranslationIndex(bool translateRequested, int translationIndex) =>
        translateRequested ? translationIndex : 0;

    public static int SelectAmmoTypeFieldIndex(int ammoSlotNumber) => ammoSlotNumber == 1 ? 1 : 2;
}
