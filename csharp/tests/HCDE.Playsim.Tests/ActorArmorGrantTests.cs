using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorArmorGrantTests
{
    [Theory]
    [InlineData("GreenArmor", 100, 33)]
    [InlineData("BlueArmor", 200, 50)]
    [InlineData("ArmorBonus", 1, 33)]
    public void StateQuantityDoesNotMultiplyArmorProtection(string name, int expected, int percent)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, name, 10));
        Assert.Equal(expected, player.Inventory.Armor); Assert.Equal(percent, player.Inventory.ArmorSavePercent);
    }

    [Fact]
    public void PatchedArmorDefaultsAndSkillFactorApplyToStateGrants()
    {
        var player = Room("Misc 0\nGreen Armor Class = 2\nBlue Armor Class = 1\n", armorFactor: 2).Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "GreenArmor", 5));
        Assert.Equal(400, player.Inventory.Armor); Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.False(ActorInventoryActions.GiveInventory(player, "BlueArmor", 5));
        player.Inventory.Armor = 0;
        Assert.True(ActorInventoryActions.GiveInventory(player, "BlueArmor", 5));
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal(33, player.Inventory.ArmorSavePercent);
    }

    [Fact]
    public void BonusPreservesWornProtectionAndRespectsPatchedMaximum()
    {
        var player = Room("Misc 0\nMax Armor = 250\n").Players.Single();
        ActorInventoryActions.GiveInventory(player, "BlueArmor");
        player.Inventory.Armor = 249;
        Assert.True(ActorInventoryActions.GiveInventory(player, "ArmorBonus", 10));
        Assert.Equal(250, player.Inventory.Armor); Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.False(ActorInventoryActions.GiveInventory(player, "ArmorBonus"));
    }

    [Fact]
    public void FamilyGrantCountsSuccessfulPlayerAndRejectsMonsterArmor()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var monster = sim.AddBot(40, 0);
        var player = sim.Players.Single(); player.MasterId = parent.Id; monster.MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "GreenArmor", 5));
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal(0, ActorInventoryActions.GiveToChildren(parent, "GreenArmor"));
    }

    private static AuthoritySimulation Room(string? patch = null, double armorFactor = 1) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: patch is null ? null : DehackedPatch.Apply(patch), spawnOptions: new SpawnOptions(ArmorFactor: armorFactor));
}
