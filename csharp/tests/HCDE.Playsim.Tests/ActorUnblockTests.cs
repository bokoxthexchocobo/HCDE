using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorUnblockTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CombinedActionEmitsBeforeUnblockingAndDropsOneVanillaItem(bool active)
    {
        var sim = Room(3004); var actor = sim.Actors.Single();
        actor.ActorSounds = new Dictionary<ActorSoundType, string>
        { [active ? ActorSoundType.Active : ActorSoundType.Death] = "combined/sound" };
        var observed = 0;
        sim.SoundRequestsReady += () =>
        {
            observed++; Assert.True(actor.Solid);
            Assert.DoesNotContain(sim.Actors, candidate => candidate.DoomEdNum == PickupCatalog.Clip);
        };
        actor.States.Configure(actor, [new ActorFrame(-1, 0, active
            ? ActorUnblockActions.ActiveAndUnblock : ActorUnblockActions.ScreamAndUnblock)], 0);
        Assert.Equal(1, observed); Assert.False(actor.Solid); Assert.True(actor.Shootable);
        Assert.False(actor.Destroyed);
        Assert.Single(sim.Actors, candidate => candidate.DoomEdNum == PickupCatalog.Clip);
        var request = Assert.Single(sim.PendingSoundRequests);
        Assert.Equal("combined/sound", request.Sound); Assert.Equal(2, request.Channel);
        Assert.Equal((int)SimulationSoundFlags.NoRumble, request.Flags);
    }

    [Theory]
    [InlineData(false, 3004)]
    [InlineData(true, 3004)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    public void CombinedActionWithoutSoundStillUnblocksAndUsesDefaultDropPolicy(bool active, int type)
    {
        var sim = Room(type); var actor = sim.Actors.Single();
        if (active) ActorUnblockActions.ActiveAndUnblock(actor);
        else ActorUnblockActions.ScreamAndUnblock(actor);
        Assert.False(actor.Solid); Assert.True(actor.Shootable); Assert.Empty(sim.PendingSoundRequests);
        Assert.Equal(type == 1 ? 0 : 1, sim.Actors.Count(candidate => candidate.DoomEdNum == PickupCatalog.Clip));
    }

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
