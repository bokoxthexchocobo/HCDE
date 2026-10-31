using HCDE.Playsim;

namespace HCDE.Playsim.Tests;

public class VmNetDiag1351SliceTests
{
    [Fact]
    public void EchoDumpAndPhases()
    {
        var silent = new EchoWorld1351 { ConsolePlayer = -1 };
        VmNetDiag1351Slice.EchoDump(silent);
        silent.ConsolePlayer = 64;
        VmNetDiag1351Slice.EchoDump(silent);
        Assert.Empty(silent.Out);

        var world = new EchoWorld1351
        {
            ConsolePlayer = 0,
            GameTic = 10,
            ClientTic = 20,
            Names = { "Fist" },
        };
        world.Players[0] = new Player1351 { ViewHeight = 41.5 };
        VmNetDiag1351Slice.EchoDump(world);
        Assert.Contains("ReadyWeapon: Fist (0x0)", world.Out[0]);
        Assert.Contains("PendingWeapon: Fist (0x0)", world.Out[0]);

        var weaponSprite = new Sprite1351 { Id = VmNetDiag1351Slice.PspWeapon, StateName = "See", Tics = 40000 };
        var chain = new Sprite1351 { Id = 0, Next = weaponSprite };
        world.Names.Add("Shotgun");
        world.Names.Add("Unused");
        world.Names.Add("See");
        world.Players[0] = new Player1351
        {
            Ready = new Weapon1351 { NameIndex = 1 },
            Pending = Weapon1351.NoChange,
            Sprites = chain,
            WeaponState = 0xab,
            PlayerState = 3,
            ViewHeight = 41.5,
        };
        world.FindName = text => text == "See" ? 4 : -1;
        world.Out.Clear();
        VmNetDiag1351Slice.EchoDump(world);
        Assert.Equal(
            "Local Echo Dump (player=0, gametic=10, ClientTic=20):\n" +
            "  ReadyWeapon: Shotgun (0x1)\n" +
            "  PendingWeapon: WP_NOCHANGE (0xffffffff)\n" +
            "  PSprite PSP_WEAPON state: None (tics=-25536, 0x4)\n" +
            "  WeaponState: 0x00ab\n" +
            "  playerstate: 3\n" +
            "  viewheight: 41.50\n",
            world.Out[0]);
        Assert.Equal(-25536, unchecked((short)40000));

        world.Names.Add("Canonical");
        world.FindName = _ => 4;
        world.Out.Clear();
        VmNetDiag1351Slice.EchoDump(world);
        Assert.Contains("PSprite PSP_WEAPON state: Canonical (tics=-25536, 0x4)", world.Out[0]);

        world.Players[0]!.Ready = new Weapon1351 { NameIndex = -1 };
        world.Players[0]!.Pending = new Weapon1351 { NameIndex = -1 };
        world.Out.Clear();
        VmNetDiag1351Slice.EchoDump(world);
        Assert.Contains("ReadyWeapon: None (0xffffffff)", world.Out[0]);
        Assert.Contains("PendingWeapon: WP_NOCHANGE (0xffffffff)", world.Out[0]);

        var blocked = new Sprite1351
        {
            Id = VmNetDiag1351Slice.PspWeapon,
            StateName = null,
            Next = new Sprite1351 { Id = VmNetDiag1351Slice.PspWeapon, StateName = "See", Tics = 3 },
        };
        world.Players[0]!.Sprites = blocked;
        world.Players[0]!.Ready = null;
        world.Players[0]!.Pending = null;
        world.FindName = _ => throw new InvalidOperationException("should-not");
        world.Out.Clear();
        VmNetDiag1351Slice.EchoDump(world);
        Assert.Contains("PSprite PSP_WEAPON state: Fist (tics=0, 0x0)", world.Out[0]);

        world.Players[0]!.Sprites = new Sprite1351 { Id = VmNetDiag1351Slice.PspWeapon, StateName = "See", Tics = 3 };
        world.FindName = null;
        Assert.Equal("FName", Assert.Throws<InvalidOperationException>(() => VmNetDiag1351Slice.EchoDump(world)).Message);

        var dirty = new UserCmd1351 { Buttons = 9, Pitch = 1, Yaw = 2, Roll = 3, Forward = 4, Side = 5, Up = 6 };
        VmNetDiag1351Slice.ApplySelfTest(null, 0, 0);
        VmNetDiag1351Slice.ApplySelfTest(dirty, 0, 0);
        Assert.Equal(9u, dirty.Buttons);
        Assert.Equal(4, dirty.Forward);
        Assert.Throws<NullReferenceException>(() => VmNetDiag1351Slice.ApplySelfTest(null, 0, 1));

        AssertPhase(-1, forward: VmNetDiag1351Slice.ForwardRun, buttons: 0);
        AssertPhase(0, forward: VmNetDiag1351Slice.ForwardRun, buttons: 0);
        AssertPhase(34, forward: VmNetDiag1351Slice.ForwardRun, buttons: 0);
        AssertPhase(35, forward: 0, buttons: VmNetDiag1351Slice.BtAttack);
        AssertPhase(69, forward: 0, buttons: VmNetDiag1351Slice.BtAttack);
        AssertPhase(70, forward: 0, buttons: VmNetDiag1351Slice.BtZoom);
        AssertPhase(104, forward: 0, buttons: VmNetDiag1351Slice.BtZoom);
        AssertPhase(105, forward: unchecked((short)-VmNetDiag1351Slice.ForwardRun), buttons: 0);
        AssertPhase(139, forward: unchecked((short)-VmNetDiag1351Slice.ForwardRun), buttons: 0);
        AssertPhase(140, forward: 0, buttons: VmNetDiag1351Slice.BtUse);
        AssertPhase(174, forward: 0, buttons: VmNetDiag1351Slice.BtUse);
        AssertPhase(175, forward: 0, buttons: VmNetDiag1351Slice.BtAttack);
        AssertPhase(209, forward: 0, buttons: VmNetDiag1351Slice.BtAttack);
        AssertPhase(210, forward: 0, buttons: VmNetDiag1351Slice.BtUse);
        AssertPhase(249, forward: 0, buttons: VmNetDiag1351Slice.BtUse);
        AssertPhase(250, forward: VmNetDiag1351Slice.ForwardRun, buttons: 0);
        var negative = new UserCmd1351 { Pitch = 8, Side = 9 };
        VmNetDiag1351Slice.ApplySelfTest(negative, 12, -3);
        Assert.Equal(VmNetDiag1351Slice.ForwardRun, negative.Forward);
        Assert.Equal(0, negative.Pitch);
        Assert.Equal(0, negative.Side);
    }

