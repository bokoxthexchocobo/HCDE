namespace HCDE.Playsim;

/// <summary>
/// The first half of D_Display from d_main.cpp 1192-1391. Chunk 1411.
/// The frame draw from line 1392 through the function close stays outside this band.
/// </summary>
public static class VmDMain1411Slice
{
    public const int BandStart = 1192;
    public const int BandEnd = 1391;
    public const int DisplayEnd = 1610;
    public const int TicRate = 35;
    public const int BlockedTraceCap = 12;
    public const int Level = 0;
    public const int DemoScreen = 3;
    public const int FullConsole = 4;
    public const int Startup = 6;
    public const int TitleLevel = 7;
    public const int ForceWipe = -1;
    public const int ForceFade = -2;
    public const int ForceBurn = -3;
    public const int ForceMelt = -4;
    public const int WipeNone = 0;
    public const int WipeMelt = 1;
    public const int WipeBurn = 2;
    public const int WipeFade = 3;
    public const int HardwareMode = 4;
    public const double DefaultFov = 90;

    public readonly record struct Note(
        string Kind,
        int Frame,
        int GameState,
        bool App,
        bool View,
        int Tic,
        bool SetMode,
        bool User,
        int Width,
        int Height,
        bool Waiting);

    public sealed class State
    {
        public bool NoDrawers;
        public bool Screen = true;
        public bool Dedicated;
        public bool DedicatedMode;
        public bool FirstFrameFocus = true;
        public int TraceFrame;
        public bool AppActive = true;
        public bool SawAppActive;
        public bool ViewActive;
        public int GameTic;
        public bool SetModeNeeded;
        public bool UserGame;
        public int GameState = Level;
        public int Width = 320;
        public int Height = 200;
        public bool ActiveInBackground;
        public int BlockedTraces;
        public bool Clocked;
        public uint Caps;
        public Func<uint>? ReadCaps;
        public int? Camera;
        public int? Mo;
        public double TimeFrac;
        public Func<int, double, double>? ReadFov;
        public double AppliedFov;
        public bool FovApplied;
        public bool VidFullscreen;
        public Action<bool>? ToggleFullscreen;
        public int ModePasses;
        public int ResizedWidth = -1;
        public int ResizedHeight = -1;
        public bool SizeNeeded;
        public bool HasStatusBar;
        public int ViewWidth;
        public int ViewHeight;
        public int ViewSizePasses;
        public Action? ExecuteSetViewSize;
        public bool NetGame;
        public int NoWipe;
        public Func<bool>? CanWipe;
        public int WipeGameState;
        public int VrMode;
        public int RenderMode = HardwareMode;
        public int WipeTypeCvar = WipeMelt;
        public int WipeStart;
        public bool WipeTypeSet;
        public int WipeType;
        public int WipeStarts;
        public Func<int>? StartWipe;
        public int FocusPasses;
        public int GcPasses;
        public int DeltaPasses;
        public readonly List<Note> Notes = new();
    }

    public static bool Display(State state)
    {
        state.GcPasses++;
        state.DeltaPasses++;
        state.WipeStart = 0;
        state.WipeTypeSet = false;
        state.FovApplied = false;
        state.Clocked = false;
        state.ResizedWidth = -1;
        state.ResizedHeight = -1;
        if (state.NoDrawers || !state.Screen || state.Dedicated || state.DedicatedMode)
            return false;

        bool traceWindow = state.TraceFrame < TicRate * 10;
        if (traceWindow && state.TraceFrame % 5 == 0)
            Add(state, "pre", state.TraceFrame, false);

        if (state.FirstFrameFocus)
        {
            state.FirstFrameFocus = false;
            state.FocusPasses++;
            if (traceWindow)
                Add(state, "focus", state.TraceFrame, false);
        }

        if (!state.SawAppActive && !state.AppActive)
        {
            state.FocusPasses++;
            state.AppActive = true;
            if (traceWindow)
                Add(state, "forced", state.TraceFrame, false);
        }

        if (state.AppActive)
            state.SawAppActive = true;

        if (!state.SawAppActive && state.TraceFrame < TicRate * 6)
        {
            if (state.TraceFrame % 10 == 0)
                Add(state, "waiting", state.TraceFrame, false);
            state.TraceFrame++;
        }
        else if (traceWindow)
        {
            state.TraceFrame++;
        }

        bool waiting = !state.SawAppActive;
        if (!state.AppActive
            && !state.SetModeNeeded
            && !state.ActiveInBackground
            && state.UserGame
            && state.GameState != Startup
            && state.GameState != TitleLevel
            && state.GameState != DemoScreen
            && !waiting)
        {
            if (state.BlockedTraces < BlockedTraceCap)
            {
                Add(state, "blocked", state.TraceFrame, waiting);
                state.BlockedTraces++;
            }

            return false;
        }

        if (traceWindow && state.TraceFrame % 5 == 0)
            Add(state, "passed", state.TraceFrame, waiting);

        state.Clocked = true;
        if (state.ReadCaps == null)
            throw new InvalidOperationException("caps");
        state.Caps = state.ReadCaps();

        if (state.Camera is null)
            state.Camera = state.Mo;

        if (state.ViewActive)
        {
            double fov = DefaultFov;
            if (state.Camera is int cam)
            {
                if (state.ReadFov == null)
                    throw new InvalidOperationException("fov");
                fov = state.ReadFov(cam, state.TimeFrac);
            }

            state.AppliedFov = fov;
            state.FovApplied = true;
        }

        if (state.SetModeNeeded)
        {
            state.SetModeNeeded = false;
            state.ModePasses++;
            state.ToggleFullscreen?.Invoke(state.VidFullscreen);
            state.ResizedWidth = state.Width;
            state.ResizedHeight = state.Height;
        }

        if (state.SizeNeeded)
        {
            if (!state.HasStatusBar)
            {
                state.ViewWidth = state.Width;
                state.ViewHeight = state.Height;
                state.SizeNeeded = false;
            }
            else
            {
                state.ViewSizePasses++;
                state.ExecuteSetViewSize?.Invoke();
            }
        }

        if (state.NetGame || state.NoWipe != 0 || (state.CanWipe != null && !state.CanWipe()))
        {
            if (state.NoWipe > 0)
                state.NoWipe--;
            state.WipeStart = 0;
            state.WipeGameState = state.GameState;
        }
        else if (state.GameState != state.WipeGameState
            && state.GameState != FullConsole
            && state.GameState != TitleLevel)
        {
            if (state.VrMode == 0 || state.RenderMode != HardwareMode)
            {
                state.WipeStarts++;
                state.WipeStart = state.StartWipe != null ? state.StartWipe() : state.WipeStarts;
                int previous = state.WipeGameState;
                if (previous == ForceFade)
                    state.WipeType = WipeFade;
                else if (previous == ForceBurn)
                    state.WipeType = WipeBurn;
                else if (previous == ForceMelt)
                    state.WipeType = WipeMelt;
                else
                    state.WipeType = state.WipeTypeCvar;
                state.WipeTypeSet = true;
            }

            state.WipeGameState = state.GameState;
        }
        else
        {
            state.WipeStart = 0;
        }

        return true;
    }

    private static void Add(State state, string kind, int frame, bool waiting)
    {
        state.Notes.Add(new Note(
            kind,
            frame,
            state.GameState,
            state.AppActive,
            state.ViewActive,
            state.GameTic,
            state.SetModeNeeded,
            state.UserGame,
            state.Screen ? state.Width : -1,
            state.Screen ? state.Height : -1,
            waiting));
    }
}
