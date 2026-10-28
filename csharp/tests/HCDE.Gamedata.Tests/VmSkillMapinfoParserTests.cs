namespace HCDE.Gamedata.Tests;

public class VmSkillMapinfoParserTests
{
    [Fact]
    public void TryApplyFlag_SetsFastMonsters()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        Assert.True(VmSkillMapinfoParser.TryApplyFlag(skill, "fastmonsters"));
        Assert.True(skill.FastMonsters);
    }

    [Fact]
    public void TryApplyOption_SetsAmmoFactor()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("Hurt Me Plenty");
        var tokens = VmLockDefTokenize.Tokenize("= 1.5");
        var i = 0;
        Assert.True(VmSkillMapinfoParser.TryApplyOption(skill, "ammofactor", tokens, ref i));
        Assert.Equal(1.5, skill.AmmoFactor);
    }

    [Fact]
    public void AssignSkillNameFromToken_SetsName()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("");
        VmSkillMapinfoParser.AssignSkillNameFromToken(skill, "hmp");
        Assert.Equal("hmp", skill.Name);
    }

    [Fact]
    public void TryApplyBareFlag_SetsNoPain()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        Assert.True(VmSkillMapinfoParser.TryApplyBareFlag(skill, "nopain"));
        Assert.True(skill.NoPain);
    }
}
