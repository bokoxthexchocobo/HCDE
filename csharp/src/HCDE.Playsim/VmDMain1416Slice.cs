using System.Globalization;
namespace HCDE.Playsim;
/// <summary>
/// Startup init, autoload files, and command-line checks from d_main.cpp 2488-2800.
/// StartupStatusLine starts at line 2802.
/// </summary>
public static class VmDMain1416Slice
{
    public const int BandStart = 2488;
    public const int BandEnd = 2800;
    public const int StatusStart = 2802;
    public const int Startup = 6;
    public const int Shareware = 2;
    public const int Mapxx = 1;
    public const int Hexen = 4;
    public const int WeaponsStay = 1 << 2;
    public const int NoMonsters = 1 << 12;
    public const int MonstersRespawn = 1 << 13;
    public const int ItemsRespawn = 1 << 14;
    public const int FastMonsters = 1 << 15;
    public const int NoCoopWeapons = 1 << 21;
    public const int NoPlayerClip = 1 << 0;
    public const int ShareKeys = 1 << 1;
    public const int LocalItems = 1 << 2;
    public const int RememberWeapon = 1 << 6;
    public sealed class State
    {
        public List<string> Args = new() { "hcde" };
        public int Flags;
        public int GameType = 1;
        public bool Batch;
        public bool Dedicated;
        public bool DisableAutoload;
        public bool Unix;
        public int LoadLights = -1;
        public int LoadBrightmaps = -1;
        public int LoadWidescreen = -1;
        public bool AutoloadLights;
        public bool AutoloadBrightmaps;
        public bool AutoloadWidescreen;
        public string ShareDir = "";
        public string ProgDir = "";
        public string Config = "GameConfig";
        public string LumpFilter = "";
        public int GameState;
        public int Responses;
        public int Defaults;
        public List<(string Name, string? Extension)> Gathers = new();
        public List<(string Name, string? Extension, bool LookFirst, string Config)> Searches = new();
        public List<(string Path, bool Check, int Position, string Config, bool Optional)> Files = new();
        public List<(string Path, string Pattern, string Config, bool Optional)> Folders = new();
        public List<(string Section, string Pattern, string Config, bool Optional)> Sections = new();
        public int Dmflags;
        public int Dmflags3;
        public int Deathmatch;
        public int Teamplay;
        public int SvGametype;
        public int Gametic;
        public uint Room;
        public string LevelName = "<none>";
        public bool Devparm;
        public string DevText = "";
        public bool SetMap;
        public bool FromLauncher;
        public string StartMap = "";
        public bool Autostart;
        public int SetSkill;
        public string StoredWarp = "";
        public float Turbo;
        public float Timelimit;
        public bool StartScreen;
        public List<string> Lines = new();
        public List<string> Traces = new();
        public List<string> Status = new();
        public Func<string, bool, string?>? FindFile;
        public Func<string, string?>? Resolve;
        public Func<string, bool>? MapData;
        public Func<string, string>? Nice;
        public Func<bool, string>? AppData;
    }
    public static void DoomInit(State state)
    {
        state.Responses++;
        state.Gathers.Add(("-deh", ".deh"));
        state.Gathers.Add(("-bex", ".bex"));
        state.Gathers.Add(("-exec", ".cfg"));
        state.Gathers.Add(("-playdemo", ".lmp"));
        state.Gathers.Add(("-file", null));
        state.Gathers.Add(("-optfile", null));
        state.GameState = Startup;
        if (!state.Batch)
            state.Lines.Add("M_LoadDefaults: Load system defaults.\n");
        state.Defaults++;
    }
    public static void AddAutoloadFiles(State state, string autoname)
    {
        state.LumpFilter = autoname + ".";
        if ((state.Flags & Shareware) == 0 && !Present(state, "-noextras"))
        {
            Extra(state, state.LoadLights, state.AutoloadLights, "-nolights", "lights.pk3");
            Extra(state, state.LoadBrightmaps, state.AutoloadBrightmaps, "-nobrightmaps", "brightmaps.pk3");
            Extra(state, state.LoadWidescreen, state.AutoloadWidescreen, "-nowidescreen", "game_widescreen_gfx.pk3");
        }
        bool joining = Present(state, "-join") || Present(state, "-dedicatedjoin") || Present(state, "-joindedicated");
        if ((state.Flags & Shareware) == 0 && !Present(state, "-noautoload") && !state.DisableAutoload && !Present(state, "-host") && !joining && !state.Dedicated)
        {
            state.Searches.Add(("zvox.wad", null, false, state.Config));
            string? wad = state.FindFile?.Invoke("zvox.wad", false);
            if (wad != null)
                state.Files.Add((wad, true, -1, state.Config, true));
            string file = state.Unix ? state.ShareDir : state.ProgDir;
            file += "skins";
            state.Folders.Add((file, "*.wad", state.Config, true));
            state.Folders.Add((file, "*.pk3", state.Config, true));
            if (state.Unix)
            {
                string home = state.AppData?.Invoke(true) ?? "";
                string skin = home + "/skins";
                file = state.Nice?.Invoke(skin) ?? skin;
                state.Folders.Add((file, "*.wad", state.Config, true));
                state.Folders.Add((file, "*.pk3", state.Config, true));
            }
            state.Sections.Add(("Global.Autoload", "*.wad", state.Config, true));
            int last = -1;
            int len;
            while ((len = Dot(state.LumpFilter, last + 1)) > 0)
            {
                state.Sections.Add((state.LumpFilter[..len] + ".Autoload", "*.wad", state.Config, true));
                last = len;
            }
        }
    }
    public static void CheckCmdLine(State state)
    {
        int flags = state.Dmflags;
        int flags3 = state.Dmflags3;
        bool joining = Present(state, "-join") || Present(state, "-dedicatedjoin") || Present(state, "-joindedicated");
        bool launching = Present(state, "-host") || joining || state.Dedicated;
        bool explicitMode = Present(state, "-altdeath") || Present(state, "-deathmatch") || Present(state, "-coop");
        bool hasType = false;
        int typeValue = -1;
        for (int i = 1; i < state.Args.Count; i++)
        {
            string arg = state.Args[i];
            if (arg.Length > 0 && arg[0] == '+' && Same(arg[1..], "sv_gametype"))
            {
                hasType = true;
                if (i + 1 < state.Args.Count)
                    typeValue = Atoi(state.Args[i + 1]);
                break;
            }
            if (arg.Length > 0 && arg[0] == '+' && Same(arg[1..], "set") && i + 1 < state.Args.Count && Same(state.Args[i + 1], "sv_gametype"))
            {
                hasType = true;
                if (i + 2 < state.Args.Count)
                    typeValue = Atoi(state.Args[i + 2]);
                break;
            }
        }
        bool invasion = hasType && typeValue == 4;
        if (!state.Batch)
            state.Lines.Add("Checking cmd-line parameters...\n");
        if (Present(state, "-nomonsters"))
            flags |= NoMonsters;
        if (Present(state, "-respawn"))
            flags |= MonstersRespawn;
        if (Present(state, "-fast"))
            flags |= FastMonsters;
        state.Devparm = Present(state, "-devparm");
        if (Present(state, "-altdeath"))
        {
            Death(state, 1, "cmdline-altdeath");
            Gametype(state, 1, "cmdline-altdeath");
            flags |= ItemsRespawn;
        }
        else if (Present(state, "-deathmatch"))
        {
            Death(state, 1, "cmdline-deathmatch");
            Gametype(state, 1, "cmdline-deathmatch");
            flags |= WeaponsStay | ItemsRespawn;
        }
        else if (Present(state, "-coop") && !invasion)
        {
            Death(state, 0, "cmdline-coop");
            Team(state, 0, "cmdline-coop");
            Gametype(state, 0, "cmdline-coop");
            flags |= NoCoopWeapons;
            flags3 |= NoPlayerClip | ShareKeys | RememberWeapon;
            if (state.GameType != Hexen)
                flags3 |= LocalItems;
        }
        else if (!explicitMode && !hasType && !launching)
        {
            Death(state, 0, "cmdline-solo-default");
            Team(state, 0, "cmdline-solo-default");
            Gametype(state, 0, "cmdline-solo-default");
        }
        state.Dmflags = flags;
        state.Dmflags3 = flags3;
        state.SetMap = false;
        state.FromLauncher = false;
        state.StartMap = state.GameType != Hexen ? ((state.Flags & Mapxx) != 0 ? "MAP01" : "E1M1") : "&wt@01";
        state.Autostart = state.StoredWarp.Length > 0;
        state.SetSkill = -1;
        string? skill = Value(state, "-skill");
        if (skill != null)
        {
            state.SetSkill = skill[0] - '1';
            state.Autostart = true;
        }
        int warp = Find(state, "-warp");
        if (warp != 0 && warp < state.Args.Count - 1)
        {
            int episode;
            int map;
            if ((state.Flags & Mapxx) != 0)
            {
                episode = 1;
                map = Atoi(state.Args[warp + 1]);
            }
            else
            {
                episode = Atoi(state.Args[warp + 1]);
                map = warp < state.Args.Count - 2 ? Atoi(state.Args[warp + 2]) : 10;
                if (map < 1 || map > 9)
                {
                    map = episode;
                    episode = 1;
                }
            }
            state.StartMap = MapName(state, episode, map);
            state.Autostart = state.SetMap = true;
        }
        string mapValue = Take(state, "+map");
        if (mapValue.Length > 0)
        {
            bool launcher = Same(mapValue, "MAP01") || Same(mapValue, "E1M1");
            string? compat = state.Resolve?.Invoke(mapValue);
            if (compat != null && (state.MapData?.Invoke(compat) ?? false))
            {
                state.Lines.Add($"HCDE: remapped startup map {mapValue} -> {compat} for active mod compatibility.\n");
                mapValue = compat;
            }
            if (!(state.MapData?.Invoke(mapValue) ?? false))
                state.Lines.Add($"Can't find map {mapValue}\n");
            else
            {
                state.StartMap = mapValue;
                state.Autostart = state.SetMap = true;
                state.FromLauncher = launcher && compat == null;
            }
        }
        if (state.Devparm)
            state.Lines.Add(state.DevText + "\n");
        string? turbo = Value(state, "-turbo");
        if (turbo != null)
        {
            double amt = Atof(turbo);
            state.Lines.Add(string.Format(CultureInfo.InvariantCulture, "turbo scale: {0:0}%\n", amt));
            state.Turbo = (float)amt;
        }
        string? timer = Value(state, "-timer");
        if (timer != null)
        {
            double time = Atof(timer);
            state.Lines.Add($"Levels will end after {time.ToString("G", CultureInfo.InvariantCulture)} minute{(time > 1 ? "s" : "")}.\n");
            state.Timelimit = (float)time;
        }
        if (Value(state, "-avg") != null)
        {
            state.Lines.Add("Austin Virtual Gaming: Levels will end after 20 minutes\n");
            state.Timelimit = 20f;
        }
        if (!state.StartScreen)
            return;
        if (state.Deathmatch != 0)
            state.Status.Add("DeathMatch...");
        if ((state.Dmflags & NoMonsters) != 0)
            state.Status.Add("No Monsters...");
        if ((state.Dmflags & MonstersRespawn) != 0)
            state.Status.Add("Respawning...");
        if (state.Autostart)
            state.Status.Add($"Warp to map {state.StartMap}, Skill {state.SetSkill + 1} ");
    }
    static void Extra(State state, int mode, bool enabled, string block, string file)
    {
        if ((mode == 1 || (mode != 0 && enabled)) && !Present(state, block))
        {
            state.Searches.Add((file, null, true, state.Config));
            string? path = state.FindFile?.Invoke(file, true);
            if (path != null)
                state.Files.Add((path, true, -1, state.Config, true));
        }
    }
    static void Death(State state, int value, string reason)
    {
        int old = state.Deathmatch;
        if (old == value)
            return;
        state.Traces.Add($"deathmatch {old} -> {value} reason={reason} gametic={state.Gametic}");
        state.Deathmatch = value;
    }
    static void Team(State state, int value, string reason)
    {
        int old = state.Teamplay;
        if (old == value)
            return;
        state.Traces.Add($"teamplay {old} -> {value} reason={reason} gametic={state.Gametic}");
        state.Teamplay = value;
    }
    static void Gametype(State state, int value, string reason)
    {
        int old = state.SvGametype;
        if (old == value)
            return;
        state.Traces.Add($"sv_gametype {old} -> {value} reason={reason} gametic={state.Gametic} room={state.Room} map={state.LevelName}");
        state.SvGametype = value;
    }
    static string MapName(State state, int episode, int level)
    {
        if ((state.Flags & Mapxx) != 0)
            return string.Format(CultureInfo.InvariantCulture, "MAP{0:00}", level);
        return string.Format(CultureInfo.InvariantCulture, "E{0}M{1}", episode, level);
    }
    static bool Present(State state, string name) => Find(state, name) != 0;
    static bool Same(string left, string right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);
    static int Find(State state, string name)
    {
        for (int i = 1; i < state.Args.Count; i++)
        {
            if (string.Equals(state.Args[i], name, StringComparison.OrdinalIgnoreCase))
                return i;
        }
        return 0;
    }
    static string? Value(State state, string name)
    {
        int i = Find(state, name);
        if (i > 0 && i < state.Args.Count - 1)
        {
            string next = state.Args[i + 1];
            if (next.Length > 0 && next[0] != '+' && next[0] != '-')
                return next;
        }
        return null;
    }
    static string Take(State state, string name)
    {
        int i = Find(state, name);
        if (i > 0 && i < state.Args.Count)
        {
            if (i < state.Args.Count - 1 && (state.Args[i + 1].Length == 0 || (state.Args[i + 1][0] != '+' && state.Args[i + 1][0] != '-')))
            {
                string taken = state.Args[i + 1];
                state.Args.RemoveAt(i);
                state.Args.RemoveAt(i);
                return taken;
            }
            state.Args.RemoveAt(i);
        }
        return "";
    }
    static int Dot(string text, int start)
    {
        if (start > 0 && text.Length <= start)
            return -1;
        if (start < 0)
            start = 0;
        return text.IndexOf('.', start);
    }
    static int Atoi(string text)
    {
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int n = 0;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            n = n * 10 + (text[i] - '0');
            i++;
        }
        return sign * n;
    }
    static double Atof(string text)
    {
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i]))
            i++;
        int start = i;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            i++;
        bool digit = false;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
        {
            digit = true;
            i++;
        }
        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            {
                digit = true;
                i++;
            }
        }
        if (!digit)
            return 0;
        return double.Parse(text[start..i], CultureInfo.InvariantCulture);
    }
}
