namespace HCDE.Playsim;
/// <summary>
/// D_InitGame launch tail from d_main.cpp 4189-4499.
/// Resource loading ends at line 4188. D_DoomMain starts at line 4507.
/// </summary>
public static class VmDMain1423Slice
{
    public const int BandStart = 4189;
    public const int BandEnd = 4499;
    public const int ResourceEnd = 4188;
    public const int DoomMain = 4507;
    public const int FromIwad = 0;
    public const int FromPwads = 1;
    public const int SkipBex = 1;
    public const int Doom = 1;
    public const int FullConsole = 4;
    public const int Startup = 6;
    public const int LoadGame = 5;
    public const int LoadGameHide = 6;
    public const int TitleLoop = 21;
    public const int MapWarp = 22;
    public const int BlockUi = 1;
    public const int TicRate = 35;
    public const int Body = 4;
    public const int Ui = 32;
    public const int Rumble = 0x40000;
    public const int NoRumble = 0x80000;
    public const int SpecialExit = 1337;
    public const string MissingSave = "Cannot find savegame ";
    public sealed class Fatal(string message) : Exception(message);
    public sealed class Patch
    {
        public string? Key;
        public string? Value;
        public bool Exists;
    }
    public sealed class State
    {
        public bool StartScreen;
        public bool NullOverlay;
        public bool WindowMissing;
        public bool ScreenMissing;
        public string? Config = "Doom";
        public bool Batch;
        public int SoundsBefore = 1;
        public int SoundsAfter = 1;
        public bool SkipBexStrings;
        public bool FoundDeh;
        public bool FoundBex;
        public int GameType = Doom;
        public bool SectionOpens;
        public List<Patch> Patches = new();
        public bool Debugging;
        public bool LevelMissing;
        public List<string>? Bots = new();
        public bool Restart;
        public bool NetOk = true;
        public bool NoRun;
        public bool Dedicated;
        public bool VideoInitialized;
        public bool FirstShader;
        public bool NoStartScreen;
        public int PerShader;
        public int MaxProgress = 4096;
        public string?[] StartupTexts = new string?[5];
        public bool CustomizeInvul;
        public string? Record;
        public bool AutoStart;
        public bool Haptics;
        public int ShaderFails;
        public bool FullscreenAspect;
        public int Width = 320;
        public int Height = 200;
        public string? LoadSlot;
        public string? BuiltSave;
        public bool SaveExists = true;
        public string? PlayDemo;
        public string? TimeDemo;
        public bool NetGame;
        public bool WarpWipe;
        public bool DemoRecording;
        public string? StartMap = "MAP01";
        public string StoredWarp = "";
        public int GameAction;
        public bool Multiplayer;
        public bool NoIntros;
        public bool NoIntroParm;
        public string? Intro = "intro";
        public bool CutsceneStarts;
        public bool InvokeCallback;
        public int GlDefs;
        public int Refresh;
        public int Decals;
        public int IwadFlags = -1;
        public int PwadLoads;
        public int Options;
        public int FinishDeh;
        public int LockedSounds;
        public int Menus;
        public int Symbols;
        public int ActorNums;
        public int Spawnables;
        public int ActorSet;
        public int Edf;
        public int Cleared;
        public List<string> Spawned = new();
        public int Tries = -1;
        public int Wanted = -1;
        public int Automap;
        public int Play;
        public int Weapons;
        public int Bars;
        public int NetChecks;
        public int NoSet;
        public int LateVideo;
        public int ShaderCalls;
        public int Palette;
        public int OverlayArg = -1;
        public bool SetMode;
        public bool Rendered;
        public int NetInit;
        public int Delayed;
        public int Invul;
        public int Recorded;
        public int SoundChannel;
        public int SoundFlags = -1;
        public string? SoundName;
        public int SoundVolume;
        public double SoundAtten = -1;
        public int WindowAdvance;
        public int ScreenProgress;
        public bool ScreenDeleted;
        public bool Began;
        public bool Ended;
        public int Joystick;
        public int Vr;
        public int Jobs;
        public string? LoadedSave;
        public bool SingleDemo;
        public string? Played;
        public string? Timed;
        public int NoWipe = -1;
        public string? Warped;
        public string? RecordedMap;
        public bool RecordedNull;
        public bool InitNew;
        public List<string> Commands = new();
        public int FrameTime;
        public int CutsceneBlock = -1;
        public string? Cutscene;
        public int Titles;
        public int ModeAdjust;
        public int Engine;
        public int GameState = -1;
        public List<string> Guards = new();
        public List<string> Lines = new();
        public List<string> Status = new();
        public List<string> Phases = new();
        public List<(string Text, int Color)> Loading = new();
        public List<string> DehFiles = new();
        public int StringChecks;
    }
    public static int Launch(State s)
    {
        if (s.StartScreen) s.Phases.Add("GL definitions");
        s.GlDefs++;
        s.Status.Add($"R_Init: Init {s.Config ?? throw new NullReferenceException()} refresh subsystem.");
        if (s.StartScreen)
        {
            s.Loading.Add(("Loading graphics", 0x3f));
            s.ScreenProgress++;
        }
        if (s.WindowMissing) throw new NullReferenceException();
        s.WindowAdvance++;
        s.Refresh++;
        if (!s.Batch) s.Lines.Add("DecalLibrary: Load decals.\n");
        s.Decals++;
        s.IwadFlags = s.SkipBexStrings ? SkipBex : 0;
        ApplySection(s);
        s.PwadLoads++;
        s.Options++;
        s.FinishDeh++;
        if (s.SoundsBefore < s.SoundsAfter) s.LockedSounds++;
        if (!s.Batch) s.Lines.Add("M_Init: Init menus.\n");
        s.Menus++;
        if (!s.Debugging) s.Symbols++;
        s.ActorNums++;
        s.Spawnables++;
        s.ActorSet++;
        s.Edf++;
        if (s.LevelMissing) throw new NullReferenceException();
        s.Cleared++;
        if (s.Bots == null) throw new NullReferenceException();
        foreach (var bot in s.Bots) s.Spawned.Add(bot);
        s.Tries = 0;
        s.Wanted = s.Spawned.Count;
        s.Status.Add("P_Init: Init Playloop state.");
        if (s.StartScreen) s.Loading.Add(("Init game engine", 0x3f));
        s.Automap++;
        s.Play++;
        s.Weapons++;
        s.Bars++;
        if (!s.Batch)
        {
            foreach (var text in s.StartupTexts)
            {
                s.StringChecks++;
                if (!string.IsNullOrEmpty(text)) s.Lines.Add(text + "\n");
            }
        }
        if (!s.Restart)
        {
            s.Status.Add("D_CheckNetGame: Checking network game status.");
            if (s.StartScreen) s.Loading.Add(("Checking network game status.", 0x3f));
            s.NetChecks++;
            if (!s.NetOk) return 0;
        }
        s.NoSet++;
        if (!(s.Restart || s.NoRun || s.Dedicated) && !s.VideoInitialized)
        {
            s.Guards.Add("late video initialization");
            s.LateVideo++;
            s.VideoInitialized = true;
        }
        if (!(s.Batch || s.NoRun || s.Dedicated)) s.Lines.Add("V_Init: allocate screen.\n");
        if (!(s.Restart || s.NoRun || s.Dedicated) && !s.FirstShader)
        {
            if (s.ScreenMissing) throw new NullReferenceException();
            s.ShaderCalls++;
            s.FirstShader = true;
        }
        else if (!(s.NoRun || s.Dedicated))
        {
            if (s.ScreenMissing) throw new NullReferenceException();
            s.Palette++;
        }
        if (!s.StartScreen && !s.NoStartScreen)
        {
            s.Guards.Add("startup overlay");
            s.OverlayArg = s.PerShader > 0 ? s.MaxProgress * 10 / 9 : s.MaxProgress + 3;
            s.SetMode = true;
            if (!s.NullOverlay)
            {
                s.StartScreen = true;
                s.Rendered = true;
            }
        }
        if (s.NoRun || s.Batch) return SpecialExit;
        s.GameState = FullConsole;
        s.NetInit++;
        if (!s.Dedicated) s.Delayed++;
        s.GameState = Startup;
        if (s.CustomizeInvul) s.Invul++;
        if (!s.Restart)
        {
            if (s.Record != null)
            {
                s.Recorded++;
                s.AutoStart = true;
            }
            if (!s.Dedicated)
            {
                s.SoundChannel = Body;
                s.SoundFlags = Ui | (s.Haptics ? Rumble : NoRumble);
                s.SoundName = "misc/startupdone";
                s.SoundVolume = 1;
                s.SoundAtten = 0;
            }
            s.Status.Add("Init complete.");
            s.WindowAdvance += s.MaxProgress;
            if (s.StartScreen)
            {
                s.ScreenDeleted = true;
                s.StartScreen = false;
            }
            if (!s.Dedicated)
            {
                if (s.ScreenMissing) throw new NullReferenceException();
                int fails = s.ShaderFails;
                while (true)
                {
                    s.ShaderCalls++;
                    if (fails == 0) break;
                    fails--;
                }
                s.Began = true;
                s.Ended = true;
                s.Joystick++;
                s.Vr++;
            }
            s.Jobs++;
            if (s.LoadSlot != null)
            {
                _ = s.BuiltSave ?? throw new NullReferenceException();
                if (!s.SaveExists) throw new Fatal(MissingSave + s.BuiltSave);
                s.LoadedSave = s.BuiltSave;
                s.GameAction = LoadGame;
            }
            if (s.PlayDemo != null)
            {
                s.SingleDemo = true;
                s.Played = s.PlayDemo;
            }
            else if (s.TimeDemo != null) s.Timed = s.TimeDemo;
            else if (s.GameAction != LoadGame && s.GameAction != LoadGameHide) OpenSession(s);
            else if (s.DemoRecording) s.RecordedNull = true;
        }
        else
        {
            s.ModeAdjust++;
            s.Titles++;
            s.SetMode = false;
        }
        s.Engine++;
        return 0;
    }
    static void ApplySection(State s)
    {
        if (s.FoundDeh || s.FoundBex || s.GameType != Doom || !s.SectionOpens) return;
        foreach (var patch in s.Patches)
        {
            if (patch.Key == null) throw new NullReferenceException();
            if (!IsPath(patch.Key)) continue;
            _ = patch.Value ?? throw new NullReferenceException();
            if (!patch.Exists) continue;
            if (!s.Batch) s.Lines.Add($"Applying patch {patch.Value}\n");
            s.DehFiles.Add(patch.Value);
        }
    }
    static void OpenSession(State s)
    {
        if (s.AutoStart || s.NetGame)
        {
            if (!s.WarpWipe) s.NoWipe = TicRate;
            _ = s.StartMap ?? throw new NullReferenceException();
            s.Warped = s.StartMap;
            if (s.DemoRecording) s.RecordedMap = s.StartMap;
            s.InitNew = true;
            if (s.StoredWarp.Length > 0)
            {
                if (s.Dedicated) s.Lines.Add($"Dedicated server ignored startup command: {s.StoredWarp}\n");
                else s.Commands.Add(s.StoredWarp);
                s.StoredWarp = "";
            }
            s.GameAction = MapWarp;
            return;
        }
        if (s.Multiplayer || s.NoIntros || s.NoIntroParm)
        {
            s.Titles++;
            return;
        }
        s.FrameTime++;
        s.Cutscene = s.Intro;
        s.CutsceneBlock = BlockUi;
        if (!s.CutsceneStarts) s.Titles++;
        else if (s.InvokeCallback) s.GameAction = TitleLoop;
    }
    static bool IsPath(string key)
    {
        if (key.Length != 4) return false;
        return (key[0] | 32) == 'p' && (key[1] | 32) == 'a' && (key[2] | 32) == 't' && (key[3] | 32) == 'h';
    }
}
