namespace HCDE.Gamedata;

/// <summary><c>ParseOuter</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainDocumentParser
{
    public static bool TryParseDocument(string text, VmTerrainDatabase db, ITerrainGameFilter games, List<string> errors,
        ITerrainFlatCatalog? flats = null)
    {
        var tokens = VmLockDefTokenize.Tokenize(text);
        var i = 0;
        var ifSkip = false;
        var braceDepth = 0;
        while (i < tokens.Count)
        {
            var word = tokens[i++];
            if (ifSkip)
            {
                if (braceDepth > 0)
                {
                    if (word == "}") braceDepth--;
                    else if (word == "{") braceDepth++;
                    continue;
                }

                if (word.Equals("endif", StringComparison.OrdinalIgnoreCase))
                {
                    ifSkip = false;
                    continue;
                }

                if (word == "{") braceDepth++;
                else if (word == "}") errors.Add("Too many left braces ('}')");
                continue;
            }

            if (word.Equals("splash", StringComparison.OrdinalIgnoreCase))
            {
                if (!TryParseSplashStatement(tokens, ref i, db, errors)) return false;
            }
            else if (word.Equals("terrain", StringComparison.OrdinalIgnoreCase))
            {
                if (!VmTerrainDefParser.TryParseTerrainStatementFrom(tokens, ref i, db, errors)) return false;
            }
            else if (word.Equals("floor", StringComparison.OrdinalIgnoreCase))
            {
                if (!VmTerrainFloorParser.TryParseFloorStatement(tokens, ref i, db, errors, flats)) return false;
            }
            else if (word.Equals("defaultterrain", StringComparison.OrdinalIgnoreCase))
            {
                if (!VmTerrainFloorParser.TryParseDefaultTerrain(tokens, ref i, db, errors)) return false;
            }
                        else if (word.StartsWith("if", StringComparison.OrdinalIgnoreCase) && word.Length > 2)
            {
                ifSkip = !games.IsActiveGame(word[2..]);
            }
            else if (word.Equals("endif", StringComparison.OrdinalIgnoreCase))
            {
                // no-op
            }
            else
            {
                errors.Add($"Unknown TERRAIN keyword '{word}'.");
                return false;
            }
        }

        return errors.Count == 0;
    }

    private static bool TryParseSplashStatement(IReadOnlyList<string> tokens, ref int i, VmTerrainDatabase db, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected splash name."); return false; }
        var name = tokens[i++];
        var modify = i < tokens.Count && tokens[i].Equals("modify", StringComparison.OrdinalIgnoreCase);
        if (modify) i++;
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' after splash header."); return false; }
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
        return VmTerrainSplashParser.TryParseSplash(name, modify, body, db, errors);
    }

}

public interface ITerrainGameFilter
{
    bool IsActiveGame(string gameSuffix);
}
