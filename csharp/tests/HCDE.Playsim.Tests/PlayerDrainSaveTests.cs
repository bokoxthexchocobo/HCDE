using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PlayerDrainSaveTests
{
    [Theory]
    [InlineData(0.5, 54)]
    [InlineData(0.125, 51)]
    [InlineData(2, 66)]
    public void SavedStrengthRestoresExactValueAndHealingContinuation(double strength, int health)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Health = 50; player.DrainStrength = strength;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(153, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        player.DrainStrength = 7; SimSavegame.Apply(sim, bytes);
        Assert.Equal(strength, player.DrainStrength); Assert.Equal(bytes, SimSavegame.Write(sim));
        Assert.Empty(sim.DrainSoundRequests());
        ActorDamage.Apply(new Actor { Health = 100, NoPain = true }, 8, player);
        Assert.Equal(health, player.Health); Assert.Equal("*drainhealth", Assert.Single(sim.DrainSoundRequests()).Sound);
    }

    [Fact]
    public void LegacyAndSnapshotRestoreClearOrPreserveStrength()
    {
        var sim = Room(); var player = sim.Players.Single(); var legacy = SimSavegame.Write(sim);
        player.DrainStrength = 0.5; var snapshot = sim.CaptureState();
        player.DrainStrength = 2; sim.RestoreState(snapshot); Assert.Equal(0.5, player.DrainStrength);
        SimSavegame.Apply(sim, legacy); Assert.Equal(0, player.DrainStrength);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void NonfiniteArchiveAndSnapshotStrengthAreRejected(double invalid)
    {
        var sim = Room(); var player = sim.Players.Single(); player.DrainStrength = 0.5;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteDoubleLittleEndian(bytes.AsSpan(bytes.Length - size + 8), invalid);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-player-drain-invalid", error);
        var snapshot = sim.CaptureState(); snapshot.Actors[0].PlayerDrainStrength = invalid;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(snapshot));
        Assert.Equal(0.5, player.DrainStrength);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
