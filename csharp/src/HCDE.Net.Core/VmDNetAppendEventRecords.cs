namespace HCDE.Net.Core;

/// <summary>HCDE DEM event packing from <c>d_net.cpp</c> (3902–4109).</summary>
public static class VmDNetAppendEventRecords
{
    public static string DemCommandName(int cmd) => cmd switch
    {
        0 => "DEM_PAUSE",
        1 => "DEM_GENERICCHEAT",
        2 => "DEM_GIVECHEAT",
        3 => "DEM_TAKECHEAT",
        4 => "DEM_SETINV",
        5 => "DEM_WARPCHEAT",
        6 => "DEM_CENTERVIEW",
        7 => "DEM_UINFCHANGED",
        8 => "DEM_SINFCHANGED",
        9 => "DEM_SINFCHANGEDXOR",
        10 => "DEM_SETPITCHLIMIT",
        11 => "DEM_NETEVENT",
        12 => "DEM_WEAPSELECT",
        13 => "DEM_READIED",
        14 => "DEM_USEFLECHETTE",
        _ => "DEM_UNKNOWN",
    };

    public static bool HasCapacityForHeader(ulong cursor, ulong capacity) =>
        cursor < capacity && capacity - cursor >= 2;

    public static bool EventCountOverflow(ushort count) => count == ushort.MaxValue;

    public static bool ShouldDropClientInputEvent(bool clientInput, bool allowed) =>
        clientInput && !allowed;

    public static ulong RewindToEventStart(ulong eventRecordStart) => eventRecordStart;

    public static bool PayloadTooLarge(ulong payloadBytes) => payloadBytes > ushort.MaxValue;

    public static bool TryReadCanonicalString(ReadOnlySpan<byte> data, ref int cursor, out ReadOnlySpan<byte> stringBytes)
    {
        stringBytes = default;
        if (cursor + 2 > data.Length) return false;
        var length = (data[cursor] << 8) | data[cursor + 1];
        cursor += 2;
        if (length > data.Length - cursor) return false;
        stringBytes = data.Slice(cursor, length);
        cursor += length;
        return true;
    }

    public static bool TryAppendLegacyWeaponIndex(List<byte> output, ushort index)
    {
        if (index > 32767) return false;
        if (index < 128)
        {
            output.Add((byte)index);
            return true;
        }

        output.Add((byte)(0x80 | (index & 0x7f)));
        output.Add((byte)(index >> 7));
        return true;
    }

    public static bool TryAppendLegacyCVarHeader(List<byte> output, byte type, int nameLen) =>
        type <= 3 && nameLen > 0 && nameLen <= 63
        && output.Count + 1 + nameLen <= int.MaxValue;

    public static int LegacyCVarHeaderByte(int nameLen, byte type) => nameLen | (type << 6);

    public static int CvarPayloadBytes(byte type, int eventTypeXor) =>
        type switch
        {
            0 => 1,
            1 or 2 => 4,
            3 => -1,
            _ => eventTypeXor == 9 ? 1 : 0,
        };

    public enum AppendEventOutcome
    {
        Ok,
        CursorPastEnd,
        DisallowedType,
        ReadFailed,
        AppendFailed,
        Overflow,
    }

    public static AppendEventOutcome ValidateAppendPreconditions(ulong cursor, ulong capacity) =>
        cursor >= capacity ? AppendEventOutcome.CursorPastEnd :
        capacity - cursor < 2 ? AppendEventOutcome.CursorPastEnd : AppendEventOutcome.Ok;

    public static bool IsAllowedTicEvent(bool allowed) => allowed;

    public static ulong ReserveEventCountSlot(ulong cursor) => cursor + 2;

    public static void WriteEventCountBE16(Span<byte> buffer, int countOffset, ushort count)
    {
        buffer[countOffset] = (byte)(count >> 8);
        buffer[countOffset + 1] = (byte)count;
    }

    public static void WritePayloadSizeBE16(Span<byte> buffer, int offset, ushort size)
    {
        buffer[offset] = (byte)(size >> 8);
        buffer[offset + 1] = (byte)size;
    }

    public static bool ClientInputFilterRewinds(ulong cursor, ulong eventStart, bool drop) =>
        drop && cursor > eventStart;

    public static string DebugPackLabel(bool clientInput) => clientInput ? "client-input" : "snapshot";

    public static bool SinChangedXorEvent(int eventType) => eventType == 9;

    public static bool AppendFieldBytes(int available, int need) => available >= need;
}
