using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathWeaponAmmoTests
{
    [Theory]
    [InlineData(WeaponKind.Pistol, PickupCatalog.Pistol)]
    [InlineData(WeaponKind.Shotgun, PickupCatalog.Shotgun)]
    [InlineData(WeaponKind.SuperShotgun, PickupCatalog.SuperShotgun)]
    [InlineData(WeaponKind.Chaingun, PickupCatalog.Chaingun)]
    [InlineData(WeaponKind.RocketLauncher, PickupCatalog.RocketLauncher)]
    [InlineData(WeaponKind.Plasma, PickupCatalog.PlasmaRifle)]
    [InlineData(WeaponKind.Bfg, PickupCatalog.Bfg)]
    public void DeathDropCopiesHeldAmmoAndIgnoresRecipientSkill(WeaponKind weapon, int editorNumber)
    {
        var sim = Room(); var player = sim.Players.Single();
        SetAmmo(player, 73);
        player.Inventory.Weapons |= weapon;
        player.Inventory.Selected = weapon;
        ActorDamage.Apply(player, 1000);
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == editorNumber);
        Assert.Equal(73, drop.PickupAmount);
        Assert.True(drop.IgnoreAmmoSkill);
        Assert.False(drop.SuppressWeaponPickupAmmo);
        Assert.True(player.Inventory.Owns(weapon));
        Assert.Equal(73, player.Inventory.Ammo(WeaponCatalog.Find(weapon)!.Ammo!.Value));
        var receiver = Room().Players.Single();
        AcsPlayerInventory.Clear(receiver);
        Assert.True(PickupCatalog.TryGive(receiver, editorNumber, drop.IgnoreAmmoSkill,
            pickupAmount: drop.PickupAmount, suppressWeaponAmmo: drop.SuppressWeaponPickupAmmo));
        Assert.True(receiver.Inventory.Owns(weapon));
        var kind = WeaponCatalog.Find(weapon)!.Ammo!.Value;
        var maximum = kind is AmmoKind.Shells or AmmoKind.Rockets ? 50 : kind == AmmoKind.Bullets ? 200 : 300;
        Assert.Equal(Math.Min(73, maximum), receiver.Inventory.Ammo(kind));
    }

    [Fact]
    public void EmptyPoolDropsWeaponWithoutDefaultAmmo()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Bullets = 0;
        ActorDamage.Apply(player, 1000);
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.True(drop.SuppressWeaponPickupAmmo);
        var receiver = new PlayerPawn(); AcsPlayerInventory.Clear(receiver);
        Assert.True(PickupCatalog.TryGive(receiver, drop.DoomEdNum, drop.IgnoreAmmoSkill,
            pickupAmount: drop.PickupAmount, suppressWeaponAmmo: drop.SuppressWeaponPickupAmmo));
        Assert.True(receiver.Inventory.Owns(WeaponKind.Pistol));
        Assert.Equal(0, receiver.Inventory.Bullets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(73)]
    public void SavedDeathDropPreservesAmmoGrant(int amount)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Bullets = amount;
        ActorDamage.Apply(player, 1000);
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Pistol);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        drop.PickupAmount = 9; drop.SuppressWeaponPickupAmmo = !drop.SuppressWeaponPickupAmmo;
        sim.RestoreState(state);
        Assert.Equal(amount, drop.PickupAmount);
        Assert.Equal(amount == 0, drop.SuppressWeaponPickupAmmo);
        Assert.True(drop.IgnoreAmmoSkill);
    }

    private static void SetAmmo(PlayerPawn player, int amount)
    {
        player.Inventory.Bullets = amount; player.Inventory.Shells = amount;
        player.Inventory.Rockets = amount; player.Inventory.Cells = amount;
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(Skill: 0));
        sim.WeaponDrop = true;
        return sim;
    }
}
