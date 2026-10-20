namespace HCDE.Gamedata.Tests;

public class VmGSkillNativeExportRecognitionTests
{
    [Fact]
    public void SkillZScriptExports_IncludesPropertyInt()
    {
        Assert.Contains("G_SkillPropertyInt", VmGSkillNativeExportRecognition.SkillZScriptExports);
    }

    [Fact]
    public void SkillVmFields_IncludesAmmoFactor()
    {
        Assert.Contains("AmmoFactor", VmGSkillNativeExportRecognition.SkillVmFields);
    }
}
