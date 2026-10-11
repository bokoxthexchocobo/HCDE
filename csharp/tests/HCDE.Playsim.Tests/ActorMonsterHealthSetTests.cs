using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorMonsterHealthSetTests
{
    [Theory]
    [InlineData("Health")]
    [InlineData("Stimpack")]
    [InlineData("Medikit")]
    [InlineData("HealthBonus")]
    [InlineData("Soulsphere")]
    [InlineData("MegasphereHealth")]
    public void MonsterSetterUsesDirectHealingAndSpawnLimitWithoutPlayerFactor(string name)
    {
        var sim = Room(); var monster = sim.AddBot(20, 0);
        monster.Health = monster.ResurrectionHealth - 10;
        var before = monster.Health;
        Assert.True(ActorInventoryActions.SetInventory(monster, name, 3));
        Assert.Equal(before + 3, monster.Health);
        Assert.True(ActorInventoryActions.SetInventory(monster, name, int.MaxValue, beyondMax: true));
        Assert.Equal(monster.ResurrectionHealth, monster.Health);
        Assert.False(ActorInventoryActions.SetInventory(monster, name, 1));
        Assert.True(ActorInventoryActions.SetInventory(monster, name, -1));
    }

    [Fact]
    public void TargetStateCallbackAndSavePreserveHealedMonster()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var monster = sim.AddBot(40, 0);
        caller.Brain!.SetSpecialTarget(monster); monster.Health = monster.ResurrectionHealth - 10;
        var before = monster.Health; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "Medikit", 3, AcsActorPointer.Target))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(before + 3, monster.Health);
        var saved = SimSavegame.Write(sim); monster.Health = 1; SimSavegame.Apply(sim, saved);
        Assert.Equal(before + 3, monster.Health);
    }

    [Fact]
    public void DeadDestroyedNullAndStoredInventoryRemainGuarded()
    {
        var sim = Room(); var monster = sim.AddBot(20, 0); monster.Health = 0;
        Assert.False(ActorInventoryActions.SetInventory(monster, "Health", 5));
        Assert.True(ActorInventoryActions.SetInventory(monster, "Health", 0));
        Assert.False(ActorInventoryActions.SetInventory(monster, "MedPatch", 1));
        Assert.False(ActorInventoryActions.SetInventory(monster, "Health", 5, AcsActorPointer.Null));
        monster.Destroy(); Assert.False(ActorInventoryActions.SetInventory(monster, "Health", 0));
        Assert.Equal(0, monster.Health);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(HealthFactor: 2));
}
