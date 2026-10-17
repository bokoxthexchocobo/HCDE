using HCDE.Gamedata;

namespace HCDE.Playsim;

public enum ActorBlendOperation : byte
{
    None, Add, Subtract, ReverseSubtract, Fuzz, FuzzOrAdd, FuzzOrSubtract, FuzzOrReverseSubtract, Shadow,
}

public enum ActorAlphaFactor : byte
{
    Zero, One, Source, InverseSource, SourceColor, InverseSourceColor,
    DestinationColor, InverseDestinationColor, Destination, InverseDestination,
}

/// <summary>Native flexible render-style visibility calculation; rendering is handled separately.</summary>
public readonly record struct ActorRenderStyle(ActorBlendOperation Operation, ActorAlphaFactor Source,
    ActorAlphaFactor Destination, byte Flags = 0)
{
    public static ActorRenderStyle Normal => new(ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource, 2);
    public static ActorRenderStyle OptFuzzy => new(ActorBlendOperation.FuzzOrAdd, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource);
    public static ActorRenderStyle Translucent => new(ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource);

    private static readonly (int Index, ActorRenderStyle Style)[] AcsStyles =
    [
        (0, FromDehacked(DehackedRenderStyle.None)), (1, Normal),
        (2, FromDehacked(DehackedRenderStyle.Fuzzy)), (3, Translucent with { Flags = 1 }),
        (4, OptFuzzy), (5, Translucent with { Flags = 10 }),
        (64, Translucent), (65, FromDehacked(DehackedRenderStyle.Add)),
        (66, Translucent with { Flags = 12 }), (67, Translucent with { Flags = 8 }),
        (68, new(ActorBlendOperation.Shadow, ActorAlphaFactor.Zero, ActorAlphaFactor.Zero)),
        (69, new(ActorBlendOperation.ReverseSubtract, ActorAlphaFactor.Source, ActorAlphaFactor.One)),
        (6, FromDehacked(DehackedRenderStyle.Add) with { Flags = 8 }),
        (7, FromDehacked(DehackedRenderStyle.Add) with { Flags = 12 }),
    ];

    public static bool TryFromAcs(int index, out ActorRenderStyle style)
    {
        foreach (var entry in AcsStyles)
            if (entry.Index == index) { style = entry.Style; return true; }
        style = default;
        return false;
    }

    public int ToAcs()
    {
        foreach (var entry in AcsStyles)
            if (entry.Style == this) return entry.Index;
        return 1;
    }

    public static ActorRenderStyle FromNative(int mode)
    {
        if ((uint)mode < (uint)AcsStyles.Length) return AcsStyles[mode].Style;
        return mode switch
        {
            14 => new(ActorBlendOperation.Add, ActorAlphaFactor.DestinationColor, ActorAlphaFactor.Zero),
            15 => new(ActorBlendOperation.Add, ActorAlphaFactor.InverseDestinationColor, ActorAlphaFactor.Zero),
            16 => new(ActorBlendOperation.Add, ActorAlphaFactor.SourceColor, ActorAlphaFactor.InverseSourceColor),
            17 => new(ActorBlendOperation.Add, ActorAlphaFactor.One, ActorAlphaFactor.Zero),
            18 => new(ActorBlendOperation.Add, ActorAlphaFactor.SourceColor, ActorAlphaFactor.One),
            _ => FromDehacked(DehackedRenderStyle.None),
        };
    }

    public int ToNative()
    {
        for (var mode = 0; mode < 19; mode++)
            if (FromNative(mode) == this) return mode;
        return -1;
    }

    public static ActorRenderStyle FromDehacked(DehackedRenderStyle style) => style switch
    {
        DehackedRenderStyle.None => new(ActorBlendOperation.None, ActorAlphaFactor.Zero, ActorAlphaFactor.Zero),
        DehackedRenderStyle.Normal => Normal,
        DehackedRenderStyle.Fuzzy => new(ActorBlendOperation.Fuzz, ActorAlphaFactor.Source, ActorAlphaFactor.InverseSource),
        DehackedRenderStyle.SoulTrans => Translucent with { Flags = 1 },
        DehackedRenderStyle.OptFuzzy => OptFuzzy,
        DehackedRenderStyle.Translucent => Translucent,
        DehackedRenderStyle.Add => new(ActorBlendOperation.Add, ActorAlphaFactor.Source, ActorAlphaFactor.One),
        DehackedRenderStyle.Shaded => Translucent with { Flags = 12 },
        _ => throw new ArgumentOutOfRangeException(nameof(style)),
    };

    public bool IsVisible(double alpha)
    {
        if (!double.IsFinite(alpha)) throw new ArgumentOutOfRangeException(nameof(alpha));
        if (Operation == ActorBlendOperation.None) return false;
        if (Operation is not (ActorBlendOperation.Add or ActorBlendOperation.ReverseSubtract)) return true;
        alpha = (Flags & 2) != 0 ? 1 : Math.Clamp(alpha, 0, 1);
        return AlphaValue(Source, alpha) != 0 || AlphaValue(Destination, alpha) != 1;
    }

    private static double AlphaValue(ActorAlphaFactor factor, double alpha) => factor switch
    {
        ActorAlphaFactor.Zero => 0,
        ActorAlphaFactor.One => 1,
        ActorAlphaFactor.Source => alpha,
        ActorAlphaFactor.InverseSource => 1 - alpha,
        // Native GetAlpha uses 0.5 for factors that depend on pixel data.
        _ => 0.5,
    };
}
