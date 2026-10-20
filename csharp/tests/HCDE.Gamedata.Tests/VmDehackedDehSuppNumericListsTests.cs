namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppNumericListsTests
{
    [Fact]
    public void ParseIntList_ReadsOrgHeights()
    {
        var list = VmDehackedDehSuppNumericLists.ParseIntList(["56", ",", "28"]);
        Assert.Equal([56, 28], list);
    }

    [Fact]
    public void ParseIntList_ReadsCodePConv()
    {
        var list = VmDehackedDehSuppNumericLists.ParseIntList(["1", ",", "2", ",", "3"]);
        Assert.Equal([1, 2, 3], list);
    }
}
