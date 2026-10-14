using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IdleReactionCadenceTests
{
    [Theory]
    [InlineData(8)]
    [InlineData(0)]
    [InlineData(-1)]
    public void UnsuccessfulIdleSearchPreservesReaction(int reaction)
    {
        var (sim, actor, enemy) = Room(); enemy.NoTarget = true; actor.ReactionTime = reaction;
        for (var i = 0; i < 5; i++) actor.Brain!.Tick(sim, actor);
        Assert.Equal(reaction, actor.ReactionTime); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void ReactionCountdownStartsAfterIdleAcquisition()
    {
        var (sim, actor, enemy) = Room(); enemy.NoTarget = true; actor.ReactionTime = 8;
        actor.Brain!.Tick(sim, actor); enemy.NoTarget = false;
        actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false);
        Assert.Equal(enemy.Id, actor.TargetId); Assert.Equal(8, actor.ReactionTime);
        actor.Brain.Tick(sim, actor, useMelee: false, useMissile: false); Assert.Equal(7, actor.ReactionTime);
    }

    [Fact]
    public void SavedIdleCounterRemainsStableAfterRestore()
    {
        var (sim, actor, enemy) = Room(); enemy.NoTarget = true; actor.ReactionTime = 8;
        var saved = SimSavegame.Write(sim, true); actor.Brain!.Tick(sim, actor); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor);
        Assert.Equal(8, actor.ReactionTime); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor, PlayerPawn Enemy) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 }] });
        return (sim, sim.Actors.Single(a => a.ClassDoomEdNum == 3004), sim.Players.Single());
    }
}
