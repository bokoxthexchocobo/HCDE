using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsSetSectorTerrainTests
{
    [Theory]
    [InlineData(0, "water", "Water", "Old")]
    [InlineData(0, "solid", "Solid", "Old")]
    [InlineData(1, "Water", "Old", "Water")]
    [InlineData(2, "Water", "Old", "Old")]
    [InlineData(-1, "Water", "Old", "Old")]
    [InlineData(0, "Unknown", "", "Old")]
    [InlineData(0, "None", "", "Old")]
    [InlineData(1, "Null", "Old", "")]
    [InlineData(0, "", "", "Old")]
    [InlineData(0, "Water\0ignored", "Water", "Old")]
    public void NativeArgumentOrderSelectorAndNameResolution(int position, string name, string floor, string ceiling)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, FloorTerrain = "Old", CeilingTerrain = "Old" },
                new LevelSector { Tag = 8, CeilingHeight = 128, FloorTerrain = "Unchanged" }],
            Things = [new LevelThing { Type = 1 }],
            TerrainDefinitions = [new("Water")],
        });
        var globals = new AcsGlobalStrings();
        var id = globals.Add(name);
        var stack = new List<int> { 77, 7, position, id };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding(), [], globals,
            AcsCallFunctions.SetSectorTerrain, 3, out var result));
        Assert.Equal(0, result);
        Assert.Equal([77], stack);
        Assert.Equal(floor, sim.Level.Sectors[0].FloorTerrain);
        Assert.Equal(ceiling, sim.Level.Sectors[0].CeilingTerrain);
        Assert.Equal("Unchanged", sim.Level.Sectors[1].FloorTerrain);
    }
}
