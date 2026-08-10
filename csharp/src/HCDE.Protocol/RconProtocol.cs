using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;

namespace HCDE.Protocol;

public static class RconProtocol
{
    public const uint MaxFrameSize = 4096;

    public static uint Fnv1aHash(ReadOnlySpan<byte> data)
    {
        uint hash = 2166136261;
        foreach (var b in data)
        {
            hash ^= b;
            hash *= 16777619;
        }

        return hash;
    }

    public static uint Fnv1aHash(string text) => Fnv1aHash(Encoding.UTF8.GetBytes(text));

    public static string FormatAuthHash(uint hash)
    {
        return hash.ToString("x8");
    }

    public static async Task SendFrameAsync(NetworkStream stream, string text, CancellationToken cancellationToken = default)
    {
        var payload = Encoding.UTF8.GetBytes(text);
        if (payload.Length == 0 || payload.Length > MaxFrameSize)
            throw new InvalidOperationException("RCON frame payload must be between 1 and 4096 bytes");

        var header = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(header, (uint)payload.Length);
        await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
        await stream.WriteAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> ReceiveFrameAsync(NetworkStream stream, CancellationToken cancellationToken = default)
    {
        var header = new byte[4];
        await ReadExactAsync(stream, header, cancellationToken).ConfigureAwait(false);

        var length = BinaryPrimitives.ReadUInt32BigEndian(header);
        if (length == 0 || length > MaxFrameSize)
            throw new InvalidOperationException("invalid RCON frame length from peer");

        var payload = new byte[length];
        await ReadExactAsync(stream, payload, cancellationToken).ConfigureAwait(false);
        return Encoding.UTF8.GetString(payload);
    }

    private static async Task ReadExactAsync(NetworkStream stream, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer[offset..], cancellationToken).ConfigureAwait(false);
            if (read <= 0)
                throw new IOException("socket receive failed");

            offset += read;
        }
    }
}
