namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapInfoLoopTests
{
    [Fact]
    public void ResolveParseAction_UmapinfoBranch()
    {
        Assert.Equal(
            VmMapInfoParseMapInfoLoop.LumpParseAction.ParseUmapinfoOnly,
            VmMapInfoParseMapInfoLoop.ResolveParseAction(VmMapInfoParseMapInfoLoop.LumpIndexUmapinfo));
    }

    [Fact]
    public void ResolveParseAction_EmapinfoCommitsFirst()
    {
        Assert.Equal(
            VmMapInfoParseMapInfoLoop.LumpParseAction.CommitThenParseEmapinfo,
            VmMapInfoParseMapInfoLoop.ResolveParseAction(VmMapInfoParseMapInfoLoop.LumpIndexEmapinfo));
    }

    [Fact]
    public void ShouldCollectSecretMap_WhenDistinct()
    {
        Assert.True(VmMapInfoParseMapInfoLoop.ShouldCollectSecretMap("MAP02", "MAP01"));
    }

    [Fact]
    public void ValidateEpisodesAndSkills_ThrowsWhenEmpty()
    {
        Assert.Throws<InvalidOperationException>(() =>
            VmMapInfoParseMapInfoLoop.ValidateEpisodesAndSkills(0, 1));
    }

    [Fact]
    public void ApplySecretLevelFlag_SetsBit()
    {
        Assert.Equal(1u, VmMapInfoParseMapInfoLoop.ApplySecretLevelFlag(0));
    }
}
