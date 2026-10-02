namespace HCDE.Playsim;

/// <summary>Native ACS <c>SetSectorTerrain</c> subset (stored names only).</summary>
internal static class SectorTerrain
{
    public const int Floor = 0;
    public const int Ceiling = 1;

    public static int ApplyTagged(AuthoritySimulation sim, int tag, string? terrainName, int position)
    {
        if (string.IsNullOrEmpty(terrainName))
            return 0;

        var count = 0;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag))
                continue;
            if (position == Ceiling)
                sector.CeilingTerrain = terrainName;
            else
                sector.FloorTerrain = terrainName;
            count++;
        }

        return count;
    }
}
