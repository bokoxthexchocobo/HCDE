using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsUntossableInventoryTests
{
    [Theory]
    [InlineData("PowerDamage")]
    [InlineData("PowerBuddha")]
    [InlineData("PowerProtection")]
    public void PowerupDropFailsWithoutEndingEffectOrCreatingPickup(string typeName)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, typeName, 1);
        var damage = player.PowerDamageTics;
        var buddha = player.PowerBuddhaTics;
        var protection = player.PowerProtectionTics;
        Assert.False(AcsPlayerInventory.Drop(player, typeName));
        Assert.Equal(damage, player.PowerDamageTics);
        Assert.Equal(buddha, player.PowerBuddhaTics);
        Assert.Equal(protection, player.PowerProtectionTics);
        Assert.Single(sim.Actors);
        Assert.Equal(1, AcsPlayerInventory.Count(player, typeName, false));
    }

    [Fact]
    public void UnsupportedLiveSpareArmorDropPreservesEveryCopy()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve");
        player.Inventory.TryKeepArmorPickup(60, 50, armorType: "Reserve");
        Assert.False(AcsPlayerInventory.Drop(player, "Reserve"));
        Assert.Equal(2, player.Inventory.SpareArmor.Count);
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Single(sim.Actors);
    }

    [Fact]
    public void ActorStateCallingAcsDropKeepsUntossablePowerAndFrame()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "PowerDamage", 1);
        var duration = player.PowerDamageTics; var result = true;
        player.States.Configure(player, [new(-1, 0), new(4, 0,
            Action: self => result = AcsPlayerInventory.Drop(self, "PowerDamage"))], 0);
        player.States.Enter(player, 1);
        Assert.False(result); Assert.Equal(1, player.States.Current);
        Assert.Equal(duration, player.PowerDamageTics);
        var bytes = SimSavegame.Write(sim);
        player.PowerDamageTics = 0; SimSavegame.Apply(sim, bytes);
        Assert.Equal(duration, player.PowerDamageTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
