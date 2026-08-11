using System.Buffers.Binary;
using System.Text;

namespace HCDE.Net.Core;

public static class CanonicalStringCodec
{
    public static int Write(Span<byte> output, ref int cursor, ReadOnlySpan<byte> stringBytes)
    {
        if (stringBytes.Length > ushort.MaxValue || output.Length - cursor < 2 + stringBytes.Length)
            return 0;

        BinaryPrimitives.WriteUInt16BigEndian(output[cursor..], (ushort)stringBytes.Length);
        cursor += 2;
        stringBytes.CopyTo(output[cursor..]);
        cursor += stringBytes.Length;
        return 2 + stringBytes.Length;
    }

    public static int WriteAscii(Span<byte> output, ref int cursor, string value)
    {
        var bytes = Encoding.ASCII.GetBytes(value);
        return Write(output, ref cursor, bytes);
    }

    public static bool TryRead(ReadOnlySpan<byte> input, ref int cursor, out ReadOnlySpan<byte> stringBytes)
    {
        stringBytes = default;
        if (input.Length - cursor < 2)
            return false;

        var length = BinaryPrimitives.ReadUInt16BigEndian(input[cursor..]);
        cursor += 2;
        if (input.Length - cursor < length)
            return false;

        stringBytes = input.Slice(cursor, length);
        cursor += length;
        return true;
    }

    public static bool TryReadLegacyNullTerminated(ReadOnlySpan<byte> input, ref int cursor, out ReadOnlySpan<byte> stringBytes)
    {
        stringBytes = default;
        if (cursor >= input.Length)
            return false;

        var start = cursor;
        while (cursor < input.Length && input[cursor] != 0)
            cursor++;

        if (cursor >= input.Length)
            return false;

        stringBytes = input[start..cursor];
        cursor++;
        return true;
    }
}
