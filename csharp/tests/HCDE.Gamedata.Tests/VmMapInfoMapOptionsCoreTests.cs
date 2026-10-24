namespace HCDE.Gamedata.Tests;

public class VmMapInfoMapOptionsCoreTests
{
    [Fact]
    public void ApplyLevelNum_SetsField()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsCore.ApplyLevelNum(draft, 5);
        Assert.Equal(5, draft.LevelNum);
    }

    [Fact]
    public void ApplySecretNext_Trims()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsCore.ApplySecretNext(draft, " MAP02 ");
        Assert.Equal("MAP02", draft.NextSecretMap);
    }

    [Fact]
    public void ApplyCluster_SetsHubForHexen()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsCore.ApplyCluster(draft, 3, hexenHack: true);
        Assert.True(draft.ClusterIsHub);
    }

    [Fact]
    public void ApplySky1_ScalesHexenSpeed()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsCore.ApplySky1(draft, "SKY2", 256, hexenHack: true);
        Assert.Equal(1, draft.SkySpeed1, 3);
    }

    [Fact]
    public void ApplySky2_AssignsPic()
    {
        var draft = new VmMapInfoLevelOptionsDraft();
        VmMapInfoMapOptionsCore.ApplySky2(draft, "SKY3", 128, hexenHack: false);
        Assert.Equal("SKY3", draft.SkyPic2);
    }
}