    [Fact]
    public void SelfTestHarness()
    {
        var other = new OsHost1351 { Windows = false, ModulePath = null };
        VmNetDiag1351Slice.SelfTest(other);
        Assert.Equal(new[]
        {
            "Starting HCDE self-test harness...\n",
            "Self-test is only supported on Windows platform currently.\n",
        }, other.Out);
        Assert.Empty(other.Removed);

        var missing = new OsHost1351 { ModulePath = null };
        VmNetDiag1351Slice.SelfTest(missing);
        Assert.Equal("Self-test error: unable to get current executable path.\n", missing.Out[1]);
        Assert.Empty(missing.Removed);

        var serverFail = new OsHost1351 { ModulePath = "hcde.exe", AppData = "D:\\data", FailSpawn = 1, SpawnError = 5 };
        VmNetDiag1351Slice.SelfTest(serverFail);
        Assert.Equal(new[]
        {
            "D:\\data/hcde_trace.hcde.latest.log",
            "D:\\data/hcde_trace.hcdeserv.latest.log",
        }, serverFail.Removed);
        Assert.Contains("Server executable: hcdeserv.exe\n", serverFail.Out);
        Assert.Contains("Self-test error: failed to spawn server (error code 5).\n", serverFail.Out);
        Assert.Empty(serverFail.Slept);
        Assert.Empty(serverFail.Killed);

        var clientFail = new OsHost1351
        {
            ModulePath = "C:\\bin\\hcde.exe",
            AppData = "D:\\data",
            FailSpawn = 2,
            SpawnError = 87,
        };
        VmNetDiag1351Slice.SelfTest(clientFail);
        Assert.Equal(new[] { "\"C:\\bin\\hcdeserv.exe\" -server 2 -port 5029 -iwad doom2.wad +map map01 +net_echo_debug 1 +debugtrace_enable 1" }, clientFail.Spawned);
        Assert.Equal(new[] { 2000 }, clientFail.Slept);
        Assert.Equal(new[] { "server" }, clientFail.Killed);
        Assert.Equal(new[] { "server-process", "server-thread" }, clientFail.Closed);
        Assert.Contains("Self-test error: failed to spawn client (error code 87).\n", clientFail.Out);
        Assert.DoesNotContain("Analyzing trace logs...\n", clientFail.Out);

        string clientLog = "D:\\data/hcde_trace.hcde.latest.log";
        string serverLog = "D:\\data/hcde_trace.hcdeserv.latest.log";
        var noClient = Host();
        VmNetDiag1351Slice.SelfTest(noClient);
        Assert.Contains("Self-test FAILED: Client trace log not found at " + clientLog + ".\n", noClient.Out);
        Assert.Equal(new[] { 2000, 12000 }, noClient.Slept);
        Assert.Equal(new[] { "client", "server" }, noClient.Killed);

        var noServer = Host();
        noServer.Logs[clientLog] = new[] { "playsim.psprite tick" };
        VmNetDiag1351Slice.SelfTest(noServer);
        Assert.Contains("Self-test FAILED: Server trace log not found at " + serverLog + ".\n", noServer.Out);
        Assert.DoesNotContain(noServer.Out, line => line.Contains("Test results summary"));

        var empty = Host();
        empty.Logs[clientLog] = new[] { "note [WEAPON DESYNC] only" };
        empty.Logs[serverLog] = new[] { "[PSPRITE DESYNC] ignored", "playsim.playerstate server" };
        VmNetDiag1351Slice.SelfTest(empty);
        Assert.Contains("Desync found in client log: note [WEAPON DESYNC] only\n", empty.Out);
        Assert.Contains("  Client events logged: 0\n", empty.Out);
        Assert.Contains("  Server events logged: 1\n", empty.Out);
        Assert.Contains("  Desyncs detected: 1\n", empty.Out);
        Assert.Contains("Self-test FAILED: Client logged 0 simulation events. Connection may have failed.\n", empty.Out);
        Assert.DoesNotContain("Desynchronization detected", string.Join("", empty.Out));

        var pass = Host();
        pass.Logs[clientLog] = new[] { "playsim.psprite ok", "playsim.playerstate ok" };
        pass.Logs[serverLog] = new[] { "playsim.psprite srv" };
        VmNetDiag1351Slice.SelfTest(pass);
        Assert.Equal(new[]
        {
            "\"C:\\bin\\hcdeserv.exe\" -server 2 -port 5029 -iwad doom2.wad +map map01 +net_echo_debug 1 +debugtrace_enable 1",
            "\"C:\\bin\\hcde.exe\" -join 127.0.0.1:5029 -iwad doom2.wad +net_self_test_run_client 1 +net_echo_debug 1 +debugtrace_enable 1",
        }, pass.Spawned);
        Assert.Equal(new[] { "client-process", "client-thread", "server-process", "server-thread" }, pass.Closed);
        Assert.Contains("  Client events logged: 2\n", pass.Out);
        Assert.Contains("  Server events logged: 1\n", pass.Out);
        Assert.Contains("Self-test PASSED: All states successfully synchronized with zero desyncs!\n", pass.Out);

        var bad = Host();
        bad.Logs[clientLog] = new[] { "xx playsim.psprite [VIEWHEIGHT DESYNC] yy", "playsim.playerstate [PLAYER STATE DESYNC]" };
        bad.Logs[serverLog] = System.Array.Empty<string>();
        VmNetDiag1351Slice.SelfTest(bad);
        Assert.Contains("Desync found in client log: xx playsim.psprite [VIEWHEIGHT DESYNC] yy\n", bad.Out);
        Assert.Contains("  Client events logged: 2\n", bad.Out);
        Assert.Contains("  Desyncs detected: 2\n", bad.Out);
        Assert.Contains("Self-test FAILED: Desynchronization detected between client and server.\n", bad.Out);
        Assert.DoesNotContain("Self-test PASSED", string.Join("", bad.Out));
    }

    static void AssertPhase(int tic, short forward, uint buttons)
    {
        var cmd = new UserCmd1351 { Pitch = 4, Yaw = 5, Roll = 6, Side = 7, Up = 8, Buttons = 99, Forward = 3 };
        VmNetDiag1351Slice.ApplySelfTest(cmd, tic, 1);
        Assert.Equal(forward, cmd.Forward);
        Assert.Equal(buttons, cmd.Buttons);
        Assert.Equal(0, cmd.Pitch);
        Assert.Equal(0, cmd.Yaw);
        Assert.Equal(0, cmd.Roll);
        Assert.Equal(0, cmd.Side);
        Assert.Equal(0, cmd.Up);
    }

    static OsHost1351 Host() => new()
    {
        ModulePath = "C:\\bin\\hcde.exe",
        AppData = "D:\\data",
    };
}
