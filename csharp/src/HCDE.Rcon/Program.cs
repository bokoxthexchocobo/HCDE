using HCDE.Rcon;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var options = ParseOptions(args);
            var client = new RconClient();
            var response = await client.ExecuteAsync(options).ConfigureAwait(false);
            Console.WriteLine(response);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("hcdercon: {0}", ex.Message);
            PrintUsage();
            return 1;
        }
    }

    static void PrintUsage()
    {
        Console.Error.WriteLine("Usage: hcdercon --port <port> --password <password> [--host 127.0.0.1] <command>");
        Console.Error.WriteLine();
        Console.Error.WriteLine("Examples:");
        Console.Error.WriteLine("  hcdercon --port 10667 --password secret ping");
        Console.Error.WriteLine("  hcdercon --port 10667 --password secret status");
    }

    static RconClientOptions ParseOptions(string[] args)
    {
        var options = new RconClientOptions();
        var commandParts = new List<string>();

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            string RequireValue(string name)
            {
                if (i + 1 >= args.Length)
                    throw new ArgumentException($"missing value for {name}");
                return args[++i];
            }

            switch (arg)
            {
                case "--host":
                case "-h":
                    options.Host = RequireValue(arg);
                    break;
                case "--port":
                case "-p":
                    options.Port = int.Parse(RequireValue(arg));
                    break;
                case "--password":
                case "--pass":
                case "-P":
                    options.Password = RequireValue(arg);
                    break;
                case "--help":
                case "/?":
                    PrintUsage();
                    Environment.Exit(0);
                    break;
                default:
                    commandParts.Add(arg);
                    break;
            }
        }

        if (options.Port is <= 0 or > 65535)
            throw new ArgumentException("port must be between 1 and 65535");
        if (string.IsNullOrEmpty(options.Password))
            throw new ArgumentException("password is required");
        if (commandParts.Count == 0)
            throw new ArgumentException("command is required");

        options.Command = string.Join(' ', commandParts);
        return options;
    }
}
