namespace HCDE.Gamedata.Tests;

public class VmSkillMapinfoParserTests
{
    [Fact]
    public void TryApplyOption_SetsAmmoFactor()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("Hurt Me Plenty");
        var tokens = VmLockDefTokenize.Tokenize("= 1.5");
        var i = 0;
        Assert.True(VmSkillMapinfoParser.TryApplyOption(skill, "ammofactor", tokens, ref i));
        Assert.Equal(1.5, skill.AmmoFactor);
    }
}
