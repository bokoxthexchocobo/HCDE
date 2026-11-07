namespace HCDE.Playsim;
/// <summary>
/// Startup status, episode and skill selection, and IWAD sprite renames
/// from d_main.cpp 2802-3083. RenameNerve starts at line 3096.
/// </summary>
public static class VmDMain1417Slice
{
    public const int BandStart = 2802;
    public const int BandEnd = 3083;
    public const int NerveStart = 3096;
    public const int Global = 0;
    public const int Sprites = 1;
    public const int Doom = 1;
    public const int Heretic = 2;
    public const int Hexen = 4;
    public const int Strife = 8;
    public const int Raven = 6;
    public const int DoomChex = 17;
    public sealed class Lump
    {
        public Lump(int space, int container, string name)
        {
            Space = space;
            Container = container;
            Name = name;
        }
        public int Space;
        public int Container;
        public string Name;
    }
    public sealed class State
    {
        public List<string> Args = new() { "hcde" };
        public bool Batch;
        public bool Dedicated;
        public bool Window;
        public bool Screen;
        public int GameType = Doom;
        public bool NoSpriteRename;
        public int Iwad;
        public int MaxIwad;
        public int DefaultSkill = -1;
        public int SetSkill = -1;
        public int? Skill;
        public List<int> SkillDefaults = new();
        public bool SetMap;
        public bool FromLauncher;
        public bool Autostart;
        public string StartMap = "";
        public List<string> Episodes = new();
        public List<string> Lines = new();
        public List<string> Phases = new();
        public List<string?> Shown = new();
        public List<string> Traces = new();
        public int Clears;
        public List<(string Who, int Advance)> Steps = new();
    }
    public static void StartupStatusLine(State state, string text, int colors = 0x3f, bool center = false)
    {
        _ = colors;
        _ = center;
        if (state.Batch)
            return;
        state.Lines.Add(text + "\n");
        if (state.Screen)
            state.Phases.Add(text);
    }
    public static void StartupPreVideoStatus(State state, string? text)
    {
        if (!state.Batch && !state.Dedicated)
            state.Shown.Add(text);
        state.Traces.Add("startup.prevideo " + (text ?? ""));
    }
    public static void StartupClearPreVideoStatus(State state) => state.Clears++;
    public static void StartupProgress(State state, int advance)
    {
        if (advance <= 0)
            return;
        if (state.Window)
            state.Steps.Add(("window", advance));
        if (state.Screen)
            state.Steps.Add(("screen", advance));
    }
    public static void CheckEpisodeCmd(State state)
    {
        bool picked = false;
        int episode = 0;
        string? value = Value(state, "-episode");
        if (value != null)
        {
            episode = Atoi(value) - 1;
            if (episode < 0 || episode >= state.Episodes.Count)
            {
                state.Lines.Add($"Invalid episode {value}\n");
                episode = 0;
            }
            else
                picked = true;
        }
        if (!picked && state.SetMap)
        {
            if (!state.FromLauncher || state.Episodes.Count == 0)
                return;
            string start = state.Episodes[0];
            if (start.Length == 0 || Same(start, state.StartMap))
                return;
            state.Lines.Add($"HCDE: startup map {state.StartMap} came from launcher default; using episode start map {start}.\n");
            state.StartMap = start;
            state.FromLauncher = false;
            return;
        }
        state.StartMap = state.Episodes[episode];
        state.SetMap = true;
        state.FromLauncher = false;
        if (picked)
            state.Autostart = true;
    }
    public static void CheckDefaultSkill(State state)
    {
        if (state.DefaultSkill >= 0)
            state.SkillDefaults.Add(state.DefaultSkill);
        if (state.SetSkill >= 0)
            state.Skill = state.SetSkill;
    }
    public static void NewFailure() => throw new InvalidOperationException("Failed to allocate memory from system heap");
    public static void RenameSprites(State state, IList<Lump> lumps, IList<string> remove)
    {
        string font = state.GameType == Strife ? "SBIGFONT" : (state.GameType & Raven) != 0 ? "HBIGFONT" : "DBIGFONT";
        string[] pairs = Pairs(state.GameType);
        bool minotaur = false;
        foreach (Lump lump in lumps)
        {
            if (lump.Space == Sprites && Head(lump.Name) == "MNTR" && At(lump.Name, 4) == 'Z')
            {
                minotaur = true;
                break;
            }
        }
        bool all = Present(state, "-oldsprites") || state.NoSpriteRename;
        foreach (Lump lump in lumps)
        {
            if (lump.Space == Sprites)
            {
                if (all || lump.Container == state.Iwad)
                {
                    lump.Name = Rename(lump.Name, pairs);
                    if (state.GameType == Hexen && FileName(lump.Name, "ARTIINVU"))
                        lump.Name = Patch(lump.Name, 4, "DEFN");
                }
                if (!minotaur && Head(lump.Name) == "MNTR")
                {
                    lump.Name = Shift(lump.Name, 4);
                    lump.Name = Shift(lump.Name, 6);
                }
                if ((state.GameType & DoomChex) == 0 && Head(lump.Name) == "BLOD")
                    lump.Name = "BLUD" + lump.Name[4..];
            }
            else if (lump.Space == Global)
            {
                if (lump.Container >= state.Iwad && lump.Container <= state.MaxIwad && remove.Contains(lump.Name))
                    lump.Name = "";
                else if (lump.Name == font)
                    lump.Name = "BIGFONT";
            }
        }
    }
    static string[] Pairs(int game) => game switch
    {
        Heretic => new[] { "HEADLICH" },
        Hexen => new[]
        {
            "BARLZBAR", "ARM1AR_1", "ARM2AR_2", "ARM3AR_3", "ARM4AR_4", "SUITZSUI", "TRE1ZTRE", "TRE2TRES",
            "CANDBCAN", "ROCKROKK", "WATRHWAT", "GIBSPOL5", "EGGMPRKM", "INVUDEFN",
        },
        Strife => new[]
        {
            "MISLSMIS", "ARM1ARM3", "ARM2ARM4", "PMAPSMAP", "TLMPTECH", "TRE1TRET", "BAR1BARC", "SHT2MPUF",
            "BARLBBAR", "TRCHTRHL", "SHRDSHAR", "BLSTMAUL", "LOGGLOGW", "VASEVAZE", "CNDLKNDL", "POT1MPOT", "SPIDSTLK",
        },
        _ => Array.Empty<string>(),
    };
    static string Rename(string name, string[] pairs)
    {
        if (name.Length < 4)
            return name;
        foreach (string pair in pairs)
        {
            if (name[..4] == pair[..4])
                name = pair[4..] + name[4..];
        }
        return name;
    }
    static string Patch(string name, int at, string text)
    {
        var chars = name.ToCharArray();
        for (int i = 0; i < text.Length && at + i < chars.Length; i++)
            chars[at + i] = text[i];
        return new string(chars);
    }
    static string Shift(string name, int index)
    {
        if (index >= name.Length)
            return name;
        char ch = name[index];
        if (ch < 'F' || ch > 'K')
            return name;
        var chars = name.ToCharArray();
        chars[index] = (char)(ch + ('U' - 'F'));
        return new string(chars);
    }
    static string Head(string name) => name.Length >= 4 ? name[..4] : name;
    static char At(string name, int index) => index < name.Length ? name[index] : '\0';
    static bool FileName(string name, string want)
    {
        for (int i = 0; i < 8; i++)
        {
            char left = i < name.Length ? name[i] : '\0';
            char right = i < want.Length ? want[i] : '\0';
            if (char.ToUpperInvariant(left) != char.ToUpperInvariant(right))
                return false;
            if (left == '\0' || right == '\0')
                return left == right;
        }
        return true;
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
        int index = Find(state, name);
        if (index > 0 && index < state.Args.Count - 1)
        {
            string next = state.Args[index + 1];
            if (next.Length > 0 && next[0] != '+' && next[0] != '-')
                return next;
        }
        return null;
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
}
