using System.Net;
using HCDE.Net.Pregame;
using HCDE.Protocol;
using HCDE.Playsim;
using HCDE.MapLoader;

namespace HCDE.Server;

public static class DedicatedServerCommandLine
{
    public static bool TryParse(string[] args, out DedicatedServerOptions options, out string? error)
    {
        options = new DedicatedServerOptions();
        error = null;
        string? iwadPath = null;
        var modPaths = new List<string>();
        var mapName = "MAP01";
        var rngSeed = 1;
        var advertiseMaster = false;
        string? masterAddress = null;
        var gameModeNameSpecified = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--help":
                case "-h":
                    return false;
                case "--iwad":
                    if (!TryReadArg(args, ref i, out iwadPath))
                    {
                        error = "--iwad requires a path";
                        return false;
                    }
                    break;
                case "--map":
                    if (!TryReadArg(args, ref i, out mapName))
                    {
                        error = "--map requires a name";
                        return false;
                    }
                    break;
                case "--file":
                    if (!TryReadArg(args, ref i, out var modPath) || string.IsNullOrWhiteSpace(modPath))
                    { error = "--file requires a WAD or PK3 path"; return false; }
                    modPaths.Add(modPath);
                    break;
                case "--port":
                    if (!TryReadArg(args, ref i, out var portText) || !int.TryParse(portText, out var port) || port is <= 0 or > 65535)
                    {
                        error = "--port requires a valid UDP port";
                        return false;
                    }

                    options.Port = port;
                    break;
                case "--bind":
                    if (!TryReadArg(args, ref i, out var bindAddress))
                    {
                        error = "--bind requires an IPv4 address";
                        return false;
                    }

                    options.BindAddress = bindAddress ?? "0.0.0.0";
                    break;
                case "--rng-seed":
                    if (!TryReadArg(args, ref i, out var seedText) || !int.TryParse(seedText, out rngSeed))
                    {
                        error = "--rng-seed requires an integer";
                        return false;
                    }
                    break;
                case "--server-name":
                    if (!TryReadArg(args, ref i, out var serverName) || string.IsNullOrWhiteSpace(serverName))
                    {
                        error = "--server-name requires a non-empty value";
                        return false;
                    }

                    options.ServerName = serverName;
                    break;
                case "--skill":
                    if (!TryReadArg(args, ref i, out var skillText) || !byte.TryParse(skillText, out var skill))
                    {
                        error = "--skill requires a byte value";
                        return false;
                    }

                    options.Skill = skill;
                    break;
                case "--deathmatch":
                    options.Deathmatch = true;
                    break;
                case "--strife-thing-flags":
                    options.ThingFlagFormat = BinaryThingFlagFormat.Strife;
                    break;
                case "--limit-pain":
                    options.Compatibility |= CompatSurface.LimitPain;
                    break;
                case "--hexen-crush-defaults":
                    options.Compatibility |= CompatSurface.HexenCrushDefaults;
                    break;
                case "--teamplay":
                    options.Teamplay = true;
                    break;
                case "--gamemode":
                    if (!TryReadArg(args, ref i, out var gameModeText) || !byte.TryParse(gameModeText, out var gameMode))
                    {
                        error = "--gamemode requires a byte value";
                        return false;
                    }

                    options.GameMode = gameMode;
                    break;
                case "--gamemode-name":
                    if (!TryReadArg(args, ref i, out var gameModeName) || string.IsNullOrWhiteSpace(gameModeName))
                    {
                        error = "--gamemode-name requires a non-empty value";
                        return false;
                    }

                    options.GameModeName = gameModeName;
                    gameModeNameSpecified = true;
                    break;
                case "--invasion-waves":
                    if (!TryReadArg(args, ref i, out var wavesText) || !int.TryParse(wavesText, out var waves) || waves is < 1 or > 65535)
                    {
                        error = "--invasion-waves requires a value from 1 to 65535";
                        return false;
                    }
                    options.InvasionWaves = waves;
                    break;
                case "--invasion-countdown":
                case "--invasion-intermission":
                    if (!TryReadArg(args, ref i, out var secondsText) || !int.TryParse(secondsText, out var seconds) || seconds is < 0 or > 3600)
                    {
                        error = $"{arg} requires seconds from 0 to 3600";
                        return false;
                    }
                    if (arg == "--invasion-countdown")
                        options.InvasionCountdownTics = seconds * GameTicClock.TicRate;
                    else
                        options.InvasionIntermissionTics = seconds * GameTicClock.TicRate;
                    break;
                case "--no-query":
                    options.EnableServerQuery = false;
                    break;
                case "--rcon-password":
                    if (!TryReadArg(args, ref i, out var rconPassword) || string.IsNullOrWhiteSpace(rconPassword))
                    {
                        error = "--rcon-password requires a non-empty value";
                        return false;
                    }

                    options.RconPassword = rconPassword;
                    break;
                case "--rcon-port":
                    if (!TryReadArg(args, ref i, out var rconPortText) || !int.TryParse(rconPortText, out var rconPort) || rconPort is < 0 or > 65535)
                    {
                        error = "--rcon-port requires a TCP port";
                        return false;
                    }

                    options.RconPort = rconPort;
                    break;
                case "--master":
                    advertiseMaster = true;
                    if (i + 1 < args.Length && !args[i + 1].StartsWith('-'))
                        masterAddress = args[++i];
                    break;
                default:
                    error = $"unknown argument: {arg}";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(iwadPath))
        {
            error = "--iwad is required";
            return false;
        }

