using System.Net;
using System.Net.Sockets;
using HCDE.Protocol;

namespace HCDE.Rcon;

public sealed class RconClientOptions
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; }
    public string Password { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
}

public sealed class RconClient
{
    public async Task<string> ExecuteAsync(RconClientOptions options, CancellationToken cancellationToken = default)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Parse(options.Host), options.Port, cancellationToken).ConfigureAwait(false);
        await using var stream = client.GetStream();

        var hello = await RconProtocol.ReceiveFrameAsync(stream, cancellationToken).ConfigureAwait(false);
        const string prefix = "nonce ";
        if (!hello.StartsWith(prefix, StringComparison.Ordinal))
            throw new InvalidOperationException("server did not send an RCON nonce");

        var nonce = hello[prefix.Length..];
        var authHash = RconProtocol.FormatAuthHash(RconProtocol.Fnv1aHash($"{nonce}:{options.Password}"));
        await RconProtocol.SendFrameAsync(stream, $"auth {authHash}", cancellationToken).ConfigureAwait(false);

        var auth = await RconProtocol.ReceiveFrameAsync(stream, cancellationToken).ConfigureAwait(false);
        if (!auth.StartsWith("OK", StringComparison.Ordinal))
            throw new InvalidOperationException(auth);

        await RconProtocol.SendFrameAsync(stream, options.Command, cancellationToken).ConfigureAwait(false);
        return await RconProtocol.ReceiveFrameAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
