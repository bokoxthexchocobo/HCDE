using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryAmmoTossTests
{
    [Theory]
    [InlineData("Clip", PickupCatalog.Clip)]
    [InlineData("Shell", PickupCatalog.Shells)]
    [InlineData("RocketAmmo", PickupCatalog.Rocket)]
    [InlineData("Cell", PickupCatalog.Cell)]
    public void DropSpawnsOneRoundWithNativeMomentumAndNoRandomDraw(string type, int editorNumber)
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, type, 5);
        player.VelocityX = Fixed.FromInt(2); player.VelocityY = Fixed.FromInt(3);
        player.VelocityZ = Fixed.FromInt(4);
        var random = sim.CombatRandomState;
        Assert.True(AcsPlayerInventory.Drop(player, type));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == editorNumber);
        Assert.Equal(1, drop.PickupAmount);
        Assert.True(drop.IgnoreAmmoSkill);
        Assert.Equal(30, drop.PickupDelay);
        Assert.Equal(10, drop.Z.ToDouble());
        Assert.Equal(7, drop.VelocityX.ToDouble());
        Assert.Equal(3, drop.VelocityY.ToDouble());
        Assert.Equal(5, drop.VelocityZ.ToDouble());
        Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void DelayPreventsCollectionUntilThirtyTicksThenReturnsOnlyOneRound()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Bullets = 10;
        Assert.True(AcsPlayerInventory.Drop(player, "Clip"));
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        drop.VelocityX = default; drop.VelocityY = default; drop.VelocityZ = default;
        drop.NoGravity = true;
        for (var i = 0; i < 29; i++) sim.Tick();
        Assert.Equal(1, drop.PickupDelay);
        Assert.Equal(9, player.Inventory.Bullets);
        sim.Tick();
        Assert.Equal(10, player.Inventory.Bullets);
        Assert.DoesNotContain(drop, sim.Actors);
    }

    [Fact]
    public void SaveRoundTripPreservesDelayAndChecksum()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Drop(player, "Clip");
        sim.Tick();
        var drop = Assert.Single(sim.Actors, a => a.DoomEdNum == PickupCatalog.Clip);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(40, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        var checksum = sim.Checksum;
        drop.PickupDelay = 0;
        sim.RestoreState(state);
        Assert.Equal(29, drop.PickupDelay);
        Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void InvalidSavedDelayIsRejected()
    {
        var sim = Room(); AcsPlayerInventory.Drop(sim.Players.Single(), "Clip");
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var start = bytes.Length - size;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(start + 12), 31);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-pickup-delay-value", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    }, compat: CompatSurface.NoTossDrops, spawnOptions: new SpawnOptions(Skill: 0));
}
