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
}
