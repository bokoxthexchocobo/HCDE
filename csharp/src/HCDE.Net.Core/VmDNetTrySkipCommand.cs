namespace HCDE.Net.Core;

/// <summary><c>Net_TrySkipCommand</c> size logic mirrored from <c>d_net.cpp</c> (9261+).</summary>
public static class VmDNetTrySkipCommand
{
    public static bool TryReadCStringBytes(int streamSize, int offset, ReadOnlySpan<byte> data, out int bytesWithNull)
    {
        bytesWithNull = 0;
        if (offset > streamSize)
            return false;
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

    public static ulong SkipSayOrTauntNative(int stringBytes) => 1ul + (ulong)stringBytes;

    public static ulong SkipAddBotNative(int stringBytes) => (ulong)stringBytes + 5ul;

    public static ulong SkipGiveOrTakeCheatNative(int stringBytes) => (ulong)stringBytes + 4ul;

    public static ulong SkipSetInvNative(int stringBytes) => (ulong)stringBytes + 5ul;

    public static ulong SkipNetEventNative(int stringBytes) => (ulong)stringBytes + 14ul;

    public static bool TrySkipZscCmdNative(int streamSize, int stringBytes, ReadOnlySpan<byte> data, out ulong skip)
    {
        skip = (ulong)stringBytes;
        if (skip > (ulong)streamSize || streamSize - (int)skip < 2)
            return false;
        var size = (data[(int)skip] << 8) | data[(int)skip + 1];
        skip += 2ul + (ulong)size;
        return true;
    }

    public static ulong SkipSummon2FamilyNative(int stringBytes) => (ulong)stringBytes + 25ul;

    public static ulong SkipWarpCheatNative() => 6ul;

    public static ulong SkipInvUseOrFovNative() => 4ul;

    public static ulong SkipInvDropNative() => 8ul;

    public static ulong SkipKickFamilyNative() => 1ul;

    public static bool UnknownCommandFailsClosedNative(int cmd) => cmd < 0;

    public static bool TrySkipSavegameNative(
        int streamSize,
        int firstStringBytes,
        ReadOnlySpan<byte> data,
        out ulong skip)
    {
        skip = (ulong)firstStringBytes;
        if (!TryReadCStringBytes(streamSize, (int)skip, data, out var secondLen))
            return false;
        skip += (ulong)secondLen;
        return true;
    }

    public static int CvarSkipPayloadBytesNative(int typeTag) =>
        typeTag switch
        {
            0 => 1,
            1 or 2 => 4,
            _ => 0,
        };

    public static ulong SinfxChangedBaseSkipNative(int typeByte) => 1ul + (ulong)(typeByte & 63);

    public static bool IsSinChangedXorNative(int cmd, int demSinfChangedXor) => cmd == demSinfChangedXor;

    public static ulong SkipRunScriptNative(ReadOnlySpan<byte> data) =>
        data.Length >= 3 ? 3ul + (ulong)data[2] * 4ul : 0ul;

    public static bool TrySkipRunNamedScriptNative(
        int streamSize,
        int stringBytes,
        ReadOnlySpan<byte> data,
        out ulong skip)
    {
        skip = (ulong)stringBytes + 1ul;
        if (skip > (ulong)streamSize)
            return false;
        skip += (ulong)(data[(int)skip - 1] & 127) * 4ul;
        return true;
    }

    public static ulong SkipRunSpecialNative(ReadOnlySpan<byte> data) =>
        data.Length >= 3 ? 3ul + (ulong)data[2] * 4ul : 0ul;

    public static ulong SkipConvReplyNative() => 3ul;

    public static bool TrySkipSetSlotFamilyNative(
        int cmd,
        int demSetSlotPnum,
        int streamSize,
        ReadOnlySpan<byte> data,
        out ulong skip)
    {
        skip = cmd == demSetSlotPnum ? 3ul : 2ul;
        if (skip == 0 || skip > (ulong)streamSize)
            return false;
        var numWeapons = data[(int)skip - 1];
        for (; numWeapons > 0; numWeapons--)
        {
            if (skip >= (ulong)streamSize)
                return false;
            skip += 1ul + (ulong)(data[(int)skip] >> 7);
        }
        return true;
    }

    public static ulong SkipAddSlotFamilyNative(ReadOnlySpan<byte> data) =>
        data.Length >= 2 ? 2ul + (ulong)(data[1] >> 7) : 0ul;

    public static ulong SkipSetPitchLimitNative() => 2ul;

    public static bool SkipExceedsStreamNative(ulong skip, int streamSize) => skip > (ulong)streamSize;

    public static ulong PayloadlessSkipSizeNative(int payloadBytes) => (ulong)payloadBytes;
}
