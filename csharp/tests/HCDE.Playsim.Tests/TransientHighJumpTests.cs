using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TransientHighJumpTests
{
    [Fact]
    public void DefaultGrantHasInventoryPresenceWithZeroRemainingTics()
    {
        var sim = Room(); var player = sim.Players.Single();
        AcsPlayerInventory.Give(player, "PowerHighJump", 1);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "PowerHighJump", false));
        Assert.Equal(0, AcsActorPowerups.RemainingTics(player, "PowerHighJump"));
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerHighJump", 0));
        sim.Tick(); Assert.False(player.HasPowerHighJump);
    }

    [Fact]
    public void TransientPowerAffectsOwnerJumpThenIsRemoved()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump();
        sim.QueueCommand(0, new PlayerCommand { Jump = true }); sim.Tick();
        Assert.Equal(16, player.Z.ToDouble()); Assert.False(player.HasPowerHighJump);
    }

    [Fact]
    public void RemovalAndClearDiscardTransientPresence()
    {
        var player = Room().Players.Single(); player.GivePowerHighJump();
        Assert.False(AcsPlayerInventory.Drop(player, "PowerHighJump"));
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerHighJump"));
        Assert.False(player.HasPowerHighJump);
        player.GivePowerHighJump(); ActorInventoryActions.ClearInventory(player);
        Assert.False(player.HasPowerHighJump);
    }

    [Fact]
    public void TimedGrantReplacesTransientRemovalAndZeroGrantKeepsLongTimer()
    {
        var player = Room().Players.Single(); player.GivePowerHighJump(); player.GivePowerHighJump(500);
        Assert.False(player.PowerHighJumpPendingRemoval);
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerHighJump"));
        Assert.Equal(500, player.PowerHighJumpTics); Assert.False(player.PowerHighJumpPendingRemoval);
        Assert.Throws<ArgumentOutOfRangeException>(() => player.GivePowerHighJump(-1));
        Assert.Equal(500, player.PowerHighJumpTics);
    }

    [Fact]
    public void SaveRestoresPresenceAndLegacyRestoreClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerDoubleFiringSpeed(90);
        var legacy = SimSavegame.Write(sim); sim.RestoreState(sim.CaptureState()); var before = sim.Checksum;
        player.GivePowerHighJump(); sim.RestoreState(sim.CaptureState()); Assert.NotEqual(before, sim.Checksum);
        var bytes = SimSavegame.Write(sim); Assert.Equal(176, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.True(player.PowerHighJumpPendingRemoval); Assert.Equal(0, player.PowerHighJumpTics);
        Assert.Equal(90, player.PowerDoubleFiringSpeedTics);
        SimSavegame.Apply(sim, legacy); Assert.False(player.HasPowerHighJump);
    }

    [Fact]
    public void InvalidPresenceFlagFailsBeforeMutation()
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerHighJump();
        var bytes = SimSavegame.Write(sim); BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(player.PowerHighJumpPendingRemoval);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] });
}
