using S = HCDE.Playsim.VmDMain1411Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1411SliceTests
{
    [Fact]
    public void SkipFocusAndInactiveBlock()
    {
        Assert.Equal(1192, S.BandStart);
        Assert.Equal(1391, S.BandEnd);
        Assert.Equal(1610, S.DisplayEnd);
        Assert.Equal(35, S.TicRate);
        Assert.Equal(12, S.BlockedTraceCap);

        var skipped = new S.State { NoDrawers = true, AppActive = false };
        Assert.False(S.Display(skipped));
        Assert.Equal(1, skipped.GcPasses);
        Assert.Equal(1, skipped.DeltaPasses);
        Assert.Equal(0, skipped.TraceFrame);
        Assert.True(skipped.FirstFrameFocus);
        Assert.Equal(0, skipped.FocusPasses);
        Assert.Empty(skipped.Notes);

        skipped.NoDrawers = false;
        skipped.Screen = false;
        Assert.False(S.Display(skipped));
        Assert.Equal(0, skipped.FocusPasses);

        skipped.Screen = true;
        skipped.Dedicated = true;
        Assert.False(S.Display(skipped));
        skipped.Dedicated = false;
        skipped.DedicatedMode = true;
        Assert.False(S.Display(skipped));
        Assert.Equal(0, skipped.TraceFrame);

        var state = new S.State
        {
            AppActive = false,
            Width = 640,
            Height = 480,
            GameTic = 3,
            ViewActive = true,
            UserGame = true,
            ReadCaps = () => 1503,
        };
        Assert.True(S.Display(state));
        Assert.False(state.FirstFrameFocus);
        Assert.True(state.AppActive);
        Assert.True(state.SawAppActive);
        Assert.Equal(2, state.FocusPasses);
        Assert.Equal(1, state.TraceFrame);
        Assert.True(state.Clocked);
        Assert.Equal(1503u, state.Caps);
        Assert.Equal(3, state.Notes.Count);
        Assert.Equal("pre", state.Notes[0].Kind);
        Assert.Equal(0, state.Notes[0].Frame);
        Assert.False(state.Notes[0].App);
        Assert.Equal(640, state.Notes[0].Width);
        Assert.Equal(480, state.Notes[0].Height);
        Assert.Equal("focus", state.Notes[1].Kind);
        Assert.Equal("forced", state.Notes[2].Kind);
        Assert.DoesNotContain(state.Notes, note => note.Kind == "waiting" || note.Kind == "passed");

        state.AppActive = false;
        int capsCalls = 0;
        state.ReadCaps = () =>
        {
            capsCalls++;
            return 1;
        };
        Assert.False(S.Display(state));
        Assert.Equal(0, capsCalls);
        Assert.False(state.Clocked);
        Assert.Equal(2, state.TraceFrame);
        Assert.Equal(1, state.BlockedTraces);
        Assert.Equal("blocked", state.Notes[^1].Kind);
        Assert.Equal(2, state.Notes[^1].Frame);
        Assert.False(state.Notes[^1].App);
        Assert.Equal(1503u, state.Caps);

        while (state.BlockedTraces < 12)
            Assert.False(S.Display(state));
        int notes = state.Notes.Count;
        Assert.Equal(12, state.BlockedTraces);
        Assert.False(S.Display(state));
        Assert.Equal(12, state.BlockedTraces);
        Assert.Equal(notes, state.Notes.Count);
        Assert.Equal(0, capsCalls);

        state.GameState = S.Startup;
        Assert.True(S.Display(state));
        Assert.True(state.Clocked);
        state.GameState = S.TitleLevel;
        state.AppActive = false;
        Assert.True(S.Display(state));
        state.GameState = S.DemoScreen;
        state.AppActive = false;
        Assert.True(S.Display(state));
        state.GameState = S.Level;
        state.AppActive = false;
        state.ActiveInBackground = true;
        Assert.True(S.Display(state));
        state.ActiveInBackground = false;
        state.AppActive = false;
        state.UserGame = false;
        Assert.True(S.Display(state));

        state.UserGame = true;
        state.AppActive = true;
        state.TraceFrame = 349;
        int before = state.Notes.Count;
        Assert.True(S.Display(state));
        Assert.Equal(350, state.TraceFrame);
        Assert.Equal("passed", state.Notes[^1].Kind);
        Assert.Equal(350, state.Notes[^1].Frame);
        Assert.False(state.Notes[^1].Waiting);
        Assert.True(S.Display(state));
        Assert.Equal(350, state.TraceFrame);
        Assert.Equal(before + 1, state.Notes.Count);
    }

    [Fact]
    public void CameraSizeAndWipeChoice()
    {
        int fovCalls = 0;
        var state = new S.State
        {
            AppActive = true,
            ViewActive = true,
            Mo = 4,
            TimeFrac = 0.5,
            ReadCaps = () => 544,
            ReadFov = (cam, frac) =>
            {
                fovCalls++;
                Assert.Equal(4, cam);
                return 77 + frac;
            },
        };
        Assert.True(S.Display(state));
        Assert.Equal(4, state.Camera);
        Assert.Equal(77.5, state.AppliedFov);
        Assert.True(state.FovApplied);
        Assert.Equal(1, fovCalls);
        Assert.Equal(544u, state.Caps);
        Assert.Equal(1, state.FocusPasses);
        Assert.DoesNotContain(state.Notes, note => note.Kind == "forced");

        state.ViewActive = false;
        state.Camera = null;
        state.Mo = null;
        Assert.True(S.Display(state));
        Assert.Null(state.Camera);
        Assert.False(state.FovApplied);
        Assert.Equal(1, fovCalls);

        state.ViewActive = true;
        state.Mo = 9;
        state.ReadFov = (cam, frac) =>
        {
            fovCalls++;
            Assert.Equal(9, cam);
            Assert.Equal(0.25, frac);
            return 40;
        };
        state.TimeFrac = 0.25;
        Assert.True(S.Display(state));
        Assert.Equal(9, state.Camera);
        Assert.Equal(40, state.AppliedFov);

        state.Camera = 2;
        state.Mo = 9;
        state.ReadFov = (cam, _) =>
        {
            fovCalls++;
            Assert.Equal(2, cam);
            return 90;
        };
        state.SetModeNeeded = true;
        state.SizeNeeded = true;
        state.VidFullscreen = false;
        state.ToggleFullscreen = fullscreen =>
        {
            Assert.False(fullscreen);
            state.Width = 640;
            state.Height = 400;
        };
        Assert.True(S.Display(state));
        Assert.False(state.SetModeNeeded);
        Assert.Equal(1, state.ModePasses);
        Assert.Equal(640, state.ResizedWidth);
        Assert.Equal(400, state.ResizedHeight);
        Assert.Equal(640, state.ViewWidth);
        Assert.Equal(400, state.ViewHeight);
        Assert.False(state.SizeNeeded);
        Assert.Equal(0, state.ViewSizePasses);
        Assert.Equal(2, state.Camera);

        state.SizeNeeded = true;
        state.HasStatusBar = true;
        int executed = 0;
        state.ExecuteSetViewSize = () => executed++;
        Assert.True(S.Display(state));
        Assert.True(state.SizeNeeded);
        Assert.Equal(1, state.ViewSizePasses);
        Assert.Equal(1, executed);
        Assert.Equal(640, state.ViewWidth);

        bool canCalled = false;
        state.NetGame = true;
        state.NoWipe = 5;
        state.GameState = S.Level;
        state.WipeGameState = S.ForceFade;
        state.CanWipe = () =>
        {
            canCalled = true;
            throw new InvalidOperationException("wipe");
        };
        Assert.True(S.Display(state));
        Assert.False(canCalled);
        Assert.Equal(4, state.NoWipe);
        Assert.Equal(0, state.WipeStart);
        Assert.False(state.WipeTypeSet);
        Assert.Equal(S.Level, state.WipeGameState);

        state.NetGame = false;
        state.NoWipe = -3;
        Assert.True(S.Display(state));
        Assert.False(canCalled);
        Assert.Equal(-3, state.NoWipe);
        Assert.False(state.WipeTypeSet);

        state.NoWipe = 0;
        state.CanWipe = () =>
        {
            canCalled = true;
            return false;
        };
        state.WipeGameState = S.ForceBurn;
        Assert.True(S.Display(state));
        Assert.True(canCalled);
        Assert.Equal(0, state.NoWipe);
        Assert.Equal(S.Level, state.WipeGameState);
        Assert.False(state.WipeTypeSet);

        state.CanWipe = () => true;
        state.WipeGameState = S.ForceFade;
        state.VrMode = 0;
        state.RenderMode = S.HardwareMode;
        state.WipeTypeCvar = S.WipeBurn;
        Assert.True(S.Display(state));
        Assert.True(state.WipeTypeSet);
        Assert.Equal(S.WipeFade, state.WipeType);
        Assert.Equal(1, state.WipeStart);
        Assert.Equal(S.Level, state.WipeGameState);

        state.WipeGameState = S.ForceBurn;
        state.GameState = 1;
        Assert.True(S.Display(state));
        Assert.Equal(S.WipeBurn, state.WipeType);
        Assert.Equal(1, state.GameState);

        state.WipeGameState = S.ForceMelt;
        state.GameState = 2;
        Assert.True(S.Display(state));
        Assert.Equal(S.WipeMelt, state.WipeType);

        state.WipeGameState = S.ForceWipe;
        state.GameState = S.Level;
        state.WipeTypeCvar = S.WipeBurn;
        Assert.True(S.Display(state));
        Assert.Equal(S.WipeBurn, state.WipeType);
        Assert.Equal(S.Level, state.WipeGameState);

        state.WipeGameState = S.DemoScreen;
        state.WipeTypeCvar = S.WipeFade;
        Assert.True(S.Display(state));
        Assert.Equal(S.WipeFade, state.WipeType);

        state.VrMode = 1;
        state.RenderMode = S.HardwareMode;
        state.GameState = S.Level;
        state.WipeGameState = 2;
        int starts = state.WipeStarts;
        Assert.True(S.Display(state));
        Assert.Equal(starts, state.WipeStarts);
        Assert.Equal(0, state.WipeStart);
        Assert.False(state.WipeTypeSet);
        Assert.Equal(S.Level, state.WipeGameState);

        state.RenderMode = 3;
        state.WipeGameState = S.Startup;
        state.WipeTypeCvar = S.WipeMelt;
        state.StartWipe = () => 0;
        Assert.True(S.Display(state));
        Assert.Equal(0, state.WipeStart);
        Assert.True(state.WipeTypeSet);
        Assert.Equal(S.WipeMelt, state.WipeType);
        Assert.Equal(S.Level, state.WipeGameState);

        state.StartWipe = null;
        state.WipeGameState = S.Level;
        Assert.True(S.Display(state));
        Assert.Equal(0, state.WipeStart);
        Assert.False(state.WipeTypeSet);
        Assert.Equal(S.Level, state.WipeGameState);

        state.GameState = S.FullConsole;
        state.WipeGameState = S.Level;
        Assert.True(S.Display(state));
        Assert.Equal(0, state.WipeStart);
        Assert.Equal(S.Level, state.WipeGameState);

        state.GameState = S.TitleLevel;
        Assert.True(S.Display(state));
        Assert.Equal(S.Level, state.WipeGameState);
        Assert.False(state.WipeTypeSet);

        state.GameState = S.Level;
        state.WipeGameState = S.ForceFade;
        state.VrMode = 0;
        state.NetGame = true;
        state.NoWipe = 0;
        Assert.True(S.Display(state));
        Assert.Equal(0, state.WipeStart);
        Assert.False(state.WipeTypeSet);
        Assert.Equal(S.Level, state.WipeGameState);

        state.NetGame = false;
        state.NoWipe = 1;
        state.WipeGameState = S.Level;
        state.CanWipe = () => throw new InvalidOperationException("wipe");
        Assert.True(S.Display(state));
        Assert.Equal(0, state.NoWipe);
        Assert.False(state.WipeTypeSet);
    }
}
