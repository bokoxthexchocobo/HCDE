using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Error cleanup, the game loop, and the Strife demo pages from d_main.cpp
/// 1619-1942. Chunk 1413. D_DoAdvanceDemo starts at line 1950.
/// </summary>
public static class VmDMain1413Slice
{
    public const int BandStart = 1619;
    public const int BandEnd = 1942;
    public const int AdvanceDemoStart = 1950;
    public const int TicRate = 35;
    public const int DemoScreen = 3;
    public const int MenuOff = 0;
    public const int FullConsoleAction = 17;
    public const int Live = 0;
    public const int Shareware = 2;
    public const int Teaser2 = 8;
    public const int ForceFade = -2;
    public const int VoiceChannel = 2;
    public const int UiSound = 32;
    public const float NormalAttenuation = 1f;

    public static readonly string[] FullVoices =
        { "svox/pro1", "svox/pro2", "svox/pro3", "svox/pro4", "svox/pro5", "svox/pro6" };

    public static readonly string[] TeaserVoices =
        { "svox/voc91", "svox/voc92", "svox/voc93", "svox/voc94", "svox/voc95", "svox/voc96" };

    public sealed class RecoverableError : Exception
    {
        public string? Text { get; }
        public RecoverableError(string? text) { Text = text; }
    }

    public sealed class FilesystemError : Exception
    {
        public string? Text { get; }
        public FilesystemError(string? text) { Text = text; }
    }

    public sealed class VmAbort : Exception
    {
        public string Stack { get; }
        public VmAbort(string? stack) { Stack = stack ?? ""; }
    }

    public sealed class State
    {
        public string? CleanupReason;
        public int GameState;
        public int GameAction;
        public int GameTic;
        public bool SaveRestore = true;
        public bool HasLevel;
        public string MapName = "";
        public int BotsRemoved;
        public bool DemoRecording;
        public bool DemoPlayback;
        public int DemoChecks;
        public int BufferClears;
        public int NewInits;
        public int MenuClears;
        public int ConsolePlayer = -1;
        public bool ReservedSlot;
        public int PlayerInGame0;
        public int PlayerState = 2;
        public int Menu = 4;
        public bool InSave = true;
        public int VmClears;
        public bool NoInterpolate;
        public int PageId = 3;
        public string? Subtitle = "old";
        public int AdvisoryId = 2;
        public int CursorPasses;
        public bool WantRestart;
        public bool NetGame;
        public bool Abort;
        public int PageTic = 1;
        public bool AdvancedDemo;
        public bool PageBlank;
        public int DemoSequence;
        public int GameFlags;
        public int WipeState = 4;
        public string? PageName;
        public bool GenericUi;
        public int Width = 320;
        public int Height = 200;
        public int HideConsole;
        public int MaybePrints;
        public List<string> Genders = new();
        public string? DefaultGender;
        public Func<string, int>? MusicLump;
        public Func<string, int>? FindPage;
        public Action? StartFrame;
        public Action? SetFrameTime;
        public Action? Rcon;
        public Action? RunTics;
        public Action? StartTic;
        public Action? Events;
        public Action? Display;
        public Action? UpdateMusic;
        public Action<string>? Command;
        public Action? MaybePrint;
        public readonly List<string> Steps = new();
        public readonly List<string> Lines = new();
        public readonly List<string> Bold = new();
        public readonly List<string> Sounds = new();
        public readonly List<string> Music = new();
    }

