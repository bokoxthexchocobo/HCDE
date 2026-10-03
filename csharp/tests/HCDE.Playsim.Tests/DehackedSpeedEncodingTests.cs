using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedSpeedEncodingTests
{
    [Theory]
    [InlineData(8, 8)]
    [InlineData(255, 255)]
    [InlineData(256, 0.00390625)]
    [InlineData(65536, 1)]
    [InlineData(98304, 1.5)]
    [InlineData(-256, -0.00390625)]
    public void SpeedAssignmentUsesNativeMagnitudeThreshold(int encoded, double expected)
    {
        var patch = DehackedPatch.Apply($"Thing 2\nSpeed = {encoded}\n");
        Assert.Equal(expected, patch.Actors.Single(actor => actor.Index == 2).Speed);
    }

    [Fact]
    public void FractionalSpeedReachesSpawnedActorAndSurvivesUnrelatedPatch()
    {
        var first = DehackedPatch.Apply("Thing 2\nSpeed = 98304\n");
        var second = DehackedPatch.Apply("Thing 2\nHit points = 88\n", first);
        Assert.Equal(1.5, first.Actors.Single(actor => actor.Index == 2).Speed);
        var actor = Assert.Single(AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 3004 }],
        }, dehacked: second).Actors);
        Assert.Equal(1.5, actor.MovementSpeed.ToDouble());
        Assert.Equal(0.375, actor.ChaseSpeed);
    }
}
