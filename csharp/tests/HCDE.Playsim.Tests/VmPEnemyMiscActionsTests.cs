namespace HCDE.Playsim.Tests;

public class VmPEnemyMiscActionsTests
{
    [Fact]
    public void ClearLastHeard_ClearsPointer()
    {
        var a = new VmPEnemyMiscActions.Actor { LastHeard = new object() };
        VmPEnemyMiscActions.ClearLastHeard(a);
        Assert.Null(a.LastHeard);
    }

    [Fact]
    public void ClassBossHealth_CoopScalesOnce()
    {
        var a = new VmPEnemyMiscActions.Actor { Health = 10 };
        VmPEnemyMiscActions.ClassBossHealth(a, true, false);
        Assert.Equal(50, a.Health);
        Assert.True(a.Special1);
    }

    [Fact]
    public void Look2_SkipsConversation()
    {
        var self = new VmPEnemyMiscActions.Actor { Flags5 = VmPEnemyMiscActions.Mf5Inconversation };
        VmPEnemyMiscActions.Look2(self, new VmPEnemyMiscActions.Look2Context());
        Assert.Equal(0, self.Threshold);
    }

    [Fact]
    public void Pain_PlayerUsesHealthBand()
    {
        var played = "";
        VmPEnemyMiscActions.Pain(
            new VmPEnemyMiscActions.Actor { IsPlayer = true, PlayerHealth = 10 },
            new VmPEnemyMiscActions.PainContext
            {
                FindSound = s => { played = s; return 1; },
                PlaySound = (_, _, _, _, _) => { },
            });
        Assert.Equal("*pain25", played);
    }

    [Fact]
    public void MonsterRail_RequiresTarget()
    {
        var called = false;
        VmPEnemyMiscActions.MonsterRail(
            new VmPEnemyMiscActions.Actor(),
            new VmPEnemyMiscActions.MonsterRailContext { RailAttack = (_, _) => called = true });
        Assert.False(called);
    }
}
