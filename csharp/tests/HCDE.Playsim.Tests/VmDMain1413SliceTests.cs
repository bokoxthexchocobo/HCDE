using S = HCDE.Playsim.VmDMain1413Slice;

namespace HCDE.Playsim.Tests;

public class VmDMain1413SliceTests
{
    [Fact]
    public void CleanupAndDoomLoopCatchTheThreeFailures()
    {
        Assert.Equal(1619, S.BandStart);
        Assert.Equal(1942, S.BandEnd);
        Assert.Equal(1950, S.AdvanceDemoStart);
        Assert.Equal(17, S.FullConsoleAction);
        Assert.Equal(35, S.TicRate);

        var shutdown = new S.State
        {
            GameState = 1,
            GameAction = 5,
            GameTic = 8,
            HasLevel = true,
            DemoRecording = true,
            ConsolePlayer = 0,
            PlayerState = 2,
            Menu = 4,
        };
        S.ErrorCleanup(shutdown, "engine-shutdown");
        Assert.Equal("engine-shutdown", shutdown.CleanupReason);
        Assert.DoesNotContain(shutdown.Lines, line => line.Contains("HCDE error cleanup"));
        Assert.False(shutdown.SaveRestore);
        Assert.Equal(1, shutdown.BotsRemoved);
        Assert.Contains("quit:engine-shutdown", shutdown.Steps);
        Assert.Equal(1, shutdown.DemoChecks);
        Assert.Equal(1, shutdown.BufferClears);
        Assert.Equal(1, shutdown.NewInits);
        Assert.Equal(1, shutdown.MenuClears);
        Assert.Equal(1, shutdown.PlayerInGame0);
        Assert.Equal(S.Live, shutdown.PlayerState);
        Assert.Contains("player=0 PST_REBORN -> PST_LIVE reason=D_QuitNetGame_fallback", shutdown.Steps);
        int bots = shutdown.Steps.IndexOf("bots");
        int quit = shutdown.Steps.IndexOf("quit:engine-shutdown");
        int demo = shutdown.Steps.IndexOf("demo-status");
        int player = shutdown.Steps.IndexOf("player=0 PST_REBORN -> PST_LIVE reason=D_QuitNetGame_fallback");
        int stackAt = shutdown.Steps.IndexOf("vm-stack");
        Assert.True(bots >= 0 && bots < quit && quit < demo && demo < player && player < stackAt);
        Assert.Equal(S.FullConsoleAction, shutdown.GameAction);
        Assert.Equal(4, shutdown.Menu);
        Assert.False(shutdown.InSave);
        Assert.Equal(1, shutdown.VmClears);

        var failed = new S.State
        {
            GameState = S.DemoScreen,
            GameAction = 5,
            GameTic = 3,
            DemoPlayback = true,
            ConsolePlayer = 2,
            Menu = 9,
        };
        S.ErrorCleanup(failed, null);
        Assert.Equal("unspecified", failed.CleanupReason);
        Assert.Contains(
            "HCDE error cleanup: reason=unspecified gamestate=3 gameaction=5 gametic=3\n",
            failed.Lines);
        Assert.Contains("error cleanup reason=unspecified gamestate=3 gameaction=5 gametic=3", failed.Steps);
        Assert.Equal(0, failed.BotsRemoved);
        Assert.Equal(1, failed.DemoChecks);
        Assert.Equal(S.MenuOff, failed.Menu);
        Assert.Equal(1, failed.PlayerInGame0);

        var reserved = new S.State { ConsolePlayer = 0, ReservedSlot = true, PlayerState = 2 };
        S.ErrorCleanup(reserved, "");
        Assert.Equal("unspecified", reserved.CleanupReason);
        Assert.Equal(0, reserved.PlayerInGame0);
        Assert.Equal(2, reserved.PlayerState);
        Assert.Equal(0, reserved.DemoChecks);

        var absent = new S.State { ConsolePlayer = -1 };
        S.ErrorCleanup(absent, "engine-shutdown ");
        Assert.Contains(absent.Lines, line => line.Contains("reason=engine-shutdown "));
        Assert.Equal(0, absent.PlayerInGame0);

        var loop = new S.State
        {
            Subtitle = "x",
            PageId = 5,
            AdvisoryId = 3,
            ConsolePlayer = -1,
            GameTic = 0,
            WantRestart = true,
        };
        S.DoomLoop(loop);
        Assert.Equal(
            new[]
            {
                "entered D_DoomLoop",
                "frame-time",
                "rcon",
                "tics",
                "start-tic",
                "events",
                "display",
                "music-update",
                "restart request detected, exiting D_DoomLoop",
            },
            loop.Steps);
        Assert.Equal(1, loop.CursorPasses);
        Assert.True(loop.NoInterpolate);
        Assert.Equal(-1, loop.PageId);
        Assert.Null(loop.Subtitle);
        Assert.Equal(-1, loop.AdvisoryId);
        Assert.False(loop.WantRestart);

        int frames = 0;
        var paced = new S.State
        {
            ConsolePlayer = 1,
            Genders = new List<string> { "m", "f" },
            GameTic = 0,
        };
        paced.RunTics = () =>
        {
            frames++;
            if (frames == 1)
                paced.GameTic = 4;
            else
                paced.WantRestart = true;
        };
        S.DoomLoop(paced);
        Assert.Equal(2, frames);
        Assert.Equal(1, paced.CursorPasses);
        Assert.Equal(-1, paced.PageId);
        Assert.Equal(2, paced.Steps.Count(step => step == "gender:f"));
        Assert.Single(paced.Steps, step => step == "start-frame");

        var net = new S.State
        {
            ConsolePlayer = -1,
            GameTic = 9,
            NetGame = true,
            HasLevel = true,
            MapName = "MAP01",
        };
        int netFrames = 0;
        net.RunTics = () =>
        {
            netFrames++;
            net.WantRestart = true;
            if (netFrames == 2)
            {
                net.NetGame = false;
                net.DemoPlayback = false;
            }
        };
        S.DoomLoop(net);
        Assert.Equal(2, netFrames);
        Assert.Contains(
            "HCDE ignored engine restart request during netgame at gametic=9 map=MAP01\n",
            net.Lines);
        Assert.Contains("ignored engine restart request during netgame gametic=9 map=MAP01", net.Steps);
        Assert.Contains("restart request detected, exiting D_DoomLoop", net.Steps);
        Assert.Single(net.Steps, step => step == "start-frame");

        var demoNet = new S.State
        {
            ConsolePlayer = -1,
            NetGame = true,
            DemoPlayback = true,
            WantRestart = true,
            HasLevel = false,
        };
        S.DoomLoop(demoNet);
        Assert.Contains("restart request detected, exiting D_DoomLoop", demoNet.Steps);
        Assert.DoesNotContain(demoNet.Lines, line => line.Contains("ignored engine restart"));

        var none = new S.State { ConsolePlayer = -1, GameTic = 2, NetGame = true, HasLevel = false };
        int noneFrames = 0;
        none.RunTics = () =>
        {
            noneFrames++;
            none.WantRestart = true;
            if (noneFrames == 2)
                none.NetGame = false;
        };
        S.DoomLoop(none);
        Assert.Contains(
            "HCDE ignored engine restart request during netgame at gametic=2 map=<none>\n",
            none.Lines);

        var abort = new S.State
        {
            ConsolePlayer = -1,
            Abort = true,
            WantRestart = true,
        };
        string? command = null;
        abort.Command = text => command = text;
        S.DoomLoop(abort);
        Assert.Equal("quickexit", command);
        Assert.True(abort.Abort);
        int quick = abort.Steps.IndexOf("quickexit");
        int exit = abort.Steps.IndexOf("restart request detected, exiting D_DoomLoop");
        Assert.True(quick >= 0 && quick < exit);

        int seen = 0;
        var recovered = new S.State { ConsolePlayer = -1, GameState = 1, GameAction = 4, GameTic = 6 };
        recovered.Display = () =>
        {
            seen++;
            if (seen == 1)
                throw new S.RecoverableError("boom");
        };
        recovered.RunTics = () =>
        {
            if (seen >= 1)
                recovered.WantRestart = true;
        };
        S.DoomLoop(recovered);
        int errorAt = recovered.Steps.IndexOf("CRecoverableError: boom");
        int musicAt = recovered.Steps.IndexOf("music-update");
        Assert.True(errorAt >= 0);
        Assert.True(musicAt > errorAt);
        Assert.Contains("\nHCDE recoverable error: boom\n", recovered.Lines);
        Assert.Contains("\nboom\n", recovered.Bold);
        Assert.Contains("quit:recoverable-error", recovered.Steps);
        Assert.Equal(S.FullConsoleAction, recovered.GameAction);
        Assert.False(recovered.SaveRestore);

        var blank = new S.State { ConsolePlayer = -1 };
        int blanks = 0;
        blank.RunTics = () =>
        {
            blanks++;
            if (blanks == 1)
                throw new S.RecoverableError(null);
            blank.WantRestart = true;
        };
        S.DoomLoop(blank);
        Assert.Contains("CRecoverableError: unknown", blank.Steps);
        Assert.DoesNotContain(blank.Lines, line => line.Contains("HCDE recoverable error"));
        Assert.Contains("quit:recoverable-error", blank.Steps);

        var disk = new S.State { ConsolePlayer = -1 };
        int disks = 0;
        disk.SetFrameTime = () =>
        {
            disks++;
            if (disks == 1)
                throw new S.FilesystemError("");
        };
        disk.RunTics = () =>
        {
            if (disks >= 1)
                disk.WantRestart = true;
        };
        S.DoomLoop(disk);
        Assert.Contains("FileSystemException: ", disk.Steps);
        Assert.Contains("\nHCDE filesystem error: \n", disk.Lines);
        Assert.Contains("\n\n", disk.Bold);
        Assert.Contains("quit:filesystem-error", disk.Steps);
        Assert.DoesNotContain("rcon", disk.Steps.GetRange(0, disk.Steps.IndexOf("FileSystemException: ")));

        var missingDisk = new S.State { ConsolePlayer = -1 };
        int missingDisks = 0;
        missingDisk.RunTics = () =>
        {
            missingDisks++;
            if (missingDisks == 1)
                throw new S.FilesystemError(null);
            missingDisk.WantRestart = true;
        };
        S.DoomLoop(missingDisk);
        Assert.Contains("FileSystemException: unknown", missingDisk.Steps);
        Assert.DoesNotContain(missingDisk.Lines, line => line.Contains("HCDE filesystem error"));

        var aborted = new S.State { ConsolePlayer = -1 };
        int vmFrames = 0;
        aborted.RunTics = () =>
        {
            vmFrames++;
            if (vmFrames == 1)
                throw new S.VmAbort("trace\n");
            aborted.WantRestart = true;
        };
        aborted.MaybePrint = () => aborted.MaybePrints++;
        S.DoomLoop(aborted);
        Assert.Contains("CVMAbortException occurred - stacktrace available", aborted.Steps);
        Assert.Contains("\nHCDE VM abort forced game cleanup.\n", aborted.Lines);
        Assert.Contains("trace\n", aborted.Lines);
        Assert.Contains("trace\n", aborted.Bold);
        Assert.Equal(1, aborted.MaybePrints);
        Assert.Contains("vm-maybe", aborted.Steps);
        Assert.Contains("quit:vm-abort", aborted.Steps);

        var emptyVm = new S.State { ConsolePlayer = -1 };
        int emptyFrames = 0;
        emptyVm.RunTics = () =>
        {
            emptyFrames++;
            if (emptyFrames == 1)
                throw new S.VmAbort("");
            emptyVm.WantRestart = true;
        };
        S.DoomLoop(emptyVm);
        Assert.Contains("\nHCDE VM abort forced game cleanup.\n", emptyVm.Lines);
        Assert.DoesNotContain(emptyVm.Lines, line => line.Length == 0);
        Assert.Contains("", emptyVm.Bold);
        Assert.Equal(0, emptyVm.MaybePrints);
        Assert.Contains("vm-maybe", emptyVm.Steps);

        var other = new S.State { ConsolePlayer = -1 };
        other.RunTics = () => throw new InvalidOperationException("nope");
        var escaped = Assert.Throws<InvalidOperationException>(() => S.DoomLoop(other));
        Assert.Equal("nope", escaped.Message);
        Assert.DoesNotContain(other.Steps, step => step.StartsWith("quit:"));
    }

