using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1419SliceTests
{
    [Fact]
    public void InputAndUi()
    {
        Assert.Equal(19021, VmDMain1419Slice.DebugPort);
        var state = new VmDMain1419Slice.State();
        state.Capture = VmDMain1419Slice.ClampCapture(-4);
        Assert.Equal(0, state.Capture);
        state.Capture = VmDMain1419Slice.ClampCapture(9);
        Assert.Equal(2, state.Capture);
        state.Capture = VmDMain1419Slice.ClampCapture(1);
        Assert.Equal(1, state.Capture);
        state.DebugServer = true;
        VmDMain1419Slice.VmDebugChanged(state, false);
        Assert.False(state.DebugServer);
        Assert.Equal(1, state.DebugStops);
        Assert.Empty(state.Lines);
        VmDMain1419Slice.VmDebugChanged(state, false);
        Assert.Equal(1, state.DebugStops);
        VmDMain1419Slice.VmDebugChanged(state, true);
        Assert.True(state.VmDebug);
        Assert.Equal(new[]
        {
            "You must restart HCDE for this change to take effect.\n",
            "Note that enabling the debug server will disable JIT compilation.\n",
        }, state.Lines);
        state.Lookspring = false;
        VmDMain1419Slice.MlookRelease(state);
        Assert.Empty(state.Net);
        state.Lookspring = true;
        VmDMain1419Slice.MlookRelease(state);
        VmDMain1419Slice.MlookRelease(state);
        Assert.Equal(new[] { 36, 36 }, state.Net);
        var filter = new VmDMain1419Slice.State { GameType = VmDMain1419Slice.Strife, Flags = VmDMain1419Slice.Shareware | 8 };
        Assert.True(VmDMain1419Slice.ValidFilter(filter, "StrifeTeaser"));
        Assert.True(VmDMain1419Slice.ValidFilter(filter, "strife"));
        Assert.False(VmDMain1419Slice.ValidFilter(filter, "Doom"));
        Assert.True(VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State { GameType = 0 }, "anything"));
        Assert.True(VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State(), "dOoM"));
        Assert.False(VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State(), "Heretic"));
        Assert.True(VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State { GameType = 4 }, "hexen"));
        Assert.True(VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State { GameType = 16 }, "Chex"));
        Assert.Throws<NullReferenceException>(() => VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State { GameType = 3 }, "x"));
        Assert.Throws<NullReferenceException>(() => VmDMain1419Slice.ValidFilter(new VmDMain1419Slice.State(), null));
        var gui = new VmDMain1419Slice.State();
        Assert.False(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Console = VmDMain1419Slice.ConsoleDown;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Console = VmDMain1419Slice.ConsoleFalling;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Console = VmDMain1419Slice.ConsoleUp;
        gui.Chat = true;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Chat = false;
        gui.Menu = VmDMain1419Slice.MenuOn;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Menu = VmDMain1419Slice.MenuNoPause;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        gui.Menu = VmDMain1419Slice.MenuWait;
        Assert.False(VmDMain1419Slice.WantGuiCapture(gui));
        gui.UiProcessors = true;
        Assert.True(VmDMain1419Slice.WantGuiCapture(gui));
        var closed = new VmDMain1419Slice.State { Menu = VmDMain1419Slice.MenuOn, HasLevel = false };
        Assert.True(VmDMain1419Slice.WantGuiCapture(closed));
        Assert.Throws<NullReferenceException>(() => VmDMain1419Slice.WantGuiCapture(new VmDMain1419Slice.State { HasLevel = false }));
        Assert.True(VmDMain1419Slice.WantLeftButton(new VmDMain1419Slice.State { GameState = VmDMain1419Slice.DemoScreen }));
        Assert.True(VmDMain1419Slice.WantLeftButton(new VmDMain1419Slice.State { GameState = VmDMain1419Slice.TitleLevel }));
        Assert.False(VmDMain1419Slice.WantLeftButton(new VmDMain1419Slice.State()));
        var mouse = new VmDMain1419Slice.State
        {
            NetGame = true,
            MoveDebug = 2,
            GameTic = 9,
            ClientTic = 8,
            FreeLook = true,
            Pitch = 1f,
            Yaw = 0.5f,
        };
        mouse.Buttons.Add(VmDMain1419Slice.Mlook);
        var ev = new VmDMain1419Slice.Ev { Type = VmDMain1419Slice.MouseEvent, X = 2, Y = 4 };
        Assert.True(VmDMain1419Slice.Dispatch(mouse, ev));
        Assert.Equal(0, ev.X);
        Assert.Equal(0, ev.Y);
        Assert.Equal(new[] { (1, 64), (0, 16) }, mouse.View);
        Assert.Equal(new[]
        {
            "HCDE direct-mouse pitch gametic=9 clienttic=8 raw=(2,4) look=64 m_pitch=1.0000 freelook=1 mlook=1",
            "HCDE direct-mouse yaw gametic=9 clienttic=8 raw=(2,4) turn=16 m_yaw=0.5000 strafe=0 lookstrafe=0",
        }, mouse.Traces);
        var quiet = new VmDMain1419Slice.State { FreeLook = true, Pitch = 0.01f };
        var small = new VmDMain1419Slice.Ev { Type = VmDMain1419Slice.MouseEvent, X = 0, Y = 1 };
        Assert.True(VmDMain1419Slice.Dispatch(quiet, small));
        Assert.Equal(new[] { (1, 0), (0, 0) }, quiet.View);
        Assert.Empty(quiet.Traces);
        var neg = new VmDMain1419Slice.State { Pitch = 1f, Yaw = 1f };
        var down = new VmDMain1419Slice.Ev { Type = VmDMain1419Slice.MouseEvent, X = -1, Y = -1 };
        neg.Buttons.Add(VmDMain1419Slice.Mlook);
        Assert.True(VmDMain1419Slice.Dispatch(neg, down));
        Assert.Equal(new[] { (1, -16), (0, -16) }, neg.View);
        var held = new VmDMain1419Slice.State();
        held.Buttons.Add(VmDMain1419Slice.Strafe);
        held.Buttons.Add(VmDMain1419Slice.Mlook);
        var stay = new VmDMain1419Slice.Ev { Type = VmDMain1419Slice.MouseEvent, X = 3, Y = 1 };
        Assert.False(VmDMain1419Slice.Dispatch(held, stay));
        Assert.Equal(3, stay.X);
        Assert.Equal(0, stay.Y);
        Assert.Equal(new[] { (1, 16) }, held.View);
        var key = new VmDMain1419Slice.Ev { Type = 1, X = 0, Y = 0 };
        Assert.False(VmDMain1419Slice.Dispatch(new VmDMain1419Slice.State(), key));
        var blocked = new VmDMain1419Slice.State { Handled = true, FreeLook = true };
        var skipped = new VmDMain1419Slice.Ev { Type = VmDMain1419Slice.MouseEvent, X = 4, Y = 4 };
        Assert.False(VmDMain1419Slice.Dispatch(blocked, skipped));
        Assert.Equal(4, skipped.X);
        Assert.Empty(blocked.View);
        Assert.True(VmDMain1419Slice.NetGame(new VmDMain1419Slice.State { NetGame = true }));
        Assert.False(VmDMain1419Slice.NetGame(new VmDMain1419Slice.State()));
        Assert.Throws<NullReferenceException>(() => VmDMain1419Slice.WantNativeMouse(new VmDMain1419Slice.State { HasLevel = false }));
        Assert.True(VmDMain1419Slice.WantNativeMouse(new VmDMain1419Slice.State { RequireMouse = true }));
        Assert.False(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Demo = true, Capture = 2 }));
        Assert.False(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Paused = true }));
        Assert.True(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 0 }));
        Assert.False(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 0, GameState = VmDMain1419Slice.Cutscene }));
        Assert.True(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 1, GameState = VmDMain1419Slice.Cutscene }));
        Assert.False(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 1, GameState = 2 }));
        Assert.True(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 2, GameState = 2 }));
        Assert.True(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = 7 }));
        Assert.False(VmDMain1419Slice.CaptureInGame(new VmDMain1419Slice.State { Capture = -3, GameState = VmDMain1419Slice.Cutscene }));
        var sound = new VmDMain1419Slice.State { Haptics = true };
        VmDMain1419Slice.PlayStartupSound(sound, "misc/menu");
        sound.Haptics = false;
        VmDMain1419Slice.PlayStartupSound(sound, "misc/menu");
        Assert.Equal((4, 32 | 0x40000, "misc/menu", 1f, 0f), sound.Sounds[0]);
        Assert.Equal((4, 32 | 0x80000, "misc/menu", 1f, 0f), sound.Sounds[1]);
        Assert.False(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State()));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { GenericUi = true }));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { LogFont = 1 }));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { DlgFont = 2 }));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { Console = VmDMain1419Slice.ConsoleFalling }));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { Multiplayer = true }));
        Assert.True(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { Menu = VmDMain1419Slice.MenuOn, MenuObject = true }));
        Assert.False(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { Menu = VmDMain1419Slice.MenuOn, MenuObject = true, Conversation = true }));
        Assert.False(VmDMain1419Slice.SpecialUi(new VmDMain1419Slice.State { Menu = VmDMain1419Slice.MenuNoPause, MenuObject = true }));
        Assert.False(VmDMain1419Slice.DisableTextureFilter(new VmDMain1419Slice.State()));
        Assert.True(VmDMain1419Slice.DisableTextureFilter(new VmDMain1419Slice.State { Hardware = false }));
        Assert.True(VmDMain1419Slice.DisableAniso(new VmDMain1419Slice.State { IntelMipmap = true }));
        var screen = new VmDMain1419Slice.State { HasLevel = false };
        VmDMain1419Slice.ScreenSizeChanged(screen);
        Assert.Equal(new[] { "crosshair" }, screen.Screen);
        screen.StatusBar = true;
        screen.HasLevel = true;
        screen.Automap = true;
        screen.Screen.Clear();
        VmDMain1419Slice.ScreenSizeChanged(screen);
        Assert.Equal(new[] { "statusbar", "crosshair", "automap" }, screen.Screen);
    }

    [Fact]
    public void CrashSceneAndText()
    {
        var full = new VmDMain1419Slice.State
        {
            Blocks = 10,
            ScreenW = 320,
            ScreenH = 200,
            ViewX = 10,
            ViewY = 20,
            ViewW = 280,
            ViewH = 160,
        };
        var wide = VmDMain1419Slice.SceneRect(full);
        Assert.Equal(10, wide.Left);
        Assert.Equal(0, wide.Top);
        Assert.Equal(280, wide.Width);
        Assert.Equal(200, wide.Height);
        Assert.Equal(320, wide.BlockWidth);
        full.Blocks = 5;
        var shrunk = VmDMain1419Slice.SceneRect(full);
        Assert.Equal(52, shrunk.Top);
        Assert.Equal(96, shrunk.Height);
        Assert.Equal(160, shrunk.BlockWidth);
        Assert.Equal(280, shrunk.Width);
        full.Blocks = 3;
        full.ScreenH = 30;
        full.ScreenW = 20;
        full.ViewY = 0;
        full.ViewH = 0;
        var masked = VmDMain1419Slice.SceneRect(full);
        Assert.Equal(8, masked.Height);
        Assert.Equal(6, masked.BlockWidth);
        Assert.Equal(26, masked.Top);
        var where = new VmDMain1419Slice.State
        {
            ViewMap = "E1M1",
            LevelName = "Hangar",
            X = 1.5,
            Y = -2,
            Z = 0.25,
            Angle = 90,
            PitchDeg = 1,
            Fps = 35,
        };
        Assert.Equal("", VmDMain1419Slice.Location(new VmDMain1419Slice.State()));
        Assert.Equal("Map E1M1: \"Hangar\",\nx = 1.5000, y = -2.0000, z = 0.2500, angle = 90.0000, pitch = 1.0000\n35 fps\n\n", VmDMain1419Slice.Location(where));
        var named = new VmDMain1419Slice.State();
        named.Names.Add("alpha");
        named.Names.Add("beta");
        Assert.Equal("beta", VmDMain1419Slice.PlayerName(named, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => VmDMain1419Slice.PlayerName(named, 4));
        var hud = new VmDMain1419Slice.State { HudToggled = true };
        VmDMain1419Slice.ConsoleToggled(hud, VmDMain1419Slice.ConsoleDown);
        Assert.Equal(0, hud.HudToggles);
        VmDMain1419Slice.ConsoleToggled(hud, VmDMain1419Slice.ConsoleFalling);
        Assert.Equal(1, hud.HudToggles);
        VmDMain1419Slice.ConsoleToggled(new VmDMain1419Slice.State(), VmDMain1419Slice.ConsoleFalling);
        var speech = new VmDMain1419Slice.State();
        speech.Levels.Add(new VmDMain1419Slice.LevelInfo { Lookup = "Dock", Name = "old" });
        speech.Levels.Add(new VmDMain1419Slice.LevelInfo { Name = "keep" });
        VmDMain1419Slice.LanguageChanged(speech, "es");
        Assert.Equal("Dock", speech.Levels[0].Name);
        Assert.Equal("keep", speech.Levels[1].Name);
        Assert.Equal(1, speech.Titles);
        var crash = new VmDMain1419Slice.State
        {
            Version = "1.2",
            Hash = "abc",
            Args = new List<string> { "hcde", "-file" },
            GameState = VmDMain1419Slice.Level,
            Map = "MAP01",
            ViewActive = true,
            ViewPosX = 1,
            ViewPosY = 2,
            ViewPosZ = 3,
            ViewYaw = 4,
            CrashSuffix = "TRACE",
        };
        crash.Wads.Add("doom2.wad");
        char[] buffer = new char[400];
        VmDMain1419Slice.CrashInfo(crash, buffer, buffer.Length, null);
        Assert.Equal(
            "HCDE version 1.2 (abc)\nCommand line: hcde -file\nWad 0: doom2.wad\n\nCurrent map: MAP01\n\nviewx = 1.000000\nviewy = 2.000000\nviewz = 3.000000\nviewangle = 4.000000TRACE\n",
            VmDMain1419Slice.Text(buffer));
        Assert.Equal("\n", crash.CrashCalls[0].Line);
        var away = new VmDMain1419Slice.State { Version = "1.2", Hash = "abc", GameState = 2 };
        away.Wads.Add("doom2.wad");
        char[] idle = new char[200];
        VmDMain1419Slice.CrashInfo(away, idle, idle.Length, "\r\n");
        Assert.Equal("HCDE version 1.2 (abc)\r\nCommand line:\r\nWad 0: doom2.wad\r\n\r\nNot in a level.\r\n", VmDMain1419Slice.Text(idle));
        var missing = new VmDMain1419Slice.State { Version = "1", Hash = "h", GameState = VmDMain1419Slice.TitleLevel };
        char[] hole = new char[200];
        VmDMain1419Slice.CrashInfo(missing, hole, hole.Length, "\n");
        Assert.Contains("Current map: <unknown>", VmDMain1419Slice.Text(hole));
        Assert.Contains("View not active.", VmDMain1419Slice.Text(hole));
        char[] tiny = new char[20];
        var cut = new VmDMain1419Slice.State { Version = "1.2", Hash = "abc" };
        VmDMain1419Slice.CrashInfo(cut, tiny, tiny.Length, "\n");
        Assert.Equal("HCDE version 1.2 ", VmDMain1419Slice.Text(tiny));
        char[] two = { 'Z', 'Z' };
        VmDMain1419Slice.CrashInfo(cut, two, 2, "\n");
        Assert.Equal('\0', two[0]);
        char[] untouched = { 'Z' };
        VmDMain1419Slice.CrashInfo(cut, untouched, 0, "\n");
        Assert.Equal('Z', untouched[0]);
        VmDMain1419Slice.CrashInfo(cut, null, 8, "\n");
        var buf = new VmDMain1419Slice.CrashBuf { Data = new char[8], End = 4, Fail = 1, Cursor = 1 };
        buf.Data[1] = 'Q';
        VmDMain1419Slice.AppendCrashText(buf, "hello");
        Assert.Equal(1, buf.Cursor);
        Assert.Equal('\0', buf.Data[1]);
        var parked = new VmDMain1419Slice.CrashBuf { Data = new char[4], End = 2, Cursor = 2 };
        parked.Data[2] = 'Q';
        VmDMain1419Slice.AppendCrashText(parked, "more");
        Assert.Equal(2, parked.Cursor);
        Assert.Equal('\0', parked.Data[2]);
        var dead = new VmDMain1419Slice.CrashBuf { Null = true, Data = new char[4], End = 3 };
        VmDMain1419Slice.AppendCrashText(dead, "x");
        Assert.Equal(0, dead.Cursor);
    }
}
