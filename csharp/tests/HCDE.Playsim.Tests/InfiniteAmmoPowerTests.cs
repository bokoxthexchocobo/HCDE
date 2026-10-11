using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InfiniteAmmoPowerTests
{
    [Fact]
    public void GrantUsesNativeDurationAndDoesNotRefreshLongRunningPower()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerInfiniteAmmo", 7));
        Assert.Equal(1050, player.PowerInfiniteAmmoTics);
        player.PowerInfiniteAmmoTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerInfiniteAmmo"));
        Assert.Equal(500, player.PowerInfiniteAmmoTics);
        player.PowerInfiniteAmmoTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerInfiniteAmmo"));
        Assert.Equal(1050, player.PowerInfiniteAmmoTics);
    }

    [Fact]
    public void EffectAllowsEmptyFiringUntilExpiration()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Bullets = 0;
        player.GivePowerInfiniteAmmo(1);
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(0, player.Inventory.Bullets);
        sim.Tick(); Assert.Equal(0, player.PowerInfiniteAmmoTics);
        player.WeaponCooldown = 0;
        Assert.False(HitscanCombat.Fire(sim, player));
    }

    [Fact]
    public void InventoryActionsCheckSetTakeAndRejectDrop()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerInfiniteAmmo", 1));
        player.PowerInfiniteAmmoTics = 25;
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerInfiniteAmmo", 0));
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerInfiniteAmmo", 4));
        Assert.Equal(25, player.PowerInfiniteAmmoTics);
        Assert.False(AcsPlayerInventory.Drop(player, "PowerInfiniteAmmo"));
        Assert.False(ActorInventoryActions.TakeInventory(player, "Clip", 1, flags: 1));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerInfiniteAmmo"));
        Assert.False(player.HasInfiniteAmmo);
    }

    [Fact]
    public void ClearRemovesItemButServerSettingStillApplies()
    {
        var sim = Room(true); var player = sim.Players.Single(); player.GivePowerInfiniteAmmo();
        ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerInfiniteAmmoTics); Assert.True(player.HasInfiniteAmmo);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "PowerInfiniteAmmo", false));
    }

    [Fact]
    public void SaveRoundTripPreservesTimerAndLegacyRestoreClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim);
        player.GivePowerInfiniteAmmo(87);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(171, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.PowerInfiniteAmmoTics = 0; SimSavegame.Apply(sim, bytes);
        Assert.Equal(87, player.PowerInfiniteAmmoTics);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerInfiniteAmmoTics);
    }

    [Fact]
    public void InvalidDurationIsRejectedBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerInfiniteAmmo(87);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(87, player.PowerInfiniteAmmoTics);
        var state = sim.CaptureState(); state.Actors.Single().PowerInfiniteAmmoTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(87, player.PowerInfiniteAmmoTics);
    }

    [Fact]
    public void DefinitionDurationAndChecksumIncludeItemEffect()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.InfiniteAmmo, -3);
        Assert.Equal(105, player.PowerInfiniteAmmoTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room(bool infinite = false) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(InfiniteAmmo: infinite));
}
