using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsVirtualSpawnHealthTests
{
    private sealed class CustomActor : Actor
    {
        public override int GetMaxHealth(bool withUpgrades) => withUpgrades ? 250 : 175;
    }

    [Fact]
    public void PropertyGetAndCheckUseVirtualMaximumWithoutUpgrades()
    {
        var sim = Room(); var actor = new CustomActor { Health = 30, ResurrectionHealth = 60 };
        Assert.Equal(175, AcsActorProperties.Get(sim, actor, 0, AcsActorProperties.SpawnHealth));
        Assert.True(AcsActorProperties.Check(sim, actor, 0, AcsActorProperties.SpawnHealth, 175));
        Assert.False(AcsActorProperties.Check(sim, actor, 0, AcsActorProperties.SpawnHealth, 60));
        Assert.Equal(60, actor.SpawnHealth());
    }

    [Fact]
    public void MonsterPropertyQueryAndHealingUseDifferentNativeMaxima()
    {
        var sim = Room(); var monster = sim.AddBot(64, 0); monster.Health = 10;
        Assert.Equal(100, AcsActorProperties.Get(sim, monster, 0, AcsActorProperties.SpawnHealth));
        Assert.True(monster.GiveBody(500)); Assert.Equal(monster.SpawnHealth(), monster.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
