using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DropKeyIdentityTests
{
    [Theory]
    [InlineData("BlueCard", PickupCatalog.BlueCard)]
    [InlineData("BlueSkull", PickupCatalog.BlueSkull)]
    [InlineData("RedCard", PickupCatalog.RedCard)]
    [InlineData("RedSkull", PickupCatalog.RedSkull)]
    [InlineData("YellowCard", PickupCatalog.YellowCard)]
    [InlineData("YellowSkull", PickupCatalog.YellowSkull)]
    public void AcsDropPreservesRequestedKeyClassIdentity(string name, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, compat: CompatSurface.NoTossDrops);
        var player = sim.Players.Single();
        Assert.Equal(1, ActorDropItem.Drop(sim, 0, player, name, 0, 256));
        var drop = Assert.Single(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum));
        Assert.Equal(expected, drop.DoomEdNum);
        sim.Tick();
        Assert.True(drop.Destroyed);
        Assert.Equal(expected is PickupCatalog.BlueCard or PickupCatalog.BlueSkull, player.Inventory.BlueKey);
        Assert.Equal(expected is PickupCatalog.RedCard or PickupCatalog.RedSkull, player.Inventory.RedKey);
        Assert.Equal(expected is PickupCatalog.YellowCard or PickupCatalog.YellowSkull, player.Inventory.YellowKey);
    }
}
