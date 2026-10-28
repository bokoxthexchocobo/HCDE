namespace HCDE.Playsim;

/// <summary>Native <c>LS_Light_*</c> from <c>p_lnspec.cpp</c> (2066–2148).</summary>
public static class VmPLnspecLightLineSpecials
{
    public enum LightKind
    {
        ForceLightning,
        RaiseByValue,
        LowerByValue,
        ChangeToValue,
        Fade,
        Glow,
        Flicker,
        Strobe,
        StrobeDoom,
        MinNeighbor,
        MaxNeighbor,
        Stop,
    }

    public readonly record struct LightInvoke(LightKind Kind, int Tag, int Arg1, int Arg2, int Arg3, int Arg4);

    public static LightInvoke ForceLightning(int mode) => new(LightKind.ForceLightning, 0, mode, 0, 0, 0);

    public static LightInvoke RaiseByValue(int tag, int value) => new(LightKind.RaiseByValue, tag, value, 0, 0, 0);

    public static int LowerByValueDelta(int value) => -value;

    public static LightInvoke LowerByValue(int tag, int value) =>
        new(LightKind.LowerByValue, tag, LowerByValueDelta(value), 0, 0, 0);

    public static LightInvoke ChangeToValue(int tag, int value) => new(LightKind.ChangeToValue, tag, value, 0, 0, 0);

    public static LightInvoke Fade(int tag, int value, int delay) =>
        new(LightKind.Fade, tag, value, VmPLnspecArgMacros.Tics(delay), 0, 0);

    public static LightInvoke Glow(int tag, int upper, int lower, int tics) =>
        new(LightKind.Glow, tag, upper, lower, VmPLnspecArgMacros.Tics(tics), 0);

    public static LightInvoke Flicker(int tag, int upper, int lower) =>
        new(LightKind.Flicker, tag, upper, lower, 0, 0);

    public static LightInvoke Strobe(int tag, int upper, int lower, int upTics, int lowTics) =>
        new(LightKind.Strobe, tag, upper, lower, VmPLnspecArgMacros.Tics(upTics), VmPLnspecArgMacros.Tics(lowTics));

    public static LightInvoke StrobeDoom(int tag, int upTics, int lowTics) =>
        new(LightKind.StrobeDoom, tag, VmPLnspecArgMacros.Tics(upTics), VmPLnspecArgMacros.Tics(lowTics), 0, 0);

    public static LightInvoke MinNeighbor(int tag) => new(LightKind.MinNeighbor, tag, 0, 0, 0, 0);

    public static int MaxNeighborLevel() => -1;

    public static LightInvoke MaxNeighbor(int tag) => new(LightKind.MaxNeighbor, tag, MaxNeighborLevel(), 0, 0, 0);

    public static LightInvoke Stop(int tag) => new(LightKind.Stop, tag, 0, 0, 0, 0);

    public static bool AlwaysSucceeds(LightKind kind) => kind == LightKind.ForceLightning;

    public static int SignedLightDelta(LightKind kind, int arg1) =>
        kind == LightKind.LowerByValue ? LowerByValueDelta(arg1) : arg1;
}
