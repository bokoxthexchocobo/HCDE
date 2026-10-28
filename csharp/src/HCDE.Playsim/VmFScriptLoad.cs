namespace HCDE.Playsim;

/// <summary>FraggleScript loader from <c>fragglescript/t_load.cpp</c> (60–311).</summary>
public static class VmFScriptLoad
{
    public const int RtScript = 0;
    public const int RtInfo = 1;
    public const int RtOther = 2;
    public const int MaptypeDoom = 0;
    public const int GameDoom = 0;
    public const int GameHeretic = 1;
    public const int FsExecute = 158;

    public sealed class LevelLoadContext
    {
        public string LevelName { get; set; } = "";
        public int Partime { get; set; }
        public string Music { get; set; } = "";
        public int MusicOrder { get; set; }
        public int SkyTexture1 { get; set; }
        public int SkyTexture2 { get; set; }
        public string ExitPic { get; set; } = "";
        public double Gravity { get; set; }
        public string NextMap { get; set; } = "";
        public string NextSecretMap { get; set; } = "";
        public int AirSupply { get; set; } = 100;
        public int MapType { get; set; }
        public bool TranslatorEmpty { get; set; } = true;
        public object? FraggleScriptThinker { get; set; }
        public List<object> SpawnedThings { get; } = new();
        public LineTranslationTable? Translator { get; set; }
        public Action InitSkyMap { get; init; } = () => { };
        public Func<string, bool> TryChangeFsMusic { get; init; } = _ => false;
        public Action<string> ChangeMusic { get; init; } = _ => { };
        public Func<string, int> GetTextureId { get; init; } = _ => 0;
        public Action<string> EmulateConsoleCmd { get; init; } = _ => { };
        public Func<object> CreateFraggleThinker { get; init; } = () => new object();
        public Action<object, string> SetThinkerScriptData { get; init; } = (_, _) => { };
        public Action<object> SetFraggleThinker { get; init; } = _ => { };
        public Func<string, int> CheckLumpByName { get; init; } = _ => -1;
        public Func<int, int> FileLength { get; init; } = _ => 0;
        public Action<int, byte[]> ReadFile { get; init; } = (_, _) => { };
        public Func<string, Exception?> FatalError { get; init; } = msg => new InvalidOperationException(msg);
    }

    public sealed class LineTranslation
    {
        public int Special { get; set; }
    }

    public sealed class LineTranslationTable
    {
        public List<LineTranslation> SimpleLineTranslations { get; } = new();
    }

    public sealed class MapLumpReader
    {
        public Func<int> LumpSize { get; init; } = () => 0;
        public Action<int, byte[]> ReadLump { get; init; } = (_, _) => { };
    }

    public static void ParseInfoCmd(
        ref int readType,
        ref bool hasScripts,
        ref int drownFlag,
        ref bool ignoreInfo,
        string line,
        LevelLoadContext level,
        System.Text.StringBuilder scriptSrc)
    {
        var chars = line.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
            if (chars[i] < 32) chars[i] = ' ';
        line = new string(chars);

        if (readType != RtScript)
        {
            var end = line.Length - 1;
            while (end >= 0 && line[end] == ' ') end--;
            var trimmed = line[..(end + 1)];
            var start = 0;
            while (start < trimmed.Length && trimmed[start] == ' ') start++;
            line = trimmed[start..];
            if (line.Length == 0) return;
            if ((line.Length >= 2 && line[0] == '/' && line[1] == '/') || line[0] == '#' || line[0] == ';')
                return;
        }

        if (line.Length > 0 && line[0] == '[')
        {
            var section = line[1..];
            if (section.StartsWith("scripts", StringComparison.OrdinalIgnoreCase))
            {
                readType = RtScript;
                hasScripts = true;
            }
            else if (section.StartsWith("level info", StringComparison.OrdinalIgnoreCase))
                readType = RtInfo;
            return;
        }

        if (readType == RtScript)
        {
            scriptSrc.Append(line);
            scriptSrc.Append('\n');
            return;
        }

        if (readType != RtInfo) return;

        var key = ReadToken(line);
        if (key.Equals("levelname", StringComparison.OrdinalIgnoreCase))
        {
            var beg = line.IndexOf('=') + 1;
            while (beg < line.Length && line[beg] <= ' ') beg++;
            var comm = line.IndexOf("//", beg, StringComparison.Ordinal);
            var value = comm >= 0 ? line[beg..comm] : line[beg..];
            level.LevelName = value.TrimEnd();
        }
        else if (key.Equals("partime", StringComparison.OrdinalIgnoreCase) && TryReadEqualsNumber(line, out var n))
            level.Partime = n;
        else if (key.Equals("music", StringComparison.OrdinalIgnoreCase) && TryReadEqualsString(line, out var music))
        {
            if (!level.TryChangeFsMusic(music))
                level.ChangeMusic(level.Music);
        }
        else if (key.Equals("skyname", StringComparison.OrdinalIgnoreCase) && TryReadEqualsString(line, out var sky))
        {
            var id = level.GetTextureId(sky);
            level.SkyTexture1 = id;
            level.SkyTexture2 = id;
            level.InitSkyMap();
        }
        else if (key.Equals("interpic", StringComparison.OrdinalIgnoreCase) && TryReadEqualsString(line, out var pic))
            level.ExitPic = pic;
        else if (key.Equals("gravity", StringComparison.OrdinalIgnoreCase) && TryReadEqualsNumber(line, out var grav))
            level.Gravity = grav * 8.0;
        else if (key.Equals("nextlevel", StringComparison.OrdinalIgnoreCase) && TryReadEqualsString(line, out var next))
            level.NextMap = next;
        else if (key.Equals("nextsecret", StringComparison.OrdinalIgnoreCase) && TryReadEqualsString(line, out var secret))
            level.NextSecretMap = secret;
        else if (key.Equals("drown", StringComparison.OrdinalIgnoreCase) && TryReadEqualsNumber(line, out var drown))
            drownFlag = drown != 0 ? 1 : 0;
        else if (key.Equals("consolecmd", StringComparison.OrdinalIgnoreCase))
        {
            var beg = line.IndexOf('=') + 1;
            while (beg < line.Length && line[beg] < ' ') beg++;
            var comm = line.IndexOf("//", beg, StringComparison.Ordinal);
            var cmd = comm >= 0 ? line[beg..comm] : line[beg..];
            level.EmulateConsoleCmd(cmd.TrimEnd());
        }
        else if (key.Equals("ignore", StringComparison.OrdinalIgnoreCase) && TryReadEqualsNumber(line, out var ign))
            ignoreInfo = ign != 0;
    }

