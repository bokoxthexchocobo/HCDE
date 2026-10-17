using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class InventoryDebrisWoundDefaultsTests
{
    [Theory]
    [InlineData(false, 3, false)]
    [InlineData(false, 4, true)]
    [InlineData(true, 3, false)]
    [InlineData(true, 4, true)]
    public void InheritedWoundThresholdAppliesAtSixHealth(bool debris, int damage, bool wounds)
    {
        var sim = Room(); var actor = Subject(sim, debris);
        Assert.Equal(6, actor.WoundHealth);
        ActorPropertyActions.ChangeFlag(actor, "SHOOTABLE", true);
        actor.RestoreHealth(10); actor.SetTypedWound("Fire", actor.GenericCrushState);
        var frame = actor.States.Current;
        ActorDamage.Apply(actor, damage, damageType: "Fire");
        Assert.Equal(10 - damage, actor.Health);
        Assert.Equal(wounds ? actor.GenericCrushState : frame, actor.States.Current);
        if (wounds) Assert.Equal(-1, actor.States.RemainingTics);
        Assert.False(actor.Destroyed);
    }

    [Fact]
    public void MapDropAndDebrisThresholdsSurviveFreshReconstruction()
    {
        var sim = Room(); Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        Subject(sim, true);
        Assert.All(sim.Actors.Where(a => a is IceChunkActor || a.DoomEdNum == PickupCatalog.Clip), a => Assert.Equal(6, a.WoundHealth));
        var saved = SimSavegame.Write(sim, true); var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        Assert.All(fresh.Actors.Where(a => a is IceChunkActor || a.DoomEdNum == PickupCatalog.Clip), a => Assert.Equal(6, a.WoundHealth));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    private static Actor Subject(AuthoritySimulation sim, bool debris)
    {
        if (!debris) return sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level, X = Fixed.FromInt(200),
            Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        return sim.Actors.OfType<IceChunkActor>().First();
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2007, X = 400 }],
        Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
