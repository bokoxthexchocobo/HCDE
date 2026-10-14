using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryUseSoundTests
{
    [Theory]
    [InlineData("MedPatch")]
    [InlineData("MedicalKit")]
    [InlineData("SurgeryKit")]
    [InlineData("ArtiHealth")]
    [InlineData("ArtiSuperHealth")]
    [InlineData("BasicArmorPickup")]
    public void SuccessfulManualUseRequestsInheritedConsolePlayerSoundAndReplays(string name)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 40;
        if (name == "BasicArmorPickup")
        {
            player.Inventory.TryKeepArmorPickup(100, 33);
            player.Inventory.TryKeepArmorPickup(100, 33);
            player.Inventory.Armor = 0;
        }
        else Assert.True(player.Inventory.GiveHealthPickup(name, 1));
        var saved = SimSavegame.Write(sim, true);
        Assert.True(ActorInventoryActions.UseInventory(player, name));
        var sound = Assert.Single(sim.DrainSoundRequests());
        Assert.Equal("misc/invuse", sound.Sound); Assert.Equal(player.Id, sound.ActorId);
        Assert.Equal(3, sound.Channel); Assert.Equal(0, sound.Flags);
        Assert.Equal(1, sound.Attenuation); Assert.True(sound.ConsolePlayerOnly);
        Assert.False(sound.PickupSound);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(1, AcsPlayerInventory.Use(player, name));
        Assert.Equal(sound, Assert.Single(sim.DrainSoundRequests()));
    }

    [Fact]
    public void FailedUseAndWeaponSelectionDoNotRequestInventorySound()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(player.Inventory.GiveHealthPickup("MedPatch", 1));
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch"));
        Assert.False(ActorInventoryActions.UseInventory(player, "MedicalKit"));
        Assert.False(ActorInventoryActions.UseInventory(player, "Fist"));
        Assert.Empty(sim.DrainSoundRequests());
        player.Health = 0;
        sim.DrainSoundRequests(); // The health setter may emit the separate death sound.
        Assert.False(ActorInventoryActions.UseInventory(player, "MedPatch"));
        Assert.Empty(sim.DrainSoundRequests());
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void SuccessfulInfiniteInventoryUseStillRequestsSound()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.Health = 40;
        Assert.True(player.Inventory.GiveHealthPickup("MedPatch", 1));
        Assert.Equal(1, AcsPlayerInventory.Use(player, "MedPatch"));
        Assert.Equal(1, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Single(sim.DrainSoundRequests());
    }

    private static AuthoritySimulation Room(bool infinite = false) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, spawnOptions: new SpawnOptions(InfiniteHealthItemUse: infinite));
}
