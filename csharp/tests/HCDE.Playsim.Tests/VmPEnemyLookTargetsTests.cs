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

    [Fact]
    public void LookForMonsters_WholeFunctionFindsTarget()
    {
        var actor = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            Species = 1,
            PlayerCanSeeActor = () => true,
            PrLookForMonsters = () => 100,
            CheckSight = (_, _) => true,
            EnumerateMonsters = () => new[]
            {
                new VmPEnemyLookTargets.MonsterCandidate
                {
                    Id = "m1",
                    Flags3 = VmPEnemyLookTargets.Mf3Ismonster,
                    Health = 50,
                    Species = 2,
                    Distance2d = 100,
                },
            },
        };
        Assert.True(VmPEnemyLookTargets.LookForMonsters(actor));
        Assert.Equal("m1", actor.Target);
    }

    [Fact]
    public void LookForEnemiesInBlock_FindsValidLink()
    {
        var lookee = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            ValidEnemyInBlock = (_, link) => link is VmPEnemyLookTargets.BlockLink bl && bl.Id == "enemy",
        };
        var hit = VmPEnemyLookTargets.LookForEnemiesInBlock(lookee, new[]
        {
            new VmPEnemyLookTargets.BlockLink { Id = "skip" },
            new VmPEnemyLookTargets.BlockLink { Id = "enemy" },
        });
        Assert.Equal("enemy", hit);
    }

    [Fact]
    public void LookForEnemies_UsesBlockSearchHit()
    {
        var actor = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            BlockmapSearch = (_, _) => "enemy",
        };
        Assert.True(VmPEnemyLookTargets.LookForEnemies(actor));
        Assert.Equal("enemy", actor.Target);
    }
}
