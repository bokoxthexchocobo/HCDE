using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWornArmorSetTests
{
    [Theory]
    [InlineData(100, false, 100, false)]
    [InlineData(50, false, 50, true)]
    [InlineData(0, false, 0, true)]
    [InlineData(-10, false, 0, true)]
    [InlineData(300, false, 100, true)]
    [InlineData(300, true, 300, true)]
    public void SetterUsesNativeReturnAndMaximumRules(int amount, bool beyondMax, int expected, bool changed)
    {
        var player = Room().Players.Single();
        PickupCatalog.GiveArmor(player.Inventory, 100, 50, "BlueArmor");
        player.Inventory.TryKeepArmorPickup(200, 33, armorType: "SpareSuit");
        Assert.Equal(changed, ActorInventoryActions.SetInventory(player, "bAsIcArMoR", amount, beyondMax: beyondMax));
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(100, player.Inventory.ArmorMaximum);
        Assert.Equal(100, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal("BlueArmor", player.Inventory.ArmorType);
        Assert.Single(player.Inventory.SpareArmor);
    }

    [Fact]
    public void DecreaseDoesNotClampPreviouslyOverMaximumArmor()
    {
        var player = Room().Players.Single();
        PickupCatalog.GiveArmor(player.Inventory, 100, 50, "BlueArmor");
        Assert.True(ActorInventoryActions.SetInventory(player, "Armor", 300, beyondMax: true));
        Assert.True(ActorInventoryActions.SetInventory(player, "Armor", 200));
        Assert.Equal(200, player.Inventory.Armor);
        Assert.True(ActorInventoryActions.SetInventory(player, "Armor", 250));
        Assert.Equal(100, player.Inventory.Armor);
    }

    [Fact]
    public void StateCallbackSelectsMasterAndSavePreservesSetAmount()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        PickupCatalog.GiveArmor(player.Inventory, 100, 50, "BlueArmor");
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "BasicArmor", 150, 4, true))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.Armor = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(150, player.Inventory.Armor);
        Assert.Equal(100, player.Inventory.ArmorMaximum);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.False(ActorInventoryActions.SetInventory(player, "Armor", 20, pointerSelector: 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "Armor", 20));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "Armor", 20));
        Assert.Equal(150, player.Inventory.Armor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
