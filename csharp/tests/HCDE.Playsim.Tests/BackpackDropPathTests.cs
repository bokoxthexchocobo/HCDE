using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BackpackDropPathTests
{
    [Fact]
    public void PublicAndScriptDropsShareTossProperties()
    {
        var direct = Room(); var scripted = Room();
        foreach (var sim in new[] { direct, scripted })
        {
            var player = sim.Players.Single();
            player.Inventory.GiveBackpack();
            player.VelocityX = Fixed.FromInt(2); player.VelocityZ = Fixed.FromInt(3);
        }
        var drop = direct.DropBackpack(direct.Players.Single())!;
        Assert.True(AcsPlayerInventory.Drop(scripted.Players.Single(), "Backpack"));
        var scriptDrop = Assert.Single(scripted.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
        Assert.Equal(drop.Z, scriptDrop.Z);
        Assert.Equal(drop.VelocityX, scriptDrop.VelocityX);
        Assert.Equal(drop.VelocityZ, scriptDrop.VelocityZ);
        Assert.Equal(drop.PickupDelay, scriptDrop.PickupDelay);
        Assert.True(drop.Depleted);
        Assert.False(drop.SpecialPickup);
        direct.Tick(); scripted.Tick();
        Assert.Equal(direct.Checksum, scripted.Checksum);
    }

    [Fact]
    public void DestroyedOwnerKeepsBackpackWhenSpawnFails()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.GiveBackpack();
        player.Destroy();
        Assert.Null(sim.DropBackpack(player));
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.DoesNotContain(sim.Actors, a => a.DoomEdNum == PickupCatalog.Backpack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
