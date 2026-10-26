namespace HCDE.Net.Core;

/// <summary>Unchecked protocol stream primitives (<c>i_protocol.cpp</c> 32–171).</summary>
public static class VmIProtocolUnchecked
{
    public static string UncheckedReadStringNative(ref int offset, byte[] buffer)
    {
        var start = offset;
        while (offset < buffer.Length && buffer[offset] != 0)
            offset++;
        var len = offset - start;
        offset++;
        return System.Text.Encoding.UTF8.GetString(buffer, start, len);
    }

    public static string UncheckedReadStringConstNative(ref int offset, byte[] buffer) =>
        UncheckedReadStringNative(ref offset, buffer);

    public static byte UncheckedReadInt8Native(ref int offset, byte[] buffer) => buffer[offset++];

    public static short UncheckedReadInt16Native(ref int offset, byte[] buffer)
    {
        var v = ReadInt16BigEndian(buffer, offset);
        offset += 2;
        return v;
    }

    public static int UncheckedReadInt32Native(ref int offset, byte[] buffer)
    {
        var v = ReadInt32BigEndian(buffer, offset);
        offset += 4;
        return v;
    }

    public static long UncheckedReadInt64Native(ref int offset, byte[] buffer)
    {
        var v = ReadInt64BigEndian(buffer, offset);
        offset += 8;
        return v;
    }

    public static float UncheckedReadFloatNative(ref int offset, byte[] buffer) =>
        BitConverter.Int32BitsToSingle(UncheckedReadInt32Native(ref offset, buffer));

    public static double UncheckedReadDoubleNative(ref int offset, byte[] buffer) =>
        BitConverter.Int64BitsToDouble(UncheckedReadInt64Native(ref offset, buffer));

    public static void UncheckedWriteStringNative(string value, ref int offset, byte[] buffer)
    {
        foreach (var ch in value)
            buffer[offset++] = (byte)ch;
        buffer[offset++] = 0;
    }

    public static void UncheckedWriteInt8Native(byte v, ref int offset, byte[] buffer) => buffer[offset++] = v;

    public static void UncheckedWriteInt16Native(short v, ref int offset, byte[] buffer)
    {
        WriteInt16BigEndian(buffer, offset, v);
        offset += 2;
    }

    public static void UncheckedWriteInt32Native(int v, ref int offset, byte[] buffer)
    {
        WriteInt32BigEndian(buffer, offset, v);
        offset += 4;
    }

    public static void UncheckedWriteInt64Native(long v, ref int offset, byte[] buffer)
    {
        WriteInt64BigEndian(buffer, offset, v);
        offset += 8;
    }

    public static void UncheckedWriteFloatNative(float v, ref int offset, byte[] buffer) =>
        UncheckedWriteInt32Native(BitConverter.SingleToInt32Bits(v), ref offset, buffer);

    public static void UncheckedWriteDoubleNative(double v, ref int offset, byte[] buffer) =>
        UncheckedWriteInt64Native(BitConverter.DoubleToInt64Bits(v), ref offset, buffer);

    public static void AdvanceStreamNative(ref int offset, ref int length, int bytes)
    {
        if (bytes > length)
            throw new InvalidOperationException("Attempted to advance past end of stream");
        offset += bytes;
        length -= bytes;
    }

    internal static short ReadInt16BigEndian(byte[] buffer, int offset) =>
        (short)((buffer[offset] << 8) | buffer[offset + 1]);

    internal static int ReadInt32BigEndian(byte[] buffer, int offset) =>
        (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];

    internal static long ReadInt64BigEndian(byte[] buffer, int offset) =>
        ((long)buffer[offset] << 56) | ((long)buffer[offset + 1] << 48) | ((long)buffer[offset + 2] << 40)
        | ((long)buffer[offset + 3] << 32) | ((long)buffer[offset + 4] << 24) | ((long)buffer[offset + 5] << 16)
        | ((long)buffer[offset + 6] << 8) | buffer[offset + 7];

    internal static void WriteInt16BigEndian(byte[] buffer, int offset, short v)
    {
        buffer[offset] = (byte)(v >> 8);
        buffer[offset + 1] = (byte)(v & 255);
    }

    internal static void WriteInt32BigEndian(byte[] buffer, int offset, int v)
    {
        buffer[offset] = (byte)(v >> 24);
        buffer[offset + 1] = (byte)((v >> 16) & 255);
        buffer[offset + 2] = (byte)((v >> 8) & 255);
        buffer[offset + 3] = (byte)(v & 255);
    }

    internal static void WriteInt64BigEndian(byte[] buffer, int offset, long v)
    {
        buffer[offset] = (byte)(v >> 56);
        buffer[offset + 1] = (byte)((v >> 48) & 255);
        buffer[offset + 2] = (byte)((v >> 40) & 255);
        buffer[offset + 3] = (byte)((v >> 32) & 255);
        buffer[offset + 4] = (byte)((v >> 24) & 255);
        buffer[offset + 5] = (byte)((v >> 16) & 255);
        buffer[offset + 6] = (byte)((v >> 8) & 255);
        buffer[offset + 7] = (byte)(v & 255);
    }
}
