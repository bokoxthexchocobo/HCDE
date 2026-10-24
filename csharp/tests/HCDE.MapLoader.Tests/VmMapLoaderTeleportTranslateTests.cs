namespace HCDE.MapLoader.Tests;

public class VmMapLoaderTeleportTranslateTests
{
    [Fact]
    public void ApplyTeleportLineArgs_SetsTidWhenTagZero()
    {
        var args = new[] { 0, 0, 0, 0, 0 };
        VmMapLoaderTeleportTranslate.ApplyTeleportLineArgs(
            VmMapLoaderTeleportTranslate.TeleportSpecial, args);
        Assert.Equal(1, args[0]);
    }

    [Fact]
    public void ShouldAssignTid_WhenSectorUntagged()
    {
        Assert.True(VmMapLoaderTeleportTranslate.ShouldAssignTid(false));
    }
}
