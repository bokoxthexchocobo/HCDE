namespace HCDE.Gamedata.Tests;

public class VmMapInfoLevelLookupTests
{
    [Fact]
    public void GetLevelOrDefault_SeedsDefaultSky()
    {
        var defaults = new VmMapInfoLevelRecord();
        var level = VmMapInfoLevelLookup.GetLevelOrDefault([], "MAP01", true, defaults);
        Assert.NotNull(level);
        Assert.Equal("Unnamed", level!.LevelName);
        Assert.Equal("SKY1", level.SkyPic1);
    }

    [Fact]
    public void FindLevelByWarpTrans_ScansBackwards()
    {
        var levels = new[]
        {
            new VmMapInfoLevelRecord { MapName = "MAP01", WarpTrans = 1 },
            new VmMapInfoLevelRecord { MapName = "MAP02", WarpTrans = 2 }
        };
        Assert.Equal("MAP02", VmMapInfoLevelLookup.FindLevelByWarpTrans(levels, 2)!.MapName);
    }
}
