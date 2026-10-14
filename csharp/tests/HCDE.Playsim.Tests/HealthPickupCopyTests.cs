using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthPickupCopyTests
{
    [Theory]
    [InlineData("MedPatch", 17, 3)]
    [InlineData("MedicalKit", 31, 3)]
    [InlineData("SurgeryKit", -50, 0)]
    [InlineData("ArtiHealth", 41, 1)]
    [InlineData("ArtiSuperHealth", 73, 2)]
    public void TossAndCollectionPreserveCustomHealingAcrossSave(string name, int health, int mode)
    {
        var sim = Room(name); var player = sim.Players.Single();
        player.Inventory.AddHealthPickup(health, 1, mode, name);
        Assert.True(ActorInventoryActions.DropInventory(player, name));
        Assert.Empty(player.Inventory.HealthPickups);
        var drop = Assert.Single(sim.Actors, a => a != player);
        Assert.Equal(health, drop.PickupHealthOverride);
        var saved = SimSavegame.Write(sim, true);
        sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum;
        drop.PickupHealthOverride = health + 1;
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved);
        drop = Assert.Single(sim.Actors, a => a != player);
        Assert.Equal(health, drop.PickupHealthOverride);
        Assert.Equal(checksum, sim.Checksum);
        drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 30; i++) sim.Tick();
        player.X = drop.X; player.Y = drop.Y; player.Z = drop.Z;
        sim.Tick();
        var held = Assert.Single(player.Inventory.HealthPickups);
        Assert.Equal(health, held.Health); Assert.Equal(mode, held.AutoUseMode); Assert.Equal(1, held.Amount);
        player.Health = 20;
        Assert.True(ActorInventoryActions.UseInventory(player, name));
        Assert.Equal(health < 0 ? 50 : 20 + health, player.Health);
    }

    [Theory]
    [InlineData(17)]
    [InlineData(10)]
    public void HandlePickupMergesOnlyMatchingHealing(int health)
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single();
        Assert.True(player.Inventory.GiveHealthPickup("MedPatch", 2));
        Assert.True(player.Inventory.GiveHealthPickup("MedPatch", 1, health));
        Assert.Equal(health == 10 ? 1 : 2, player.Inventory.HealthPickups.Count);
        Assert.Equal(3, player.Inventory.HealthPickups.Sum(item => item.Amount));
        if (health != 10)
        {
            Assert.Equal(1, player.Inventory.HealthPickups.Single(item => item.Health == health).Amount);
            Assert.Equal(2, player.Inventory.HealthPickups.Single(item => item.Health == 10).Amount);
        }
    }

    [Fact]
    public void OlderSaveClearsOverrideAndMalformedTrailerRejects()
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single();
        player.Inventory.GiveHealthPickup("MedPatch", 1);
        Assert.True(AcsPlayerInventory.Drop(player, "MedPatch"));
        var old = SimSavegame.Write(sim, true);
        var drop = Assert.Single(sim.Actors, a => a != player);
        drop.PickupHealthOverride = 17;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(255, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        SimSavegame.Apply(sim, old);
        Assert.Null(Assert.Single(sim.Actors, a => a != player).PickupHealthOverride);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void InvalidHealingCannotBeSavedOrGranted()
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single();
        Assert.Throws<ArgumentOutOfRangeException>(() => player.Inventory.GiveHealthPickup("MedPatch", 1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => player.Inventory.GiveHealthPickup("MedPatch", 1, -50));
        player.PickupHealthOverride = 0;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(sim, true));
        Assert.Empty(player.Inventory.HealthPickups);
    }

    [Theory]
    [InlineData("MedPatch")]
    [InlineData("ArtiHealth")]
    public void LastItemTossPreservesManualOnlyOverride(string name)
    {
        var sim = Room(name); var player = sim.Players.Single();
        player.Inventory.AddHealthPickup(17, 1, 0, name);
        Assert.True(AcsPlayerInventory.Drop(player, name));
        Assert.Empty(player.Inventory.HealthPickups);
        Assert.Equal(0, Assert.Single(sim.Actors, a => a != player).PickupAutoUseOverride);
    }

    private static AuthoritySimulation Room(string name)
    {
        var raven = name.StartsWith("Arti", StringComparison.Ordinal);
        return AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
        }, spawnOptions: new SpawnOptions(StrifeHealthPickups: !raven, RavenHealthPickups: raven));
    }
}
