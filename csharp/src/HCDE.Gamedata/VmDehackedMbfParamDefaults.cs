namespace HCDE.Gamedata;

/// <summary>MBF code pointer emitter defaults from <c>d_dehacked.cpp</c> (786–935).</summary>
public static class VmDehackedMbfParamDefaults
{
    public static double MushroomVerticalRange(int misc1) =>
        misc1 != 0 ? VmDehackedNumericConversion.DehToDouble(misc1) : 4.0;

    public static double MushroomHorizontalRange(int misc2) =>
        misc2 != 0 ? VmDehackedNumericConversion.DehToDouble(misc2) : 0.5;

    public static float PlaySoundAttenuation(bool misc2None) => misc2None ? 0f : 1f;

    public static int DehSoundIndex(int misc1, bool required) =>
        Math.Max(0, misc1 - 1);
}
