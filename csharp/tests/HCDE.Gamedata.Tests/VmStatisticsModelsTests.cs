namespace HCDE.Gamedata.Tests;

public class VmStatisticsModelsTests
{
    [Fact]
    public void StatFileDefault_MatchesNativeCvar()
    {
        Assert.Equal("zdoomstat.txt", VmStatisticsConfig.StatFileDefault);
    }
}
