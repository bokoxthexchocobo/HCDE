namespace HCDE.Gamedata;

/// <summary><c>S_AddSNDINFO</c> include and game-filter preprocessing from <c>s_advsound.cpp</c>.</summary>
public interface ISndInfoGameFilter
{
    bool IsActiveGame(string gameSuffix);
}

public static class VmSndInfoPreprocessor
{
    public static string ExpandIncludes(string text, Func<string, string?> includeLoader)
    {
        var tokens = Tokenize(text);
        var output = new List<string>();
        for (var i = 0; i < tokens.Count;)
        {
            if (tokens[i].Equals("$include", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count)
            {
                var path = tokens[i + 1];
                var included = includeLoader(path);
                if (included is not null)
                    output.AddRange(Tokenize(included));
                i += 2;
                continue;
            }

            output.Add(tokens[i++]);
        }

        return string.Join(' ', output);
    }

    public static List<string> FilterGameBlocks(IReadOnlyList<string> tokens, ISndInfoGameFilter games)
    {
        var output = new List<string>();
        var skip = false;
        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.StartsWith("$if", StringComparison.OrdinalIgnoreCase) && token.Length > 3)
            {
                skip = !games.IsActiveGame(token[3..]);
                continue;
            }

            if (token.Equals("$endif", StringComparison.OrdinalIgnoreCase))
            {
                skip = false;
                continue;
            }

            if (!skip)
                output.Add(token);
        }

        return output;
    }

    private static List<string> Tokenize(string text) => VmLockDefTokenize.Tokenize(text);
}
