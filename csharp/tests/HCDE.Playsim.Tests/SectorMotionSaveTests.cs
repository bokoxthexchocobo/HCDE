using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorMotionSaveTests
{
    [Theory]
    [InlineData(MotionKind.Floor)]
    [InlineData(MotionKind.Lift)]
    [InlineData(MotionKind.Ceiling)]
    [InlineData(MotionKind.Stair)]
    public void MovementAndLifecycleContinueIdenticallyAfterSave(MotionKind kind)
    {
        var sim = Room();
        if (kind == MotionKind.Stair) Assert.True(StairActions.Start(sim, null, 7, 2, 8, reset: 4, delay: 1));
        else if (kind == MotionKind.Ceiling)
            Assert.True(CeilingActions.Start(sim, null, 7, CeilingTarget.Crush, 4, upSpeed: 2, amount: 120,
                returnToStart: true, loop: true, slow: true));
        else sim.Motions.Add(new SectorMotion { SectorIndex = 0, Kind = kind, ClosedFloor = 0, TargetFloor = 8,
            Speed = 2, Delay = 3, CloseAfterOpen = kind == MotionKind.Lift });
        sim.Tick(); var saved = SimSavegame.Write(sim, true);
        var floors = new List<double>(); var ceilings = new List<double>();
        for (var i = 0; i < 14; i++) { sim.Tick(); floors.Add(sim.FloorOf(0)); ceilings.Add(sim.CeilingOf(0)); }
        var expected = SimSavegame.Write(sim, true); var checksum = sim.Checksum;
        SimSavegame.Apply(sim, saved);
        for (var i = 0; i < 14; i++)
        {
            sim.Tick(); Assert.Equal(floors[i], sim.FloorOf(0)); Assert.Equal(ceilings[i], sim.CeilingOf(0));
        }
        Assert.Equal(expected, SimSavegame.Write(sim, true)); Assert.Equal(checksum, sim.Checksum);
    }

    [Fact]
    public void MixedMoversRetainOrderAndSnapshotDoesNotAliasMutableState()
    {
        var sim = Room();
        sim.Motions.Add(new SectorMotion { Kind = MotionKind.Floor, SectorIndex = 0, TargetFloor = 8, Speed = 1 });
        Assert.True(CeilingActions.Start(sim, null, 7, CeilingTarget.LowerBy, 2, amount: 8));
        var state = sim.CaptureState(); var before = SimSavegame.Write(state);
        sim.Motions[0].Wait = 3; sim.Motions.Reverse();
        Assert.Equal(before, SimSavegame.Write(state));
        sim.RestoreState(state);
        Assert.Equal(new[] { MotionKind.Floor, MotionKind.Ceiling }, sim.Motions.Select(m => m.Kind));
        Assert.Equal(0, sim.Motions[0].Wait);
    }

    [Fact]
    public void EmptyOrLegacySnapshotClearsLaterMovingSectorTimeline()
    {
        var sim = Room(); var old = SimSavegame.Write(sim, true);
        Assert.True(StairActions.Start(sim, null, 7, 2, 8));
        SimSavegame.Apply(sim, old); Assert.Empty(sim.Motions);
        var floor = sim.FloorOf(0); sim.Tick(); Assert.Equal(floor, sim.FloorOf(0));
    }

    [Fact]
    public void PausedCrusherRetainsStasisAndResumeTag()
    {
        var sim = Room(); Assert.True(CeilingActions.Start(sim, null, 7, CeilingTarget.Crush, 2, loop: true));
        sim.Motions[0].Paused = true; var saved = SimSavegame.Write(sim, true);
        sim.Motions.Clear(); SimSavegame.Apply(sim, saved);
        sim.Tick(); Assert.Equal(128, sim.CeilingOf(0)); Assert.True(Assert.Single(sim.Motions).Paused);
        Assert.True(CeilingActions.Start(sim, null, 7, CeilingTarget.Crush, 2, loop: true));
        sim.Tick(); Assert.Equal(126, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void MalformedMotionArchiveRejects(int field)
    {
        var sim = Room(); Assert.True(StairActions.Start(sim, null, 7, 2, 8));
        var bytes = SimSavegame.Write(sim, true);
        Assert.Equal(258, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(bytes.Length - 4));
        var record = bytes.AsSpan(bytes.Length - size + 8);
        if (field == 0) BinaryPrimitives.WriteInt32LittleEndian(record, int.MaxValue);
        else if (field == 1) BinaryPrimitives.WriteInt32LittleEndian(record[40..], 99);
        else if (field == 2) BinaryPrimitives.WriteInt32LittleEndian(record[48..], 2);
        else BinaryPrimitives.WriteDoubleLittleEndian(record[88..], double.NaN);
        Assert.False(SimSavegame.TryRead(bytes, out _, out _));
    }

    [Fact]
    public void InvalidSnapshotRejectsBeforeReplacingCurrentMotions()
    {
        var sim = Room(); Assert.True(StairActions.Start(sim, null, 7, 2, 8));
        var state = sim.CaptureState();
        state.SectorMotions = [Assert.Single(state.SectorMotions!) with { Wait = -1 }];
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, Assert.Single(sim.Motions).Wait); Assert.Equal(0, sim.FloorOf(0));
    }

    [Fact]
    public void MixedDoorAndFloorArchivePreservesBothMovers()
    {
        var sim = Room();
        sim.Motions.Add(new SectorMotion { Kind = MotionKind.Door, SectorIndex = 0, TargetCeiling = 136, Speed = 2 });
        sim.Motions.Add(new SectorMotion { Kind = MotionKind.Floor, SectorIndex = 0, TargetFloor = 8, Speed = 1 });
        var saved = SimSavegame.Write(sim, true);
        sim.Tick(); var expected = SimSavegame.Write(sim, true);
        sim.Motions.Clear(); SimSavegame.Apply(sim, saved); sim.Tick();
        Assert.Equal(expected, SimSavegame.Write(sim, true));
        Assert.Equal(130, sim.CeilingOf(0)); Assert.Equal(1, sim.FloorOf(0));
    }

    [Fact]
    public void NativeZeroSpeedCeilingCanBeSavedWithoutMoving()
    {
        var sim = Room(); Assert.True(CeilingActions.Start(sim, null, 7, CeilingTarget.LowerBy, 0, amount: 8));
        var saved = SimSavegame.Write(sim, true);
        sim.Motions.Clear(); SimSavegame.Apply(sim, saved); sim.Tick();
        Assert.Equal(128, sim.CeilingOf(0)); Assert.Single(sim.Motions);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }]
    });
}
