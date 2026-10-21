using System.Buffers.Binary;

namespace HCDE.Net.Core;

/// <summary><c>HCDEAppendCanonicalString</c>, null-string, and weapon-index helpers from <c>d_net.cpp</c> (3642–3684).</summary>
public static class VmDNetCanonicalPrimitive
{
    public static bool TryAppendCanonicalString(Span<byte> output, ref int cursor, ReadOnlySpan<byte> stringBytes)
    {
        if (stringBytes.Length > ushort.MaxValue)
            return false;

        return CanonicalStringCodec.Write(output, ref cursor, stringBytes) > 0;
    }

    public static bool TryAppendCanonicalNullString(
        ReadOnlySpan<byte> data,
        ref int inputCursor,
        Span<byte> output,
        ref int outputCursor)
    {
        if (inputCursor >= data.Length)
            return false;

        if (!CanonicalStringCodec.TryReadLegacyNullTerminated(data, ref inputCursor, out var stringBytes))
            return false;

        return TryAppendCanonicalString(output, ref outputCursor, stringBytes);
    }

    public static bool TryAppendCanonicalWeaponIndex(
        ReadOnlySpan<byte> data,
        ref int inputCursor,
        Span<byte> output,
        ref int outputCursor)
    {
        if (!TryReadByteField(data, ref inputCursor, out var first))
            return false;

        var index = (ushort)(first & 0x7fu);
        if ((first & 0x80u) != 0)
        {
            if (!TryReadByteField(data, ref inputCursor, out var high))
                return false;
            index |= (ushort)(high << 7);
        }

        if (output.Length - outputCursor < 2)
            return false;

        BinaryPrimitives.WriteUInt16BigEndian(output[outputCursor..], index);
        outputCursor += 2;
        return true;
    }

    public static bool TryReadByteField(ReadOnlySpan<byte> data, ref int cursor, out byte value)
    {
        value = 0;
        if (cursor >= data.Length)
            return false;
        value = data[cursor++];
        return true;
    }
}
