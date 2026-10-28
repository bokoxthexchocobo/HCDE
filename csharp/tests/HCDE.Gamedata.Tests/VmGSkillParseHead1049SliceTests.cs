namespace HCDE.Gamedata.Tests;

public class VmGSkillParseHead1049SliceTests
{
    [Fact]
    public void ApplyParseSkillFactoryDefaults_MatchesNativeBaseline()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("X");
        VmGSkillParseHead1049Slice.ApplyParseSkillFactoryDefaults(skill);
        Assert.Equal(1, skill.AmmoFactor);
        Assert.Equal(VmGSkillParseHead1049Slice.DropAmmoFactorUnset, skill.DropAmmoFactor);
        Assert.False(skill.FastMonsters);
        Assert.Equal(string.Empty, skill.TextColor);
    }

    [Fact]
    public void ClassifySkillParseKeyword_RecognizesHeadBandKeys()
    {
        Assert.Equal(
            VmGSkillParseHead1049Slice.SkillParseKeyword.KickbackFactor,
            VmGSkillParseHead1049Slice.ClassifySkillParseKeyword("kickbackfactor"));
        Assert.Equal(
            VmGSkillParseHead1049Slice.SkillParseKeyword.PlayerRespawn,
            VmGSkillParseHead1049Slice.ClassifySkillParseKeyword("playerrespawn"));
    }

    [Fact]
    public void TryApplySkillParseFloatProperty_SetsFactors()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("UV");
        Assert.True(VmGSkillParseHead1049Slice.TryApplySkillParseFloatProperty(
            skill, VmGSkillParseHead1049Slice.SkillParseKeyword.AmmoFactor, 0.5));
        Assert.Equal(0.5, skill.AmmoFactor);
    }

    [Fact]
    public void TryApplySkillParseFlagProperty_SetsFlags()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("NM");
        Assert.True(VmGSkillParseHead1049Slice.TryApplySkillParseFlagProperty(
            skill, VmGSkillParseHead1049Slice.SkillParseKeyword.DisableCheats));
        Assert.True(skill.DisableCheats);
    }

    [Fact]
    public void AssignSkillBlockNameFromScanner_SetsName()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("");
        VmGSkillParseHead1049Slice.AssignSkillBlockNameFromScanner(skill, "UltraViolence");
        Assert.Equal("UltraViolence", skill.Name);
    }

    [Fact]
    public void DefaultSkillIndexUnset_MatchesGlobal()
    {
        Assert.Equal(VmGSkillGlobals.DefaultSkillUnset, VmGSkillParseHead1049Slice.DefaultSkillIndexUnset);
    }

    [Fact]
    public void TryApplySkillParseFloatProperty_RejectsFlagKeywords()
    {
        var skill = VmSkillMapinfoParser.CreateDefaultSkill("HMP");
        Assert.False(VmGSkillParseHead1049Slice.TryApplySkillParseFloatProperty(
            skill, VmGSkillParseHead1049Slice.SkillParseKeyword.FastMonsters, 1));
    }
}