        if (!File.Exists(iwadPath))
        {
            error = $"IWAD not found: {iwadPath}";
            return false;
        }

        if (!IPAddress.TryParse(options.BindAddress, out _))
        {
            error = $"invalid bind address: {options.BindAddress}";
            return false;
        }

        if (advertiseMaster)
        {
            options.EnableMasterAdvertise = true;
            if (!TryParseMasterAddress(masterAddress, options, out error))
                return false;
        }

        try
        {
            var paths = new[] { iwadPath }.Concat(modPaths).ToArray();
            long total = 0;
            foreach (var path in paths)
            {
                total += new FileInfo(path).Length;
                if (total > WadLoadOrder.MaxBytes) { error = "WAD load order exceeds 256 MiB"; return false; }
            }
            var archives = paths.Select(File.ReadAllBytes).ToArray();
            if (modPaths.Count == 0) options.IwadBytes = archives[0];
            else if (!ModResources.TryLoad(archives, out var resources, out error)) return false;
            else { options.IwadBytes = resources.MapWad; options.Resources = resources; }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        { error = exception.Message; return false; }
        if (options.GameMode == 4 && !gameModeNameSpecified)
            options.GameModeName = "Invasion";
        options.Pregame = new PregameHostOptions
        {
            Session = new PregameSessionSnapshot
            {
                MapLoad = new MapLoadInfo { MapName = mapName ?? "MAP01", RngSeed = rngSeed },
                GameInfo = new GameInfoPayload { GameId = options.Pregame.GameId },
            },
        };
        return true;
    }

    public static void PrintUsage()
    {
        Console.WriteLine(
            "Usage: hcdeserv --iwad <path> [--map <name>] [--port <port>] [--bind <ipv4>] [--rng-seed <int>]");
        Console.WriteLine("       [--strife-thing-flags] (Strife binary spawn flags only; not full Strife support)");
        Console.WriteLine("       [--server-name <name>] [--skill <0-255>] [--deathmatch] [--teamplay]");
        Console.WriteLine("       [--limit-pain] (Pain Elementals stop spawning when 21 Lost Souls exist)");
        Console.WriteLine("       [--hexen-crush-defaults] (converted crush/stop defaults only; not full Hexen support)");
        Console.WriteLine("       [--file <WAD-or-PK3>] (repeat in load order; later maps override earlier maps)");
        Console.WriteLine("       [--gamemode <id>] [--gamemode-name <label>] [--no-query]");
        Console.WriteLine("       [--invasion-waves <1-65535>] [--invasion-countdown <seconds>] [--invasion-intermission <seconds>]");
        Console.WriteLine("       [--master [host[:port]]] [--rcon-password <secret>] [--rcon-port <port>]");
        Console.WriteLine();
        Console.WriteLine("Managed dedicated-server scaffold: pregame host pump with map-load bootstrap handoff.");
        Console.WriteLine("Defaults: --bind 0.0.0.0 --port 10666 --map MAP01 --rng-seed 1");
        Console.WriteLine("Invasion: --gamemode 4 (defaults: 8 waves, 30-second countdown, 1-second intermission).");
        Console.WriteLine($"Master advertise defaults: {MasterProtocol.DefaultMasterHost}:{MasterProtocol.DefaultMasterPort}");
    }

    internal static bool TryParseMasterAddress(string? masterAddress, DedicatedServerOptions options, out string? error)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(masterAddress))
            return true;

        var host = masterAddress;
        var port = (int)options.MasterPort;
        var colon = masterAddress.LastIndexOf(':');
        if (colon > 0 && colon < masterAddress.Length - 1)
        {
            host = masterAddress[..colon];
            if (!int.TryParse(masterAddress[(colon + 1)..], out port) || port is <= 0 or > 65535)
            {
                error = $"invalid master port in: {masterAddress}";
                return false;
            }
        }

        if (!IPAddress.TryParse(host, out _))
        {
            error = $"invalid master host: {host}";
            return false;
        }

        options.MasterHost = host;
        options.MasterPort = (ushort)port;
        return true;
    }

    private static bool TryReadArg(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length)
            return false;

        value = args[++index];
        return true;
    }
}
