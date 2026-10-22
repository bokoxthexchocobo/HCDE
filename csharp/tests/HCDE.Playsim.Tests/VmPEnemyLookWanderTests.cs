namespace HCDE.Playsim.Tests;

public class VmPEnemyLookWanderTests
{
    [Fact]
    public void LookForEnemiesEx_UsesFriendlyseeblocksWhenRangeSentinel()
    {
        var self = new VmPEnemyLookWander.Monster { Friendlyseeblocks = 10 };
        var list = new List<VmPEnemyLookWander.Monster>();
        var seenRange = 0.0;
        VmPEnemyLookWander.LookForEnemiesEx(
            new VmPEnemyLookWander.EnemiesExContext
            {
                ThingsInRange = (_, r) =>
                {
                    seenRange = r;
                    return new[] { new VmPEnemyLookWander.Monster() };
                },
                ValidEnemy = (_, _, _) => true,
            },
            self,
            list,
            -1,
            true,
            false,
            null);
        Assert.Equal(10 * VmPEnemyLookWander.Mapblockunits, seenRange);
        Assert.Single(list);
    }

    [Fact]
    public void Look_SkipsWhenInConversation()
    {
        var self = new VmPEnemyLookWander.Monster { Flags5 = VmPEnemyLookWander.Mf5Inconversation };
        var playersCalled = false;
        VmPEnemyLookWander.Look(
            self,
            new VmPEnemyLookWander.LookContext
            {
                LookForPlayers = (_, _, _) =>
                {
                    playersCalled = true;
                    return false;
                },
            });
        Assert.False(playersCalled);
    }

    [Fact]
    public void Look_AmbushEnemyRequiresSight()
    {
        var enemy = new VmPEnemyLookWander.Monster { Flags = VmPEnemyLookWander.MfShootable };
        var self = new VmPEnemyLookWander.Monster
        {
            Flags = VmPEnemyLookWander.MfAmbush,
            LastHeard = enemy,
        };
        var sawState = false;
        var scannedPlayers = false;
        VmPEnemyLookWander.Look(
            self,
            new VmPEnemyLookWander.LookContext
            {
                IsFriend = (_, _) => false,
                CheckSight = (_, _) => false,
                LookForPlayers = (_, _, _) =>
                {
                    scannedPlayers = true;
                    return false;
                },
                SetState = (_, _) => sawState = true,
            });
        Assert.False(sawState);
        Assert.True(scannedPlayers);
    }

    [Fact]
    public void Wander_DecrementsReactionTime()
    {
        var self = new VmPEnemyLookWander.Monster { Reactiontime = 3 };
        VmPEnemyLookWander.Wander(self, 0, new VmPEnemyLookWander.WanderContext());
        Assert.Equal(2, self.Reactiontime);
    }

    [Fact]
    public void Wander_CallsNewChaseDirWhenMovecountExpires()
    {
        var self = new VmPEnemyLookWander.Monster { Movecount = 0 };
        var called = false;
        VmPEnemyLookWander.Wander(
            self,
            0,
            new VmPEnemyLookWander.WanderContext
            {
                SmartMove = _ => true,
                NewChaseDir = _ => called = true,
            });
        Assert.True(called);
        Assert.Equal(4, self.Movecount);
    }
}
