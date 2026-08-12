using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class CanonicalWeaponIndexCodec
{
    public const ushort MaxWeaponIndex = 32767;

    public static bool TryAppendFromLegacy(
        ReadOnlySpan<byte> legacy,
        ref int legacyCursor,
        Span<byte> output,
        ref int cursor)
    {
        if (legacy.Length - legacyCursor < 1)
            return false;

        var first = legacy[legacyCursor++];
        var index = (ushort)(first & 0x7f);
        if ((first & 0x80) != 0)
        {
            if (legacy.Length - legacyCursor < 1)
                return false;

            index |= (ushort)(legacy[legacyCursor++] << 7);
        }

        if (output.Length - cursor < 2)
            return false;

        BinaryPrimitives.WriteUInt16BigEndian(output[cursor..], index);
        cursor += 2;
        return true;
    }

    public static bool TryReadCanonical(
        ReadOnlySpan<byte> canonical,
        ref int cursor,
        out ushort index)
    {
        index = 0;
        if (canonical.Length - cursor < 2)
            return false;

        index = BinaryPrimitives.ReadUInt16BigEndian(canonical[cursor..]);
        cursor += 2;
        return index <= MaxWeaponIndex;
    }
}
