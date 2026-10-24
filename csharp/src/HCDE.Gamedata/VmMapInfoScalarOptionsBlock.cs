namespace HCDE.Gamedata;

/// <summary>MAPINFO scalar options from <c>g_mapinfo.cpp</c> (1567–1716).</summary>
public static class VmMapInfoScalarOptionsBlock
{
    public static int PackDefaultEnvironment(int high, int low) => (high << 8) | low;

    public static int ClampFogDensity(int value) => Math.Clamp(value, 0, 512) >> 1;

    public static bool TryParseNumericEnvironmentPair(int first, int? second, out int packed)
    {
        packed = first << 8;
        if (second is int s) packed |= s;
        return true;
    }
}
