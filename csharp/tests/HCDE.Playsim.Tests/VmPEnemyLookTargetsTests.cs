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
    public void LookEnemies_BlockSearchHit() =>
        Assert.True(VmPEnemyLookTargets.EvaluateLookEnemies(
            new VmPEnemyLookTargets.LookEnemiesContext(true, false, false, true, false, false, 0, false),
            out var found) && found);
}
