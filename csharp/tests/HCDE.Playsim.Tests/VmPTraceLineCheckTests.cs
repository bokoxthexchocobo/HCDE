namespace HCDE.Playsim.Tests;

public class VmPTraceLineCheckTests
{
    private static VmPTraceLineCheck.LineCheckState FlatTwoSided(
        double hitZ,
        uint lineFlags = VmPTraceLineCheck.MlTwoSided,
        uint wallMask = 0,
        double nearFloor = 0,
        double nearCeil = 128,
        double farFloor = 0,
        double farCeil = 128)
    {
        var front = new object();
        var back = new object();
        return new VmPTraceLineCheck.LineCheckState
        {
            CurSector = front,
            CurSectorNum = 1,
            LineFrontSector = front,
            LineFrontSectorNum = 1,
            LineBackSector = back,
            LineBackSectorNum = 2,
            LineHasBackSector = true,
            LineFlags = lineFlags,
            HitX = 16,
            HitY = 16,
            HitZ = hitZ,
            StartX = 0,
            StartY = 0,
            StartZ = 32,
            Dist = 10,
            Frac = 0.5,
            InterceptLine = new object(),
            SectorFloorZAtPoint = (sec, _, _) => sec == front ? nearFloor : farFloor,
            SectorCeilingZAtPoint = (sec, _, _) => sec == front ? nearCeil : farCeil,
            GetSectorTexture = (_, part) => part == VmPTraceLineCheck.SectorFloor ? 11 : 12,
            LevelSectorFromNum = (_, num) => num,
            CheckSectorPlane = (_, _) => true,
            GetFFloors = _ => Array.Empty<VmPTraceLineCheck.FFloorRover>(),
            CopySectorToDummy = (dst, src) =>
            {
                dst.FloorZAtPoint = (_, _) => src == back ? farFloor : nearFloor;
                dst.CeilingZAtPoint = (_, _) => src == back ? farCeil : nearCeil;
                dst.FFloors = Array.Empty<VmPTraceLineCheck.FFloorRover>();
            },
            WallMask = wallMask,
        };
    }

    [Fact]
    public void LineCheck_OneSidedLineHitsMiddleWall()
    {
        var s = FlatTwoSided(64, lineFlags: 0);
        s.LineHasBackSector = false;
        s.LineBackSector = null;
        Assert.False(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitWall, s.HitType);
        Assert.Equal(VmPTraceLineCheck.TierMiddle, s.Tier);
        Assert.Equal(10, s.ResultDistance);
    }

    [Fact]
    public void LineCheck_WallMaskBlocksShotInOpening()
    {
        var s = FlatTwoSided(64, wallMask: 0x100);
        s.LineFlags = VmPTraceLineCheck.MlTwoSided | 0x100;
        Assert.False(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitWall, s.HitType);
        Assert.Equal(VmPTraceLineCheck.TierMiddle, s.Tier);
    }

    [Fact]
    public void LineCheck_PassesIntoBackSectorWhenOpeningClear()
    {
        var s = FlatTwoSided(64);
        Assert.True(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitNone, s.HitType);
        Assert.Equal(s.LineBackSector, s.CurSector);
        Assert.Equal(2, s.CurSectorNum);
        Assert.Equal(10, s.EnterDist);
    }

    [Fact]
    public void LineCheck_CompatSameSectorActivatesAndContinues()
    {
        var sector = new object();
        var activations = new List<int>();
        var s = FlatTwoSided(64);
        s.LineFrontSector = sector;
        s.LineBackSector = sector;
        s.LineFrontSectorNum = 5;
        s.LineBackSectorNum = 5;
        s.CurSector = sector;
        s.CurSectorNum = 5;
        s.CompatTrace = true;
        s.TraceFlags = VmPTraceLineCheck.TracePCross | VmPTraceLineCheck.TraceImpact;
        s.ActivateLine = (_, _, _, spac) => activations.Add(spac);
        Assert.True(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitNone, s.HitType);
        Assert.Contains(VmPTraceLineCheck.SpacPCross, activations);
        Assert.Contains(VmPTraceLineCheck.SpacImpact, activations);
    }

