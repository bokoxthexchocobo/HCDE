namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawKeyBar::Parse</c> tail from <c>sbarinfo_commands.cpp</c> (**2408–2451**). Chunk **1089**; **1088** owns **2400–2407** orientation/number seam.
/// </summary>
public static class VmSbarinfoCommandDrawKeyBar1089Slice
{
    public enum KeyBarParseFlag
    {
        None,
        ReverseRows,
        Reverse,
        Unknown
    }

    public static (bool Recognized, KeyBarParseFlag Flag) TryClassifyKeyBarParseFlagKeyword(string identifier)
    {
        if (identifier.Equals("reverserows", StringComparison.OrdinalIgnoreCase))
            return (true, KeyBarParseFlag.ReverseRows);
        if (identifier.Equals("reverse", StringComparison.OrdinalIgnoreCase))
            return (true, KeyBarParseFlag.Reverse);
        return (false, KeyBarParseFlag.Unknown);
    }

    public static (bool ContinueFlagLoop, bool RequireCommaBeforeIconSize) ResolveKeyBarFlagListContinuation(
        bool nextTokenIsPipe) =>
        nextTokenIsPipe
            ? (ContinueFlagLoop: true, RequireCommaBeforeIconSize: false)
            : (ContinueFlagLoop: false, RequireCommaBeforeIconSize: true);

    public static int ResolveKeyBarIconSizeFromParse(bool autoToken, int intConst) =>
        autoToken ? VmSbarinfoCommandDrawKeyBar1088Slice.UnspecifiedIconSize : intConst;

    public static (bool ParseKeyOffset, bool ParseRowLayout) ResolveKeyBarOptionalLayoutParse(
        bool commaAfterBaseCoordinates,
        bool commaAfterKeyOffset) =>
        !commaAfterBaseCoordinates
            ? (ParseKeyOffset: false, ParseRowLayout: false)
            : !commaAfterKeyOffset
                ? (ParseKeyOffset: true, ParseRowLayout: false)
                : (ParseKeyOffset: true, ParseRowLayout: true);

    public static int ResolveKeyBarRowIconSizeFromParse(bool autoToken, int intConst) =>
        autoToken ? VmSbarinfoCommandDrawKeyBar1088Slice.UnspecifiedIconSize : intConst;
}

/// <summary>
/// <c>CommandDrawBar</c> ctor, draw clip policy, and parse head from the same native band (**2467–2550**).
/// </summary>
public static class VmSbarinfoCommandDrawBar1089Slice
{
    public const int MissingImageIndex = -1;
    public const int DefaultBorder = 0;

    public enum DrawBarValueType
    {
        Health,
        Armor,
        Ammo1,
        Ammo2,
        Ammo,
        Frags,
        Kills,
        Items,
        Secrets,
        Airtime,
        SavePercent,
        PowerupTime,
        Inventory,
        Unknown
    }

    public readonly record struct DrawBarCtorDefaults(
        int Border,
        bool Horizontal,
        bool Reverse,
        int Foreground,
        int Background,
        DrawBarValueType Type,
        int InterpolationSpeed,
        double DrawValue,
        int Pixel);

    public static DrawBarCtorDefaults CreateDrawBarCtorDefaults() =>
        new(
            DefaultBorder,
            Horizontal: false,
            Reverse: false,
            Foreground: MissingImageIndex,
            Background: MissingImageIndex,
            Type: DrawBarValueType.Health,
            InterpolationSpeed: 0,
            DrawValue: 0,
            Pixel: MissingImageIndex);

    public static bool ShouldSkipDrawBarDraw(int foregroundIndex, bool foregroundImagePresent) =>
        foregroundIndex == MissingImageIndex || !foregroundImagePresent;

    public static double InvertNormalizedDrawValueForBorderDraw(int border, double drawValue) =>
        border != 0 ? 1.0 - drawValue : drawValue;

    public static bool BackgroundMatchesForegroundDimensions(
        bool backgroundPresent,
        double fgWidth,
        double fgHeight,
        double bgWidth,
        double bgHeight) =>
        backgroundPresent && bgWidth == fgWidth && bgHeight == fgHeight;

    public static double ComputeDrawBarClipFillRemaining(double sizeOfImage, double normalizedValue) =>
        sizeOfImage - sizeOfImage * normalizedValue;

    public static int ResolveDrawBarClipComponentIndex(bool horizontal, bool reverse) =>
        (!horizontal ? 1 : 0) | ((horizontal ? !reverse : reverse) ? 2 : 0);

    public static (double Cx, double Cy, double Cr, double Cb) ApplyDrawBarBorderClipInset(
        int border,
        double cx,
        double cy,
        double cr,
        double cb) =>
        border != 0
            ? (cx + border, cy + border, cr + border, cb + border)
            : (cx, cy, cr, cb);

    public static (bool Recognized, DrawBarValueType Type, bool UsesComparator) TryClassifyDrawBarStatKeyword(
        string identifier)
    {
        if (identifier.Equals("health", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Health, true);
        if (identifier.Equals("armor", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Armor, true);
        if (identifier.Equals("ammo1", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Ammo1, false);
        if (identifier.Equals("ammo2", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Ammo2, false);
        if (identifier.Equals("ammo", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Ammo, false);
        if (identifier.Equals("frags", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Frags, false);
        if (identifier.Equals("kills", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Kills, false);
        if (identifier.Equals("items", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Items, false);
        if (identifier.Equals("secrets", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Secrets, false);
        if (identifier.Equals("airtime", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.Airtime, false);
        if (identifier.Equals("savepercent", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.SavePercent, false);
        if (identifier.Equals("poweruptime", StringComparison.OrdinalIgnoreCase))
            return (true, DrawBarValueType.PowerupTime, false);
        return (false, DrawBarValueType.Unknown, false);
    }

    public static (bool UseDefaultAmmoClass, bool RequiresInventoryItemName) ResolveDrawBarAmmoParseShape(
        bool parenthesizedSyntax)
    {
        if (!parenthesizedSyntax)
            return (false, true);
        return (false, false);
    }

    public static bool ShouldReplaceDrawBarAmmoClass(bool classFound, bool isAmmoKind) =>
        !classFound || !isAmmoKind;
}
