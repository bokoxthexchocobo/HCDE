namespace HCDE.Gamedata.Tests;

public class VmGSkillInfoAssignExport1047SliceTests
{
    [Fact]
    public void ApplySkillInfoOperatorAssign_CopiesTailFieldsAndMaps()
    {
        var source = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        source.DisableCheats = true;
        source.AcsReturn = 4;
        VmGSkillInfoAssignExport1047Slice.SetSkillReplacement(source, "Imp", "ChaosImp");
        var target = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        VmGSkillInfoAssignExport1047Slice.ApplySkillInfoOperatorAssign(target, source);
        Assert.Equal("UV", target.Name);
        Assert.True(target.DisableCheats);
        Assert.Equal(4, target.AcsReturn);
        Assert.Equal("ChaosImp", VmGSkillInfoAssignExport1047Slice.GetSkillReplacement(target, "Imp"));
    }

    [Fact]
    public void ResolveSkillInfoTextColor_EmptyUsesUntranslated()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("Hey");
        Assert.Equal(
            VmGSkillInfoAssignExport1047Slice.CrUntranslated,
            VmGSkillInfoAssignExport1047Slice.ResolveSkillInfoTextColor(skill, _ => 5));
    }

    [Fact]
    public void ResolveSkillInfoTextColor_LogsUndefined()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("NM");
        skill.TextColor = "BadColor";
        string? logged = null;
        var color = VmGSkillInfoAssignExport1047Slice.ResolveSkillInfoTextColor(
            skill,
            _ => null,
            msg => logged = msg);
        Assert.Equal(VmGSkillInfoAssignExport1047Slice.CrUntranslated, color);
        Assert.Contains("Undefined color", logged);
    }

    [Fact]
    public void ReplacementMaps_RoundTrip()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("ITYTD");
        VmGSkillInfoAssignExport1047Slice.SetSkillReplacedBy(skill, "ChaosImp", "DoomImp");
        Assert.Equal("DoomImp", VmGSkillInfoAssignExport1047Slice.GetSkillReplacedBy(skill, "ChaosImp"));
        Assert.Null(VmGSkillInfoAssignExport1047Slice.GetSkillReplacement(skill, "Missing"));
    }

    [Fact]
    public void DefineFieldExportNames_IncludesAmmoFactor()
    {
        Assert.True(VmGSkillInfoAssignExport1047Slice.IsExportedSkillInfoField("AmmoFactor"));
        Assert.False(VmGSkillInfoAssignExport1047Slice.IsExportedSkillInfoField("NotAField"));
    }

    [Fact]
    public void InvokeNativeGetTextColorAction_MatchesInstanceMethod()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HNT");
        skill.TextColor = "Green";
        Assert.Equal(3, VmGSkillInfoAssignExport1047Slice.InvokeNativeGetTextColorAction(skill, _ => 3));
    }

    [Fact]
    public void AllSkillsGlobalExportName_MatchesDefineGlobal()
    {
        Assert.Equal("AllSkills", VmGSkillInfoAssignExport1047Slice.AllSkillsGlobalExportName);
    }

    [Fact]
    public void FormatUndefinedSkillTextColorMessage_IncludesSkillName()
    {
        Assert.Contains(
            "skill UV",
            VmGSkillInfoAssignExport1047Slice.FormatUndefinedSkillTextColorMessage("X", "UV"));
    }
}
