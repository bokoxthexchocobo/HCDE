using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileHealthDefaultsTests
{
    [Theory]
    [InlineData(-3)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(1000)]
    public void PatchedHealthInitializesWithoutDeathTransitionAndSurvivesFreshSave(int health)
    {
        var text = $"Thing 35\nHit points = {health}\n";
        var (sim, missile) = Room(text); Assert.Equal(health, missile.Health); Assert.Equal(health, missile.SpawnHealth());
        Assert.False(missile.Corpse); Assert.False(missile.Killed); Assert.False(missile.Destroyed);
        var saved = SimSavegame.Write(sim, true); var (fresh, loaded) = Room(text);
        SimSavegame.Apply(fresh, saved); Assert.Equal(health, loaded.Health); Assert.Equal(health, loaded.SpawnHealth());
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        missile.RestoreHealth(33); SimSavegame.Apply(sim, saved); Assert.Equal(health, missile.Health);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    public void PatchedHealthControlsShootableMissileDamageAndClassGibThreshold(int health)
    {
        var (sim, missile) = Room($"Thing 35\nBits = SHOOTABLE+NOGRAVITY\nHit points = {health}\n");
        Assert.Equal(-health, missile.GetGibHealth());
        ActorDamage.Apply(missile, 3); Assert.Equal(health - 3, missile.Health); Assert.False(missile.IsDead);
        var saved = SimSavegame.Write(sim, true); ActorDamage.Apply(missile, health * 2); Assert.True(missile.IsDead);
        SimSavegame.Apply(sim, saved);
        var restored = sim.Actors.Single(a => a.Id == missile.Id);
        Assert.Equal(health - 3, restored.Health); Assert.Equal(health, restored.SpawnHealth());
    }

    [Theory]
    [InlineData(ProjectileKind.Rocket, 34)]
    [InlineData(ProjectileKind.ImpBall, 32)]
    [InlineData(ProjectileKind.RevenantTracer, 7)]
    public void OtherProjectileKindsUseMatchingThingDefaults(ProjectileKind kind, int thing)
    {
        var (sim, _) = Room($"Thing {thing}\nHit points = 77\n");
        var missile = sim.SpawnProjectile(sim.Players.Single(), kind); Assert.Equal(77, missile.Health); Assert.Equal(77, missile.SpawnHealth());
    }

    [Fact]
    public void InheritedHealthRemainsExplicitThroughUnrelatedPatch()
    {
        var original = DehackedPatch.Apply("Thing 35\nHit points = 80\n");
        var inherited = DehackedPatch.Apply("Thing 35\nMass = 200\n", original); Assert.Empty(inherited.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] }, dehacked: inherited);
        var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(80, missile.Health); Assert.Equal(200, missile.Mass); Assert.True(inherited.Actors.Single(a => a.Index == 35).HealthPatched);
    }

    [Fact]
    public void UnrelatedPatchPreservesStockHealthAndSpawnMetadata()
    {
        var (sim, missile) = Room("Thing 35\nMass = 200\n");
        var stock = new ProjectileActor(sim.Players.Single(), ProjectileKind.Plasma);
        Assert.Equal(stock.Health, missile.Health); Assert.Equal(stock.SpawnHealth(), missile.SpawnHealth());
        Assert.Equal(stock.GetGibHealth(), missile.GetGibHealth());
    }

    [Theory]
    [InlineData(-3, 1)]
    [InlineData(0, 0)]
    [InlineData(80, 80)]
    public void ExplicitMonsterClassificationUsesNativeNonzeroMinimum(int health, int expected)
    {
        var (_, missile) = Room($"Thing 35\nBits = COUNTKILL+NOGRAVITY\nHit points = {health}\n");
        Assert.True(missile.IsMonster); Assert.Equal(expected, missile.Health); Assert.Equal(expected, missile.SpawnHealth());
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile) Room(string text)
    {
        var patch = DehackedPatch.Apply(text); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }] }, dehacked: patch);
        return (sim, sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma));
    }
}
