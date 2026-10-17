using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CrashedFlagTests
{
    [Fact]
    public void FreezeSetsCrashedAndSaveRestoresIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.False(player.Crashed);
        ActorFreezeActions.FreezeDeath(player);
        Assert.True(player.Crashed);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(311, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.True(restored.Players.Single().Crashed);
        Assert.Equal(saved, SimSavegame.Write(restored, true));
    }

    [Fact]
    public void AcsFlagChangesChecksumAndOlderSaveClearsIt()
    {
        var sim = Room(); var player = sim.Players.Single();
        sim.RestoreState(sim.CaptureState());
        var baseline = sim.Checksum; var older = SimSavegame.Write(sim, true);
        Assert.True(AcsActorFlags.TrySet(player, "actor.crashed", true));
        Assert.True(AcsActorFlags.TryGet(player, "CRASHED", out var value)); Assert.True(value);
        sim.RestoreState(sim.CaptureState());
        Assert.NotEqual(baseline, sim.Checksum);
        SimSavegame.Apply(sim, older);
        Assert.False(player.Crashed); Assert.Equal(baseline, sim.Checksum);
        Assert.Equal(older, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(2)]
    public void ReaderRejectsInvalidFlagValues(int value)
    {
        var sim = Room(); sim.Players.Single().Crashed = true;
        var saved = SimSavegame.Write(sim, true);
        var start = saved.Length - BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 8), value);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-crashed-value", error);
    }

    [Fact]
    public void RestoreRejectsInvalidFlagBeforeMutatingActor()
    {
        var sim = Room(); var player = sim.Players.Single(); var state = sim.CaptureState();
        state.Actors[0].CrashedFlag = 2;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.False(player.Crashed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
    });
}
