namespace HCDE.Gamedata.Tests;

public class HcdeEdfParseUtilitiesTests
{
    [Fact]
    public void NormalizeResourceName_CollapsesDotDot()
    {
        Assert.Equal("a/b", HcdeEdfParseUtilities.NormalizeResourceName("a/../a/b"));
    }
}
