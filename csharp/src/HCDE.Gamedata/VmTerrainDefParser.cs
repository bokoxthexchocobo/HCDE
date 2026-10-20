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
        while (i < tokens.Count)
        {
            if (tokens[i] == "{") { i++; continue; }
            if (tokens[i] == "}") break;
            var key = tokens[i++];
            if (key.Equals("splash", StringComparison.OrdinalIgnoreCase))
            {
                if (i >= tokens.Count) { errors.Add("Expected splash name."); return false; }
                def.SplashIndex = db.FindSplash(tokens[i++]);
            }
            else if (key.Equals("damageamount", StringComparison.OrdinalIgnoreCase))
                def.DamageAmount = ReadInt(tokens, ref i, errors);
            else if (key.Equals("damagetype", StringComparison.OrdinalIgnoreCase))
                def.DamageType = ParseDamageType(tokens, ref i, errors);
            else if (key.Equals("damagetimemask", StringComparison.OrdinalIgnoreCase))
                def.DamageTimeMask = ReadInt(tokens, ref i, errors);
            else if (key.Equals("footclip", StringComparison.OrdinalIgnoreCase))
                def.FootClip = ReadDouble(tokens, ref i, errors);
            else if (key.Equals("stepvolume", StringComparison.OrdinalIgnoreCase))
                def.StepVolume = (float)ReadDouble(tokens, ref i, errors);
            else if (key.Equals("liquid", StringComparison.OrdinalIgnoreCase))
                def.IsLiquid = true;
            else if (key.Equals("allowprotection", StringComparison.OrdinalIgnoreCase))
                def.AllowProtection = true;
            else if (key.Equals("damageonland", StringComparison.OrdinalIgnoreCase))
                def.DamageOnLand = true;
            else if (key.Equals("friction", StringComparison.OrdinalIgnoreCase))
            {
                var input = ReadDouble(tokens, ref i, errors);
                VmTerrainFrictionParser.ApplyFrictionInput(input, out var friction, out var move);
                def.Friction = friction;
                def.MoveFactor = move;
            }
            else if (key.Equals("stepsound", StringComparison.OrdinalIgnoreCase))
                def.StepSound = ReadString(tokens, ref i, errors);
        }

        return errors.Count == 0;
    }

    private static string ParseDamageType(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        var raw = ReadString(tokens, ref i, errors);
        return raw.Equals("Lava", StringComparison.OrdinalIgnoreCase) ? "Fire" : raw;
    }

    private static string ReadString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected string."); return ""; }
        return tokens[i++];
    }

    private static int ReadInt(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !int.TryParse(tokens[i], out var n)) { errors.Add("Expected int."); return 0; }
        i++;
        return n;
    }

    private static double ReadDouble(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f)) { errors.Add("Expected float."); return 0; }
        i++;
        return f;
    }
}
