using System.Buffers.Binary;
using System.IO.Compression;

namespace HCDE.Net.Transport;

public enum GameplayWireDecodeStatus
{
    Ok,
    TooShort,
    BadCrc,
    CompressedMalformed,
    DecompressFailed,
}

/// <summary>
/// Wire encode/decode for in-game packets: 4-byte BE CRC prefix + payload.
/// Non-setup packets fold the 8-byte <see cref="GameId"/> into the CRC (see <c>SendPacket</c> in <c>i_net.cpp</c>).
/// </summary>
public static class GameplayWireCodec
{
    public const int CrcPrefixSize = 4;
    public const int GameIdSize = 8;

    public static bool IsGameplayPacket(ReadOnlySpan<byte> netBuffer) =>
        netBuffer.Length > 0 && (netBuffer[0] & (byte)NetCommandFlags.Setup) == 0;

    public static uint ComputeCrc(ReadOnlySpan<byte> netBuffer, ReadOnlySpan<byte> gameId)
    {
        var crc = Crc32.Calc(netBuffer);
        return gameId.Length >= GameIdSize
            ? Crc32.Add(crc, gameId[..GameIdSize])
            : crc;
    }

    public static int Encode(ReadOnlySpan<byte> netBuffer, ReadOnlySpan<byte> gameId, Span<byte> wireBuffer)
    {
        if (wireBuffer.Length < netBuffer.Length + CrcPrefixSize)
            return 0;

        netBuffer.CopyTo(wireBuffer[CrcPrefixSize..]);
        BinaryPrimitives.WriteUInt32BigEndian(wireBuffer, ComputeCrc(netBuffer, gameId));
        return netBuffer.Length + CrcPrefixSize;
    }

    public static GameplayWireDecodeStatus TryDecode(
        ReadOnlySpan<byte> wireBuffer,
        ReadOnlySpan<byte> gameId,
        Span<byte> netBuffer,
        out int netLength)
    {
        netLength = 0;
        if (wireBuffer.Length < CrcPrefixSize + 1)
            return GameplayWireDecodeStatus.TooShort;

        var payload = wireBuffer[CrcPrefixSize..];
        var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(wireBuffer);
        var actualCrc = ComputeCrc(payload, gameId);
        if (expectedCrc != actualCrc)
            return GameplayWireDecodeStatus.BadCrc;

        var firstByte = payload[0];
        if ((firstByte & (byte)NetCommandFlags.Compressed) != 0)
        {
            if (payload.Length <= 1)
                return GameplayWireDecodeStatus.CompressedMalformed;

            try
            {
                netBuffer[0] = (byte)(firstByte & ~(byte)NetCommandFlags.Compressed);
                using var compressed = new MemoryStream(payload[1..].ToArray());
                using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
                using var decompressed = new MemoryStream();
                zlib.CopyTo(decompressed);
                var data = decompressed.ToArray();
                if (data.Length == 0 || netBuffer.Length < data.Length + 1)
                    return GameplayWireDecodeStatus.DecompressFailed;
                data.CopyTo(netBuffer[1..]);
                netLength = data.Length + 1;
                return GameplayWireDecodeStatus.Ok;
            }
            catch (InvalidDataException)
            {
                return GameplayWireDecodeStatus.DecompressFailed;
            }
        }

        if (netBuffer.Length < payload.Length)
            return GameplayWireDecodeStatus.TooShort;

        payload.CopyTo(netBuffer);
        netLength = payload.Length;
        return GameplayWireDecodeStatus.Ok;
    }
}
