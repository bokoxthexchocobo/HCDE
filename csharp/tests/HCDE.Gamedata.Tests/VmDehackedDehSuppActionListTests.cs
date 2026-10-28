namespace HCDE.Gamedata.Tests;

public class VmDehackedDehSuppActionListTests
{
    [Fact]
    public void ResolveActionSymbol_RenamesCPosAttack()
    {
        Assert.Equal("A_CPosAttackDehacked", VmDehackedDehSuppActionList.ResolveActionSymbol("CPosAttack"));
    }

    [Fact]
    public void ParseActionList_AcceptsNullAndPointer()
    {
        var list = VmDehackedDehSuppActionList.ParseActionList(["NULL", ",", "FirePistol"]);
        Assert.Equal(2, list.Count);
        Assert.Null(list[0]);
        Assert.Equal("A_FirePistol", list[1]);
    }
}
