namespace HCDE.Gamedata.Tests;

public class VmMapInfoLevelResetTests
{
    [Fact]
    public void CreateDefaults_SetsHexenFlags2()
    {
        var d = VmMapInfoLevelReset.CreateDefaults(true);
        Assert.Equal(0, d.Flags2);
    }

    [Fact]
    public void CreateDefaults_SetsDoomLaxMonster()
    {
        var d = VmMapInfoLevelReset.CreateDefaults(false);
        Assert.NotEqual(0, d.Flags2);
    }
}
