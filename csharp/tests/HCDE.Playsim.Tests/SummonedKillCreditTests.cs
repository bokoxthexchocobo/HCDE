using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SummonedKillCreditTests
{
    [Fact]
    public void LethalHitCreditsSummonerToDeathTargetAndSpecialButKeepsDamageHistory()
    {
        var (sim, minion, victim) = Room(); var player = sim.Players.Single();
        player.VelocityX = Fixed.FromInt(8); minion.VelocityX = Fixed.FromInt(6);
        ActorDamage.Apply(victim, 1000, minion);
        Assert.True(victim.IsDead); Assert.Equal(player.Id, victim.TargetId);
        Assert.Equal(minion.Id, victim.LastDamageSourceId);
        Assert.Equal(0, player.VelocityX.Raw); Assert.Equal(6, minion.VelocityX.ToDouble());
    }

    [Fact]
    public void NonlethalHitKeepsMinionAsDamageSource()
    {
        var (_, minion, victim) = Room();
        ActorDamage.Apply(victim, 1, minion, DamageFlags.NoPain);
        Assert.False(victim.IsDead); Assert.Equal(minion.Id, victim.LastDamageSourceId); Assert.Equal(19, victim.Special);
    }

    [Fact]
    public void BuddhaProtectionDoesNotCreditOrExecuteDeathSpecial()
    {
        var (sim, minion, victim) = Room(); victim.Buddha = true;
        sim.Players.Single().VelocityX = Fixed.FromInt(8);
        ActorDamage.Apply(victim, 1000, minion);
        Assert.Equal(1, victim.Health); Assert.Equal(19, victim.Special); Assert.Equal(minion.Id, victim.LastDamageSourceId);
        Assert.Equal(8, sim.Players.Single().VelocityX.ToDouble());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InvalidOrAbsentSummoningRelationshipKeepsOriginalSource(int kind)
    {
        var (sim, minion, victim) = Room();
        if (kind == 0) minion.SummonedMonster = false;
        if (kind == 1) minion.TracerId = null;
        if (kind == 2) minion.TracerId = sim.AddBot(100, 100).Id;
        if (kind == 3) sim.Players.Single().Destroy();
        minion.VelocityX = Fixed.FromInt(6);
        ActorDamage.Apply(victim, 1000, minion);
        Assert.Equal(minion.Id, victim.TargetId); Assert.Equal(0, minion.VelocityX.Raw);
    }

    [Fact]
    public void DeadRegisteredPlayerPawnStillReceivesCreditAsNativePointerRuleAllows()
    {
        var (sim, minion, victim) = Room(); var player = sim.Players.Single(); player.Health = 0;
        ActorDamage.Apply(victim, 1000, minion); Assert.Equal(player.Id, victim.TargetId);
    }

    [Fact]
    public void MissileInflictorDeathTypeSurvivesSourceCreditTransfer()
    {
        var (sim, minion, victim) = Room();
        var missile = sim.SpawnProjectile(minion, ProjectileKind.ImpBall); missile.DeathType = "Ice";
        victim.SetTypedDeath("Ice", victim.DeathState);
        ActorDamage.Apply(victim, 1000, minion, inflictor: missile);
        Assert.Equal(sim.Players.Single().Id, victim.TargetId); Assert.Equal("Ice", victim.DamageType);
        Assert.Equal(minion.Id, victim.LastDamageSourceId);
    }

    [Fact]
    public void PoisonDamageDoesNotUseOrdinaryDamageSummonerRemap()
    {
        var (sim, minion, _) = Room(); var victim = sim.Players.Single();
        ActorDamage.ApplyPoisonDamage(victim, 1000, minion);
        Assert.True(victim.IsDead); Assert.Equal(minion.Id, victim.TargetId);
    }

    [Fact]
    public void RestoredSummoningRelationshipKeepsDeathCreditDeterministic()
    {
        var (sim, minion, victim) = Room(); var bytes = SimSavegame.Write(sim, true);
        ActorDamage.Apply(victim, 1000, minion); var expected = (victim.TargetId, victim.LastDamageSourceId, victim.Health);
        SimSavegame.Apply(sim, bytes); ActorDamage.Apply(victim, 1000, minion);
        Assert.Equal(expected, (victim.TargetId, victim.LastDamageSourceId, victim.Health));
    }

    private static (AuthoritySimulation Sim, Actor Minion, Actor Victim) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1, X = 400 }, new LevelThing { Type = 3004, X = 200, Special = 19 }] });
        var minion = sim.AddBot(0, 0); minion.SummonedMonster = true; minion.TracerId = sim.Players.Single().Id;
        var victim = sim.Actors.First(a => a.Special == 19); victim.DoHarmSpecies = true;
        return (sim, minion, victim);
    }
}
