using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorVisibilityTests
{
    [Theory]
    [InlineData(128, 0, true)]
    [InlineData(127, 0, false)]
    [InlineData(0, 128, true)]
    [InlineData(0, 129, false)]
    [InlineData(0, 0, true)]
    public void CustomDistanceLimitsIncludeBoundaries(double maximum, double minimum, bool expected)
    {
        var sim = Room();
        Assert.Equal(expected, MonsterBrain.IsTargetablePlayer(sim, sim.Actors[0], sim.Players.Single(), false,
            new ActorLookParameters(maximum, minimum)));
    }

    [Theory]
    [InlineData(60, false)]
    [InlineData(90, true)]
    public void CustomFieldOfViewUsesNativeHalfAngle(double fieldOfView, bool expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Y = Fixed.FromInt(128);
        Assert.Equal(expected, ActorVisibility.IsVisible(sim, sim.Actors[0], player, false,
            new ActorLookParameters(FieldOfView: fieldOfView)));
    }

    [Theory]
    [InlineData(0, false, true)]
    [InlineData(1, false, false)]
    [InlineData(1, true, true)]
    public void CloseRearTargetExceptionIsDisabledByMinimumDistance(double minimum, bool allAround, bool expected)
    {
        var sim = Room(); var player = sim.Players.Single(); player.X = Fixed.FromInt(-32);
        Assert.Equal(expected, ActorVisibility.IsVisible(sim, sim.Actors[0], player, allAround,
            new ActorLookParameters(MinDistance: minimum)));
    }

    [Fact]
    public void EnemyQueryAppliesCustomDistanceParametersToMonsters()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Blockmap = new MapBlockmapRecord(new MapBlockmapHeader(0, 0, 8, 8), []),
            Things = [new LevelThing { Type = 3004, X = 64, Y = 64, Friendly = true },
                new LevelThing { Type = 3001, X = 320, Y = 64 }]
        });
        var targets = new List<Actor>();
        Assert.Equal(0, ActorEnemySearchActions.LookForEnemiesEx(sim.Actors[0], targets, 512,
            parameters: new ActorLookParameters(MaxDistance: 255)));
        Assert.Equal(1, ActorEnemySearchActions.LookForEnemiesEx(sim.Actors[0], targets, 512,
            parameters: new ActorLookParameters(MaxDistance: 256)));
        Assert.Same(sim.Actors[1], targets.Single());
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 3004 }, new LevelThing { Type = 1, X = 128 }]
    });
}
