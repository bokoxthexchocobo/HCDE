namespace HCDE.Playsim;

/// <summary>Light thinker constructors from <c>a_lights.cpp</c> (98–236).</summary>
public static class VmALightsConstruct
{
    public static (int max, int min) FireFlickerFromSector(int sectorLight, int minSurrounding) =>
        (sectorLight, VmALightsFireFlicker.ClampLight(minSurrounding + 16));

    public static (int max, int min) LightFlashFromSector(int sectorLight, int minSurrounding) =>
        (sectorLight, minSurrounding);

    public static (int max, int min) LightFlashFromArgs(int min, int max) =>
        (VmALightsFireFlicker.ClampLight(max), VmALightsFireFlicker.ClampLight(min));

    public static (int max, int min) StrobeHexen(int upper, int lower) =>
        (VmALightsFireFlicker.ClampLight(upper), VmALightsFireFlicker.ClampLight(lower));

    public static (int max, int min, int bright, int dark) StrobeDoom(int sectorLight, int minSurrounding, int utics, int ltics) =>
        (sectorLight, VmALightsStrobe.ResolveMinLightWhenEqual(sectorLight, minSurrounding), utics, ltics);

    public static int FlickerConstructCount(int prValue) => VmALightsFlicker.InitialCount(prValue);

    public static int LightFlashInitialCount(int prValue, int maxTime) =>
        VmALightsLightFlash.NextCountWhenBright(prValue, maxTime);
}
