using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageWakeTargetGateTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExistingUnshootableTargetStillRefreshesThresholdAndSeeState(bool dead)
    {
        var sim = Room(); var victim = sim.AddBot(0, 0, 3004);
        var source = sim.AddBot(200, 0, 3001); source.ThingId = 7;
        victim.Brain!.SetTargetThingId(sim, 7);
        victim.Brain.SetChaseThreshold(0, false);
        victim.ReactionTime = 17;
        // This fixture's managed class has no See label; use an existing state as its See destination.
        victim.SeeState = victim.PainState;
        victim.States.Enter(victim, victim.SpawnState);
        if (dead) source.Health = 0;
        source.Shootable = false;
        ActorDamage.Apply(victim, 1, source, DamageFlags.NoPain);
        Assert.Equal(source.Id, victim.Brain.TargetId);
        Assert.Equal(victim.Brain.DefThreshold, victim.Brain.Threshold);
        Assert.Equal(victim.SeeState, victim.States.Current);
        Assert.Equal(0, victim.ReactionTime);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void NewDeadTargetRequiresShootableFlag(bool shootable, bool targets)
    {
        var sim = Room(); var victim = sim.AddBot(0, 0, 3004);
        var source = sim.AddBot(200, 0, 3001);
        source.Health = 0; source.Shootable = shootable;
        ActorDamage.Apply(victim, 1, source, DamageFlags.NoPain);
        Assert.Equal(targets ? source.Id : (uint?)null, victim.Brain!.TargetId);
        Assert.Equal(targets ? victim.Brain.DefThreshold : 0, victim.Brain.Threshold);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }],
    });
}
