using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealThingTests
{
    [Theory]
    [InlineData(20, 0, 70)]
    [InlineData(-100, 0, 100)]
    [InlineData(200, 1, 200)]
    [InlineData(200, 120, 120)]
    [InlineData(-10, 120, 40)]
    public void PlayerActionUsesNativeDefaultAndExplicitCaps(int amount, int maximum, int expected)
    {
        var player = new PlayerPawn { Health = 50 };
        Assert.True(HealthActions.Execute(248, player, amount, maximum));
        Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void NonplayerIgnoresExplicitCapAndNullActivatorFails()
    {
        var actor = new Actor { Health = 50, ResurrectionHealth = 100 };
        Assert.True(HealthActions.Execute(248, actor, 200, 999));
        Assert.Equal(100, actor.Health);
        Assert.False(HealthActions.Execute(248, null, 10, 0));
        Assert.Null(HealthActions.Execute(247, actor, 10, 0));
    }

    [Fact]
    public void MapLineActivatesAndClearsAfterHealing()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }],
        });
        var player = sim.Players.Single(); player.Health = 50;
        var line = new LevelLine { Special = 248, Arg0 = 10, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, player, line, true));
        Assert.Equal(60, player.Health);
        Assert.Equal(0, line.Special);
    }
}
