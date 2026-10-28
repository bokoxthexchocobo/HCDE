namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawImage</c> draw-scale and parse keyword policy from <c>sbarinfo_commands.cpp</c> (**1–150**).
/// Chunk **1073**; pivot after <c>sbarinfo.cpp</c> EOF (**1640**). Band hull excludes license banner and host I/O.
/// </summary>
public static class VmSbarinfoCommandDrawImage1073Slice
{
    public const int ForceDimensionUnset = -1;
    public const uint DiForceScale = 0x40;
    public const uint DiDrawInBox = 0x800000;
    public const uint DiAlternateOnFail = 0x1000000;

    public enum DrawImageType
    {
        PlayerIcon,
        Ammo1,
        Ammo2,
        Armor,
        WeaponIcon,
        Sigil,
        HexenArmorArmor,
        HexenArmorShield,
        HexenArmorHelm,
        HexenArmorAmulet,
        InventoryIcon,
        NormalImage
    }

    public static bool ShouldInvokeParentDrawBeforeTexture(uint flags) => (flags & DiAlternateOnFail) != 0;

    public static double ComputeFrameAlpha(double blockAlpha, double commandAlpha) => blockAlpha * commandAlpha;

    public static (int Width, int Height) ComputeDrawInBoxDimensions(
        int maxWidth,
        int maxHeight,
        double textureWidth,
        double textureHeight,
        double spawnScaleX,
        double spawnScaleY,
        uint flags)
    {
        var texWidth = (int)(textureWidth * spawnScaleX);
        var texHeight = (int)(textureHeight * spawnScaleY);
        var scale1 = 1.0;
        var scale2 = 1.0;
        if (maxWidth != ForceDimensionUnset && (maxWidth < texWidth || (flags & DiForceScale) != 0))
            scale1 = maxWidth / (double)texWidth;
        if (maxHeight != ForceDimensionUnset && (maxHeight < texHeight || (flags & DiForceScale) != 0))
            scale2 = maxHeight / (double)texHeight;
        if ((flags & DiForceScale) != 0)
        {
            if (maxWidth == ForceDimensionUnset || (maxHeight != ForceDimensionUnset && scale2 < scale1))
                scale1 = scale2;
        }
        else if (scale2 < scale1)
            scale1 = scale2;
        return ((int)(texWidth * scale1), (int)(texHeight * scale1));
    }

    public static (int Width, int Height) ResolveSpawnScaledDimensions(
        double textureWidth,
        double textureHeight,
        double spawnScaleX,
        double spawnScaleY) =>
        ((int)(textureWidth * spawnScaleX), (int)(textureHeight * spawnScaleY));

    public static bool ShouldUseDrawInBoxPath(uint flags) => (flags & DiDrawInBox) != 0;

    public static (DrawImageType Type, bool GetImage, bool SetTranslatable) ClassifyDrawImageKeyword(string identifier)
    {
        if (identifier.Equals("playericon", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.PlayerIcon, false, false);
        if (identifier.Equals("ammoicon1", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.Ammo1, false, false);
        if (identifier.Equals("ammoicon2", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.Ammo2, false, false);
        if (identifier.Equals("armoricon", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.Armor, false, false);
        if (identifier.Equals("weaponicon", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.WeaponIcon, false, false);
        if (identifier.Equals("sigil", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.Sigil, false, false);
        if (identifier.Equals("translatable", StringComparison.OrdinalIgnoreCase))
            return (DrawImageType.NormalImage, true, true);
        return (DrawImageType.InventoryIcon, false, false);
    }

    public static DrawImageType ResolveHexenArmorPiece(string pieceName)
    {
        if (pieceName.Equals("armor", StringComparison.OrdinalIgnoreCase))
            return DrawImageType.HexenArmorArmor;
        if (pieceName.Equals("shield", StringComparison.OrdinalIgnoreCase))
            return DrawImageType.HexenArmorShield;
        if (pieceName.Equals("helm", StringComparison.OrdinalIgnoreCase))
            return DrawImageType.HexenArmorHelm;
        if (pieceName.Equals("amulet", StringComparison.OrdinalIgnoreCase))
            return DrawImageType.HexenArmorAmulet;
        return DrawImageType.HexenArmorArmor;
    }

    public static bool IsUnknownHexenArmorPiece(string pieceName) =>
        !pieceName.Equals("armor", StringComparison.OrdinalIgnoreCase)
        && !pieceName.Equals("shield", StringComparison.OrdinalIgnoreCase)
        && !pieceName.Equals("helm", StringComparison.OrdinalIgnoreCase)
        && !pieceName.Equals("amulet", StringComparison.OrdinalIgnoreCase);

    public static (bool Parenthesized, bool GetImageAfterComma) BeginHexenArmorParse(bool sawOpenParen) =>
        (sawOpenParen, true);

    public static int ResolveInventoryImageIndex() => ForceDimensionUnset;

    public static bool ShouldCloseHexenArmorParen(bool parenthesized, bool sawImageToken) =>
        parenthesized && sawImageToken;
}
