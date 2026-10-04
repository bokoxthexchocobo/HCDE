using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAmmoBoxTests
{
    [Theory]
    [InlineData("ClipBox", AmmoKind.Bullets, 200)]
    [InlineData("ShellBox", AmmoKind.Shells, 50)]
    [InlineData("RocketBox", AmmoKind.Rockets, 50)]
    [InlineData("CellPack", AmmoKind.Cells, 300)]
    public void GrantAddsRequestedParentAmmoAndQueryUsesBoxDefault(string type, AmmoKind kind, int maximum)
    {
        var player = new PlayerPawn();
        var before = player.Inventory.Ammo(kind);
        player.Inventory.Pending = WeaponKind.Pistol;
        AcsPlayerInventory.Give(player, type.ToLowerInvariant(), 3);
        Assert.Equal(before + 3, player.Inventory.Ammo(kind));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Pending);
        Assert.Equal(0, AcsPlayerInventory.Count(player, type, false));
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, type, true));
        Assert.Equal(maximum, AcsPlayerInventory.Count(new Actor(), type, true));
        AcsPlayerInventory.SetAmmoCapacity(player, type, 1);
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, type, true));
        Assert.Equal(before + 3, player.Inventory.Ammo(kind));
    }

    [Theory]
    [InlineData("ClipBox", AmmoKind.Bullets)]
    [InlineData("ShellBox", AmmoKind.Shells)]
    [InlineData("RocketBox", AmmoKind.Rockets)]
    [InlineData("CellPack", AmmoKind.Cells)]
    public void GrantUsesAmmoSkillFactorAndStringTable(string type, AmmoKind kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        }, spawnOptions: new SpawnOptions(Skill: 0));
        var player = sim.Players.Single();
        var before = player.Inventory.Ammo(kind);
        AcsPlayerInventory.Give(player, [type], 0, 3);
        Assert.Equal(before + 6, player.Inventory.Ammo(kind));
        AcsPlayerInventory.Take(player, type, 1);
        Assert.Equal(before + 6, player.Inventory.Ammo(kind));
    }

    [Theory]
    [InlineData("ClipBox", AmmoKind.Bullets)]
    [InlineData("ShellBox", AmmoKind.Shells)]
    [InlineData("RocketBox", AmmoKind.Rockets)]
    [InlineData("CellPack", AmmoKind.Cells)]
    public void BoxMaximumDoesNotTrackBackpackCapacity(string type, AmmoKind kind)
    {
        var player = new PlayerPawn();
        var maximum = AcsPlayerInventory.Count(player, type, true);
        AcsPlayerInventory.Give(player, "Backpack", 1);
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, type, true));
        var before = player.Inventory.Ammo(kind);
        AcsPlayerInventory.Give(player, type, 0);
        AcsPlayerInventory.Give(player, type, -1);
        Assert.Equal(before, player.Inventory.Ammo(kind));
        Assert.Equal(0, AcsPlayerInventory.Count(null, type, true));
    }
}
