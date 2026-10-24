namespace HCDE.Net.Core;

/// <summary>Unchecked protocol stream primitives (<c>i_protocol.cpp</c> 32–181).</summary>
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
        var v = (short)((buffer[offset] << 8) | buffer[offset + 1]);
        offset += 2;
        return v;
    }

    public static int UncheckedReadInt32Native(ref int offset, byte[] buffer)
    {
        var v = (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
        offset += 4;
        return v;
    }

    public static long UncheckedReadInt64Native(ref int offset, byte[] buffer)
    {
        long v = 0;
        for (var i = 0; i < 8; i++)
            v = (v << 8) | buffer[offset + i];
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
        buffer[offset++] = (byte)(v >> 8);
        buffer[offset++] = (byte)(v & 255);
    }

    public static void UncheckedWriteInt32Native(int v, ref int offset, byte[] buffer)
    {
        buffer[offset++] = (byte)(v >> 24);
        buffer[offset++] = (byte)((v >> 16) & 255);
        buffer[offset++] = (byte)((v >> 8) & 255);
        buffer[offset++] = (byte)(v & 255);
    }

    public static void UncheckedWriteInt64Native(long v, ref int offset, byte[] buffer)
    {
        buffer[offset++] = (byte)(v >> 56);
        buffer[offset++] = (byte)((v >> 48) & 255);
        buffer[offset++] = (byte)((v >> 40) & 255);
        buffer[offset++] = (byte)((v >> 32) & 255);
        buffer[offset++] = (byte)((v >> 24) & 255);
        buffer[offset++] = (byte)((v >> 16) & 255);
        buffer[offset++] = (byte)((v >> 8) & 255);
        buffer[offset++] = (byte)(v & 255);
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
}
