namespace HCDE.Gamedata;

public enum VmDoomSmallFontRecipe
{
    None,
    FontAReplaced,
    FontADefault,
    StcfnReplaced,
    StcfnDefault
}

/// <summary>Font lump selection logic from <c>InitDoomFonts</c> in <c>doomfont.cpp</c>.</summary>
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
}
