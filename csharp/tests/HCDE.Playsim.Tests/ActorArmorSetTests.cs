using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorArmorSetTests
{
    [Theory]
    [InlineData("GreenArmor", 100, 33)]
    [InlineData("BlueArmor", 200, 50)]
    [InlineData("ArmorBonus", 1, 33)]
    [InlineData("BlueArmorForMegasphere", 200, 50)]
    public void CreationUsesDefaultSaveAmountWithoutCountOrSkillMultiplication(string name, int armor, int percent)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), 100, beyondMax: true));
        Assert.Equal(armor, player.Inventory.Armor); Assert.Equal(percent, player.Inventory.ArmorSavePercent);
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.True(ActorInventoryActions.SetInventory(player, name, -1));
        Assert.Equal(armor, player.Inventory.Armor);
    }

    [Fact]
    public void FailedSuitPickupPreservesWornMetadataAndBonusUsesItsCap()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "BlueArmor", 1));
        Assert.False(ActorInventoryActions.SetInventory(player, "GreenArmor", 1));
        Assert.False(ActorInventoryActions.SetInventory(player, "BlueArmor", 1));
        Assert.False(ActorInventoryActions.SetInventory(player, "ArmorBonus", 1));
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal("BlueArmor", player.Inventory.ArmorType);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
    }

    [Fact]
    public void TargetStateCallbackAndSavePreserveCreatedArmor()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.Brain!.SetSpecialTarget(player); var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "GreenArmor", 1, AcsActorPointer.Target))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.Armor = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(100, player.Inventory.Armor); Assert.Equal(33, player.Inventory.ArmorSavePercent);
        Assert.False(ActorInventoryActions.SetInventory(player, "GreenArmor", 1, AcsActorPointer.Null));
        Assert.False(ActorInventoryActions.SetInventory(caller, "GreenArmor", 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "GreenArmor", 1));
        Assert.Equal(100, player.Inventory.Armor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(ArmorFactor: 2));
}
