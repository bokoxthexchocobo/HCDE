namespace HCDE.Gamedata.Tests;

public class VmGSkillPropertyQueryTests
{
    [Fact]
    public void QueryInt_FastMonsters_HonorsDmflag()
    {
        var skills = new[] { VmSkillMapinfoParser.CreateDefaultSkill("HMP") };
        var ctx = new VmGSkillQueryContext { GameSkill = 0, DmFlags = VmGSkillQueryContext.DfFastMonsters };
        Assert.Equal(1, VmGSkillPropertyQuery.QueryInt(VmSkillPropertyInt.FastMonsters, skills, ctx));
    }

    [Fact]
    public void QueryFloat_AmmoFactor_UsesDoubleWhenFlagSet()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("NM");
        skill.AmmoFactor = 1;
        skill.DoubleAmmoFactor = 2;
        var ctx = new VmGSkillQueryContext { GameSkill = 0, DmFlags2 = VmGSkillQueryContext.Df2YesDoubleAmmo };
        Assert.Equal(2, VmGSkillPropertyQuery.QueryFloat(VmSkillPropertyFloat.AmmoFactor, [skill], ctx));
    }

    [Fact]
    public void ResolveSkillMenuName_UsesPlayerClassOverride()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        skill.MenuName = "Hurt Me Plenty";
        skill.MenuNamesForPlayerClass["Marine"] = "Custom";
        Assert.Equal("Custom", VmGSkillPropertyQuery.ResolveSkillMenuName(skill, "Marine"));
    }

    [Fact]
    public void VerifyGameSkill_ClampsHighIndex()
    {
        Assert.Equal(1, VmGSkillPropertyQuery.VerifyGameSkill(9, 2));
    }
}
