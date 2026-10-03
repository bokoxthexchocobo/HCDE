using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;
public class PainMassacreTests
{
    [Theory]
    [InlineData("Massacre", 0)]
    [InlineData("massacre", 0)]
    [InlineData("Fire", 3)]
    [InlineData(null, 3)]
    public void DelayedDeathBurstSuppressesOnlyMassacre(string? damageType, int expected)
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors);
        ActorDamage.Apply(parent, 1000, damageType: damageType);
        for (var tic = 0; tic < 32; tic++) sim.Tick();
        Assert.Equal(expected, sim.Actors.Count(a => a.DoomEdNum == 3006));
        Assert.Equal(expected == 0 ? "Massacre" : null, parent.DeathDamageType);
    }
    [Fact]
    public void NonlethalMassacreDoesNotSuppressLaterOrdinaryDeath()
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors);
        ActorDamage.Apply(parent, 1, damageType: "Massacre"); Assert.Null(parent.DeathDamageType);
        ActorDamage.Apply(parent, 1000);
        for (var tic = 0; tic < 32; tic++) sim.Tick();
        Assert.Equal(3, sim.Actors.Count(a => a.DoomEdNum == 3006));
    }
    [Fact]
    public void RevivalClearsMassacreAndAllowsSpawningAgain()
    {
        var sim = Room(); var parent = Assert.Single(sim.Actors);
        ActorDamage.Apply(parent, 1000, damageType: "Massacre");
        Assert.Null(sim.SpawnLostSoul(parent, null, 0));
        parent.Health = parent.ResurrectionHealth; Assert.Null(parent.DeathDamageType);
        Assert.NotNull(sim.SpawnLostSoul(parent, null, 0));
    }
    [Fact]
    public void MassacredWaveParentCompletesWithoutRegisteringChildren()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 512 }] });
        sim.Invasion.Enabled = true; sim.Invasion.Configure(1, 0, 35); sim.Tick();
        var parent = Assert.Single(sim.Actors); parent.Brain = MonsterBrain.ForType(71);
        ActorDamage.Apply(parent, 1000, damageType: "Massacre");
        for (var tic = 0; tic < 32; tic++) sim.Tick();
        Assert.Equal(InvasionPhase.Victory, sim.Invasion.Phase); Assert.Equal(1, sim.Invasion.Spawned);
        Assert.Equal(0, sim.Invasion.ActiveMonsters); Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == 3006);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 71 }] });
}