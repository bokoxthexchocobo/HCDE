using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryWeaponTossTests
{
    [Theory]
    [InlineData("Chainsaw", WeaponKind.Chainsaw, PickupCatalog.Chainsaw)]
    [InlineData("Pistol", WeaponKind.Pistol, PickupCatalog.Pistol)]
    [InlineData("Shotgun", WeaponKind.Shotgun, PickupCatalog.Shotgun)]
    [InlineData("SuperShotgun", WeaponKind.SuperShotgun, PickupCatalog.SuperShotgun)]
    [InlineData("Chaingun", WeaponKind.Chaingun, PickupCatalog.Chaingun)]
    [InlineData("RocketLauncher", WeaponKind.RocketLauncher, PickupCatalog.RocketLauncher)]
    [InlineData("PlasmaRifle", WeaponKind.Plasma, PickupCatalog.PlasmaRifle)]
    [InlineData("BFG9000", WeaponKind.Bfg, PickupCatalog.Bfg)]
    public void TossAndRecollectionConserveAmmo(string type, WeaponKind weapon, int editorNumber)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, type, 1);
        var ammo = Ammo(player);
        Assert.True(AcsPlayerInventory.Drop(player, type));
        Assert.False(player.Inventory.Owns(weapon));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == editorNumber);
        Assert.True(drop.SuppressWeaponPickupAmmo);
        Assert.Equal(30, drop.PickupDelay);
        Assert.False(drop.SpecialPickup);
        Assert.Equal(ammo, Ammo(player));
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 30; i++) sim.Tick();
        Assert.True(player.Inventory.Owns(weapon));
        Assert.Equal(ammo, Ammo(player));
        Assert.DoesNotContain(drop, sim.Actors);
    }

    [Fact]
    public void DroppingReadyWeaponRaisesReplacementImmediately()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Pending = null;
        Assert.True(AcsPlayerInventory.Drop(player, "Shotgun"));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(122, player.WeaponOffsetY);
    }

    [Fact]
    public void SaveRoundTripKeepsZeroAmmoFlagAfterDelayExpires()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        AcsPlayerInventory.Drop(player, "Shotgun");
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Shotgun);
        drop.X = Fixed.FromInt(500); drop.VelocityX = default;
        for (var i = 0; i < 30; i++) sim.Tick();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(40, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var checksum = sim.Checksum;
        drop.SuppressWeaponPickupAmmo = false;
        sim.RestoreState(state);
        Assert.True(drop.SuppressWeaponPickupAmmo);
        Assert.Equal(0, drop.PickupDelay);
        Assert.Equal(checksum, sim.Checksum);
        var shells = player.Inventory.Shells;
        drop.X = player.X; drop.Y = player.Y; drop.Z = player.Z;
        sim.Tick();
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(shells, player.Inventory.Shells);
    }

    [Fact]
    public void UnknownPickupFlagBitsAreRejected()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        AcsPlayerInventory.Drop(player, "Shotgun");
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 12), 256);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    private static int[] Ammo(PlayerPawn player) =>
        [player.Inventory.Bullets, player.Inventory.Shells, player.Inventory.Rockets, player.Inventory.Cells];

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, spawnOptions: new SpawnOptions(Skill: 0));
}
