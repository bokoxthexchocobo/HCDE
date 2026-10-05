using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class LevelMassacreTests
{
    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 1)]
    public void LevelOperationReportsActualKillsAndFiltersFriendlies(bool baddies, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 100 },
                new LevelThing { Type = 3004, X = 200 }, new LevelThing { Type = 3004, X = 300 }],
        });
        sim.Actors[2].Friendly = true;
        sim.Actors[3].Dormant = true;
        Assert.Equal(expected, sim.Massacre(baddies));
        Assert.Equal(0, sim.Massacre(baddies));
        Assert.Equal(!baddies, sim.Actors[2].IsDead);
        Assert.False(sim.Actors[3].IsDead);
        Assert.False(sim.Players.Single().IsDead);
    }

    [Fact]
    public void FailedActorAttemptRestoresFlagsAndDoesNotLoop()
    {
        var actor = new Actor { DoomEdNum = LineSpecials.TeleportDestType, Health = 100,
            Shootable = false, Dormant = true, Invulnerable = true };
        Assert.False(actor.Massacre());
        Assert.Equal(100, actor.Health);
        Assert.False(actor.Shootable);
        Assert.True(actor.Dormant);
        Assert.True(actor.Invulnerable);
    }
}
