namespace HCDE.Playsim;

/// <summary><c>DLightFlash</c> from <c>a_lights.cpp</c> (194–237).</summary>
public static class VmALightsLightFlash
{
    public const int DefaultMaxTime = 64;
    public const int DefaultMinTime = 7;

    public static int NextCountWhenDark(int prValue, int minTime) => (prValue & minTime) + 1;

    public static int NextCountWhenBright(int prValue, int maxTime) => (prValue & maxTime) + 1;

    public static int ResolveLightWhenAtMax(int minLight) => minLight;

    public static int ResolveLightWhenNotAtMax(int maxLight) => maxLight;

    public static (int maxTime, int minTime) DefaultTiming() => (DefaultMaxTime, DefaultMinTime);
}
