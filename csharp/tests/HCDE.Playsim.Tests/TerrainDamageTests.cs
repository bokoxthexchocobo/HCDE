using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TerrainDamageTests
{
    [Theory]
    [InlineData(0, 37)]
    [InlineData(3, 79)]
    public void PlayerDamageUsesMaskPlusOneModuloCadence(int mask, int health)
    {
        var sim = Room(mask);
        for (var i = 0; i < 9; i++) sim.Tick();
        Assert.Equal(health, sim.Players.Single().Health);
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(false, true, true, true)]
    public void MonsterEligibilityMatchesSectorDamagePolicy(bool hurtMonsters, bool noDamage, bool force, bool hurt)
    {
        var sim = Room(0, hurtMonsters); var actor = sim.Actors.Single(a => a.IsMonster);
        var health = actor.Health; actor.NoSectorDamage = noDamage; actor.ForceSectorDamage = force;
        sim.Tick(); Assert.Equal(health - (hurt ? 7 : 0), actor.Health);
    }

    [Fact]
    public void AirborneActorDoesNotTakeTerrainDamageEvenWhenSectorHarmsInAir()
    {
        var sim = Room(0); var actor = sim.Players.Single(); actor.Z = Fixed.FromInt(32); actor.NoGravity = true;
        sim.Tick(); Assert.Equal(100, actor.Health);
    }

    [Fact]
    public void FreshSaveContinuationPreservesCadenceAndTypedDamageFactor()
    {
        var sim = Room(3); sim.Players.Single().SetDamageFactor("Fire", 2);
        for (var i = 0; i < 6; i++) sim.Tick();
        var saved = SimSavegame.Write(sim); var restored = Room(3); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored));
        for (var i = 0; i < 3; i++) { sim.Tick(); restored.Tick(); }
        Assert.Equal(58, sim.Players.Single().Health);
        Assert.Equal(sim.Checksum, restored.Checksum); Assert.Equal(SimSavegame.Write(sim), SimSavegame.Write(restored));
    }

    private static AuthoritySimulation Room(int mask, bool hurtMonsters = false)
    {
        Assert.True(TerrainDefinitionParser.TryParseData($"terrain Hot {{ damagetype Fire damageamount 7 damagetimemask {mask} }} floor FLAT Hot", out var data, out var error), error);
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            TerrainDefinitions = data.Definitions, FloorTerrainMappings = data.Floors,
            Sectors = [new LevelSector { CeilingHeight = 256, FloorPic = "FLAT", HurtMonsters = hurtMonsters, HarmInAir = true }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 96 }],
        }, rngSeed: 42);
        sim.Actors.Single(a => a.IsMonster).Brain = null;
        return sim;
    }
}
