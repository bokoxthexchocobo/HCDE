namespace HCDE.Gamedata.Tests;

public class VmMapInfoLumpPrecedenceTests
{
    [Fact]
    public void ShouldSkipClassicMapinfo_WhenZmapinfoPresent()
    {
        Assert.True(VmMapInfoLumpPrecedence.ShouldSkipLump(
            VmMapInfoLumpPrecedence.LumpIndexMapinfo, hasZmapinfo: true, hasMapinfo: false, hasUmapinfo: false));
    }

    [Fact]
    public void ShouldSkipEmapinfo_WhenNativeMapinfoInSameWad()
    {
        Assert.True(VmMapInfoLumpPrecedence.ShouldSkipLump(
            VmMapInfoLumpPrecedence.LumpIndexEmapinfo, hasZmapinfo: false, hasMapinfo: true, hasUmapinfo: false));
    }

    [Fact]
    public void ShouldSkipUmapinfo_WhenZmapinfoPresent()
    {
        Assert.True(VmMapInfoLumpPrecedence.ShouldSkipUmapinfo(true, false));
        Assert.False(VmMapInfoLumpPrecedence.ShouldSkipUmapinfo(false, false));
    }
}