    [Fact]
    public void PageTickerDrawerAndStrifeDemoSequence()
    {
        var page = new S.State { PageTic = 1, AdvancedDemo = false };
        S.PageTicker(page);
        Assert.Equal(0, page.PageTic);
        Assert.False(page.AdvancedDemo);
        S.PageTicker(page);
        Assert.Equal(-1, page.PageTic);
        Assert.True(page.AdvancedDemo);
        page.AdvancedDemo = false;
        S.AdvanceDemo(page);
        Assert.True(page.AdvancedDemo);

        var drawn = new S.State
        {
            Width = 640,
            Height = 400,
            PageId = 2,
            Subtitle = "HELLO",
            GenericUi = true,
            AdvisoryId = 1,
        };
        S.PageDrawer(drawn);
        Assert.Equal(
            new[]
            {
                "clear:0:0:640:400:0:0",
                "page:full:unmasked:bilinear",
                "subtitle:newsmall:HELLO",
                "advisory:4:160:320x200",
            },
            drawn.Steps);
        drawn.Steps.Clear();
        drawn.GenericUi = false;
        drawn.PageId = -1;
        drawn.Subtitle = "";
        drawn.AdvisoryId = -1;
        S.PageDrawer(drawn);
        Assert.Equal(new[] { "clear:0:0:640:400:0:0", "subtitle:small:" }, drawn.Steps);
        drawn.Steps.Clear();
        drawn.Subtitle = null;
        S.PageDrawer(drawn);
        Assert.Equal(new[] { "clear:0:0:640:400:0:0" }, drawn.Steps);

        var retail = Demo();
        retail.MusicLump = name =>
        {
            Assert.Equal("d_logo", name);
            return 1;
        };
        S.StrifeAdvance(retail);
        Assert.Equal(S.DemoScreen, retail.GameState);
        Assert.Equal(1, retail.DemoSequence);
        Assert.Equal("TITLEPIC", retail.PageName);
        Assert.Equal(8, retail.PageId);
        Assert.Equal(6 * 35, retail.PageTic);
        Assert.False(retail.PageBlank);
        Assert.Equal(new[] { "d_logo" }, retail.Music);
        Assert.Equal(1, retail.HideConsole);
        Assert.Null(retail.Subtitle);
        Assert.Equal(4, retail.WipeState);
        Assert.Empty(retail.Sounds);

        S.StrifeAdvance(retail);
        Assert.Equal(2, retail.DemoSequence);
        Assert.Equal("", retail.PageName);
        Assert.Equal(10, retail.PageTic);
        Assert.True(retail.PageBlank);
        Assert.Equal("2:32:bishop/active:1:1", retail.Sounds[0]);
        Assert.Single(retail.Music);
        Assert.Equal(1, retail.HideConsole);

        retail.Subtitle = "old";
        retail.WipeState = 4;
        S.StrifeAdvance(retail);
        Assert.Equal(3, retail.DemoSequence);
        Assert.Equal("RGELOGO", retail.PageName);
        Assert.Equal(4 * 35, retail.PageTic);
        Assert.False(retail.PageBlank);
        Assert.Null(retail.Subtitle);
        Assert.Equal(4, retail.WipeState);

        S.StrifeAdvance(retail);
        Assert.Equal(4, retail.DemoSequence);
        Assert.Equal("PANEL1", retail.PageName);
        Assert.Equal(7 * 35, retail.PageTic);
        Assert.Equal("$TXT_SUB_INTRO1", retail.Subtitle);
        Assert.Equal("d_intro", retail.Music[^1]);
        Assert.Equal("2:32:svox/pro1:1:1", retail.Sounds[^1]);

        S.StrifeAdvance(retail);
        Assert.Equal("PANEL2", retail.PageName);
        Assert.Equal(9 * 35, retail.PageTic);
        Assert.Equal("$TXT_SUB_INTRO2", retail.Subtitle);
        Assert.Equal("2:32:svox/pro2:1:1", retail.Sounds[^1]);
        S.StrifeAdvance(retail);
        Assert.Equal("PANEL3", retail.PageName);
        Assert.Equal(12 * 35, retail.PageTic);
        Assert.Equal("2:32:svox/pro3:1:1", retail.Sounds[^1]);
        S.StrifeAdvance(retail);
        Assert.Equal("PANEL4", retail.PageName);
        Assert.Equal(11 * 35, retail.PageTic);
        Assert.Equal("2:32:svox/pro4:1:1", retail.Sounds[^1]);
        S.StrifeAdvance(retail);
        Assert.Equal("PANEL5", retail.PageName);
        Assert.Equal(10 * 35, retail.PageTic);
        Assert.Equal("2:32:svox/pro5:1:1", retail.Sounds[^1]);
        S.StrifeAdvance(retail);
        Assert.Equal("PANEL6", retail.PageName);
        Assert.Equal(16 * 35, retail.PageTic);
        Assert.Equal("$TXT_SUB_INTRO6", retail.Subtitle);
        Assert.Equal("2:32:svox/pro6:1:1", retail.Sounds[^1]);
        Assert.Equal(10, retail.DemoSequence);

        S.StrifeAdvance(retail);
        Assert.Equal(11, retail.DemoSequence);
        Assert.Equal("CREDIT", retail.PageName);
        Assert.Equal(12 * 35, retail.PageTic);
        Assert.Equal(S.ForceFade, retail.WipeState);
        Assert.Null(retail.Subtitle);

        S.StrifeAdvance(retail);
        Assert.Equal(0, retail.DemoSequence);
        Assert.Equal("TITLEPIC", retail.PageName);
        Assert.Equal(2, retail.HideConsole);

        var shareware = Demo();
        shareware.GameFlags = S.Shareware;
        shareware.DemoSequence = 8;
        shareware.MusicLump = _ => 0;
        S.StrifeAdvance(shareware);
        Assert.Equal(9, shareware.DemoSequence);
        Assert.Equal("PANEL6", shareware.PageName);
        Assert.Equal("2:32:svox/voc96:1:1", shareware.Sounds[^1]);
        S.StrifeAdvance(shareware);
        Assert.Equal(10, shareware.DemoSequence);
        Assert.Equal("vellogo", shareware.PageName);
        Assert.Equal(6 * 35, shareware.PageTic);
        Assert.Equal(S.ForceFade, shareware.WipeState);
        S.StrifeAdvance(shareware);
        Assert.Equal(11, shareware.DemoSequence);
        Assert.Equal("CREDIT", shareware.PageName);

        var teaser = Demo();
        teaser.GameFlags = S.Shareware | S.Teaser2;
        teaser.DemoSequence = 3;
        teaser.MusicLump = _ => 0;
        S.StrifeAdvance(teaser);
        Assert.Equal("d_fmintr", Assert.Single(teaser.Music));
        Assert.Equal("2:32:svox/voc91:1:1", Assert.Single(teaser.Sounds));
        Assert.Equal(4, teaser.DemoSequence);

        var missing = Demo();
        missing.MusicLump = name =>
        {
            Assert.Equal("d_logo", name);
            return -1;
        };
        S.StrifeAdvance(missing);
        Assert.Equal("", Assert.Single(missing.Music));

        var forced = Demo();
        forced.DemoSequence = 9;
        forced.MusicLump = _ => 1;
        forced.WipeState = 4;
        S.StrifeAdvance(forced);
        Assert.Equal("vellogo", forced.PageName);
        Assert.Equal(S.ForceFade, forced.WipeState);
        Assert.Equal(10, forced.DemoSequence);

        var negative = Demo();
        negative.DemoSequence = -3;
        negative.MusicLump = _ => 1;
        S.StrifeAdvance(negative);
        Assert.Equal("TITLEPIC", negative.PageName);
        Assert.Equal(-2, negative.DemoSequence);
        Assert.Equal(1, negative.HideConsole);
    }

    private static S.State Demo() => new()
    {
        FindPage = name => name.Length,
        Subtitle = "stale",
        WipeState = 4,
    };
}
