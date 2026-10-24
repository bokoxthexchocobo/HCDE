namespace HCDE.Playsim;

/// <summary><c>DStrobe</c> from <c>a_lights.cpp</c> (264–316).</summary>
public static class VmALightsStrobe
{
    public static int NextBrightCount(int brightTime) => brightTime;

    public static int NextDarkCount(int darkTime) => darkTime;

    public static int ResolveMinLightWhenEqual(int maxLight, int minLight) =>
        minLight == maxLight ? 0 : minLight;

    public static int HexenStyleInitialCount() => 1;

    public static int DoomStyleInitialCount(int prValue) => (prValue & 7) + 1;

    public static bool IsAtMinLight(int sectorLight, int minLight) => sectorLight == minLight;
}
