namespace HCDE.Client;

/// <summary><c>Net_TrySkipCommand</c> from <c>d_net.cpp</c> (9261–9506).</summary>
public static class VmDNetTrySkipCommand
{
    public const int DemSay = 1;
    public const int DemTaunt = 2;
    public const int DemWarpCheat = 3;
    public const int DemZscCmd = 4;

    public static bool TryReadCStringBytes(int streamSize, int offset, ReadOnlySpan<byte> data, out int bytesWithNull)
    {
        bytesWithNull = 0;
        if (offset > streamSize) return false;
        var available = streamSize - offset;
        for (var i = 0; i < available; i++)
        {
            if (data[offset + i] == 0)
            {
                bytesWithNull = i + 1;
                return true;
            }
        }

        return false;
    }

    public static bool SkipCStringPlus(int stringBytes, int extra, out int skip) =>
        TryComputeSkip(stringBytes, extra, out skip);

    public static bool TryComputeSkip(int stringBytes, int extra, out int skip)
    {
        skip = stringBytes + extra;
        return true;
    }

    public static bool TrySkipSay(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 1, out skip);

    public static bool TrySkipAddBot(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 5, out skip);

    public static bool TrySkipGiveCheat(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 4, out skip);

    public static bool TrySkipSetInv(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 5, out skip);

    public static bool TrySkipNetEvent(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 14, out skip);

    public static bool TrySkipZscCmd(int streamSize, int stringBytes, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = stringBytes;
        if (skip > streamSize || streamSize - skip < 2) return false;
        var len = (stream[skip] << 8) | stream[skip + 1];
        skip += 2 + len;
        return true;
    }

    public static bool TrySkipSummon2(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 25, out skip);

    public static bool TrySkipChangeMap2(int stringBytes, out int skip) =>
        SkipCStringPlus(stringBytes, 1, out skip);

    public static bool TrySkipCStringOnly(int stringBytes, out int skip)
    {
        skip = stringBytes;
        return true;
    }

    public static bool TrySkipFixed(int bytes, out int skip)
    {
        skip = bytes;
        return true;
    }

    public static bool TrySkipRunScript(int argCount, out int skip)
    {
        skip = 3 + argCount * 4;
        return true;
    }

    public static bool TrySkipRunNamedScript(int streamSize, int stringBytes, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = stringBytes + 1;
        if (skip > streamSize) return false;
        skip += (stream[skip - 1] & 127) * 4;
        return true;
    }

    public static bool TrySkipSinFChanged(int streamSize, ReadOnlySpan<byte> stream, bool xorVariant, out int skip)
    {
        skip = 0;
        if (streamSize < 1) return false;
        var t = stream[0];
        skip = 1 + (t & 63);
        if (!xorVariant)
        {
            var type = t >> 6;
            skip += type switch { 0 => 1, 1 or 2 => 4, 3 => 0, _ => 0 };
            if (type == 3)
            {
                if (!TryReadCStringBytes(streamSize, skip, stream, out var sb)) return false;
                skip += sb;
            }
        }
        else skip += 1;

        return true;
    }

    public static bool TrySkipSetSlot(int streamSize, ReadOnlySpan<byte> stream, bool pnum, out int skip)
    {
        skip = 2 + (pnum ? 1 : 0);
        if (skip == 0 || skip > streamSize) return false;
        var numWeapons = stream[skip - 1];
        for (var i = 0; i < numWeapons; i++)
        {
            if (skip >= streamSize) return false;
            skip += 1 + (stream[skip] >> 7);
        }

        return true;
    }

    public static bool TrySkipAddSlot(int streamSize, ReadOnlySpan<byte> stream, out int skip)
    {
        skip = 0;
        if (streamSize < 2) return false;
        skip = 2 + (stream[1] >> 7);
        return true;
    }

    public static bool CanAdvance(int skip, int streamSize) => skip <= streamSize;

    public static int AdvanceCursor(int cursor, int skip) => cursor + skip;

    public static bool UnknownCommand(int cmd) => cmd < 0;

}
