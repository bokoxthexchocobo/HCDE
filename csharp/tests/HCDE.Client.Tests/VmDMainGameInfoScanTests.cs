namespace HCDE.Client.Tests;

public class VmDMainGameInfoScanTests
{
    [Fact]
    public void ParseStartupType_Doom()
    {
        Assert.Equal(VmDMainGameInfoScan.StartupType.Doom, VmDMainGameInfoScan.ParseStartupType("doom"));
    }

    [Fact]
    public void TryApplyGameInfoKey_StartupTitle()
    {
        var state = new VmDMainGameInfoScan.GameInfoParseState("", "", VmDMainGameInfoScan.StartupType.Default, 0, 0, "", -1, -1, -1, "", false);
        Assert.True(VmDMainGameInfoScan.TryApplyGameInfoKey("STARTUPTITLE", new[] { "HCDE" }, ref state));
        Assert.Equal("HCDE", state.StartupName);
    }

    [Fact]
    public void ShouldSetMapxxFlag_WhenAnyMap01Present()
    {
        Assert.True(VmDMainGameInfoScan.ShouldSetMapxxFlag(true, false, false));
    }

    [Fact]
    public void ReservedFolders_IncludesMaps()
    {
        Assert.Contains("maps/", VmDMainGameInfoScan.ReservedFolders);
    }
}