    public static void ErrorCleanup(State state, string? reason)
    {
        string cleanupReason = string.IsNullOrEmpty(reason) ? "unspecified" : reason;
        state.CleanupReason = cleanupReason;
        bool normalShutdown = cleanupReason == "engine-shutdown";
        if (!normalShutdown)
        {
            state.Lines.Add(
                "HCDE error cleanup: reason=" + cleanupReason
                + " gamestate=" + state.GameState
                + " gameaction=" + state.GameAction
                + " gametic=" + state.GameTic + "\n");
            state.Steps.Add(
                "error cleanup reason=" + cleanupReason
                + " gamestate=" + state.GameState
                + " gameaction=" + state.GameAction
                + " gametic=" + state.GameTic);
        }

        state.SaveRestore = false;
        if (state.HasLevel)
        {
            state.BotsRemoved++;
            state.Steps.Add("bots");
        }

        state.Steps.Add("quit:" + cleanupReason);
        if (state.DemoRecording || state.DemoPlayback)
        {
            state.DemoChecks++;
            state.Steps.Add("demo-status");
        }

        state.BufferClears++;
        state.NewInits++;
        state.MenuClears++;
        if (state.ConsolePlayer != -1 && !state.ReservedSlot)
        {
            int oldState = state.PlayerState;
            state.PlayerInGame0 = 1;
            state.PlayerState = Live;
            state.Steps.Add(
                "player=0 " + PlayerStateName(oldState) + " -> " + PlayerStateName(Live)
                + " reason=D_QuitNetGame_fallback");
        }

        state.GameAction = FullConsoleAction;
        if (state.GameState == DemoScreen)
            state.Menu = MenuOff;
        state.InSave = false;
        state.VmClears++;
        state.Steps.Add("vm-stack");
    }

    public static void DoomLoop(State state)
    {
        int lastTic = 0;
        state.Steps.Add("entered D_DoomLoop");
        state.NoInterpolate = true;
        state.PageId = -1;
        state.Subtitle = null;
        state.AdvisoryId = -1;
        state.CursorPasses++;
        while (true)
        {
            try
            {
                if (state.ConsolePlayer != -1)
                {
                    state.DefaultGender = state.Genders[state.ConsolePlayer];
                    state.Steps.Add("gender:" + state.DefaultGender);
                }

                if (state.GameTic > lastTic)
                {
                    lastTic = state.GameTic;
                    state.Steps.Add("start-frame");
                    state.StartFrame?.Invoke();
                }

                state.Steps.Add("frame-time");
                state.SetFrameTime?.Invoke();
                state.Steps.Add("rcon");
                state.Rcon?.Invoke();
                state.Steps.Add("tics");
                state.RunTics?.Invoke();
                state.Steps.Add("start-tic");
                state.StartTic?.Invoke();
                state.Steps.Add("events");
                state.Events?.Invoke();
                state.Steps.Add("display");
                state.Display?.Invoke();
                state.Steps.Add("music-update");
                state.UpdateMusic?.Invoke();
                if (state.Abort)
                {
                    state.Steps.Add("quickexit");
                    state.Command?.Invoke("quickexit");
                }

                if (!state.WantRestart)
                    continue;

                if (state.NetGame && !state.DemoPlayback)
                {
                    string map = state.HasLevel ? state.MapName : "<none>";
                    state.Lines.Add(
                        "HCDE ignored engine restart request during netgame at gametic="
                        + state.GameTic + " map=" + map + "\n");
                    state.Steps.Add(
                        "ignored engine restart request during netgame gametic="
                        + state.GameTic + " map=" + map);
                    state.WantRestart = false;
                    continue;
                }

                state.Steps.Add("restart request detected, exiting D_DoomLoop");
                state.WantRestart = false;
                return;
            }
            catch (RecoverableError error)
            {
                string shown = string.IsNullOrEmpty(error.Text) ? "unknown" : error.Text;
                state.Steps.Add("CRecoverableError: " + shown);
                if (!string.IsNullOrEmpty(error.Text))
                {
                    state.Lines.Add("\nHCDE recoverable error: " + error.Text + "\n");
                    state.Bold.Add("\n" + error.Text + "\n");
                }

                ErrorCleanup(state, "recoverable-error");
            }
            catch (FilesystemError error)
            {
                bool present = error.Text != null;
                string shown = present ? error.Text! : "unknown";
                state.Steps.Add("FileSystemException: " + shown);
                if (present)
                {
                    state.Lines.Add("\nHCDE filesystem error: " + error.Text + "\n");
                    state.Bold.Add("\n" + error.Text + "\n");
                }

                ErrorCleanup(state, "filesystem-error");
            }
            catch (VmAbort error)
            {
                state.Steps.Add("CVMAbortException occurred - stacktrace available");
                state.Lines.Add("\nHCDE VM abort forced game cleanup.\n");
                state.Steps.Add("vm-maybe");
                state.MaybePrint?.Invoke();
                if (error.Stack.Length > 0)
                    state.Lines.Add(error.Stack);
                state.Bold.Add(error.Stack);
                ErrorCleanup(state, "vm-abort");
            }
        }
    }

