namespace HCDE.Playsim.Tests;

public class VmPTraceTraverseTests
{
    [Fact]
    public void TraceTraverse_EntersLinePortalAndSkipsLineCheck()
    {
        var setup = false;
        var portal = false;
        var result = VmPTraceTraverse.TraceTraverse(new VmPTraceTraverse.TraceTraverseContext
        {
            HitTypeNone = 0,
            CurrentHitType = 0,
            Steps =
            [
                new VmPTraceTraverse.InterceptStep
                {
                    IsLine = true,
                    IsLinePortal = true,
                    OnLineSidePrecise = true,
                    EnterFloorZ = 0,
                    EnterCeilingZ = 64,
                    HitZ = 32,
                    PortalType = VmPTraceTraverse.PorttLinked,
                },
                new VmPTraceTraverse.InterceptStep { IsLine = true, LineCheckPasses = false },
            ],
            Setup3DFloors = () => setup = true,
            EnterLinePortal = _ => portal = true,
            LineCheck = s => s.LineCheckPasses,
        });
        Assert.True(setup);
        Assert.True(portal);
        Assert.False(result);
    }

    [Fact]
    public void TraceTraverse_FinalizeMissWhenNoHit()
    {
        var finalized = false;
        _ = VmPTraceTraverse.TraceTraverse(new VmPTraceTraverse.TraceTraverseContext
        {
            HitTypeNone = 0,
            CurrentHitType = 0,
            MaxDist = 100,
            FinalizeMiss = () => finalized = true,
        });
        Assert.True(finalized);
    }

    [Fact]
    public void TraceTraverse_Records3DWaterSplashOncePerSector()
    {
        object? water = null;
        var rover = new VmPTraceTraverse.FFloorRover
        {
            Flags = VmPTraceTraverse.FfExists,
            IsLiquid = true,
            TopZAtHit = 40,
            Rover = new object(),
        };
        _ = VmPTraceTraverse.TraceTraverse(new VmPTraceTraverse.TraceTraverseContext
        {
            HitTypeNone = 0,
            CurrentHitType = 0,
            Steps =
            [
                new VmPTraceTraverse.InterceptStep
                {
                    SectorNum = 1,
                    SectorFloorZAtHit = 0,
                    SectorFFloors = [rover],
                    Check3DFloorPlane = _ => true,
                },
                new VmPTraceTraverse.InterceptStep
                {
                    SectorNum = 1,
                    SectorFloorZAtHit = 0,
                    SectorFFloors = [rover],
                    Check3DFloorPlane = _ => true,
                },
            ],
            RecordCrossed3DWater = w => water = w,
        });
        Assert.Same(rover.Rover, water);
    }

    [Fact]
    public void TraceTraverse_AssignsFfloorOnFloorHit()
    {
        var rover = new VmPTraceTraverse.FFloorRover
        {
            Flags = VmPTraceTraverse.FfExists,
            TopZAtHit = 48,
            Rover = new object(),
        };
        object? assigned = null;
        _ = VmPTraceTraverse.TraceTraverse(new VmPTraceTraverse.TraceTraverseContext
        {
            HitTypeNone = 0,
            CurrentHitType = VmPTraceTraverse.TraceHitFloor,
            HitPosZ = 48,
            SectorFloorZAtHit = 0,
            Steps =
            [
                new VmPTraceTraverse.InterceptStep { SectorFFloors = [rover] },
            ],
            AssignFfloor = ff => assigned = ff,
        });
        Assert.Same(rover.Rover, assigned);
    }

    [Fact]
    public void LinePortalAllowed_RespectsPortalRestrict()
    {
        Assert.True(VmPTraceTraverse.LinePortalAllowed(VmPTraceTraverse.PorttLinked, VmPTraceTraverse.TracePortalRestrict));
        Assert.False(VmPTraceTraverse.LinePortalAllowed(2, VmPTraceTraverse.TracePortalRestrict));
        Assert.True(VmPTraceTraverse.LinePortalAllowed(2, 0));
    }
}
