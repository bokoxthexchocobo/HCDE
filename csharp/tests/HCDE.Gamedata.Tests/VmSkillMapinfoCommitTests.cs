namespace HCDE.Gamedata.Tests;

public class VmSkillMapinfoCommitTests
{
    [Fact]
    public void ApplyNativeDefaultFields_SetsDropAmmoFactorNegativeOne()
    {
        var skill = new VmSkillInfo();
        VmSkillMapinfoCommit.ApplyNativeDefaultFields(skill);
        Assert.Equal(-1, skill.DropAmmoFactor);
    }

    [Fact]
    public void TryUpsertSkill_ReplacesByName()
    {
        var list = new List<VmSkillInfo> { VmSkillMapinfoParser.CreateDefaultSkill("HMP") };
        var incoming = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        incoming.AmmoFactor = 0.5;
        var def = -1;
        Assert.True(VmSkillMapinfoCommit.TryUpsertSkill(list, incoming, false, false, ref def));
        Assert.Equal(0.5, list[0].AmmoFactor);
    }
}
