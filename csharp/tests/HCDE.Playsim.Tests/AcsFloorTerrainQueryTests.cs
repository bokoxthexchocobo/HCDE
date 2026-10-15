using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsFloorTerrainQueryTests
{
    [Theory]
    [InlineData("", "Water")]
    [InlineData("mud", "Mud")]
    [InlineData("None", "Water")]
    [InlineData("Null", "Water")]
    [InlineData("Unknown", "Unknown")]
    public void QueryResolvesTerrainDefinitionAndPreservesUnregisteredOverrides(string explicitTerrain, string expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = "FWATER", FloorTerrain = explicitTerrain }],
            Things = [new LevelThing { Type = 1 }],
            TerrainDefinitions = [new("Solid"), new("Water"), new("Mud")],
            FloorTerrainMappings = [new("fwater", "Water")],
            DefaultTerrain = "Solid",
        });
        AssertTerrain(sim, 0, expected);
    }

    [Fact]
    public void UnmappedFloorUsesLevelDefaultAndMissingActorReturnsEmpty()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
            TerrainDefinitions = [new("Solid")],
            DefaultTerrain = "Solid",
        });
        AssertTerrain(sim, 0, "Solid");
        AssertTerrain(sim, 999, "");
    }

    [Fact]
    public void OrdinaryFloorReturnsBuiltInSolid()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        AssertTerrain(sim, 0, "Solid");
    }

    private static void AssertTerrain(AuthoritySimulation sim, int tid, string expected)
    {
        var globals = new AcsGlobalStrings();
        var stack = new List<int> { 77, tid };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack,
            new AcsActivatorBinding { Value = sim.Players.Single() }, [], globals,
            AcsCallFunctions.GetActorFloorTerrain, 1, out var result));
        Assert.Equal(expected, globals.Get(result));
        Assert.Equal([77], stack);
    }
}
