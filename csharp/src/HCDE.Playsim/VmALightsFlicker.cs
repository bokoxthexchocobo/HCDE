namespace HCDE.Playsim;

/// <summary><c>DFlicker</c> from <c>a_lights.cpp</c> (136–167).</summary>
public static class VmALightsFlicker
{
    public static bool ShouldDecrementCount(int count) => count > 0;

    public static int NextCountFromMax(int prValue) => (prValue & 7) + 1;

    public static int NextCountFromMin(int prValue) => (prValue & 31) + 1;

    public static int InitialCount(int prValue) => (prValue & 64) + 1;

    public static int SelectLightWhenAtMax(int maxLight) => maxLight;

    public static int SelectLightWhenNotAtMax(int minLight) => minLight;
}
