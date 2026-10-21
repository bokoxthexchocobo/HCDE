namespace HCDE.Playsim.Tests;

public class VmPEnemyLookPlayersTests
{
    [Fact]
    public void LastEnemyRetarget_WhenAliveNonFriend() =>
        Assert.True(VmPEnemyLookPlayers.LastEnemyRetarget(true, 1, false));

    [Fact]
    public void SinglePlayerDeadMonsters_Triggers() =>
        Assert.True(VmPEnemyLookPlayers.SinglePlayerDeadMonsters(true, true, false, 0, true, false));

    [Fact]
    public void GoalReactionGate_BlocksEarly() =>
        Assert.True(VmPEnemyLookPlayers.GoalReactionGate(true, 10, 20));

    [Fact]
    public void ALookTidBranch_WhenHateSet() =>
        Assert.True(VmPEnemyLookPlayers.TidHateUsesLookForTid(1));

    [Fact]
    public void ALookAmbushSight_WhenNoSight() =>
        Assert.True(VmPEnemyLookPlayers.AmbushNeedsSight(ambush: true, sight: false));
}
