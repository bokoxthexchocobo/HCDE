using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWornArmorTakeTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-20, 80)]
    [InlineData(20, 80)]
    [InlineData(int.MinValue, 0)]
    public void DepletionPreservesArmorMetadataAndStoredSuit(int amount, int remaining)
    {
        var player = Room().Players.Single();
        PickupCatalog.GiveArmor(player.Inventory, 100, 50, "BlueArmor");
        player.Inventory.TryKeepArmorPickup(200, 33, armorType: "SpareSuit");
        Assert.True(ActorInventoryActions.TakeInventory(player, "bAsIcArMoR", amount, flags: 1));
        Assert.Equal(remaining, player.Inventory.Armor);
        Assert.Equal(100, player.Inventory.ArmorMaximum);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal("BlueArmor", player.Inventory.ArmorType);
        Assert.Single(player.Inventory.SpareArmor);
        Assert.Equal(100, player.Health);
    }

    [Fact]
    public void StateCallbackUsesTargetAndReportsAlreadyDepletedArmor()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.Brain!.SetSpecialTarget(player); player.Inventory.Armor = 10;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "Armor"))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result); Assert.Equal(0, player.Inventory.Armor);
        Assert.False(ActorInventoryActions.TakeFromTarget(caller, "Armor"));
        player.Inventory.Armor = 10;
        Assert.False(ActorInventoryActions.TakeInventory(player, "Armor", pointerSelector: 1));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "Armor"));
        Assert.Equal(10, player.Inventory.Armor);
    }

    [Fact]
    public void FamilyRemovalCountsOnlyChildrenWithArmor()
    {
        var sim = Room(); var player = sim.Players.Single(); var parent = sim.AddBot(20, 0);
        player.MasterId = parent.Id; player.Inventory.Armor = 100;
        var sibling = sim.AddBot(40, 0); sibling.MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "BasicArmor", 25));
        Assert.Equal(75, player.Inventory.Armor);
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(sibling, "Armor"));
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "Armor"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
