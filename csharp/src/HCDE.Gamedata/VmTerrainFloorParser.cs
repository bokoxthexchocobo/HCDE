namespace HCDE.Gamedata;

/// <summary><c>ParseFloor</c> / <c>ParseDefault</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainFloorParser
{
    public static bool TryParseFloorStatement(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors,
        ITerrainFlatCatalog? flats = null)
    {
        var optional = i < tokens.Count && tokens[i].Equals("optional", StringComparison.OrdinalIgnoreCase);
        if (optional) i++;
        if (i >= tokens.Count) { errors.Add("Expected flat name."); return false; }
        var flat = tokens[i++];
        var flatExists = true;
        var flatIndex = -1;
        if (flats is not null)
        {
            flats.TryResolveFlat(flat, out flatIndex, out flatExists);
            if (!flatExists)
            {
                if (!optional)
                    errors.Add($"Unknown flat {flat}");
                if (i >= tokens.Count) { errors.Add("Expected terrain name."); return false; }
                i++;
                return errors.Count == 0;
            }
        }

        if (i >= tokens.Count) { errors.Add("Expected terrain name."); return false; }
        var terrainToken = tokens[i++];
        if (terrainToken.Equals("Null", StringComparison.OrdinalIgnoreCase) || terrainToken.Equals("None", StringComparison.OrdinalIgnoreCase))
        {
            db.FloorTerrains[flat] = -1;
            if (flatIndex >= 0)
                db.FloorTerrainByTextureIndex[flatIndex] = -1;
            return true;
        }

        var terrain = db.FindTerrain(terrainToken);
        if (terrain < 0)
            errors.Add($"Unknown terrain {terrainToken}");
        var stored = terrain < 0 ? 0 : terrain;
        db.FloorTerrains[flat] = stored;
        if (flatIndex >= 0)
            db.FloorTerrainByTextureIndex[flatIndex] = stored;
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
