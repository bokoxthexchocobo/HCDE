using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HighJumpStrengthTests
{
    [Theory]
    [InlineData(3, 24)]
    [InlineData(0.5, 4)]
    [InlineData(0, 8)]
    [InlineData(-2, 8)]
    public void JumpUsesPositiveStrengthAndIgnoresNonpositiveFactors(double strength, int expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(10, strength: strength);
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(expected, player.Z.ToDouble());
    }

    [Fact]
    public void SameClassRefreshKeepsOriginalStrengthAndExpirationResetsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(1, strength: 3);
        player.GivePowerHighJump(2, strength: 4); Assert.Equal(3, player.PowerHighJumpStrength);
        sim.Tick(); sim.Tick(); Assert.Equal(2, player.PowerHighJumpStrength);
        player.GivePowerHighJump(10); Assert.Equal(2, player.PowerHighJumpStrength);
    }

    [Fact]
    public void TakeAndClearResetStrength()
    {
        var player = Room().Players.Single(); player.GivePowerHighJump(10, strength: 3);
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerHighJump"));
        Assert.Equal(2, player.PowerHighJumpStrength);
        player.GivePowerHighJump(strength: 4); ActorInventoryActions.ClearInventory(player);
        Assert.Equal(2, player.PowerHighJumpStrength);
    }

    [Fact]
    public void SavePreservesTransientCustomStrengthAndLegacyUsesDefault()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(10);
        var legacy = SimSavegame.Write(sim); AcsPlayerInventory.Take(player, "PowerHighJump", 1);
        player.GivePowerHighJump(strength: 3.25);
        var bytes = SimSavegame.Write(sim); Assert.Equal(177, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.True(player.PowerHighJumpPendingRemoval); Assert.Equal(3.25, player.PowerHighJumpStrength);
        SimSavegame.Apply(sim, legacy); Assert.Equal(2, player.PowerHighJumpStrength);
    }

    [Fact]
    public void NonfiniteStrengthRejectedBeforeGrantOrRestoreMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump(10, strength: 3);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerHighJump(20, strength: double.NaN));
        Assert.Equal(10, player.PowerHighJumpTics); Assert.Equal(3, player.PowerHighJumpStrength);
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - 12), double.PositiveInfinity);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        var state = sim.CaptureState(); state.Actors.Single().PowerHighJumpStrength = double.NaN;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(3, player.PowerHighJumpStrength);
    }

    [Fact]
    public void ChecksumDistinguishesStrengthsWithEqualTimers()
    {
        var first = Room(); var second = Room();
        first.Players.Single().GivePowerHighJump(10, strength: 3);
        second.Players.Single().GivePowerHighJump(10, strength: 4);
        first.RestoreState(first.CaptureState()); second.RestoreState(second.CaptureState());
        Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
