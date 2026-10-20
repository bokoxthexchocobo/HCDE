namespace HCDE.Gamedata;

/// <summary><c>ReadDecals</c>, <c>ParseDecal</c>, <c>ParseDecalGroup</c> from <c>decallib.cpp</c>.</summary>
public static class VmDecalLibDocumentParser
{
    public static bool TryParseDocument(string text, VmDecalDatabase db, List<string> errors)
    {
        var tokens = VmLockDefTokenize.Tokenize(text);
        var i = 0;
        while (i < tokens.Count)
        {
            if (!TryParseTopLevel(tokens, ref i, db, errors))
                return false;
        }

        return errors.Count == 0;
    }

    private static bool TryParseTopLevel(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) return true;
        var key = tokens[i++];
        if (key.Equals("decal", StringComparison.OrdinalIgnoreCase))
            return TryParseDecalHeader(tokens, ref i, db, errors);
        if (key.Equals("decalgroup", StringComparison.OrdinalIgnoreCase))
            return TryParseDecalGroup(tokens, ref i, db, errors);

        errors.Add($"Unknown decaldef keyword '{key}'.");
        return false;
    }

    public static bool TryParseDecalHeader(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected decal name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count) { errors.Add("Expected decal id or '{'."); return false; }
        ushort id = 0;
        if (!tokens[i].Equals("{", StringComparison.Ordinal))
        {
            if (!VmDecalLibParser.TryParseDecalId(tokens[i], out id, out var numeric) || !numeric)
            {
                errors.Add("Decal ID must be between 1 and 65535.");
                return false;
            }

            i++;
        }

        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after decal header."); return false; }
        i++;
        var def = new VmDecalDefinition { Name = name, Id = id };
        if (!VmDecalLibParser.TryParseDecalBody(tokens, ref i, def.Template, errors)) return false;
        if (i < tokens.Count && tokens[i] == "}") i++;
        db.Decals.Add(def);
        return true;
    }

    public static bool TryParseDecalGroup(IReadOnlyList<string> tokens, ref int i, VmDecalDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected group name."); return false; }
        var name = tokens[i++];
        if (i >= tokens.Count) { errors.Add("Expected group id."); return false; }
        if (!VmDecalLibParser.TryParseDecalId(tokens[i], out var id, out var numeric) || !numeric)
        {
            errors.Add("Decal group ID must be numeric.");
            return false;
        }

        i++;
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after decalgroup header."); return false; }
        i++;
        var group = new VmDecalGroupDefinition { Name = name, Id = id };
        while (i < tokens.Count && tokens[i] != "}")
        {
            if (i + 1 >= tokens.Count) { errors.Add("Expected decal name and weight."); return false; }
            var decalName = tokens[i++];
            if (!int.TryParse(tokens[i++], out var weight)) { errors.Add("Expected numeric weight."); return false; }
            if (!db.TryGetDecal(decalName, out _))
                errors.Add($"{decalName} has not been defined");
            group.Entries.Add((decalName, weight));
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        db.Groups.Add(group);
        return errors.Count == 0;
    }
}
