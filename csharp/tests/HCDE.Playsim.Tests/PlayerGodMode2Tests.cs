using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerGodMode2Tests
{
    [Theory]
    [InlineData(20, DamageFlags.None)]
    [InlineData(20, DamageFlags.Forced)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.None)]
    [InlineData(ActorDamage.TelefragDamage, DamageFlags.Forced)]
    public void UltimateGodModeProtectsHealthAndArmor(int damage, DamageFlags flags)
    {
        var player = new PlayerPawn { GodMode2 = true, Health = 100, PainChance = 256 };
        player.Inventory.Armor = 50; player.Inventory.ArmorSavePercent = 50;
        var before = player.States.Current;
        Assert.Equal(default, ActorDamage.Apply(player, damage, flags: flags));
        Assert.Equal(100, player.Health); Assert.Equal(50, player.Inventory.Armor);
        Assert.Equal(before, player.States.Current); Assert.Null(player.LastDamageSourceId);
    }

    [Fact]
    public void OrdinaryGodModeStillAllowsTelefrag()
    {
        var player = new PlayerPawn { GodMode = true, Health = 100 };
        Assert.True(ActorDamage.Apply(player, ActorDamage.TelefragDamage).Killed);
    }

    [Fact]
    public void SaveSnapshotAndLegacyRestorePreserveOrResetUltimateGodMode()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single();
        var legacy = SimSavegame.Write(sim);
        player.GodMode2 = true;
        var snapshot = sim.CaptureState();
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(154, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.GodMode2 = false; sim.RestoreState(snapshot);
        Assert.True(player.GodMode2);
        player.GodMode2 = false; SimSavegame.Apply(sim, bytes);
        Assert.True(player.GodMode2); Assert.Equal(bytes, SimSavegame.Write(sim));
        var invalid = (byte[])bytes.Clone();
        var size = BinaryPrimitives.ReadInt32LittleEndian(invalid.AsSpan(invalid.Length - 4));
        invalid[invalid.Length - size + 8] = 2;
        Assert.False(SimSavegame.TryRead(invalid, out _, out var error));
        Assert.Equal("save-player-god-mode2-invalid", error);
        SimSavegame.Apply(sim, legacy); Assert.False(player.GodMode2);
    }
}
