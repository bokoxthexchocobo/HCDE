namespace HCDE.Gamedata.Tests;

public class VmMapInfoLookupLevelNameTests
{
    [Fact]
    public void StripLocalizedHeader_RemovesMapPrefix()
    {
        var stripped = VmMapInfoLookupLevelName.StripLocalizedHeader("MAP01", "1: Entryway");
        Assert.Equal("Entryway", stripped);
    }
}
