namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapInfo1029SliceTests
{
    [Fact]
    public void ApplySetLevelNum_ClearsDuplicatesOnOtherMaps()
    {
        var a = new VmMapInfoParseMapInfo1029Slice.LevelNumEntry { LevelNum = 5, Id24LevelNum = 9 };
        var b = new VmMapInfoParseMapInfo1029Slice.LevelNumEntry { LevelNum = 5 };
        var c = new VmMapInfoParseMapInfo1029Slice.LevelNumEntry { LevelNum = 2 };
        VmMapInfoParseMapInfo1029Slice.ApplySetLevelNum([a, b, c], b, 5);
        Assert.Equal(0, a.LevelNum);
        Assert.Equal(5, b.LevelNum);
        Assert.Equal(0, b.Id24LevelNum);
    }

    [Fact]
    public void ShouldFatalOnCoreIncludeOverride_DetectsCoreOverride()
    {
        Assert.True(VmMapInfoParseMapInfo1029Slice.ShouldFatalOnCoreIncludeOverride(parserFileContainer: 0, includeFileContainer: 2));
        Assert.False(VmMapInfoParseMapInfo1029Slice.ShouldFatalOnCoreIncludeOverride(1, 2));
    }

    [Fact]
    public void ResolveSkyPic2WhenNoFlat_CopiesPrimarySky()
    {
        Assert.Equal("SKY1", VmMapInfoParseMapInfo1029Slice.ResolveSkyPic2WhenNoFlat("-NOFLAT-", "SKY1"));
    }

    [Fact]
    public void TryParseTopLevelKeyword_RejectsGameInfoOnOldFormat()
    {
        Assert.False(VmMapInfoParseMapInfo1029Slice.TryParseTopLevelKeyword(
            "gameinfo", VmMapInfoParseMapInfo1029Slice.FmtOld, out _, out var err));
        Assert.Contains("old MAPINFO", err);
    }

    [Fact]
    public void TryParseTopLevelKeyword_AllowsClusterKeywordOnNewFormat()
    {
        Assert.True(VmMapInfoParseMapInfo1029Slice.TryParseTopLevelKeyword(
            "cluster", VmMapInfoParseMapInfo1029Slice.FmtNew, out var cmd, out _));
        Assert.Equal(VmMapInfoParseMapInfo1029Slice.MapInfoTopLevelCommand.Cluster, cmd);
    }

    [Fact]
    public void TryParseTopLevelKeyword_MapsClearsSkills()
    {
        Assert.True(VmMapInfoParseMapInfo1029Slice.TryParseTopLevelKeyword(
            "clearskills", VmMapInfoParseMapInfo1029Slice.FmtOld, out var cmd, out _));
        Assert.Equal(VmMapInfoParseMapInfo1029Slice.MapInfoTopLevelCommand.ClearSkills, cmd);
    }

    [Fact]
    public void LumpAlreadyParsed_TracksDuplicates()
    {
        Assert.True(VmMapInfoParseMapInfo1029Slice.LumpAlreadyParsed([1, 2, 3], 2));
        Assert.False(VmMapInfoParseMapInfo1029Slice.LumpAlreadyParsed([1, 2], 9));
    }
}
