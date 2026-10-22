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
                new VmPTraceTraverse.InterceptStep { IsLinePortal = true },
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
            FinalizeMiss = () => finalized = true,
        });
        Assert.True(finalized);
    }
}
