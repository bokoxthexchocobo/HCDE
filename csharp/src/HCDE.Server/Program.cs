using System.Net;
using HCDE.Net.Pregame;

namespace HCDE.Server;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        if (!TryParseOptions(args, out var options, out var error))
        {
            if (error is not null)
                Console.Error.WriteLine(error);

            PrintUsage();
            return error is null ? 0 : 2;
        }

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => cts.Cancel();

        try
        {
            using var host = new DedicatedServerHost(options);
            Console.WriteLine(
                "hcdeserv listening on {0}:{1} map={2}",
                options.BindAddress,
                host.BoundPort,
                options.Pregame.Session.MapLoad.MapName);

            while (!cts.IsCancellationRequested)
            {
                host.Pump((ulong)Environment.TickCount64);
                await Task.Delay(10, cts.Token).ConfigureAwait(false);
            }

            return 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine("hcdeserv: {0}", ex.Message);
            return 1;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage: hcdeserv --iwad <path> [--map <name>] [--port <port>] [--bind <ipv4>] [--rng-seed <int>]");
        Console.WriteLine();
        Console.WriteLine("Managed dedicated-server scaffold: pregame host pump with map-load bootstrap handoff.");
        Console.WriteLine("Defaults: --bind 0.0.0.0 --port 10666 --map MAP01 --rng-seed 1");
    }

    static bool TryParseOptions(string[] args, out DedicatedServerOptions options, out string? error)
    {
        options = new DedicatedServerOptions();
        error = null;
        string? iwadPath = null;
        var mapName = "MAP01";
        var rngSeed = 1;

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

    static bool TryReadArg(string[] args, ref int index, out string? value)
    {
        value = null;
        if (index + 1 >= args.Length)
            return false;

        value = args[++index];
        return true;
    }
}
