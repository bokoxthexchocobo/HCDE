using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmDMain1423SliceTests
{
    [Fact]
    public void DefinitionsMenusAndEarlyExits()
    {
        Assert.Equal(311, VmDMain1423Slice.BandEnd - VmDMain1423Slice.BandStart + 1);
        Assert.Equal(4188, VmDMain1423Slice.ResourceEnd);
        Assert.Equal(4507, VmDMain1423Slice.DoomMain);
        var loaded = new VmDMain1423Slice.State
        {
            StartScreen = true,
            Config = "Heretic",
            SkipBexStrings = true,
            SoundsBefore = 4,
            SoundsAfter = 7,
            FoundDeh = false,
            FoundBex = false,
            SectionOpens = true,
            Patches =
            {
                new() { Key = "PATH", Value = "a.deh", Exists = true },
                new() { Key = "Size", Value = "1", Exists = true },
                new() { Key = "path", Value = "missing.deh", Exists = false },
                new() { Key = "Path", Value = "b.bex", Exists = true },
            },
            Bots = new List<string> { "one", "two" },
            StartupTexts = new string?[] { "Hello", "", null, "World", "!" },
        };
        Assert.Equal(0, VmDMain1423Slice.Launch(loaded));
        Assert.Equal(new[] { "GL definitions" }, loaded.Phases);
        Assert.Contains("R_Init: Init Heretic refresh subsystem.", loaded.Status);
        Assert.Contains(("Loading graphics", 0x3f), loaded.Loading);
        Assert.Contains(("Init game engine", 0x3f), loaded.Loading);
        Assert.Equal(VmDMain1423Slice.SkipBex, loaded.IwadFlags);
        Assert.Equal(1, loaded.PwadLoads);
        Assert.Equal(new[] { "a.deh", "b.bex" }, loaded.DehFiles);
        Assert.Contains("Applying patch a.deh\n", loaded.Lines);
        Assert.Contains("Applying patch b.bex\n", loaded.Lines);
        Assert.Equal(1, loaded.LockedSounds);
        Assert.Equal(1, loaded.Symbols);
        Assert.Equal(new[] { "one", "two" }, loaded.Spawned);
        Assert.Equal(0, loaded.Tries);
        Assert.Equal(2, loaded.Wanted);
        Assert.Equal(5, loaded.StringChecks);
        Assert.Contains("Hello\n", loaded.Lines);
        Assert.Contains("World\n", loaded.Lines);
        Assert.DoesNotContain(loaded.Lines, line => line == "\n");
        Assert.Equal(1, loaded.NoSet);
        Assert.Equal(VmDMain1423Slice.Startup, loaded.GameState);
        Assert.Equal(1, loaded.Engine);
        Assert.Equal(4097, loaded.WindowAdvance);
        var same = new VmDMain1423Slice.State { SoundsBefore = 3, SoundsAfter = 3, Debugging = true, Bots = new() };
        Assert.Equal(0, VmDMain1423Slice.Launch(same));
        Assert.Equal(0, same.LockedSounds);
        Assert.Equal(0, same.Symbols);
        Assert.Equal(0, same.Wanted);
        var heretic = new VmDMain1423Slice.State
        {
            GameType = 2,
            SectionOpens = true,
            Patches = { new() { Key = "Path", Value = "x.deh", Exists = true } },
        };
        VmDMain1423Slice.Launch(heretic);
        Assert.Empty(heretic.DehFiles);
        var found = new VmDMain1423Slice.State
        {
            FoundBex = true,
            SectionOpens = true,
            Patches = { new() { Key = "Path", Value = "x.deh", Exists = true } },
        };
        VmDMain1423Slice.Launch(found);
        Assert.Empty(found.DehFiles);
        var closed = new VmDMain1423Slice.State
        {
            SectionOpens = false,
            Patches = { new() { Key = "Path", Value = "x.deh", Exists = true } },
        };
        VmDMain1423Slice.Launch(closed);
        Assert.Empty(closed.DehFiles);
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { Config = null }));
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { WindowMissing = true }));
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { LevelMissing = true }));
        var cleared = new VmDMain1423Slice.State { Bots = null };
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(cleared));
        Assert.Equal(1, cleared.Cleared);
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State
        {
            Patches = { new() { Key = null, Value = "x.deh", Exists = true } },
            SectionOpens = true,
        }));
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State
        {
            Patches = { new() { Key = "Path", Value = null, Exists = true } },
            SectionOpens = true,
        }));
        var net = new VmDMain1423Slice.State { StartScreen = true, NetOk = false };
        Assert.Equal(0, VmDMain1423Slice.Launch(net));
        Assert.Equal(1, net.NetChecks);
        Assert.Equal(0, net.NoSet);
        Assert.Equal(-1, net.GameState);
        Assert.Equal(0, net.Engine);
        Assert.Contains(("Checking network game status.", 0x3f), net.Loading);
        Assert.False(net.ScreenDeleted);
        var batch = new VmDMain1423Slice.State
        {
            Batch = true,
            VideoInitialized = false,
            FirstShader = true,
            NoStartScreen = false,
            MaxProgress = 8,
        };
        Assert.Equal(VmDMain1423Slice.SpecialExit, VmDMain1423Slice.Launch(batch));
        Assert.DoesNotContain(batch.Lines, line => line.StartsWith("Decal", StringComparison.Ordinal));
        Assert.DoesNotContain(batch.Lines, line => line.StartsWith("M_Init", StringComparison.Ordinal));
        Assert.DoesNotContain(batch.Lines, line => line.StartsWith("V_Init", StringComparison.Ordinal));
        Assert.Equal(0, batch.StringChecks);
        Assert.Equal(1, batch.LateVideo);
        Assert.Contains("late video initialization", batch.Guards);
        Assert.Equal(1, batch.Palette);
        Assert.Equal(11, batch.OverlayArg);
        Assert.True(batch.SetMode);
        Assert.True(batch.Rendered);
        Assert.Equal(-1, batch.GameState);
        Assert.Equal(0, batch.Engine);
        Assert.Equal(1, batch.WindowAdvance);
    }

    [Fact]
    public void WarpTitleSaveAndRestart()
    {
        var warp = new VmDMain1423Slice.State
        {
            StartScreen = true,
            FirstShader = true,
            AutoStart = true,
            Record = "demo1",
            Haptics = true,
            ShaderFails = 2,
            Width = 640,
            Height = 480,
            FullscreenAspect = true,
            StartMap = "E1M1",
            StoredWarp = "map e1m2",
            MaxProgress = 10,
        };
        Assert.Equal(0, VmDMain1423Slice.Launch(warp));
        Assert.True(warp.AutoStart);
        Assert.Equal(1, warp.Recorded);
        Assert.Equal(VmDMain1423Slice.Body, warp.SoundChannel);
        Assert.Equal(VmDMain1423Slice.Ui | VmDMain1423Slice.Rumble, warp.SoundFlags);
        Assert.Equal("misc/startupdone", warp.SoundName);
        Assert.Equal(1, warp.SoundVolume);
        Assert.Equal(0d, warp.SoundAtten);
        Assert.Equal(VmDMain1423Slice.TicRate, warp.NoWipe);
        Assert.Equal("E1M1", warp.Warped);
        Assert.Equal(new[] { "map e1m2" }, warp.Commands);
        Assert.Equal("", warp.StoredWarp);
        Assert.Equal(VmDMain1423Slice.MapWarp, warp.GameAction);
        Assert.True(warp.InitNew);
        Assert.True(warp.ScreenDeleted);
        Assert.Equal(3, warp.ShaderCalls);
        Assert.True(warp.Began);
        Assert.True(warp.Ended);
        Assert.Equal(1, warp.Joystick);
        Assert.Equal(1, warp.Vr);
        Assert.Equal(1, warp.Jobs);
        Assert.Equal(11, warp.WindowAdvance);
        Assert.Contains("V_Init: allocate screen.\n", warp.Lines);
        Assert.Contains("Init complete.", warp.Status);
        var quiet = new VmDMain1423Slice.State { Dedicated = true, FirstShader = false, AutoStart = true, Haptics = false, StoredWarp = "say hi", StartMap = "MAP02" };
        Assert.Equal(0, VmDMain1423Slice.Launch(quiet));
        Assert.Null(quiet.SoundName);
        Assert.Equal(0, quiet.ShaderCalls);
        Assert.Equal(0, quiet.Palette);
        Assert.Equal(0, quiet.Delayed);
        Assert.False(quiet.Began);
        Assert.Contains("Dedicated server ignored startup command: say hi\n", quiet.Lines);
        Assert.Equal(VmDMain1423Slice.MapWarp, quiet.GameAction);
        Assert.DoesNotContain(quiet.Lines, line => line.StartsWith("V_Init", StringComparison.Ordinal));
        var wipe = new VmDMain1423Slice.State { NetGame = true, WarpWipe = true, StartMap = "MAP03", DemoRecording = true };
        VmDMain1423Slice.Launch(wipe);
        Assert.Equal(-1, wipe.NoWipe);
        Assert.Equal("MAP03", wipe.RecordedMap);
        Assert.False(wipe.RecordedNull);
        var title = new VmDMain1423Slice.State { Multiplayer = true };
        VmDMain1423Slice.Launch(title);
        Assert.Equal(1, title.Titles);
        Assert.Equal(0, title.FrameTime);
        var intro = new VmDMain1423Slice.State { Intro = "TITLEMAP", CutsceneStarts = false };
        VmDMain1423Slice.Launch(intro);
        Assert.Equal(1, intro.FrameTime);
        Assert.Equal("TITLEMAP", intro.Cutscene);
        Assert.Equal(VmDMain1423Slice.BlockUi, intro.CutsceneBlock);
        Assert.Equal(1, intro.Titles);
        var scene = new VmDMain1423Slice.State { CutsceneStarts = true, InvokeCallback = true, Intro = "INTRO" };
        VmDMain1423Slice.Launch(scene);
        Assert.Equal(0, scene.Titles);
        Assert.Equal(VmDMain1423Slice.TitleLoop, scene.GameAction);
        var held = new VmDMain1423Slice.State { CutsceneStarts = true, InvokeCallback = false };
        VmDMain1423Slice.Launch(held);
        Assert.Equal(0, held.GameAction);
        Assert.Equal(0, held.Titles);
        var skip = new VmDMain1423Slice.State { NoIntroParm = true };
        VmDMain1423Slice.Launch(skip);
        Assert.Equal(1, skip.Titles);
        Assert.Equal(0, skip.FrameTime);
        var save = new VmDMain1423Slice.State { LoadSlot = "1", BuiltSave = "save1.zds", SaveExists = true, AutoStart = true, DemoRecording = true };
        VmDMain1423Slice.Launch(save);
        Assert.Equal("save1.zds", save.LoadedSave);
        Assert.Equal(VmDMain1423Slice.LoadGame, save.GameAction);
        Assert.False(save.InitNew);
        Assert.True(save.RecordedNull);
        Assert.Null(save.RecordedMap);
        var missing = new VmDMain1423Slice.State { LoadSlot = "9", BuiltSave = "gone.zds", SaveExists = false };
        var fatal = Assert.Throws<VmDMain1423Slice.Fatal>(() => VmDMain1423Slice.Launch(missing));
        Assert.Equal("Cannot find savegame gone.zds", fatal.Message);
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { LoadSlot = "1", BuiltSave = null }));
        var demo = new VmDMain1423Slice.State { PlayDemo = "", TimeDemo = "skip", AutoStart = true };
        VmDMain1423Slice.Launch(demo);
        Assert.True(demo.SingleDemo);
        Assert.Equal("", demo.Played);
        Assert.Null(demo.Timed);
        Assert.False(demo.InitNew);
        var timed = new VmDMain1423Slice.State { TimeDemo = "bench" };
        VmDMain1423Slice.Launch(timed);
        Assert.Equal("bench", timed.Timed);
        Assert.False(timed.SingleDemo);
        var hide = new VmDMain1423Slice.State { GameAction = VmDMain1423Slice.LoadGameHide, DemoRecording = false, AutoStart = true };
        VmDMain1423Slice.Launch(hide);
        Assert.False(hide.InitNew);
        Assert.False(hide.RecordedNull);
        Assert.Equal(VmDMain1423Slice.LoadGameHide, hide.GameAction);
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { AutoStart = true, StartMap = null }));
        var restart = new VmDMain1423Slice.State
        {
            Restart = true,
            FirstShader = false,
            VideoInitialized = true,
            NoStartScreen = false,
            SetMode = true,
            CustomizeInvul = true,
            ScreenMissing = false,
        };
        Assert.Equal(0, VmDMain1423Slice.Launch(restart));
        Assert.Equal(0, restart.NetChecks);
        Assert.Equal(1, restart.Palette);
        Assert.Equal(0, restart.LateVideo);
        Assert.Equal(0, restart.ShaderCalls);
        Assert.False(restart.SetMode);
        Assert.Equal(1, restart.Titles);
        Assert.Equal(1, restart.ModeAdjust);
        Assert.Equal(1, restart.Invul);
        Assert.Equal(VmDMain1423Slice.Startup, restart.GameState);
        Assert.Equal(1, restart.Delayed);
        Assert.Equal(1, restart.WindowAdvance);
        Assert.Contains("V_Init: allocate screen.\n", restart.Lines);
        var norun = new VmDMain1423Slice.State { NoRun = true, NoStartScreen = true, FirstShader = false, VideoInitialized = false };
        Assert.Equal(VmDMain1423Slice.SpecialExit, VmDMain1423Slice.Launch(norun));
        Assert.Equal(0, norun.LateVideo);
        Assert.Equal(0, norun.ShaderCalls);
        Assert.Equal(0, norun.Palette);
        Assert.Equal(0, norun.Engine);
        var shader = new VmDMain1423Slice.State { FirstShader = false, VideoInitialized = true, NoStartScreen = true };
        VmDMain1423Slice.Launch(shader);
        Assert.Equal(2, shader.ShaderCalls);
        Assert.Equal(0, shader.Palette);
        Assert.True(shader.FirstShader);
        Assert.Throws<NullReferenceException>(() => VmDMain1423Slice.Launch(new VmDMain1423Slice.State { Restart = true, ScreenMissing = true }));
        var per = new VmDMain1423Slice.State { NoStartScreen = false, StartScreen = false, PerShader = 2, MaxProgress = 9, NullOverlay = true, Batch = true };
        Assert.Equal(VmDMain1423Slice.SpecialExit, VmDMain1423Slice.Launch(per));
        Assert.Equal(10, per.OverlayArg);
        Assert.True(per.SetMode);
        Assert.False(per.Rendered);
        Assert.False(per.StartScreen);
        Assert.Contains("startup overlay", per.Guards);
    }
}
