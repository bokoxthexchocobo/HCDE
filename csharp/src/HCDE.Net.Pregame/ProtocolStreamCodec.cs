using System.Buffers.Binary;
using System.Text;

namespace HCDE.Net.Pregame;

public static class ProtocolStreamCodec
{
    public static int WriteNullTerminatedString(Span<byte> buffer, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        if (buffer.Length < bytes.Length + 1)
            return 0;
        bytes.CopyTo(buffer);
        buffer[bytes.Length] = 0;
        return bytes.Length + 1;
    }

    public static bool TryReadNullTerminatedString(ReadOnlySpan<byte> buffer, ref int offset, out string value)
    {
        value = "";
        if (offset >= buffer.Length)
            return false;

        var start = offset;
        while (offset < buffer.Length && buffer[offset] != 0)
            offset++;
        if (offset >= buffer.Length)
            return false;

        value = Encoding.ASCII.GetString(buffer[start..offset]);
        offset++;
        return true;
    }

    public static int WriteInt8(Span<byte> buffer, byte value)
    {
        if (buffer.Length < 1)
            return 0;
        buffer[0] = value;
        return 1;
    }

    public static bool TryReadInt8(ReadOnlySpan<byte> buffer, ref int offset, out byte value)
    {
        value = 0;
        if (offset >= buffer.Length)
            return false;
        value = buffer[offset++];
        return true;
    }

    public static int WriteInt32(Span<byte> buffer, int value)
    {
        if (buffer.Length < 4)
            return 0;
        BinaryPrimitives.WriteInt32BigEndian(buffer, value);
        return 4;
    }

    public static bool TryReadInt32(ReadOnlySpan<byte> buffer, ref int offset, out int value)
    {
        value = 0;
        if (offset + 4 > buffer.Length)
            return false;
        value = BinaryPrimitives.ReadInt32BigEndian(buffer[offset..]);
        offset += 4;
        return true;
    }

    public static int WriteUInt16BigEndian(Span<byte> buffer, ushort value)
    {
        if (buffer.Length < 2)
            return 0;
        BinaryPrimitives.WriteUInt16BigEndian(buffer, value);
        return 2;
    }

    public static bool TryReadUInt16BigEndian(ReadOnlySpan<byte> buffer, ref int offset, out ushort value)
    {
        value = 0;
        if (offset + 2 > buffer.Length)
            return false;
        value = BinaryPrimitives.ReadUInt16BigEndian(buffer[offset..]);
        offset += 2;
        return true;
    }
}
