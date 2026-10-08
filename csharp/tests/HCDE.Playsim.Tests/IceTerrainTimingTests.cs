using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceTerrainTimingTests
{
    [Theory]
    [InlineData("floor FLAT Hot", -2)]
    [InlineData("floor FLAT None", 1)]
    public void ParsedFloorMappingControlsTimerAndChecksum(string mapping, int shift)
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Hot { damagetype Fire } terrain Cold { damagetype Ice } defaultterrain Cold " + mapping, out var data, out var error), error);
        var level = new PlayLevel { TerrainDefinitions = data.Definitions, FloorTerrainMappings = data.Floors, DefaultTerrain = data.DefaultTerrain,
            Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "FLAT" }] };
        var sim = AuthoritySimulation.Start(level.CopyForSimulation(), rngSeed: 42); var control = Room("None");
        var chunk = new IceChunkActor(10) { Simulation = sim }; chunk.States.Enter(chunk, 1);
        Assert.Equal(Scale(control.NextIceTics(), shift), chunk.RemainingTics);
        sim.Tick(); var different = AuthoritySimulation.Start(new PlayLevel { TerrainDefinitions = data.Definitions,
            Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "FLAT" }] }, rngSeed: 42);
        different.Tick(); Assert.NotEqual(sim.Checksum, different.Checksum);
    }

    [Fact]
    public void ParsedTerrainDefinitionControlsChunkTimer()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain Hot { damagetype Lava }", out var definitions, out var error), error);
        var sim = AuthoritySimulation.Start(new PlayLevel
        { TerrainDefinitions = definitions, Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Hot" }] }, rngSeed: 42);
        var control = Room("None"); var chunk = new IceChunkActor(10) { Simulation = sim };
        chunk.States.Enter(chunk, 1); Assert.Equal(control.NextIceTics() >> 2, chunk.RemainingTics);
    }

    [Theory]
    [InlineData("Fire", -2)]
    [InlineData("Lava", -2)]
    [InlineData("ice", 1)]
    [InlineData("Acid", 0)]
    [InlineData("None", 0)]
    public void TerrainScalesCreationAndSuccessorTimersWithFreshSaveContinuation(string damageType, int shift)
    {
        var sim = Room(damageType); var control = Room("None");
        foreach (var world in new[] { sim, control })
            world.SpawnIceChunks(new Actor { Simulation = world, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray(); var references = control.Actors.OfType<IceChunkActor>().ToArray();
        Assert.Equal(references.Length, chunks.Length);
        for (var i = 0; i < chunks.Length; i++)
            Assert.Equal(Scale(references[i].RemainingTics, shift), chunks[i].RemainingTics);
        Assert.Equal(control.NextIceTics(), sim.NextIceTics());
        var saved = SimSavegame.Write(sim); var restored = Room(damageType); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored));
        var loaded = restored.Actors.OfType<IceChunkActor>().First();
        chunks[0].States.Enter(chunks[0], 1); loaded.States.Enter(loaded, 1);
        Assert.Equal(chunks[0].RemainingTics, loaded.RemainingTics);
        references[0].States.Enter(references[0], 1);
        Assert.Equal(Scale(references[0].RemainingTics, shift), chunks[0].RemainingTics);
        sim.Tick(); restored.Tick(); Assert.Equal(sim.Checksum, restored.Checksum);
    }

    [Fact]
    public void TerrainNameAloneDoesNotImplyFireDamage()
    {
        var level = new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128, FloorTerrain = "Fire" }] };
        var sim = AuthoritySimulation.Start(level, rngSeed: 42); var control = AuthoritySimulation.Start(level.CopyForSimulation(), rngSeed: 42);
        var chunk = new IceChunkActor(10) { Simulation = sim };
        chunk.States.Enter(chunk, 1); Assert.Equal(control.NextIceTics(), chunk.RemainingTics);
    }

    [Fact]
    public void InitialFrameUsesPositionSectorBeforeChunkSectorIsAssigned()
    {
        var level = new PlayLevel
        {
            TerrainDefinitions = [new LevelTerrainDefinition("Hot", "Fire")],
            Sectors = [new LevelSector { CeilingHeight = 256 }, new LevelSector { CeilingHeight = 256, FloorTerrain = "Hot" }],
            Sides = [new LevelSide { Sector = 1 }],
            Lines = [
                new LevelLine { X1 = 256, Y1 = -128, X2 = 512, Y2 = -128, SideFront = 0, SideBack = -1 },
                new LevelLine { X1 = 512, Y1 = -128, X2 = 512, Y2 = 128, SideFront = 0, SideBack = -1 },
                new LevelLine { X1 = 512, Y1 = 128, X2 = 256, Y2 = 128, SideFront = 0, SideBack = -1 },
                new LevelLine { X1 = 256, Y1 = 128, X2 = 256, Y2 = -128, SideFront = 0, SideBack = -1 }],
        };
        var sim = AuthoritySimulation.Start(level, rngSeed: 42);
        var control = AuthoritySimulation.Start(level.CopyForSimulation(), rngSeed: 42);
        var chunk = new IceChunkActor(10) { Simulation = sim, X = Fixed.FromInt(384) };
        Assert.Equal(-1, chunk.SectorIndex);
        chunk.States.Enter(chunk, 1); Assert.Equal(control.NextIceTics() >> 2, chunk.RemainingTics);
    }

    [Fact]
    public void TerrainConfigurationParticipatesInChecksumAndSurvivesLevelCopy()
    {
        var fire = Room("Fire"); var ice = Room("Ice");
        Assert.Equal("Fire", Assert.Single(fire.Level.CopyForSimulation().TerrainDefinitions).DamageType);
        fire.Tick(); ice.Tick(); Assert.NotEqual(fire.Checksum, ice.Checksum);
    }

    private static int Scale(int value, int shift) => shift < 0 ? value >> -shift : value << shift;
    private static AuthoritySimulation Room(string type) => AuthoritySimulation.Start(new PlayLevel
    {
        TerrainDefinitions = [new LevelTerrainDefinition("HeatedFloor", type)],
        Sectors = [new LevelSector { CeilingHeight = 256, FloorTerrain = "heatedfloor" }],
    }.CopyForSimulation(), rngSeed: 42);
}
