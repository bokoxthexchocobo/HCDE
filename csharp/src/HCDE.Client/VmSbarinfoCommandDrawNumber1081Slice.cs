namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawNumber::Parse</c> flag tail, optional coloring, and <c>Tick</c> value dispatch head from
/// <c>sbarinfo_commands.cpp</c> (**1201–1350**). Chunk **1081**; follows **1080** inventory validation seam (**1200**).
/// </summary>
public static class VmSbarinfoCommandDrawNumber1081Slice
{
    public enum DrawNumberParseKeyword
    {
        FillZeros,
        WhenNotZero,
        DontCap,
        DrawShadow,
        Interpolate,
        Alignment,
        Prefix,
        Unknown
    }

    public static (bool Recognized, DrawNumberParseKeyword Keyword) ClassifyDrawNumberParseKeyword(string identifier)
    {
        if (identifier.Equals("fillzeros", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.FillZeros);
        if (identifier.Equals("whennotzero", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.WhenNotZero);
        if (identifier.Equals("dontcap", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.DontCap);
        if (identifier.Equals("drawshadow", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.DrawShadow);
        if (identifier.Equals("interpolate", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.Interpolate);
        if (identifier.Equals("alignment", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.Alignment);
        if (identifier.Equals("prefix", StringComparison.OrdinalIgnoreCase))
            return (true, DrawNumberParseKeyword.Prefix);
        return (false, DrawNumberParseKeyword.Unknown);
    }

    public enum DrawNumberFlagListSeparator
    {
        PipeContinues,
        CommaRequired
    }

    public static DrawNumberFlagListSeparator ResolveDrawNumberFlagListSeparator(bool sawPipeSeparator) =>
        sawPipeSeparator ? DrawNumberFlagListSeparator.PipeContinues : DrawNumberFlagListSeparator.CommaRequired;

    public static (bool EnableShadow, int ShadowX, int ShadowY) ApplyDrawNumberShadowFlag(
        bool sawDrawShadowKeyword,
        bool hasParenthesizedOffsets,
        int parsedShadowX,
        int parsedShadowY,
        int priorShadowX,
        int priorShadowY)
    {
        if (!sawDrawShadowKeyword)
            return (false, priorShadowX, priorShadowY);
        if (hasParenthesizedOffsets)
            return (true, parsedShadowX, parsedShadowY);
        return (true, priorShadowX, priorShadowY);
    }

    public static (bool Recognized, VmSbarinfoCommandDrawString1077Slice.DrawStringAlignment Alignment, bool InvalidKeyword)
        TryParseDrawNumberAlignment(string identifier) =>
        VmSbarinfoCommandDrawString1077Slice.TryParseDrawStringAlignment(identifier);

    public static (bool UsePrefix, string CombinedPadding) ResolveDrawNumberPrefixWhenConstant(
        bool requestedUsePrefix,
        bool valueIsConstant,
        string constantPrefix,
        string paddingSuffix)
    {
        if (!requestedUsePrefix)
            return (false, paddingSuffix);
        if (!valueIsConstant)
            return (true, paddingSuffix);
        return (false, constantPrefix + paddingSuffix);
    }

    public static bool ShouldParseDrawNumberLowHighColoring(
        bool sawCommaAfterCoordinates,
        bool parsedFontSpacingInt,
        bool sawCommaAfterSpacingSlot)
    {
        if (!sawCommaAfterCoordinates)
            return false;
        var needsComma = parsedFontSpacingInt;
        return !needsComma || sawCommaAfterSpacingSlot;
    }

    public static int ResolveDrawNumberParseClosingInterpolation(
        VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind valueKind,
        bool scriptInterpolateHealth,
        int scriptHealthSpeed,
        bool scriptInterpolateArmor,
        int scriptArmorSpeed,
        int parseFlagSpeed)
    {
        if (valueKind == VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Health && scriptInterpolateHealth)
            return scriptHealthSpeed;
        if (valueKind == VmSbarinfoCommandDrawString1079Slice.DrawNumberValueKind.Armor && scriptInterpolateArmor)
            return scriptArmorSpeed;
        return parseFlagSpeed;
    }

    public static int ClampDrawNumberHealthForDisplay(int health, bool lowerHealthCap) =>
        lowerHealthCap && health < 0 ? 0 : health;

    public static int SelectDrawNumberTickInterpolationSpeed(
        bool scriptInterpolates,
        int scriptSpeed,
        int currentSpeed) =>
        scriptInterpolates ? scriptSpeed : currentSpeed;

    public static int ResolveDrawNumberPrefixTickCache(bool usePrefix, int priorCache) =>
        usePrefix ? VmSbarinfoCommandDrawString1078Slice.InvalidStringCache : priorCache;

    public static (bool ShouldReturnEarly, string DisplayString) ResolveDrawNumberTickWhenAmmoMissing(
        bool ammoIsNull,
        string priorDisplayString) =>
        ammoIsNull ? (true, string.Empty) : (false, priorDisplayString);

    public static int ResolveInventoryDrawNumberAmount(bool itemFound, int amount) =>
        itemFound ? amount : 0;

    public static int ResolveArmorDrawNumberAmount(bool armorPresent, int armorAmount) =>
        armorPresent ? armorAmount : 0;
}
