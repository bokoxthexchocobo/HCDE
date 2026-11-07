namespace HCDE.Playsim;

/// <summary>
/// The rest of D_Display from d_main.cpp 1392-1610. Chunk 1412.
/// The wipe-start choice ends at line 1391. D_ErrorCleanup starts at line 1619.
/// </summary>
public static class VmDMain1412Slice
{
    public const int BandStart = 1392;
    public const int BandEnd = 1610;
    public const int CleanupStart = 1619;
    public const int Level = 0;
    public const int DemoScreen = 3;
    public const int FullConsole = 4;
    public const int TitleLevel = 7;
    public const int Intro = 8;
    public const int Cutscene = 9;
    public const int StatusBar = 0;
    public const int Fullscreen = 1;
    public const int None = 2;
    public const int AltHud = 3;
    public const int MenuOff = 0;
    public const int Red = 6;
    public const int WipeNone = 0;
    public const int IconWipeBlock = 10;

    public readonly record struct Broken(string Text, int Width);
    public readonly record struct Pic(int Width, int Height, double Left);

    public sealed class State
    {
        public double FrameTime, AnimatedAt = double.NaN, SkyAt = double.NaN, TimeFrac, TicFrac;
        public Func<double>? ReadTime;
        public bool BeganFrame, ClearedClip, Began2D, TraceWindow, CameraHasPlayer, HudToggled;
        public bool Automap, ViewActive, AltHudOn, DrawFullHud, Interpolated, PauseExtended;
        public bool UsePauseString = true, GenericUi, Multiplayer, HasStatusBar = true, Unclocked;
        public int Width = 320, Height = 200, TwoDWidth, TwoDHeight, GameState = Level, GameTic;
        public int TraceFrame, ConsolePlayer, ViewHeight = 200, Blocks = 10, StatusTop = 168;
        public int Paused, LastPauseArg = int.MinValue, CleanX = 1, CleanY = 1, BigHeight = 8;
        public int SmallHeight = 8, NewSmallHeight = 8, PausePos, LastMaxWidth = -1, SndDraw;
        public int WipeStart, NoWipe, DecRefs, LastWipeEnd = -1, LastWipeType = -1;
        public int CycleValue = 1, StoredCycles = -1;
        public int? Camera, Actor, ViewSector, WipeType;
        public bool? WipeStopSound;
        public Action<bool, Action>? Render;
        public Func<int?>? RenderView;
        public Func<int, int>? DrawPaused;
        public Func<string, int, IReadOnlyList<Broken>>? BreakLines;
        public Func<string, Pic?>? FindIcon;
        public Func<int>? ReadWipeEnd;
        public string PauseText = "PAUSE", ByText = "by %s";
        public string? DrawIcon;
        public Pic PauseSign = new(64, 16, 0);
        public List<string> Names = new();
        public readonly List<string> Steps = new();
        public int Menu = MenuOff;
    }

    public static bool Present(State state)
    {
        if (state.ReadTime == null)
            throw new InvalidOperationException("time");
        state.FrameTime = state.ReadTime();
        state.AnimatedAt = state.FrameTime;
        state.SkyAt = state.TimeFrac;
        state.BeganFrame = true;
        state.ClearedClip = true;

        if ((state.GameState == Level || state.GameState == TitleLevel) && state.GameTic != 0)
        {
            if (state.TraceWindow && state.TraceFrame % 10 == 0)
            {
                Step(state, "world:" + state.TraceFrame + ":" + state.GameState + ":" + state.GameTic
                    + ":" + (state.Camera?.ToString() ?? "null")
                    + ":" + (state.Actor?.ToString() ?? "null")
                    + ":" + state.ConsolePlayer);
            }

            if (state.Render == null || state.RenderView == null)
                throw new InvalidOperationException("render");
            state.Interpolated = true;
            state.Render(true, () => state.ViewSector = state.RenderView());
            if (state.TraceWindow && state.TraceFrame % 10 == 0)
            {
                Step(state, "post:" + state.TraceFrame + ":" + (state.ViewSector?.ToString() ?? "null")
                    + ":" + (state.HudToggled ? 1 : 0) + ":" + (state.Automap ? 1 : 0));
            }

            Begin(state);
            if (!state.HudToggled)
                DrawWorldHud(state);
        }
        else
        {
            Begin(state);
            switch (state.GameState)
            {
                case FullConsole:
                    Step(state, "page");
                    Step(state, "console");
                    Step(state, "menu");
                    Step(state, "end");
                    return false;
                case DemoScreen:
                    Step(state, "page");
                    break;
                case Cutscene:
                case Intro:
                    Step(state, "job");
                    break;
            }
        }

        if (!state.HudToggled)
        {
            Step(state, "chat");
            DrawPause(state);
            DrawIcon(state);
            if (state.SndDraw != 0)
                Step(state, "wave:" + state.SndDraw);
        }

        Finish(state);
        return true;
    }

