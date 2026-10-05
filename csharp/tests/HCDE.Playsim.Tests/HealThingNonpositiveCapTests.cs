using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HealThingNonpositiveCapTests
{
    [Theory]
    [InlineData(-20, 50, -10, 30)]
    [InlineData(-20, 5, -10, -15)]
    [InlineData(-5, 50, -10, -5)]
    public void NegativeExplicitCapIsOnlyAnEligibilityThreshold(int initial, int amount, int cap, int expected)
    {
        var player = new PlayerPawn { Health = initial };
        Assert.True(HealthActions.Execute(248, player, amount, cap));
        Assert.Equal(expected, player.Health);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void NonpositivePatchedSoulCapDoesNotClampDirectHealing(int cap)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
            dehacked: DehackedPatch.Apply($"Misc 0\nMax Soulsphere = {cap}\n"));
        var player = sim.Players.Single(); player.Health = -20;
        Assert.True(HealthActions.Execute(248, player, 50, 1)); Assert.Equal(30, player.Health);
    }

    [Fact]
    public void ZeroModeStillUsesGiveBodyAndDoesNotHealDeadPlayer()
    {
        var player = new PlayerPawn { Health = -20 };
        Assert.True(HealthActions.Execute(248, player, 50, 0)); Assert.Equal(-20, player.Health);
    }
}
