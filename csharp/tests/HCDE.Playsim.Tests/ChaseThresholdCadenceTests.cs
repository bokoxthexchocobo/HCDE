using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseThresholdCadenceTests
{
    [Fact]
    public void PainDoesNotConsumeChaseThreshold()
    {
        var (sim, actor, _) = Room(); actor.States.Enter(actor, actor.PainState);
        actor.Brain!.Tick(sim, actor); Assert.Equal(10, actor.Brain.Threshold);
        actor.States.Enter(actor, actor.SpawnState); actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(9, actor.Brain.Threshold);
    }

    [Fact]
    public void RaiseStateDoesNotConsumeChaseThreshold()
    {
        var (sim, actor, _) = Room(); actor.RaiseState = 4;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1), new(-1, 2), new(-1, 3), new(-1, 4)], 4);
        actor.Brain!.Tick(sim, actor); Assert.Equal(10, actor.Brain.Threshold);
    }

    [Fact]
    public void SkullChargeDoesNotConsumeChaseThreshold()
    {
        var (sim, actor, enemy) = Room(); actor.Brain!.StartCharge(actor, enemy, 20);
        actor.Brain.Tick(sim, actor); Assert.True(actor.Brain.Charging); Assert.Equal(10, actor.Brain.Threshold);
    }

    [Fact]
    public void PainDefersMissingTargetThresholdResetUntilChase()
    {
        var (sim, actor, enemy) = Room(); actor.States.Enter(actor, actor.PainState); enemy.Destroy();
        actor.Brain!.Tick(sim, actor); Assert.Equal(10, actor.Brain.Threshold);
        actor.States.Enter(actor, actor.SpawnState); actor.Brain.Tick(sim, actor);
        Assert.Equal(0, actor.Brain.Threshold);
    }

    [Fact]
    public void SavedPainPreservesThresholdOnContinuation()
    {
        var (sim, actor, _) = Room(); actor.States.Enter(actor, actor.PainState);
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(10, actor.Brain.Threshold); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, PlayerPawn Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); var enemy = sim.Players.Single();
        actor.SetTargetPointer(enemy); actor.Brain!.SetChaseThreshold(10, false); return (sim, actor, enemy);
    }
}
