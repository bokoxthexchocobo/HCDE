using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DroppedPickupGrindTests
{
    [Theory]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, true, false, false)]
    public void InventoryGrindingHonorsItemsDroppedAndDontGib(bool dropped, bool items, bool dontGib, bool destroyed)
    {
        var sim = Room(); var pickup = sim.Actors.Single(actor => actor.ClassDoomEdNum == PickupCatalog.Clip);
        pickup.Dropped = dropped; pickup.DontGib = dontGib;
        Assert.False(ActorPhysics.GrindActor(pickup, items));
        Assert.Equal(destroyed, pickup.Destroyed);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ActualCeilingCrusherDestroysOnlyUnprotectedDroppedPickup(bool dontGib, bool destroyed)
    {
        var sim = Room(); var pickup = sim.Actors.Single(actor => actor.ClassDoomEdNum == PickupCatalog.Clip);
        pickup.Dropped = true; pickup.DontGib = dontGib;
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            CeilingDirection = -1, Speed = 64, TargetCeiling = 0, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(destroyed, pickup.Destroyed);
    }

    [Fact]
    public void RestoredDontGibPickupRetainsCrusherProtection()
    {
        var sim = Room(); var pickup = sim.Actors.Single(actor => actor.ClassDoomEdNum == PickupCatalog.Clip);
        pickup.Dropped = pickup.DontGib = true;
        var saved = SimSavegame.Write(sim, true); var restored = Room(); SimSavegame.Apply(restored, saved);
        var loaded = restored.Actors.Single(actor => actor.ClassDoomEdNum == PickupCatalog.Clip);
        Assert.True(loaded.Dropped); Assert.True(loaded.DontGib);
        ActorPhysics.GrindActor(loaded); Assert.False(loaded.Destroyed);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 64 }],
        Things = [new LevelThing { Type = 1, X = -100 }, new LevelThing { Type = PickupCatalog.Clip, X = 50 }],
    });
}
