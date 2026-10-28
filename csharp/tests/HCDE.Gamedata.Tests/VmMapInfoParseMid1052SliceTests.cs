namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMid1052SliceTests
{
    [Fact]
    public void CheckWarpTransMapTailReturnsFalse_MatchesNativeFallthrough()
    {
        Assert.False(VmMapInfoParseMid1052Slice.CheckWarpTransMapTailReturnsFalse());
    }

    [Fact]
    public void SecretLevelVisited_DetectsVisitedSecret()
    {
        var levels = new[] { new VmMapInfoVisitState { IsSecret = true, Visited = true } };
        Assert.True(VmMapInfoParseMid1052Slice.SecretLevelVisited(levels));
    }

    [Fact]
    public void ResolveClusterInfo_UsesDefaultWhenMissing()
    {
        var defaults = new VmMapInfoClusterRecord { Cluster = -1 };
        var hit = VmMapInfoParseMid1052Slice.ResolveClusterInfo([], 5, defaults);
        Assert.Equal(-1, hit.Cluster);
    }

    [Fact]
    public void ResolveClusterInfo_FindsWadCluster()
    {
        var clusters = new[] { new VmMapInfoClusterRecord { Cluster = 3 } };
        Assert.Equal(3, VmMapInfoParseMid1052Slice.ResolveClusterInfo(clusters, 3, new VmMapInfoClusterRecord()).Cluster);
    }

    [Fact]
    public void ShouldCleanAttachedLevelSnapshot_RequiresBoth()
    {
        Assert.True(VmMapInfoParseMid1052Slice.ShouldCleanAttachedLevelSnapshot(true, true));
        Assert.False(VmMapInfoParseMid1052Slice.ShouldCleanAttachedLevelSnapshot(true, false));
    }

    [Fact]
    public void ExtraRuntimeSnapshotClears_CountsPrimaryAndVm()
    {
        Assert.Equal(2, VmMapInfoParseMid1052Slice.ExtraRuntimeSnapshotClears(true, true));
        Assert.Equal(0, VmMapInfoParseMid1052Slice.ExtraRuntimeSnapshotClears(false, false));
    }

    [Fact]
    public void ClearDeferedForEachLevel_InvokesPerIndex()
    {
        var seen = new List<int>();
        VmMapInfoParseMid1052Slice.ClearDeferedForEachLevel(3, seen.Add);
        Assert.Equal(new[] { 0, 1, 2 }, seen);
    }

    [Fact]
    public void ApplyLevelInfoResetHead_SetsDoomLaxMonsterFlags2()
    {
        var level = new VmMapInfoLevelInfoResetDraft();
        VmMapInfoParseMid1052Slice.ApplyLevelInfoResetHead(level, hexenGameType: false);
        Assert.Equal(VmMapInfoParseMid1052Slice.Level2LaxMonsterActivation, level.Flags2);
        Assert.Equal(VmMapInfoLevelReset.NoFlatSky, level.SkyPic1);
        Assert.Equal(VmMapInfoParseMid1052Slice.NameNone, level.Slideshow);
    }

    [Fact]
    public void ApplyLevelInfoResetHead_ClearsHexenFlags2()
    {
        var level = new VmMapInfoLevelInfoResetDraft { Flags2 = 99 };
        VmMapInfoParseMid1052Slice.ApplyLevelInfoResetHead(level, hexenGameType: true);
        Assert.Equal(0u, level.Flags2);
        Assert.Equal(VmMapInfoParseMid1052Slice.OutsideFogDefault, level.OutsideFog);
    }
}
