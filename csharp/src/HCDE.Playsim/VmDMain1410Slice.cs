using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Renderer caps, the palette tester, the fps counter, the lag HUD, and the
/// 2D overlay close from d_main.cpp 914-1182. Chunk 1410.
/// D_Display starts at line 1192 and stays outside this band.
/// </summary>
public static class VmDMain1410Slice
{
    public const int BandStart = 914;
    public const int BandEnd = 1182;
    public const int DisplayStart = 1192;
    public const int TicRate = 35;
    public const int Green = 3;
    public const int Gold = 5;
    public const int Red = 6;
    public const int White = 9;
    public const uint Unclipped = 1u << 5;
    public const uint Colormap = 1u << 9;
    public const uint TrueColor = 1u << 11;
    public const uint Voxels = 1u << 12;
    public const uint HardwareBase =
        (1u << 0) | (1u << 1) | (1u << 2) | (1u << 3) | (1u << 4)
        | (1u << 6) | (1u << 7) | (1u << 8) | (1u << 10);

    public readonly record struct Dim(bool Assigned, int Color, int X, int Y, int Size);
    public readonly record struct Rect(int X, int Y, int X2, int Y2, int Color);
    public readonly record struct TextLine(int Color, int X, int Y, string Text, int VirtualWidth, int VirtualHeight);

    public sealed class Lag
    {
        public bool Stalled;
        public int Backlog;
        public int Available;
        public int Run;
        public int Stale;
        public int Wave;
        public int Mirrors;
        public int PendingSpawns;
        public int PendingEvents;
        public int WorldSteps;
        public string State = "";
        public string? Invasion;
        public float LastMirror;
        public float AvgMirror;
        public float AvgWorld;
        public float MaxWorld;
    }

    public sealed class State
    {
        public bool Hardware;
        public bool TrueColor;
        public bool DrawVoxels;
        public int ToneMap = 5;
        public int Width = 320;
        public int Height = 200;
        public int TextScale = 1;
        public int FontHeight = 8;
        public int BlackIndex;
        public float TimeScale = 1f;
        public ulong FrameMs;
        public ulong LastMs;
        public ulong LastSec;
        public ulong FrameCount;
        public ulong LastFps;
        public ulong LastMsCount;
        public bool VidFps;
        public bool Ticker;
        public int ShowPalette;
        public long LastTic;
        public int TicDup = 1;
        public bool HudToggled;
        public bool DrawLag;
        public Lag? Metrics;
        public int[] BaseColors = new int[256];
        public readonly Dictionary<int, int[]> Translations = new();
        public Func<string, int> StringWidth = text => text.Length;
        public Func<long> Now = () => 0;
        public readonly List<string> Trace = new();
        public readonly List<Dim> Dims = new();
        public readonly List<Rect> Rects = new();
        public readonly List<TextLine> Lines = new();
    }

    public static uint GetCaps(State state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        uint flags;
        if (!state.Hardware)
            flags = Unclipped | (state.TrueColor ? TrueColor : Colormap);
        else
        {
            flags = HardwareBase;
            if (state.ToneMap != 5)
                flags |= TrueColor;
        }
        if (state.DrawVoxels)
            flags |= Voxels;
        return flags;
    }

