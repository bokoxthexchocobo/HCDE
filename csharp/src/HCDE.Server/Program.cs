using HCDE.Net.Pregame;

namespace HCDE.Server;

static class Program
{
    static async Task<int> Main(string[] args)
    {
        if (!DedicatedServerCommandLine.TryParse(args, out var options, out var error))
        {
            if (error is not null)
                Console.Error.WriteLine(error);

            DedicatedServerCommandLine.PrintUsage();
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
                "hcdeserv listening on {0}:{1} map={2} query={3} master={4}",
                options.BindAddress,
                host.BoundPort,
                options.Pregame.Session.MapLoad.MapName,
                options.EnableServerQuery ? "on" : "off",
                options.EnableMasterAdvertise ? $"{options.MasterHost}:{options.MasterPort}" : "off");

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
}
