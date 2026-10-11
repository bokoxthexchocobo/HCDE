using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoubleFiringSpeedPowerTests
{
    [Fact]
    public void NativeDurationAndRefreshRulesApply()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerDoubleFiringSpeed", 7));
        Assert.Equal(1400, player.PowerDoubleFiringSpeedTics);
        player.PowerDoubleFiringSpeedTics = 500;
        Assert.False(ActorInventoryActions.GiveInventory(player, "PowerDoubleFiringSpeed"));
        Assert.Equal(500, player.PowerDoubleFiringSpeedTics);
        player.PowerDoubleFiringSpeedTics = 100;
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerDoubleFiringSpeed"));
        Assert.Equal(1400, AcsActorPowerups.RemainingTics(player, "PowerDoubleFiringSpeed"));
    }

    [Fact]
    public void PistolRefiresAfterTenTicksWithoutChangingAmmoCost()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDoubleFiringSpeed();
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(19, player.WeaponCooldown);
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(1, player.WeaponCooldown); Assert.False(HitscanCombat.Fire(sim, player));
        sim.Tick(); Assert.Equal(0, player.WeaponCooldown);
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(48, player.Inventory.Bullets);
    }

    [Fact]
    public void PickupAndExpirationChangeAnExistingCooldown()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(HitscanCombat.Fire(sim, player));
        sim.Tick(); Assert.Equal(18, player.WeaponCooldown);
        player.GivePowerDoubleFiringSpeed(1);
        sim.Tick(); Assert.Equal(16, player.WeaponCooldown);
        Assert.Equal(0, player.PowerDoubleFiringSpeedTics);
        sim.Tick(); Assert.Equal(15, player.WeaponCooldown);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    public void CooldownNeverUnderflows(int initial, int expected)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.WeaponCooldown = initial; player.GivePowerDoubleFiringSpeed();
        sim.Tick(); Assert.Equal(expected, player.WeaponCooldown);
    }

    [Fact]
    public void InventoryTakeAndClearRemoveUntossablePower()
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerDoubleFiringSpeed", 1));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerDoubleFiringSpeed", 0));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerDoubleFiringSpeed", false));
        Assert.False(AcsPlayerInventory.Drop(player, "PowerDoubleFiringSpeed"));
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerDoubleFiringSpeed"));
        Assert.Equal(0, player.PowerDoubleFiringSpeedTics);
        player.GivePowerDoubleFiringSpeed(); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0, player.PowerDoubleFiringSpeedTics);
    }

    [Fact]
    public void SavePreservesAllFiniteTimersAndCooldownAndLegacyClearsSpeed()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.GivePowerDrain(90); player.GivePowerInfiniteAmmo(80); player.GivePowerRegeneration(70);
        var legacy = SimSavegame.Write(sim);
        player.GivePowerDoubleFiringSpeed(100); Assert.True(HitscanCombat.Fire(sim, player));
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(174, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(100, player.PowerDoubleFiringSpeedTics); Assert.Equal(19, player.WeaponCooldown);
        Assert.Equal(90, player.PowerDrainTics); Assert.Equal(80, player.PowerInfiniteAmmoTics);
        Assert.Equal(70, player.PowerRegenerationTics);
        sim.Tick(); Assert.Equal(17, player.WeaponCooldown);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.PowerDoubleFiringSpeedTics);
    }

    [Fact]
    public void InvalidDurationFailsBeforeRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDoubleFiringSpeed(100);
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), -1);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.Equal(100, player.PowerDoubleFiringSpeedTics);
        var state = sim.CaptureState(); state.Actors.Single().PowerDoubleFiringSpeedTics = -1;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(100, player.PowerDoubleFiringSpeedTics);
    }

    [Fact]
    public void DefinitionUnitsAndChecksumIncludePower()
    {
        var sim = Room(); var player = sim.Players.Single(); var before = sim.Checksum;
        player.GivePowerup(ManagedPowerupKind.DoubleFiringSpeed, -3);
        Assert.Equal(105, player.PowerDoubleFiringSpeedTics);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
