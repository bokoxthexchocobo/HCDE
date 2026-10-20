namespace HCDE.Gamedata.Tests;

public class VmEMapInfoLevelBootstrapTests
{
    [Fact]
    public void SetLevelNum_ClearsDuplicate()
    {
        var a = new VmEMapInfoWadLevel { LevelNum = 5 };
        var b = new VmEMapInfoWadLevel();
        var levels = new List<VmEMapInfoWadLevel> { a, b };
        VmEMapInfoLevelBootstrap.SetLevelNum(levels, b, 5);
        Assert.Equal(0, a.LevelNum);
        Assert.Equal(5, b.LevelNum);
    }

    [Fact]
    public void GetOrCreateLevel_AssignsDefaultMapNum()
    {
        var levels = new List<VmEMapInfoWadLevel>();
        var level = VmEMapInfoLevelBootstrap.GetOrCreateLevel(
            levels,
            "map01",
            new VmEMapInfoWadLevel(),
            (_, name) => levels.FirstOrDefault(l => l.MapName == name));
        Assert.Equal("MAP01", level.MapName);
        Assert.Equal(1, level.LevelNum);
    }
}
