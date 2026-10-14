using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealthPickupAutoUseCopyTests
{
    public static IEnumerable<object[]> Tosses()
    {
        foreach (var name in new[] { "MedPatch", "MedicalKit", "SurgeryKit", "ArtiHealth", "ArtiSuperHealth" })
        {
            var defaultMode = name switch { "SurgeryKit" => 0, "ArtiHealth" => 1, "ArtiSuperHealth" => 2, _ => 3 };
            foreach (var count in new[] { 1, 2 })
                yield return [name, count, defaultMode, defaultMode == 0 ? 3 : 0];
        }
    }

    [Theory]
    [MemberData(nameof(Tosses))]
    public void LastItemRetainsModeButSplitCopyUsesClassDefault(string name, int count, int defaultMode, int customMode)
    {
        var sim = Room(name); var player = sim.Players.Single();
        player.Inventory.AddHealthPickup(17, count, customMode, name);
        Assert.True(AcsPlayerInventory.Drop(player, name, 100));
        var expectedMode = count == 1 ? customMode : defaultMode;
        var drop = Assert.Single(sim.Actors, a => a != player);
        Assert.Equal(count == 1 ? customMode : (int?)null, drop.PickupAutoUseOverride);
        if (count == 2) Assert.Equal(customMode, Assert.Single(player.Inventory.HealthPickups).AutoUseMode);
        player.Inventory.TakeHealthPickup(name, int.MaxValue);
        var saved = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        drop = Assert.Single(sim.Actors, a => a != player);
        drop.NoGravity = true; drop.VelocityX = drop.VelocityY = drop.VelocityZ = default;
        for (var i = 0; i < 30; i++) sim.Tick();
        player.X = drop.X; player.Y = drop.Y; player.Z = drop.Z;
        sim.Tick();
        var item = Assert.Single(player.Inventory.HealthPickups);
        Assert.Equal(expectedMode, item.AutoUseMode); Assert.Equal(17, item.Health); Assert.Equal(1, item.Amount);
        if (expectedMode == 3)
        {
            player.Health = 40; ActorDamage.Apply(player, 1);
            Assert.Equal(56, player.Health); Assert.Empty(player.Inventory.HealthPickups);
        }
    }

    [Theory]
    [InlineData(0, 3)]
    [InlineData(3, 0)]
    public void MatchingHeldStackKeepsItsModeWhenHandlingPickup(int heldMode, int incomingMode)
    {
        var player = Room("MedPatch").Players.Single();
        player.Inventory.AddHealthPickup(17, 2, heldMode, "MedPatch");
        Assert.True(player.Inventory.GiveHealthPickup("MedPatch", 1, 17, incomingMode));
        var item = Assert.Single(player.Inventory.HealthPickups);
        Assert.Equal(heldMode, item.AutoUseMode); Assert.Equal(3, item.Amount);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(4)]
    public void InvalidSavedModeRejectsBeforeRestoreMutation(int mode)
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single();
        player.PickupAutoUseOverride = 0;
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(256, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + 8), mode);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        var state = sim.CaptureState(); state.Actors[0].PickupAutoUseOverride = mode;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, player.PickupAutoUseOverride);
    }

    [Fact]
    public void OldSaveClearsModeAndModeChangesChecksum()
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single(); var old = SimSavegame.Write(sim, true);
        player.PickupAutoUseOverride = 0; sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var saved = SimSavegame.Write(sim, true);
        player.PickupAutoUseOverride = 1; sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(checksum, sim.Checksum);
        SimSavegame.Apply(sim, saved); Assert.Equal(checksum, sim.Checksum);
        SimSavegame.Apply(sim, old); Assert.Null(player.PickupAutoUseOverride);
    }

    [Fact]
    public void InvalidGrantModeAndUnsupportedNegativeSplitPreserveInventory()
    {
        var sim = Room("MedPatch"); var player = sim.Players.Single();
        Assert.Throws<ArgumentOutOfRangeException>(() => player.Inventory.GiveHealthPickup("MedPatch", 1, 17, 4));
        player.Inventory.AddHealthPickup(-50, 2, 0, "MedPatch");
        Assert.Throws<NotSupportedException>(() => AcsPlayerInventory.Drop(player, "MedPatch"));
        Assert.Equal(2, Assert.Single(player.Inventory.HealthPickups).Amount); Assert.Single(sim.Actors);
    }

    private static AuthoritySimulation Room(string name)
    {
        var raven = name.StartsWith("Arti", StringComparison.Ordinal);
        return AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 512 }], Things = [new LevelThing { Type = 1 }]
        }, spawnOptions: new SpawnOptions(StrifeHealthPickups: !raven, RavenHealthPickups: raven));
    }
}
