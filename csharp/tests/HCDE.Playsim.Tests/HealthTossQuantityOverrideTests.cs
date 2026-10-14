using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthTossQuantityOverrideTests
{
    public static IEnumerable<object[]> Requests()
    {
        foreach (var name in new[] { "MedPatch", "MedicalKit", "SurgeryKit", "ArtiHealth", "ArtiSuperHealth" })
            foreach (var amount in new[] { int.MinValue, 0, 3, int.MaxValue })
                yield return [name, amount];
    }

    [Theory]
    [MemberData(nameof(Requests))]
    public void HealthOverrideIgnoresQuantityAndReplaysOneItemToss(string name, int amount)
    {
        var raven = name.StartsWith("Arti", StringComparison.Ordinal);
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }],
            Things = [new LevelThing { Type = 1 }]
        }, spawnOptions: new SpawnOptions(StrifeHealthPickups: !raven, RavenHealthPickups: raven));
        var player = sim.Players.Single(); player.Health = 30;
        Assert.True(player.Inventory.GiveHealthPickup(name, 4));
        var saved = SimSavegame.Write(sim, true);
        Assert.True(ActorInventoryActions.DropInventory(player, name, amount));
        Assert.Equal(3, player.Inventory.CountHealthPickup(name));
        var drop = Assert.Single(sim.Actors, actor => actor != player);
        Assert.Equal(1, drop.PickupAmount); Assert.Equal(30, drop.PickupDelay);
        Assert.Equal(30, player.Health);
        var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.True(AcsPlayerInventory.Drop(player, name, amount));
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, expected);
        drop = Assert.Single(sim.Actors, actor => actor != player);
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 30; i++) sim.Tick();
        player.X = drop.X; player.Y = drop.Y; player.Z = drop.Z;
        sim.Tick();
        Assert.Equal(4, player.Inventory.CountHealthPickup(name));
        Assert.True(drop.Destroyed);
    }
}
