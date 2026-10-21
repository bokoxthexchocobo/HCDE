namespace HCDE.Net.Core.Tests;

public class VmDNetAppendEventRecordsTests
{
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
}
