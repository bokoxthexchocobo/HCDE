namespace HCDE.Playsim.Tests;

public class VmPEnemyValidLookTests
{
    [Fact]
    public void ShootableAlive_RejectsDormant() =>
        Assert.False(VmPEnemyValidLook.ShootableAlive(4, 1, 1));

    [Fact]
    public void MbfSkipMutualFight_WhenHealthyFriend() =>
        Assert.True(VmPEnemyValidLook.MbfSkipMutualFight(true, true, 150, true, 50, 50));

    [Fact]
    public void ValidEnemyPassesAll_RequiresVisible() =>
        Assert.False(VmPEnemyValidLook.ValidEnemyPassesAll(true, true, true, false, false));
}