    public static void DrawPaletteTester(State state, int paletteNo)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        int block = state.Height / 50;
        int k = 0;
        for (int row = 0; row < 16; row++)
        {
            for (int col = 0; col < 16; col++)
            {
                bool assigned = false;
                int color = 0;
                if (paletteNo > 1)
                {
                    if (!state.Translations.TryGetValue(paletteNo - 2, out int[]? palette) || palette == null)
                        throw new InvalidOperationException("translation");
                    color = palette[k];
                    assigned = true;
                }
                else
                {
                    _ = state.BaseColors[k];
                }
                state.Dims.Add(new Dim(assigned, color, col * block, row * block, block));
                k++;
            }
        }
    }

    public static void CalcFps(State state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (state.TimeScale <= 0f)
            throw new InvalidOperationException("timescale");
        ulong howlong = (ulong)((state.FrameMs - state.LastMs) / (double)state.TimeScale);
        if (unchecked((int)howlong) <= 0)
            return;
        uint thisSec = (uint)(state.FrameMs / 1000);
        if (state.LastSec < thisSec)
        {
            ulong frames = state.FrameCount / (thisSec - state.LastSec);
            state.LastFps = (ulong)(frames * state.TimeScale);
            state.LastSec = thisSec;
            state.FrameCount = 0;
        }
        state.FrameCount++;
        state.LastMs = state.FrameMs;
        state.LastMsCount = howlong;
    }

    public static string FpsStat(State state)
    {
        CalcFps(state);
        return string.Format(CultureInfo.InvariantCulture, "{0,2} ms ({1,3} fps)", state.LastMsCount, state.LastFps);
    }

    public static void DrawRateStuff(State state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (state.VidFps)
        {
            if (state.TextScale == 0)
                throw new DivideByZeroException("text scale");
            string text = FpsStat(state);
            int rateX = state.Width / state.TextScale - state.StringWidth(text);
            state.Rects.Add(new Rect(rateX * state.TextScale, 0, state.Width, state.FontHeight * state.TextScale, state.BlackIndex));
            state.Lines.Add(new TextLine(White, rateX, 0, text, state.Width / state.TextScale, state.Height / state.TextScale));
        }
        if (state.Ticker)
        {
            long now = state.Now();
            long tics = now - state.LastTic;
            state.LastTic = now;
            if (tics > 20)
                tics = 20;
            int i;
            for (i = 0; i < tics * 2; i += 2)
                state.Rects.Add(new Rect(i, state.Height - 1, i + 1, state.Height, 255));
            for (; i < 40; i += 2)
                state.Rects.Add(new Rect(i, state.Height - 1, i + 1, state.Height, 0));
        }
        if (state.ShowPalette != 0)
            DrawPaletteTester(state, state.ShowPalette);
    }

    public static void DrawLagHud(State state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (!state.DrawLag)
            return;
        Lag metrics = state.Metrics ?? throw new InvalidOperationException("metrics");
        if (state.TextScale == 0)
            throw new DivideByZeroException("text scale");
        int color = Green;
        if (metrics.Stalled || metrics.Backlog > TicRate)
            color = Red;
        else if (metrics.Backlog > state.TicDup * 2)
            color = Gold;
        int y = state.FontHeight + 2;
        int lineH = state.FontHeight + 1;
        int virtualW = state.Width / state.TextScale;
        int virtualH = state.Height / state.TextScale;
        AddLag(state, color, y, virtualW, virtualH, string.Format(
            CultureInfo.InvariantCulture,
            "HCDE lag: {0} | backlog {1} | avail {2} run {3} stale {4}{5}",
            metrics.State, metrics.Backlog, metrics.Available, metrics.Run, metrics.Stale,
            metrics.Stalled ? " STALL" : ""));
        y += lineH;
        string invasion = metrics.Invasion == "disabled"
            ? "invasion: idle (disabled = not started yet)"
            : string.Format(CultureInfo.InvariantCulture, "invasion: {0} wave {1}", metrics.Invasion ?? "?", metrics.Wave);
        AddLag(state, White, y, virtualW, virtualH, invasion);
        y += lineH;
        AddLag(state, White, y, virtualW, virtualH, string.Format(
            CultureInfo.InvariantCulture,
            "mirrors {0} | pending spawn/event {1}/{2} | world steps {3}",
            metrics.Mirrors, metrics.PendingSpawns, metrics.PendingEvents, metrics.WorldSteps));
        y += lineH;
        AddLag(state, White, y, virtualW, virtualH, string.Format(
            CultureInfo.InvariantCulture,
            "perf mirror {0:0.00}/{1:0.00} ms | world {2:0.00}/{3:0.00} ms",
            metrics.LastMirror, metrics.AvgMirror, metrics.AvgWorld, metrics.MaxWorld));
    }

    public static void DrawOverlays(State state)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        state.Trace.Add("console");
        state.Trace.Add("menu");
        DrawRateStuff(state);
        DrawLagHud(state);
        if (!state.HudToggled)
            state.Trace.Add("stat");
    }

    public static void End2DAndUpdate(State state, Action bench)
    {
        if (state == null)
            throw new ArgumentNullException(nameof(state));
        if (bench == null)
            throw new ArgumentNullException(nameof(bench));
        state.Trace.Add("end");
        bench();
        state.Trace.Add("update");
        state.Trace.Add("framedone");
    }

    private static void AddLag(State state, int color, int y, int virtualW, int virtualH, string text)
    {
        state.Lines.Add(new TextLine(color, 0, y, text, virtualW, virtualH));
    }
}
