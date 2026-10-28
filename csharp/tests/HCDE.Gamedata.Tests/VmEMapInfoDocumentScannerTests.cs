namespace HCDE.Gamedata.Tests;

public class VmEMapInfoDocumentScannerTests
{
    [Fact]
    public void TryParseSectionHeader_ReadsMap()
    {
        Assert.True(VmEMapInfoDocumentScanner.TryParseSectionHeader("[ map01 ]", out var map));
        Assert.Equal("MAP01", map);
    }
}
