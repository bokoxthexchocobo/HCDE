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
}
