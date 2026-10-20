namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppNumericListsTests
{
    [Fact]
    public void ParseIntList_ReadsOrgHeights()
    {
        var list = VmDehackedDehSuppNumericLists.ParseIntList(["56", ",", "28"]);
        Assert.Equal([56, 28], list);
    }
}
