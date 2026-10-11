using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class RegenerationStrengthTests
{
    [Theory]
    [InlineData(7.9, 20, 27)]
    [InlineData(0.9, 20, 20)]
    [InlineData(-50.9, 20, 50)]
    [InlineData(-50, 60, 60)]
    public void PulseUsesIntegerGiveBodySemantics(double strength, int health, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = health;
        player.GivePowerRegeneration(strength: strength); sim.Tick();
        Assert.Equal(expected, player.Health);
        Assert.Equal(expected > health ? 1 : 0, sim.DrainSoundRequests().Count(sound => sound.Sound == "*regenerate"));
    }

    [Fact]
    public void RefreshKeepsStrengthAndExpirationResetsDefault()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerRegeneration(1, strength: 9);
        player.GivePowerRegeneration(2, strength: 20); Assert.Equal(9, player.PowerRegenerationStrength);
        sim.Tick(); sim.Tick(); Assert.Equal(5, player.PowerRegenerationStrength);
    }

    [Fact]
    public void TakeAndClearResetStrength()
    {
        var player = Room().Players.Single(); player.GivePowerRegeneration(strength: 9);
        ActorInventoryActions.TakeInventory(player, "PowerRegeneration"); Assert.Equal(5, player.PowerRegenerationStrength);
        player.GivePowerRegeneration(strength: 8); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(5, player.PowerRegenerationStrength);
    }

    [Fact]
    public void SavePreservesAllCustomStrengthsAndLegacyUsesDefault()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(90, strength: 3);
        player.GivePowerDrain(80, strength: 0.75); player.GivePowerRegeneration(70);
        var legacy = SimSavegame.Write(sim); ActorInventoryActions.TakeInventory(player, "PowerRegeneration");
        player.GivePowerRegeneration(70, strength: 7.9);
        var bytes = SimSavegame.Write(sim); Assert.Equal(179, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(7.9, player.PowerRegenerationStrength); Assert.Equal(70, player.PowerRegenerationTics);
        Assert.Equal(3, player.PowerHighJumpStrength); Assert.Equal(0.75, player.PowerDrainStrength);
        SimSavegame.Apply(sim, legacy); Assert.Equal(5, player.PowerRegenerationStrength);
    }

    [Fact]
    public void InvalidStrengthFailsBeforeGrantOrRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerRegeneration(70, strength: 9);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerRegeneration(strength: double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerRegeneration(strength: (double)int.MaxValue + 1));
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - 12), double.PositiveInfinity);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        var state = sim.CaptureState(); state.Actors.Single().PowerRegenerationStrength = double.NaN;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(9, player.PowerRegenerationStrength); Assert.Equal(70, player.PowerRegenerationTics);
    }

    [Fact]
    public void EqualDurationDifferentStrengthChangesChecksum()
    {
        var first = Room(); var second = Room(); first.Players.Single().GivePowerRegeneration(70, strength: 8);
        second.Players.Single().GivePowerRegeneration(70, strength: 9);
        first.RestoreState(first.CaptureState()); second.RestoreState(second.CaptureState());
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
