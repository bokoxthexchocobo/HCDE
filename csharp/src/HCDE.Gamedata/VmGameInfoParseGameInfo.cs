namespace HCDE.Gamedata;

/// <summary><c>FMapInfoParser::ParseGameInfo</c> body extensions from <c>gi.cpp</c>.</summary>
public static class VmGameInfoParseGameInfo
{
    public static bool TryParseBlock(IReadOnlyList<string> tokens, ref int i, VmGameInfoDocument doc, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{'."); return false; }
        i++;
        while (i < tokens.Count && tokens[i] != "}")
        {
            var key = tokens[i++];
            if (key.Equals("intro", StringComparison.OrdinalIgnoreCase))
            {
                if (i < tokens.Count) i++;
                continue;
            }

            if (i >= tokens.Count || tokens[i] != "=") { errors.Add($"Expected '=' after '{key}'."); return false; }
            i++;
            if (!VmGameInfoParser.TryParseKey(key, tokens, ref i, doc, errors))
                return false;
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        return errors.Count == 0;
    }
}
