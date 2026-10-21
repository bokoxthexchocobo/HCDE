namespace HCDE.Net.Core.Tests;

public class VmDNetAppendEventRecordsTests
{
    private static bool AppendOneInputByte(
        byte _,
        Span<byte> output,
        ref ulong outCursor,
        ulong cap,
        ReadOnlySpan<byte> input,
        ref ulong inCursor) =>
        VmDNetAppendEventRecords.TryAppendOutputByte(output, ref outCursor, cap, input[(int)inCursor++]);

    private static bool AppendHugePayload(
        byte _,
        Span<byte> _output,
        ref ulong outCursor,
        ulong _cap,
        ReadOnlySpan<byte> _input,
        ref ulong _inCursor)
    {
        outCursor += (ulong)ushort.MaxValue + 1u;
        return true;
    }

    [Fact]
    public void DemCommandName_KnownPause() =>
        Assert.Equal("DEM_PAUSE", VmDNetAppendEventRecords.DemCommandName(0));

    [Fact]
    public void ShouldDropClientInputEvent_WhenNotAllowed() =>
        Assert.True(VmDNetAppendEventRecords.ShouldDropClientInputEvent(true, false));

    [Fact]
    public void ValidateAppendPreconditions_RejectsCursorAtCapacity() =>
        Assert.Equal(
            VmDNetAppendEventRecords.AppendEventOutcome.CursorPastEnd,
            VmDNetAppendEventRecords.ValidateAppendPreconditions(10, 10));

    [Fact]
    public void ValidateAppendPreconditions_RequiresTwoByteHeaderRoom() =>
        Assert.Equal(
            VmDNetAppendEventRecords.AppendEventOutcome.CursorPastEnd,
            VmDNetAppendEventRecords.ValidateAppendPreconditions(9, 10));

    [Fact]
    public void EventCountOverflow_AtUInt16Max() =>
        Assert.True(VmDNetAppendEventRecords.EventCountOverflow(ushort.MaxValue));

    [Fact]
    public void ClientInputFilterRewinds_WhenDroppingDisallowedEvent()
    {
        const ulong eventStart = 4;
        const ulong cursorAfterAppend = 12;
        Assert.True(VmDNetAppendEventRecords.ClientInputFilterRewinds(
            cursorAfterAppend, eventStart, drop: true));
        Assert.Equal(eventStart, VmDNetAppendEventRecords.RewindToEventStart(eventStart));
    }

    [Fact]
    public void PayloadTooLarge_RejectsOversizedPayload() =>
        Assert.True(VmDNetAppendEventRecords.PayloadTooLarge((ulong)ushort.MaxValue + 1));

    [Fact]
    public void WriteEventCountBE16_WritesBigEndian()
    {
        Span<byte> buf = stackalloc byte[2];
        VmDNetAppendEventRecords.WriteEventCountBE16(buf, 0, 0x0102);
        Assert.Equal(0x01, buf[0]);
        Assert.Equal(0x02, buf[1]);
    }

    [Fact]
    public void DebugPackLabel_ClientInputVsSnapshot()
    {
        Assert.Equal("client-input", VmDNetAppendEventRecords.DebugPackLabel(true));
        Assert.Equal("snapshot", VmDNetAppendEventRecords.DebugPackLabel(false));
    }

    [Fact]
    public void TryReadCanonicalString_RoundTrip()
    {
        var data = new byte[] { 0, 3, (byte)'a', (byte)'b', (byte)'c' };
        var cursor = 0;
        Assert.True(VmDNetAppendEventRecords.TryReadCanonicalString(data, ref cursor, out var span));
        Assert.Equal(3, span.Length);
    }

    [Fact]
    public void TryAppendLegacyWeaponIndex_TwoByteForm()
    {
        var buf = new List<byte>();
        Assert.True(VmDNetAppendEventRecords.TryAppendLegacyWeaponIndex(buf, 200));
        Assert.Equal(2, buf.Count);
    }

    [Fact]
    public void LegacyCVarHeader_EncodesTypeInHighBits() =>
        Assert.Equal(0x45, VmDNetAppendEventRecords.LegacyCVarHeaderByte(5, 1));

    [Fact]
    public void AppendEventRecords_ClientInputFilterRewinds()
    {
        Span<byte> output = stackalloc byte[64];
        ulong cursor = 0;
        var input = new byte[] { 1, 0, 2, 0 };
        Assert.True(VmDNetAppendEventRecords.AppendEventRecords(
            output,
            ref cursor,
            (ulong)output.Length,
            input,
            clientInput: true,
            VmDNetAppendEventRecords.TryReadByte,
            _ => true,
            VmDNetAppendEventRecords.TryAppendOutputByte,
            AppendOneInputByte,
            eventType => eventType == 1));

        Assert.Equal(0x0001, (output[0] << 8) | output[1]);
        Assert.True(cursor < 10);
    }

    [Fact]
    public void AppendEventRecords_RejectsDisallowedTicEvent()
    {
        Span<byte> output = stackalloc byte[16];
        ulong cursor = 0;
        var input = new byte[] { 9 };
        Assert.False(VmDNetAppendEventRecords.AppendEventRecords(
            output, ref cursor, (ulong)output.Length, input, false,
            VmDNetAppendEventRecords.TryReadByte, _ => false,
            VmDNetAppendEventRecords.TryAppendOutputByte, AppendOneInputByte, _ => true));
    }

    [Fact]
    public void AppendEventRecords_RejectsCursorAtCapacity()
    {
        Span<byte> output = stackalloc byte[1];
        ulong cursor = 1;
        Assert.False(VmDNetAppendEventRecords.AppendEventRecords(
            output, ref cursor, 1, ReadOnlySpan<byte>.Empty, false,
            VmDNetAppendEventRecords.TryReadByte, _ => true,
            VmDNetAppendEventRecords.TryAppendOutputByte, AppendOneInputByte, _ => true));
    }

    [Fact]
    public void AppendEventRecords_WritesTwoEventPayloadSizes()
    {
        Span<byte> output = stackalloc byte[64];
        ulong cursor = 0;
        var input = new byte[] { 1, 5, 2, 7 };
        Assert.True(VmDNetAppendEventRecords.AppendEventRecords(
            output, ref cursor, (ulong)output.Length, input, false,
            VmDNetAppendEventRecords.TryReadByte, _ => true,
            VmDNetAppendEventRecords.TryAppendOutputByte, AppendOneInputByte, _ => true));
        Assert.Equal(0x0002, (output[0] << 8) | output[1]);
        Assert.Equal(1, output[2]);
        Assert.Equal(2, output[6]);
    }

    [Fact]
    public void AppendEventRecords_RejectsPayloadTooLarge()
    {
        Span<byte> output = stackalloc byte[64];
        ulong cursor = 0;
        var input = new byte[] { 1 };
        Assert.False(VmDNetAppendEventRecords.AppendEventRecords(
            output, ref cursor, (ulong)output.Length, input, false,
            VmDNetAppendEventRecords.TryReadByte, _ => true,
            VmDNetAppendEventRecords.TryAppendOutputByte,
            AppendHugePayload,
            _ => true));
    }

    [Fact]
    public void TryAppendLegacyCVarChange_BoolPayload()
    {
        var input = new byte[] { 0, 0, 1, (byte)'x', 1 };
        var cursor = 0;
        var output = new List<byte>();
        Assert.True(VmDNetAppendEventRecords.TryAppendLegacyCVarChange(8, output, input, ref cursor));
        Assert.Equal(3, output.Count);
        Assert.Equal(1, output[2]);
    }
}
