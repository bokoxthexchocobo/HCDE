using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgReplacementSprayTests
{
    [Fact]
    public void ReplacementFoilInvulBypassesMonsterInvulnerability()
    {
        var (sim, owner, target, missile) = Setup();
        target.Invulnerable = true;
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            spawnSpray: _ => new Actor { DamageType = "BFGSplash", FoilInvul = true });
        Assert.Equal(75, target.Health);
    }

    [Fact]
    public void ReplacementDamageTypeControlsTypedOnlyTarget()
    {
        var (sim, owner, target, missile) = Setup();
        target.Health = 1; target.DeathState = -1;
        target.SetTypedDeath("Fire", 3);
        Actor? observed = null;
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            spawnSpray: hit => { observed = hit; return new Actor { DamageType = "Fire" }; });
        Assert.Same(target, observed);
        Assert.True(target.IsDead);
        Assert.Equal(3, target.States.Current);
    }

    [Fact]
    public void SpeciesFilteredSprayIsDestroyedBeforeRollingDamage()
    {
        var (sim, owner, target, missile) = Setup();
        var spray = new Actor { MThruSpecies = true };
        var random = sim.CombatRandomState;
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, spawnSpray: _ => spray);
        Assert.True(spray.Destroyed);
        Assert.Equal(100, target.Health);
        Assert.Equal(random, sim.CombatRandomState);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, -24)]
    public void ReplacementFoilBuddhaControlsMonsterSurvival(bool foil, int health)
    {
        var (sim, owner, target, missile) = Setup();
        target.Health = 1; target.Buddha = true;
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25,
            spawnSpray: _ => new Actor { DamageType = "BFGSplash", FoilBuddha = foil });
        Assert.Equal(health, target.Health);
    }

    [Fact]
    public void FailedSpraySpawnUsesNativeFallbackDamageType()
    {
        var (sim, owner, target, missile) = Setup();
        target.Health = 1; target.DeathState = -1;
        target.SetTypedDeath("BFGSplash", 3);
        BfgSprayActions.Apply(sim, missile, owner, numRays: 1, fixedDamage: 25, spawnSpray: _ => null);
        Assert.True(target.IsDead);
    }

    private static (AuthoritySimulation, Actor, Actor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }] });
        var owner = sim.AddBot(0, 0, 3001);
        var target = sim.AddBot(200, 0, 3001);
        owner.Brain = target.Brain = null;
        target.Health = 100; target.PainChance = 0; target.DoHarmSpecies = true;
        return (sim, owner, target, new Actor { Angle = BamAngle.FromDegrees(45) });
    }
}
