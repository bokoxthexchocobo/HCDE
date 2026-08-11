namespace HCDE.Net.Core;

public static class CanonicalRunArgsCodec
{
    public static bool TryBuildFromLegacy(
        ReadOnlySpan<byte> legacy,
        ref int legacyCursor,
        Span<byte> output,
        ref int cursor,
        bool named)
    {
        byte argCount;
        if (named)
        {
            if (!CanonicalStringCodec.TryReadLegacyNullTerminated(legacy, ref legacyCursor, out var nameBytes)
                || CanonicalStringCodec.Write(output, ref cursor, nameBytes) == 0
                || legacy.Length - legacyCursor < 1)
                return false;

            argCount = legacy[legacyCursor++];
            output[cursor++] = (byte)(argCount & 127);
        }
        else
        {
            if (!TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, 2)
                || legacy.Length - legacyCursor < 1)
                return false;

            argCount = legacy[legacyCursor++];
            output[cursor++] = argCount;
        }

        return TryCopyFixedLegacy(legacy, ref legacyCursor, output, ref cursor, argCount * 4);
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
