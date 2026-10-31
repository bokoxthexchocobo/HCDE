using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// Local echo dump, self-test inputs, and the Windows harness from
/// <c>d_net_diag.cpp</c> (813-1075). Chunk 1351. Process, path, and log
/// reads stay on the host object. Name text is the name table.
/// </summary>
public static class VmNetDiag1351Slice
{
    public const int MaxPlayers = 64;
    public const int PspWeapon = 1;
    public const uint BtAttack = 1;
    public const uint BtUse = 2;
    public const uint BtZoom = 128;
    public const short ForwardRun = 0x3200;
    public const uint NoChangeIndex = 0xFFFFFFFFu;

    public static void EchoDump(EchoWorld1351 world)
    {
        if (world.ConsolePlayer < 0 || world.ConsolePlayer >= MaxPlayers)
            return;
        Player1351 player = world.Players[world.ConsolePlayer] ?? throw new NullReferenceException("player");
        uint readyIndex = 0;
        if (player.Ready != null)
            readyIndex = unchecked((uint)player.Ready.NameIndex);
        uint pendingIndex = 0;
        if (player.Pending != null && !ReferenceEquals(player.Pending, Weapon1351.NoChange))
            pendingIndex = unchecked((uint)player.Pending.NameIndex);
        else if (ReferenceEquals(player.Pending, Weapon1351.NoChange))
            pendingIndex = NoChangeIndex;
        uint stateIndex = 0;
        short tics = 0;
        Sprite1351? sprite = player.Sprites;
        while (sprite != null && sprite.Id != PspWeapon)
            sprite = sprite.Next;
        if (sprite != null && sprite.StateName != null)
        {
            if (world.FindName == null)
                throw new InvalidOperationException("FName");
            stateIndex = unchecked((uint)world.FindName(sprite.StateName));
            tics = unchecked((short)sprite.Tics);
        }
        string readyName = NameText(world.Names, readyIndex);
        string pendingName = pendingIndex == NoChangeIndex ? "WP_NOCHANGE" : NameText(world.Names, pendingIndex);
        string stateName = NameText(world.Names, stateIndex);
        var inv = CultureInfo.InvariantCulture;
        world.Out.Add(string.Format(inv,
            "Local Echo Dump (player={0}, gametic={1}, ClientTic={2}):\n" +
            "  ReadyWeapon: {3} (0x{4:x})\n" +
            "  PendingWeapon: {5} (0x{6:x})\n" +
            "  PSprite PSP_WEAPON state: {7} (tics={8}, 0x{9:x})\n" +
            "  WeaponState: 0x{10:x4}\n" +
            "  playerstate: {11}\n" +
            "  viewheight: {12}\n",
            world.ConsolePlayer, world.GameTic, world.ClientTic,
            readyName, readyIndex,
            pendingName, pendingIndex,
            stateName, tics, stateIndex,
            player.WeaponState,
            player.PlayerState,
            player.ViewHeight.ToString("0.00", inv)));
    }

    public static void ApplySelfTest(UserCmd1351? cmd, int clientTic, int runClient)
    {
        if (runClient == 0)
            return;
        if (cmd == null)
            throw new NullReferenceException("cmd");
        cmd.Buttons = 0;
        cmd.Pitch = 0;
        cmd.Yaw = 0;
        cmd.Roll = 0;
        cmd.Forward = 0;
        cmd.Side = 0;
        cmd.Up = 0;
        int phase = clientTic % 250;
        if (phase < 35)
            cmd.Forward = ForwardRun;
        else if (phase < 70)
            cmd.Buttons |= BtAttack;
        else if (phase < 105)
            cmd.Buttons |= BtZoom;
        else if (phase < 140)
            cmd.Forward = unchecked((short)-ForwardRun);
        else if (phase < 175)
            cmd.Buttons |= BtUse;
        else if (phase < 210)
            cmd.Buttons |= BtAttack;
        else
            cmd.Buttons |= BtUse;
    }

