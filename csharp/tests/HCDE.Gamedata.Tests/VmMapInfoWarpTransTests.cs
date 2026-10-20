namespace HCDE.Gamedata.Tests;

public class VmMapInfoWarpTransTests
{
    [Fact]
    public void TryResolveWarpTransMap_UsesLevelRecord()
    {
        var levels = new List<VmMapInfoLevelRecord> { new() { MapName = "MAP05", WarpTrans = 7 } };
        Assert.True(VmMapInfoWarpTrans.TryResolveWarpTransMap("&wt@07", false, levels, out var map));
        Assert.Equal("MAP05", map);
    }

    [Fact]
    public void SecretLevelVisited_DetectsVisitedSecret()
    {
        var levels = new List<VmMapInfoVisitState> { new() { IsSecret = true, Visited = true } };
        Assert.True(VmMapInfoWarpTrans.SecretLevelVisited(levels));
    }
}
