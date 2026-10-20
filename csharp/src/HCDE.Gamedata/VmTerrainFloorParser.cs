namespace HCDE.Gamedata;

/// <summary><c>ParseFloor</c> / <c>ParseDefault</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainFloorParser
{
    public static bool TryParseFloorStatement(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors)
    {
        var optional = i < tokens.Count && tokens[i].Equals("optional", StringComparison.OrdinalIgnoreCase);
        if (optional) i++;
        if (i >= tokens.Count) { errors.Add("Expected flat name."); return false; }
        var flat = tokens[i++];
        if (i >= tokens.Count) { errors.Add("Expected terrain name."); return false; }
        var terrainToken = tokens[i++];
        if (terrainToken.Equals("Null", StringComparison.OrdinalIgnoreCase) || terrainToken.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            db.FloorTerrains[flat] = -1;
            return true;
        }

        var terrain = db.FindTerrain(terrainToken);
        if (terrain < 0 && !optional)
            errors.Add($"Unknown terrain {terrainToken}");
        db.FloorTerrains[flat] = terrain < 0 ? 0 : terrain;
        return errors.Count == 0;
    }

    public static bool TryParseDefaultTerrain(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected terrain name."); return false; }
        var name = tokens[i++];
        var terrain = db.FindTerrain(name);
        if (terrain < 0)
        {
            errors.Add($"Unknown terrain {name}");
            terrain = 0;
        }

        db.DefaultTerrainIndex = terrain;
        return true;
    }
}
