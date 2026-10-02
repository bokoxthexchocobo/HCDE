using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupArchiveTests
{
    private static AuthoritySimulation Room(int type = PickupCatalog.Clip) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type, X = 100 }],
    });

    [Theory]
    [InlineData(5, false, false)]
    [InlineData(17, true, false)]
    [InlineData(0, false, true)]
    [InlineData(0, false, false)]
    public void PickupPropertiesRoundTripAndContinueIdentically(int amount, bool ignoreSkill, bool depleted)
    {
        var original = Room(); var pickup = original.Actors[1];
        pickup.PickupAmount = amount; pickup.IgnoreAmmoSkill = ignoreSkill; pickup.Depleted = depleted;
        original.Tick();
        var bytes = SimSavegame.Write(original);
        Assert.Equal(17, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var restored = Room();
        restored.Actors[1].PickupAmount = 99;
        restored.Actors[1].IgnoreAmmoSkill = !ignoreSkill;
        restored.Actors[1].Depleted = !depleted;
        SimSavegame.Apply(restored, bytes);
        Assert.Equal(amount, restored.Actors[1].PickupAmount);
        Assert.Equal(ignoreSkill, restored.Actors[1].IgnoreAmmoSkill);
        Assert.Equal(depleted, restored.Actors[1].Depleted);
        Assert.Equal(original.Checksum, restored.Checksum);
        original.DoubleAmmo = restored.DoubleAmmo = true;
        original.Players.Single().X = restored.Players.Single().X = pickup.X;
        original.Tick(); restored.Tick();
        Assert.Equal(original.Players.Single().Inventory.Bullets, restored.Players.Single().Inventory.Bullets);
        Assert.Equal(original.Checksum, restored.Checksum);
    }

    [Fact]
    public void DepletedBackpackStillRaisesCapsWithoutGivingAmmoAfterRestore()
    {
        var original = Room(PickupCatalog.Backpack);
        original.Actors[1].Depleted = true;
        var restored = Room(PickupCatalog.Backpack);
        SimSavegame.Apply(restored, SimSavegame.Write(original));
        var player = restored.Players.Single(); var before = player.Inventory.Bullets;
        player.X = restored.Actors[1].X; restored.Tick();
        Assert.True(player.Inventory.HasBackpack);
        Assert.Equal(before, player.Inventory.Bullets);
    }

    [Fact]
    public void LegacyArchivePreservesExistingPickupProperties()
    {
        var sim = Room(); var state = sim.CaptureState();
        foreach (var pose in state.Actors) pose.Pickup = null;
        var bytes = SimSavegame.Write(state);
        Assert.Equal(16, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Actors[1].PickupAmount = 19; sim.Actors[1].IgnoreAmmoSkill = true;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(19, sim.Actors[1].PickupAmount);
        Assert.True(sim.Actors[1].IgnoreAmmoSkill);
    }

    [Theory]
    [InlineData(-1, 4)]
    [InlineData(5, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 12)]
    public void InvalidPickupFieldsAreRejected(int amount, int flags)
    {
        var bytes = SimSavegame.Write(Room());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 12), amount);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), flags);
        Assert.False(SimSavegame.TryRead(bytes, out var state, out var error));
        Assert.Equal("save-pickup-properties", error); Assert.Empty(state.Actors);
    }

    [Fact]
    public void TruncatedPickupArchiveIsRejected()
    {
        var bytes = SimSavegame.Write(Room());
        for (var length = 0; length < bytes.Length; length++)
            Assert.False(SimSavegame.TryRead(bytes.AsSpan(0, length), out _, out _));
    }

    [Fact]
    public void DroppedWeaponAmmoSurvivesRestoreOntoMatchingActors()
    {
        var original = Room(); var restored = Room();
        Assert.True(original.SpawnDroppedPickup(original.Players.Single(), PickupCatalog.Shotgun));
        Assert.True(restored.SpawnDroppedPickup(restored.Players.Single(), PickupCatalog.Shotgun));
        restored.Actors[^1].PickupAmount = 80;
        SimSavegame.Apply(restored, SimSavegame.Write(original));
        restored.Tick();
        Assert.Equal(4, restored.Players.Single().Inventory.Shells);
    }

    [Theory]
    [InlineData(-1, "save-pickup-size")]
    [InlineData(9, "save-pickup-size")]
    [InlineData(16, "save-pickup-count")]
    public void InvalidTrailerSizeOrCountIsRejected(int size, string expected)
    {
        var bytes = SimSavegame.Write(Room());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), size);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal(expected, error);
    }

    [Fact]
    public void NegativePickupAmountFailsBeforeRestoreMutation()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.Actors[1].Pickup = new(-1, false, false);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, sim.Actors[1].PickupAmount);
    }
}
