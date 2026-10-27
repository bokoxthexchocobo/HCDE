namespace HCDE.Playsim.Tests;

public class VmAFloorDflooorHead917SliceTests
{
    [Fact]
    public void PlayFloorSound_UsesSeqTypeWhenSet()
    {
        var sec = new VmAFloorDflooorHead917Slice.FloorSector { SeqType = 4 };
        int? typeArg = null;
        VmAFloorDflooorHead917Slice.PlayFloorSound(
            sec,
            (_, _, type, _, _) => typeArg = type,
            null,
            null);
        Assert.Equal(4, typeArg);
    }

    [Fact]
    public void PlayFloorSound_SkipsWhenSilentMove()
    {
        var sec = new VmAFloorDflooorHead917Slice.FloorSector { Flags = VmAFloorDflooorHead917Slice.SecfSilentmove };
        var called = false;
        VmAFloorDflooorHead917Slice.PlayFloorSound(sec, (_, _, _, _, _) => called = true, null, null);
        Assert.False(called);
    }

    [Fact]
    public void SerializeBundle_RoundTripsFields()
    {
        var floor = new VmAFloorDflooorHead917Slice.FloorThinker
        {
            Sector = new VmAFloorDflooorHead917Slice.FloorSector(),
            Type = 3,
            Speed = 8,
            ResetCount = 2,
            Hexencrush = true,
        };
        var bundle = VmAFloorDflooorHead917Slice.CaptureSerializeFields(floor);
        floor.Type = 0;
        VmAFloorDflooorHead917Slice.ApplySerializeFields(floor, bundle);
        Assert.Equal(3, floor.Type);
        Assert.Equal(8, floor.Speed);
        Assert.True(floor.Hexencrush);
    }

    [Fact]
    public void AdvanceStairTiming_ResetCountZero_SetsResetStair()
    {
        var floor = new VmAFloorDflooorHead917Slice.FloorThinker
        {
            Sector = new VmAFloorDflooorHead917Slice.FloorSector(),
            Type = VmAFloorDflooorHead917Slice.BuildStair,
            Direction = 1,
            OrgDist = 64,
            FloorDestDist = 128,
            ResetCount = 1,
        };
        Assert.False(VmAFloorDflooorHead917Slice.AdvanceStairTiming(floor));
        Assert.Equal(VmAFloorDflooorHead917Slice.ResetStair, floor.Type);
        Assert.Equal(-1, floor.Direction);
        Assert.Equal(64, floor.FloorDestDist);
    }

    [Fact]
    public void AdvanceStairTiming_PauseTime_ReturnsEarly()
    {
        var floor = new VmAFloorDflooorHead917Slice.FloorThinker
        {
            Sector = new VmAFloorDflooorHead917Slice.FloorSector(),
            Type = VmAFloorDflooorHead917Slice.WaitStair,
            PauseTime = 2,
        };
        Assert.True(VmAFloorDflooorHead917Slice.AdvanceStairTiming(floor));
        Assert.Equal(1, floor.PauseTime);
    }

    [Fact]
    public void TickFloorMover_PastDest_AppliesTextureAndPromotesStairlock()
    {
        var sec = new VmAFloorDflooorHead917Slice.FloorSector { Stairlock = VmAFloorDflooorHead917Slice.StairlockBuilding };
        string? tex = null;
        sec.SetFloorTexture = t => tex = t;
        var floor = new VmAFloorDflooorHead917Slice.FloorThinker
        {
            Sector = sec,
            Type = VmAFloorDflooorHead917Slice.GenFloorChg,
            Direction = 1,
            Texture = "FLAT1",
        };
        var stopped = false;
        Assert.True(VmAFloorDflooorHead917Slice.TickFloorMover(
            floor,
            (_, _, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            _ => stopped = true,
            _ => floor.StopInterpolationCalled = true));
        Assert.Equal("FLAT1", tex);
        Assert.True(stopped);
        Assert.True(floor.StopInterpolationCalled);
        Assert.Equal(VmAFloorDflooorHead917Slice.StairlockDone, sec.Stairlock);
        Assert.Null(sec.FloorData);
    }

    [Fact]
    public void TickFloorMover_BuildStairPastDest_BecomesWaitStair()
    {
        var floor = new VmAFloorDflooorHead917Slice.FloorThinker
        {
            Sector = new VmAFloorDflooorHead917Slice.FloorSector(),
            Type = VmAFloorDflooorHead917Slice.BuildStair,
            Direction = 1,
            ResetCount = 2,
        };
        VmAFloorDflooorHead917Slice.TickFloorMover(
            floor,
            (_, _, _, _, _, _, _) => VmADoorTickSound.MoveResult.PastDest,
            null,
            null);
        Assert.Equal(VmAFloorDflooorHead917Slice.WaitStair, floor.Type);
        Assert.Equal(1, floor.ResetCount);
    }

    [Fact]
    public void DfloorDefinedFieldNames_MatchesSerializeArity()
    {
        Assert.Equal(15, VmAFloorDflooorHead917Slice.DfloorDefinedFieldNames.Count);
    }

    [Fact]
    public void PromoteStairlockBuilding_NoOpWhenNotBuilding()
    {
        var sec = new VmAFloorDflooorHead917Slice.FloorSector { Stairlock = 0 };
        Assert.False(VmAFloorDflooorHead917Slice.PromoteStairlockBuilding(sec));
    }
}
