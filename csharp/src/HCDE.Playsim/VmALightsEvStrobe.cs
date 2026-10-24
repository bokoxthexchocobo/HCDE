namespace HCDE.Playsim;

/// <summary><c>EV_StartLightStrobing</c> overloads from <c>a_lights.cpp</c> (570–596).</summary>
public static class VmALightsEvStrobe
{
    public static bool ShouldSkipSectorWithLightingData(bool hasLightingData) => hasLightingData;

    public static (int upper, int lower, int utics, int ltics) FourArgStrobe(int upper, int lower, int utics, int ltics) =>
        (upper, lower, utics, ltics);

    public static (int utics, int ltics, bool hexenStyle) TwoArgStrobe(int utics, int ltics) => (utics, ltics, false);
}
