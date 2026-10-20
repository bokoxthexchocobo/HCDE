namespace HCDE.Gamedata.Tests;

public class VmDoomEdSpawnInitTests
{
    [Fact]
    public void SortClassMapForDump_OrdersByKey()
    {
        var sorted = VmDoomEdSpawnInit.SortClassMapForDump(new Dictionary<int, string> { [3] = "C", [1] = "A" });
        Assert.Equal(1, sorted[0].Key);
    }

    [Fact]
    public void InitSpawnablesFromMapinfo_ResolvesClasses()
    {
        var pending = new Dictionary<int, VmMapinfoSpawnItem> { [1] = new() { ClassName = "DoomImp" } };
        var spawnables = new Dictionary<int, string>();
        VmDoomEdSpawnInit.InitSpawnablesFromMapinfo(pending, new Dictionary<int, VmMapinfoSpawnItem>(), spawnables, new Dictionary<int, string>(), _ => true, []);
        Assert.Equal("DoomImp", spawnables[1]);
    }

    [Fact]
    public void ClearStrifeTypes_EmptiesMap()
    {
        var map = new Dictionary<int, string> { [1] = "A" };
        VmDoomEdSpawnInit.ClearStrifeTypes(map);
        Assert.Empty(map);
    }
}
