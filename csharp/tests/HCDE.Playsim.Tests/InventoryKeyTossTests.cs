using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryKeyTossTests
{
    [Theory]
    [InlineData("BlueCard", PickupCatalog.BlueCard)]
    [InlineData("BlueSkull", PickupCatalog.BlueSkull)]
    [InlineData("RedCard", PickupCatalog.RedCard)]
    [InlineData("RedSkull", PickupCatalog.RedSkull)]
    [InlineData("YellowCard", PickupCatalog.YellowCard)]
    [InlineData("YellowSkull", PickupCatalog.YellowSkull)]
    public void KeyDropSpawnsRequestedClassWithDelayedEligibility(string type, int editorNumber)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, type, 1);
        var random = sim.CombatRandomState;
        Assert.True(AcsPlayerInventory.Drop(player, type.ToLowerInvariant()));
        Assert.Equal(0, AcsPlayerInventory.Count(player, type, false));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == editorNumber);
        Assert.Equal(30, drop.PickupDelay);
        Assert.False(drop.SpecialPickup);
        Assert.Equal(10, drop.Z.ToDouble());
        Assert.Equal(5, drop.VelocityX.ToDouble());
        Assert.Equal(random, sim.CombatRandomState);
        Assert.False(AcsPlayerInventory.Drop(player, type));
    }

    [Fact]
    public void RecollectionRestoresKeyAfterDelay()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "BlueSkull", 1);
        AcsPlayerInventory.Drop(player, "BlueSkull");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.BlueSkull);
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.False(player.Inventory.BlueKey);
        sim.Tick();
        Assert.True(player.Inventory.BlueKey);
        Assert.DoesNotContain(drop, sim.Actors);
    }

    [Fact]
    public void DroppingOneColorPreservesOtherColors()
    {
        var sim = Room(); var player = sim.Players.Single();
        foreach (var type in new[] { "BlueCard", "RedCard", "YellowCard" })
            AcsPlayerInventory.Give(player, type, 1);
        AcsPlayerInventory.Drop(player, "RedCard");
        Assert.True(player.Inventory.BlueKey);
        Assert.False(player.Inventory.RedKey);
        Assert.True(player.Inventory.YellowKey);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
