using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageSelfTests
{
    [Theory]
    [InlineData("DoomImp", "Demonic", 0, 90)]
    [InlineData("DoomImp", "Other", 0, 100)]
    [InlineData("ZombieMan", "Demonic", 0, 100)]
    [InlineData("DoomImp", "Other", 256, 90)]
    [InlineData("ZombieMan", "Demonic", 256, 90)]
    [InlineData("ZombieMan", "Other", 256, 100)]
    [InlineData("DoomImp", "Demonic", 64, 100)]
    [InlineData("ZombieMan", "Demonic", 64, 90)]
    [InlineData("DoomImp", "Demonic", 128, 100)]
    [InlineData("DoomImp", "Other", 128, 90)]
    [InlineData(null, "None", 192, 90)]
    public void ExactClassAndSpeciesFiltersHonorExclusionAndEither(string? type, string? species, int flags, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0, 3001); actor.Health = 100;
        ActorPropertyActions.SetSpecies(actor, "Demonic");
        ActorHealthActions.DamageSelf(actor, 10, flags: flags, sourceSelector: AcsActorPointer.Null,
            inflictorSelector: AcsActorPointer.Null, classFilter: type, speciesFilter: species);
        Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(10, 0, 40)]
    [InlineData(-10, 0, 60)]
    [InlineData(0, 0, 50)]
    [InlineData(-10, 4, 10)]
    public void DamageSelfUsesSignedAmountAndKillAddsCurrentHealth(int amount, int flags, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var actor = sim.Players.Single(); actor.Health = 50;
        ActorHealthActions.DamageSelf(actor, amount, flags: flags);
        Assert.Equal(expected, actor.Health);
    }

    [Theory]
    [InlineData(0, 90, 100)]
    [InlineData(2, 93, 97)]
    public void ArmorIsBypassedUnlessAffectArmorIsSet(int flags, int health, int armor)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var actor = sim.Players.Single(); actor.Inventory.Armor = 100;
        actor.Inventory.ArmorSavePercent = PlayerInventory.GreenSavePercent;
        ActorHealthActions.DamageSelf(actor, 10, flags: flags);
        Assert.Equal(health, actor.Health); Assert.Equal(armor, actor.Inventory.Armor);
    }
}
