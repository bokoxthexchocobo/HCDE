namespace HCDE.Client;

/// <summary>
/// <c>CreateLuminosityTranslationRanges</c>, <c>V_ApplyLuminosityTranslation</c>, and <c>CalcDefaultTranslation</c> head from <c>v_font.cpp</c> (**601–750**).
/// Chunk **1061**; follows **1060** (**451–600**). <c>ImageArena</c>, <c>FFont::RecordAllTextureColors</c>, and pixel buffers remain host-injected.
/// </summary>
public static class VmVFontLuminosity1061Slice
{
    /// <summary>Native <c>CR_UNTRANSLATED</c> table index in the luminosity build loop.</summary>
    public const int UntranslatedTableIndex = 12;

    public readonly record struct TranslationParmSlice(
        int RangeStart,
        int RangeEnd,
        byte StartR,
        byte StartG,
        byte StartB,
        byte EndR,
        byte EndG,
        byte EndB);

    public static int ComputeRemapPaletteOffset(int tableIndex, int variant) =>
        tableIndex * 512 + variant * 256;

    public static bool ShouldBuildLuminosityRemap(int tableIndex) =>
        tableIndex != UntranslatedTableIndex;

    public static bool ContainsLuminance(TranslationParmSlice parm, int luminance) =>
        parm.RangeStart <= luminance && parm.RangeEnd >= luminance;

    public static bool ShouldAdvanceTranslationParm(TranslationParmSlice current, TranslationParmSlice next) =>
        next.RangeStart > current.RangeEnd;

    public static (byte R, byte G, byte B) InterpolateTranslationColor(int luminance, TranslationParmSlice parm)
    {
        var divisor = parm.RangeEnd - parm.RangeStart;
        if (divisor <= 0)
            return (parm.StartR, parm.StartG, parm.StartB);

        var rangeV = ((luminance - parm.RangeStart) << 8) / divisor;
        var r = ((parm.StartR << 8) + rangeV * (parm.EndR - parm.StartR)) >> 8;
        var g = ((parm.StartG << 8) + rangeV * (parm.EndG - parm.StartG)) >> 8;
        var b = ((parm.StartB << 8) + rangeV * (parm.EndB - parm.StartB)) >> 8;
        return ((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }

    public static bool ShouldSkipLuminosityApply(int colorRange, int maxColorRange) =>
        colorRange >= maxColorRange;

    public static int ComputeGrayLuminance(byte b, byte g, byte r) =>
        (77 * r + 143 * g + 37 * b) >> 8;

    public static int ComputeLuminanceRemapIndex(int gray, int lumMin, int lumMax)
    {
        var lumRange = lumMax - lumMin + 1;
        if (lumRange <= 0)
            return 0;
        var lumAdjust = (gray - lumMin) * 255 / lumRange;
        return Math.Clamp(lumAdjust, 0, 255);
    }

    public static (byte R, byte G, byte B) ScaleRgbBelowMin(byte r, byte g, byte b, int gray, int lumMin)
    {
        if (lumMin == 0)
            return (r, g, b);
        return (
            (byte)(r * gray / lumMin),
            (byte)(g * gray / lumMin),
            (byte)(b * gray / lumMin));
    }

    public static (byte R, byte G, byte B) ScaleRgbAboveMax(byte r, byte g, byte b, int gray, int lumMax)
    {
        if (lumMax == 0)
            return (r, g, b);
        return (
            (byte)Math.Clamp(r * gray / lumMax, 0, 255),
            (byte)Math.Clamp(g * gray / lumMax, 0, 255),
            (byte)Math.Clamp(b * gray / lumMax, 0, 255));
    }

    public static bool ShouldMapDefaultTranslationLum(double lum) => lum is >= 0 and <= 1;

    public static int DefaultTranslationRemapIndex(double lum) => (int)(lum * 255);

    public static (byte R, byte G, byte B) InterpolateDefaultRemapGap(
        byte lowR,
        byte lowG,
        byte lowB,
        byte highR,
        byte highG,
        byte highB,
        int lowIndex,
        int highIndex,
        int index)
    {
        var weight = (index - lowIndex) / (double)(highIndex - lowIndex);
        var r = (int)(lowR + weight * (highR - lowR));
        var g = (int)(lowG + weight * (highG - lowG));
        var b = (int)(lowB + weight * (highB - lowB));
        return ((byte)Math.Clamp(r, 0, 255), (byte)Math.Clamp(g, 0, 255), (byte)Math.Clamp(b, 0, 255));
    }
}
