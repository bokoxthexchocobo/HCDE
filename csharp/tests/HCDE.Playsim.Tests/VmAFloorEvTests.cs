namespace HCDE.Playsim.Tests;

public class VmAFloorEvTests
{
    [Fact]
    public void EvDoFloor_CallsCreateFloorPerTag()
    {
        var ok = VmAFloorEvLineFloor919Slice.EvDoFloor(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => new VmAFloorEv.StairSector { SectorNum = 0 },
                ToCreateSector = s => new VmAFloorCreate.FloorSector { PointToDist = (_, _, z) => z },
                CreateFloor = new VmAFloorCreate.CreateFloorContext
                {
                    CreateThinker = s => new VmAFloorCreate.FloorThinker { Sector = s },
                },
            },
            VmAFloorCreate.FloorRaiseByValue,
            null,
            1,
            4,
            16,
            0,
            0,
            false,
            false);
        Assert.True(ok);
    }

    [Fact]
    public void EvFloorCrushStop_OnlyRaiseAndCrush()
    {
        var sec = new VmAFloorEv.StairSector { FloorData = new object() };
        var destroyed = false;
        VmAFloorEvLineFloor919Slice.EvFloorCrushStop(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                IsFloorThinker = _ => true,
                GetFloorThinkerType = _ => VmAFloorCreate.FloorRaiseAndCrush,
                StopSequence = _ => { },
                DestroyFloorData = _ => destroyed = true,
            },
            1,
            null);
        Assert.True(destroyed);
    }

    [Fact]
    public void EvStopFloor_DestroyActiveThinker()
    {
        var sec = new VmAFloorEv.StairSector { FloorData = new object() };
        var destroyed = false;
        VmAFloorEvLineFloor919Slice.EvStopFloor(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                StopSequence = _ => { },
                DestroyFloorData = _ => { sec.FloorData = null; destroyed = true; },
            },
            1,
            null);
        Assert.True(destroyed);
    }

    [Fact]
    public void EvDoChange_TrigChange_CopiesFrontTexture()
    {
        var sec = new VmAFloorEv.StairSector();
        var tex = "";
        VmAFloorEvDonutChange920Slice.EvDoChange(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                SetFloorTexture = (s, t) => tex = t,
                TransferSpecial = (_, _) => { },
            },
            new VmAFloorEv.EvLine { FrontSector = new VmAFloorEv.StairSector { FloorTexture = "FLAT1" } },
            VmAFloorEv.TrigChangeOnly,
            1);
        Assert.Equal("FLAT1", tex);
    }

    [Fact]
    public void EvBuildStairs_ZeroSpeed_ReturnsFalse()
    {
        Assert.False(VmAFloorEvLineFloor919Slice.EvBuildStairs(new VmAFloorEv.FloorEvContext(), 1, VmAFloorEv.BuildUp, null, 8, 0, 0, 0, 0, 0));
    }

    [Fact]
    public void EvBuildStairs_TwoSidedChain_SpawnsStairThinker()
    {
        var sec0 = new VmAFloorEv.StairSector
        {
            SectorNum = 0,
            FloorTexture = "FLAT1",
            PointToDist = (_, _, z) => z,
            CenterFloor = 0,
        };
        var sec1 = new VmAFloorEv.StairSector
        {
            SectorNum = 1,
            FloorTexture = "FLAT1",
            PointToDist = (_, _, z) => z,
            CenterFloor = 8,
        };
        sec0.Lines.Add(new VmAFloorEv.StairLine
        {
            TwoSided = true,
            FrontSector = sec0,
            BackSector = sec1,
        });
        var thinkers = 0;
        var ok = VmAFloorEvLineFloor919Slice.EvBuildStairs(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = i => i == 0 ? sec0 : sec1,
                PlaneMovingFloor = _ => false,
                BumpValidcount = _ => 1,
                CreateStairThinker = _ =>
                {
                    thinkers++;
                    return new VmAFloorTickElevator.FloorThinker { Sector = new VmAFloorTickElevator.FloorSector() };
                },
                StartFloorSound = _ => { },
            },
            1,
            VmAFloorEv.BuildUp,
            null,
            8,
            4,
            0,
            0,
            0,
            0);
        Assert.True(ok);
        Assert.True(thinkers >= 1);
        Assert.Equal(VmAFloorTickElevator.StairlockBuilding, sec1.Stairlock);
    }

    [Fact]
    public void EvDoDonut_CreatesSlimeAndPillarThinkers()
    {
        var s1 = new VmAFloorEv.StairSector
        {
            SectorNum = 0,
            PointToDist = (_, _, z) => z,
        };
        var s2 = new VmAFloorEv.StairSector
        {
            SectorNum = 1,
            PointToDist = (_, _, z) => z,
        };
        var s3 = new VmAFloorEv.StairSector { FloorTexture = "SLIME", PointToDist = (_, _, z) => z };
        s1.Lines.Add(new VmAFloorEv.StairLine { FrontSector = s1, BackSector = s2 });
        s2.Lines.Add(new VmAFloorEv.StairLine { TwoSided = true, BackSector = s3, FrontSector = s2 });
        var types = new List<int>();
        VmAFloorEvDonut921Slice.EvDoDonut(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = i => i switch { 0 => s1, 1 => s2, _ => s3 },
                GetNextSector = (a, ln, _) => ln.BackSector ?? a,
                FindHighestFloorPoint = (_, _) => 32,
                CreateStairThinker = _ => new VmAFloorTickElevator.FloorThinker { Sector = new VmAFloorTickElevator.FloorSector(), Type = 0 },
                StartFloorSound = t => types.Add(t.Type),
            },
            1,
            null,
            4,
            8);
        Assert.Contains(VmAFloorTickElevator.DonutRaise, types);
        Assert.Contains(VmAFloorCreate.FloorLowerToNearest, types);
    }

    [Fact]
    public void EvDoDonut_SkipsWhenFloorPlaneMoving()
    {
        var sec = new VmAFloorEv.StairSector { SectorNum = 0 };
        Assert.False(VmAFloorEvDonut921Slice.EvDoDonut(
            new VmAFloorEv.FloorEvContext
            {
                SectorTagIterator = (_, _) => new[] { 0 },
                SectorByIndex = _ => sec,
                PlaneMovingFloor = _ => true,
            },
            1,
            null,
            4,
            8));
    }
}
