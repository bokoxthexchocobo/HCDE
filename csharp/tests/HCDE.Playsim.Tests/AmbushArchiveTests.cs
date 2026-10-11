using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AmbushArchiveTests
{
    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void RuntimeFlagChangesRoundTripInPoseAndArchive(bool spawn, bool runtime)
    {
        var sim = Room(spawn); var actor = sim.Actors.Single();
        actor.Ambush = runtime;
        var state = sim.CaptureState();
        actor.Ambush = spawn; sim.RestoreState(state);
        Assert.Equal(runtime, actor.Ambush);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(182, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var loaded = Room(spawn); SimSavegame.Apply(loaded, bytes);
        Assert.Equal(runtime, loaded.Actors.Single().Ambush);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LegacyArchivesRestoreSpawnDefault(bool spawn)
    {
        var sim = Room(spawn); var bytes = SimSavegame.Write(sim);
        Assert.NotEqual(182, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.Actors.Single().Ambush = !spawn;
        SimSavegame.Apply(sim, bytes);
        Assert.Equal(spawn, sim.Actors.Single().Ambush);
    }

    [Fact]
    public void CorruptFlagIsRejectedBeforeApplyingState()
    {
        var sim = Room(false); sim.Actors.Single().Ambush = true;
        var bytes = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-ambush-value", error);
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, bytes));
        Assert.True(sim.Actors.Single().Ambush);
    }

    private static AuthoritySimulation Room(bool ambush) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004, Ambush = ambush }],
    });
}
