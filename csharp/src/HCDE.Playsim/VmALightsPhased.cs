namespace HCDE.Playsim;

/// <summary><c>DPhased</c> from <c>a_lights.cpp</c> (471–544).</summary>
public static class VmALightsPhased
{
    public const int Steps = 12;
    public const int PhaseWrap = 63;

    public static int TickLightLevel(int phase, int baseLevel)
    {
        if (phase < Steps)
            return ((255 - baseLevel) * phase) / Steps + baseLevel;
        if (phase < 2 * Steps)
            return ((255 - baseLevel) * (2 * Steps - phase - 1) / Steps) + baseLevel;
        return baseLevel;
    }

    public static int NextPhase(int phase) => phase == 0 ? PhaseWrap : phase - 1;

    public static int ComputeChildPhase(int numSteps, int index) =>
        ((numSteps - index - 1) * 64) / numSteps;

    public static int ResolveBaseLevel(int sectorLight, int fallbackLight) =>
        sectorLight != 0 ? sectorLight : fallbackLight;

    public static (int baseLevel, int phase) Construct(int baselevel, int phase) => (baselevel, phase);

    public static bool ShouldClearSpecialAfterPhase() => true;
}
