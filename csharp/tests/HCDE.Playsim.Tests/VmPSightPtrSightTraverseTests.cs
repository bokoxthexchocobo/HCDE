namespace HCDE.Playsim.Tests;

public class VmPSightPtrSightTraverseTests
{
    [Fact]
    public void PtrSightTraverse_ClosedDoorStops()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.5,
            OpenRange = 0,
            OpenPortalFlags = 0,
            HasLinkedPortal = false,
        };
        Assert.False(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
    }

    [Fact]
    public void PtrSightTraverse_UpdatesSlopes()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.5,
            SightStartZ = 32,
            OpenBottom = 0,
            OpenTop = 64,
            OpenRange = 1,
        };
        Assert.True(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
        Assert.True(ctx.BottomSlope > VmPSightPtrSightTraverse.LineOpenMin);
    }
}
