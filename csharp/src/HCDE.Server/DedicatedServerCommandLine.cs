using System.Net;
using HCDE.Net.Pregame;
using HCDE.Protocol;

namespace HCDE.Server;

public static class DedicatedServerCommandLine
{
    public static bool TryParse(string[] args, out DedicatedServerOptions options, out string? error)
    {
        options = new DedicatedServerOptions();
        error = null;
        string? iwadPath = null;
        var mapName = "MAP01";
        var rngSeed = 1;
        var advertiseMaster = false;
        string? masterAddress = null;

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
                    break;
                case "--no-query":
                    options.EnableServerQuery = false;
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

        options.IwadBytes = File.ReadAllBytes(iwadPath);
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
        Console.WriteLine("       [--server-name <name>] [--skill <0-255>] [--deathmatch] [--teamplay]");
        Console.WriteLine("       [--gamemode <id>] [--gamemode-name <label>] [--no-query]");
        Console.WriteLine("       [--master [host[:port]]]");
        Console.WriteLine();
        Console.WriteLine("Managed dedicated-server scaffold: pregame host pump with map-load bootstrap handoff.");
        Console.WriteLine("Defaults: --bind 0.0.0.0 --port 10666 --map MAP01 --rng-seed 1");
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
