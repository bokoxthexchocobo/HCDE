using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PowerStrengthTests
{
    [Theory]
    [InlineData(WeaponKind.Fist, 10)]
    [InlineData(WeaponKind.Chainsaw, 1)]
    [InlineData(WeaponKind.Pistol, 1)]
    public void StrengthMultipliesOnlyFistDamage(WeaponKind weapon, int multiplier)
    {
        var (sim, player, target) = Room(); var (control, ordinary, controlTarget) = Room();
        player.Inventory.Weapons |= weapon; ordinary.Inventory.Weapons |= weapon;
        player.Inventory.Selected = ordinary.Inventory.Selected = weapon;
        player.GivePowerStrength();
        Assert.True(HitscanCombat.Fire(control, ordinary)); Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal((1000 - controlTarget.Health) * multiplier, 1000 - target.Health);
        Assert.Equal(control.CaptureState().PunchRandomState, sim.CaptureState().PunchRandomState);
    }

    [Fact]
    public void TimerCountsUpAndRepeatedGrantResetsIt()
    {
        var (_, player, _) = Room(); player.GivePowerStrength(); Assert.Equal(1, player.PowerStrengthTics);
        for (var i = 0; i < 1100; i++) player.Tick();
        Assert.True(player.HasPowerStrength); Assert.Equal(1101, player.PowerStrengthTics);
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerStrength")); Assert.Equal(1, player.PowerStrengthTics);
    }

    [Fact]
    public void InventoryGiveCountSetAndTakeUsePowerPresence()
    {
        var (_, player, _) = Room(); Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerStrength", true));
        Assert.Equal(0, AcsPlayerInventory.Count(player, "PowerStrength", false));
        AcsPlayerInventory.Give(player, "PowerStrength", 1); player.Tick(); var timer = player.PowerStrengthTics;
        Assert.False(ActorInventoryActions.SetInventory(player, "PowerStrength", 1)); Assert.Equal(timer, player.PowerStrengthTics);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerStrength", false));
        AcsPlayerInventory.Take(player, "PowerStrength", 1); Assert.False(player.HasPowerStrength);
    }

    [Fact]
    public void StrengthClearsOnDeath()
    {
        var (_, player, _) = Room(); player.GivePowerStrength(); ActorDamage.Apply(player, 1000);
        Assert.False(player.HasPowerStrength);
    }

    [Fact]
    public void StrengthSaveReplaysPunchAndOlderSaveClearsPower()
    {
        var (sim, player, _) = Room(); player.Inventory.Selected = WeaponKind.Fist;
        var legacy = SimSavegame.Write(sim, true); player.GivePowerStrength(); player.Tick();
        var saved = SimSavegame.Write(sim, true); Assert.Equal(251, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        Assert.True(HitscanCombat.Fire(sim, player)); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Assert.Equal(2, player.PowerStrengthTics);
        Assert.True(HitscanCombat.Fire(sim, player)); Assert.Equal(expected, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, legacy); Assert.False(player.HasPowerStrength);
    }

    [Fact]
    public void MalformedStrengthTrailerRejects()
    {
        var (sim, player, _) = Room(); player.GivePowerStrength(); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), int.MaxValue);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    private static (AuthoritySimulation Sim, PlayerPawn Player, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3001, X = 48 }]
        });
        var target = sim.Actors[1]; target.Health = 1000; target.Brain!.Enabled = false;
        return (sim, sim.Players.Single(), target);
    }
}
