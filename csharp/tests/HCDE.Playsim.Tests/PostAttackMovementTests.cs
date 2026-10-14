using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PostAttackMovementTests
{
    [Theory]
    [InlineData(MonsterChaseFlags.None, true)]
    [InlineData(MonsterChaseFlags.DontMove, false)]
    [InlineData(MonsterChaseFlags.NoPostAttackTurn, false)]
    [InlineData(MonsterChaseFlags.StopIfBlocked, false)]
    [InlineData(MonsterChaseFlags.NightmareFast, true)]
    public void NativeControlsGatePostAttackMovement(MonsterChaseFlags flags, bool moves)
    {
        var (sim, actor) = Room(); actor.JustAttacked = true;
        actor.Brain!.Tick(sim, actor, flags | MonsterChaseFlags.NoPlayActive);
        Assert.False(actor.JustAttacked); Assert.Equal(moves, actor.VelocityX.Raw > 0);
        Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void PostAttackMovementUsesChaseGoalInsteadOfEnemy()
    {
        var (sim, actor) = Room(); var goal = sim.Actors.Single(a => a.ClassDoomEdNum == 9024);
        actor.GoalId = goal.Id; actor.ChaseGoal = true; actor.JustAttacked = true;
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId); Assert.True(actor.VelocityY.Raw > 0);
        Assert.Equal(0, actor.VelocityX.Raw); Assert.Equal(0, actor.Brain.WindupTics);
    }

    [Fact]
    public void NoPostAttackTurnDoesNotSuppressOrdinaryMovement()
    {
        var (sim, actor) = Room();
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive | MonsterChaseFlags.NoPostAttackTurn, false, false);
        Assert.True(actor.VelocityX.Raw > 0);
    }

    [Fact]
    public void SavedGuardReplaysPostAttackMovement()
    {
        var (sim, actor) = Room(); actor.JustAttacked = true; var saved = SimSavegame.Write(sim, true);
        actor.Brain!.Tick(sim, actor, MonsterChaseFlags.NoPlayActive); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); actor.Brain.Tick(sim, actor, MonsterChaseFlags.NoPlayActive);
        Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 300 },
                new LevelThing { Type = 9024, Y = 300 }] });
        var actor = sim.Actors.Single(a => a.ClassDoomEdNum == 3004); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
