namespace HCDE.Playsim.Tests;

public class VmPEnemyLookActionTests
{
    [Fact]
    public void LookForEnemiesEx_CollectsValidEnemies()
    {
        var self = new VmPEnemyLookAction.LookActor();
        var enemy = new VmPEnemyLookAction.LookActor();
        var targets = new List<VmPEnemyLookAction.LookActor>();
        var count = VmPEnemyLookAction.LookForEnemiesEx(
            new VmPEnemyLookAction.LookForEnemiesExContext
            {
                BlockThingsIterator = _ => new[] { enemy },
                ValidEnemyInBlock = (_, _) => true,
            },
            self,
            targets,
            512,
            true,
            false);
        Assert.Equal(1, count);
        Assert.Single(targets);
    }

    [Fact]
    public void Look_SkipsConversation()
    {
        var self = new VmPEnemyLookAction.LookActor { Flags5 = VmPEnemyLookAction.Mf5Inconversation };
        var called = false;
        VmPEnemyLookAction.Look(self, new VmPEnemyLookAction.LookContext
        {
            LookForPlayers = (_, _, _) => { called = true; return true; },
        });
        Assert.False(called);
    }

    [Fact]
    public void Look_WakesWhenLookForPlayersSucceeds()
    {
        var self = new VmPEnemyLookAction.LookActor { SeeState = new object(), SeeSound = "see" };
        string? played = null;
        VmPEnemyLookAction.Look(self, new VmPEnemyLookAction.LookContext
        {
            LookForPlayers = (actor, _, _) =>
            {
                actor.Target = new VmPEnemyLookAction.LookActor();
                return true;
            },
            PlaySound = (_, _, s, _) => played = s,
            SetState = (_, _) => { },
        });
        Assert.Equal("see", played);
    }

    [Fact]
    public void Wander_DecrementsReactionTime()
    {
        var self = new VmPEnemyLookAction.LookActor { Reactiontime = 3 };
        VmPEnemyLookAction.Wander(self, 0, new VmPEnemyLookAction.WanderContext());
        Assert.Equal(2, self.Reactiontime);
    }
}
