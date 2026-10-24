namespace HCDE.Client.Tests;

public class VmDMainCheckCmdlineMapTests
{
    [Fact]
    public void DefaultStartMap_Mapxx_UsesMap01()
    {
        Assert.Equal("MAP01", VmDMainCheckCmdlineMap.DefaultStartMap(false, true));
    }

    [Fact]
    public void TryParseSkillDigit_SubtractsOne()
    {
        Assert.True(VmDMainCheckCmdlineMap.TryParseSkillDigit("3", out var skill));
        Assert.Equal(2, skill);
    }

    [Fact]
    public void ParseWarpEpisode_SwapsWhenMapOutOfRange()
    {
        var (ep, map) = VmDMainCheckCmdlineMap.ParseWarpEpisode("5", "12");
        Assert.Equal((1, 5), (ep, map));
    }

    [Fact]
    public void IsLauncherDefaultMap_AcceptsE1M1()
    {
        Assert.True(VmDMainCheckCmdlineMap.IsLauncherDefaultMap("e1m1"));
    }

    [Fact]
    public void ShouldReplaceLauncherDefault_WhenEpisodeDiffers()
    {
        Assert.True(VmDMainCheckEpisodeCmd.ShouldReplaceLauncherDefault(
            true, 1, "MAP02", "MAP01"));
    }

    [Fact]
    public void TryParseEpisodeArg_RejectsHighIndex()
    {
        Assert.True(VmDMainCheckEpisodeCmd.TryParseEpisodeArg("9", 3, out _, out var valid));
        Assert.False(valid);
    }
}
