namespace HCDE.Playsim.Tests;

public class VmActionCheckTargetInLosTests
{
    private static VmActionCheckTargetInLos.TargetInLosContext MonsterWithTarget(int health = 50) =>
        new(false, false, false, false, true, false, false, false, true, false, health);

    [Fact]
    public void Plan_NonPlayer_NoTarget_Fails()
    {
        var ctx = MonsterWithTarget() with { HasMonsterTarget = false };
        Assert.Equal(
            VmActionCheckTargetInLos.TargetLosPlan.FailNoTarget,
            VmActionCheckTargetInLos.PlanCheckIfTargetInLos(ctx, 0, 90, 10, 0, 0, true, 0));
    }

    [Fact]
    public void Plan_CombatantOnly_RejectsNonMonster()
    {
        var ctx = MonsterWithTarget() with { TargetIsMonster = false, TargetIsPlayer = false };
        Assert.Equal(
            VmActionCheckTargetInLos.TargetLosPlan.FailNotCombatant,
            VmActionCheckTargetInLos.PlanCheckIfTargetInLos(ctx, VmActionCheckTargetInLos.JlosfCombatantonly, 90, 10, 0, 0, true, 0));
    }

    [Fact]
    public void Plan_AllyNoJump_BlocksFriends()
    {
        var ctx = MonsterWithTarget() with { IsFriend = true };
        Assert.Equal(
            VmActionCheckTargetInLos.TargetLosPlan.FailAlly,
            VmActionCheckTargetInLos.PlanCheckIfTargetInLos(ctx, VmActionCheckTargetInLos.JlosfAllynojump, 90, 10, 0, 0, true, 0));
    }

    [Fact]
    public void Plan_CloseDistance_NoJump_Fails()
    {
        var ctx = MonsterWithTarget();
        Assert.Equal(
            VmActionCheckTargetInLos.TargetLosPlan.FailCloseNoJump,
            VmActionCheckTargetInLos.PlanCheckIfTargetInLos(
                ctx, VmActionCheckTargetInLos.JlosfClosenojump, 90, 5, 0, 10, true, 0));
    }

    [Fact]
    public void Plan_InTargetLos_FovOutside_Fails()
    {
        var ctx = MonsterWithTarget();
        Assert.Equal(
            VmActionCheckTargetInLos.TargetLosPlan.FailFov,
            VmActionCheckTargetInLos.PlanCheckIfInTargetLos(ctx, 0, 45, 10, 0, 0, true, 30));
    }

    [Fact]
    public void ResolveNonPlayer_ProjectileNonSeeker_NoTarget()
    {
        var ctx = MonsterWithTarget() with { IsMissile = true, HasMonsterTarget = true };
        Assert.False(VmActionCheckTargetInLos.ResolveNonPlayerHasTarget(ctx, VmActionCheckTargetInLos.JlosfProjectile));
    }
}