    [Fact]
    public void LineCheck_3DFloorEdgeHitsFFloorTier()
    {
        var front = new object();
        var back = new object();
        var rover = new VmPTraceLineCheck.FFloorRover
        {
            Flags = VmPTraceLineCheck.FfExists,
            Rover = "ffloor-edge",
            BottomZAtPoint = (_, _) => 40,
            TopZAtPoint = (_, _) => 80,
            BottomZAtCenter = (_, _) => 40,
            TopZAtCenter = (_, _) => 80,
        };
        var s = new VmPTraceLineCheck.LineCheckState
        {
            CurSector = front,
            CurSectorNum = 1,
            LineFrontSector = front,
            LineFrontSectorNum = 1,
            LineBackSector = back,
            LineBackSectorNum = 2,
            LineHasBackSector = true,
            LineFlags = VmPTraceLineCheck.MlTwoSided,
            LineSpecial = 1,
            HitZ = 60,
            HitX = 8,
            HitY = 8,
            Dist = 4,
            Frac = 0.25,
            InShootThrough = 1,
            TraceFlags = VmPTraceLineCheck.TraceImpact,
            SectorFloorZAtPoint = (_, _, _) => 0,
            SectorCeilingZAtPoint = (_, _, _) => 128,
            GetSectorTexture = (_, _) => 3,
            LevelSectorFromNum = (_, n) => n,
            CheckSectorPlane = (_, _) => true,
            GetFFloors = sec => sec == back ? new[] { rover } : Array.Empty<VmPTraceLineCheck.FFloorRover>(),
            CopySectorToDummy = (dst, src) =>
            {
                dst.FFloors = new[] { rover };
                dst.FloorZAtPoint = (_, _) => 0;
                dst.CeilingZAtPoint = (_, _) => 128;
            },
            ActivateLine = (_, _, _, _) => { },
        };
        Assert.False(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitWall, s.HitType);
        Assert.Equal(VmPTraceLineCheck.TierFFloor, s.Tier);
        Assert.Equal("ffloor-edge", s.Ffloor);
    }

    [Fact]
    public void LineCheck_HitsFloorBeforeWallWhenBelowFloorPlane()
    {
        var s = FlatTwoSided(-1);
        Assert.False(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(VmPTraceLineCheck.TraceHitFloor, s.HitType);
        Assert.Equal(11, s.HitTexture);
    }

    [Fact]
    public void LineCheck_TraceSkipRecursesWithSpecial3DPass()
    {
        var front = new object();
        var back = new object();
        var calls = 0;
        var rover = new VmPTraceLineCheck.FFloorRover
        {
            Flags = VmPTraceLineCheck.FfExists | VmPTraceLineCheck.FfShootThrough,
            BottomZAtPoint = (_, _) => 0,
            TopZAtPoint = (_, _) => 128,
        };
        var s = new VmPTraceLineCheck.LineCheckState
        {
            CurSector = front,
            CurSectorNum = 1,
            LineFrontSector = front,
            LineFrontSectorNum = 1,
            LineBackSector = back,
            LineBackSectorNum = 2,
            LineHasBackSector = true,
            LineFlags = VmPTraceLineCheck.MlTwoSided,
            HitZ = -1,
            Dist = 6,
            Frac = 0.3,
            TraceFlags = VmPTraceLineCheck.Trace3DCallback,
            HasTraceCallback = true,
            InShootThrough = 1,
            SectorFloorZAtPoint = (_, _, _) => 0,
            SectorCeilingZAtPoint = (_, _, _) => 128,
            GetSectorTexture = (_, _) => 1,
            LevelSectorFromNum = (_, n) => n,
            CheckSectorPlane = (_, _) => true,
            GetFFloors = _ => new[] { rover },
            CopySectorToDummy = (dst, _) =>
            {
                dst.FFloors = new[] { rover };
                dst.FloorZAtPoint = (_, _) => 0;
                dst.CeilingZAtPoint = (_, _) => 128;
            },
            TraceCallback = () =>
            {
                calls++;
                return calls == 1 ? VmPTraceLineCheck.TraceSkip : VmPTraceLineCheck.TraceStop;
            },
        };
        Assert.False(VmPTraceLineCheck.LineCheck(s));
        Assert.Equal(2, calls);
    }

    [Fact]
    public void LineCheck_WaterCrossRecordsHeightSecWhenPlaneValid()
    {
        var front = new object();
        var back = new object();
        var hsec = new object();
        var s = FlatTwoSided(48);
        s.StartZ = 64;
        s.GetHeightSec = _ => hsec;
        s.SectorFloorZAtPoint = (sec, _, _) =>
        {
            if (sec == hsec) return 50;
            return sec == front ? 0 : 0;
        };
        s.CheckSectorPlane = (_, floor) => floor;
        Assert.True(VmPTraceLineCheck.LineCheck(s));
        Assert.NotNull(s.CrossedWater);
        Assert.Equal(0, s.CrossedWaterDistance);
    }
}
