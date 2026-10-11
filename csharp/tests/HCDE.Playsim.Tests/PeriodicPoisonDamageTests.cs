using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PeriodicPoisonDamageTests
{
    private sealed class ObservingPlayer : PlayerPawn
    {
        public int CounterSeen { get; private set; }
        public override void CheckPoison() => CounterSeen = DamageCount;
    }

    [Fact]
    public void DamageFlashFadesBeforeVirtualPoisonCheck()
    {
        var player = new ObservingPlayer { Health = 100, NoPain = true };
        ActorDamage.Apply(player, 10); player.Tick();
        Assert.Equal(9, player.CounterSeen);
    }
    private static AuthoritySimulation Room(double factor = 1) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(SkillDamageFactor: factor));
    private static void SetTic(AuthoritySimulation sim, int tic)
    {
        sim.Thinkers.Clock.Restore(tic);
    }

    [Theory]
    [InlineData(15, 3, 100)]
    [InlineData(16, 0, 99)]
    [InlineData(32, 0, 99)]
    public void CheckPoisonUsesSixteenTicCadenceAndDealsLastDose(int tic, int count, int health)
    {
        var sim = Room(); var player = sim.Players.Single(); player.ApplyPoison(3);
        SetTic(sim, tic); player.CheckPoison();
        Assert.Equal(count, player.PoisonCount); Assert.Equal(health, player.Health);
    }

    [Fact]
    public void LivingTickRunsPoisonButProtectionStillAllowsCounterToDecay()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ApplyPoison(10); player.GodMode = true;
        SetTic(sim, 16); player.Tick();
        Assert.Equal(5, player.PoisonCount); Assert.Equal(100, player.Health);
        player.GodMode = false; SetTic(sim, 32); player.Tick();
        Assert.Equal(0, player.PoisonCount); Assert.Equal(99, player.Health);
    }

    [Fact]
    public void PoisonBypassesArmorAndDoesNotAddOrdinaryDamageFlash()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 100; player.ApplyPoison(5);
        SetTic(sim, 16); player.CheckPoison();
        Assert.Equal(99, player.Health); Assert.Equal(100, player.Inventory.Armor); Assert.Equal(0, player.DamageCount);
    }

    [Theory]
    [InlineData(0.5, 100)]
    [InlineData(2, 98)]
    public void PoisonScalesEvenSinglePointDamage(double factor, int health)
    {
        var sim = Room(factor); var player = sim.Players.Single(); player.ApplyPoison(5);
        SetTic(sim, 16); player.CheckPoison(); Assert.Equal(health, player.Health);
    }

    [Fact]
    public void PassiveAndTypedFactorsUsePoisonDamageType()
    {
        var sim = Room(4); var player = sim.Players.Single(); player.DamageFactor = Fixed.FromDouble(0.5);
        player.SetDamageFactor("Fire", 0.5); player.ApplyPoison(5, new Actor { DamageType = "Fire" });
        SetTic(sim, 16); player.CheckPoison(); Assert.Equal(99, player.Health);
    }

    [Theory]
    [InlineData(false, 16, 0)]
    [InlineData(true, 16, 0)]
    [InlineData(true, 64, 2)]
    public void PoisonPainUsesTypedStateOnlyAtSixtyFourTicBoundary(bool playPain, int tic, int state)
    {
        var sim = Room(); var player = sim.Players.Single(); player.SetTypedPain("Ice", 2);
        player.ApplyPoison(5, new Actor { DamageType = "Fire", PainType = "Ice" });
        SetTic(sim, tic); ActorDamage.ApplyPoisonDamage(player, 1, playPainSound: playPain);
        Assert.Equal(state, player.States.Current);
    }

    [Fact]
    public void RawTelefragBypassesOrdinaryProtectionButBuddhaTwoSurvives()
    {
        var player = new PlayerPawn { Health = 100, GodMode = true, Buddha = true };
        Assert.True(ActorDamage.ApplyPoisonDamage(player, ActorDamage.TelefragDamage).Killed);
        player = new PlayerPawn { Health = 100, Buddha2 = true };
        Assert.False(ActorDamage.ApplyPoisonDamage(player, ActorDamage.TelefragDamage).Killed); Assert.Equal(1, player.Health);
    }
}
