namespace HCDE.Gamedata.Tests;

public class VmGSkillParseMid1050SliceTests
{
    [Fact]
    public void ConvertRespawnTimeSecondsToTics_UsesTicRate()
    {
        Assert.Equal(35 * 12, VmGSkillParseMid1050Slice.ConvertRespawnTimeSecondsToTics(12));
    }

    [Fact]
    public void ConvertAggressivenessInput_InvertsClamped()
    {
        Assert.Equal(0.7, VmGSkillParseMid1050Slice.ConvertAggressivenessInput(0.3));
    }

    [Fact]
    public void ApplySpawnFilterWord_AddsDifficultyBits()
    {
        var filter = VmGSkillParseMid1050Slice.ApplySpawnFilterWord(0, "nightmare");
        Assert.Equal(VmGSkillParseMid1050Slice.SpawnFilterNightmare, filter);
    }

    [Fact]
    public void ApplyReplaceActorPair_RegistersBothMaps()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        VmGSkillParseMid1050Slice.ApplyReplaceActorPair(skill, "DoomImp", "ChaosImp");
        Assert.Equal("ChaosImp", VmGSkillInfoAssignExport1047Slice.GetSkillReplacement(skill, "DoomImp"));
        Assert.Equal("DoomImp", VmGSkillInfoAssignExport1047Slice.GetSkillReplacedBy(skill, "ChaosImp"));
    }

    [Fact]
    public void TryApplySkillParseMidProperty_SetsAcsReturnFlag()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("NM");
        Assert.True(VmGSkillParseMid1050Slice.TryApplySkillParseMidProperty(
            skill,
            VmGSkillParseMid1050Slice.SkillParseMidKeyword.AcsReturn,
            0,
            3,
            "",
            null,
            out var acsSet));
        Assert.True(acsSet);
        Assert.Equal(3, skill.AcsReturn);
    }

    [Fact]
    public void FormatSkillTextColorToken_WrapsBrackets()
    {
        Assert.Equal("[Green]", VmGSkillParseMid1050Slice.FormatSkillTextColorToken("Green"));
    }

    [Fact]
    public void TryApplySkillParseMidFlag_NoInfighting()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        Assert.True(VmGSkillParseMid1050Slice.TryApplySkillParseMidFlag(
            skill, VmGSkillParseMid1050Slice.SkillParseMidKeyword.NoInfighting));
        Assert.Equal(VmGSkillParseMid1050Slice.Level2NoInfighting, skill.Infighting);
    }

    [Fact]
    public void ClassifySkillParseMidKeyword_ReplaceActor()
    {
        Assert.Equal(
            VmGSkillParseMid1050Slice.SkillParseMidKeyword.ReplaceActor,
            VmGSkillParseMid1050Slice.ClassifySkillParseMidKeyword("ReplaceActor"));
    }
}
