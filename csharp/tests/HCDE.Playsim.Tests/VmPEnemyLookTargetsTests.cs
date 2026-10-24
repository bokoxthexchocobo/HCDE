namespace HCDE.Playsim.Tests;

public class VmPEnemyLookTargetsTests
{
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
    public void LookMonsters_SkipsOutOfRange()
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
                    Id = "far",
                    Flags3 = VmPEnemyLookTargets.Mf3Ismonster,
                    Health = 50,
                    Species = 2,
                    Distance2d = VmPEnemyLookTargets.MonsLookRange + 1,
                },
            },
        };
        Assert.False(VmPEnemyLookTargets.LookForMonsters(actor));
    }

    [Fact]
    public void TidBlock_MatchesDirectTid()
    {
        var lookee = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            TidToHate = 5,
            IsVisible = _ => true,
        };
        var hit = VmPEnemyLookTargets.LookForTidInBlock(lookee, new[]
        {
            new VmPEnemyLookTargets.BlockLink
            {
                Id = "match",
                Flags = VmPEnemyLookTargets.MfShootable,
                Health = 50,
                Tid = 5,
            },
        });
        Assert.Equal("match", hit);
    }

    [Fact]
    public void LookTid_FallbackGoal()
    {
        var goal = new object();
        var actor = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            TidToHate = 1,
            Goal = goal,
            BlockmapSearch = (_, _) => null,
            ActorIterator = (_, _) => Array.Empty<object?>(),
        };
        Assert.True(VmPEnemyLookTargets.LookForTid(actor, false));
        Assert.Equal(goal, actor.Target);
    }

    [Fact]
    public void LookTid_BudgetExhausted()
    {
        var actor = new VmPEnemyLookTargets.LookActorState
        {
            ActorId = "self",
            TidToHate = 1,
            PrLook3 = () => 0,
            BlockmapSearch = (_, _) => null,
            ActorIterator = (_, _) => Enumerable.Range(0, 20).Select(_ => (object?)new VmPEnemyLookTargets.BlockLink
            {
                Id = "x",
                Flags = VmPEnemyLookTargets.MfShootable,
                Health = 1,
            }),
        };
        Assert.False(VmPEnemyLookTargets.LookForTid(actor, false));
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
