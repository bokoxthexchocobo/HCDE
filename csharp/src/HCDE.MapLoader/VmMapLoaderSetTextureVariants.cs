namespace HCDE.MapLoader;

/// <summary>MapLoader <c>SetTexture</c> variants and <c>SetMapThingUserData</c> from <c>maploader.cpp</c> (143–334, 1238–1273).</summary>
public static class VmMapLoaderSetTextureVariants
{
    public const int MissingTextureWarnLimit = 8;

    public static string TruncateFlatName(string name, int maxLen = 8) =>
        name.Length <= maxLen ? name : name[..maxLen];

    public static bool ShouldPrintMissingTextureWarning(int count) =>
        count <= MissingTextureWarnLimit;

    public static bool ShouldScanLinedefsForMissingSide(bool textureMissing) => textureMissing;

    public static (bool UseColormap, bool UseWallTexture, bool UseHex) ResolveDeepWaterBlend(
        string name,
        Func<string, uint> colormapLookup,
        Func<string, bool> wallExists)
    {
        var cmap = colormapLookup(name);
        if (cmap != 0)
            return (true, false, false);
        if (wallExists(name))
            return (false, true, false);
        return (false, false, true);
    }

    public static bool TryParseLegacyColorHash(string name, out uint color, out bool validColor)
    {
        color = 0;
        validColor = false;
        if (name.Length == 0 || name[0] == '#')
            return false;
        if (!uint.TryParse(name, System.Globalization.NumberStyles.HexNumber, null, out color))
            return false;
        validColor = name.Length >= 2 && name.Length <= 6;
        return true;
    }

    public static bool TryParseLegacyRgbHash(string name, out uint argb, out bool validColor, bool isFog)
    {
        argb = 0;
        validColor = false;
        if (name.Length == 0 || name[0] != '#')
            return false;
        if (name.Length < 7)
            return false;

        var body = name[1..];
        if (body.Length < 6)
            return false;

        static int Hex(char c) => c is >= '0' and <= '9' ? c - '0' : char.ToLowerInvariant(c) - 'a' + 10;
        int red = Hex(body[0]) * 16 + Hex(body[1]);
        int green = Hex(body[2]) * 16 + Hex(body[3]);
        int blue = Hex(body[4]) * 16 + Hex(body[5]);
        var factor = isFog ? 0 : Math.Clamp((body.Length > 6 ? body[6] & 0xDF : 'A') - 'A', 0, 25);
        if (!isFog && factor == 0)
            return false;
        var alpha = isFog ? 0 : factor * 255 / 25;
        argb = (uint)(alpha << 24 | red << 16 | green << 8 | blue);
        validColor = true;
        return true;
    }

    public static bool IsWritableUserVar(bool isNative, bool isPrivate, bool isProtected, bool isStatic, bool isScalar) =>
        !isNative && !isPrivate && !isProtected && !isStatic && isScalar;

    public static bool ShouldApplyStringUserData(string typeName) =>
        typeName.Equals("String", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldApplyFloatUserData(string typeName) =>
        typeName.Equals("float", StringComparison.OrdinalIgnoreCase)
        || typeName.Equals("double", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldApplyIntUserData(string typeName, bool isBool) =>
        isBool || typeName.Equals("int", StringComparison.OrdinalIgnoreCase);

    public static int AdvanceUserDataIndex(int index) => index + 1;

    public static bool ShouldSummarizeExtraUsage(int count) => count > MissingTextureWarnLimit;

    public static int ExtraUsageCount(int count) => count - MissingTextureWarnLimit;

    public static string FormatExtraUsageMessage(string textureName, int extraCount) =>
        $"Missing texture '{textureName}' is used {extraCount} more times\n";
}
