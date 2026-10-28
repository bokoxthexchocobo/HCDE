namespace HCDE.Client;

/// <summary>
/// <c>CommandAspectRatio::FindRatio</c> and default <c>ratioMap</c> from <c>sbarinfo_commands.cpp</c> (**1951–1990**).
/// Chunk **1086**; follows **1085** aspect parse through <c>ASPECTRATIO_5_4</c> (**1950**). String tokens stay on **1085**.
/// </summary>
public static class VmSbarinfoCommandAspectRatio1086Slice
{
    public readonly record struct AspectRatioTarget(
        float CanonicalAspect,
        VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind Kind);

    public static AspectRatioTarget[] CreateNearestRatioTargetTable() =>
    [
        new(21f / 9f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_9),
        new(16f / 9f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_9),
        new(17f / 10f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio17_10),
        new(16f / 10f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_10),
        new(4f / 3f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio4_3),
        new(5f / 4f, VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio5_4)
    ];

    public static VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind SelectNearestAspectRatioKind(
        float activeAspect,
        ReadOnlySpan<AspectRatioTarget> targets)
    {
        var bestKind = targets[0].Kind;
        var bestDistance = MathF.Abs(targets[0].CanonicalAspect - activeAspect);
        for (var i = 1; i < targets.Length; i++)
        {
            var distance = MathF.Abs(targets[i].CanonicalAspect - activeAspect);
            if (distance < bestDistance)
            {
                bestKind = targets[i].Kind;
                bestDistance = distance;
            }
        }

        return bestKind;
    }

    public static int[] CreateDefaultAspectRatioRemapTable() =>
    [
        (int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio4_3,
        (int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_9,
        (int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_10,
        (int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio16_10,
        (int)VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind.Ratio5_4
    ];

    public static bool ResolveAspectRatioTickTruthFromNearestFind(
        int[] ratioMap,
        VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind configuredRatio,
        VmSbarinfoCommandAspectRatio1085Slice.AspectRatioKind nearestKind) =>
        VmSbarinfoCommandAspectRatio1085Slice.ResolveAspectRatioFlowControlTruth(
            ratioMap[(int)nearestKind],
            (int)configuredRatio);
}

/// <summary>
/// <c>CommandDrawShader</c> ctor defaults and parse validation from the same band (**1994–2039**).
/// </summary>
public static class VmSbarinfoCommandDrawShader1086Slice
{
    public static (bool WidthValid, bool HeightValid) ValidateDrawShaderDimensions(int width, int height) =>
        (width >= 1, height >= 1);

    public static (bool Recognized, bool Vertical, bool UnknownDirection) ClassifyDrawShaderDirectionKeyword(
        string identifier)
    {
        if (identifier.Equals("vertical", StringComparison.OrdinalIgnoreCase))
            return (true, true, false);
        if (identifier.Equals("horizontal", StringComparison.OrdinalIgnoreCase))
            return (true, false, false);
        return (false, false, true);
    }

    public static (bool ReverseEnabled, bool ExpectTrailingComma) ApplyDrawShaderReverseKeyword(bool sawReverseKeyword) =>
        sawReverseKeyword ? (true, true) : (false, false);

    public static int ResolveDrawShaderTextureSlotIndex(bool vertical, bool reverse) =>
        (vertical ? 2 : 0) + (reverse ? 1 : 0);
}

/// <summary>
/// <c>CommandDrawInventoryBar</c> ctor defaults from the same band (**2054–2063**).
/// </summary>
public static class VmSbarinfoCommandDrawInventoryBar1086Slice
{
    public const int DefaultBarSize = 7;
    public const int DefaultShadowOffset = 2;

    public enum InventoryBarStyle
    {
        Doom = 0
    }

    public readonly record struct DrawInventoryBarCtorDefaults(
        InventoryBarStyle Style,
        int Size,
        bool AlwaysShow,
        bool NoArtibox,
        bool NoArrows,
        bool AlwaysShowCounter,
        bool Translucent,
        bool Vertical,
        bool Shadow,
        int ShadowX,
        int ShadowY,
        int FontSpacing);

    public static DrawInventoryBarCtorDefaults CreateDrawInventoryBarCtorDefaults() =>
        new(
            InventoryBarStyle.Doom,
            DefaultBarSize,
            AlwaysShow: false,
            NoArtibox: false,
            NoArrows: false,
            AlwaysShowCounter: false,
            Translucent: false,
            Vertical: false,
            Shadow: false,
            ShadowX: DefaultShadowOffset,
            ShadowY: DefaultShadowOffset,
            FontSpacing: 0);
}
