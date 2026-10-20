namespace HCDE.Playsim;

/// <summary><c>EV_StartLightGlowing</c> from <c>a_lights.cpp</c> (740–766).</summary>
public static class VmALightsEvStartLightGlowing
{
    public static bool ShouldAbortNonPositiveTics(int tics) => tics <= 0;

    public static (int upper, int lower) NormalizeBounds(int upper, int lower) =>
        upper < lower ? (lower, upper) : (upper, lower);

    public static bool ShouldSkipSectorWithLightingData(bool hasLightingData) => hasLightingData;
}
