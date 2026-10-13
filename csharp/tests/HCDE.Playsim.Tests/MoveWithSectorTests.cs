using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class MoveWithSectorTests
{
    [Theory]
    [InlineData(MotionKind.Ceiling, false, false, false)]
    [InlineData(MotionKind.Ceiling, true, false, true)]
    [InlineData(MotionKind.Ceiling, true, true, false)]
    [InlineData(MotionKind.Floor, false, false, false)]
    [InlineData(MotionKind.Floor, true, false, true)]
    [InlineData(MotionKind.Floor, true, true, false)]
    [InlineData(MotionKind.Door, false, false, false)]
    [InlineData(MotionKind.Door, true, false, true)]
    [InlineData(MotionKind.Door, true, true, false)]
    public void NoBlockmapActorsObstructOnlyWithMoveWithSector(MotionKind kind,
        bool moveWithSector, bool noSector, bool obstructs)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, blockmap: 1, sector: noSector ? 1 : 0);
        ActorPropertyActions.ChangeFlag(actor, "MOVEWITHSECTOR", moveWithSector);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind,
            TargetFloor = 32, FloorDirection = 1, TargetCeiling = 0, ClosedCeiling = 0,
            Closing = kind == MotionKind.Door, CeilingDirection = -1, Speed = 16, CrushDamage = -1 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(kind == MotionKind.Floor ? obstructs ? 0 : 16 : 0, sim.FloorOf(0));
        Assert.Equal(kind == MotionKind.Floor ? 64 : obstructs ? 64 : 48, sim.CeilingOf(0));
        Assert.Equal(100, actor.Health);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void NoBlockmapCrushDamageRequiresMoveWithSector(bool moveWithSector, bool noSector, bool crushed)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, blockmap: 1, sector: noSector ? 1 : 0);
        ActorPropertyActions.ChangeFlag(actor, "MOVEWITHSECTOR", moveWithSector);
        sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = MotionKind.Ceiling,
            TargetCeiling = 0, CeilingDirection = -1, Speed = 16, CrushDamage = 10 });
        LineSpecials.TickMotions(sim);
        Assert.Equal(crushed ? 90 : 100, actor.Health);
        Assert.Equal(48, sim.CeilingOf(0));
    }

    [Fact]
    public void FlagRoundTripAndLegacyRestorePreserveChecksum()
    {
        var sim = Room(); var actor = sim.Players.Single();
        var baseline = SimSavegame.Write(sim); SimSavegame.Apply(sim, baseline); var hash = sim.Checksum;
        ActorPropertyActions.ChangeFlag(actor, "movewithsector", true);
        Assert.True(AcsActorFlags.TryGet(actor, "Actor.MOVEWITHSECTOR", out var flag));
        Assert.True(flag);
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(192, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        ActorPropertyActions.ChangeFlag(actor, "MOVEWITHSECTOR", false);
        SimSavegame.Apply(sim, bytes);
        Assert.True(actor.MoveWithSector); Assert.NotEqual(hash, sim.Checksum);
        Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, baseline);
        Assert.False(actor.MoveWithSector); Assert.Equal(hash, sim.Checksum);
        Assert.Equal(baseline, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void StationaryPinchHonorsMoverEligibility(bool moveWithSector, bool noSector, bool crushed)
    {
        var sim = Room(); var actor = sim.Players.Single();
        ActorPropertyActions.ChangeLinkFlags(actor, blockmap: 1, sector: noSector ? 1 : 0);
        ActorPropertyActions.ChangeFlag(actor, "MOVEWITHSECTOR", moveWithSector);
        for (var tick = 0; tick < 4; tick++) sim.Tick();
        sim.Ceilings[0] = 32;
        ActorPhysics.FitToSector(sim, actor, sectorPinchCrush: true);
        Assert.Equal(crushed ? 90 : 100, actor.Health);
    }

    [Fact]
    public void ArchiveRejectsInvalidFlag()
    {
        var sim = Room(); sim.Players.Single().MoveWithSector = true;
        var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 8), 2);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-move-with-sector-value", error);
    }

    [Fact]
    public void RaisingRestoresDefaultMoverFlag()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] });
        var actor = sim.Actors.Single();
        actor.Health = 0; actor.States.Enter(actor, ActorStateMachine.Corpse);
        actor.MoveWithSector = true;
        Assert.True(ThingRaise.Execute(sim, 17, actor, 0, 2));
        Assert.False(actor.MoveWithSector);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 64 }], Things = [new LevelThing { Type = 1 }] });
}
