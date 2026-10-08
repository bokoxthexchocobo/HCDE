using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MapSpawnTimingTests
{
    [Fact]
    public void MapCreationSharesStreamInActorOrderAndSaveRestoresContinuation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 85 }, new LevelThing { Type = 86 }],
        }, rngSeed: 42);
        Assert.Equal(2, sim.Actors[0].States.RemainingTics);
        Assert.Equal(3, sim.Actors[1].States.RemainingTics);
        var saved = SimSavegame.Write(sim);
        Assert.Equal(235u, sim.NextMapSpawnRandom());
        SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim));
        Assert.Equal(235u, sim.NextMapSpawnRandom());
    }

    [Fact]
    public void NativeSpawnMapThingReferenceVectorUsesByteOutput()
    {
        var sim = Room();
        foreach (var expected in new uint[] { 165, 250, 235, 198 }) Assert.Equal(expected, sim.NextMapSpawnRandom());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(1000)]
    public void MapSpawnRandomizesPositiveDurationWithoutOtherStreamDraws(int tics)
    {
        var sim = Room(); var control = Room(); var actor = sim.Actors[0];
        actor.States.ConfigureSpawn(actor, [new(tics, 0)], 0);
        actor.LevelSpawned();
        Assert.Equal(1 + (int)(control.NextMapSpawnRandom() % (uint)tics), actor.States.RemainingTics);
        Assert.Equal(control.NextMapSpawnRandom(), sim.NextMapSpawnRandom());
        Assert.Equal(control.NextStateRandom(), sim.NextStateRandom());
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(7, true)]
    public void HoldingZeroAndSynchronizedFramesSkipRandomDraw(int tics, bool synchronized)
    {
        var sim = Room(); var actor = sim.Actors[0]; actor.Synchronized = synchronized;
        actor.States.ConfigureSpawn(actor, [new(tics, 0)], 0);
        actor.LevelSpawned();
        Assert.Equal(tics, actor.States.RemainingTics);
        Assert.Equal(Room().NextMapSpawnRandom(), sim.NextMapSpawnRandom());
    }

    [Fact]
    public void RuntimeSpawnDoesNotRandomizeAndDetachedMapTimingFailsExplicitly()
    {
        var sim = Room(); var actor = sim.Actors[0];
        actor.States.ConfigureSpawn(actor, [new(7, 0)], 0);
        ThingActivation.InitializeSpawn(actor, false, mapSpawn: false);
        Assert.Equal(7, actor.States.RemainingTics);
        Assert.Equal(Room().NextMapSpawnRandom(), sim.NextMapSpawnRandom());
        var detached = new Actor(); detached.States.ConfigureSpawn(detached, [new(7, 0)], 0);
        Assert.Throws<InvalidOperationException>(() => detached.LevelSpawned());
    }

    [Fact]
    public void SaveContinuesStreamAndFlagAndOlderSaveResetsBoth()
    {
        var sim = Room(); var actor = sim.Actors[0]; var older = SimSavegame.Write(sim);
        sim.NextMapSpawnRandom();
        Assert.True(AcsActorFlags.TrySet(actor, "Actor.SYNCHRONIZED", true));
        Assert.True(AcsActorFlags.TryGet(actor, "synchronized", out var value)); Assert.True(value);
        var saved = SimSavegame.Write(sim); var next = sim.NextMapSpawnRandom();
        var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored)); Assert.True(restored.Actors[0].Synchronized);
        Assert.Equal(next, restored.NextMapSpawnRandom());
        SimSavegame.Apply(sim, older); Assert.False(actor.Synchronized);
        Assert.Equal(Room().NextMapSpawnRandom(), sim.NextMapSpawnRandom());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void InvalidArchiveIsRejectedBeforeApply(int mutation)
    {
        var sim = Room(); sim.NextMapSpawnRandom(); var saved = SimSavegame.Write(sim);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        var start = saved.Length - size;
        switch (mutation)
        {
            case 0: BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), 23); break;
            case 1: BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start), 112); break;
            case 2: BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 4), 2); break;
            case 3: BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 20), 2); break;
        }
        var checksum = sim.Checksum;
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.NotNull(error);
        Assert.Equal(checksum, sim.Checksum);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
