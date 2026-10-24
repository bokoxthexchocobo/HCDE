namespace HCDE.Gamedata.Tests;

public class VmGSkillGlobalsTests
{
    [Fact]
    public void CreateEmptySkillList_StartsEmpty()
    {
        Assert.Empty(VmGSkillGlobals.CreateEmptySkillList());
    }
}
