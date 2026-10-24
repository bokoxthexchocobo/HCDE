namespace HCDE.Gamedata.Tests;

public class VmTeamNativeExportRecognitionTests
{
    [Fact]
    public void TeamZScriptExports_IncludesChangeTeam()
    {
        Assert.Contains("FTeam.ChangeTeam", VmTeamNativeExportRecognition.TeamZScriptExports);
    }

    [Fact]
    public void TeamZScriptExports_IncludesIsValid()
    {
        Assert.Contains("FTeam.IsValid", VmTeamNativeExportRecognition.TeamZScriptExports);
    }
}
