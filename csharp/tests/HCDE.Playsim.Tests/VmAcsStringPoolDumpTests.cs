namespace HCDE.Playsim.Tests;

public class VmAcsStringPoolDumpTests
{
    [Fact]
    public void ShouldListEntry_SkipsFreeSlots()
    {
        Assert.False(VmAcsStringPoolDump.ShouldListEntry(VmAcsStringPool.FreeEntry));
        Assert.True(VmAcsStringPoolDump.ShouldListEntry(1));
    }

    [Fact]
    public void FormatEntryLine_MatchesNativeLayout()
    {
        Assert.Equal("   3. ( 2) \"foo\"", VmAcsStringPoolDump.FormatEntryLine(3, 2, "foo"));
    }
}
