namespace HCDE.Gamedata.Tests;

public class VmGSkillPropertyExport1048SliceTests
{
    [Fact]
    public void ResolveInfightingProperty_TotalAndNone()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        skill.Infighting = VmGSkillPropertyExport1048Slice.Level2TotalInfighting;
        Assert.Equal(1, VmGSkillPropertyExport1048Slice.ResolveInfightingProperty(skill, 0));
        skill.Infighting = VmGSkillPropertyExport1048Slice.Level2NoInfighting;
        Assert.Equal(-1, VmGSkillPropertyExport1048Slice.ResolveInfightingProperty(skill, 0));
    }

    [Fact]
    public void ResolveFloatSkillProperty_DoubleAmmoFlag()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("NM");
        skill.AmmoFactor = 1;
        skill.DoubleAmmoFactor = 2;
        Assert.Equal(2, VmGSkillPropertyExport1048Slice.ResolveFloatSkillProperty(
            VmSkillPropertyFloat.AmmoFactor, skill, VmGSkillPropertyExport1048Slice.Df2YesDoubleAmmo));
    }

    [Fact]
    public void ResolveDisplayedSkillName_PerClassAndDollar()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("Hey");
        skill.MenuName = "$MENU";
        skill.MenuNamesForPlayerClass["Marine"] = "MarineSkill";
        Assert.Equal("MarineSkill", VmGSkillPropertyExport1048Slice.ResolveDisplayedSkillName(
            skill, "Marine", _ => "Localized"));
    }

    [Fact]
    public void ClampActiveGameSkill_Bounds()
    {
        Assert.Equal(2, VmGSkillPropertyExport1048Slice.ClampActiveGameSkill(9, 3));
        Assert.Equal(0, VmGSkillPropertyExport1048Slice.ClampActiveGameSkill(-1, 3));
    }

    [Fact]
    public void ApplySkillInfoOperatorAssignHead_CopiesPrefixFields()
    {
        var source = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        source.FastMonsters = true;
        source.KickbackFactor = 0.5;
        var target = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        VmGSkillPropertyExport1048Slice.ApplySkillInfoOperatorAssignHead(target, source);
        Assert.Equal("UV", target.Name);
        Assert.True(target.FastMonsters);
        Assert.Equal(0.5, target.KickbackFactor);
    }

    [Fact]
    public void ResolveSkillPropertyIntTail_CoversBandCases()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("ITYTD");
        skill.AcsReturn = 2;
        skill.PlayerRespawn = true;
        Assert.Equal(2, VmGSkillPropertyExport1048Slice.ResolveSkillPropertyIntTail(
            VmSkillPropertyInt.AcsReturn, skill, 0));
        Assert.Equal(1, VmGSkillPropertyExport1048Slice.ResolveSkillPropertyIntTail(
            VmSkillPropertyInt.PlayerRespawn, skill, 0));
    }

    [Fact]
    public void ExportNames_MatchDefineActionFunctions()
    {
        Assert.Equal("G_SkillPropertyInt", VmGSkillPropertyExport1048Slice.SkillPropertyIntExportName);
        Assert.Equal("G_SkillName", VmGSkillPropertyExport1048Slice.SkillNameExportName);
    }
}
