namespace HCDE.Playsim.Tests;

public class VmPSightCheckSightTests
{
    [Fact]
    public void CheckSight_RejectsNullActors()
    {
        var ctx = new VmPSightCheckSight.CheckSightContext { Viewer = new object(), Target = null };
        Assert.False(VmPSightCheckSight.CheckSight(ctx));
    }

    [Fact]
    public void CheckSight_SelfVisible()
    {
        var self = new object();
        var ctx = new VmPSightCheckSight.CheckSightContext { Viewer = self, Target = self, RejectTableAllows = true };
        Assert.True(VmPSightCheckSight.CheckSight(ctx));
    }

    [Fact]
    public void SightTraverseIntercepts_SortsAndTraverses()
    {
        var ctx = new VmPSightTraverseIntercepts.SightTraverseInterceptsContext
        {
            StartFrac = 0,
            InterceptFracFromLine = i => i.Line switch
            {
                1 => 0.25,
                2 => 0.5,
                _ => 1,
            },
            PtrSightTraverse = _ => true,
        };
        ctx.Intercepts.Add(new VmPSightTraverseIntercepts.SightIntercept { IsLine = true, Line = 2 });
        ctx.Intercepts.Add(new VmPSightTraverseIntercepts.SightIntercept { IsLine = true, Line = 1 });
        Assert.True(VmPSightTraverseIntercepts.SightTraverseIntercepts(ctx));
    }

    [Fact]
    public void SightTraverseIntercepts_StopsWhenPtrSightTraverseFails()
    {
        var ctx = new VmPSightTraverseIntercepts.SightTraverseInterceptsContext
        {
            StartFrac = 0,
            InterceptFracFromLine = _ => 0.5,
            PtrSightTraverse = _ => false,
        };
        ctx.Intercepts.Add(new VmPSightTraverseIntercepts.SightIntercept { IsLine = true, Line = 1 });
        Assert.False(VmPSightTraverseIntercepts.SightTraverseIntercepts(ctx));
    }

    [Fact]
    public void SightTraverseIntercepts_BlocksFfFloorBottomCrossing()
    {
        var ctx = new VmPSightTraverseIntercepts.SightTraverseInterceptsContext
        {
            StartFrac = 0,
            SightStartZ = 32,
            TopSlope = 40,
            BottomSlope = -8,
            LastZTop = 20,
            LastZBottom = 18,
            MySeethrough = true,
            TargetInLastSector = true,
            TargetSectorFFloors = new[]
            {
                new VmPSightTraverseIntercepts.SightFFloorCrossing(
                    Exists: true,
                    Flags: VmPSightTraverseIntercepts.FfExists,
                    BottomZ: 22,
                    TopZ: 64),
            },
        };
        Assert.False(VmPSightTraverseIntercepts.SightTraverseIntercepts(ctx));
    }

    [Fact]
    public void CheckSight_RejectsHeightSecWaterBoundary()
    {
        var ctx = new VmPSightCheckSight.CheckSightContext
        {
            Viewer = new object(),
            Target = new object(),
            RejectTableAllows = true,
            ViewerHeightSec = true,
            ViewerTop = 40,
            ViewerZ = 48,
            ViewerHeightSecFloorZ = 44,
            TargetZ = 0,
        };
        Assert.False(VmPSightCheckSight.CheckSight(ctx));
    }

    [Fact]
    public void CheckSight_RetriesPortalPathWhenPrimaryFails()
    {
        var ctx = new VmPSightCheckSight.CheckSightContext
        {
            Viewer = new object(),
            Target = new object(),
            RejectTableAllows = true,
            ViewerX = 0,
            ViewerY = 0,
            TargetX = 100,
            TargetY = 0,
            RunPrimarySightPath = _ => false,
            RunPortalSightPath = (_, task) => task.PortalGroup == 7,
        };
        ctx.PortalTasks.Add(new VmPSightPathTraverse.SightPortalTask(0, 1, -1, 0, 7));
        Assert.True(VmPSightCheckSight.CheckSight(ctx));
    }
}
