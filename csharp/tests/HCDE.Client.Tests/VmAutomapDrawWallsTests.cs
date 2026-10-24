namespace HCDE.Client.Tests;

public class VmAutomapDrawWallsTests
{
    [Fact]
    public void CheckSecret_BoomStyle_ReturnsSecretSector()
    {
        var s = VmAutomapDrawWalls.CheckSecret(true, true, false, false, false, 1, 0, 0x20);
        Assert.Equal(VmAutomapDrawWalls.SecretLineStyle.SecretSector, s);
    }

    [Fact]
    public void ShouldDrawPortalPass_SkipsMapPortalGroupPass()
    {
        Assert.False(VmAutomapDrawWalls.ShouldDrawPortalPass(1, 0, 0, true));
        Assert.True(VmAutomapDrawWalls.ShouldDrawPortalPass(1, 1, 0, true));
    }

    [Fact]
    public void ClassifyVisibleWall_OneSided_WhenNoBack()
    {
        var style = VmAutomapDrawWalls.ClassifyVisibleWall(
            0, 0, 1, 0, false, false, 0, 10, false,
            VmAutomapDrawWalls.SecretLineStyle.None, 0, false,
            false, false, false, false, false, false, false, false,
            false, false, false, false);
        Assert.Equal(VmAutomapDrawWalls.WallDrawStyle.OneSided, style);
    }

    [Fact]
    public void ClassifyUnseenWall_Revealed_DrawsNotSeen()
    {
        var style = VmAutomapDrawWalls.ClassifyUnseenWall(false, 0x80, 0x80, 0, 0, false, false);
        Assert.Equal(VmAutomapDrawWalls.WallDrawStyle.NotSeen, style);
    }
}
