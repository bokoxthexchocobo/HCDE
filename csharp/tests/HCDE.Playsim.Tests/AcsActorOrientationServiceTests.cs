using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsActorOrientationServiceTests
{
    [Fact]
    public void ChangeActorPitch_UpdatesActivatorPitchDegrees()
    {
        var sim = Room();
        var player = sim.Players.First();
        var stack = new List<int> { 0, 8192 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = player },
            [], new AcsGlobalStrings(), AcsCallFunctions.ChangeActorPitch, 2, out _));
        Assert.Equal(45, player.PitchDegrees, 1);
    }

    [Fact]
    public void ChangeActorRoll_UpdatesMatchedTidActors()
    {
        var sim = Room();
        var stack = new List<int> { 42, 32768 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new(),
            [], new AcsGlobalStrings(), AcsCallFunctions.ChangeActorRoll, 2, out _));
        Assert.All(sim.Actors, actor => Assert.Equal(BamAngle.Angle180, actor.Roll.Raw));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, Id = 42 }, new LevelThing { Type = 2, Id = 42 }],
    });
}
