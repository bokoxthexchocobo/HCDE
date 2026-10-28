namespace HCDE.Gamedata;

/// <summary>HU font glyph range constants from <c>doomfont.cpp</c>.</summary>
public static class VmDoomFontConstants
{
    public const byte HuFontStart = (byte)'!';
    public const byte HuFontEnd = 0xFF;
    public static int HuFontSize => HuFontEnd - HuFontStart + 1;
}
