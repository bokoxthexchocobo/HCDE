namespace HCDE.Playsim;

/// <summary>Applies native ACS terrain overrides through managed terrain definitions.</summary>
internal static class SectorTerrain
{
    public const int Floor = 0;
    public const int Ceiling = 1;

    public static int ApplyTagged(AuthoritySimulation sim, int tag, string? terrainName, int position)
    {
        if (position != Floor && position != Ceiling)
            return 0;

        var resolvedName = string.IsNullOrEmpty(terrainName)
            || terrainName.Equals("None", StringComparison.OrdinalIgnoreCase)
            || terrainName.Equals("Null", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : sim.Level.FindTerrainDefinition(terrainName)?.Name ?? string.Empty;

        var count = 0;
        foreach (var sector in sim.Level.Sectors)
        {
            if (!sector.MatchesTag(tag))
                continue;
            if (position == Ceiling)
                sector.CeilingTerrain = resolvedName;
            else
                sector.FloorTerrain = resolvedName;
            count++;
        }

        return count;
    }
}
