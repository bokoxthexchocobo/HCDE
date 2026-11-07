namespace HCDE.Playsim;
/// <summary>
/// D_InitGame startup screen and resource load from d_main.cpp 4019-4188.
/// The boot phase ends at line 4018. GL definitions start at line 4190.
/// </summary>
public static class VmDMain1422Slice
{
    public const int BandStart = 4019;
    public const int BandEnd = 4188;
    public const int BootEnd = 4018;
    public const int GlPhase = 4190;
    public const int DefaultStartup = 0;
    public const int DoomStartup = 1;
    public const int HereticStartup = 2;
    public const int HexenStartup = 3;
    public const int StrifeStartup = 4;
    public const int Heretic = 2;
    public const int Hexen = 4;
    public const int Strife = 8;
    public const int Flat = 2;
    public const string NoClasses = "No player classes defined";
    public sealed class Fatal(string message) : Exception(message);
    public sealed class State
    {
        public int Startup = DefaultStartup;
        public int GameType = 1;
        public string? Config = "doom";
        public bool HasExec;
        public int ExecRuns;
        public bool Dedicated;
        public int Delayed;
        public int DelayedCalls;
        public string? Language = "en";
        public string? LoadedLanguage;
        public bool Restart;
        public string? CpuDump = "cpu";
        public string? CompatValue;
        public int CompatMode = -1;
        public bool Batch;
        public int MaxProgress = 4096;
        public int PerShader;
        public bool NoStartScreen;
        public bool VideoInitialized;
        public bool FirstShader;
        public bool ScreenMissing;
        public bool NullStartScreen;
        public int ScreenArg = -1;
        public bool SetMode;
        public bool Rendered;
        public int WindowArg = -1;
        public string WindowKind = "";
        public int WindowProgress;
        public int ScreenProgress;
        public List<string> Guards = new();
        public List<string> Lines = new();
        public List<string> Status = new();
        public List<string> Phases = new();
        public int Clears;
        public int CmdLine;
        public int Reverb;
        public int SoundData;
        public string? MapInfo = "MAPINFO";
        public string? ParsedMap;
        public int Skill;
        public int Episode;
        public string? MessageBox = "MessageBox";
        public string? Endoom = "ENDOOM";
        public double Blur = 0.5;
        public string? CopiedBox;
        public string? CopiedEndoom;
        public double CopiedBlur;
        public int Statistics;
        public int MusInfo;
        public int Upscale;
        public int FramesCleared;
        public int TexturePasses = 1;
        public int HacksPassed;
        public int TilesPassed;
        public int Patches;
        public int Anim;
        public int Boom;
        public string? Border = "FLOOR0_1";
        public int BorderType = -1;
        public bool ConbackTrue;
        public double ConbackAmount = -1;
        public int Wide;
        public int Fonts;
        public int DoomFonts;
        public int Translations;
        public bool? GenericUi;
        public int Teams;
        public int Translations2;
        public int Actors;
        public int ZsCallbacks;
        public int Jobs;
        public int PlayerSetup;
        public int WadSettings;
        public int PlayerClasses = 1;
        public int Callbacks;
        public int FontColors;
        public int Chat;
        public int Strings;
        public int PreVideoClears;
        public int ShaderCompiles;
        public int VideoInits;
    }
    public static void LoadResources(State s)
    {
        if (s.Startup == DefaultStartup)
        {
            if (s.GameType == Hexen) s.Startup = HexenStartup;
            else if (s.GameType == Heretic) s.Startup = HereticStartup;
            else if (s.GameType == Strife) s.Startup = StrifeStartup;
        }
        _ = s.Config ?? throw new NullReferenceException();
        if (s.HasExec)
        {
            s.ExecRuns++;
            s.HasExec = false;
        }
        if (s.Dedicated)
        {
            s.DelayedCalls++;
            if (s.Delayed > 0)
                s.Lines.Add($"Dedicated server ignored {s.Delayed} delayed startup command{(s.Delayed == 1 ? "" : "s")}.\n");
        }
        s.LoadedLanguage = s.Language;
        s.Strings++;
        s.FontColors++;
        s.Chat++;
        if (!s.Restart)
        {
            s.Status.Add("I_Init: Setting up machine state.");
            s.Lines.Add(s.CpuDump ?? throw new NullReferenceException());
        }
        s.Callbacks++;
        if (s.CompatValue != null) s.CompatMode = Strtoll(s.CompatValue);
        if (!s.Batch && !s.Dedicated) s.Lines.Add("ST_Init: Init startup screen.\n");
        if (!s.Restart)
        {
            if (s.Dedicated)
            {
                s.WindowKind = "dedicated";
                s.WindowArg = s.MaxProgress;
            }
            else
            {
                s.Guards.Add("startup progress window");
                s.WindowKind = "instance";
                s.WindowArg = s.MaxProgress;
            }
        }
        else
        {
            s.WindowKind = "restart";
            s.WindowArg = 0;
        }
        if (!s.NoStartScreen && !s.Dedicated)
        {
            s.Guards.Add("startup video loader");
            s.PreVideoClears++;
            s.VideoInits++;
            if (s.ScreenMissing) throw new NullReferenceException();
            s.ShaderCompiles++;
            s.VideoInitialized = true;
            s.FirstShader = true;
            s.ScreenArg = s.PerShader > 0 ? s.MaxProgress * 10 / 9 : s.MaxProgress + 3;
            s.SetMode = true;
            if (!s.NullStartScreen) s.Rendered = true;
        }
        s.CmdLine++;
        s.Reverb++;
        s.Status.Add("S_InitData: Load sound definitions.");
        s.SoundData++;
        s.Status.Add("G_ParseMapInfo: Load map definitions.");
        s.ParsedMap = s.MapInfo;
        s.Skill++;
        s.Episode++;
        s.CopiedBox = s.MessageBox;
        s.CopiedEndoom = s.Endoom;
        s.CopiedBlur = s.Blur;
        s.Statistics++;
        s.MusInfo++;
        s.Status.Add("Texman.Init: Init texture manager.");
        s.Upscale++;
        s.FramesCleared++;
        s.HacksPassed++;
        s.TilesPassed++;
        for (int i = 0; i < s.TexturePasses; i++)
        {
            s.WindowProgress++;
            if (!s.NoStartScreen && !s.Dedicated && !s.NullStartScreen) s.ScreenProgress++;
        }
        s.Patches++;
        s.Anim++;
        s.Boom++;
        s.BorderType = Flat;
        _ = s.Border ?? throw new NullReferenceException();
        s.ConbackTrue = true;
        s.ConbackAmount = 0.25;
        s.Wide++;
        s.WindowProgress++;
        if (!s.NoStartScreen && !s.Dedicated && !s.NullStartScreen) s.ScreenProgress++;
        s.Fonts++;
        s.DoomFonts++;
        s.Translations++;
        s.GenericUi = false;
        s.Status.Add("ParseTeamInfo: Load team definitions.");
        s.Teams++;
        s.Translations2++;
        if (!s.NoStartScreen && !s.Dedicated && !s.NullStartScreen) s.Phases.Add("actor definitions");
        s.Actors++;
        s.ZsCallbacks++;
        s.Jobs++;
        s.PlayerSetup++;
        if (!s.NoStartScreen && !s.Dedicated && !s.NullStartScreen) s.Phases.Add("key bindings");
        s.WadSettings++;
        if (s.PlayerClasses == 0) throw new Fatal(NoClasses);
        s.WindowProgress++;
        if (!s.NoStartScreen && !s.Dedicated && !s.NullStartScreen) s.ScreenProgress++;
    }
    static int Strtoll(string text)
    {
        int i = 0;
        while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
        int sign = 1;
        if (i < text.Length && text[i] is '+' or '-')
        {
            if (text[i] == '-') sign = -1;
            i++;
        }
        long value = 0;
        while (i < text.Length && text[i] is >= '0' and <= '9') value = value * 10 + (text[i++] - '0');
        return (int)(sign * value);
    }
}
