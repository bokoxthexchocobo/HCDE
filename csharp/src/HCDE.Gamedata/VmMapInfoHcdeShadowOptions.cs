namespace HCDE.Gamedata;

/// <summary>HCDE MAPINFO shadow overrides from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoHcdeShadowOptions
{
    public static int ClampShadowProfile(int profile)
    {
        if (profile < -1) return -1;
        if (profile > 7) return 7;
        return profile;
    }

    public static int NormalizeShadowQualityCap(int qualityCap)
    {
        if (qualityCap < 0) return -1;
        int[] tiers = [128, 256, 512, 1024, 2048, 4096, 8192];
        var best = tiers[0];
        var bestDist = Math.Abs(qualityCap - best);
        foreach (var tier in tiers)
        {
            var dist = Math.Abs(qualityCap - tier);
            if (dist < bestDist)
            {
                best = tier;
                bestDist = dist;
            }
        }

        return best;
    }

    public static int ClampShadowMaxLights(int maxLights)
    {
        if (maxLights < 0) return -1;
        return Math.Min(maxLights, 1024);
    }
}