    public static void SelfTest(OsHost1351 host)
    {
        Say(host, "Starting HCDE self-test harness...\n");
        if (!host.Windows)
        {
            Say(host, "Self-test is only supported on Windows platform currently.\n");
            return;
        }
        if (host.ModulePath == null)
        {
            Say(host, "Self-test error: unable to get current executable path.\n");
            return;
        }
        string currentExe = host.ModulePath;
        int lastSlash = LastSlash(currentExe);
        string binDir = lastSlash >= 0 ? currentExe.Substring(0, lastSlash + 1) : "";
        string serverExe = binDir + "hcdeserv.exe";
        Say(host, "Parent executable: " + currentExe + "\n");
        Say(host, "Server executable: " + serverExe + "\n");
        string clientLog = host.AppData + "/hcde_trace.hcde.latest.log";
        string serverLog = host.AppData + "/hcde_trace.hcdeserv.latest.log";
        host.Removed.Add(clientLog);
        host.Removed.Add(serverLog);
        string serverCmd = "\"" + serverExe + "\" -server 2 -port 5029 -iwad doom2.wad +map map01 +net_echo_debug 1 +debugtrace_enable 1";
        Say(host, "Spawning server process...\n");
        if (host.FailSpawn == 1)
        {
            Say(host, "Self-test error: failed to spawn server (error code " + host.SpawnError.ToString(CultureInfo.InvariantCulture) + ").\n");
            return;
        }
        host.Spawned.Add(serverCmd);
        host.Slept.Add(2000);
        string clientCmd = "\"" + currentExe + "\" -join 127.0.0.1:5029 -iwad doom2.wad +net_self_test_run_client 1 +net_echo_debug 1 +debugtrace_enable 1";
        Say(host, "Spawning client process...\n");
        if (host.FailSpawn == 2)
        {
            Say(host, "Self-test error: failed to spawn client (error code " + host.SpawnError.ToString(CultureInfo.InvariantCulture) + ").\n");
            host.Killed.Add("server");
            host.Closed.Add("server-process");
            host.Closed.Add("server-thread");
            return;
        }
        host.Spawned.Add(clientCmd);
        Say(host, "Running deterministic simulation for 12 seconds...\n");
        host.Slept.Add(12000);
        Say(host, "Terminating test processes...\n");
        host.Killed.Add("client");
        host.Killed.Add("server");
        host.Closed.Add("client-process");
        host.Closed.Add("client-thread");
        host.Closed.Add("server-process");
        host.Closed.Add("server-thread");
        Say(host, "Analyzing trace logs...\n");
        if (!host.Logs.TryGetValue(clientLog, out string[]? clientLines) || clientLines == null)
        {
            Say(host, "Self-test FAILED: Client trace log not found at " + clientLog + ".\n");
            return;
        }
        if (!host.Logs.TryGetValue(serverLog, out string[]? serverLines) || serverLines == null)
        {
            Say(host, "Self-test FAILED: Server trace log not found at " + serverLog + ".\n");
            return;
        }
        bool desyncFound = false;
        int desyncCount = 0;
        int clientEvents = 0;
        int serverEvents = 0;
        foreach (string line in clientLines)
        {
            if (line.Contains("playsim.psprite") || line.Contains("playsim.playerstate"))
                clientEvents++;
            if (line.Contains("[PLAYER STATE DESYNC]") || line.Contains("[WEAPON DESYNC]")
                || line.Contains("[PSPRITE DESYNC]") || line.Contains("[VIEWHEIGHT DESYNC]"))
            {
                desyncFound = true;
                desyncCount++;
                Say(host, "Desync found in client log: " + line + "\n");
            }
        }
        foreach (string line in serverLines)
        {
            if (line.Contains("playsim.psprite") || line.Contains("playsim.playerstate"))
                serverEvents++;
        }
        var inv = CultureInfo.InvariantCulture;
        Say(host, "Test results summary:\n");
        Say(host, "  Client events logged: " + clientEvents.ToString(inv) + "\n");
        Say(host, "  Server events logged: " + serverEvents.ToString(inv) + "\n");
        Say(host, "  Desyncs detected: " + desyncCount.ToString(inv) + "\n");
        if (clientEvents == 0)
        {
            Say(host, "Self-test FAILED: Client logged 0 simulation events. Connection may have failed.\n");
            return;
        }
        if (desyncFound)
            Say(host, "Self-test FAILED: Desynchronization detected between client and server.\n");
        else
            Say(host, "Self-test PASSED: All states successfully synchronized with zero desyncs!\n");
    }

    public static string NameText(List<string> names, uint index)
    {
        if (index >= (uint)names.Count)
            return "None";
        return names[(int)index];
    }

    static int LastSlash(string path)
    {
        int slash = path.LastIndexOf('/');
        int back = path.LastIndexOf('\\');
        return slash > back ? slash : back;
    }

    static void Say(OsHost1351 host, string line) => host.Out.Add(line);
}

public sealed class EchoWorld1351
{
    public int ConsolePlayer;
    public int GameTic;
    public int ClientTic;
    public Player1351?[] Players = new Player1351?[VmNetDiag1351Slice.MaxPlayers];
    public List<string> Names = new();
    public Func<string, int>? FindName;
    public List<string> Out = new();
}

public sealed class Player1351
{
    public Weapon1351? Ready;
    public Weapon1351? Pending;
    public Sprite1351? Sprites;
    public ushort WeaponState;
    public byte PlayerState;
    public double ViewHeight;
}

public sealed class Weapon1351
{
    public static readonly Weapon1351 NoChange = new();
    public int NameIndex;
}

public sealed class Sprite1351
{
    public int Id;
    public Sprite1351? Next;
    public string? StateName;
    public int Tics;
}

public sealed class UserCmd1351
{
    public uint Buttons;
    public short Pitch;
    public short Yaw;
    public short Roll;
    public short Forward;
    public short Side;
    public short Up;
}

public sealed class OsHost1351
{
    public bool Windows = true;
    public string? ModulePath;
    public string AppData = "";
    public uint SpawnError;
    public int FailSpawn;
    public List<string> Removed = new();
    public List<string> Spawned = new();
    public List<int> Slept = new();
    public List<string> Killed = new();
    public List<string> Closed = new();
    public Dictionary<string, string[]?> Logs = new();
    public List<string> Out = new();
}
