using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerWeaponSaveTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(8)]
    public void OwnershipSelectionAndPendingRestoreExactly(int pending)
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory;
        inventory.Weapons |= WeaponKind.Shotgun | WeaponKind.Chainsaw;
        inventory.Selected = WeaponKind.Chainsaw; inventory.Pending = pending == -1 ? null : (WeaponKind)pending;
        inventory.Shells = 23;
        var saved = SimSavegame.Write(sim); Assert.Equal(166, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(WeaponKind.Fist | WeaponKind.Pistol | WeaponKind.Shotgun | WeaponKind.Chainsaw, inventory.Weapons);
        Assert.Equal(WeaponKind.Chainsaw, inventory.Selected); Assert.Equal(pending == -1 ? null : (WeaponKind?)pending, inventory.Pending);
        Assert.Equal(23, inventory.Shells);
    }

    [Fact]
    public void OmittedDefaultWeaponsClearLaterOwnershipAndPendingSelection()
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory; var saved = SimSavegame.Write(sim);
        inventory.Weapons |= WeaponKind.Bfg; inventory.Selected = WeaponKind.Bfg; inventory.Pending = WeaponKind.Fist;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(WeaponKind.Fist | WeaponKind.Pistol, inventory.Weapons);
        Assert.Equal(WeaponKind.Pistol, inventory.Selected); Assert.Null(inventory.Pending);
    }

    [Theory]
    [InlineData(12, 512)]
    [InlineData(16, 3)]
    [InlineData(20, -2)]
    public void InvalidWeaponValuesRejectBeforeMutation(int offset, int value)
    {
        var sim = Room(); var inventory = sim.Players.Single().Inventory; inventory.Weapons |= WeaponKind.Shotgun;
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.True(inventory.Owns(WeaponKind.Shotgun));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
