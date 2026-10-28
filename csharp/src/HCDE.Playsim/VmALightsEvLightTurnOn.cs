namespace HCDE.Playsim;

/// <summary><c>EV_LightTurnOn</c> from <c>a_lights.cpp</c> (635–669).</summary>
public static class VmALightsEvLightTurnOn
{
    public const int CompatLight = 1 << 0;

    public static bool ShouldSearchNeighborBrightness(int bright) => bright < 0;

    public static int MaxNeighborLight(IEnumerable<int> neighborLights, int currentBright)
    {
        foreach (var level in neighborLights)
        {
            if (level > currentBright)
                currentBright = level;
        }

        return currentBright;
    }

    public static int ApplyCompatLightFlag(int compatFlags, int tbright) =>
        (compatFlags & CompatLight) != 0 ? tbright : tbright;
}
