namespace HCDE.Playsim.Tests;

public class VmPEnemyBossDeathTests
{
    [Fact]
    public void CheckBossDeath_FailsWhenNoLivingPlayers()
    {
        var ctx = new VmPEnemyBossDeath.LevelBossContext
        {
            PlayerInGame = _ => false,
        };
        var self = new VmPEnemyBossDeath.BossActor
        {
            TypeName = "Fatso",
            ReplaceeTypeName = "Fatso",
            IsSelf = true,
        };
        Assert.False(VmPEnemyBossDeath.CheckBossDeath(self, ctx));
    }

    [Fact]
    public void CheckBossDeath_FailsWhenOtherBossAlive()
    {
        var ctx = new VmPEnemyBossDeath.LevelBossContext
        {
            PlayerInGame = i => i == 0,
            PlayerHealth = _ => 100,
            LivingBosses = new[]
            {
                new VmPEnemyBossDeath.BossActor
                {
                    TypeName = "Fatso",
                    ReplaceeTypeName = "Fatso",
                    Health = 100,
                    IsSelf = false,
                },
            },
        };
        var self = new VmPEnemyBossDeath.BossActor
        {
            TypeName = "Fatso",
            ReplaceeTypeName = "Fatso",
            IsSelf = true,
        };
        Assert.False(VmPEnemyBossDeath.CheckBossDeath(self, ctx));
    }

    [Fact]
    public void PlanBossDeath_RunsSpecialActionWhenTypeMatches()
    {
        var ctx = new VmPEnemyBossDeath.LevelBossContext
        {
            PlayerInGame = i => i == 0,
            PlayerHealth = _ => 100,
            LivingBosses = Array.Empty<VmPEnemyBossDeath.BossActor>(),
            SpecialActions = new[]
            {
                new VmPEnemyBossDeath.BossSpecialAction { Type = "Fatso", Action = 11, Args = new[] { 1 } },
            },
        };
        var self = new VmPEnemyBossDeath.BossActor
        {
            TypeName = "Fatso",
            ReplaceeTypeName = "Fatso",
            IsSelf = true,
        };
        var plan = VmPEnemyBossDeath.PlanBossDeath(self, ctx);
        Assert.True(plan.RunSpecialActions);
        Assert.Single(plan.SpecialActions);
    }

    [Fact]
    public void PlanBossDeath_ExitWhenAllDeadAndMap07()
    {
        var ctx = new VmPEnemyBossDeath.LevelBossContext
        {
            LevelFlags = VmPEnemyBossDeath.LevelMap07Special | VmPEnemyBossDeath.LevelSpeckillmonsters,
            PlayerInGame = i => i == 0,
            PlayerHealth = _ => 100,
            LivingBosses = Array.Empty<VmPEnemyBossDeath.BossActor>(),
        };
        var self = new VmPEnemyBossDeath.BossActor
        {
            TypeName = "Fatso",
            ReplaceeTypeName = "Fatso",
            Flags8 = VmPEnemyBossDeath.Mf8Map07Boss1,
            IsSelf = true,
        };
        var plan = VmPEnemyBossDeath.PlanBossDeath(self, ctx);
        Assert.True(plan.RunVictory);
        Assert.True(plan.Speckill);
        Assert.True(plan.Map07Lower666);
        Assert.True(plan.ExitLevel);
    }
}
