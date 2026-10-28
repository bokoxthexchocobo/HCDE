namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppAmmoNamesTests
{
    [Fact]
    public void ParseAmmoNameList_AcceptsNullAndTypes()
    {
        var list = VmDehackedDehSuppAmmoNames.ParseAmmoNameList(["NULL", ",", "Clip"]);
        Assert.Equal(2, list.Count);
        Assert.Null(list[0]);
        Assert.Equal("Clip", list[1]);
    }
}
