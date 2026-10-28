namespace HCDE.Gamedata.Tests;

public class VmSkillInfoRuntimeTests
{
    [Fact]
    public void Clone_CopiesReplacementMaps()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        VmSkillInfoRuntime.SetReplacement(skill, "DoomImp", "ChaosImp");
        var copy = VmSkillInfoRuntime.Clone(skill);
        Assert.Equal("ChaosImp", VmSkillInfoRuntime.GetReplacement(copy, "DoomImp"));
    }

    [Fact]
    public void GetTextColor_EmptyIsUntranslated()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("Hey");
        Assert.Equal(VmSkillInfoRuntime.CrUntranslated, VmSkillInfoRuntime.GetTextColor(skill));
    }

    [Fact]
    public void GetReplacedBy_ReturnsMapping()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        VmSkillInfoRuntime.SetReplacedBy(skill, "ChaosImp", "DoomImp");
        Assert.Equal("DoomImp", VmSkillInfoRuntime.GetReplacedBy(skill, "ChaosImp"));
    }
}
