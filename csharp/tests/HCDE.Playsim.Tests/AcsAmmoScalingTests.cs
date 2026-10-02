using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsAmmoScalingTests
{
    [Theory]
    [InlineData(0, 14)]
    [InlineData(1, 7)]
    [InlineData(2, 7)]
    [InlineData(3, 7)]
    [InlineData(4, 14)]
    public void AmmoGrantUsesSkillFactor(int skill, int expected)
    {
        var sim = Room(skill);
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        AcsPlayerInventory.Give(player, "Shell", 7);
        Assert.Equal(expected, player.Inventory.Shells);
        Assert.Null(player.Inventory.Pending);
    }

    [Theory]
    [InlineData(0, 30, 12, 3, 60)]
    [InlineData(2, 15, 6, 1, 30)]
    [InlineData(4, 30, 12, 3, 60)]
    public void BackpackUsesScaledDefaultAmmoOncePerGrant(int skill, int bullets, int shells, int rockets, int cells)
    {
        var sim = Room(skill);
        sim.AmmoFactor = 1.5;
        var player = sim.Players.Single();
        player.Inventory.Bullets = 0;
        player.Inventory.Pending = WeaponKind.Chaingun;
        AcsPlayerInventory.Give(player, "Backpack", 50);
        AcsPlayerInventory.Give(player, "Backpack", 1);
        Assert.Equal(2 * bullets, player.Inventory.Bullets);
        Assert.Equal(2 * shells, player.Inventory.Shells);
        Assert.Equal(2 * rockets, player.Inventory.Rockets);
        Assert.Equal(2 * cells, player.Inventory.Cells);
        Assert.Equal(400, player.Inventory.MaxBullets);
        Assert.Equal(WeaponKind.Chaingun, player.Inventory.Pending);
    }

    [Theory]
    [InlineData(0, false, 1.5, 9)]
    [InlineData(2, false, 1.5, 4)]
    [InlineData(2, true, 1.5, 9)]
    [InlineData(0, false, 0.0, 0)]
    public void AmmoGrantMultipliesTruncatesAndPreservesPending(int skill, bool doubleAmmo, double factor, int expected)
    {
        var sim = Room(skill);
        sim.DoubleAmmo = doubleAmmo;
        sim.AmmoFactor = factor;
        var player = sim.Players.Single();
        player.Inventory.Pending = WeaponKind.Shotgun;
        AcsPlayerInventory.Give(player, "RocketAmmo", 3);
        Assert.Equal(expected, player.Inventory.Rockets);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void ScaledGrantsRespectNormalAndBackpackCaps()
    {
        var sim = Room(0);
        var player = sim.Players.Single();
        player.Inventory.Shells = 49;
        AcsPlayerInventory.Give(player, "Shell", 7);
        Assert.Equal(50, player.Inventory.Shells);
        AcsPlayerInventory.Give(player, "Backpack", 1);
        Assert.Equal(58, player.Inventory.Shells);
        AcsPlayerInventory.Give(player, "Shell", 100);
        Assert.Equal(100, player.Inventory.Shells);
    }

    private static AuthoritySimulation Room(int skill) => AuthoritySimulation.Start(new PlayLevel
    {
        MapName = "MAP01",
        Sectors = [new LevelSector { Index = 0, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 32, Y = 64 }],
    }, spawnOptions: new SpawnOptions(Skill: skill));
}
