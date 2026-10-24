namespace HCDE.Gamedata.Tests;

public class VmMapInfoDocumentHeaderTests
{
    [Fact]
    public void MapBlockKeys_IncludesGameinfo() =>
        Assert.Contains("gameinfo", VmMapInfoDocumentHeader.MapBlockKeys);
}
