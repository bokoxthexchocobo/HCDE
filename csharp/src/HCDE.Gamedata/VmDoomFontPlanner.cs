namespace HCDE.Gamedata;

public enum VmDoomSmallFontRecipe
{
    None,
    FontAReplaced,
    FontADefault,
    StcfnReplaced,
    StcfnDefault
}

/// <summary><c>InitDoomFonts</c> lump selection and fallbacks from <c>doomfont.cpp</c> (not <c>FFont</c> construction).</summary>
public static class VmDoomFontPlanner
{
    public const byte HuFontStart = (byte)'!';
    public const byte HuFontEnd = 0xFF;
    public static int HuFontSize => HuFontEnd - HuFontStart + 1;

    public static VmDoomSmallFontRecipe PlanSmallFont(bool hasFontaS, int fontA33Container, int iwadNum, bool hasStcfn033, int stcfn065Container)
    {
        if (hasFontaS)
            return fontA33Container > iwadNum ? VmDoomSmallFontRecipe.FontAReplaced : VmDoomSmallFontRecipe.FontADefault;
        if (hasStcfn033)
            return stcfn065Container > iwadNum ? VmDoomSmallFontRecipe.StcfnReplaced : VmDoomSmallFontRecipe.StcfnDefault;
        return VmDoomSmallFontRecipe.None;
    }

    public static bool ShouldSubstituteBigUpperWithBigFont(bool bigUpperIsFolder, bool bigFontIsFolder) =>
        bigUpperIsFolder && !bigFontIsFolder;

    public static string ResolveConFont(string? smallFont, string? conFont) => conFont ?? smallFont ?? "";

    public static string ResolveSmallFont2(bool hasStbfn033) => hasStbfn033 ? "STBFN" : "";

    public static string ResolveBigFont(bool hasFontbS, bool hasZBigFont) =>
        hasFontbS ? "FONTB" : hasZBigFont ? "ZBIGFONT" : "";

    /// <summary>Branch selection for BigFont / BigUpper without <c>new FFont</c> (native seam).</summary>
    public static (string BigFontRecipe, string OriginalBigRecipe) PlanBigFont(bool hasFontbS, bool hasZBigFont) =>
        hasFontbS
            ? ("FONTB", "FONTB")
            : hasZBigFont
                ? ("ZBIGFONT", "bigfont")
                : ("", "bigfont");

    public static string ResolveIntermissionFont(bool hasWinum0, string? bigFont) =>
        hasWinum0 ? "IntermissionFont_Doom" : bigFont ?? "BigFont";

    public static (string Small, string Big) ResolveOriginalFallbacks(string? originalSmall, string? conFont, string? originalBig, string? bigFont) =>
        (originalSmall ?? conFont ?? "", originalBig ?? bigFont ?? "");
}
