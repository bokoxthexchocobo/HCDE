namespace HCDE.Playsim;

/// <summary>
/// Steam library folders and installed-game paths from <c>d_steam.cpp</c> (<b>1–164</b>).
/// Chunk <b>1203</b>. The install root and directory probes are host results.
/// A parse error becomes an empty game list, matching the recoverable catch.
/// </summary>
public static class VmSteamPaths1203Slice
{
    public static readonly (string Path, int AppId)[] Apps =
    {
        ("Doom 2/base", 2300),
        ("Final Doom/base", 2290),
        ("Heretic Shadow of the Serpent Riders/base", 2390),
        ("Hexen/base", 2360),
        ("Hexen Deathkings of the Dark Citadel/base", 2370),
        ("Ultimate Doom/base", 2280),
        ("Ultimate Doom/base/doom2", 2280),
        ("Ultimate Doom/base/tnt", 2280),
        ("Ultimate Doom/base/plutonia", 2280),
        ("DOOM 3 BFG Edition/base/wads", 208200),
        ("Strife", 317040),
        ("Ultimate Doom/rerelease/DOOM_Data/StreamingAssets", 2280),
        ("Ultimate Doom/rerelease", 2280),
        ("Doom 2/rerelease/DOOM II_Data/StreamingAssets", 2300),
        ("Doom 2/finaldoombase", 2300),
        ("Master Levels of Doom/doom2", 9160),
        ("Heretic + Hexen/dos/base/heretic", 3286930),
        ("Heretic + Hexen/dos/base/hexen", 3286930),
        ("Heretic + Hexen/dos/base/hexendk", 3286930),
    };

    public sealed class ParseError : Exception
    {
        public ParseError(string message) : base(message) { Detail = message; }
        public string Detail;
    }

    public static List<string> ParseRegistry(string? text)
    {
        var result = new List<string>();
        if (text == null)
            return result;
        var sc = new Scanner(text);
        sc.MustGetString();
        sc.MustGet('{');
        while (sc.GetToken() && sc.Kind != '}')
        {
            sc.MustBeString();
            sc.MustGet('{');
            while (sc.GetToken() && sc.Kind != '}')
            {
                sc.MustBeString();
                if (sc.Text.Equals("path", StringComparison.OrdinalIgnoreCase))
                {
                    sc.MustGetString();
                    result.Add(sc.Text + "/steamapps/common");
                    FindEndBlock(sc);
                    break;
                }

                if (sc.Check('{'))
                    FindEndBlock(sc);
                else
                    sc.MustGetString();
            }
        }

        return result;
    }

    public static List<string> GetGamePaths(string? steamPath, string? registry, Func<string, bool> exists)
    {
        var result = new List<string>();
        if (string.IsNullOrEmpty(steamPath) || !exists(steamPath))
            return result;
        List<string> folders;
        try
        {
            folders = ParseRegistry(registry);
        }
        catch (ParseError)
        {
            return result;
        }

        for (int i = 0; i < folders.Count; i++)
            folders[i] = folders[i].Replace('\\', '/') + "/";
        folders.Add(steamPath + "/steamapps/common/");
        foreach (var app in Apps)
        {
            foreach (string folder in folders)
            {
                string candidate = folder + app.Path;
                if (exists(candidate))
                    result.Add(candidate);
            }
        }

        return result;
    }

    static void FindEndBlock(Scanner sc)
    {
        int depth = 1;
        do
        {
            if (sc.Check('}'))
                --depth;
            else if (sc.Check('{'))
                ++depth;
            else
                sc.MustGetAny();
        }
        while (depth != 0);
    }

    sealed class Scanner
    {
        readonly string Source;
        int Index;
        public int Kind;
        public string Text = "";
        bool Held;

        public Scanner(string source) => Source = source;

        public bool GetToken()
        {
            if (Held)
            {
                Held = false;
                return Kind != 0;
            }

            Skip();
            if (Index >= Source.Length)
            {
                Kind = 0;
                Text = "";
                return false;
            }

            if (Source[Index] == '"')
            {
                var built = new System.Text.StringBuilder();
                for (Index++; Index < Source.Length && Source[Index] != '"'; Index++)
                {
                    if (Source[Index] == '\\' && Index + 1 < Source.Length && Source[Index + 1] == '"')
                    {
                        built.Append('"');
                        Index++;
                    }
                    else
                        built.Append(Source[Index]);
                }

                Text = built.ToString();
                if (Index < Source.Length)
                    Index++;
                Kind = 's';
                return true;
            }

            char token = Source[Index++];
            Text = token.ToString();
            Kind = token;
            return true;
        }

        public void MustGetString()
        {
            if (!GetToken() || Kind != 's')
                throw new ParseError("Expected string.");
        }

        public void MustBeString()
        {
            if (Kind != 's')
                throw new ParseError("Expected string.");
        }

        public void MustGet(char token)
        {
            if (!GetToken() || Kind != token)
                throw new ParseError($"Expected '{token}'.");
        }

        public void MustGetAny()
        {
            if (!GetToken())
                throw new ParseError("Missing token.");
        }

        public bool Check(char token)
        {
            if (!GetToken())
                return false;
            if (Kind == token)
                return true;
            Held = true;
            return false;
        }

        void Skip()
        {
            while (Index < Source.Length)
            {
                char c = Source[Index];
                if (c is ' ' or '\t' or '\r' or '\n' or '\v' or '\f')
                {
                    Index++;
                    continue;
                }

                if (c == '/' && Index + 1 < Source.Length && (Source[Index + 1] == '/' || Source[Index + 1] == '*'))
                {
                    if (Source[Index + 1] == '*')
                    {
                        Index += 2;
                        while (Index + 1 < Source.Length && !(Source[Index] == '*' && Source[Index + 1] == '/'))
                            Index++;
                        Index = Index + 1 < Source.Length ? Index + 2 : Source.Length;
                    }
                    else
                    {
                        while (Index < Source.Length && Source[Index] != '\n')
                            Index++;
                    }

                    continue;
                }

                return;
            }
        }
    }
}
