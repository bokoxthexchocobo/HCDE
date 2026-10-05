using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedHealThingTests
{
    [Theory]
    [InlineData(300, 100, 300)]
    [InlineData(150, 100, 150)]
    [InlineData(150, 200, 200)]
    public void SoulMaximumModeUsesPatchWithoutHealthUpgradePolicy(int cap, int initial, int expected)
    {
        var sim = Room(cap); var player = sim.Players.Single();
        player.Health = initial; player.BonusHealth = 20; player.Stamina = 10; player.MaxPickupHealth = 400;
        Assert.True(HealthActions.Execute(248, player, 500, 1));
        Assert.Equal(expected, player.Health);
    }

    [Fact]
    public void ExplicitMaximumStillTakesItsLiteralValue()
    {
        var player = Room(300).Players.Single();
        Assert.True(HealthActions.Execute(248, player, 500, 180)); Assert.Equal(180, player.Health);
    }

    [Fact]
    public void MonsterStillUsesSpawnHealthInsteadOfSoulMaximum()
    {
        var monster = Room(300).AddBot(64, 0); monster.Health = 10;
        Assert.True(HealthActions.Execute(248, monster, 500, 1));
        Assert.Equal(monster.SpawnHealth(), monster.Health);
    }

    private static AuthoritySimulation Room(int cap) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        dehacked: DehackedPatch.Apply($"Misc 0\nMax Soulsphere = {cap}\n"));
}
