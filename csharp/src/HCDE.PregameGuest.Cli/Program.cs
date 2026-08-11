using System.Net;
using HCDE.Net.Pregame;
using HCDE.Net.Transport;

namespace HCDE.PregameGuest.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var options = GuestCliOptions.Parse(args);
        if (options.ShowHelp)
        {
            GuestCliOptions.PrintHelp();
            return 0;
        }

        if (!NetworkEndpoint.TryParse(options.ServerAddress, out var serverEndpoint))
        {
            Console.Error.WriteLine($"Invalid server address: {options.ServerAddress}");
            return 2;
        }

        using var transport = new UdpTransport();
        transport.Bind(0);
        transport.SetNonBlocking(true);

        var engineInfo = new EngineInfoSnapshot
        {
            Major = options.EngineMajor,
            Minor = options.EngineMinor,
            Revision = options.EngineRevision,
            WadCrcs = options.WadCrcs,
        };

        var guest = new global::HCDE.Net.Pregame.PregameGuest(transport, new PregameGuestOptions
        {
            ServerAddress = serverEndpoint,
            Password = options.Password,
            EngineInfo = engineInfo,
            UserInfo = options.UserInfo,
            ConnectFlags = HcdeConnectFlags.ServerAuthority,
        });

        var deadline = Environment.TickCount64 + options.TimeoutMilliseconds;
        while (Environment.TickCount64 < deadline)
        {
            var now = (ulong)Environment.TickCount64;
            guest.Pump(now);

            if (guest.Phase is PregameGuestPhase.Ready or PregameGuestPhase.Starting)
            {
                Console.WriteLine($"phase={guest.Phase} slot={guest.AssignedClientSlot} token=0x{guest.Connection.SessionToken:X8}");
                if (guest.ReceivedMapLoad is { } mapLoad)
                    Console.WriteLine($"map={mapLoad.MapName} seed={mapLoad.RngSeed}");
                return 0;
            }

            if (guest.Phase == PregameGuestPhase.Rejected)
            {
                if (guest.VerificationError is { } verificationError)
                    Console.Error.WriteLine($"verification-error={verificationError.Kind} files={string.Join(',', verificationError.Files)}");
                Console.Error.WriteLine($"rejected={guest.RejectReason}");
                return 1;
            }

            await Task.Delay(10);
        }

        Console.Error.WriteLine($"timeout phase={guest.Phase}");
        return 3;
    }
}
