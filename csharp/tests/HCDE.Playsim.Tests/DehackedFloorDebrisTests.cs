using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedFloorDebrisTests
{
    [Theory]
    [InlineData(135, 79, "ColonGibs", 4)]
    [InlineData(136, 80, "SmallBloodPool", 1)]
    [InlineData(137, 81, "BrainStem", 4)]
    public void NativeDefaultsAndRemappedDestroyedReconstruction(int index, int editor, string name, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nID # = 9096\nMass = 200\n"); Assert.Empty(patch.Errors);
        var definition = patch.Actors.Single(a => a.Index == index); Assert.Equal(name, definition.Name); Assert.Equal(editor, definition.OriginalDoomEdNum);
        var sim = Room(9096, patch); var actor = Assert.Single(sim.Actors);
        Assert.Equal(height, actor.Height.ToDouble()); Assert.Equal(20, actor.Radius.ToDouble()); Assert.True(actor.NoBlockmap);
        Assert.True(actor.MoveWithSector); Assert.False(actor.Solid); Assert.False(actor.Shootable); Assert.Null(actor.Brain);
        var saved = SimSavegame.Write(sim, true); actor.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.NotSame(actor, Assert.Single(sim.Actors)); Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(9096, patch); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Theory]
    [InlineData(135, 79)]
    [InlineData(136, 80)]
    [InlineData(137, 81)]
    public void SectorMotionActuallyCarriesNoBlockmapDebrisAfterRestore(int index, ushort editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n");
        var sim = Room(editor, patch); var actor = Assert.Single(sim.Actors);
        SimSavegame.Apply(sim, SimSavegame.Write(sim, true));
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 24, FloorDirection = 1, Speed = 8 });
        sim.Tick();
        Assert.True(sim.FloorOf(0) == 8 && actor.Z.ToDouble() == 8,
            $"floor={sim.FloorOf(0)} z={actor.Z.ToDouble()} mover={actor.IsSectorMoverActor} sector={actor.SectorIndex}");
        sim.Tick(); Assert.Equal(16, actor.Z.ToDouble());
        sim.Tick(); Assert.Equal(24, actor.Z.ToDouble());
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 0, FloorDirection = -1, Speed = 12 });
        sim.Tick(); Assert.Equal(12, actor.Z.ToDouble());
    }

    [Fact]
    public void RuntimeMoveWithSectorDisableSurvivesSaveAndDoesNotAffectClassDefault()
    {
        var patch = DehackedPatch.Apply("Thing 135\nMass = 200\n"); var sim = Room(79, patch);
        sim.Actors.Single().MoveWithSector = false; var saved = SimSavegame.Write(sim, true);
        var fresh = Room(79, patch); SimSavegame.Apply(fresh, saved); Assert.False(fresh.Actors.Single().MoveWithSector);
        Assert.True(Room(79, patch).Actors.Single().MoveWithSector); Assert.Equal(saved, SimSavegame.Write(fresh, true));
    }

    [Fact]
    public void PrimaryFlagReplacementKeepsSecondarySectorMovementAndCanEnableCollision()
    {
        var original = DehackedPatch.Apply("Thing 136\nMass = 200\n");
        var changed = DehackedPatch.Apply("Thing 136\nBits = SOLID\nHeight = 2097152\n", original); Assert.Empty(changed.Errors);
        var actor = Assert.Single(Room(80, changed).Actors); Assert.True(actor.MoveWithSector); Assert.False(actor.NoBlockmap);
        Assert.True(actor.Solid); Assert.Equal(32, actor.Height.ToDouble());
        Assert.True(Assert.Single(Room(80, original).Actors).NoBlockmap);
    }

    [Theory]
    [InlineData(79)]
    [InlineData(80)]
    [InlineData(81)]
    public void StockDebrisAlsoReceivesMissingNativeSectorMovementDefault(ushort editor)
    {
        var sim = Room(editor, null); Assert.True(sim.Actors.Single().MoveWithSector);
        var target = sim.Actors.Single(); target.X = Fixed.FromInt(30);
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma); missile.SetXYZ(0, 0, 1);
        missile.VelocityX = Fixed.FromInt(30); missile.VelocityY = missile.VelocityZ = default;
        missile.Tick(); Assert.False(missile.Destroyed);
    }

    private static AuthoritySimulation Room(ushort type, DehackedPatchResult? patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = type }], Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
