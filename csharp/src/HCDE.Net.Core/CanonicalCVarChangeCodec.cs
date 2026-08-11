namespace HCDE.Net.Core;

public static class CanonicalCVarChangeCodec
{
    public const byte CvarBool = 0;
    public const byte CvarInt = 1;
    public const byte CvarFloat = 2;
    public const byte CvarString = 3;

    public static bool TryBuildFromLegacy(
        ReadOnlySpan<byte> legacy,
        ref int legacyCursor,
        Span<byte> output,
        ref int cursor,
        bool xorVariant)
    {
        if (legacy.Length - legacyCursor < 1)
            return false;

        var descriptor = legacy[legacyCursor++];
        var type = (byte)(descriptor >> 6);
        var nameBytes = descriptor & 0x3F;
        if (type > CvarString || nameBytes == 0 || legacy.Length - legacyCursor < nameBytes)
            return false;

        output[cursor++] = type;
        if (CanonicalStringCodec.Write(output, ref cursor, legacy.Slice(legacyCursor, nameBytes)) == 0)
            return false;

        legacyCursor += nameBytes;

        if (xorVariant)
            return TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 1);

        return type switch
        {
            CvarBool => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 1),
            CvarInt or CvarFloat => TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 4),
            CvarString => CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var stringBytes)
                && CanonicalStringCodec.Write(output, ref cursor, stringBytes) > 0,
            _ => false,
        };
    }

    private static bool TryCopyFixedLegacy(ReadOnlySpan<byte> legacy, ref int legacyCursor, Span<byte> output, ref int cursor, int size)
    {
        if (legacy.Length - legacyCursor < size || output.Length - cursor < size)
            return false;

        legacy.Slice(legacyCursor, size).CopyTo(output[cursor..]);
        legacyCursor += size;
        cursor += size;
        return true;
    }
}
