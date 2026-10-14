using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathTargetTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KillerReplacesPreviousTargetWithOrWithoutBrain(bool brain)
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); var old = sim.AddBot(400, 0);
        if (!brain) victim.Brain = null;
        victim.SetTargetPointer(old);
        ActorDamage.Apply(victim, 1000, sim.Players.Single());
        Assert.Equal(sim.Players.Single().Id, victim.TargetId);
        Assert.Same(sim.Players.Single(), AcsActorPointer.Resolve(sim, victim, AcsActorPointer.Target));
    }

    [Fact]
    public void DeathActionObservesSourceRatherThanInflictor()
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); victim.Brain = null;
        var inflictor = sim.AddBot(400, 0); uint? observed = null;
        victim.States.Configure(victim, [new(-1, 0), new(-1, 1, a => observed = a.TargetId)], 0);
        victim.DeathState = 1;
        ActorDamage.Apply(victim, 1000, sim.Players.Single(), inflictor: inflictor);
        Assert.Equal(sim.Players.Single().Id, observed);
    }

    [Fact]
    public void SourceLessDeathPreservesPreviousTarget()
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); victim.SetTargetPointer(sim.Players.Single());
        ActorDamage.Apply(victim, 1000); Assert.Equal(sim.Players.Single().Id, victim.TargetId);
    }

    [Fact]
    public void DirectHealthDeathDoesNotInventAKillerTarget()
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); victim.SetTargetPointer(sim.Players.Single());
        victim.Health = 0; Assert.Equal(sim.Players.Single().Id, victim.TargetId);
    }

    [Fact]
    public void PlayerDeathExposesKillerAsTarget()
    {
        var sim = Room(); var player = sim.Players.Single(); var killer = sim.AddBot(200, 0);
        ActorDamage.Apply(player, 1000, killer); Assert.Equal(killer.Id, player.TargetId);
    }

    [Fact]
    public void DestroyedKillerClearsDeathTarget()
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); victim.Brain = null;
        var killer = sim.AddBot(400, 0); ActorDamage.Apply(victim, 1000, killer);
        killer.Destroy(); Assert.Null(victim.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeathTargetSurvivesSaveAndIdentityReconstruction(bool reconstruct)
    {
        var sim = Room(); var victim = sim.AddBot(200, 0); var id = victim.Id;
        ActorDamage.Apply(victim, 1000, sim.Players.Single());
        var saved = SimSavegame.Write(sim, true);
        if (reconstruct) { victim.Destroy(); sim.Tick(); }
        else victim.SetTargetPointer(null);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(sim.Players.Single().Id, sim.Actors.Single(a => a.Id == id).TargetId);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
