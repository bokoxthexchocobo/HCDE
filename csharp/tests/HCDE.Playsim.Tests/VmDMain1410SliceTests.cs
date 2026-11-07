using S = HCDE.Playsim.VmDMain1410Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1410SliceTests
{
    [Fact]
    public void CapsPaletteAndFpsMatchTheRendererAndTheSecondBoundary()
    {
        var state = new S.State();
        Assert.Equal(914, S.BandStart);
        Assert.Equal(1182, S.BandEnd);
        Assert.Equal(1192, S.DisplayStart);
        Assert.Equal(1503u, S.HardwareBase);
        state.TrueColor = true;
        Assert.Equal(2080u, S.GetCaps(state));
        state.TrueColor = false;
        Assert.Equal(544u, S.GetCaps(state));
        state.DrawVoxels = true;
        Assert.Equal(4640u, S.GetCaps(state));
        state.Hardware = true;
        state.TrueColor = true;
        state.DrawVoxels = false;
        state.ToneMap = 5;
        Assert.Equal(1503u, S.GetCaps(state));
        state.ToneMap = 4;
        Assert.Equal(3551u, S.GetCaps(state));
        state.ToneMap = 5;
        state.DrawVoxels = true;
        Assert.Equal(5599u, S.GetCaps(state));

        state.Height = 100;
        state.BaseColors = new int[256];
        for (int i = 0; i < 256; i++)
            state.BaseColors[i] = 7;
        S.DrawPaletteTester(state, 1);
        Assert.Equal(256, state.Dims.Count);
        Assert.False(state.Dims[0].Assigned);
        Assert.Equal(0, state.Dims[0].Color);
        Assert.Equal(0, state.Dims[0].X);
        Assert.Equal(2, state.Dims[1].X);
        Assert.Equal(2, state.Dims[16].Y);
        Assert.Equal(2, state.Dims[0].Size);
        state.Dims.Clear();
        state.BaseColors = Array.Empty<int>();
        Assert.Throws<IndexOutOfRangeException>(() => S.DrawPaletteTester(state, 0));
        Assert.Empty(state.Dims);
        state.BaseColors = new int[256];
        var translation = new int[256];
        for (int i = 0; i < 256; i++)
            translation[i] = i + 10;
        state.Translations[0] = translation;
        S.DrawPaletteTester(state, 2);
        Assert.True(state.Dims[0].Assigned);
        Assert.Equal(10, state.Dims[0].Color);
        Assert.Equal(265, state.Dims[255].Color);
        Assert.Throws<InvalidOperationException>(() => S.DrawPaletteTester(state, 3));

        var fps = new S.State { TimeScale = 1f };
        fps.FrameMs = 500;
        S.CalcFps(fps);
        Assert.Equal(0ul, fps.LastFps);
        Assert.Equal(1ul, fps.FrameCount);
        Assert.Equal(500ul, fps.LastMsCount);
        ulong frozen = fps.FrameCount;
        S.CalcFps(fps);
        Assert.Equal(frozen, fps.FrameCount);
        fps.FrameMs = 1500;
        S.CalcFps(fps);
        Assert.Equal(1ul, fps.LastFps);
        Assert.Equal(1ul, fps.LastSec);
        Assert.Equal(1ul, fps.FrameCount);
        Assert.Equal(1000ul, fps.LastMsCount);
        fps.FrameMs = 1600;
        S.CalcFps(fps);
        Assert.Equal(1ul, fps.LastFps);
        Assert.Equal(2ul, fps.FrameCount);
        fps.FrameMs = 2500;
        Assert.Equal("900 ms (  2 fps)", S.FpsStat(fps));
        Assert.Equal(2ul, fps.LastFps);
        var fast = new S.State { TimeScale = 2f, FrameMs = 1 };
        S.CalcFps(fast);
        Assert.Equal(0ul, fast.FrameCount);
        fast.FrameMs = 1000;
        S.CalcFps(fast);
        Assert.Equal(0ul, fast.LastFps);
        Assert.Equal(500ul, fast.LastMsCount);
        Assert.Throws<InvalidOperationException>(() => S.CalcFps(new S.State { TimeScale = 0f }));
    }

    [Fact]
    public void RateTickerLagAndOverlayOrder()
    {
        var state = new S.State
        {
            Width = 100,
            Height = 40,
            TextScale = 2,
            FontHeight = 10,
            BlackIndex = 4,
            TimeScale = 1f,
            FrameMs = 1000,
            StringWidth = text => 6,
        };
        S.DrawRateStuff(state);
        Assert.Empty(state.Lines);
        Assert.Empty(state.Rects);
        state.VidFps = true;
        S.DrawRateStuff(state);
        Assert.Equal("1000 ms (  0 fps)", Assert.Single(state.Lines).Text);
        Assert.Equal(S.White, state.Lines[0].Color);
        Assert.Equal(44, state.Lines[0].X);
        Assert.Equal(50, state.Lines[0].VirtualWidth);
        Assert.Equal(20, state.Lines[0].VirtualHeight);
        Assert.Equal(new S.Rect(88, 0, 100, 20, 4), Assert.Single(state.Rects));

        state.Lines.Clear();
        state.Rects.Clear();
        state.VidFps = false;
        long clock = 5;
        state.Ticker = true;
        state.Now = () => clock;
        S.DrawRateStuff(state);
        Assert.Equal(5, state.LastTic);
        Assert.Equal(20, state.Rects.Count);
        Assert.Equal(39, state.Rects[0].Y);
        Assert.Equal(255, state.Rects[0].Color);
        Assert.Equal(255, state.Rects[4].Color);
        Assert.Equal(0, state.Rects[5].Color);
        Assert.Equal(39, state.Height - 1);
        clock = 100;
        S.DrawRateStuff(state);
        Assert.Equal(20, state.Rects.Count(rect => rect.Color == 255) - 5);
        clock = 90;
        state.Rects.Clear();
        S.DrawRateStuff(state);
        Assert.Equal(20, state.Rects.Count);
        Assert.Equal(0, state.Rects[0].Color);
        state.Ticker = false;
        state.ShowPalette = 2;
        state.Height = 50;
        state.Translations[0] = new int[256];
        int before = state.Dims.Count;
        S.DrawRateStuff(state);
        Assert.Equal(before + 256, state.Dims.Count);

        state.Lines.Clear();
        state.DrawLag = false;
        state.Metrics = null;
        S.DrawLagHud(state);
        Assert.Empty(state.Lines);
        state.DrawLag = true;
        Assert.Throws<InvalidOperationException>(() => S.DrawLagHud(state));
        state.Metrics = new S.Lag
        {
            Backlog = 2,
            State = "ok",
            Available = 3,
            Run = 1,
            Stale = 0,
            Invasion = "Disabled",
            Wave = 4,
            Mirrors = 8,
            PendingSpawns = 1,
            PendingEvents = 2,
            WorldSteps = 9,
            LastMirror = 1.5f,
            AvgMirror = 2f,
            AvgWorld = 3.25f,
            MaxWorld = 4f,
        };
        state.TicDup = 1;
        S.DrawLagHud(state);
        Assert.Equal(S.Green, state.Lines[0].Color);
        Assert.Equal("HCDE lag: ok | backlog 2 | avail 3 run 1 stale 0", state.Lines[0].Text);
        Assert.Equal(12, state.Lines[0].Y);
        Assert.Equal("invasion: Disabled wave 4", state.Lines[1].Text);
        Assert.Equal(S.White, state.Lines[1].Color);
        Assert.Equal(23, state.Lines[1].Y);
        Assert.Equal("mirrors 8 | pending spawn/event 1/2 | world steps 9", state.Lines[2].Text);
        Assert.Equal("perf mirror 1.50/2.00 ms | world 3.25/4.00 ms", state.Lines[3].Text);
        state.Lines.Clear();
        state.Metrics.Backlog = 3;
        S.DrawLagHud(state);
        Assert.Equal(S.Gold, state.Lines[0].Color);
        state.Lines.Clear();
        state.Metrics.Backlog = 36;
        S.DrawLagHud(state);
        Assert.Equal(S.Red, state.Lines[0].Color);
        state.Lines.Clear();
        state.Metrics.Backlog = 0;
        state.Metrics.Stalled = true;
        state.Metrics.Invasion = "disabled";
        S.DrawLagHud(state);
        Assert.Equal(S.Red, state.Lines[0].Color);
        Assert.EndsWith(" STALL", state.Lines[0].Text);
        Assert.Equal("invasion: idle (disabled = not started yet)", state.Lines[1].Text);
        state.Lines.Clear();
        state.Metrics.Invasion = null;
        state.Metrics.Wave = 6;
        S.DrawLagHud(state);
        Assert.Equal("invasion: ? wave 6", state.Lines[1].Text);

        var overlay = new S.State { DrawLag = true, Metrics = new S.Lag { Invasion = null } };
        S.DrawOverlays(overlay);
        Assert.Equal(new[] { "console", "menu", "stat" }, overlay.Trace);
        Assert.Equal(4, overlay.Lines.Count);
        overlay.Trace.Clear();
        overlay.Lines.Clear();
        overlay.HudToggled = true;
        S.DrawOverlays(overlay);
        Assert.Equal(new[] { "console", "menu" }, overlay.Trace);
        int steps = 0;
        S.End2DAndUpdate(overlay, () => steps++);
        Assert.Equal(new[] { "console", "menu", "end", "update", "framedone" }, overlay.Trace);
        Assert.Equal(1, steps);
        overlay.Trace.Clear();
        Assert.Throws<InvalidOperationException>(() => S.End2DAndUpdate(overlay, () => throw new InvalidOperationException("bench")));
        Assert.Equal("end", Assert.Single(overlay.Trace));
        Assert.Throws<ArgumentNullException>(() => S.End2DAndUpdate(overlay, null!));
    }
}
