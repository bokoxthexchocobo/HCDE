using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorUnblockTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UnblockingPreservesShootabilityAndOtherDeathFlags(bool shootable)
    {
        var actor = new Actor { Solid = true, Shootable = shootable, IceCorpse = true, Corpse = true };
        ActorUnblockActions.NoBlocking(actor, drop: false);
        Assert.False(actor.Solid); Assert.Equal(shootable, actor.Shootable);
        Assert.True(actor.IceCorpse); Assert.True(actor.Corpse); Assert.False(actor.Destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OptionalVanillaDropsRunOnlyWhenRequested(bool drop)
    {
        var sim = Room(3004); var actor = sim.Actors.Single();
        ActorUnblockActions.NoBlocking(actor, drop);
        Assert.False(actor.Solid); Assert.True(actor.Shootable);
        Assert.Equal(drop ? 1 : 0, sim.Actors.Count(candidate => candidate.DoomEdNum == PickupCatalog.Clip));
        Assert.False(actor.Destroyed);
    }

    [Fact]
    public void PlayerUnblocksWithoutDroppingInventoryOrDrawingRandomness()
    {
        var sim = Room(1); var player = sim.Players.Single(); var random = sim.CombatRandomState;
        var bullets = player.Inventory.Bullets;
        ActorUnblockActions.NoBlocking(player);
        Assert.False(player.Solid); Assert.True(player.Shootable); Assert.Single(sim.Actors);
        Assert.Equal(bullets, player.Inventory.Bullets); Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void DetachedActorWithoutDropMetadataCanUnblock()
    {
        var actor = new Actor(); ActorUnblockActions.NoBlocking(actor);
        Assert.False(actor.Solid); Assert.False(actor.Destroyed);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = type }] });
}
