using S = HCDE.Playsim.VmDMain1412Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1412SliceTests
{
    [Fact]
    public void WorldBranchChoosesTheHudAndSkipsAZeroTic()
    {
        Assert.Equal(1392, S.BandStart);
        Assert.Equal(1610, S.BandEnd);
        Assert.Equal(1619, S.CleanupStart);
        Assert.Equal(3, S.AltHud);
        Assert.Equal(6, S.Red);

        var state = Live();
        state.GameTic = 5;
        state.TraceWindow = true;
        state.TraceFrame = 10;
        state.Camera = 3;
        state.Actor = 4;
        state.CameraHasPlayer = true;
        state.ConsolePlayer = 1;
        state.ViewActive = true;
        state.AltHudOn = true;
        state.Blocks = 11;
        state.TicFrac = 0.5;
        Assert.True(S.Present(state));
        Assert.Equal(12.5, state.FrameTime);
        Assert.Equal(12.5, state.AnimatedAt);
        Assert.Equal(0.25, state.SkyAt);
        Assert.True(state.BeganFrame && state.ClearedClip && state.Began2D);
        Assert.Equal(320, state.TwoDWidth);
        Assert.Equal(200, state.TwoDHeight);
        Assert.True(state.Interpolated);
        Assert.Equal(9, state.ViewSector);
        Assert.True(state.Unclocked);
        Assert.Equal(1, state.StoredCycles);
        Assert.Equal(
            new[]
            {
                "world:10:0:5:3:4:1",
                "post:10:9:0:0",
                "blend:9",
                "border",
                "bottom:3",
                "cross:0.5",
                "call:3:0.5",
                "top:3",
                "chat",
                "overlays",
                "end",
                "direct:10:5",
                "unclock",
            },
            state.Steps);

        state.Steps.Clear();
        state.Automap = true;
        state.ViewActive = false;
        state.TraceFrame = 11;
        Assert.True(S.Present(state));
        Assert.Equal(
            new[]
            {
                "blend:9",
                "map:200",
                "bottom:3",
                "althud",
                "call:3:0.5",
                "top:3",
                "chat",
                "overlays",
                "end",
                "unclock",
            },
            state.Steps);

        state.Steps.Clear();
        state.Automap = false;
        state.ViewActive = true;
        state.AltHudOn = false;
        state.DrawFullHud = true;
        state.TraceFrame = 20;
        Assert.True(S.Present(state));
        Assert.Contains("bottom:1", state.Steps);
        Assert.Contains("call:1:0.5", state.Steps);
        Assert.DoesNotContain(state.Steps, step => step == "althud" || step.StartsWith("cross:"));

        state.Steps.Clear();
        state.DrawFullHud = false;
        Assert.True(S.Present(state));
        Assert.Contains("bottom:2", state.Steps);

        state.Steps.Clear();
        state.AltHudOn = true;
        state.Blocks = 10;
        Assert.True(S.Present(state));
        Assert.Contains("bottom:0", state.Steps);

        state.Steps.Clear();
        state.Blocks = 11;
        state.ViewHeight = 160;
        state.Automap = true;
        state.ViewActive = false;
        state.StatusTop = 168;
        Assert.True(S.Present(state));
        Assert.Contains("map:168", state.Steps);
        Assert.Contains("bottom:0", state.Steps);
        Assert.DoesNotContain("border", state.Steps);

        state.Steps.Clear();
        state.Automap = false;
        state.Camera = null;
        state.CameraHasPlayer = true;
        state.ViewHeight = 200;
        state.ViewActive = true;
        Assert.True(S.Present(state));
        Assert.Contains("bottom:3", state.Steps);
        Assert.DoesNotContain(state.Steps, step => step.StartsWith("cross:"));

        state.Steps.Clear();
        state.HudToggled = true;
        state.TraceWindow = false;
        state.TraceFrame = 10;
        Assert.True(S.Present(state));
        Assert.DoesNotContain(state.Steps, step => step.StartsWith("world:") || step == "blend:9" || step == "chat");
        Assert.Contains("overlays", state.Steps);
        Assert.Contains("unclock", state.Steps);

        var title = Live();
        title.GameState = S.TitleLevel;
        title.GameTic = 2;
        Assert.True(S.Present(title));
        Assert.Contains(title.Steps, step => step.StartsWith("blend:"));

        var quiet = Live();
        quiet.GameTic = 0;
        quiet.Render = (_, _) => throw new InvalidOperationException("render");
        Assert.True(S.Present(quiet));
        Assert.Contains("chat", quiet.Steps);
        Assert.DoesNotContain(quiet.Steps, step => step.StartsWith("blend:"));

        var bare = Live();
        bare.GameTic = 4;
        bare.HasStatusBar = false;
        var missing = Assert.Throws<InvalidOperationException>(() => S.Present(bare));
        Assert.Equal("statusbar", missing.Message);
        Assert.False(bare.Unclocked);
        Assert.True(bare.Interpolated);
    }

    [Fact]
    public void ScreensPauseIconAndWipe()
    {
        var full = Live();
        full.GameState = S.FullConsole;
        full.GameTic = 8;
        full.TraceWindow = true;
        full.TraceFrame = 10;
        full.Render = (_, _) => throw new InvalidOperationException("render");
        Assert.False(S.Present(full));
        Assert.Equal(new[] { "page", "console", "menu", "end" }, full.Steps);
        Assert.False(full.Unclocked);
        Assert.Equal(-1, full.StoredCycles);
        Assert.Equal(12.5, full.AnimatedAt);
        Assert.True(full.BeganFrame);

        var demo = Live();
        demo.GameState = S.DemoScreen;
        demo.TraceWindow = true;
        demo.TraceFrame = 3;
        Assert.True(S.Present(demo));
        Assert.Equal(new[] { "page", "chat", "overlays", "end", "unclock" }, demo.Steps);

        demo.Steps.Clear();
        demo.HudToggled = true;
        Assert.True(S.Present(demo));
        Assert.Equal(new[] { "page", "overlays", "end", "unclock" }, demo.Steps);

        var cut = Live();
        cut.GameState = S.Cutscene;
        Assert.True(S.Present(cut));
        Assert.Equal("job", cut.Steps[0]);
        var intro = Live();
        intro.GameState = S.Intro;
        Assert.True(S.Present(intro));
        Assert.Equal("job", intro.Steps[0]);

        int seen = int.MinValue;
        int maxWidth = -1;
        var paused = Live();
        paused.GameState = 1;
        paused.Paused = 2;
        paused.Multiplayer = true;
        paused.GenericUi = false;
        paused.Names = new List<string> { "Ada", "Sam" };
        paused.PauseText = "HOLD";
        paused.ByText = "by %s";
        paused.CleanX = 2;
        paused.CleanY = 2;
        paused.BigHeight = 8;
        paused.SmallHeight = 4;
        paused.BreakLines = (text, width) =>
        {
            maxWidth = width;
            return new[] { new S.Broken(text, 10) };
        };
        paused.DrawPaused = arg =>
        {
            seen = arg;
            return 0;
        };
        Assert.True(S.Present(paused));
        Assert.Equal(1, seen);
        Assert.Equal(160, maxWidth);
        Assert.Equal(20, paused.PausePos);
        Assert.Contains("text:big:6:HOLD:150:4", paused.Steps);
        Assert.Contains("text:small:6:by Sam:150:24", paused.Steps);

        var skipped = Live();
        skipped.GameState = 1;
        skipped.Paused = 2;
        skipped.DrawPaused = _ => 3;
        Assert.True(S.Present(skipped));
        Assert.Equal(1, skipped.LastPauseArg);
        Assert.DoesNotContain(skipped.Steps, step => step.StartsWith("text:") || step.StartsWith("sign:"));

        var extended = Live();
        extended.GameState = 1;
        extended.PauseExtended = true;
        extended.Multiplayer = true;
        extended.Names = new List<string> { "Ada" };
        extended.DrawPaused = arg =>
        {
            seen = arg;
            return 0;
        };
        Assert.True(S.Present(extended));
        Assert.Equal(-1, seen);
        Assert.Contains(extended.Steps, step => step.StartsWith("text:big:"));
        Assert.DoesNotContain(extended.Steps, step => step.StartsWith("text:small:") || step.StartsWith("text:newsmall:"));

        var closed = Live();
        closed.GameState = 1;
        closed.Paused = 1;
        closed.Menu = 1;
        closed.DrawPaused = _ => throw new InvalidOperationException("pause");
        Assert.True(S.Present(closed));
        Assert.Equal(int.MinValue, closed.LastPauseArg);

        var sign = Live();
        sign.GameState = 1;
        sign.Paused = 1;
        sign.UsePauseString = false;
        sign.PauseSign = new S.Pic(64, 16, 2);
        sign.CleanX = 2;
        sign.CleanY = 2;
        Assert.True(S.Present(sign));
        Assert.Contains("sign:100:4", sign.Steps);
        Assert.Equal(36, sign.PausePos);

        var icon = Live();
        icon.GameState = 1;
        icon.DrawIcon = "STAR";
        icon.FindIcon = name => name == "STAR" ? new S.Pic(20, 10, 0) : null;
        icon.SndDraw = 4;
        icon.WipeStart = 5;
        icon.WipeType = 2;
        icon.NoWipe = -1;
        icon.ReadWipeEnd = () => 8;
        Assert.True(S.Present(icon));
        Assert.Null(icon.DrawIcon);
        Assert.Equal(10, icon.NoWipe);
        Assert.Contains("icon:150:95", icon.Steps);
        Assert.Contains("wave:4", icon.Steps);
        Assert.Contains("wipe:5:8:2", icon.Steps);
        Assert.Equal(5, icon.WipeStart);
        Assert.Equal(0, icon.DecRefs);
        Assert.Equal(false, icon.WipeStopSound);
        Assert.DoesNotContain("end", icon.Steps);
        Assert.Contains("unclock", icon.Steps);

        var missingIcon = Live();
        missingIcon.GameState = 1;
        missingIcon.DrawIcon = "";
        missingIcon.FindIcon = _ => null;
        missingIcon.WipeStart = 3;
        missingIcon.WipeType = S.WipeNone;
        Assert.True(S.Present(missingIcon));
        Assert.Null(missingIcon.DrawIcon);
        Assert.Equal(10, missingIcon.NoWipe);
        Assert.DoesNotContain(missingIcon.Steps, step => step.StartsWith("icon:"));
        Assert.Equal(0, missingIcon.WipeStart);
        Assert.Equal(1, missingIcon.DecRefs);

        var none = Live();
        none.GameState = 1;
        none.WipeStart = 7;
        none.WipeType = S.WipeNone;
        none.NoWipe = 0;
        Assert.True(S.Present(none));
        Assert.Equal(0, none.WipeStart);
        Assert.Equal(1, none.DecRefs);
        Assert.Contains("end", none.Steps);
        Assert.DoesNotContain(none.Steps, step => step.StartsWith("wipe:"));

        var unread = Live();
        unread.GameState = 1;
        unread.WipeStart = 4;
        unread.NoWipe = -1;
        unread.WipeType = null;
        Assert.True(S.Present(unread));
        Assert.Equal(1, unread.DecRefs);

        var hidden = Live();
        hidden.GameState = 1;
        hidden.HudToggled = true;
        hidden.WipeStart = 6;
        hidden.WipeType = 1;
        hidden.NoWipe = 0;
        Assert.True(S.Present(hidden));
        Assert.Equal(0, hidden.WipeStart);
        Assert.DoesNotContain("chat", hidden.Steps);
        Assert.Contains("overlays", hidden.Steps);
    }

    private static S.State Live() => new()
    {
        ReadTime = () => 12.5,
        TimeFrac = 0.25,
        Render = (_, action) => action(),
        RenderView = () => 9,
    };
}
