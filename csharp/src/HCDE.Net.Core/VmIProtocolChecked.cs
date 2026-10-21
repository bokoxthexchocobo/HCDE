namespace HCDE.Net.Core;

/// <summary>Bounds-checked protocol stream I/O (<c>i_protocol.cpp</c> 175–389).</summary>
public static class VmIProtocolChecked
{
    public static string ReadStringNative(ProtocolStream stream, Action<string>? fatal = null)
    {
        var len = 0;
        while (len < stream.Length && stream.Buffer[stream.Offset + len] != 0)
            len++;
        if (len == stream.Length)
            fatal?.Invoke("Attempted to read past end of stream");
        var s = System.Text.Encoding.UTF8.GetString(stream.Buffer, stream.Offset, len);
        Advance(ref stream, len + 1);
        return s;
    }

    public static string ReadStringConstNative(ProtocolStream stream, Action<string>? fatal = null) =>
        ReadStringNative(stream, fatal);

    public static void ReadBytesNative(byte[] dst, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (dst.Length > stream.Length)
            fatal?.Invoke("Attempted to read past end of stream");
        if (dst.Length > 0)
        {
            Array.Copy(stream.Buffer, stream.Offset, dst, 0, dst.Length);
            Advance(ref stream, dst.Length);
        }
    }

    public static byte ReadInt8Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 1)
            fatal?.Invoke("Attempted to read past end of stream");
        var v = stream.Buffer[stream.Offset];
        Advance(ref stream, 1);
        return v;
    }

    public static short ReadInt16Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 2)
            fatal?.Invoke("Attempted to read past end of stream");
        var v = (short)((stream.Buffer[stream.Offset] << 8) | stream.Buffer[stream.Offset + 1]);
        Advance(ref stream, 2);
        return v;
    }

    public static int ReadInt32Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 4)
            fatal?.Invoke("Attempted to read past end of stream");
        var v = (stream.Buffer[stream.Offset] << 24) | (stream.Buffer[stream.Offset + 1] << 16)
            | (stream.Buffer[stream.Offset + 2] << 8) | stream.Buffer[stream.Offset + 3];
        Advance(ref stream, 4);
        return v;
    }

    public static long ReadInt64Native(ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 8)
            fatal?.Invoke("Attempted to read past end of stream");
        long v = 0;
        for (var i = 0; i < 8; i++)
            v = (v << 8) | stream.Buffer[stream.Offset + i];
        Advance(ref stream, 8);
        return v;
    }

    public static float ReadFloatNative(ref ProtocolStream stream, Action<string>? fatal = null) =>
        BitConverter.Int32BitsToSingle(ReadInt32Native(ref stream, fatal));

    public static double ReadDoubleNative(ref ProtocolStream stream, Action<string>? fatal = null) =>
        BitConverter.Int64BitsToDouble(ReadInt64Native(ref stream, fatal));

    public static void WriteStringNative(string value, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        var start = stream.Offset;
        foreach (var ch in value)
        {
            if (stream.Length == 0)
                fatal?.Invoke("Attempted to write past end of stream");
            stream.Buffer[stream.Offset++] = (byte)ch;
            stream.Length--;
        }
        if (stream.Length == 0)
            fatal?.Invoke("Attempted to write past end of stream");
        stream.Buffer[stream.Offset++] = 0;
        stream.Length--;
        _ = stream.Offset - start;
    }

    public static void WriteBytesNative(byte[] source, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (source.Length > stream.Length)
            fatal?.Invoke("Attempted to write past end of stream");
        if (source.Length > 0)
        {
            Array.Copy(source, 0, stream.Buffer, stream.Offset, source.Length);
            Advance(ref stream, source.Length);
        }
    }

    public static void WriteInt8Native(byte v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 1)
            fatal?.Invoke("Attempted to write past end of stream");
        stream.Buffer[stream.Offset] = v;
        Advance(ref stream, 1);
    }

    public static void WriteInt16Native(short v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 2)
            fatal?.Invoke("Attempted to write past end of stream");
        stream.Buffer[stream.Offset] = (byte)(v >> 8);
        stream.Buffer[stream.Offset + 1] = (byte)(v & 255);
        Advance(ref stream, 2);
    }

    public static void WriteInt32Native(int v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 4)
            fatal?.Invoke("Attempted to write past end of stream");
        stream.Buffer[stream.Offset] = (byte)(v >> 24);
        stream.Buffer[stream.Offset + 1] = (byte)((v >> 16) & 255);
        stream.Buffer[stream.Offset + 2] = (byte)((v >> 8) & 255);
        stream.Buffer[stream.Offset + 3] = (byte)(v & 255);
        Advance(ref stream, 4);
    }

    public static void WriteInt64Native(long v, ref ProtocolStream stream, Action<string>? fatal = null)
    {
        if (stream.Length < 8)
            fatal?.Invoke("Attempted to write past end of stream");
        stream.Buffer[stream.Offset] = (byte)(v >> 56);
        stream.Buffer[stream.Offset + 1] = (byte)((v >> 48) & 255);
        stream.Buffer[stream.Offset + 2] = (byte)((v >> 40) & 255);
        stream.Buffer[stream.Offset + 3] = (byte)((v >> 32) & 255);
        stream.Buffer[stream.Offset + 4] = (byte)((v >> 24) & 255);
        stream.Buffer[stream.Offset + 5] = (byte)((v >> 16) & 255);
        stream.Buffer[stream.Offset + 6] = (byte)((v >> 8) & 255);
        stream.Buffer[stream.Offset + 7] = (byte)(v & 255);
        Advance(ref stream, 8);
    }

    public static void WriteFloatNative(float v, ref ProtocolStream stream, Action<string>? fatal = null) =>
        WriteInt32Native(BitConverter.SingleToInt32Bits(v), ref stream, fatal);

    public static void WriteDoubleNative(double v, ref ProtocolStream stream, Action<string>? fatal = null) =>
        WriteInt64Native(BitConverter.DoubleToInt64Bits(v), ref stream, fatal);

    private static void Advance(ref ProtocolStream stream, int bytes)
    {
        stream.Offset += bytes;
        stream.Length -= bytes;
    }
}

public struct ProtocolStream
{
    public byte[] Buffer;
    public int Offset;
    public int Length;
}
