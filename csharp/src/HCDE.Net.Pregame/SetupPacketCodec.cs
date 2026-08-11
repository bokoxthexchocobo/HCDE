using System.Buffers.Binary;
using System.IO.Compression;
using HCDE.Net.Transport;

namespace HCDE.Net.Pregame;

public enum SetupPacketDecodeStatus
{
    Ok,
    TooShort,
    BadCrc,
    CompressedMalformed,
    DecompressFailed,
}

/// <summary>
/// Wire encode/decode for setup packets: 4-byte big-endian CRC prefix + payload.
/// </summary>
public static class SetupPacketCodec
{
    public const int CrcPrefixSize = 4;

    public static int Encode(ReadOnlySpan<byte> netBuffer, Span<byte> wireBuffer)
    {
        if (wireBuffer.Length < netBuffer.Length + CrcPrefixSize)
            return 0;

        netBuffer.CopyTo(wireBuffer[CrcPrefixSize..]);
        var crc = HCDE.Net.Transport.Crc32.Calc(netBuffer);
        BinaryPrimitives.WriteUInt32BigEndian(wireBuffer, crc);
        return netBuffer.Length + CrcPrefixSize;
    }

    public static SetupPacketDecodeStatus TryDecode(
        ReadOnlySpan<byte> wireBuffer,
        Span<byte> netBuffer,
        out int netLength)
    {
        netLength = 0;
        if (wireBuffer.Length < CrcPrefixSize + 1)
            return SetupPacketDecodeStatus.TooShort;

        var payload = wireBuffer[CrcPrefixSize..];
        var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(wireBuffer);
        var actualCrc = HCDE.Net.Transport.Crc32.Calc(payload);
        if (expectedCrc != actualCrc)
            return SetupPacketDecodeStatus.BadCrc;

        var firstByte = payload[0];
        if ((firstByte & (byte)NetCommandFlags.Compressed) != 0)
        {
            if (payload.Length <= 1)
                return SetupPacketDecodeStatus.CompressedMalformed;

            try
            {
                netBuffer[0] = (byte)(firstByte & ~(byte)NetCommandFlags.Compressed);
                using var compressed = new MemoryStream(payload[1..].ToArray());
                using var zlib = new ZLibStream(compressed, CompressionMode.Decompress);
                using var decompressed = new MemoryStream();
                zlib.CopyTo(decompressed);
                var data = decompressed.ToArray();
                if (data.Length == 0 || netBuffer.Length < data.Length + 1)
                    return SetupPacketDecodeStatus.DecompressFailed;
                data.CopyTo(netBuffer[1..]);
                netLength = data.Length + 1;
                return SetupPacketDecodeStatus.Ok;
            }
            catch (InvalidDataException)
            {
                return SetupPacketDecodeStatus.DecompressFailed;
            }
        }

        if (netBuffer.Length < payload.Length)
            return SetupPacketDecodeStatus.TooShort;

        payload.CopyTo(netBuffer);
        netLength = payload.Length;
        return SetupPacketDecodeStatus.Ok;
    }

    public static int EncodeCompressed(ReadOnlySpan<byte> netBuffer, Span<byte> wireBuffer)
    {
        if (netBuffer.Length < MinCompressionSize)
            return Encode(netBuffer, wireBuffer);

        if (wireBuffer.Length < CrcPrefixSize + 2)
            return 0;

        var payload = wireBuffer[(CrcPrefixSize + 1)..];
        wireBuffer[CrcPrefixSize] = (byte)(netBuffer[0] | (byte)NetCommandFlags.Compressed);
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Fastest, leaveOpen: true))
            zlib.Write(netBuffer[1..]);

        var compressed = output.ToArray();
        if (CrcPrefixSize + 1 + compressed.Length > wireBuffer.Length)
            return 0;

        compressed.CopyTo(payload);
        var payloadLength = 1 + compressed.Length;
        var crc = HCDE.Net.Transport.Crc32.Calc(wireBuffer.Slice(CrcPrefixSize, payloadLength));
        BinaryPrimitives.WriteUInt32BigEndian(wireBuffer, crc);
        return CrcPrefixSize + payloadLength;
    }

    public const int MinCompressionSize = NetConstants.MinCompressionSize;

    public static bool IsSetupPacket(ReadOnlySpan<byte> netBuffer) =>
        netBuffer.Length > 0 && (netBuffer[0] & (byte)NetCommandFlags.Setup) != 0;
}
