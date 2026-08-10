using HCDE.Master;

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

        var server = new MasterServer(options);
        cts.Token.Register(server.RequestStop);

        try
        {
            await server.RunAsync(cts.Token).ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine("hcdemaster: {0}", ex.Message);
            return 1;
        }
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage: hcdemaster [--bind <ipv4>] [--port <port>] [--ttl <seconds>] [--max-packets <count>] [--quiet]");
        Console.WriteLine();
        Console.WriteLine("Receives HCDE dedicated-server heartbeats and answers Doom Connector master-list queries.");
        Console.WriteLine("Defaults: --bind 0.0.0.0 --port 15000 --ttl 180");
    }

    static bool TryParseOptions(string[] args, out MasterServerOptions options, out string? error)
    {
        options = new MasterServerOptions();
        error = null;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--help":
                case "-h":
                    return false;
                case "--bind":
                {
                    if (i + 1 >= args.Length)
                    {
                        error = "--bind requires a value";
                        return false;
                    }

                    options.BindAddress = args[++i];
                    break;
                }
                case "--port":
                case "-p":
                {
                    if (i + 1 >= args.Length)
                    {
                        error = "--port requires a value";
                        return false;
                    }

                    var value = args[++i];
                    if (!ushort.TryParse(value, out var port) || port == 0)
                    {
                        error = $"Invalid port: {value}";
                        return false;
                    }

                    options.Port = port;
                    break;
                }
                case "--ttl":
                {
                    if (i + 1 >= args.Length)
                    {
                        error = "--ttl requires a value";
                        return false;
                    }

                    var value = args[++i];
                    if (!int.TryParse(value, out var ttl) || ttl is <= 0 or > 86400)
                    {
                        error = $"Invalid ttl: {value}";
                        return false;
                    }

                    options.TtlSeconds = ttl;
                    break;
                }
                case "--max-packets":
                {
                    if (i + 1 >= args.Length)
                    {
                        error = "--max-packets requires a value";
                        return false;
                    }

                    var value = args[++i];
                    if (!int.TryParse(value, out var maxPackets) || maxPackets < 0)
                    {
                        error = $"Invalid max-packets: {value}";
                        return false;
                    }

                    options.MaxPackets = maxPackets;
                    break;
                }
                case "--quiet":
                case "-q":
                    options.Quiet = true;
                    break;
                default:
                    error = $"Unknown argument: {arg}";
                    return false;
            }
        }

        return true;
    }
}
