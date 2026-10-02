using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorAcsAngleUnitTests
{
    [Theory]
    [InlineData(79, 16384, 90)]
    [InlineData(79, -16384, 270)]
    [InlineData(79, 81920, 90)]
    [InlineData(80, 8192, 45)]
    [InlineData(80, -8192, -45)]
    [InlineData(80, 32768, -180)]
    [InlineData(80, 57344, -45)]
    [InlineData(80, 73728, 45)]
    public void ChangeActorAnglesUseFixedTurnsForEveryMatchingTid(int function, int value, double degrees)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 256 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var first = sim.AddBot(64, 0); var second = sim.AddBot(64, 64);
        first.ThingId = second.ThingId = 7;
        var stack = new List<int> { 987, 7, value, 1 };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new AcsActivatorBinding { Value = sim.Players.Single() },
            [], new AcsGlobalStrings(), function, 3, out var result));
        Assert.Equal(0, result);
        Assert.Equal(new[] { 987 }, stack);
        foreach (var actor in new Actor[] { first, second })
            Assert.Equal(degrees, function == 79 ? actor.Angle.ToDegrees() : actor.PitchDegrees, 5);
        Assert.Equal(0u, sim.Players.Single().Angle.Raw);
    }
}