    public static bool ParseInfo(LevelLoadContext level, MapLumpReader map, ref bool ignoreInfo)
    {
        ignoreInfo = false;
        var scriptSrc = new System.Text.StringBuilder();
        var hasScripts = false;
        var drownFlag = -1;
        var readType = RtOther;
        var fsglobal = false;
        byte[] lump;
        var lumpsize = map.LumpSize();
        if (lumpsize == 0)
        {
            var lumpnum = level.CheckLumpByName("FSGLOBAL");
            if (lumpnum < 0) return false;
            lumpsize = level.FileLength(lumpnum);
            if (lumpsize == 0) return false;
            fsglobal = true;
            lump = new byte[lumpsize + 3];
            level.ReadFile(lumpnum, lump);
        }
        else
        {
            lump = new byte[lumpsize + 3];
            map.ReadLump(0, lump);
        }

        lump[lumpsize] = (byte)'\n';
        lump[lumpsize + 1] = (byte)'\r';
        lumpsize += 2;
        var text = System.Text.Encoding.Latin1.GetString(lump, 0, lumpsize);
        var startOfLine = 0;
        for (var rover = 0; rover < text.Length; rover++)
        {
            if (text[rover] != '\n') continue;
            var line = text[startOfLine..rover];
            if (!ignoreInfo)
                ParseInfoCmd(ref readType, ref hasScripts, ref drownFlag, ref ignoreInfo, line, level, scriptSrc);
            startOfLine = rover + 1;
        }

        if (!hasScripts) return false;

        if (level.FraggleScriptThinker != null)
            throw level.FatalError("Only one FraggleThinker is allowed to exist at a time.\nCheck your code.")!;

        var th = level.CreateFraggleThinker();
        level.SetThinkerScriptData(th, scriptSrc.ToString());
        level.SetFraggleThinker(th);
        level.FraggleScriptThinker = th;

        if (drownFlag == -1)
            drownFlag = level.MapType != MaptypeDoom || fsglobal ? 1 : 0;
        if (drownFlag == 0)
            level.AirSupply = 0;
        return true;
    }

    public static void LoadScripts(LevelLoadContext level, MapLumpReader map, int gameType)
    {
        var ignore = false;
        var hasScripts = ParseInfo(level, map, ref ignore);
        if ((gameType == GameDoom || gameType == GameHeretic)
            && level.TranslatorEmpty
            && level.MapType == MaptypeDoom
            && level.Translator != null
            && level.Translator.SimpleLineTranslations.Count > 272)
        {
            var idx272 = 272 - 2 * (hasScripts ? 1 : 0);
            var idx270 = 270;
            if (level.Translator.SimpleLineTranslations[idx272].Special == FsExecute)
            {
                (level.Translator.SimpleLineTranslations[idx270], level.Translator.SimpleLineTranslations[idx272]) =
                    (level.Translator.SimpleLineTranslations[idx272], level.Translator.SimpleLineTranslations[idx270]);
            }
        }
    }

    public static void AddSpawnedThing(LevelLoadContext level, object actor)
    {
        if (level.FraggleScriptThinker != null)
            level.SpawnedThings.Add(actor);
    }

    static string ReadToken(string line)
    {
        var i = 0;
        while (i < line.Length && !char.IsWhiteSpace(line[i])) i++;
        return line[..i];
    }

    static bool TryReadEqualsNumber(string line, out int number)
    {
        number = 0;
        var eq = line.IndexOf('=');
        if (eq < 0) return false;
        var tail = line[(eq + 1)..].Trim();
        var parts = tail.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 && int.TryParse(parts[0], out number);
    }

    static bool TryReadEqualsString(string line, out string value)
    {
        value = "";
        var eq = line.IndexOf('=');
        if (eq < 0) return false;
        value = line[(eq + 1)..].Trim();
        return value.Length > 0;
    }
}