    public static void PageTicker(State state)
    {
        state.PageTic--;
        if (state.PageTic < 0)
            AdvanceDemo(state);
    }

    public static void PageDrawer(State state)
    {
        state.Steps.Add("clear:0:0:" + state.Width + ":" + state.Height + ":0:0");
        if (state.PageId >= 0)
            state.Steps.Add("page:full:unmasked:bilinear");
        if (state.Subtitle != null)
        {
            string font = state.GenericUi ? "newsmall" : "small";
            state.Steps.Add("subtitle:" + font + ":" + state.Subtitle);
        }

        if (state.AdvisoryId >= 0)
            state.Steps.Add("advisory:4:160:320x200");
    }

    public static void AdvanceDemo(State state)
    {
        state.AdvancedDemo = true;
    }

    public static void StrifeAdvance(State state)
    {
        string[] voices = (state.GameFlags & Shareware) != 0 ? TeaserVoices : FullVoices;
        string? pageName = null;
        string? subtitle = null;
        state.GameState = DemoScreen;
        state.PageBlank = false;
        switch (state.DemoSequence)
        {
            case 1:
                state.PageTic = 10 * TicRate / 35;
                pageName = "";
                state.PageBlank = true;
                Sound(state, "bishop/active");
                break;
            case 2:
                state.PageTic = 4 * TicRate;
                pageName = "RGELOGO";
                break;
            case 3:
                state.PageTic = 7 * TicRate;
                pageName = "PANEL1";
                subtitle = "$TXT_SUB_INTRO1";
                Sound(state, voices[0]);
                state.Music.Add((state.GameFlags & Teaser2) != 0 ? "d_fmintr" : "d_intro");
                break;
            case 4:
                state.PageTic = 9 * TicRate;
                pageName = "PANEL2";
                subtitle = "$TXT_SUB_INTRO2";
                Sound(state, voices[1]);
                break;
            case 5:
                state.PageTic = 12 * TicRate;
                pageName = "PANEL3";
                subtitle = "$TXT_SUB_INTRO3";
                Sound(state, voices[2]);
                break;
            case 6:
                state.PageTic = 11 * TicRate;
                pageName = "PANEL4";
                subtitle = "$TXT_SUB_INTRO4";
                Sound(state, voices[3]);
                break;
            case 7:
                state.PageTic = 10 * TicRate;
                pageName = "PANEL5";
                subtitle = "$TXT_SUB_INTRO5";
                Sound(state, voices[4]);
                break;
            case 8:
                state.PageTic = 16 * TicRate;
                pageName = "PANEL6";
                subtitle = "$TXT_SUB_INTRO6";
                Sound(state, voices[5]);
                break;
            case 9:
                state.PageTic = 6 * TicRate;
                pageName = "vellogo";
                state.WipeState = ForceFade;
                break;
            case 10:
                state.PageTic = 12 * TicRate;
                pageName = "CREDIT";
                state.WipeState = ForceFade;
                break;
            default:
                state.PageTic = 6 * TicRate;
                pageName = "TITLEPIC";
                if (state.MusicLump == null)
                    throw new InvalidOperationException("music");
                state.Music.Add(state.MusicLump("d_logo") < 0 ? "" : "d_logo");
                state.HideConsole++;
                break;
        }

        int previous = state.DemoSequence;
        state.DemoSequence = previous + 1;
        if (previous > 10)
            state.DemoSequence = 0;
        if (state.DemoSequence == 9 && (state.GameFlags & Shareware) == 0)
            state.DemoSequence = 10;

        if (pageName != null)
        {
            if (state.FindPage == null)
                throw new InvalidOperationException("page");
            state.PageName = pageName;
            state.PageId = state.FindPage(pageName);
            state.Subtitle = subtitle;
        }
        else
        {
            state.Subtitle = null;
        }
    }

    private static string PlayerStateName(int state) => state switch
    {
        0 => "PST_LIVE",
        1 => "PST_DEAD",
        2 => "PST_REBORN",
        3 => "PST_ENTER",
        4 => "PST_GONE",
        _ => "PST_UNKNOWN",
    };

    private static void Sound(State state, string name)
    {
        state.Sounds.Add(
            VoiceChannel + ":" + UiSound + ":" + name + ":1:"
            + NormalAttenuation.ToString(CultureInfo.InvariantCulture));
    }
}
