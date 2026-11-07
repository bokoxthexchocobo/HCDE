namespace HCDE.Playsim;
/// <summary>
/// The first phase of D_InitGame from d_main.cpp 3856-4018.
/// Startup type selection starts at line 4020. The function closes at line 4499.
/// </summary>
public static class VmDMain1421Slice
{
    public const int BandStart = 3856;
    public const int BandEnd = 4018;
    public const int FunctionEnd = 4499;
    public const int NextPhase = 4020;
    public const int Raven = 6;
    public const int DoomChex = 17;
    public const int DoomStrifeChex = 25;
    public const int Any = 0;
    public const string MissingIwad = "Cannot find a game IWAD (doom.wad, heretic.wad, etc).\nPlease place a valid game IWAD in the engine directory or configure your search paths.";
    public const string NoFiles = "FileSystem: no files found";
    public static readonly string?[] GameNames = { null, "Doom", "Heretic", null, "Hexen", null, null, null, "Strife", null, null, null, null, null, null, null, "Chex" };
    public sealed class Fatal(string message) : Exception(message);
    public sealed class Exec
    {
        public List<string> Files = new();
        public List<string> Plus = new();
    }
    public sealed class State
    {
        public bool HasIwad = true;
        public string? IwadName = "doom.wad";
        public string? Autoname = "doom";
        public int GameType = 1;
        public int Flags;
        public bool NoKeyboardCheats;
        public string? ConfigName = "doom";
        public string? DeleteLumps = "";
        public bool VmDebug;
        public string? DebugValue;
        public bool Jit = true;
        public bool JitAot = true;
        public bool DapDebugging;
        public bool Restart;
        public bool Dedicated;
        public bool Batch;
        public bool ScreenNull;
        public int ScreenSizeInits;
        public int ScreenInits;
        public List<string> Guards = new();
        public string? SaveFolder;
        public int CopiedType;
        public int CopiedFlags;
        public bool CopiedCheats;
        public string? CopiedConfig;
        public string? RngValue;
        public int RngSeed;
        public int StaticRngSeed;
        public bool UseStaticRng;
        public int MadeSeed = 1;
        public int SrandSeed = int.MinValue;
        public int Clears;
        public int CallbackDisables;
        public string? SetupConfig;
        public string? AutoloadName;
        public bool NoAutoexec;
        public List<string> Autoexec = new();
        public List<string> GatheredExec = new();
        public List<string> Plus = new();
        public Exec? Exec;
        public List<string> PullinWads = new();
        public int Pullins;
        public List<string> Lines = new();
        public List<string> Status = new();
        public List<string> Filters = new();
        public string LumpFilter = "doom.id.doom";
        public int Reserved;
        public List<string> AllWads = new();
        public List<string> Pwads = new();
        public bool AllowDuplicates;
        public int? EntryOverride;
        public int Postprocess;
        public string? SpriteLumps;
        public bool ClearedWads;
        public bool Shrunk;
        public int Mapxx;
        public int Eternity;
        public int Edf;
        public int DefCvars;
        public int Palette;
        public int SoundTrace;
        public int SoundInit;
        public int TextureGuess;
        public int MaxProgress;
        public int PerShaderProgress = -1;
        public bool NoRun;
        public bool Join;
        public bool DedicatedJoin;
        public bool JoinDedicated;
        public bool Host;
        public bool NoStartScreen;
        public bool VideoInitialized = true;
        public bool FirstShaderCompiled = true;
    }
    public static void Boot(State s)
    {
        if (!s.HasIwad) throw new Fatal(MissingIwad);
        s.Guards.Add("entities");
        s.Guards.Add("server-init");
        s.Guards.Add("iwad:" + (s.IwadName ?? ""));
        s.DapDebugging = s.VmDebug;
        if (s.DebugValue != null || s.DapDebugging)
        {
            s.DapDebugging = true;
            s.Jit = false;
            s.JitAot = false;
        }
        if (!s.Restart)
        {
            if (!s.Dedicated)
            {
                s.Guards.Add("startup dummy framebuffer");
                s.ScreenSizeInits++;
                s.ScreenInits++;
            }
            else s.ScreenNull = true;
        }
        s.SaveFolder = s.Autoname ?? throw new NullReferenceException();
        s.CopiedType = s.GameType;
        s.CopiedFlags = s.Flags;
        s.CopiedCheats = s.NoKeyboardCheats;
        s.CopiedConfig = s.ConfigName ?? throw new NullReferenceException();
        if (s.RngValue != null)
        {
            s.RngSeed = s.StaticRngSeed = Atoi(s.RngValue);
            s.UseStaticRng = true;
            if (!s.Batch) s.Lines.Add($"D_DoomInit: Static RNGseed {s.RngSeed} set.\n");
        }
        else
        {
            s.RngSeed = s.MadeSeed;
            s.UseStaticRng = false;
        }
        s.SrandSeed = s.RngSeed;
        s.Clears++;
        s.CallbackDisables++;
        s.SetupConfig = s.CopiedConfig;
        s.AutoloadName = s.Autoname;
        var exec = Multi(s.NoAutoexec ? new List<string>() : s.Autoexec, null);
        exec = Multi(s.GatheredExec, exec);
        if (s.Plus.Count > 0)
        {
            exec ??= new Exec();
            exec.Plus.AddRange(s.Plus);
        }
        s.Exec = exec;
        if (exec != null)
        {
            s.Pullins++;
            s.AllWads.AddRange(s.PullinWads);
        }
        if (!s.Batch) s.Lines.Add("W_Init: Init WADfiles.\n");
        s.Status.Add("Scanning and mounting WAD/PK3 archives...");
        AddBlanket(s);
        string? typeName = s.GameType >= 0 && s.GameType < GameNames.Length ? GameNames[s.GameType] : null;
        if (typeName == null) throw new NullReferenceException();
        s.Filters.Add("game-" + typeName);
        s.Filters.Add(s.LumpFilter);
        int at = s.LumpFilter.IndexOf("doom.id.doom", StringComparison.Ordinal);
        if (at >= 0)
        {
            string tail = s.LumpFilter.Length <= 12 ? "" : s.LumpFilter[12..];
            s.Filters.Add("doom.doom" + tail);
        }
        s.Reserved++;
        foreach (string pwad in s.Pwads) s.AllWads.Add(pwad);
        for (int i = 0; i < s.Pwads.Count; i++) s.Pwads[i] = "";
        if (!s.AllowDuplicates)
        {
            for (int i = 0; i < s.AllWads.Count; i++)
            {
                for (int j = i + 1; j < s.AllWads.Count; j++)
                {
                    if (s.AllWads[i] == s.AllWads[j]) s.AllWads.RemoveAt(j--);
                }
            }
        }
        int entries = s.EntryOverride ?? s.AllWads.Count;
        if (entries == 0) throw new Fatal(NoFiles);
        s.Postprocess++;
        s.SpriteLumps = s.DeleteLumps;
        s.Status.Add("Parsing loaded game resources...");
        s.AllWads.Clear();
        s.ClearedWads = true;
        s.Shrunk = true;
        s.Mapxx++;
        s.Eternity++;
        s.Edf++;
        s.DefCvars++;
        s.Palette++;
        s.SoundTrace++;
        if (!s.Batch)
            s.Lines.Add(s.Dedicated ? "S_Init: Setting up server sound tables.\n" : "S_Init: Setting up sound.\n");
        s.SoundInit++;
        s.MaxProgress = s.TextureGuess > 0 ? Math.Max(s.TextureGuess * 4, 4096) : 4096;
        s.PerShaderProgress = 0;
        s.NoStartScreen = s.Batch || s.Restart || s.Join || s.DedicatedJoin || s.JoinDedicated || s.Host || s.Dedicated || s.NoRun;
        s.VideoInitialized = false;
        s.FirstShaderCompiled = false;
    }
    static void AddBlanket(State s)
    {
        (int match, string? name)[] blanket =
        {
            (Raven, "game-Raven"),
            (DoomStrifeChex, "game-DoomStrifeChex"),
            (DoomChex, "game-DoomChex"),
            (Any, null),
        };
        foreach (var inf in blanket)
        {
            if ((s.GameType & inf.match) != 0) s.Filters.Add(inf.name!);
        }
    }
    static Exec? Multi(List<string> files, Exec? exec)
    {
        foreach (string file in files)
        {
            exec ??= new Exec();
            exec.Files.Add(file);
        }
        return exec;
    }
    static int Atoi(string text)
    {
        int i = 0;
        while (i < text.Length && text[i] is ' ' or '\t' or '\n' or '\r' or '\v' or '\f') i++;
        int sign = 1;
        if (i < text.Length && text[i] is '+' or '-')
        {
            if (text[i] == '-') sign = -1;
            i++;
        }
        int value = 0;
        while (i < text.Length && text[i] is >= '0' and <= '9') value = value * 10 + (text[i++] - '0');
        return sign * value;
    }
}
