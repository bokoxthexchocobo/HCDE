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

    public static bool EventCountOverflow(ushort count) => count == ushort.MaxValue;

    public static bool ShouldDropClientInputEvent(bool clientInput, bool allowed) =>
        clientInput && !allowed;

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

    public static int LegacyCVarHeaderByte(int nameLen, byte type) => nameLen | (type << 6);

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

    public static bool SinChangedXorEvent(int eventType) => eventType == 9;

    public delegate bool ReadByteDelegate(ReadOnlySpan<byte> data, ref ulong cursor, out byte value);

    public delegate bool AppendByteDelegate(Span<byte> output, ref ulong cursor, ulong capacity, byte value);

    public delegate bool AppendPayloadDelegate(
        byte eventType,
        Span<byte> output,
        ref ulong cursor,
        ulong capacity,
        ReadOnlySpan<byte> input,
        ref ulong inputCursor);

    /// <summary>Mirrors <c>HCDEAppendEventRecords</c> control flow from <c>d_net.cpp</c> (3932–4026).</summary>
    public static bool AppendEventRecords(
        Span<byte> output,
        ref ulong cursor,
        ulong outputCapacity,
        ReadOnlySpan<byte> input,
        bool clientInput,
        ReadByteDelegate readByte,
        Func<byte, bool> isAllowedTicEventType,
        AppendByteDelegate appendByte,
        AppendPayloadDelegate appendCanonicalPayload,
        Func<byte, bool> isAllowedClientInputEventType)
    {
        if (ValidateAppendPreconditions(cursor, outputCapacity) != AppendEventOutcome.Ok)
            return false;

        var countOffset = cursor;
        cursor += 2;
        ushort eventCount = 0;
        ulong inputCursor = 0;
        while (inputCursor < (ulong)input.Length)
        {
            if (!readByte(input, ref inputCursor, out var eventType))
                return false;
            if (!isAllowedTicEventType(eventType))
                return false;
            if (EventCountOverflow(eventCount))
                return false;

            var eventRecordStart = cursor;
            if (!appendByte(output, ref cursor, outputCapacity, eventType))
                return false;
            if (ValidateAppendPreconditions(cursor, outputCapacity) != AppendEventOutcome.Ok)
                return false;

            var payloadSizeOffset = cursor;
            cursor += 2;
            var payloadStart = cursor;
            if (!appendCanonicalPayload(eventType, output, ref cursor, outputCapacity, input, ref inputCursor))
                return false;

            var payloadBytes = cursor - payloadStart;
            if (PayloadTooLarge(payloadBytes))
                return false;

            if (ShouldDropClientInputEvent(clientInput, isAllowedClientInputEventType(eventType)))
            {
                cursor = eventRecordStart;
                continue;
            }

            WritePayloadSizeBE16(output, (int)payloadSizeOffset, (ushort)payloadBytes);
            eventCount++;
        }

        WriteEventCountBE16(output, (int)countOffset, eventCount);
        return true;
    }

    public static bool TryReadByte(ReadOnlySpan<byte> data, ref ulong cursor, out byte value)
    {
        if (cursor >= (ulong)data.Length)
        {
            value = 0;
            return false;
        }

        value = data[(int)cursor];
        cursor++;
        return true;
    }

    public static bool TryAppendOutputByte(Span<byte> output, ref ulong cursor, ulong capacity, byte value)
    {
        if (cursor >= capacity)
            return false;
        output[(int)cursor] = value;
        cursor++;
        return true;
    }

    public static bool TryAppendLegacyCVarChange(
        byte eventType,
        List<byte> output,
        ReadOnlySpan<byte> input,
        ref int inputCursor)
    {
        if (inputCursor >= input.Length)
            return false;
        var type = input[inputCursor++];
        if (type > 3)
            return false;
        if (!TryReadCanonicalString(input, ref inputCursor, out var nameBytes)
            || nameBytes.Length == 0
            || nameBytes.Length > 63)
        {
            return false;
        }

        output.Add((byte)LegacyCVarHeaderByte(nameBytes.Length, type));
        foreach (var b in nameBytes)
            output.Add(b);

        if (SinChangedXorEvent(eventType))
            return TryCopyFieldBytes(output, input, ref inputCursor, 1);

        return type switch
        {
            0 => TryCopyFieldBytes(output, input, ref inputCursor, 1),
            1 or 2 => TryCopyFieldBytes(output, input, ref inputCursor, 4),
            3 => TryAppendLegacyStringFromCanonical(output, input, ref inputCursor),
            _ => false,
        };
    }

    private static bool TryCopyFieldBytes(List<byte> output, ReadOnlySpan<byte> input, ref int inputCursor, int count)
    {
        if (inputCursor + count > input.Length)
            return false;
        for (var i = 0; i < count; i++)
            output.Add(input[inputCursor++]);
        return true;
    }

    private static bool TryAppendLegacyStringFromCanonical(List<byte> output, ReadOnlySpan<byte> input, ref int inputCursor)
    {
        if (!TryReadCanonicalString(input, ref inputCursor, out var stringBytes))
            return false;
        foreach (var b in stringBytes)
            output.Add(b);
        output.Add(0);
        return true;
    }
}
