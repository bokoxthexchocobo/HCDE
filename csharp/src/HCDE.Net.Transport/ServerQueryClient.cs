namespace HCDE.Net.Transport;

public sealed class ServerQueryClientOptions
{
    public string Address { get; set; } = "127.0.0.1";
    public int Port { get; set; }
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(500);
}

public sealed class ServerQueryClient
{
    public ServerQuerySnapshot Query(ServerQueryClientOptions options)
    {
        if (!NetworkEndpoint.TryParse(options.Address, out var endpoint, options.Port > 0 ? options.Port : NetConstants.DefaultGamePort))
            throw new ArgumentException("invalid server address", nameof(options));

        using var transport = new UdpTransport();
        transport.Bind(0);
        transport.SetNonBlocking(true);

        var request = ServerQueryCodec.CreateLauncherChallengeRequest();
        transport.Send(request, endpoint);

        var buffer = new byte[NetConstants.MaxMessageLength];
        var deadline = DateTime.UtcNow + options.Timeout;
        while (DateTime.UtcNow < deadline)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (!transport.TryReceive(
                    buffer,
                    out var received,
                    out _,
                    remaining > TimeSpan.Zero ? remaining : TimeSpan.FromMilliseconds(1)))
            {
                continue;
            }

            if (!ServerQueryCodec.TryReadResponse(buffer.AsSpan(0, received), out var snapshot, out var error))
                throw new InvalidOperationException(error ?? "invalid query reply");

            return snapshot;
        }

        throw new TimeoutException("Query timed out");
    }
}
