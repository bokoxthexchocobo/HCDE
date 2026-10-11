using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PowerDrainItemTests
{
    [Fact]
    public void GrantUsesNativeDurationAndRefreshRules()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerDrain", 7));
        Assert.Equal(2100, player.PowerDrainTics);
        player.PowerDrainTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerDrain"));
        Assert.Equal(500, player.PowerDrainTics);
        player.PowerDrainTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerDrain"));
        Assert.Equal(2100, AcsActorPowerups.RemainingTics(player, "PowerDrain"));
    }

    [Fact]
    public void DefaultStrengthUsesPostArmorDamageAndExpires()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50;
        player.GivePowerDrain(1);
        ActorDamage.Apply(new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50, NoPain = true }, 20, player);
        Assert.Equal(55, player.Health);
        sim.Tick(); Assert.Equal(0, player.PowerDrainTics);
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 20, player);
        Assert.Equal(55, player.Health);
    }

    [Fact]
    public void ExpirationPreservesDirectDrainStrength()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50; player.DrainStrength = 0.25;
        player.GivePowerDrain(1);
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 20, player);
        Assert.Equal(60, player.Health);
        sim.Tick();
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 20, player);
        Assert.Equal(65, player.Health); Assert.Equal(0.25, player.DrainStrength);
    }

    [Fact]
    public void InventoryOperationsRemoveUntossablePower()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerDrain", 1));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerDrain", false));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerDrain", 0));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerDrain"));
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerDrain"));
        Assert.Equal(0, player.PowerDrainTics);
        player.GivePowerDrain(); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerDrainTics);
    }

    [Fact]
    public void SavePreservesBothTimersAndSignedCapacityAndLegacyClearsDrain()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerInfiniteAmmo(87);
        var legacy = SimSavegame.Write(sim);
        player.GivePowerDrain(93); player.Inventory.MaxBullets = -7;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(172, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.PowerDrainTics = 0; player.PowerInfiniteAmmoTics = 0;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(93, player.PowerDrainTics); Assert.Equal(87, player.PowerInfiniteAmmoTics);
        Assert.Equal(-7, player.Inventory.MaxBullets);
        SimSavegame.Apply(sim, legacy);
        Assert.Equal(0, player.PowerDrainTics); Assert.Equal(87, player.PowerInfiniteAmmoTics);
    }

    [Fact]
    public void InvalidDurationIsRejectedBeforeMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDrain(93);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(93, player.PowerDrainTics);
        var state = sim.CaptureState(); state.Actors.Single().PowerDrainTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(93, player.PowerDrainTics);
    }

    [Fact]
    public void DefinitionDurationAndChecksumIncludeDrain()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.Drain, -3);
        Assert.Equal(105, player.PowerDrainTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
