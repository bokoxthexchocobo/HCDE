using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorStoredArmorTakeTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(-1, 1)]
    [InlineData(int.MinValue, 0)]
    public void ExplicitRemovalUsesHeldClassAndDoesNotEquip(int amount, int remaining)
    {
        var player = Room().Players.Single(); Prepare(player);
        Assert.True(ActorInventoryActions.TakeInventory(player, "sToReDsUiT", amount, flags: 1));
        Assert.Equal(remaining, player.Inventory.SpareArmor.Count(spare => spare.ArmorType == "StoredSuit"));
        Assert.Contains(player.Inventory.SpareArmor, spare => spare.ArmorType == "OtherSuit");
        Assert.Equal(10, player.Inventory.Armor);
        Assert.Equal(100, player.Health);
        Assert.False(ActorInventoryActions.TakeInventory(player, "MissingSuit"));
    }

    [Fact]
    public void TargetStateActionResolvesPointerAndPreservesNullOrDestroyedReceivers()
    {
        var sim = Room(); var player = sim.Players.Single(); Prepare(player);
        var caller = sim.AddBot(20, 0); var target = sim.AddBot(40, 0);
        caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "StoredSuit", 1, pointerSelector: 4))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "StoredSuit", false));
        Assert.False(ActorInventoryActions.TakeInventory(player, "StoredSuit", pointerSelector: 1));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "StoredSuit"));
        Assert.Equal(2, player.Inventory.SpareArmor.Count);
    }

    [Fact]
    public void FamilyRemovalCountsSuccessfulReceiversAndExcludesCaller()
    {
        var sim = Room(); var player = sim.Players.Single(); Prepare(player);
        var parent = sim.AddBot(20, 0); var sibling = sim.AddBot(40, 0);
        player.MasterId = parent.Id; sibling.MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "StoredSuit", 1));
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(sibling, "StoredSuit"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "StoredSuit"));
        Assert.Equal("OtherSuit", Assert.Single(player.Inventory.SpareArmor).ArmorType);
    }

    [Fact]
    public void ExplicitRemovalStillConsumesWithInfiniteUseAndSurvivesSave()
    {
        var sim = Room(); var player = sim.Players.Single(); Prepare(player);
        Assert.True(ActorInventoryActions.TakeInventory(player, "StoredSuit", 1));
        var saved = SimSavegame.Write(sim);
        player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "StoredSuit", false));
        Assert.True(ActorInventoryActions.TakeInventory(player, "StoredSuit"));
        Assert.False(ActorInventoryActions.TakeInventory(player, "StoredSuit"));
        Assert.Equal("OtherSuit", Assert.Single(player.Inventory.SpareArmor).ArmorType);
    }

    private static void Prepare(PlayerPawn player)
    {
        player.Inventory.Armor = 10;
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
        player.Inventory.TryKeepArmorPickup(200, 33, armorType: "OtherSuit");
        player.Inventory.TryKeepArmorPickup(100, 50, armorType: "StoredSuit");
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteStoredInventoryUse: true));
}
