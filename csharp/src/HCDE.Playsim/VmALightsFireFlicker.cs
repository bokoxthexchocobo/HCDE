namespace HCDE.Playsim;

/// <summary><c>DFireFlicker</c> from <c>a_lights.cpp</c> (74–112).</summary>
public static class VmALightsFireFlicker
{
    public const int DefaultCountdown = 4;
    public const int RandomAmountShift = 4;

    public static int NextRandomAmount(int prValue) => (prValue & 3) << RandomAmountShift;

    public static int ResolveLightLevel(int sectorLight, int amount, int minLight) =>
        sectorLight - amount < minLight ? minLight : sectorLight - amount;

    public static int ComputeMinSurroundingPlus16(int minSurrounding) => minSurrounding + 16;

    public static int ClampLight(int level) => Math.Clamp(level, 0, 255);

    public static int ResetCountdown() => DefaultCountdown;
}
