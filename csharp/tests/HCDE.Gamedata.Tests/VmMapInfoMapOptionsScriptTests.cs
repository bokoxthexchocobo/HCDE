namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsScriptTests
{
    [Fact]
    public void ApplyEdName_SetsField()
    {
        var info = new VmMapInfoLevelInfo();
        VmMapInfoMapOptionsScript.ApplyEdName(info, "MYED");
        Assert.Equal("MYED", info.EdName);
    }

    [Fact]
    public void ApplyIntroCutscene_SetsLump()
    {
        var info = new VmMapInfoLevelInfo();
        VmMapInfoMapOptionsScript.ApplyIntroCutscene(info, "INTRO");
        Assert.Equal("INTRO", info.IntroCutscene);
    }
}
