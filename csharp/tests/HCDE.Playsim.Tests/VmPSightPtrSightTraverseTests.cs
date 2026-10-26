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
        Assert.True(ctx.TopSlope < VmPSightPtrSightTraverse.LineOpenMax);
    }

    [Fact]
    public void PtrSightTraverse_CompatSameSectorSkips()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            CompatTrace = true,
            SameSectorLine = true,
            InterceptFrac = 0.5,
            OpenRange = 0,
        };
        Assert.True(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
    }

    [Fact]
    public void PtrSightTraverse_FracZeroContinues()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0,
            OpenRange = 1,
        };
        Assert.True(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
    }

    [Fact]
    public void PtrSightTraverse_LinkedPortalQueuesAndStops()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.25,
            OpenRange = 1,
            OpenBottom = 0,
            OpenTop = 128,
            HasLinkedPortal = true,
            LinkedPortalGroup = 42,
            PortalDir = VmPSightPtrSightTraverse.PortalDirFloor,
        };
        Assert.False(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
        Assert.Single(ctx.PortalQueue);
        Assert.Equal(42, ctx.PortalQueue[0].Group);
    }

    [Fact]
    public void PtrSightTraverse_LineBlocksSightCollapsesOpening()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.5,
            SightStartZ = 16,
            OpenBottom = 8,
            OpenTop = 24,
            OpenRange = 4,
            LineBlocksSight = true,
        };
        Assert.False(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
    }

    [Fact]
    public void PtrSightTraverse_3dFloorBlocksViewCone()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.5,
            SightStartZ = 0,
            OpenBottom = -100,
            OpenTop = 100,
            OpenRange = 1,
            FrontFFloors =
            [
                new VmPSightPtrSightTraverse.SightFFloorAtHit(true, VmPSightPtrSightTraverse.FfExists, -120, 250),
            ],
        };
        Assert.False(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
    }

    [Fact]
    public void PtrSightTraverse_SwapPortalFlagsSides()
    {
        var swapped = VmPSightPtrSightTraverse.SwapPortalFlagsSides(
            VmPSightPtrSightTraverse.SoTopFront | VmPSightPtrSightTraverse.SoBottomBack);
        Assert.Equal(VmPSightPtrSightTraverse.SoTopBack | VmPSightPtrSightTraverse.SoBottomFront, swapped);
    }

    [Fact]
    public void PtrSightTraverse_FinalizesLastZ()
    {
        var ctx = new VmPSightPtrSightTraverse.SightTraverseContext
        {
            InterceptFrac = 0.5,
            SightStartZ = 10,
            OpenBottom = 0,
            OpenTop = 100,
            OpenRange = 1,
        };
        Assert.True(VmPSightPtrSightTraverse.PtrSightTraverse(ctx));
        Assert.Equal(ctx.TopSlope * 0.5 + 10, ctx.LastZTop);
        Assert.Equal(ctx.BottomSlope * 0.5 + 10, ctx.LastZBottom);
    }
}
