namespace HCDE.Gamedata;

/// <summary><c>SetSplashDefaults</c> / <c>ParseSplash</c> from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainSplashParser
{
    public static void SetSplashDefaults(VmSplashDef splash)
    {
        splash.SmallSplashSound = "";
        splash.NormalSplashSound = "";
        splash.SmallSplashClass = "";
        splash.SplashBaseClass = "";
        splash.SplashChunkClass = "";
        splash.ChunkXVelShift = 8;
        splash.ChunkYVelShift = 8;
        splash.ChunkZVelShift = 8;
        splash.ChunkBaseZVel = 1;
        splash.SmallSplashClip = 12;
        splash.NoAlert = false;
    }

    public static bool TryParseSplash(string name, bool modify, string body, VmTerrainDatabase db, List<string> errors)
    {
        var index = db.FindSplash(name);
        var isNew = false;
        VmSplashDef splash;
        if (index < 0)
        {
            splash = new VmSplashDef { Name = name };
            SetSplashDefaults(splash);
            db.Splashes.Add(splash);
            isNew = true;
        }
        else
        {
            splash = db.Splashes[index];
        }

        if (modify && !isNew)
            SetSplashDefaults(splash);

        var tokens = VmLockDefTokenize.Tokenize(body);
        var i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i] == "{") { i++; continue; }
            if (tokens[i] == "}") break;
            var key = tokens[i++];
            if (key.Equals("smallsound", StringComparison.OrdinalIgnoreCase))
                splash.SmallSplashSound = ReadString(tokens, ref i, errors);
            else if (key.Equals("sound", StringComparison.OrdinalIgnoreCase))
                splash.NormalSplashSound = ReadString(tokens, ref i, errors);
            else if (key.Equals("smallclip", StringComparison.OrdinalIgnoreCase))
                splash.SmallSplashClip = ReadDouble(tokens, ref i, errors);
            else if (key.Equals("smallclass", StringComparison.OrdinalIgnoreCase))
                splash.SmallSplashClass = ReadString(tokens, ref i, errors);
            else if (key.Equals("baseclass", StringComparison.OrdinalIgnoreCase))
                splash.SplashBaseClass = ReadString(tokens, ref i, errors);
            else if (key.Equals("chunkclass", StringComparison.OrdinalIgnoreCase))
                splash.SplashChunkClass = ReadString(tokens, ref i, errors);
            else if (key.Equals("chunkxvelshift", StringComparison.OrdinalIgnoreCase))
                splash.ChunkXVelShift = ReadInt(tokens, ref i, errors);
            else if (key.Equals("chunkyvelshift", StringComparison.OrdinalIgnoreCase))
                splash.ChunkYVelShift = ReadInt(tokens, ref i, errors);
            else if (key.Equals("chunkzvelshift", StringComparison.OrdinalIgnoreCase))
                splash.ChunkZVelShift = ReadInt(tokens, ref i, errors);
            else if (key.Equals("chunkbasezvel", StringComparison.OrdinalIgnoreCase))
                splash.ChunkBaseZVel = ReadDouble(tokens, ref i, errors);
            else if (key.Equals("noalert", StringComparison.OrdinalIgnoreCase))
                splash.NoAlert = ReadBool(tokens, ref i, errors);
        }

        return errors.Count == 0;
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

    private static bool ReadBool(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected bool."); return false; }
        var tok = tokens[i++];
        return tok is "1" or "true" or "True";
    }
}
