namespace HCDE.Playsim.Tests;

public class VmPEnemyLookTargetsTests
{
    [Fact]
    public void LookMonsters_SkipsOutOfRange() =>
        Assert.False(VmPEnemyLookTargets.TryPickLookTarget(
            new VmPEnemyLookTargets.LookMonsterContext(true, VmPEnemyLookTargets.MonsLookRange + 1, 100, 0, 1, 2, true),
            out _));

    [Fact]
    public void LookMonsters_FindsInRange() =>
        Assert.True(VmPEnemyLookTargets.TryPickLookTarget(
            new VmPEnemyLookTargets.LookMonsterContext(true, 100, 100, 0, 1, 2, true),
            out var found) && found);

    [Fact]
    public void TidBlock_MatchesDirectTid() =>
        Assert.True(VmPEnemyLookTargets.TryPickTidInBlock(
            new VmPEnemyLookTargets.TidBlockCandidate(
                VmPEnemyLookTargets.MfShootable, 0, 50, false, 5, 5, false, 0, 0, 0, 0, true, false),
            out var found) && found);

    [Fact]
    public void LookTid_FallbackGoal() =>
        Assert.True(VmPEnemyLookTargets.FallbackToGoal(true, true, true));

    [Fact]
    public void LookTid_StaleLastActor() =>
        Assert.True(VmPEnemyLookTargets.LastLookTidStale(true, 3, 5));

    [Fact]
    public void LookTid_BudgetExhausted() =>
        Assert.Equal(7, VmPEnemyLookTargets.LookIteratorBudget(0));

    [Fact]
    public void LookEnemies_FallbackLastEnemy() =>
        Assert.True(VmPEnemyLookTargets.FallbackToLastEnemy(true, true, 10, false));
}
