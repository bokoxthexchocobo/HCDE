using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SignedAmmoCapacitySaveTests
{
    [Theory]
    [InlineData("Clip")]
    [InlineData("Shell")]
    [InlineData("RocketAmmo")]
    [InlineData("Cell")]
    public void SignedCapacityRoundTripsWithoutReducingAmmo(string typeName)
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorInventoryActions.SetInventory(player, typeName, 10);
        ActorInventoryActions.SetAmmoCapacity(player, typeName, int.MinValue);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(170, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        ActorInventoryActions.SetAmmoCapacity(player, typeName, 100);
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(int.MinValue, ActorInventoryActions.GetAmmoCapacity(player, typeName));
        Assert.True(ActorInventoryActions.CheckInventory(player, typeName, 10));
        Assert.False(ActorInventoryActions.GiveAmmo(player, typeName, 1));
        var state = sim.CaptureState();
        ActorInventoryActions.SetAmmoCapacity(player, typeName, 100);
        sim.RestoreState(state);
        Assert.Equal(int.MinValue, ActorInventoryActions.GetAmmoCapacity(player, typeName));
    }

    [Fact]
    public void SignedCapacitiesCannotBeSmuggledIntoOldVersion()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorInventoryActions.SetAmmoCapacity(player, "Clip", -7);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(4), 169);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        ActorInventoryActions.SetAmmoCapacity(player, "Clip", 123);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(123, player.Inventory.MaxBullets);
    }

    [Fact]
    public void NegativeHeldAmmoStillFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single();
        ActorInventoryActions.SetAmmoCapacity(player, "Clip", -7);
        var bytes = SimSavegame.Write(sim);
        // The last entry is the nine-int ammo payload followed by trailer length.
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 40), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(-7, player.Inventory.MaxBullets);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
