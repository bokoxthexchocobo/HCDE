using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorGetFloorTerrainTests
{
    [Theory]
    [InlineData("Mud", "Water", "Lava", "Mud")]
    [InlineData("None", "Water", "Lava", "Water")]
    [InlineData("Null", "", "Lava", "Lava")]
    [InlineData("", "", "", "Solid")]
    [InlineData("mUd", "Water", "Lava", "Mud")]
    public void ResolutionUsesSectorThenTextureThenDefaultEvenAboveFloor(string sector, string mapping, string fallback, string expected)
    {
        var sim = Room(sector, mapping, fallback); var actor = sim.Players.Single();
        actor.Z = Fixed.FromInt(64); actor.NoGravity = true;
        var terrain = actor.GetFloorTerrain(); Assert.NotNull(terrain); Assert.Equal(expected, terrain.Name);
        Assert.Same(sim.Level.FindTerrainDefinition(expected), terrain);
        var saved = SimSavegame.Write(sim, true); var fresh = Room(sector, mapping, fallback); SimSavegame.Apply(fresh, saved);
        Assert.Equal(expected, fresh.Players.Single().GetFloorTerrain()?.Name);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void RuntimeSectorOverrideAndClearAreVisibleImmediately()
    {
        var sim = Room("", "Water", "Lava"); var actor = sim.Players.Single();
        Assert.Equal("Water", actor.GetFloorTerrain()?.Name);
        Assert.Equal(1, SectorTerrain.ApplyTagged(sim, 7, "Mud", SectorTerrain.Floor));
        Assert.Equal("Mud", actor.GetFloorTerrain()?.Name); Assert.Equal(3, actor.GetFloorTerrain()?.FootClip);
        var saved = SimSavegame.Write(sim, true); var fresh = Room("", "Water", "Lava"); SimSavegame.Apply(fresh, saved);
        Assert.Equal("Mud", fresh.Players.Single().GetFloorTerrain()?.Name);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.Equal(1, SectorTerrain.ApplyTagged(sim, 7, "None", SectorTerrain.Floor));
        Assert.Equal("Water", actor.GetFloorTerrain()?.Name);
    }

    [Fact]
    public void UnresolvedTerrainRetainsResolverNullAndDetachedQueriesFailExplicitly()
    {
        var sim = Room("Unknown", "Water", "Lava"); Assert.Null(sim.Players.Single().GetFloorTerrain());
        Assert.Throws<InvalidOperationException>(() => new Actor().GetFloorTerrain());
    }

    private static AuthoritySimulation Room(string sector, string mapping, string fallback) => AuthoritySimulation.Start(new PlayLevel {
        TerrainDefinitions = [new("Mud", FootClip: 3), new("Water", IsLiquid: true), new("Lava", DamageAmount: 10)],
        FloorTerrainMappings = string.IsNullOrEmpty(mapping) ? [] : [new("FLAT1", mapping)], DefaultTerrain = fallback,
        Sectors = [new LevelSector { CeilingHeight = 256, FloorPic = "FLAT1", FloorTerrain = sector, Tag = 7 }],
        Things = [new LevelThing { Type = 1 }]
    });
}
