using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SkullChargeReactionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-7)]
    [InlineData(int.MaxValue)]
    public void StartingAndTickingChargePreservesReactionTime(int reaction)
    {
        var sim = Room();
        var soul = sim.Actors.Single(actor => actor.DoomEdNum == 3006);
        var target = sim.Players.Single();
        soul.ReactionTime = reaction;
        soul.Brain!.StartCharge(soul, target);
        Assert.Equal(reaction, soul.ReactionTime);
        Assert.True(soul.Brain.Charging);
        Assert.Equal(20, soul.VelocityX.ToDouble());
        soul.Brain.Tick(sim, soul);
        Assert.Equal(reaction, soul.ReactionTime);
        Assert.Equal(20, soul.VelocityX.ToDouble());
        soul.Brain.StopCharge(soul);
        Assert.Equal(reaction, soul.ReactionTime);
    }

    [Fact]
    public void SpawnedChargingSoulKeepsItsInitialReactionCounter()
    {
        var sim = Room();
        var parent = sim.Actors.Single(actor => actor.DoomEdNum == 71);
        var soul = sim.SpawnLostSoul(parent, sim.Players.Single(), 0);
        Assert.NotNull(soul);
        Assert.True(soul.Brain!.Charging);
        Assert.Equal(10, soul.ReactionTime);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006 },
            new LevelThing { Type = 71, Y = 300 }],
    });
}
