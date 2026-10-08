using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GoalReferenceLifetimeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestroyedGoalClearsBeforeAndAfterRemoval(bool remove)
    {
        var sim = Room(); var owner = sim.Actors[0]; var goal = sim.Actors[1];
        owner.GoalId = goal.Id; goal.Destroy();
        if (remove) sim.Tick();
        Assert.Null(owner.GoalId);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        Assert.Null(state.Actors.Single(a => a.Id == owner.Id).GoalPointer);
    }

    [Fact]
    public void SaveCaptureClearsDestroyedGoalWithoutPriorQuery()
    {
        var sim = Room(); var owner = sim.Actors[0]; owner.GoalId = sim.Actors[1].Id;
        sim.Actors[1].Destroy();
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        Assert.Null(state.Actors.Single(a => a.Id == owner.Id).GoalPointer); Assert.Null(owner.GoalId);
    }

    [Fact]
    public void DeadGoalRemainsReferencedUntilDestroyed()
    {
        var sim = Room(); var owner = sim.Actors[0]; var goal = sim.Actors[1];
        owner.GoalId = goal.Id; goal.Health = 0;
        Assert.False(goal.Destroyed); Assert.Equal(goal.Id, owner.GoalId);
        SimSavegame.Apply(sim, SimSavegame.Write(sim)); Assert.Equal(goal.Id, owner.GoalId);
        goal.Destroy(); Assert.Null(owner.GoalId);
    }

    [Fact]
    public void UnresolvedAttachedIdClearsWhileDetachedIdIsRetained()
    {
        var sim = Room(); sim.Actors[0].GoalId = uint.MaxValue; Assert.Null(sim.Actors[0].GoalId);
        var detached = new Actor { GoalId = uint.MaxValue }; Assert.Equal(uint.MaxValue, detached.GoalId);
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Things = [new LevelThing { Type = 3001 }, new LevelThing { Type = 3004, X = 100 }],
            Sectors = [new LevelSector { CeilingHeight = 512 }] });
        foreach (var actor in sim.Actors) actor.Brain = null;
        return sim;
    }
}
