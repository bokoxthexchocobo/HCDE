using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsCheckActorTextureServiceTests
{
    [Fact]
    public void MatchesSectorFloorFlat()
    {
        var level = new PlayLevel
        {
            MapName = "MAP01",
            Sectors = [new LevelSector { FloorPic = "FLOOR4_8", CeilingPic = "CEIL1_1" }],
            Things = [new LevelThing { Type = 3001, X = 0, Y = 0 }],
        };
        var sim = AuthoritySimulation.Start(level);
        var actor = sim.Actors.First();
        Assert.True(AcsCheckActorTextureService.TryCheck(sim, 0, actor, "FLOOR4_8", floor: true));
        Assert.False(AcsCheckActorTextureService.TryCheck(sim, 0, actor, "CEIL1_1", floor: true));
        Assert.True(AcsCheckActorTextureService.TryCheck(sim, 0, actor, "CEIL1_1", floor: false));
    }
}
