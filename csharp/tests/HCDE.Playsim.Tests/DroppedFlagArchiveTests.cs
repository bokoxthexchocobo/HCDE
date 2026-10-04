using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedFlagArchiveTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlagInterfaceAndSaveRestoreAgreeForNonPickupActor(bool dropped)
    {
        var sim = Room(); var actor = sim.Players.Single();
        Assert.True(AcsActorFlags.TrySet(actor, "dRoPpEd", dropped));
        Assert.True(AcsActorFlags.TryGet(actor, "DROPPED", out var actual));
        Assert.Equal(dropped, actual);
        sim.Tick(); var checksum = sim.Checksum;
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        actor.Dropped = !dropped;
        sim.RestoreState(state);
        Assert.Equal(dropped, actor.Dropped);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void DropRestoreDoesNotChangeGrantMetadata()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        AcsPlayerInventory.Drop(player, "Shotgun");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
        var state = sim.CaptureState();
        AcsActorFlags.TrySet(drop, "DROPPED", false);
        Assert.True(drop.SuppressWeaponPickupAmmo);
        sim.RestoreState(state);
        Assert.True(drop.Dropped);
        Assert.True(drop.SuppressWeaponPickupAmmo);
        Assert.Equal(30, drop.PickupDelay);
    }

    [Fact]
    public void DestroyedActorRejectsDroppedFlagAccess()
    {
        var actor = new Actor(); actor.Destroy();
        Assert.False(AcsActorFlags.TrySet(actor, "DROPPED", true));
        Assert.False(AcsActorFlags.TryGet(actor, "DROPPED", out _));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
