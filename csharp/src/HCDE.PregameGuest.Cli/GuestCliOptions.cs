namespace HCDE.PregameGuest.Cli;

public sealed class GuestCliOptions
{
    public bool ShowHelp { get; set; }
    public string ServerAddress { get; set; } = "127.0.0.1:5029";
    public string Password { get; set; } = "";
    public string UserInfo { get; set; } = "name\\guest";
    public byte EngineMajor { get; set; } = 1;
    public byte EngineMinor { get; set; }
    public byte EngineRevision { get; set; }
    public List<string> WadCrcs { get; } = [];
    public int LiveTicks { get; set; }
    public int TimeoutMilliseconds { get; set; } = 30000;

    public static GuestCliOptions Parse(string[] args)
    {
        var options = new GuestCliOptions();
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    return options;
                case "--server":
                    options.ServerAddress = RequireValue(args, ref i);
                    break;
                case "--password":
                    options.Password = RequireValue(args, ref i);
                    break;
                case "--userinfo":
                    options.UserInfo = RequireValue(args, ref i);
                    break;
                case "--engine-version":
                {
                    var parts = RequireValue(args, ref i).Split('.');
                    options.EngineMajor = byte.Parse(parts[0]);
                    options.EngineMinor = parts.Length > 1 ? byte.Parse(parts[1]) : (byte)0;
                    options.EngineRevision = parts.Length > 2 ? byte.Parse(parts[2]) : (byte)0;
                    break;
                }
                case "--wad-crc":
                    options.WadCrcs.Add(RequireValue(args, ref i));
                    break;
                case "--timeout-ms":
                    options.TimeoutMilliseconds = int.Parse(RequireValue(args, ref i));
                    break;
                case "--live-ticks":
                    options.LiveTicks = int.Parse(RequireValue(args, ref i));
                    break;
            }
        }

        return options;
    }

    private static string RequireValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {args[index]}");
        return args[++index];
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
            HCDE pregame guest CLI — joins a server through the C# pregame pump.

            Usage:
              dotnet run --project csharp/src/HCDE.PregameGuest.Cli -- --server 127.0.0.1:5029
                [--password PASS] [--userinfo name\\guest]
                [--engine-version 1.0.0] [--wad-crc <crc>]...
                [--timeout-ms 30000] [--live-ticks 0]
            """);
    }
}
