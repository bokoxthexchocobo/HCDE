using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkSpawnFloorClipTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 32)]
    [InlineData(64, 32)]
    public void InitialClippingUsesFinalSpawnHeightAndReplays(int z, int height)
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Z = Fixed.FromInt(z), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(height) });
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); Assert.NotEmpty(chunks);
        Assert.All(chunks, chunk => {
            Assert.Equal(chunk.Z.Raw == 0 ? 8 : 0, chunk.FloorClip);
            Assert.Equal(chunk.Z.Raw == 0, chunk.OnGround);
            Assert.Equal(chunk.Z, chunk.PreviousZ);
        });
        if (z > 0) Assert.All(chunks, chunk => Assert.Equal(0, chunk.FloorClip));
        if (height == 0 && z == 0) Assert.All(chunks, chunk => Assert.Equal(8, chunk.FloorClip));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
        TerrainDefinitions = [new LevelTerrainDefinition("Water", FootClip: 8)], DefaultTerrain = "Water",
    });
}
