using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerTurnSaveTests
{
    [Fact]
    public void RestoredHeldTurnContinuesWithoutRestartingCountdown()
    {
        var sim = Room(); sim.QueueCommand(0, new PlayerCommand { Turn180 = true }); sim.Tick();
        var player = sim.Players.Single(); var ticks = player.TurnTicks; var angle = player.Angle;
        var saved = SimSavegame.Write(sim); Assert.Equal(169, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        player.RestoreTurn(0, false); SimSavegame.Apply(sim, saved);
        Assert.Equal(ticks, player.TurnTicks); Assert.True(player.TurnHeld);
        sim.QueueCommand(0, new PlayerCommand { Turn180 = true, YawDelta = 16384 }); sim.Tick();
        Assert.Equal(ticks - 1, player.TurnTicks);
        Assert.InRange(player.Angle.ToDegrees() - angle.ToDegrees(), 19.99, 20.01);
    }

    [Fact]
    public void DefaultSaveClearsLaterTurnCountdownAndLatch()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim); var player = sim.Players.Single();
        player.RestoreTurn(5, true); SimSavegame.Apply(sim, saved);
        Assert.Equal(0, player.TurnTicks); Assert.False(player.TurnHeld);
    }

    [Theory]
    [InlineData(68, -1)]
    [InlineData(68, 10)]
    [InlineData(72, 2)]
    public void InvalidTurnStateRejectsBeforeMutation(int offset, int value)
    {
        var sim = Room(); var player = sim.Players.Single(); player.RestoreTurn(5, true);
        var saved = SimSavegame.Write(sim); var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - size + offset), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved)); Assert.Equal(5, player.TurnTicks);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
