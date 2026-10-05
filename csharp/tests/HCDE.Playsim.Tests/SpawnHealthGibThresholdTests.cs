using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SpawnHealthGibThresholdTests
{
    [Theory]
    [InlineData(2.5, 75, 1)]
    [InlineData(2.5, 101, 2)]
    [InlineData(-37, 74, 1)]
    [InlineData(-37, 75, 2)]
    public void SpawnHealthUpdatesDefaultGibThreshold(double health, int damage, int expectedState)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 3004, Health = health }],
        });
        var actor = Assert.Single(sim.Actors);
        Assert.Equal(-actor.Health, actor.GibHealth);
        actor.DeathState = 1; actor.ExtremeDeathState = 2;
        actor.States.Configure(actor, [new ActorFrame(-1, 0), new ActorFrame(-1, 1), new ActorFrame(-1, 2)], 0);
        ActorDamage.Apply(actor, damage, null);
        Assert.Equal(expectedState, actor.States.Current);
    }
}
