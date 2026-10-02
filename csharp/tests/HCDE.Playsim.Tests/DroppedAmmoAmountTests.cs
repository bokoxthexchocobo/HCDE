using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedAmmoAmountTests
{
    [Theory]
    [InlineData(3004, AmmoKind.Bullets, 5)]
    [InlineData(84, AmmoKind.Bullets, 5)]
    [InlineData(9, AmmoKind.Shells, 4)]
    [InlineData(65, AmmoKind.Bullets, 10)]
    public void ShatteredMonsterDropGrantsHalfMapAmmo(int type, AmmoKind ammo, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 100 }],
        });
        var player = sim.Players.Single();
        var before = player.Inventory.Ammo(ammo);
        sim.SpawnIceChunks(sim.Actors.Single(actor => actor.DoomEdNum == type));
        var drop = Assert.Single(sim.Actors, actor => PickupCatalog.IsPickup(actor.DoomEdNum));
        player.X = drop.X; player.Y = drop.Y;
        sim.Tick();
        Assert.Equal(before + expected, player.Inventory.Ammo(ammo));
        Assert.True(drop.Destroyed);
    }

    [Theory]
    [InlineData(PickupCatalog.Clip, 0, 5)]
    [InlineData(PickupCatalog.Rocket, 0, 1)]
    [InlineData(PickupCatalog.Shotgun, 99, 4)]
    [InlineData(PickupCatalog.Clip, 7, 7)]
    public void DropAmountIsAdjustedBeforeSkillScaling(int type, int requested, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        sim.DoubleAmmo = true;
        var player = sim.Players.Single();
        var ammo = type == PickupCatalog.Shotgun ? AmmoKind.Shells
            : type == PickupCatalog.Rocket ? AmmoKind.Rockets : AmmoKind.Bullets;
        var before = player.Inventory.Ammo(ammo);
        Assert.True(sim.SpawnDroppedPickup(player, type, pickupAmount: requested));
        sim.Tick();
        Assert.Equal(before + expected * 2, player.Inventory.Ammo(ammo));
    }
}
