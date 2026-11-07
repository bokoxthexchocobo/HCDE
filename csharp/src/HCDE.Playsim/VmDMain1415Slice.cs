using System.Globalization;
namespace HCDE.Playsim;
/// <summary>
/// Patch args, exec lists, command-line files, GAMEINFO, and the MAPxx flag
/// from d_main.cpp 2251-2480. D_DoomInit starts at line 2488.
/// </summary>
public static class VmDMain1415Slice
{
    public const int BandStart = 2251;
    public const int BandEnd = 2480;
    public const int DoomInitStart = 2488;
    public const int Identifier = 257;
    public const int Mapxx = 1;
    public const int Global = 0;
    public const int DefaultStartup = 0;
    public const int DoomStartup = 1;
    public const int HereticStartup = 2;
    public const int HexenStartup = 3;
    public const int StrifeStartup = 4;
    public static readonly string[] Folders =
    {
        "flats/", "textures/", "hires/", "sprites/", "voxels/", "colormaps/", "acs/", "maps/",
        "voices/", "patches/", "graphics/", "sounds/", "music/", "materials/", "models/", "fonts/", "brightmaps/",
    };
    public static readonly string[] Prefixes =
    {
        "mapinfo", "zmapinfo", "umapinfo", "emapinfo", "gameinfo", "sndinfo", "sndseq", "sbarinfo",
        "menudef", "gldefs", "animdefs", "decorate", "zscript", "iwadinfo", "complvl", "terrain", "maps/",
    };
    public static readonly string[] Blocked = { "*.bat", "*.exe", "__macosx/*", "*/__macosx/*", "*~" };
    public sealed class ScriptError : Exception
    {
        public ScriptError(string message) : base(message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
        }
    }
    public readonly record struct Wad(string Name, bool Optional);
    public readonly record struct Lump(string Short, string Full, int Container, int Namespace, string Text, string Resource);
    public readonly record struct Filter(string[] Folders, string[] Prefixes, string[] Blocked);
    public sealed class State
    {
        public string Config = "GameConfig";
        public bool ShowLauncher;
        public bool NoSpriteRename;
        public int Flags;
        public int Iwad;
        public int LoadLights = -1;
        public int LoadBrightmaps = -1;
        public int LoadWidescreen = -1;
        public int StartupType;
        public int Fg;
        public int Bk;
        public int ScriptSize = -1;
        public string StartupName = "";
        public string Song = "";
        public string Steam = "";
        public string? Script;
        public string? UsedResource;
        public List<string> FileArgs = new();
        public List<string> OptArgs = new();
        public List<Wad> Wads = new();
        public List<Lump> CheckLumps = new();
        public List<Lump> MapLumps = new();
        public List<(int Kind, string Text)> Tokens = new();
        public List<(string Name, string Ext, bool LookFirst, string Config)> Searches = new();
        public List<(string Path, int Lump)> DehLoads = new();
        public List<(string Name, string Ext, string Config, bool Optional)> WildCalls = new();
        public List<(string Path, bool Found, bool Directory)> Looks = new();
        public List<(string Name, int Position, bool Check, bool Optional)> Adds = new();
        public List<string> Colors = new();
        public Filter? UsedFilter;
        public Func<string, string, bool, string, string?> Search = (_, _, _, _) => null;
        public Func<string, string?, string?> ParseExec = (_, exec) => exec;
        public Action<List<Wad>, string, string, string, bool> AddWild = (wads, name, _, _, optional) => wads.Add(new Wad(name, optional));
        public Func<string, bool> Accept = _ => true;
        public Func<string, (bool Found, bool Directory)> Exists = _ => (false, false);
        public Func<List<Wad>, bool> Init = _ => false;
        public Func<string, int> Color = text => text.Length;
    }
    public static bool ConsiderPatches(State state, IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            var found = Seek(state, arg, ".deh");
            if (found == null)
                found = Seek(state, arg, ".bex");
            if (found != null)
                state.DehLoads.Add((found, 0));
        }
        return args.Count > 0;
    }
    public static string? MultiExec(State state, IReadOnlyList<string> list, string? exec)
    {
        foreach (var item in list)
            exec = state.ParseExec(item, exec);
        return exec;
    }
    public static void GetCmdLineFiles(State state, bool optional)
    {
        var args = optional ? state.OptArgs : state.FileArgs;
        int start = optional ? 0 : state.Wads.Count;
        for (int i = start; i < args.Count; i++)
        {
            int before = state.Wads.Count;
            state.WildCalls.Add((args[i], ".wad", state.Config, optional));
            state.AddWild(state.Wads, args[i], ".wad", state.Config, optional);
            for (int j = before; j < state.Wads.Count; j++)
                state.Wads[j] = new Wad(state.Wads[j].Name, false);
        }
    }
    public static string ParseGameInfo(State state, List<Wad> pwads, string fn, string data)
    {
        state.Script = "GAMEINFO";
        state.ScriptSize = data.Length;
        var scan = new Scan(state.Tokens);
        int cut = fn.LastIndexOf('/');
        if (cut < 0)
            cut = fn.LastIndexOf(':');
        string iwad = "";
        int pos = 0;
        while (scan.Get())
        {
            scan.MustBe(Identifier);
            string key = scan.Text;
            scan.Must(61);
            if (Same(key, "IWAD") && !state.ShowLauncher)
            {
                scan.MustString();
                iwad = scan.Text;
            }
            else if (Same(key, "LOAD"))
            {
                do
                {
                    scan.MustString();
                    string check = cut < 0 ? scan.Text : fn[..cut] + "/" + scan.Text;
                    var look = state.Exists(check);
                    state.Looks.Add((check, look.Found, look.Directory));
                    string chosen = look.Found ? check : scan.Text;
                    bool added = state.Accept(chosen);
                    state.Adds.Add((chosen, pos, true, false));
                    if (added)
                    {
                        pwads.Add(new Wad(chosen, false));
                        pos++;
                    }
                }
                while (scan.Check(44));
            }
            else if (Same(key, "NOSPRITERENAME"))
            {
                scan.MustString();
                state.NoSpriteRename = Same(scan.Text, "true");
            }
            else if (Same(key, "STARTUPTITLE"))
            {
                scan.MustString();
                state.StartupName = scan.Text;
            }
            else if (Same(key, "STARTUPCOLORS"))
            {
                scan.MustString();
                state.Colors.Add(scan.Text);
                state.Fg = state.Color(scan.Text);
                scan.MustName(",");
                scan.MustString();
                state.Colors.Add(scan.Text);
                state.Bk = state.Color(scan.Text);
            }
            else if (Same(key, "STARTUPTYPE"))
            {
                scan.MustString();
                state.StartupType = Same(scan.Text, "DOOM") ? DoomStartup
                    : Same(scan.Text, "HERETIC") ? HereticStartup
                    : Same(scan.Text, "HEXEN") ? HexenStartup
                    : Same(scan.Text, "STRIFE") ? StrifeStartup
                    : DefaultStartup;
            }
            else if (Same(key, "STARTUPSONG"))
            {
                scan.MustString();
                state.Song = scan.Text;
            }
            else if (Same(key, "LOADLIGHTS"))
                state.LoadLights = scan.Number() != 0 ? 1 : 0;
            else if (Same(key, "LOADBRIGHTMAPS"))
                state.LoadBrightmaps = scan.Number() != 0 ? 1 : 0;
            else if (Same(key, "LOADWIDESCREEN"))
                state.LoadWidescreen = scan.Number() != 0 ? 1 : 0;
            else if (Same(key, "DISCORDAPPID"))
                scan.MustString();
            else if (Same(key, "STEAMAPPID"))
            {
                scan.MustString();
                state.Steam = scan.Text;
            }
            else
            {
                do
                    scan.MustAny();
                while (scan.Check(44));
            }
        }
        return iwad;
    }
    public static string CheckGameInfo(State state, List<Wad> pwads)
    {
        state.UsedFilter = new Filter((string[])Folders.Clone(), (string[])Prefixes.Clone(), (string[])Blocked.Clone());
        if (!state.Init(pwads))
            return "";
        int num = FindShort(state.CheckLumps, "GAMEINFO", Global, -1);
        if (num < 0)
            return "";
        var lump = state.CheckLumps[num];
        state.UsedResource = lump.Resource;
        return ParseGameInfo(state, pwads, lump.Resource, lump.Text);
    }
    public static void SetMapxxFlag(State state)
    {
        int name = FindShort(state.MapLumps, "MAP01", Global, state.Iwad);
        int wad = FindFull(state.MapLumps, "maps/map01.wad", state.Iwad);
        int map = FindFull(state.MapLumps, "maps/map01.map", state.Iwad);
        if (name >= 0 || wad >= 0 || map >= 0)
            state.Flags |= Mapxx;
    }
    static string? Seek(State state, string arg, string ext)
    {
        state.Searches.Add((arg, ext, false, state.Config));
        return state.Search(arg, ext, false, state.Config);
    }
    static int FindShort(List<Lump> lumps, string name, int space, int container)
    {
        if (name.Length > 8 && (name.Contains('/') || name.Contains('.')))
            return -1;
        for (int i = lumps.Count - 1; i >= 0; i--)
        {
            if (lumps[i].Namespace != space || !Same(lumps[i].Short, name))
                continue;
            if (container < 0 || lumps[i].Container == container)
                return i;
        }
        return -1;
    }
    static int FindFull(List<Lump> lumps, string name, int container)
    {
        if (container < 0 && name.StartsWith('/'))
            name = name[1..];
        for (int i = lumps.Count - 1; i >= 0; i--)
        {
            if (!Same(lumps[i].Full, name))
                continue;
            if (container < 0 || lumps[i].Container == container)
                return i;
        }
        return -1;
    }
    static bool Same(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    sealed class Scan
    {
        readonly List<(int Kind, string Text)> tokens;
        int index;
        public int Kind;
        public string Text = "";
        public Scan(List<(int Kind, string Text)> tokens) => this.tokens = tokens;
        public bool Get()
        {
            if (index >= tokens.Count)
                return false;
            (Kind, Text) = tokens[index++];
            return true;
        }
        public void Unget()
        {
            if (index > 0)
                index--;
        }
        public void MustAny()
        {
            if (!Get())
                throw new ScriptError("Missing token (unexpected end of file).");
        }
        public void MustBe(int kind)
        {
            if (Kind != kind)
                throw new ScriptError("Expected " + Name(kind, null) + " but got " + Name(Kind, Text) + " instead.");
        }
        public void Must(int kind)
        {
            MustAny();
            MustBe(kind);
        }
        public void MustString()
        {
            if (!Get())
                throw new ScriptError("Missing string (unexpected end of file).");
        }
        public void MustName(string name)
        {
            MustString();
            if (!Same(Text, name))
                throw new ScriptError("Expected '" + name + "', got '" + Text + "'.");
        }
        public bool Check(int kind)
        {
            if (!Get())
                return false;
            if (Kind == kind)
                return true;
            Unget();
            return false;
        }
        public int Number()
        {
            if (!Get())
                throw new ScriptError("Missing integer (unexpected end of file).");
            if (Text == "MAXINT")
                return int.MaxValue;
            if (!int.TryParse(Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                throw new ScriptError("SC_GetNumber: Bad numeric constant \"" + Text + "\".");
            return number;
        }
        static string Name(int token, string? text)
        {
            if (token > 32 && token < 256)
                return "'" + (char)token + "'";
            if (token == Identifier)
                return text == null ? "identifier" : "identifier '" + text + "'";
            return "Unknown(" + token + ")";
        }
    }
}
