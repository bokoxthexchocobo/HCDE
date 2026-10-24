namespace HCDE.Gamedata.Tests;

public class VmMapInfoLevelScalarsTests
{
    [Fact]
    public void ApplyTeamDamage_StoresValue()
    {
        var info = new VmMapInfoLevelInfo();
        VmMapInfoLevelScalars.ApplyTeamDamage(info, 0.5);
        Assert.Equal(0.5, info.TeamDamage);
    }

    [Fact]
    public void ParseFogDensity_ClampsAndHalves()
    {
        Assert.Equal(256, VmMapInfoLevelScalars.ParseFogDensity(600));
        Assert.Equal(0, VmMapInfoLevelScalars.ParseFogDensity(-5));
    }

    [Fact]
    public void ParseLightMode_TreatsEightAsNotSet()
    {
        Assert.Equal(VmMapInfoLightMode.NotSet, VmMapInfoLevelScalars.ParseLightMode(8));
        Assert.Equal(VmMapInfoLightMode.Hexen, VmMapInfoLevelScalars.ParseLightMode(4));
    }

    [Fact]
    public void ParseThickFogMultiplier_IgnoresNonPositive()
    {
        Assert.Null(VmMapInfoLevelScalars.ParseThickFogMultiplier(0));
        Assert.Equal(1.5f, VmMapInfoLevelScalars.ParseThickFogMultiplier(1.5));
    }
}
