using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorDebrisRevivalTests
{
    [Theory]
    [InlineData(135, 79, false)]
    [InlineData(136, 80, false)]
    [InlineData(137, 81, false)]
    [InlineData(135, 79, true)]
    [InlineData(136, 80, true)]
    [InlineData(137, 81, true)]
    public void RevivalRestoresOriginalClassMoverDefaultAfterSavedRuntimeDisable(int index, ushort editor, bool remapped)
    {
        var sim = Room(index, editor, remapped); var actor = Assert.Single(sim.Actors);
        Assert.True(actor.DefaultMoveWithSector); actor.MoveWithSector = false; actor.Health = 0;
        var saved = SimSavegame.Write(sim, true); var fresh = Room(index, editor, remapped); SimSavegame.Apply(fresh, saved);
        var loaded = Assert.Single(fresh.Actors); Assert.False(loaded.MoveWithSector); Assert.True(loaded.DefaultMoveWithSector);
        ActorRaise.ReviveSupported(actor); ActorRaise.ReviveSupported(loaded);
        ActorRaise.EnterRaiseState(actor, null); ActorRaise.EnterRaiseState(loaded, null);
        Assert.True(actor.MoveWithSector); Assert.True(loaded.MoveWithSector); Assert.False(actor.IsDead);
        Assert.Equal(SimSavegame.Write(sim, true), SimSavegame.Write(fresh, true));
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 16, FloorDirection = 1, Speed = 8 });
        fresh.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Floor, TargetFloor = 16, FloorDirection = 1, Speed = 8 });
        sim.Tick(); fresh.Tick(); Assert.Equal(8, actor.Z.ToDouble()); Assert.Equal(actor.Z, loaded.Z);
        Assert.Equal(sim.Checksum, fresh.Checksum);
    }

    [Theory]
    [InlineData(3004)]
    [InlineData(73)]
    [InlineData(54)]
    public void OtherRepresentedClassesStillClearRuntimeMoverOnRevival(ushort editor)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = editor }] });
        var actor = Assert.Single(sim.Actors); actor.MoveWithSector = true; actor.Health = 0;
        Assert.False(actor.DefaultMoveWithSector); ActorRaise.ReviveSupported(actor); Assert.False(actor.MoveWithSector);
    }

    [Fact]
    public void MapRemapNumberDoesNotReplaceOriginalClassMoverDefault()
    {
        var debris = Room(135, 79, true); Assert.Equal(9095, debris.Actors.Single().DoomEdNum);
        Assert.True(debris.Actors.Single().DefaultMoveWithSector);
        var patch = DehackedPatch.Apply("Thing 129\nID # = 79\n"); Assert.Empty(patch.Errors);
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 79 }] }, dehacked: patch);
        var hanging = sim.Actors.Single(); Assert.Equal(73, hanging.ClassDoomEdNum); Assert.False(hanging.DefaultMoveWithSector);
        hanging.Health = 0; hanging.MoveWithSector = true; ActorRaise.ReviveSupported(hanging); Assert.False(hanging.MoveWithSector);
    }

    private static AuthoritySimulation Room(int index, ushort editor, bool remapped)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 200\n" + (remapped ? "ID # = 9095\n" : "")); Assert.Empty(patch.Errors);
        return AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = remapped ? (ushort)9095 : editor }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
    }
}
