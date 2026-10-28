namespace HCDE.Net.Core;

/// <summary>Bounds-checked protocol stream I/O (<c>i_protocol.cpp</c> 167–389).</summary>
public static class VmIProtocolChecked
{
    public const string ReadPastEndMessage = "Attempted to read past end of stream";
    public const string WritePastEndMessage = "Attempted to write past end of stream";

    public static void AdvanceStreamNative(ref ProtocolStream stream, int bytes, Action<string>? fatal = null)
    {
        if (bytes > stream.Length)
            fatal?.Invoke(ReadPastEndMessage);
        stream.Offset += bytes;
        stream.Length -= bytes;
    }

    public static string ReadStringNative(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var len = 0;
        while (len < stream.Length && stream.Buffer[stream.Offset + len] != 0)
            len++;
        if (len == stream.Length)
            fatal?.Invoke(ReadPastEndMessage);
        var s = System.Text.Encoding.UTF8.GetString(stream.Buffer, stream.Offset, len);
        AdvanceStreamNative(ref stream, len + 1, fatal);
        return s;
    }

    public static string ReadStringConstNative(ref ProtocolStream stream, Action<string>? fatal = null) =>
        ReadStringNative(ref stream, fatal);

    public static void ReadBytesNative(byte[] dst, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (dst.Length > stream.Length)
            fatal?.Invoke(ReadPastEndMessage);
        if (dst.Length > 0)
        {
            Array.Copy(stream.Buffer, stream.Offset, dst, 0, dst.Length);
            AdvanceStreamNative(ref stream, dst.Length, fatal);
        }
    }

    public static byte ReadInt8Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanRead(ref stream, 1, fatal);
        var v = stream.Buffer[stream.Offset];
        AdvanceStreamNative(ref stream, 1, fatal);
        return v;
    }

    public static short ReadInt16Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 2)
            fatal?.Invoke(ReadPastEndMessage);
        var off = stream.Offset;
        short v = (short)((stream.Buffer[off] << 8) | stream.Buffer[off + 1]);
        AdvanceStreamNative(ref stream, 2, fatal);
        return v;
    }

    public static int ReadInt32Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 4)
            fatal?.Invoke(ReadPastEndMessage);
        var off = stream.Offset;
        var v = (stream.Buffer[off] << 24) | (stream.Buffer[off + 1] << 16) | (stream.Buffer[off + 2] << 8)
            | stream.Buffer[off + 3];
        AdvanceStreamNative(ref stream, 4, fatal);
        return v;
    }

    public static long ReadInt64Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 8)
            fatal?.Invoke(ReadPastEndMessage);
        var off = stream.Offset;
        long v = ((long)stream.Buffer[off] << 56) | ((long)stream.Buffer[off + 1] << 48)
            | ((long)stream.Buffer[off + 2] << 40) | ((long)stream.Buffer[off + 3] << 32)
            | ((long)stream.Buffer[off + 4] << 24) | ((long)stream.Buffer[off + 5] << 16)
            | ((long)stream.Buffer[off + 6] << 8) | stream.Buffer[off + 7];
        AdvanceStreamNative(ref stream, 8, fatal);
        return v;
    }

    public static float ReadFloatNative(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var bits = ReadInt32Native(ref stream, fatal);
        return BitConverter.Int32BitsToSingle(bits);
    }

    public static double ReadDoubleNative(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var bits = ReadInt64Native(ref stream, fatal);
        return BitConverter.Int64BitsToDouble(bits);
    }

    public static void WriteStringNative(string value, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var start = stream.Offset;
        var remaining = stream.Length;
        var i = 0;
        while (i < value.Length)
        {
            if (remaining-- == 0)
                fatal?.Invoke(WritePastEndMessage);
            stream.Buffer[stream.Offset++] = (byte)value[i];
            i++;
        }

        if (remaining == 0)
            fatal?.Invoke(WritePastEndMessage);
        stream.Buffer[stream.Offset++] = 0;
        stream.Length -= stream.Offset - start;
    }

    public static void WriteBytesNative(byte[] source, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (source.Length > stream.Length)
            fatal?.Invoke(WritePastEndMessage);
        if (source.Length > 0)
        {
            Array.Copy(source, 0, stream.Buffer, stream.Offset, source.Length);
            AdvanceStreamNative(ref stream, source.Length, fatal);
        }
    }

    public static void WriteFStringNative(byte[] source, ref ProtocolStream stream, Action<string>? fatal = null) =>
        WriteBytesNative(source, ref stream, fatal);

    public static void WriteInt8Native(byte v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanWrite(ref stream, 1, fatal);
        stream.Buffer[stream.Offset] = v;
        AdvanceStreamNative(ref stream, 1, fatal);
    }

    public static void WriteInt16Native(short v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 2)
            fatal?.Invoke(WritePastEndMessage);
        var off = stream.Offset;
        stream.Buffer[off] = (byte)(v >> 8);
        stream.Buffer[off + 1] = (byte)(v & 255);
        AdvanceStreamNative(ref stream, 2, fatal);
    }

    public static void WriteInt32Native(int v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 4)
            fatal?.Invoke(WritePastEndMessage);
        var off = stream.Offset;
        stream.Buffer[off] = (byte)(v >> 24);
        stream.Buffer[off + 1] = (byte)((v >> 16) & 255);
        stream.Buffer[off + 2] = (byte)((v >> 8) & 255);
        stream.Buffer[off + 3] = (byte)(v & 255);
        AdvanceStreamNative(ref stream, 4, fatal);
    }

    public static void WriteInt64Native(long v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 8)
            fatal?.Invoke(WritePastEndMessage);
        var off = stream.Offset;
        stream.Buffer[off] = (byte)(v >> 56);
        stream.Buffer[off + 1] = (byte)((v >> 48) & 255);
        stream.Buffer[off + 2] = (byte)((v >> 40) & 255);
        stream.Buffer[off + 3] = (byte)((v >> 32) & 255);
        stream.Buffer[off + 4] = (byte)((v >> 24) & 255);
        stream.Buffer[off + 5] = (byte)((v >> 16) & 255);
        stream.Buffer[off + 6] = (byte)((v >> 8) & 255);
        stream.Buffer[off + 7] = (byte)(v & 255);
        AdvanceStreamNative(ref stream, 8, fatal);
    }

    public static void WriteFloatNative(float v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var bits = BitConverter.SingleToInt32Bits(v);
        WriteInt32Native(bits, ref stream, fatal);
    }

    public static void WriteDoubleNative(double v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var bits = BitConverter.DoubleToInt64Bits(v);
        WriteInt64Native(bits, ref stream, fatal);
    }

    static void EnsureCanRead(ref ProtocolStream stream, int bytes, Action<string>? fatal)
    {
        if (stream.Length < bytes)
            fatal?.Invoke(ReadPastEndMessage);
    }

    static void EnsureCanWrite(ref ProtocolStream stream, int bytes, Action<string>? fatal)
    {
        if (stream.Length < bytes)
            fatal?.Invoke(WritePastEndMessage);
    }
}

public struct ProtocolStream
{
    public byte[] Buffer;
    public int Offset;
    public int Length;
}
