namespace HCDE.Client;

/// <summary>
/// <c>CommandDrawNumber::Tick</c> draw tail from <c>sbarinfo_commands.cpp</c> (**1501–1537**).
/// Chunk **1083**; follows **1082** interpolation seam (**1500**).
/// </summary>
public static class VmSbarinfoCommandDrawNumber1083Slice
{
    public const int DrawNumberLowHighDisabled = -1;

    public static (bool AbortTick, string DisplayString) ResolveDrawNumberWhenNotZeroEarlyExit(
        bool whenNotZero,
        int drawValue,
        string priorDisplayString) =>
        whenNotZero && drawValue == 0
            ? (true, string.Empty)
            : (false, priorDisplayString);

    public enum DrawNumberTranslationBand
    {
        Normal,
        Low,
        High
    }

    public static DrawNumberTranslationBand ResolveDrawNumberTranslationBand(
        int drawValue,
        int lowValue,
        int highValue)
    {
        if (lowValue != DrawNumberLowHighDisabled && drawValue <= lowValue)
            return DrawNumberTranslationBand.Low;
        if (highValue != DrawNumberLowHighDisabled && drawValue >= highValue)
            return DrawNumberTranslationBand.High;
        return DrawNumberTranslationBand.Normal;
    }

    public static int ComputeDrawNumberDigitCapMax(int length) =>
        length <= 9 ? (int)Math.Ceiling(Math.Pow(10, length)) - 1 : int.MaxValue;

    public static int ClampDrawNumberWhenDigitCapped(int drawValue, int length, bool fillZeros)
    {
        var maxval = ComputeDrawNumberDigitCapMax(length);
        if (!fillZeros || length == 1)
            return Math.Clamp(drawValue, -maxval, maxval);
        var minval = length <= 9
            ? (int)-(Math.Ceiling(Math.Pow(10, length - 1)) - 1)
            : int.MinValue;
        return Math.Clamp(drawValue, minval, maxval);
    }

    public static (int DrawValue, bool UseFillZeros) ApplyDrawNumberDontCapModulo(
        int drawValue,
        int length,
        bool fillZeros)
    {
        if (length > 9)
            return (drawValue, fillZeros);
        var exponentLength = length > 1 && drawValue < 0 ? length - 1 : length;
        var limit = (int)Math.Ceiling(Math.Pow(10, exponentLength));
        var useFillZeros = fillZeros;
        if (drawValue >= limit)
            useFillZeros = true;
        return (drawValue % limit, useFillZeros);
    }

    public static (int DrawValue, bool UseFillZeros) ApplyDrawNumberCapPolicy(
        int drawValue,
        int length,
        bool fillZeros,
        bool dontCap)
    {
        if (!dontCap)
            return (ClampDrawNumberWhenDigitCapped(drawValue, length, fillZeros), fillZeros);
        if (length <= 9)
            return ApplyDrawNumberDontCapModulo(drawValue, length, fillZeros);
        return (drawValue, fillZeros);
    }

    public static int ResolveDrawNumberZeroPadWidth(int length, int drawValue) =>
        drawValue < 0 ? length - 1 : length;

    public static string FormatDrawNumberDisplayString(
        bool useFillZeros,
        string prefixPart,
        string prefixPadding,
        int length,
        int drawValue)
    {
        if (useFillZeros)
        {
            var width = ResolveDrawNumberZeroPadWidth(length, drawValue);
            return $"{prefixPart}{prefixPadding}{Math.Abs(drawValue).ToString($"D{width}")}";
        }

        return $"{prefixPart}{prefixPadding}{drawValue}";
    }
}

/// <summary>
/// <c>CommandDrawMugShot</c> ctor defaults and <c>Parse</c> head from the same native band (**1598–1650**).
/// </summary>
public static class VmSbarinfoCommandDrawMugShot1083Slice
{
    public const int DefaultAccuracy = 5;
    public const int DefaultFaceMaxLength = 3;
    public const int MinimumAccuracy = 1;
    public const int MaximumAccuracy = 9;

    [Flags]
    public enum CommandDrawMugShotStateFlags
    {
        Standard = 0x0,
        XDeathFace = 0x1,
        AnimatedGodMode = 0x2,
        DisableGrin = 0x4,
        DisableOuch = 0x8,
        DisablePain = 0x10,
        DisableRampage = 0x20,
        Custom = 0x40
    }

    public enum CommandDrawMugShotParseKeyword
    {
        XDeathFace,
        AnimatedGodMode,
        DisableGrin,
        DisableOuch,
        DisablePain,
        DisableRampage,
        Custom,
        Unknown
    }

    public enum MugShotFlagListSeparator
    {
        PipeContinues,
        CommaRequired
    }

    public static (int Accuracy, CommandDrawMugShotStateFlags StateFlags) CreateCommandDrawMugShotDefaults() =>
        (DefaultAccuracy, CommandDrawMugShotStateFlags.Standard);

    public static bool IsCommandDrawMugShotDefaultFaceTooLong(int charCount) =>
        charCount > DefaultFaceMaxLength;

    public static bool IsCommandDrawMugShotAccuracyInRange(int accuracy) =>
        accuracy >= MinimumAccuracy && accuracy <= MaximumAccuracy;

    public static (bool Recognized, CommandDrawMugShotParseKeyword Keyword) ClassifyCommandDrawMugShotParseKeyword(
        string identifier)
    {
        if (identifier.Equals("xdeathface", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.XDeathFace);
        if (identifier.Equals("animatedgodmode", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.AnimatedGodMode);
        if (identifier.Equals("disablegrin", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.DisableGrin);
        if (identifier.Equals("disableouch", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.DisableOuch);
        if (identifier.Equals("disablepain", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.DisablePain);
        if (identifier.Equals("disablerampage", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.DisableRampage);
        if (identifier.Equals("custom", StringComparison.OrdinalIgnoreCase))
            return (true, CommandDrawMugShotParseKeyword.Custom);
        return (false, CommandDrawMugShotParseKeyword.Unknown);
    }

    public static CommandDrawMugShotStateFlags ApplyCommandDrawMugShotParseKeyword(
        CommandDrawMugShotStateFlags current,
        CommandDrawMugShotParseKeyword keyword) =>
        keyword switch
        {
            CommandDrawMugShotParseKeyword.XDeathFace =>
                current | CommandDrawMugShotStateFlags.XDeathFace,
            CommandDrawMugShotParseKeyword.AnimatedGodMode =>
                current | CommandDrawMugShotStateFlags.AnimatedGodMode,
            CommandDrawMugShotParseKeyword.DisableGrin =>
                current | CommandDrawMugShotStateFlags.DisableGrin,
            CommandDrawMugShotParseKeyword.DisableOuch =>
                current | CommandDrawMugShotStateFlags.DisableOuch,
            CommandDrawMugShotParseKeyword.DisablePain =>
                current | CommandDrawMugShotStateFlags.DisablePain,
            CommandDrawMugShotParseKeyword.DisableRampage =>
                current | CommandDrawMugShotStateFlags.DisableRampage,
            CommandDrawMugShotParseKeyword.Custom =>
                current | CommandDrawMugShotStateFlags.Custom,
            _ => current
        };

    public static MugShotFlagListSeparator ResolveCommandDrawMugShotFlagListSeparator(bool sawPipeSeparator) =>
        sawPipeSeparator ? MugShotFlagListSeparator.PipeContinues : MugShotFlagListSeparator.CommaRequired;
}
