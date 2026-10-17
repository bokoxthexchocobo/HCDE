using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFallActionTests
{
    [Theory]
    [InlineData(3004)]
    [InlineData(9)]
    [InlineData(65)]
    [InlineData(3001)]
    [InlineData(1)]
    public void FallMatchesNativeNoBlockingIncludingDropsAndFreshSave(int type)
    {
        var sim = Room(type); var reference = Room(type);
        var actor = sim.Actors.Single(); var expected = reference.Actors.Single();
        actor.Corpse = expected.Corpse = true;
        ActorUnblockActions.Fall(actor); ActorUnblockActions.NoBlocking(expected);
        Assert.False(actor.Solid); Assert.True(actor.Shootable); Assert.True(actor.Corpse);
        Assert.False(actor.Destroyed); Assert.Empty(sim.PendingSoundRequests);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(SimSavegame.Write(reference, true), saved);
        var fresh = Room(type); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void FallCanRunFromAnActorStateWithoutChangingShootability()
    {
        var actor = new Actor { Solid = true, Shootable = false, IceCorpse = true };
        actor.States.Configure(actor, [new(-1, 0), new(-1, 1, ActorUnblockActions.Fall)], 0);
        actor.States.Enter(actor, 1);
        Assert.False(actor.Solid); Assert.False(actor.Shootable);
        Assert.True(actor.IceCorpse); Assert.False(actor.Destroyed);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = type }]
    });
}
