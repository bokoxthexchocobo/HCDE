namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseHead1051SliceTests
{
    [Fact]
    public void FindWadLevelIndex_IsCaseInsensitive()
    {
        var levels = new[] { new VmMapInfoLevelRecord { MapName = "MAP01" } };
        Assert.Equal(0, VmMapInfoParseHead1051Slice.FindWadLevelIndex(levels, "map01"));
    }

    [Fact]
    public void FindLevelInfo_SeedsDefaultWhenAllowed()
    {
        var defaults = new VmMapInfoLevelRecord();
        var lev = VmMapInfoParseHead1051Slice.FindLevelInfo([], "MISSING", true, defaults);
        Assert.NotNull(lev);
        Assert.Equal("Unnamed", lev!.LevelName);
        Assert.Equal("SKY1", lev.SkyPic1);
        Assert.Equal("SKYMIST1", lev.SkyMistPic);
    }

    [Fact]
    public void FindLevelByNum_MatchesLevelNumField()
    {
        var levels = new[]
        {
            new VmMapInfoLevelRecord { MapName = "MAP01", LevelNum = 1 },
            new VmMapInfoLevelRecord { MapName = "MAP02", LevelNum = 2 }
        };
        Assert.Equal("MAP02", VmMapInfoParseHead1051Slice.FindLevelByNum(levels, 2)!.MapName);
    }

    [Fact]
    public void FindLevelByWarpTrans_ScansFromEnd()
    {
        var levels = new[]
        {
            new VmMapInfoLevelRecord { MapName = "MAP01", WarpTrans = 7 },
            new VmMapInfoLevelRecord { MapName = "MAP07", WarpTrans = 7 }
        };
        Assert.Equal("MAP07", VmMapInfoParseHead1051Slice.FindLevelByWarpTrans(levels, 7)!.MapName);
    }

    [Fact]
    public void TryResolveCheckWarpTransMap_ReplacesWithLevelName()
    {
        var levels = new[] { new VmMapInfoLevelRecord { MapName = "MAP07", WarpTrans = 7 } };
        Assert.True(VmMapInfoParseHead1051Slice.TryResolveCheckWarpTransMap(
            "&wt@07", false, levels, out var resolved));
        Assert.Equal("MAP07", resolved);
    }

    [Fact]
    public void TryResolveCheckWarpTransMap_SubstituteBuildsMapPrefix()
    {
        Assert.False(VmMapInfoParseHead1051Slice.TryResolveCheckWarpTransMap(
            "&wt@99", true, [], out var resolved));
        Assert.Equal("MAP99", resolved);
    }

    [Fact]
    public void IsWarpTransMapPrefix_RequiresWtAt()
    {
        Assert.True(VmMapInfoParseHead1051Slice.IsWarpTransMapPrefix("&WT@1"));
        Assert.False(VmMapInfoParseHead1051Slice.IsWarpTransMapPrefix("MAP01"));
    }

    [Fact]
    public void ParsedLumpsInitialCapacity_MatchesNativeDefault()
    {
        Assert.Equal(8, VmMapInfoParseHead1051Slice.ParsedLumpsInitialCapacity);
    }
}
