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
        var len = MeasureNullTerminatedLength(stream, fatal);
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
        EnsureCanRead(ref stream, 2, fatal);
        var v = VmIProtocolUnchecked.ReadInt16BigEndian(stream.Buffer, stream.Offset);
        AdvanceStreamNative(ref stream, 2, fatal);
        return v;
    }

    public static int ReadInt32Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanRead(ref stream, 4, fatal);
        var v = VmIProtocolUnchecked.ReadInt32BigEndian(stream.Buffer, stream.Offset);
        AdvanceStreamNative(ref stream, 4, fatal);
        return v;
    }

    public static long ReadInt64Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanRead(ref stream, 8, fatal);
        var v = VmIProtocolUnchecked.ReadInt64BigEndian(stream.Buffer, stream.Offset);
        AdvanceStreamNative(ref stream, 8, fatal);
        return v;
    }

    public static float ReadFloatNative(ref ProtocolStream stream, Action<string>? fatal = null) =>
        BitConverter.Int32BitsToSingle(ReadInt32Native(ref stream, fatal));

    public static double ReadDoubleNative(ref ProtocolStream stream, Action<string>? fatal = null) =>
        BitConverter.Int64BitsToDouble(ReadInt64Native(ref stream, fatal));

    public static void WriteStringNative(string value, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var start = stream.Offset;
        var remaining = stream.Length;
        foreach (var ch in value)
        {
            if (remaining-- == 0)
                fatal?.Invoke(WritePastEndMessage);
            stream.Buffer[stream.Offset++] = (byte)ch;
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
        EnsureCanWrite(ref stream, 2, fatal);
        VmIProtocolUnchecked.WriteInt16BigEndian(stream.Buffer, stream.Offset, v);
        AdvanceStreamNative(ref stream, 2, fatal);
    }

    public static void WriteInt32Native(int v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanWrite(ref stream, 4, fatal);
        VmIProtocolUnchecked.WriteInt32BigEndian(stream.Buffer, stream.Offset, v);
        AdvanceStreamNative(ref stream, 4, fatal);
    }

    public static void WriteInt64Native(long v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        EnsureCanWrite(ref stream, 8, fatal);
        VmIProtocolUnchecked.WriteInt64BigEndian(stream.Buffer, stream.Offset, v);
        AdvanceStreamNative(ref stream, 8, fatal);
    }

    public static void WriteFloatNative(float v, ref ProtocolStream stream, Action<string>? fatal = null) =>
        WriteInt32Native(BitConverter.SingleToInt32Bits(v), ref stream, fatal);

    public static void WriteDoubleNative(double v, ref ProtocolStream stream, Action<string>? fatal = null) =>
        WriteInt64Native(BitConverter.DoubleToInt64Bits(v), ref stream, fatal);

    static int MeasureNullTerminatedLength(ProtocolStream stream, Action<string>? fatal)
    {
        var len = 0;
        while (len < stream.Length && stream.Buffer[stream.Offset + len] != 0)
            len++;
        if (len == stream.Length)
            fatal?.Invoke(ReadPastEndMessage);
        return len;
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
