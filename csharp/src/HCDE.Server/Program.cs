using HCDE.Net.Pregame;
using HCDE.Net.Transport;

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

            var scheduler = new ServerTicScheduler((ulong)Environment.TickCount64);
            while (!cts.IsCancellationRequested)
            {
                var now = (ulong)Environment.TickCount64;
                var due = scheduler.TakeDueTics(now);
                for (var tic = 0; tic < due; tic++)
                {
                    host.Pump(now);
                    if (!host.PregameHost.StartGameSent
                        && host.PregameHost.Clients.Any(client => client.Status == ConnectionStatus.Ready))
                        host.PregameHost.StartGame(now);
                }
                await Task.Delay(10, cts.Token).ConfigureAwait(false);
            }

            return 0;
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("hcdeserv: {0}", ex.Message);
            return 1;
        }
    }
}
