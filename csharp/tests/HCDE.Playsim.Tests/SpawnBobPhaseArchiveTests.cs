using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnBobPhaseArchiveTests
{
    [Theory]
    [InlineData(64)]
    [InlineData(118)]
    [InlineData(255)]
    public void NativeSpawnByteIsStoredAndRoundTrips(int phase)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        ActorSpawner.InitializePlayerLookCursor(actor, () => (uint)phase);
        Assert.Equal(phase, actor.FloatBobPhase);
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(236, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        actor.FloatBobPhase = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(phase, actor.FloatBobPhase); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void FullByteArchivePreservesOtherActorsLowerPhase()
    {
        var sim = Room(); var first = sim.Actors.Single(); var second = sim.AddBot(200, 0);
        first.FloatBobPhase = 255; ActorPropertyActions.SetFloatBobPhase(second, 12);
        var saved = SimSavegame.Write(sim, true); first.FloatBobPhase = second.FloatBobPhase = 0;
        SimSavegame.Apply(sim, saved); Assert.Equal(255, first.FloatBobPhase); Assert.Equal(12, second.FloatBobPhase);
    }

    [Fact]
    public void CorruptionTargetsNestedFeatureRatherThanOuterTrailer()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.DontOverlap = true; actor.FloatBobPhase = 255;
        var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 195);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 8), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-dont-overlap-value", error);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(256)]
    public void FullByteArchiveRejectsInvalidPhase(int phase)
    {
        var sim = Room(); sim.Actors.Single().FloatBobPhase = 255; var saved = SimSavegame.Write(sim);
        var start = ArchiveLayerFixture.TrailerStart(saved, 236);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 12), phase);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error)); Assert.Equal("save-spawnbobphase-value", error);
    }

    [Fact]
    public void ScriptSetterKeepsNativeNarrowerRange()
    {
        var actor = new Actor { FloatBobPhase = 255 };
        ActorPropertyActions.SetFloatBobPhase(actor, 64); Assert.Equal(255, actor.FloatBobPhase);
        ActorPropertyActions.SetFloatBobPhase(actor, 63); Assert.Equal(63, actor.FloatBobPhase);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }]
    });
}
