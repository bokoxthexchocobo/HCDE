using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerWeaponPhaseSaveTests
{
    [Theory]
    [InlineData(false, 86, 2)]
    [InlineData(true, 32, 8)]
    public void RestoredLowerContinuesOnNextTick(bool instant, int nextHeight, int nextWeapon)
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun; player.Inventory.Pending = WeaponKind.Shotgun;
        player.WeaponOffsetY = 80; player.WeaponLowering = true; player.InstantWeaponSwitch = instant;
        player.AttackPressed = true; player.WeaponCooldown = 10;
        var saved = SimSavegame.Write(sim); Assert.Equal(167, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.WeaponOffsetY = 32; player.WeaponLowering = false; player.InstantWeaponSwitch = false;
        player.AttackPressed = false; player.WeaponCooldown = 0; player.Inventory.Pending = null;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(80, player.WeaponOffsetY); Assert.True(player.WeaponLowering);
        Assert.Equal(instant, player.InstantWeaponSwitch); Assert.True(player.AttackPressed); Assert.Equal(10, player.WeaponCooldown);
        sim.Tick(); Assert.Equal(nextHeight, player.WeaponOffsetY); Assert.Equal((WeaponKind)nextWeapon, player.Inventory.Selected);
        Assert.Equal(9, player.WeaponCooldown);
    }

    [Fact]
    public void WeaponAttackLatchChangesChecksumAndRestoresFromSave()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.RestoreState(sim.CaptureState()); var idle = sim.Checksum;
        player.AttackPressed = true; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(idle, sim.Checksum);
        var active = sim.Checksum; var saved = SimSavegame.Write(sim);
        player.AttackPressed = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.AttackPressed); Assert.Equal(active, sim.Checksum);
    }

    [Fact]
    public void PendingNoWeaponAndNoChangeHaveDistinctChecksumsAndRestoreExactly()
    {
        var sim = Room(); var player = sim.Players.Single(); sim.RestoreState(sim.CaptureState()); var noChange = sim.Checksum;
        player.Inventory.Pending = (WeaponKind)0; sim.RestoreState(sim.CaptureState()); Assert.NotEqual(noChange, sim.Checksum);
        var noWeapon = sim.Checksum; var saved = SimSavegame.Write(sim);
        player.Inventory.Pending = null; SimSavegame.Apply(sim, saved);
        Assert.Equal((WeaponKind)0, player.Inventory.Pending); Assert.Equal(noWeapon, sim.Checksum);
    }

    [Fact]
    public void DefaultSaveRestoresReadyWeaponPhase()
    {
        var sim = Room(); var player = sim.Players.Single(); var saved = SimSavegame.Write(sim);
        player.WeaponOffsetY = 80; player.WeaponLowering = true; player.InstantWeaponSwitch = true; player.AttackPressed = true;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(32, player.WeaponOffsetY); Assert.False(player.WeaponLowering);
        Assert.False(player.InstantWeaponSwitch); Assert.False(player.AttackPressed); Assert.True(player.WeaponReady);
    }

    [Theory]
    [InlineData(24, 129)]
    [InlineData(28, 8)]
    public void InvalidWeaponPhaseRejectsBeforeMutation(int offset, int value)
    {
        var sim = Room(); var player = sim.Players.Single(); player.WeaponOffsetY = 80;
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.Equal(80, player.WeaponOffsetY);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
