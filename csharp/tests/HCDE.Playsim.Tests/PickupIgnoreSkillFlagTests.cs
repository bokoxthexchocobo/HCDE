using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PickupIgnoreSkillFlagTests
{
    [Theory]
    [InlineData("IGNORESKILL")]
    [InlineData("inventory.ignoreskill")]
    public void ScriptFlagSuppressesBabyAmmoScaling(string flag)
    {
        var sim = Room(PickupCatalog.Clip); var player = sim.Players.Single();
        var pickup = sim.Actors[^1];
        Assert.True(AcsActorFlags.TrySet(pickup, flag, true));
        Assert.True(AcsActorFlags.TryGet(pickup, flag, out var value));
        Assert.True(value);
        sim.Tick();
        Assert.Equal(60, player.Inventory.Bullets);
    }

    [Theory]
    [InlineData(PickupCatalog.GreenArmor, 100)]
    [InlineData(PickupCatalog.ArmorBonus, 1)]
    public void ScriptFlagSuppressesArmorFactor(int type, int expected)
    {
        var sim = Room(type);
        Assert.True(AcsActorFlags.TrySet(sim.Actors[^1], "INVENTORY.IGNORESKILL", true));
        sim.Tick();
        Assert.Equal(expected, sim.Players.Single().Inventory.Armor);
    }

    [Fact]
    public void HealthPickupStillUsesHealthFactor()
    {
        var sim = Room(PickupCatalog.Stimpack); var player = sim.Players.Single();
        player.Health = 10;
        Assert.True(AcsActorFlags.TrySet(sim.Actors[^1], "IGNORESKILL", true));
        sim.Tick();
        Assert.Equal(30, player.Health);
    }

    [Fact]
    public void FlagChangePersistsThroughPickupArchive()
    {
        var sim = Room(PickupCatalog.Clip); var pickup = sim.Actors[^1];
        AcsActorFlags.TrySet(pickup, "IGNORESKILL", true);
        Assert.True(SimSavegame.TryRead(SimSavegame.Write(sim), out var state, out var error), error);
        pickup.IgnoreAmmoSkill = false;
        sim.RestoreState(state);
        Assert.True(pickup.IgnoreAmmoSkill);
        sim.Tick();
        Assert.Equal(60, sim.Players.Single().Inventory.Bullets);
    }

    [Fact]
    public void NonInventoryActorRejectsInventoryFlag()
    {
        var actor = new PlayerPawn();
        Assert.False(AcsActorFlags.TrySet(actor, "IGNORESKILL", true));
        Assert.False(AcsActorFlags.TryGet(actor, "INVENTORY.IGNORESKILL", out _));
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = type }],
    }, spawnOptions: new SpawnOptions(Skill: 0, ArmorFactor: 2, HealthFactor: 2));
}
