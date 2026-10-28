namespace HCDE.Gamedata;

/// <summary>Thing patch group clamping from <c>PatchThing</c> in <c>d_dehacked.cpp</c>.</summary>
public static class VmDehackedThingGroupLimits
{
    public static int ClampInfightingGroup(long value)
    {
        if (value < 0) return 0;
        return (int)value;
    }

    public static int ClampProjectileGroup(long value) => value < 0 ? -1 : (int)value;

    public static int ClampSplashGroup(long value) => value < 0 ? 0 : (int)value;
}
