using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ClearTargetFallbackTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ClearTargetCannotRevealOlderFallback(bool activeTarget)
    {
        var (sim, actor, oldTarget) = Room();
        if (activeTarget) actor.SetTargetPointer(sim.Players.Single());
        actor.LastHeardTargetId = oldTarget.Id; actor.MasterId = oldTarget.Id; actor.TracerId = oldTarget.Id;
        ActorPropertyActions.ClearTarget(actor);
        Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId); Assert.Null(actor.Brain!.TargetId);
        Assert.Null(actor.LastHeardTargetId); Assert.Equal(oldTarget.Id, actor.MasterId); Assert.Equal(oldTarget.Id, actor.TracerId);
        actor.Brain = null; Assert.Null(actor.TargetId);
    }

    [Fact]
    public void RestoringClearedSnapshotRemovesLaterHiddenFallback()
    {
        var (sim, actor, oldTarget) = Room(); ActorPropertyActions.ClearTarget(actor);
        var saved = SimSavegame.Write(sim, true);
        actor.StoredTargetId = oldTarget.Id; actor.SetTargetPointer(sim.Players.Single());
        SimSavegame.Apply(sim, saved);
        Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RestoringActiveBrainTargetDoesNotRetainAnotherFallback()
    {
        var (sim, actor, oldTarget) = Room(); actor.SetTargetPointer(sim.Players.Single());
        var saved = SimSavegame.Write(sim, true); SimSavegame.Apply(sim, saved);
        Assert.Equal(sim.Players.Single().Id, actor.TargetId); Assert.Null(actor.StoredTargetId);
        actor.Brain!.ClearTarget(); Assert.Null(actor.TargetId); Assert.NotNull(oldTarget);
    }

    [Fact]
    public void ClearingFromStateActionDoesNotRevealFallback()
    {
        var (sim, actor, _) = Room(); actor.SetTargetPointer(sim.Players.Single());
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: ActorPropertyActions.ClearTarget)], 0);
        actor.States.Enter(actor, 1); Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor, Actor OldTarget) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }]
        });
        var actor = sim.AddBot(0, 0); var oldTarget = sim.AddBot(200, 0, 3001);
        var brain = actor.Brain; actor.Brain = null; actor.SetTargetPointer(oldTarget); actor.Brain = brain;
        return (sim, actor, oldTarget);
    }
}
