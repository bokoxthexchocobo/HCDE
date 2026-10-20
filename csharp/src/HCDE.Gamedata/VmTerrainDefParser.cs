namespace HCDE.Gamedata;

/// <summary><c>ParseTerrain</c> / <c>ParseDamage</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainDefParser
{
    public static bool TryParseTerrainKeyword(string text, VmTerrainDatabase db, List<string> errors)
    {
        var tokens = VmLockDefTokenize.Tokenize(text);
        var i = 0;
        if (i >= tokens.Count || !tokens[i].Equals("terrain", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Expected terrain keyword.");
            return false;
        }

        i++;
        return TryParseTerrainStatement(tokens, ref i, db, errors);
    }

    public static bool TryParseTerrainStatementFrom(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors)
        => TryParseTerrainStatement(tokens, ref i, db, errors);

    private static bool TryParseTerrainStatement(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected terrain name."); return false; }
        var name = tokens[i++];
        var modify = i < tokens.Count && tokens[i].Equals("modify", StringComparison.OrdinalIgnoreCase);
        if (modify) i++;
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after terrain header."); return false; }
        i++;
        var bodyStart = i;
        var depth = 1;
        while (i < tokens.Count && depth > 0)
        {
            if (tokens[i] == "{") depth++;
            else if (tokens[i] == "}") depth--;
            i++;
        }

        var body = string.Join(' ', tokens.Skip(bodyStart).Take(i - bodyStart - 1));
        return TryParseTerrain(name, modify, body, db, errors);
    }

    public static bool TryParseTerrain(string name, bool modify, string body, VmTerrainDatabase db, List<string> errors)
    {
        var index = db.FindTerrain(name);
        if (index < 0)
        {
            db.Terrains.Add(new VmTerrainDef { Name = name, SplashIndex = -1, DamageTimeMask = 31 });
            index = db.Terrains.Count - 1;
        }
        else if (modify)
        {
            var saved = db.Terrains[index].Name;
            db.Terrains[index] = new VmTerrainDef { Name = saved, SplashIndex = -1, DamageTimeMask = 31 };
        }

        var def = db.Terrains[index];
        var tokens = VmLockDefTokenize.Tokenize(body);
        var i = 0;
        var ctx = new VmTerrainGenericContext
        {
            Database = db,
            Target = def,
            OwnerType = "terrain",
            OwnerName = name,
        };
        VmTerrainGenericParser.ParseBody(tokens, ref i, VmTerrainDefDescriptors.Keywords,
            VmTerrainDefDescriptors.Parser, ctx, errors);

        return errors.Count == 0;
    }
}
