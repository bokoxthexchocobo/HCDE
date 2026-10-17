using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorTerrainSaveTests
{
    [Theory]
    [InlineData("Mud", "Water")]
    [InlineData("None", "None")]
    public void OverridesAndExplicitClearsRoundTripBothPlanes(string floor, string ceiling)
    {
        var sim = Room(); var old = SimSavegame.Write(sim, true);
        SectorTerrain.ApplyTagged(sim, 7, floor, SectorTerrain.Floor);
        SectorTerrain.ApplyTagged(sim, 7, ceiling, SectorTerrain.Ceiling);
        sim.RestoreState(sim.CaptureState());
        var checksum = sim.Checksum; var saved = SimSavegame.Write(sim, true);
        Assert.Equal(364, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(sim.Level.Sectors[0].FloorTerrain, fresh.Level.Sectors[0].FloorTerrain);
        Assert.Equal(sim.Level.Sectors[0].CeilingTerrain, fresh.Level.Sectors[0].CeilingTerrain);
        Assert.Equal(checksum, fresh.Checksum); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        SimSavegame.Apply(sim, old);
        Assert.Equal("Water", sim.Level.Sectors[0].FloorTerrain); Assert.Equal("Mud", sim.Level.Sectors[0].CeilingTerrain);
        Assert.Equal(old, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void RestoredTerrainContinuesPeriodicDamage()
    {
        var sim = Room(); SectorTerrain.ApplyTagged(sim, 7, "Mud", SectorTerrain.Floor);
        var fresh = Room(); SimSavegame.Apply(fresh, SimSavegame.Write(sim, true));
        sim.Tick(); fresh.Tick();
        Assert.Equal(93, fresh.Players.Single().Health);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void TerrainChangeParticipatesInChecksum()
    {
        var sim = Room(); var baseline = sim.Checksum;
        SectorTerrain.ApplyTagged(sim, 7, "Mud", SectorTerrain.Floor);
        sim.RestoreState(sim.CaptureState()); Assert.NotEqual(baseline, sim.Checksum);
    }

    [Fact]
    public void StructuredTerrainRequiresGeometryAndMatchingCount()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.SectorTerrains = [];
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        state.SectorTerrains = [new("Mud", "Water")]; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void InvalidNameLengthsRejectBinaryArchive(int length)
    {
        var sim = Room(); SectorTerrain.ApplyTagged(sim, 7, "Mud", SectorTerrain.Floor);
        var bytes = SimSavegame.Write(sim, true);
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - size + 8), length);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-sectorterrain-value", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        TerrainDefinitions = [new("Water"), new("Mud", DamageAmount: 7, DamageTimeMask: 0)],
        Sectors = [new LevelSector { CeilingHeight = 128, Tag = 7, FloorTerrain = "Water", CeilingTerrain = "Mud" }],
        Things = [new LevelThing { Type = 1 }]
    });
}
