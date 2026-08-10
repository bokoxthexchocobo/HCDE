using System.Buffers.Binary;
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
        var crc = Crc32.Calc(netBuffer);
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
        var actualCrc = Crc32.Calc(payload);
        if (expectedCrc != actualCrc)
            return SetupPacketDecodeStatus.BadCrc;

        var firstByte = payload[0];
        if ((firstByte & (byte)NetCommandFlags.Compressed) != 0)
            return SetupPacketDecodeStatus.CompressedMalformed;

        if (netBuffer.Length < payload.Length)
            return SetupPacketDecodeStatus.TooShort;

        payload.CopyTo(netBuffer);
        netLength = payload.Length;
        return SetupPacketDecodeStatus.Ok;
    }

    public static bool IsSetupPacket(ReadOnlySpan<byte> netBuffer) =>
        netBuffer.Length > 0 && (netBuffer[0] & (byte)NetCommandFlags.Setup) != 0;
}
