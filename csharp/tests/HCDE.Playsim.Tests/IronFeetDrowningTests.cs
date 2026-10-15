using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IronFeetDrowningTests
{
    [Theory]
    [InlineData("Drowning")]
    [InlineData("drowning")]
    public void SuitAbsorbsDrowningWithoutArmorOrDamageFeedback(string type)
    {
        var player = Room().Players.Single(); player.GivePowerIronFeet(); player.Inventory.Armor = 100;
        var result = ActorDamage.Apply(player, 20, damageType: type, inflictor: new Actor { ForcePain = true });
        Assert.Equal(default, result); Assert.Equal(100, player.Health); Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal(0, player.DamageCount); Assert.Null(player.LastDamageSourceId);
    }

    [Theory]
    [InlineData(DamageFlags.None, 100)]
    [InlineData(DamageFlags.NoProtect, 100)]
    [InlineData(DamageFlags.NoFactor, 100)]
    [InlineData(DamageFlags.BypassArmor, 80)]
    [InlineData(DamageFlags.Forced, 80)]
    public void InventoryAbsorptionUsesNativeDamageFlags(DamageFlags flags, int health)
    {
        var player = Room().Players.Single(); player.GivePowerIronFeet();
        ActorDamage.Apply(player, 20, flags: flags, damageType: "Drowning");
        Assert.Equal(health, player.Health);
    }

    [Theory]
    [InlineData("Fire")]
    [InlineData("Slime")]
    [InlineData("None")]
    public void SuitDoesNotAbsorbOtherDirectDamageTypes(string type)
    {
        var player = Room().Players.Single(); player.GivePowerIronFeet();
        ActorDamage.Apply(player, 20, damageType: type); Assert.Equal(80, player.Health);
    }

    [Fact]
    public void ArmorPiercingInflictorBypassesSuitAbsorption()
    {
        var player = Room().Players.Single(); player.GivePowerIronFeet();
        ActorDamage.Apply(player, 20, damageType: "Drowning", inflictor: new Actor { PierceArmor = true });
        Assert.Equal(80, player.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TelefragProtectionRequiresLaxDamage(bool lax)
    {
        var player = Room().Players.Single(); player.GivePowerIronFeet(); player.LaxTelefragDamage = lax;
        ActorDamage.Apply(player, ActorDamage.TelefragDamage, damageType: "Drowning");
        Assert.Equal(!lax, player.IsDead);
        if (lax) Assert.Equal(100, player.Health);
    }

    [Fact]
    public void RemovalAndRestorationControlDrowningProtection()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet();
        var saved = SimSavegame.Write(sim, true); AcsPlayerInventory.Take(player, "PowerIronFeet", 1);
        ActorDamage.Apply(player, 20, damageType: "Drowning"); Assert.Equal(80, player.Health);
        SimSavegame.Apply(sim, saved);
        ActorDamage.Apply(player, 20, damageType: "Drowning"); Assert.Equal(100, player.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    });
}
