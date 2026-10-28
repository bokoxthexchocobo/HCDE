namespace HCDE.Playsim.Tests;

public class VmActionPlayerSkinCheckTests
{
    [Fact]
    public void PlayerSkinIsOtherGame_WhenPlayerAndOtherSkin() =>
        Assert.True(VmActionPlayerSkinCheck.PlayerSkinIsOtherGame(true, true));

    [Fact]
    public void PlayerSkinIsOtherGame_FalseWithoutPlayer() =>
        Assert.False(VmActionPlayerSkinCheck.PlayerSkinIsOtherGame(false, true));
}
