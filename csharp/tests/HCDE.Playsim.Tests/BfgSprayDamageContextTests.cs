using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgSprayDamageContextTests
{
    [Theory]
    [InlineData("BFGSplash", true)]
    [InlineData("Fire", false)]
    public void SprayUsesNativeDefaultDamageType(string acceptedType, bool killed)
    {
        var (sim, owner, target) = Setup();
        target.Health = 1;
        target.DeathState = -1;
        target.SetTypedDeath(acceptedType, 3);
        Fire(sim, owner);
        Assert.Equal(killed, target.IsDead);
        if (killed) Assert.Equal(3, target.States.Current);
    }

    [Fact]
    public void DefaultSprayUsesOwnerAsInflictorForDeathOverride()
    {
        var (sim, owner, target) = Setup();
        target.Health = 1;
        target.DeathState = -1;
        target.SetTypedDeath("Fire", 3);
        owner.DeathType = "Fire";
        Fire(sim, owner);
        Assert.True(target.IsDead);
        Assert.Equal(3, target.States.Current);
    }

    private static void Fire(AuthoritySimulation sim, PlayerPawn owner)
    {
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Bfg);
        for (var tic = 0; tic < 20 && !missile.Destroyed; tic++) sim.Tick();
        Assert.True(missile.Destroyed);
    }

    private static (AuthoritySimulation, PlayerPawn, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3003 }, new LevelThing { Type = 1, X = -200 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Sides = [new LevelSide { Sector = 0 }],
        }, rngSeed: 42);
        var target = sim.Actors.Single(actor => actor.DoomEdNum == 3003);
        target.Brain = null;
        return (sim, sim.Players.Single(), target);
    }
}
