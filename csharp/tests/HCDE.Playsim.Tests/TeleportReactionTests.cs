using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportReactionTests
{
    [Fact]
    public void PlayerTeleportFreezesMovementForEighteenCommandTics()
    {
        var sim = Room(); var player = Assert.Single(sim.Players);
        player.ReactionTime = -5;
        Assert.True(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.Equal(18, player.ReactionTime);
        var x = player.X;
        for (var remaining = 17; remaining >= 0; remaining--)
        {
            sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
            Assert.Equal(remaining, player.ReactionTime);
            Assert.Equal(x, player.X);
        }
        sim.QueueCommand(0, new PlayerCommand { ForwardMove = 8192 }); sim.Tick();
        Assert.True(player.X.Raw > x.Raw);
    }

    [Fact]
    public void FailedTeleportPreservesReactionCounter()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1 }] });
        var player = Assert.Single(sim.Players); player.ReactionTime = 7;
        Assert.False(LineSpecials.Execute(sim, player, LineSpecials.Teleport, 0));
        Assert.Equal(7, player.ReactionTime);
    }

    [Fact]
    public void MonsterTeleportPreservesItsReactionCounter()
    {
        var sim = Room(); var monster = sim.AddBot(-64, 0); monster.ReactionTime = -5;
        Assert.True(LineSpecials.Execute(sim, monster, LineSpecials.Teleport, 0));
        Assert.Equal(-5, monster.ReactionTime);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = LineSpecials.TeleportDestType, X = 200 }],
    });
}
