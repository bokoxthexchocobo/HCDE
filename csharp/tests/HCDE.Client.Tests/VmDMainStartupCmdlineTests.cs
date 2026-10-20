namespace HCDE.Client.Tests;

public class VmDMainStartupCmdlineTests
{
    [Fact]
    public void ShouldLoadLightsPk3_WhenStartupRequests()
    {
        Assert.True(VmDMainStartupCmdline.ShouldLoadLightsPk3(1, false, false, false));
    }

    [Fact]
    public void TryParseExplicitGametype_PlusSetForm()
    {
        Assert.True(VmDMainStartupCmdline.TryParseExplicitGametype(
            new[] { "game", "+set", "sv_gametype", "4" }, out var gt));
        Assert.Equal(4, gt);
    }

    [Fact]
    public void ShouldResetSoloGametype_WhenLocalSolo()
    {
        Assert.True(VmDMainStartupCmdline.ShouldResetSoloGametype(false, false));
    }

    [Fact]
    public void ApplyCoopMode_SetsDf3LocalItems_WhenNotHexen()
    {
        var (_, f3) = VmDMainStartupCmdlineFlags.ApplyCoopMode(0, 0, false);
        Assert.NotEqual(0, f3 & VmDMainStartupCmdlineFlags.Df3LocalItems);
    }

    [Fact]
    public void ApplyMonsterFlags_SetsNoMonsters()
    {
        var f = VmDMainStartupCmdlineFlags.ApplyMonsterFlags(0, true, false, false);
        Assert.NotEqual(0, f & VmDMainStartupCmdlineFlags.DfNoMonsters);
    }
}
