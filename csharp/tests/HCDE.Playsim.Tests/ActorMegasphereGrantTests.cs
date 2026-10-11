using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorMegasphereGrantTests
{
    [Fact]
    public void NativePickupSequenceGrantsBothHelpersThroughStateCallbacks()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(player); player.Health = 10;
        var armor = false; var health = false;
        caller.States.Configure(caller, [new(-1, 0),
            new(0, 2, Action: self => armor = ActorInventoryActions.GiveToTarget(self, "BlueArmorForMegasphere")),
            new(5, 0, Action: self => health = ActorInventoryActions.GiveToTarget(self, "MegasphereHealth"))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(armor); Assert.True(health); Assert.Equal(2, caller.States.Current);
        Assert.Equal(200, player.Health); Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent); Assert.Equal("BlueArmorForMegasphere", player.Inventory.ArmorType);
        Assert.False(ActorInventoryActions.GiveInventory(player, "MegasphereHealth"));
        Assert.False(ActorInventoryActions.GiveInventory(player, "BlueArmorForMegasphere"));
    }

    [Theory]
    [InlineData(0, 200)]
    [InlineData(-1, 200)]
    [InlineData(2, 200)]
    public void ArmorActionQuantityDoesNotMultiplySaveAmount(int amount, int expected)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "bluearmorformegasphere", amount));
        Assert.Equal(expected, player.Inventory.Armor); Assert.Equal(expected, player.Inventory.ArmorMaximum);
        Assert.Equal(expected, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void PatchedHealthClassMultipliesDefaultUnlikeAcsGrant()
    {
        var player = Room("Misc 0\nMegasphere Health = 300\n").Players.Single(); player.Health = 10;
        Assert.True(ActorInventoryActions.GiveInventory(player, "MegasphereHealth")); Assert.Equal(300, player.Health);
        player.Health = 10; AcsPlayerInventory.Give(player, "MegasphereHealth", 1); Assert.Equal(11, player.Health);
    }

    [Fact]
    public void PatchedBlueArmorClassDoesNotChangeMegasphereArmorAndSaveRetainsIt()
    {
        var sim = Room("Misc 0\nBlue Armor Class = 1\n"); var player = sim.Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "BlueArmorForMegasphere"));
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal(50, player.Inventory.ArmorSavePercent);
        var saved = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(200, player.Inventory.Armor); Assert.Equal("BlueArmorForMegasphere", player.Inventory.ArmorType);
    }

    private static AuthoritySimulation Room(string? patch = null) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: patch is null ? null : DehackedPatch.Apply(patch));
}
