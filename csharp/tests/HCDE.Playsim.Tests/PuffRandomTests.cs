using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PuffRandomTests
{
    [Theory]
    [InlineData(0, 15, 3, 12, 2, -83, 4)]
    [InlineData(1, 28, 1, 43, 2, -63, 4)]
    [InlineData(12345, 42, 4, 68, 4, 88, 1)]
    public void NamedStreamMatchesNativeSpawnPuffVectors(int seed, int z1, int t1, int z2, int t2, int z3, int t3)
    {
        var sim = Room(seed);
        foreach (var (z, tics) in new[] { (z1, t1), (z2, t2), (z3, t3) })
        {
            var puff = sim.SpawnHitscanPuff(100, 0, 32);
            Assert.Equal(32 + z / 64.0, puff.Z.ToDouble()); Assert.Equal(tics, puff.States.RemainingTics);
            Assert.Equal(12 + tics, puff.RemainingTics); Assert.Equal(puff.Z, puff.PreviousZ);
        }
    }

    [Fact]
    public void NoRandomZSkipsHeightDrawsButRandomizesFirstFrame()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 32, noRandomZ: true);
        Assert.Equal(32, puff.Z.ToDouble()); Assert.InRange(puff.States.RemainingTics, 1, 4);
        var control = Room(); control.SpawnHitscanPuff(100, 0, 32);
        Assert.NotEqual(control.CaptureState().PuffRandomState, sim.CaptureState().PuffRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SaveRestoresFutureSpawnSequenceAndChecksum(bool identities)
    {
        var sim = Room(); sim.SpawnHitscanPuff(100, 0, 32);
        var before = SimSavegame.Write(sim, identities);
        Assert.Equal(222, BinaryPrimitives.ReadUInt16LittleEndian(before.AsSpan(4)));
        var expected = Enumerable.Range(0, 12).Select(_ => Spawn(sim)).ToArray();
        SimSavegame.Apply(sim, before);
        Assert.True(SimSavegame.TryRead(before, out var saved, out var error), error);
        Assert.Equal(saved.PuffRandomState, sim.CaptureState().PuffRandomState);
        if (identities) Assert.Equal(before, SimSavegame.Write(sim, identities));
        // Ordinary saves preserve later actors, so compare the stream's output independently of membership.
        var actual = Enumerable.Range(0, 12).Select(_ => Spawn(sim)).ToArray();
        Assert.Equal(expected.Select(item => (item.Z, item.Tics)), actual.Select(item => (item.Z, item.Tics)));
        if (identities) Assert.Equal(expected, actual);
    }

    [Fact]
    public void RestoringSnapshotBeforeFirstPuffResetsStreamUsage()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        var expected = Spawn(sim); Assert.NotNull(sim.CaptureState().PuffRandomState);
        SimSavegame.Apply(sim, before);
        Assert.Null(sim.CaptureState().PuffRandomState); Assert.Equal(before, SimSavegame.Write(sim, true));
        Assert.Equal(expected, Spawn(sim));
    }

    [Fact]
    public void PuffStreamDoesNotAdvanceCombatOrMapSpawnRandomness()
    {
        var sim = Room(); var before = sim.CaptureState();
        for (var i = 0; i < 20; i++) sim.SpawnHitscanPuff(100, 0, 32);
        var after = sim.CaptureState();
        Assert.Equal(before.CombatRandomState, after.CombatRandomState);
        Assert.Equal(before.MapSpawnRandomState, after.MapSpawnRandomState);
        Assert.Equal(before.StateRandomState, after.StateRandomState);
        Assert.Equal(before.DropItemRandomState, after.DropItemRandomState);
    }

    [Fact]
    public void ReconstructedPuffDoesNotConsumeSpawnRandomness()
    {
        var sim = Room(); var puff = sim.SpawnHitscanPuff(100, 0, 32);
        var before = SimSavegame.Write(sim, true); var state = sim.CaptureState().PuffRandomState;
        puff.Destroy(); sim.Tick(); SimSavegame.Apply(sim, before);
        Assert.Equal(state, sim.CaptureState().PuffRandomState); Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ExpiredPuffsDoNotDiscardTheSavedRandomStream()
    {
        var sim = Room(); sim.SpawnHitscanPuff(100, 0, 32);
        for (var i = 0; i < 20; i++) sim.Tick();
        Assert.Empty(sim.Actors.OfType<PuffActor>());
        var before = SimSavegame.Write(sim, true); var expected = Spawn(sim);
        SimSavegame.Apply(sim, before);
        Assert.Equal(before, SimSavegame.Write(sim, true)); Assert.Equal(expected, Spawn(sim));
    }

    [Theory]
    [InlineData(0, 222, "save-puffrandom-header")]
    [InlineData(0, 14, "save-puffrandom-header")]
    [InlineData(12, 15, "save-puffrandom-size")]
    public void RejectsMalformedRandomTrailer(int offset, int value, string expected)
    {
        var sim = Room(); sim.SpawnHitscanPuff(100, 0, 32); var bytes = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16 + offset), value);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal(expected, error);
    }

    [Fact]
    public void IncompleteRandomSnapshotIsRejectedBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        var state = new SimSaveState { PuffRandomState = 1 };
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static (int Z, int Tics, uint Checksum) Spawn(AuthoritySimulation sim)
    {
        var puff = sim.SpawnHitscanPuff(100, 0, 32); sim.Tick();
        return (puff.Z.Raw, puff.States.RemainingTics, sim.Checksum);
    }
    private static AuthoritySimulation Room(int seed = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    }, rngSeed: seed);
}