    private static void DrawWorldHud(State state)
    {
        if (!state.HasStatusBar)
            throw new InvalidOperationException("statusbar");
        Step(state, "blend:" + (state.ViewSector?.ToString() ?? "null"));
        if (state.Automap)
        {
            int bottom = state.AltHudOn && state.ViewHeight == state.Height
                ? state.ViewHeight
                : state.StatusTop;
            Step(state, "map:" + bottom);
        }

        if (!state.Automap || state.ViewActive)
            Step(state, "border");

        int hud;
        if (state.AltHudOn && state.ViewHeight == state.Height && state.Blocks > 10)
            hud = AltHud;
        else if (state.ViewHeight == state.Height && state.ViewActive && state.Blocks > 10)
            hud = state.DrawFullHud ? Fullscreen : None;
        else
            hud = StatusBar;

        Step(state, "bottom:" + hud);
        if (hud == AltHud && (state.DrawFullHud || state.Automap))
            Step(state, "althud");
        if (hud == AltHud && state.CameraHasPlayer && state.Camera != null && !state.Automap)
            Step(state, "cross:" + state.TicFrac.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Step(state, "call:" + hud + ":" + state.TicFrac.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Step(state, "top:" + hud);
    }

    private static void DrawPause(State state)
    {
        if ((state.Paused == 0 && !state.PauseExtended) || state.Menu != MenuOff || !state.HasStatusBar)
            return;
        bool skip = false;
        if (state.DrawPaused != null)
        {
            state.LastPauseArg = state.Paused - 1;
            skip = state.DrawPaused(state.LastPauseArg) != 0;
        }

        if (skip)
            return;
        if (state.CleanX == 0)
            throw new InvalidOperationException("clean");
        int maxWidth = state.TwoDWidth / state.CleanX;
        state.LastMaxWidth = maxWidth;
        int pausePos = 0;
        if (state.UsePauseString)
        {
            var lines = Lines(state, state.PauseText, maxWidth);
            int y = 4;
            foreach (var line in lines)
            {
                int x = (state.TwoDWidth - line.Width * state.CleanX) / 2;
                Step(state, "text:big:" + Red + ":" + line.Text + ":" + x + ":" + y);
                y += state.BigHeight * state.CleanY;
            }

            pausePos = y;
        }
        else
        {
            Pic tex = state.PauseSign;
            double x = (state.Width - tex.Width * state.CleanX) / 2.0 + tex.Left * state.CleanX;
            Step(state, "sign:" + x.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":4");
            pausePos = tex.Height * state.CleanY + 4;
        }

        state.PausePos = pausePos;
        if (state.Paused != 0 && state.Multiplayer)
        {
            if (state.Paused < 1 || state.Paused > state.Names.Count)
                throw new InvalidOperationException("player");
            string font = state.GenericUi ? "newsmall" : "small";
            int height = state.GenericUi ? state.NewSmallHeight : state.SmallHeight;
            string text = state.ByText.Replace("%s", state.Names[state.Paused - 1]);
            var lines = Lines(state, text, maxWidth);
            int y = 4;
            foreach (var line in lines)
            {
                int x = (state.TwoDWidth - line.Width * state.CleanX) / 2;
                Step(state, "text:" + font + ":" + Red + ":" + line.Text + ":" + x + ":" + (pausePos + y));
                y += height * state.CleanY;
            }
        }
    }

    private static void DrawIcon(State state)
    {
        if (state.DrawIcon == null)
            return;
        string name = state.DrawIcon;
        state.DrawIcon = null;
        Pic? pic = state.FindIcon != null ? state.FindIcon(name) : null;
        if (pic is Pic tex)
        {
            int x = 160 - tex.Width / 2;
            int y = 100 - tex.Height / 2;
            Step(state, "icon:" + x + ":" + y);
        }

        state.NoWipe = IconWipeBlock;
    }

    private static void Finish(State state)
    {
        bool direct;
        int wipeType = 0;
        if (state.WipeStart == 0 || state.NoWipe < 0)
            direct = true;
        else if (state.WipeType is not int typed)
            throw new InvalidOperationException("wipe");
        else
        {
            wipeType = typed;
            direct = typed == WipeNone || state.HudToggled;
        }

        if (direct)
        {
            if (state.WipeStart != 0)
            {
                state.DecRefs++;
                state.WipeStart = 0;
            }

            Step(state, "overlays");
            Step(state, "end");
            if (state.TraceWindow && state.TraceFrame % 10 == 0)
                Step(state, "direct:" + state.TraceFrame + ":" + state.GameTic);
        }
        else
        {
            state.LastWipeEnd = state.ReadWipeEnd != null ? state.ReadWipeEnd() : 0;
            state.LastWipeType = wipeType;
            state.WipeStopSound = false;
            Step(state, "wipe:" + state.WipeStart + ":" + state.LastWipeEnd + ":" + wipeType);
            Step(state, "overlays");
            if (state.TraceWindow && state.TraceFrame % 10 == 0)
                Step(state, "wipelog:" + state.TraceFrame + ":" + state.GameTic);
        }

        state.Unclocked = true;
        state.StoredCycles = state.CycleValue;
        Step(state, "unclock");
    }

    private static void Begin(State state)
    {
        state.Began2D = true;
        state.TwoDWidth = state.Width;
        state.TwoDHeight = state.Height;
    }

    private static IReadOnlyList<Broken> Lines(State state, string text, int maxWidth)
    {
        if (state.BreakLines != null)
            return state.BreakLines(text, maxWidth);
        return new[] { new Broken(text, text.Length) };
    }

    private static void Step(State state, string step) => state.Steps.Add(step);
}
