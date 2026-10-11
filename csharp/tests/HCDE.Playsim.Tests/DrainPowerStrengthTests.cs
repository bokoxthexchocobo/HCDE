using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DrainPowerStrengthTests
{
    [Theory]
    [InlineData(0.75, 57)]
    [InlineData(1.5, 65)]
    [InlineData(0, 50)]
    [InlineData(-1, 50)]
    public void StrengthScalesPostArmorDamageAndTruncates(double strength, int expected)
    {
        var player = Room().Players.Single(); player.Health = 50; player.GivePowerDrain(strength: strength);
        ActorDamage.Apply(new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50, NoPain = true }, 20, player);
        Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void RefreshPreservesStrengthAndExpirationReturnsToDirectSetting()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50; player.DrainStrength = 0.25;
        player.GivePowerDrain(1, strength: 0.75); player.GivePowerDrain(2, strength: 1);
        Assert.Equal(0.75, player.PowerDrainStrength);
        sim.Tick(); sim.Tick(); Assert.Equal(0.5, player.PowerDrainStrength);
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 20, player);
        Assert.Equal(55, player.Health);
    }

    [Fact]
    public void TakeAndClearResetItemStrength()
    {
        var player = Room().Players.Single(); player.GivePowerDrain(strength: 0.75);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerDrain")); Assert.Equal(0.5, player.PowerDrainStrength);
        player.GivePowerDrain(strength: 1); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(0.5, player.PowerDrainStrength);
    }

    [Fact]
    public void SavePreservesBothCustomStrengthsAndLegacyDefaultsDrain()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(99, strength: 3);
        player.GivePowerDrain(80); var legacy = SimSavegame.Write(sim);
        AcsPlayerInventory.Take(player, "PowerDrain", 1); player.GivePowerDrain(80, strength: 0.75);
        var bytes = SimSavegame.Write(sim); Assert.Equal(178, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(0.75, player.PowerDrainStrength); Assert.Equal(80, player.PowerDrainTics);
        Assert.Equal(3, player.PowerHighJumpStrength); Assert.Equal(99, player.PowerHighJumpTics);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0.5, player.PowerDrainStrength);
    }

    [Fact]
    public void InvalidStrengthFailsBeforeMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDrain(80, strength: 0.75);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerDrain(100, strength: double.NaN));
        Assert.Equal(80, player.PowerDrainTics);
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - 12), double.PositiveInfinity);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        var state = sim.CaptureState(); state.Actors.Single().PowerDrainStrength = double.NaN;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0.75, player.PowerDrainStrength);
    }

    [Fact]
    public void ChecksumIncludesStrengthWithEqualDuration()
    {
        var first = Room(); var second = Room(); first.Players.Single().GivePowerDrain(80, strength: 0.75);
        second.Players.Single().GivePowerDrain(80, strength: 1);
        first.RestoreState(first.CaptureState()); second.RestoreState(second.CaptureState());
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
