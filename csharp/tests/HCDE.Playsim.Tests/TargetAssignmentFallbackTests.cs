using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TargetAssignmentFallbackTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedAssignmentClearsObsoleteFallback(bool assignTarget)
    {
        var (sim, actor) = Room(); var target = sim.Players.Single();
        actor.SetTargetPointer(assignTarget ? target : null);
        Assert.Equal(assignTarget ? target.Id : (uint?)null, actor.TargetId);
        Assert.Null(actor.StoredTargetId);
        actor.Brain!.ClearTarget(); Assert.Null(actor.TargetId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RearrangementDoesNotRetainOldFallback(bool clear)
    {
        var (sim, actor) = Room(); actor.MasterId = sim.Players.Single().Id;
        ActorPointerActions.RearrangePointers(actor, clear ? AcsActorPointer.Null : AcsActorPointer.Master);
        Assert.Equal(clear ? (uint?)null : actor.MasterId, actor.TargetId); Assert.Null(actor.StoredTargetId);
        actor.Brain!.ClearTarget(); Assert.Null(actor.TargetId);
    }

    [Fact]
    public void PointerTransferNullDoesNotRevealOldFallback()
    {
        var (_, actor) = Room();
        ActorPointerActions.TransferPointer(actor, sourceField: AcsActorPointer.Tracer, recipientField: AcsActorPointer.Target);
        Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId);
    }

    [Fact]
    public void ClearedAssignmentStaysClearedAcrossSave()
    {
        var (sim, actor) = Room(); actor.SetTargetPointer(null); var saved = SimSavegame.Write(sim, true);
        actor.SetTargetPointer(sim.Players.Single()); SimSavegame.Apply(sim, saved);
        Assert.Null(actor.TargetId); Assert.Null(actor.StoredTargetId); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void AssignmentWithoutBrainStillUsesGeneralPointer()
    {
        var (sim, actor) = Room(); actor.Brain = null; actor.SetTargetPointer(sim.Players.Single());
        Assert.Equal(sim.Players.Single().Id, actor.StoredTargetId); Assert.Equal(actor.StoredTargetId, actor.TargetId);
        actor.SetTargetPointer(null); Assert.Null(actor.TargetId);
    }

    private static (AuthoritySimulation Sim, Actor Actor) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = 500 }]
        });
        var actor = sim.AddBot(0, 0); var previous = sim.AddBot(200, 0, 3001);
        var brain = actor.Brain; actor.Brain = null; actor.SetTargetPointer(previous); actor.Brain = brain;
        Assert.NotNull(actor.StoredTargetId);
        return (sim, actor);
    }
}
