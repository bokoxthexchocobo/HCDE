namespace HCDE.Gamedata.Tests;

public class VmGiNativeExportRecognitionTests
{
    [Fact]
    public void GameInfoVmFields_IncludesGibfactor()
    {
        Assert.Contains("gibfactor", VmGiNativeExportRecognition.GameInfoVmFields);
    }

    [Fact]
    public void GameInfoMacroKeys_IncludesTitleMusic()
    {
        Assert.Contains("titleMusic", VmGameInfoStaticSchema.GameInfoMacroKeys);
    }
}
