using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RememberedFriendlyNoiseTests
{
    [Fact]
    public void NoiseRecordedWhileDisabledWakesIntoSeeLater()
    {
        var (sim, actor, friend, _) = Room(); actor.Brain!.Enabled = false;
        SoundPropagation.Alert(sim, friend); Assert.Equal(friend.Id, actor.LastHeardTargetId); Assert.Equal(0, actor.States.Current);
        actor.Brain.Enabled = true; actor.Brain.Tick(sim, actor);
        Assert.Equal(1, actor.States.Current); Assert.Null(actor.TargetId); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void StoredNoiseDoesNotRestartSeeFrameOnEveryTick()
    {
        var (sim, actor, friend, _) = Room(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, _ => calls++)], 0);
        actor.LastHeardTargetId = friend.Id;
        actor.Brain!.Tick(sim, actor); actor.Brain.Tick(sim, actor);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void ReplacementEnemyEntersSeeOnlyOnce()
    {
        var (sim, actor, friend, enemy) = Room(); var calls = 0;
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, _ => calls++)], 0);
        actor.LastHeardTargetId = friend.Id; actor.Brain!.RememberHateTarget(enemy.Id);
        actor.Brain.Tick(sim, actor); Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(1, calls);
    }

    [Fact]
    public void HateIgnoresRememberedFriendlyNoise()
    {
        var (sim, actor, friend, _) = Room(); actor.TidToHate = 999; actor.LastHeardTargetId = friend.Id;
        actor.Brain!.Tick(sim, actor); Assert.Equal(0, actor.States.Current); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void DeadFriendlyNoiseDoesNotEnterSee()
    {
        var (sim, actor, friend, _) = Room(); actor.LastHeardTargetId = friend.Id; friend.Health = 0;
        actor.Brain!.Tick(sim, actor); Assert.Equal(0, actor.States.Current); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void SavedFriendlyNoiseReplaysTransition()
    {
        var (sim, actor, friend, _) = Room(); actor.LastHeardTargetId = friend.Id;
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(1, actor.States.Current); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor Friend, Actor Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 3001, X = 200 }, new LevelThing { Type = 3002, Y = 300 }] });
        var actor = sim.Actors[0]; var friend = sim.Actors[1]; actor.Friendly = friend.Friendly = true;
        actor.PainState = -1; actor.SeeState = 1; actor.States.Configure(actor, [new(-1, 0), new(-1, 1)], 0);
        return (sim, actor, friend, sim.Actors[2]);
    }
}
